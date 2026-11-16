using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CollabSpace.DTOs;
using CollabSpace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleRepository(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            var roleDtos = new List<RoleDto>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleDtos.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    UserCount = usersInRole.Count
                });
            }

            return roleDtos;
        }

        public async Task<(bool Succeeded, string Message)> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return (false, "Role name cannot be empty.");
            }

            var trimmedName = roleName.Trim();
            if (await _roleManager.RoleExistsAsync(trimmedName))
            {
                return (false, $"Role '{trimmedName}' already exists.");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(trimmedName));
            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return (true, $"Role '{trimmedName}' created successfully.");
        }

        public async Task<(bool Succeeded, string Message)> DeleteRoleAsync(string roleName)
        {
            // Protect standard root roles from being deleted
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("Member", StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Default role '{roleName}' cannot be deleted.");
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return (false, "Role not found.");
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Any())
            {
                return (false, $"Cannot delete role '{roleName}' because {usersInRole.Count} user(s) are currently assigned to it.");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return (true, $"Role '{roleName}' deleted successfully.");
        }

        public async Task<IEnumerable<UserRoleAssignmentDto>> GetUsersWithRolesAsync()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var list = new List<UserRoleAssignmentDto>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                list.Add(new UserRoleAssignmentDto
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Name = user.Name,
                    Email = user.Email ?? string.Empty,
                    AssignedRoles = userRoles
                });
            }

            return list;
        }

        public async Task<(bool Succeeded, string Message)> AssignUserRolesAsync(string userId, IEnumerable<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

            // Guardrail: Ensure at least one role remains (e.g. Member)
            if (!selectedRoles.Any())
            {
                return (false, "A user must have at least one role assigned.");
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return (false, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
            }

            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return (false, string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }

            return (true, "Roles updated successfully.");
        }
    }
}