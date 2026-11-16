using System.Collections.Generic;
using System.Threading.Tasks;
using CollabSpace.DTOs;

namespace CollabSpace.Repositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<(bool Succeeded, string Message)> CreateRoleAsync(string roleName);
        Task<(bool Succeeded, string Message)> DeleteRoleAsync(string roleName);
        Task<IEnumerable<UserRoleAssignmentDto>> GetUsersWithRolesAsync();
        Task<(bool Succeeded, string Message)> AssignUserRolesAsync(string userId, IEnumerable<string> selectedRoles);
    }
}