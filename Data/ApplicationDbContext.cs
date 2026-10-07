using CollabSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace CollabSpace.Data
{
    // Inheriting from IdentityDbContext<User> brings in all Identity tables
    // and configures our custom User class
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
        public DbSet<WorkspaceInvite> WorkspaceInvites => Set<WorkspaceInvite>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Note :::: CRITICAL: Always call base.OnModelCreating first.
            // This configures the primary keys, indexes, and relations for the Identity tables.
            base.OnModelCreating(builder);

            builder.Entity<WorkspaceMember>()
                .HasKey(wm => new { wm.WorkspaceId, wm.UserId });

            builder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.User)
                .WithMany()
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure WorkspaceInvite relationships
            builder.Entity<WorkspaceInvite>()
                .HasOne(wi => wi.Workspace)
                .WithMany(w => w.Invites)
                .HasForeignKey(wi => wi.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WorkspaceInvite>()
                .HasOne(wi => wi.ReceiverUser)
                .WithMany()
                .HasForeignKey(wi => wi.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkspaceInvite>()
                .HasOne(wi => wi.SenderUser)
                .WithMany()
                .HasForeignKey(wi => wi.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    
    }
}

/** Commenting my old code after switching to IdentityDbContext<User>
**
*
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }

**/