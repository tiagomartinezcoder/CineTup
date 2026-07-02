using CineTup.Application.Abstractions;
using CineTup.Application.Abstractions.Infraestructure;
using CineTup.Application.Exceptions;
using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Domain.Entities;

namespace CineTup.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

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
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException("Usuario no encontrado.");

            string currentRole = user switch
            {
                Client => "Client",
                Admin => "Admin",
                SysAdmin => "SysAdmin",
                _ => throw new Exception("Tipo de usuario desconocido")
            };

            if (currentRole.Equals(request.NewRole, StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("El usuario ya tiene ese rol.");

            var validRoles = new[] { "Client", "Admin", "SysAdmin" };

            if (!validRoles.Contains(request.NewRole))
                throw new ValidationException("Rol inválido.");

            await _userRepository.UpdateRoleAsync(user, request.NewRole);
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException("Usuario no encontrado.");

            user.IsDeleted = true;
            user.DeletedDateTime = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }
    }
}