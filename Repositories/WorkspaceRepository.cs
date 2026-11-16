using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CollabSpace.Data;
using CollabSpace.DTOs;
using CollabSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public WorkspaceRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(string userId)
        {
            var memberships = await _context.WorkspaceMembers
                .AsNoTracking()
                .Include(wm => wm.Workspace)
                .Where(wm => wm.UserId == userId)
                .ToListAsync();

            return memberships.Select(wm =>
            {
                var dto = _mapper.Map<WorkspaceDto>(wm.Workspace);
                dto.CurrentUserRole = wm.WorkspaceRole;
                return dto;
            });
        }

        public async Task<IEnumerable<WorkspaceDto>> GetAllWorkspacesAsync(string currentUserId)
        {
            var workspaces = await _context.Workspaces
                .AsNoTracking()
                .Include(w => w.Members)
                .ToListAsync();

            return workspaces.Select(w =>
            {
                var dto = _mapper.Map<WorkspaceDto>(w);
                var membership = w.Members.FirstOrDefault(m => m.UserId == currentUserId);
                dto.CurrentUserRole = membership?.WorkspaceRole ?? "None";
                return dto;
            });
        }

        public async Task<Workspace?> GetByIdAsync(int id)
        {
            return await _context.Workspaces.FindAsync(id);
        }

        public async Task<WorkspaceDto?> GetWorkspaceDetailsAsync(int id, string currentUserId)
        {
            var ws = await _context.Workspaces
                .AsNoTracking()
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (ws == null) return null;

            var dto = _mapper.Map<WorkspaceDto>(ws);
            var membership = ws.Members.FirstOrDefault(m => m.UserId == currentUserId);
            dto.CurrentUserRole = membership?.WorkspaceRole ?? "None";
            return dto;
        }

        public async Task<WorkspaceDto> CreateWorkspaceAsync(WorkspaceCreateDto dto, string ownerId)
        {
            var ws = _mapper.Map<Workspace>(dto);
            ws.OwnerId = ownerId;
            ws.CreatedAt = DateTime.UtcNow;

            _context.Workspaces.Add(ws);
            await _context.SaveChangesAsync();

            // Automatically set the workspace creator as WorkspaceManager
            var member = new WorkspaceMember
            {
                WorkspaceId = ws.Id,
                UserId = ownerId,
                WorkspaceRole = "WorkspaceManager",
                JoinedAt = DateTime.UtcNow
            };

            _context.WorkspaceMembers.Add(member);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<WorkspaceDto>(ws);
            result.CurrentUserRole = "WorkspaceManager";
            return result;
        }

        public async Task<bool> UpdateWorkspaceAsync(int id, WorkspaceCreateDto dto)
        {
            var ws = await _context.Workspaces.FindAsync(id);
            if (ws == null) return false;

            ws.Name = dto.Name;
            ws.Description = dto.Description;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteWorkspaceAsync(int id)
        {
            var ws = await _context.Workspaces.FindAsync(id);
            if (ws == null) return false;

            _context.Workspaces.Remove(ws);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<WorkspaceMemberDto>> GetMembersAsync(int workspaceId)
        {
            var members = await _context.WorkspaceMembers
                .AsNoTracking()
                .Include(wm => wm.User)
                .Where(wm => wm.WorkspaceId == workspaceId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WorkspaceMemberDto>>(members);
        }

        public async Task<bool> AddMemberAsync(int workspaceId, string userId, string role)
        {
            var exists = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

            if (exists) return false;

            _context.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId = userId,
                WorkspaceRole = role,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMemberRoleAsync(int workspaceId, string userId, string newRole)
        {
            var member = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

            if (member == null) return false;

            member.WorkspaceRole = newRole;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int workspaceId, string userId)
        {
            var member = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

            if (member == null) return false;

            _context.WorkspaceMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetMemberRoleAsync(int workspaceId, string userId)
        {
            var member = await _context.WorkspaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

            return member?.WorkspaceRole;
        }

        public async Task<(bool Succeeded, string Message)> RequestWorkspaceAccessAsync(int workspaceId, string userId)
        {
            // Check if workspace exists
            var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
            if (!workspaceExists)
            {
                return (false, "Workspace not found.");
            }

            // Check if user is already a member
            var isMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);
            if (isMember)
            {
                return (false, "You are already a member of this workspace.");
            }

            // Check if there is already an active pending request
            var existingRequest = await _context.WorkspaceAccessRequests
                .AnyAsync(ar => ar.WorkspaceId == workspaceId && ar.RequesterUserId == userId && ar.Status == "Pending");
            if (existingRequest)
            {
                return (false, "You already have a pending access request for this workspace.");
            }

            var request = new WorkspaceAccessRequest
            {
                WorkspaceId = workspaceId,
                RequesterUserId = userId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _context.WorkspaceAccessRequests.Add(request);
            await _context.SaveChangesAsync();

            return (true, "Access request submitted successfully.");
        }

        public async Task<IEnumerable<WorkspaceAccessRequestDto>> GetPendingAccessRequestsForWorkspaceAsync(int workspaceId)
        {
            var requests = await _context.WorkspaceAccessRequests
                .AsNoTracking()
                .Include(ar => ar.Workspace)
                .Include(ar => ar.RequesterUser)
                .Where(ar => ar.WorkspaceId == workspaceId && ar.Status == "Pending")
                .ToListAsync();

            return _mapper.Map<IEnumerable<WorkspaceAccessRequestDto>>(requests);
        }

        public async Task<IEnumerable<WorkspaceAccessRequestDto>> GetUserAccessRequestsAsync(string userId)
        {
            var requests = await _context.WorkspaceAccessRequests
                .AsNoTracking()
                .Include(ar => ar.Workspace)
                .Include(ar => ar.RequesterUser)
                .Where(ar => ar.RequesterUserId == userId)
                .OrderByDescending(ar => ar.RequestedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WorkspaceAccessRequestDto>>(requests);
        }

        public async Task<(bool Succeeded, string Message)> ReviewAccessRequestAsync(int requestId, string reviewerUserId, bool approve)
        {
            var request = await _context.WorkspaceAccessRequests
                .Include(ar => ar.Workspace)
                .FirstOrDefaultAsync(ar => ar.Id == requestId && ar.Status == "Pending");

            if (request == null)
            {
                return (false, "Access request not found or already processed.");
            }

            // Verify reviewer is either the WorkspaceManager or a Global Admin
            var reviewerMembership = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == request.WorkspaceId && wm.UserId == reviewerUserId);

            var isWorkspaceManager = reviewerMembership?.WorkspaceRole == "WorkspaceManager";

            // We can also allow global admins to approve (Requirement 2)
            var reviewerUser = await _context.Users.FindAsync(reviewerUserId);
            var isGlobalAdmin = reviewerUser != null && await _context.UserRoles
                .AnyAsync(ur => ur.UserId == reviewerUserId); // Checked against identity roles

            if (!isWorkspaceManager && !isGlobalAdmin)
            {
                return (false, "You do not have permission to review access requests for this workspace.");
            }

            request.Status = approve ? "Approved" : "Rejected";
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = reviewerUserId;

            if (approve)
            {
                // Add requester as a Member
                _context.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = request.WorkspaceId,
                    UserId = request.RequesterUserId,
                    WorkspaceRole = "Member",
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return (true, approve ? "Request approved and member added." : "Request rejected.");
        }

        public async Task<(bool Succeeded, string Message)> SendInviteAsync(
    int workspaceId,
    string senderUserId,
    string receiverEmail,
    bool isGlobalAdmin)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return (false, "Workspace not found.");
            }

            // Permission Check:
            // Sender must be:
            // 1. A Global Admin (can invite anyone to ANY workspace), OR
            // 2. The WorkspaceManager / Owner of this specific workspace
            var senderRole = await GetMemberRoleAsync(workspaceId, senderUserId);
            var isManagerOrOwner = senderRole == "WorkspaceManager" || workspace.OwnerId == senderUserId;

            if (!isGlobalAdmin && !isManagerOrOwner)
            {
                return (false, "You do not have permission to invite members to this workspace.");
            }

            var receiverUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == receiverEmail);
            if (receiverUser == null)
            {
                return (false, $"No registered user found with email '{receiverEmail}'.");
            }

            var isAlreadyMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == receiverUser.Id);

            if (isAlreadyMember)
            {
                return (false, "This user is already a member of this workspace.");
            }

            var hasPendingInvite = await _context.WorkspaceInvites
                .AnyAsync(wi => wi.WorkspaceId == workspaceId && wi.ReceiverUserId == receiverUser.Id && wi.Status == "Pending");

            if (hasPendingInvite)
            {
                return (false, "An invitation is already pending for this user.");
            }

            var invite = new WorkspaceInvite
            {
                WorkspaceId = workspaceId,
                SenderUserId = senderUserId,
                ReceiverUserId = receiverUser.Id,
                Status = "Pending",
                SentAt = DateTime.UtcNow
            };

            _context.WorkspaceInvites.Add(invite);
            await _context.SaveChangesAsync();

            return (true, $"Invitation sent to {receiverEmail}.");
        }

        public async Task<IEnumerable<WorkspaceInviteDto>> GetUserPendingInvitesAsync(string userId)
        {
            var invites = await _context.WorkspaceInvites
                .AsNoTracking()
                .Include(wi => wi.Workspace)
                .Include(wi => wi.SenderUser)
                .Where(wi => wi.ReceiverUserId == userId && wi.Status == "Pending")
                .ToListAsync();

            return _mapper.Map<IEnumerable<WorkspaceInviteDto>>(invites);
        }

        public async Task<(bool Succeeded, string Message)> RespondToInviteAsync(int inviteId, string userId, bool accept)
        {
            var invite = await _context.WorkspaceInvites
                .FirstOrDefaultAsync(wi => wi.Id == inviteId && wi.ReceiverUserId == userId && wi.Status == "Pending");

            if (invite == null)
            {
                return (false, "Invitation not found or already processed.");
            }

            if (accept)
            {
                invite.Status = "Accepted";

                var isAlreadyMember = await _context.WorkspaceMembers
                    .AnyAsync(wm => wm.WorkspaceId == invite.WorkspaceId && wm.UserId == userId);

                if (!isAlreadyMember)
                {
                    _context.WorkspaceMembers.Add(new WorkspaceMember
                    {
                        WorkspaceId = invite.WorkspaceId,
                        UserId = userId,
                        WorkspaceRole = "Member",
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                invite.Status = "Declined";
            }

            await _context.SaveChangesAsync();
            return (true, accept ? "Invitation accepted!" : "Invitation declined.");
        }
    }
}