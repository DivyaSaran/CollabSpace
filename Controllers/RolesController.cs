using CollabSpace.DTOs;
using CollabSpace.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Strictly Admin only
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepo;

        public RolesController(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        // GET: /api/Roles
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleRepo.GetAllRolesAsync();
            return Ok(roles);
        }

        // POST: /api/Roles
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var (succeeded, message) = await _roleRepo.CreateRoleAsync(dto.RoleName);
            if (!succeeded)
            {
                return BadRequest(new { message });
            }
            return Ok(new { message });
        }

        // DELETE: /api/Roles/{roleName}
        [HttpDelete("{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var (succeeded, message) = await _roleRepo.DeleteRoleAsync(roleName);
            if (!succeeded)
            {
                return BadRequest(new { message });
            }
            return Ok(new { message });
        }

        // GET: /api/Roles/user-assignments
        [HttpGet("user-assignments")]
        public async Task<IActionResult> GetUserRoleAssignments()
        {
            var assignments = await _roleRepo.GetUsersWithRolesAsync();
            return Ok(assignments);
        }

        // POST: /api/Roles/assign
        [HttpPost("assign")]
        public async Task<IActionResult> AssignUserRoles([FromBody] UpdateUserRolesDto dto)
        {
            var (succeeded, message) = await _roleRepo.AssignUserRolesAsync(dto.UserId, dto.Roles);
            if (!succeeded)
            {
                return BadRequest(new { message });
            }
            return Ok(new { message });
        }
    }
}