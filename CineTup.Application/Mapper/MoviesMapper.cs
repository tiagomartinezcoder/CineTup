using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Domain.Entities;

namespace CineTup.Application.Mapper
{
    public static class MoviesMapper
    {
        public static MovieResponse ToMovieResponse(this Movie movie)
        {
            return new MovieResponse
            {
                Id = movie.Id,
                Title = movie.Title,
                Director = movie.Director,
                Category = movie.Category,
                Summary = movie.Summary,
                ImageUrl = movie.ImageUrl,
                BannerUrl = movie.BannerUrl,
                Duration = movie.Duration,
                Language = movie.Language,
                IsAvailable = movie.IsAvailable,
            };
        }

        public static Movie ToMovie(this MovieRequest movieRequest)
        {
            return new Movie
            {
                Title = movieRequest.Title,
                Director = movieRequest.Director,
                Category = movieRequest.Category,
                Summary = movieRequest.Summary,
                ImageUrl = movieRequest.ImageUrl,
                BannerUrl = movieRequest.BannerUrl,
                Duration = movieRequest.Duration,
                Language = movieRequest.Language,
                IsAvailable = movieRequest.IsAvailable,
                IsDeleted = false
            };
        }

        public static void ApplyUpdate(this Movie movie, MovieUpdateRequest update)
        {
            if (update.Title != null)
                movie.Title = update.Title;
            if (update.Director != null)
                movie.Director = update.Director;
            if (update.Category != null)
                movie.Category = update.Category;
            if (update.Summary != null)
                movie.Summary = update.Summary;
            if (update.ImageUrl != null)
                movie.ImageUrl = update.ImageUrl;
            if (update.BannerUrl != null)
                movie.BannerUrl = update.BannerUrl;
            if (update.Duration.HasValue)
                movie.Duration = update.Duration.Value;
            if (update.Language != null)
                movie.Language = update.Language;
            if (update.IsAvailable.HasValue)
                movie.IsAvailable = update.IsAvailable.Value;
        }
    }
}
        