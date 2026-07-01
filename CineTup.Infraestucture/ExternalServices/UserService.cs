using CineTup.Application.Abstractions;
using CineTup.Application.Exceptions;
using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Domain.Entities;
using CineTup.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CineTup.Infraestucture.ExternalServices
{
    public class UserService : IUserService
    {
        private readonly CineTupDbContext _context;

        public UserService(CineTupDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            return users.Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Rol = u switch
                {
                    Client => "Client",
                    Admin => "Admin",
                    SysAdmin => "SysAdmin",
                    _ => "Unknown"
                }
            }).ToList();
        }

        public async Task UpdateRoleAsync(int userId, UpdateRoleRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
                throw new NotFoundException("Usuario no encontrado.");

            string currentRole = user switch
            {
                Client => "Client",
                Admin => "Admin",
                SysAdmin => "SysAdmin",
                _ => throw new Exception("Tipo de usuario desconocido")
            };

            string newRole = request.NewRole;

            if (string.Equals(currentRole, newRole, StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("El usuario ya tiene asignado ese rol.");

            var validRoles = new[] { "Client", "Admin", "SysAdmin" };
            if (!validRoles.Contains(newRole))
                throw new ValidationException("Rol no válido. Los roles válidos son: Client, Admin, SysAdmin.");

            if (string.Equals(currentRole, "SysAdmin", StringComparison.OrdinalIgnoreCase) &&
                await _context.SysAdmins.CountAsync(s => !s.IsDeleted) <= 1)
                throw new ValidationException("No se puede cambiar el rol del único SysAdmin en el sistema.");

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (user is Client client)
                    {
                        foreach (var ticket in client.Tickets)
                        {
                            ticket.ClientId = null;
                            ticket.IsAvailable = true;
                            ticket.PurchaseDate = null;
                        }
                        _context.Clients.Remove(client);
                    }
                    else if (user is Admin admin)
                    {
                        _context.Admins.Remove(admin);
                    }
                    else if (user is SysAdmin)
                    {
                        _context.SysAdmins.Remove((SysAdmin)user);
                    }

                    bool emailExists = newRole switch
                    {
                        "Client" => _context.Clients.Any(c => c.Email == user.Email && !c.IsDeleted),
                        "Admin" => _context.Admins.Any(a => a.Email == user.Email && !a.IsDeleted),
                        "SysAdmin" => _context.SysAdmins.Any(s => s.Email == user.Email && !s.IsDeleted),
                        _ => false
                    };

                    if (emailExists)
                        throw new ConflictException("Ya existe un usuario con ese email en el rol de destino.");

                    User targetUser = newRole switch
                    {
                        "Client" => new Client { Name = user.Name, Email = user.Email, Password = user.Password, UpdateDateTime = DateTime.UtcNow },
                        "Admin" => new Admin { Name = user.Name, Email = user.Email, Password = user.Password, UpdateDateTime = DateTime.UtcNow },
                        "SysAdmin" => new SysAdmin { Name = user.Name, Email = user.Email, Password = user.Password, UpdateDateTime = DateTime.UtcNow },
                        _ => throw new ValidationException("Rol no válido.")
                    };

                    _context.Add(targetUser);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
                throw new NotFoundException("Usuario no encontrado.");

            user.IsDeleted = true;
            user.DeletedDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
