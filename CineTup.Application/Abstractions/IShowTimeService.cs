using CineTup.Application.Requests;
using CineTup.Application.Responses;

namespace CineTup.Application.Abstractions
{
    public interface IShowTimeService
    {
        Task<List<ShowTimeResponse>> GetAllAsync();
        Task<ShowTimeResponse> GetByIdAsync(int id);
        Task<ShowTimeResponse> CreateAsync(ShowTimeRequest request);
        Task UpdateAsync(ShowTimeUpdateRequest request, int id);
        Task DeleteAsync(int id);
    }
}