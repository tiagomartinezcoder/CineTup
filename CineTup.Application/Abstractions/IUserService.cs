using CineTup.Application.Requests;
using CineTup.Application.Responses;
using System.Collections.Generic;

namespace CineTup.Application.Abstractions
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetAllUsersAsync();
        Task UpdateRoleAsync(int userId, UpdateRoleRequest request);
        Task DeleteUserAsync(int userId);
    }
}
