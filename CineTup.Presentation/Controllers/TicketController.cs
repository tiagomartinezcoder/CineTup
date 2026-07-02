using CineTup.Application.Abstractions;
using CineTup.Application.Exceptions;
using CineTup.Application.Responses;
using CineTup.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace CineTup.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [Authorize(Policy = Policies.AdminOnly)]
        [HttpGet]
        public async Task<ActionResult<List<TicketResponse>>> GetAllAsync()
        {
            try
            {
                var tickets = await _ticketService.GetAllAsync();
                if (!tickets.Any())
                    return NotFound("No se encontraron tickets.");
                return Ok(tickets);
            }
            catch (DatabaseException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado.");
            }
        }

        [Authorize(Policy = Policies.AdminOnly)]
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketResponse>> GetByIdAsync([FromRoute] int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null)
                return NotFound("Ticket no encontrado.");
            return Ok(ticket);

        }

        [Authorize(Policy = Policies.ClientOnly)]
        [HttpPost("{id}/buy")]
        public async Task<ActionResult<TicketResponse>> BuyTicketAsync([FromRoute] int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int clientId))
            {
                return Unauthorized("No se pudo identificar al cliente.");
            }
            var ticket = await _ticketService.BuyTicketAsync(id, clientId);
            return Ok(ticket);
        }

        [HttpGet("showtime/{showTimeId}/available")]
        public async Task<ActionResult<List<TicketResponse>>> GetAvailableTicketsAsync([FromRoute] int showTimeId)
        {
            var tickets = await _ticketService.GetAvailableTicketsAsync(showTimeId);
            return Ok(tickets);
        }
    }
}
