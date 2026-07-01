using System;
using System.Collections.Generic;
using System.Text;
using CineTup.Application.Requests;
using CineTup.Application.Responses;

namespace CineTup.Application.Abstractions
{
    public interface IMovieService
    {
        Task<List<MovieResponse>> GetAllAsync();
        Task<MovieResponse> GetByIdAsync(int id);
        Task<MovieResponse> CreateAsync(MovieRequest movie);
        Task UpdateAsync(MovieUpdateRequest movie, int id);
        Task DeleteAsync(int id);
        
    }
}
