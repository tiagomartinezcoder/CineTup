using System;
using System.Collections.Generic;
using System.Text;
using CineTup.Domain.Entities;

namespace CineTup.Application.Abstractions.Infraestructure
{
    public interface IMovieRepository : IBaseRepository<Movie>
    {
        Task<List<Movie>> GetAllWithShowTimesAsync();
        Task<bool> ExistsByTitleAsync(string title, int? excludeId = null);
    }
}
