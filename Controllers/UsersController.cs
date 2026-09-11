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

        [HttpGet] 
        public ActionResult<IEnumerable<User>> GetAllUsers() 
        { return _context.Users.ToList(); 
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null) 
            {
                return NotFound(new { messahe = $"User with user id {id} is not found" });
            }
            return user;
        }

        [HttpPost]
        public ActionResult<User> CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public ActionResult<User> UpdateUser(int id, User user)
        {
            if(id != user.Id)
            {
                return BadRequest(new { message = "User ID mismatch" });
            }

            user.Name = user.Name;
            user.Username = user.Username;
            user.Email = user.Email;
            user.Role = user.Role;

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                user.Password = user.Password;
            }
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
