namespace CollabSpace.Models
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
        public ICollection<WorkspaceInvite> Invites { get; set; } = new List<WorkspaceInvite>();
    }

    public class WorkspaceMember
    {
        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        // Role in this specific workspace: "WorkspaceAdmin" or "Member"
        public string WorkspaceRole { get; set; } = "Member";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    public class WorkspaceInvite
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        // The user who receives the invite
        public string ReceiverUserId { get; set; } = string.Empty;
        public User ReceiverUser { get; set; } = null!;

        // The Admin who created the invite
        public string SenderUserId { get; set; } = string.Empty;
        public User SenderUser { get; set; } = null!;

        // Status tracking: "Pending", "Accepted", "Declined"
        public string Status { get; set; } = "Pending";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
