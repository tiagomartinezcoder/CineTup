using CineTup.Application.Abstractions.Infraestructure;
using CineTup.Domain.Entities;
using CineTup.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CineTup.Infraestucture.Persistence.Repository
{
    public class MovieRepository : BaseRepository<Movie>, IMovieRepository
    {
        public MovieRepository(CineTupDbContext context) : base(context)
        {
        }
        public async Task<List<Movie>> GetAllWithShowTimesAsync()
        {
            return await _context.Set<Movie>()
                .Include(m => m.ShowTimes)
                .ToListAsync();
        }

        public async Task<bool> ExistsByTitleAsync(string title, int? excludeId = null)
        {
            return await _dbSet.AnyAsync(m =>
                m.Title == title &&
                !m.IsDeleted &&
                m.Id != (excludeId ?? 0));
        }

        public override async Task DeleteAsync(int id)
        {
            var movie = await _context.Set<Movie>()
                .Include(m => m.ShowTimes)
                    .ThenInclude(st => st.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (movie == null) return;

            foreach (var showTime in movie.ShowTimes.Where(st => !st.IsDeleted))
            {
                foreach (var ticket in showTime.Tickets.Where(t => !t.IsDeleted))
                {
                    ticket.IsDeleted = true;
                    ticket.DeletedDateTime = DateTime.UtcNow;
                    ticket.UpdateDateTime = DateTime.UtcNow;
                }

                showTime.IsDeleted = true;
                showTime.DeletedDateTime = DateTime.UtcNow;
                showTime.UpdateDateTime = DateTime.UtcNow;
            }

            movie.IsDeleted = true;
            movie.DeletedDateTime = DateTime.UtcNow;
            movie.UpdateDateTime = DateTime.UtcNow;

            await SaveChangesAsync();
        }
    }
}
