using System.Security.Claims;
using CollabSpace.Data;
using CollabSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires the user to be signed in
    public class WorkspacesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public WorkspacesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper: retrieve the currently logged-in user's GUID ID
        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // GET: /api/Workspaces/my-workspaces
        // Returns all workspaces the logged-in user belongs to
        [HttpGet("my-workspaces")]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var userId = GetCurrentUserId();

            var workspaces = await _context.WorkspaceMembers
                .Where(wm => wm.UserId == userId)
                .Select(wm => new
                {
                    wm.Workspace.Id,
                    wm.Workspace.Name,
                    wm.Workspace.Description,
                    wm.WorkspaceRole,
                    wm.Workspace.CreatedAt
                })
                .ToListAsync();

            return Ok(workspaces);
        }

        // POST: /api/Workspaces
        // Creates a new workspace (Admin only) 
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Workspace name is required." });
            }

            var userId = GetCurrentUserId();

            var workspace = new Workspace
            {
                Name = dto.Name,
                Description = dto.Description,
                OwnerId = userId
            };

            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();

            // Automatically register the creator as WorkspaceAdmin
            var membership = new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserId = userId,
                WorkspaceRole = "WorkspaceAdmin"
            };

            _context.WorkspaceMembers.Add(membership);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Workspace created successfully.", workspace.Id });
        }

        // GET: /api/Workspaces/{workspaceId}/members
        // View all members in a given workspace
        [HttpGet("{workspaceId}/members")]
        public async Task<IActionResult> GetWorkspaceMembers(int workspaceId)
        {
            var userId = GetCurrentUserId();

            // Ensure the requesting user is a member of the workspace
            var isMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

            if (!isMember)
            {
                return Forbid();
            }

            var members = await _context.WorkspaceMembers
                .Where(wm => wm.WorkspaceId == workspaceId)
                .Select(wm => new
                {
                    wm.UserId,
                    wm.User.UserName,
                    wm.User.Name,
                    wm.User.Email,
                    wm.WorkspaceRole,
                    wm.JoinedAt
                })
                .ToListAsync();

            return Ok(members);
        }

        // POST: /api/Workspaces/{workspaceId}/invite
        // Admin sends an invitation to another user via email
        [HttpPost("{workspaceId}/invite")]
        public async Task<IActionResult> SendInvite(int workspaceId, [FromBody] SendInviteDto dto)
        {
            var senderId = GetCurrentUserId();

            // 1. Verify the sender is a WorkspaceAdmin for this workspace
            var senderMembership = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == senderId);

            if (senderMembership == null || senderMembership.WorkspaceRole != "WorkspaceAdmin")
            {
                return Forbid();
            }

            // 2. Find the user to invite by email
            var receiverUser = await _userManager.FindByEmailAsync(dto.ReceiverEmail);
            if (receiverUser == null)
            {
                return NotFound(new { message = $"No user found with email '{dto.ReceiverEmail}'." });
            }

            // 3. Check if the user is already a member
            var alreadyMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == receiverUser.Id);

            if (alreadyMember)
            {
                return Conflict(new { message = "This user is already a member of this workspace." });
            }

            // 4. Check if a pending invite already exists
            var pendingInvite = await _context.WorkspaceInvites
                .AnyAsync(wi => wi.WorkspaceId == workspaceId && wi.ReceiverUserId == receiverUser.Id && wi.Status == "Pending");

            if (pendingInvite)
            {
                return Conflict(new { message = "An invitation is already pending for this user." });
            }

            // 5. Create the invite
            var invite = new WorkspaceInvite
            {
                WorkspaceId = workspaceId,
                SenderUserId = senderId,
                ReceiverUserId = receiverUser.Id,
                Status = "Pending"
            };

            _context.WorkspaceInvites.Add(invite);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Invitation sent to {dto.ReceiverEmail}." });
        }

        // GET: /api/Workspaces/invites/my-invites
        // Returns all pending invites for the logged-in user
        [HttpGet("invites/my-invites")]
        public async Task<IActionResult> GetMyPendingInvites()
        {
            var userId = GetCurrentUserId();

            var invites = await _context.WorkspaceInvites
                .Where(wi => wi.ReceiverUserId == userId && wi.Status == "Pending")
                .Select(wi => new
                {
                    wi.Id,
                    wi.WorkspaceId,
                    WorkspaceName = wi.Workspace.Name,
                    WorkspaceDescription = wi.Workspace.Description,
                    SenderName = wi.SenderUser.Name,
                    wi.SentAt
                })
                .ToListAsync();

            return Ok(invites);
        }

        // POST: /api/Workspaces/invites/{inviteId}/respond
        // Accept or decline an invitation
        [HttpPost("invites/{inviteId}/respond")]
        public async Task<IActionResult> RespondToInvite(int inviteId, [FromBody] RespondInviteDto dto)
        {
            var userId = GetCurrentUserId();

            var invite = await _context.WorkspaceInvites
                .FirstOrDefaultAsync(wi => wi.Id == inviteId && wi.ReceiverUserId == userId && wi.Status == "Pending");

            if (invite == null)
            {
                return NotFound(new { message = "Invitation not found or already processed." });
            }

            if (dto.Accept)
            {
                invite.Status = "Accepted";

                // Add user to the workspace members list
                var membership = new WorkspaceMember
                {
                    WorkspaceId = invite.WorkspaceId,
                    UserId = userId,
                    WorkspaceRole = "Member"
                };

                _context.WorkspaceMembers.Add(membership);
            }
            else
            {
                invite.Status = "Declined";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = dto.Accept ? "Invitation accepted!" : "Invitation declined." });
        }
    }

    // DTO Classes
    public class CreateWorkspaceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SendInviteDto
    {
        public string ReceiverEmail { get; set; } = string.Empty;
    }

    public class RespondInviteDto
    {
        public bool Accept { get; set; }
    }
}