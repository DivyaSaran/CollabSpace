using System.Security.Claims;
using CollabSpace.DTOs;
using CollabSpace.Models;
using CollabSpace.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CollabSpace.Data;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires the user to be signed in
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceRepository _workspaceRepo;
        private readonly UserManager<User> _userManager;

        public WorkspacesController(
            IWorkspaceRepository workspaceRepo,
            UserManager<User> userManager)
        {
            _workspaceRepo = workspaceRepo;
            _userManager = userManager;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private bool IsGlobalAdmin() =>
            User.IsInRole("Admin");

        // ---------------------------------------------------------------------
        // Workspace Retrieval & Creation
        // ---------------------------------------------------------------------

        // GET: /api/Workspaces/my-workspaces
        [HttpGet("my-workspaces")]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var userId = GetCurrentUserId();

            if (IsGlobalAdmin())
            {
                var allWorkspaces = await _workspaceRepo.GetAllWorkspacesAsync(userId);
                return Ok(allWorkspaces);
            }

            var workspaces = await _workspaceRepo.GetUserWorkspacesAsync(userId);
            return Ok(workspaces);
        }

        // GET: /api/Workspaces/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllWorkspaces()
        {
            var userId = GetCurrentUserId();
            var allWorkspaces = await _workspaceRepo.GetAllWorkspacesAsync(userId);
            return Ok(allWorkspaces);
        }

        // POST: /api/Workspaces
        // Any member can create a workspace and becomes its WorkspaceManager
        [HttpPost]
        public async Task<IActionResult> CreateWorkspace([FromBody] WorkspaceCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Workspace name is required." });
            }

            var userId = GetCurrentUserId();
            var created = await _workspaceRepo.CreateWorkspaceAsync(dto, userId);

            return Ok(new { message = "Workspace created successfully.", id = created.Id });
        }

        // GET: /api/Workspaces/{workspaceId}/members
        [HttpGet("{workspaceId}/members")]
        public async Task<IActionResult> GetWorkspaceMembers(int workspaceId)
        {
            var userId = GetCurrentUserId();
            var role = await _workspaceRepo.GetMemberRoleAsync(workspaceId, userId);

            if (role == null && !IsGlobalAdmin())
            {
                return Forbid();
            }

            var members = await _workspaceRepo.GetMembersAsync(workspaceId);
            return Ok(members);
        }

        // ---------------------------------------------------------------------
        // Existing Feature Intact: Outbound Invitations
        // Admin can invite to ANY workspace; WorkspaceManager can invite to theirs
        // ---------------------------------------------------------------------

        // POST: /api/Workspaces/{workspaceId}/invite
        [HttpPost("{workspaceId}/invite")]
        public async Task<IActionResult> SendInvite(int workspaceId, [FromBody] SendInviteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ReceiverEmail))
            {
                return BadRequest(new { message = "Receiver email is required." });
            }

            var senderId = GetCurrentUserId();
            var (succeeded, message) = await _workspaceRepo.SendInviteAsync(
                workspaceId,
                senderId,
                dto.ReceiverEmail.Trim(),
                IsGlobalAdmin());

            if (!succeeded)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        // GET: /api/Workspaces/invites/my-invites
        [HttpGet("invites/my-invites")]
        public async Task<IActionResult> GetMyPendingInvites()
        {
            var userId = GetCurrentUserId();
            var invites = await _workspaceRepo.GetUserPendingInvitesAsync(userId);
            return Ok(invites);
        }

        // POST: /api/Workspaces/invites/{inviteId}/respond
        [HttpPost("invites/{inviteId}/respond")]
        public async Task<IActionResult> RespondToInvite(int inviteId, [FromBody] RespondInviteDto dto)
        {
            var userId = GetCurrentUserId();
            var (succeeded, message) = await _workspaceRepo.RespondToInviteAsync(inviteId, userId, dto.Accept);

            if (!succeeded)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        // ---------------------------------------------------------------------
        // New Feature: Inbound Access Requests & Approvals
        // ---------------------------------------------------------------------

        // POST: /api/Workspaces/{workspaceId}/request-access
        [HttpPost("{workspaceId}/request-access")]
        public async Task<IActionResult> RequestAccess(int workspaceId)
        {
            var userId = GetCurrentUserId();
            var (succeeded, message) = await _workspaceRepo.RequestWorkspaceAccessAsync(workspaceId, userId);

            if (!succeeded)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        // GET: /api/Workspaces/{workspaceId}/access-requests
        [HttpGet("{workspaceId}/access-requests")]
        public async Task<IActionResult> GetPendingRequests(int workspaceId)
        {
            var userId = GetCurrentUserId();
            var role = await _workspaceRepo.GetMemberRoleAsync(workspaceId, userId);

            if (role != "WorkspaceManager" && !IsGlobalAdmin())
            {
                return Forbid();
            }

            var requests = await _workspaceRepo.GetPendingAccessRequestsForWorkspaceAsync(workspaceId);
            return Ok(requests);
        }

        // POST: /api/Workspaces/access-requests/{requestId}/review
        [HttpPost("access-requests/{requestId}/review")]
        public async Task<IActionResult> ReviewAccessRequest(int requestId, [FromBody] ReviewRequestDto dto)
        {
            var reviewerId = GetCurrentUserId();
            var (succeeded, message) = await _workspaceRepo.ReviewAccessRequestAsync(requestId, reviewerId, dto.Approve);

            if (!succeeded)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        // ---------------------------------------------------------------------
        // Requirement 2: Granular Member Removal Enforcement
        // "Admins cannot delete other members' workspace users"
        // ---------------------------------------------------------------------

        // DELETE: /api/Workspaces/{workspaceId}/members/{targetUserId}
        [HttpDelete("{workspaceId}/members/{targetUserId}")]
        public async Task<IActionResult> RemoveMember(int workspaceId, string targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            var workspace = await _workspaceRepo.GetByIdAsync(workspaceId);

            if (workspace == null)
            {
                return NotFound(new { message = "Workspace not found." });
            }

            var currentRole = await _workspaceRepo.GetMemberRoleAsync(workspaceId, currentUserId);
            var isOwnerOrManager = currentRole == "WorkspaceManager" || workspace.OwnerId == currentUserId;

            if (!isOwnerOrManager)
            {
                if (IsGlobalAdmin() && workspace.OwnerId != currentUserId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        message = "Global Admins cannot remove members from workspaces owned by other users."
                    });
                }

                return Forbid();
            }

            var removed = await _workspaceRepo.RemoveMemberAsync(workspaceId, targetUserId);
            if (!removed)
            {
                return BadRequest(new { message = "Could not remove member from this workspace." });
            }

            return Ok(new { message = "Member removed successfully." });
        }
    }

    public class ReviewRequestDto
    {
        public bool Approve { get; set; }
    }
}







