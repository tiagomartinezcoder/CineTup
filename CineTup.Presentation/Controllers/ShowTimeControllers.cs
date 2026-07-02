using CineTup.Application.Abstractions;
using CineTup.Application.Exceptions;
using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTup.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowTimeController : ControllerBase
    {
        private readonly IShowTimeService _showTimeService;

        public ShowTimeController(IShowTimeService showTimeService)
        {
            _showTimeService = showTimeService;
        }

        [HttpGet]
        public async Task<ActionResult<ShowTimeResponse>> GetAllAsync()
        {
            var showTimes = await _showTimeService.GetAllAsync();
            if (!showTimes.Any())
                return NotFound("No se encontraron funciones.");
            return Ok(showTimes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShowTimeResponse>> GetByIdAsync([FromRoute] int id)
        {
            var showTime = await _showTimeService.GetByIdAsync(id);
            if (showTime == null)
                return NotFound("Función no encontrada.");
            return Ok(showTime);
        }

        [Authorize(Policy = Policies.AdminOnly)]
        [HttpPost]
        public async Task<ActionResult<ShowTimeResponse>> CreateAsync([FromBody] ShowTimeRequest showTime)
        {
            var createdShowTime = await _showTimeService.CreateAsync(showTime);
            return CreatedAtAction(
                actionName: "GetById",
                routeValues: new { id = createdShowTime.Id },
                value: createdShowTime);
        }

        [Authorize(Policy = Policies.AdminOnly)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync([FromRoute] int id)
        {
            await _showTimeService.DeleteAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.AdminOnly)]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(
            [FromBody] ShowTimeUpdateRequest showTime,
            [FromRoute] int id)
        {
            await _showTimeService.UpdateAsync(showTime, id);
            return NoContent();

        }
    }
}
