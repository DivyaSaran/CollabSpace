using CollabSpace.Models;
using Microsoft.AspNetCore.Mvc;
using CollabSpace.Data;
using Microsoft.EntityFrameworkCore;


namespace CollabSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/Users?pageNumber=1&pageSize=5
        [HttpGet]
        public async Task<ActionResult<Pagination<User>>> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            // Protect against zero or negative values
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50; // Guard against requesting too much data at once

            var query = _context.Users.AsNoTracking();
            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize) // Fixed: (pageNumber - 1)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new Pagination<User>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = users
            });
        }

        // GET: /api/Users/entered-guid-id
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            // Notice id is now a string to match IdentityUser.Id
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            return Ok(user);
        }

        // DELETE: /api/Users/entered-guid-id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}



/** commenting out previous code after switching to IdentityUser 
 * as there were to many errors showing up **/




/*namespace CollabSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet] 
        public async Task<ActionResult<Pagination<User>>> GetAllUsers(int pageNumber = 1, int pageSize = 5)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50; // Guard against excessive page sizes

            var query = _context.Users.AsNoTracking();
            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new Pagination<User>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = users
            });
        }
        

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) 
            {
                return NotFound(new { message = $"User with user id {id} is not found" });
            }
            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
           var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower());

            if (emailExists)
            {
                return BadRequest(new { message = "A user with this email already exists." });
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);

            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Database constraint violation: Email must be unique." });
            }

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(int id, User user)
        {
            if(id != user.Id)
            {
                return BadRequest(new { message = "User ID mismatch" });
            }
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            var emailTaken = await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.Id != id);
            if (emailTaken)
            {
                return Conflict(new { message = $"The email '{user.Email}' is already taken by another user." });
            }

            existingUser.Name = user.Name;
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}*/
