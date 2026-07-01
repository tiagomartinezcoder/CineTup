using CineTup.Application.Abstractions.Infraestructure;
using CineTup.Domain.Entities;
using CineTup.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CineTup.Infraestucture.Persistence.Repository
{
    public class ShowTimeRepository : BaseRepository<ShowTime>, IShowTimeRepository
    {
        public ShowTimeRepository(CineTupDbContext context) : base(context)
        {
        }

        public override async Task<List<ShowTime>> GetAllAsync()
        {
            return await _dbSet
                .Where(st => !st.IsDeleted)
                .OrderBy(st => st.StartTime)
                .ToListAsync();
        }

        public override async Task<ShowTime?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(st => st.Id == id && !st.IsDeleted);
        }

        public async Task<bool> ExistsOverlappingShowTimeAsync(int movieId, DateTime startTime, DateTime endTime, int? excludeShowTimeId = null)
        {
            return await _dbSet.AnyAsync(st =>
                st.MovieId == movieId &&
                !st.IsDeleted &&
                st.Id != (excludeShowTimeId ?? 0) &&
                st.StartTime < endTime &&
                st.StartTime.AddMinutes(st.Movie.Duration) > startTime);
        }

        public override async Task DeleteAsync(int id)
        {
            var tickets = await _context.Set<Ticket>()
                .Where(t => t.ShowTimeId == id && !t.IsDeleted)
                .ToListAsync();

            foreach (var ticket in tickets)
            {
                ticket.IsDeleted = true;
                ticket.DeletedDateTime = DateTime.UtcNow;
                ticket.UpdateDateTime = DateTime.UtcNow;
            }

            var showTime = await _dbSet
                .FirstOrDefaultAsync(st => st.Id == id && !st.IsDeleted);

            if (showTime != null)
            {
                showTime.IsDeleted = true;
                showTime.DeletedDateTime = DateTime.UtcNow;
                showTime.UpdateDateTime = DateTime.UtcNow;
            }

            await SaveChangesAsync();
        }
    }
}
