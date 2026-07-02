using CineTup.Application.Requests;
using CineTup.Domain.Entities;

namespace CineTup.Application.Abstractions.Infraestructure
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task UpdateRoleAsync(User user, string newRole);
        Task UpdateAsync(User user);
    }
}