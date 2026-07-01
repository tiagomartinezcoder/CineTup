using CineTup.Application.Abstractions;
using CineTup.Application.Exceptions;
using CineTup.Application.Responses;
using CineTup.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CineTup.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetAllAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            if (users == null || users.Count == 0)
                return NotFound("No se encontraron usuarios.");
            return Ok(users);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync([FromRoute] int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/assign-role")]
        public async Task<ActionResult> AssignRoleAsync(
            [FromRoute] int id, 
            [FromQuery] string currentRole, 
            [FromQuery] string newRole)
        {
            await _userService.AssignRoleAsync(id, currentRole, newRole);
            return NoContent();
        }
    }
}
