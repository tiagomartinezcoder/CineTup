using CineTup.Application.Abstractions;
using CineTup.Application.Exceptions;
using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Domain.Entities;
using CineTup.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace CineTup.Infraestucture.ExternalServices
{
    public class AuthService : IAuthService
    {
        private readonly CineTupDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITmdbService _tmdbService;
        public AuthService(CineTupDbContext context, IConfiguration configuration, ITmdbService tmdbService)
        {
            _context = context;
            _configuration = configuration;
            _tmdbService = tmdbService;
        }
        public async Task<AuthResponse> SingUp(SignUpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("El nombre es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ValidationException("El email es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("La contraseña es obligatoria.");
            }

            if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ValidationException($"El email '{request.Email}' no es válido.");
            }
            if (request.Password.Length < 8)
            {
                throw new ValidationException("La contraseña debe tener al menos 8 caracteres.");
            }
            bool emailExists = _context.Clients.Any(c => c.Email == request.Email)
                           || _context.Admins.Any(a => a.Email == request.Email)
                           || _context.SysAdmins.Any(u => u.Email == request.Email);
            if (emailExists)
            {
                throw new ConflictException($"El email '{request.Email}' ya está registrado.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new Client
            {
                Name = request.Name,
                Email = request.Email,
                Password = hashedPassword
            };
            _context.Clients.Add(user);
            string? avatarUrl = await _tmdbService.GetRandomMoviePosterAsync();
            user.AvatarUrl = avatarUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) {
                Console.WriteLine(ex.ToString());
                throw new DatabaseException("Error al guardar los datos en la base de datos.", 
                    ex);
            }
            int userId = user.Id;

            return new AuthResponse
            {
                Token = GenerateToken(userId, request.Email, "Client"),
                Rol = "Client",
                UserId = userId,
                Email = request.Email,
                AvatarUrl = avatarUrl
            };
        }
        public async Task<AuthResponse> SingIn(SignInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ValidationException("El email es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("La contraseña es obligatoria.");
            }

            int userId;
            string rol;
            string? avatarUrl = null;

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == request.Email);
            if (client != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, client.Password))
                {
                    throw new UnauthorizedException("Credenciales inválidas.");
                }
                userId = client.Id;
                rol = "Client";
                avatarUrl = client.AvatarUrl;
            }
            else
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email);
                if (admin != null)
                {
                    if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.Password))
                    {
                        throw new UnauthorizedException("Credenciales inválidas.");
                    }
                    userId = admin.Id;
                    rol = "Admin";
                    avatarUrl = admin.AvatarUrl;

                }
                else
                {
                    var sysAdmin = await _context.SysAdmins.FirstOrDefaultAsync(u => u.Email == request.Email);
                    if (sysAdmin != null)
                    {
                        if (!BCrypt.Net.BCrypt.Verify(request.Password, sysAdmin.Password))
                        {
                            throw new UnauthorizedException("Credenciales inválidas.");
                        }
                        userId = sysAdmin.Id;
                        rol = "SysAdmin";
                        avatarUrl = sysAdmin.AvatarUrl;
                    }
                    else
                    {
                        throw new UnauthorizedException("Credenciales inválidas.");
                    }
                }
            }
            return new AuthResponse
            {
                Token = GenerateToken(userId, request.Email, rol),
                Rol = rol,
                UserId = userId,
                Email = request.Email,
                AvatarUrl = avatarUrl
            };
        }
        private string GenerateToken(int userId, string email, string rol)
        {
            string key = _configuration["Jwt:Key"]!;
            string issuer = _configuration["Jwt:Issuer"]!;
            string audience = _configuration["Jwt:Audience"]!;
            int expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"]!);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
