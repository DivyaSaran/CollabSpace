using System;

namespace CollabSpace.DTOs
{
    // Used when displaying a workspace card or summary in the UI
    public class WorkspaceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty; // e.g. "WorkspaceManager", "Member"
        public DateTime CreatedAt { get; set; }
    }

    // Used when submitting the form to create or edit a workspace
    public class WorkspaceCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Used when displaying members inside a workspace table
    public class WorkspaceMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WorkspaceRole { get; set; } = "Member";
        public DateTime JoinedAt { get; set; }
    }

    public class WorkspaceInviteDto
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string WorkspaceDescription { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    public class SendInviteDto
    {
        public string ReceiverEmail { get; set; } = string.Empty;
    }

    public class RespondInviteDto
    {
        public bool Accept { get; set; }
    }

    public class WorkspaceAccessRequestDto
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string RequesterUserId { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string RequesterEmail { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; }
    }
}