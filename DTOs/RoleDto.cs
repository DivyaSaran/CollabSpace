using System.Collections.Generic;

namespace CollabSpace.DTOs
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class CreateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserRoleAssignmentDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> AssignedRoles { get; set; } = new List<string>();
    }

    public class UpdateUserRolesDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}