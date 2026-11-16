using System.Collections.Generic;
using System.Threading.Tasks;
using CollabSpace.DTOs;
using CollabSpace.Models;

namespace CollabSpace.Repositories
{
    public interface IWorkspaceRepository
    {
        // --------------------------------------------------
        // Workspace Query Operations
        // --------------------------------------------------
      
        /// Retrieves all workspaces where the given user is an active member.
        Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(string userId);

        /// Retrieves all workspaces in the system (used by Admins to manage all workspaces).
        Task<IEnumerable<WorkspaceDto>> GetAllWorkspacesAsync(string currentUserId);

        /// Retrieves a raw Workspace entity by Id for internal operations.
        Task<Workspace?> GetByIdAsync(int id);

        /// Retrieves detailed information for a single workspace, mapped to DTO.
        Task<WorkspaceDto?> GetWorkspaceDetailsAsync(int id, string currentUserId);

        // --------------------------------------------------
        // Workspace Mutation Operations
        // --------------------------------------------------
        /// Creates a new workspace and assigns the creator as WorkspaceManager.
        Task<WorkspaceDto> CreateWorkspaceAsync(WorkspaceCreateDto dto, string ownerId);

        /// Updates the name and description of an existing workspace.
        Task<bool> UpdateWorkspaceAsync(int id, WorkspaceCreateDto dto);

        /// Deletes a workspace by Id.
        Task<bool> DeleteWorkspaceAsync(int id);

        // --------------------------------------------------
        // Member Management Operations
        // --------------------------------------------------
        /// Lists all members belonging to a workspace with their roles.
        Task<IEnumerable<WorkspaceMemberDto>> GetMembersAsync(int workspaceId);

        /// Adds a user to a workspace with a specified role.
        Task<bool> AddMemberAsync(int workspaceId, string userId, string role);

        /// Updates a member's role within a workspace (e.g., from Member to WorkspaceManager).
        Task<bool> UpdateMemberRoleAsync(int workspaceId, string userId, string newRole);

        /// Removes a member from a workspace.
        Task<bool> RemoveMemberAsync(int workspaceId, string userId);

        /// Checks the current role of a user in a given workspace. Returns null if not a member.
        Task<string?> GetMemberRoleAsync(int workspaceId, string userId);

        // Outbound Invitations
        Task<(bool Succeeded, string Message)> SendInviteAsync(int workspaceId, string senderUserId, string receiverEmail, bool isGlobalAdmin);
        Task<IEnumerable<WorkspaceInviteDto>> GetUserPendingInvitesAsync(string userId);
        Task<(bool Succeeded, string Message)> RespondToInviteAsync(int inviteId, string userId, bool accept);

        // Access Requests
        Task<(bool Succeeded, string Message)> RequestWorkspaceAccessAsync(int workspaceId, string userId);
        Task<IEnumerable<WorkspaceAccessRequestDto>> GetPendingAccessRequestsForWorkspaceAsync(int workspaceId);
        Task<IEnumerable<WorkspaceAccessRequestDto>> GetUserAccessRequestsAsync(string userId);
        Task<(bool Succeeded, string Message)> ReviewAccessRequestAsync(int requestId, string reviewerUserId, bool approve);

    }
}