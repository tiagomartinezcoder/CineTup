using CineTup.Application.Abstractions;
using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CineTup.Presentation.Controllers
{
    [Authorize(Policy = Policies.SysAdminOnly)]
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

        [HttpPut("{id}/role")]
        public async Task<ActionResult> UpdateRoleAsync([FromRoute] int id, [FromBody] UpdateRoleRequest request)
        {
            await _userService.UpdateRoleAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync([FromRoute] int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
