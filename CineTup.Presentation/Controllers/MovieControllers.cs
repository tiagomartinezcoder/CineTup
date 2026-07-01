using CineTup.Application.Abstractions;
using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTup.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MovieResponse>>> GetAllAsync()
        {
            var movies = await _movieService.GetAllAsync();
            if (!movies.Any())
                return NotFound("No se encontraron películas.");
            return Ok(movies);
        }
      

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieResponse>> GetByIdAsync([FromRoute] int id)
        {
            var movie = await _movieService.GetByIdAsync(id);
            if (movie == null)
                return NotFound("Película no encontrada.");
            return Ok(movie);
        }

        [Authorize(Policy = Policies.AdminOnly)]
        [HttpPost]
        public async Task<ActionResult<MovieResponse>> CreateAsync([FromBody] MovieRequest movie)
        {
            var createdMovie = await _movieService.CreateAsync(movie);
            return CreatedAtAction(
                actionName: "GetById",
                routeValues: new { id = createdMovie.Id },
                value: createdMovie);
        }


        [Authorize(Policy = Policies.AdminOnly)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync([FromRoute] int id)
        {
            await _movieService.DeleteAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.AdminOnly)]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync([FromBody] MovieUpdateRequest movie, [FromRoute] int id)
        {
            await _movieService.UpdateAsync(movie, id);
            return NoContent();
        }
    }
}
