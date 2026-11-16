using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CollabSpace.DTOs;
using CollabSpace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UserRepository(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize)
        {
            var query = _userManager.Users.AsNoTracking();
            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var dto = _mapper.Map<UserDto>(user);
                dto.Roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(dto);
            }

            return (userDtos, totalCount);
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = await _userManager.GetRolesAsync(user);
            return dto;
        }

        public async Task<IdentityResultStatus> CreateUserAsync(UserCreateUpdateDto dto)
        {
            var user = _mapper.Map<User>(dto);

            // Ensure a valid GUID string exists for the primary key
            // else errors out during adding a new user
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            var result = await _userManager.CreateAsync(user, dto.Password!);

            if (!result.Succeeded)
            {
                return new IdentityResultStatus(false, result.Errors.Select(e => e.Description));
            }

            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Member" : dto.Role;
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            await _userManager.AddToRoleAsync(user, role);
            return new IdentityResultStatus(true, Enumerable.Empty<string>());
        }

        public async Task<IdentityResultStatus> UpdateUserAsync(UserCreateUpdateDto dto)
        {
            if (string.IsNullOrEmpty(dto.Id))
            {
                return new IdentityResultStatus(false, new[] { "User Id is required for updating." });
            }

            var existingUser = await _userManager.FindByIdAsync(dto.Id);
            if (existingUser == null)
            {
                return new IdentityResultStatus(false, new[] { "User not found." });
            }

            existingUser.UserName = dto.UserName;
            existingUser.Name = dto.Name;
            existingUser.Email = dto.Email;

            var updateResult = await _userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded)
            {
                return new IdentityResultStatus(false, updateResult.Errors.Select(e => e.Description));
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var resetResult = await _userManager.ResetPasswordAsync(existingUser, token, dto.Password);
                if (!resetResult.Succeeded)
                {
                    return new IdentityResultStatus(false, resetResult.Errors.Select(e => e.Description));
                }
            }

            return new IdentityResultStatus(true, Enumerable.Empty<string>());
        }

        public async Task<IdentityResultStatus> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new IdentityResultStatus(false, new[] { "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);
            return new IdentityResultStatus(result.Succeeded, result.Errors.Select(e => e.Description));
        }
    }
}