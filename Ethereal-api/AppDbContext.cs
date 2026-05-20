using Ethereal_api.IService;
using Microsoft.EntityFrameworkCore;

namespace Ethereal_api;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Entities.EtherealStatus> ethereal_status { get; set; }
    public DbSet<Entities.EtherealUser> ethereal_user { get; set; }
    public DbSet<Entities.EtherealRecord> ethereal_record { get; set; }
    public DbSet<Entities.EtherealAttachment> ethereal_attachment { get; set; }
    public DbSet<Entities.EtherealComment> ethereal_comment { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.EtherealUser>(e =>
        {
            e.ToTable("ethereal_user");
            e.HasIndex(u => u.UserName).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });
        modelBuilder.Entity<Entities.EtherealUser>()
            .HasData(new Entities.EtherealUser
            {
                UserId = 1,
                UserName = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Email = "admin@123",
                FullName = "System Admin",
                Role = "Admin",
                Department = "IT",
                Phone = "0000",
                Position = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        modelBuilder.Entity<Entities.EtherealRecord>(e =>
        {
            e.ToTable("ethereal_record");
            e.HasOne(r => r.Assignee) // 一条记录有一个负责人
                .WithMany(u => u.AssignedRecords) // 一个用户有多个负责的记录
                .HasForeignKey(r => r.AssigneeUserId) // 外键是AssigneeUserId
                .OnDelete(DeleteBehavior.SetNull); // 用户删除后，记录的负责人置空
            e.HasOne(r => r.Creator)
                .WithMany(u => u.CreatedRecords)
                .HasForeignKey(r => r.CreatorRecordUserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Status)
                .WithMany(s => s.Records)
                .HasForeignKey(r => r.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Entities.EtherealAttachment>(e =>
        {
            e.ToTable("ethereal_attachment");
            e.HasOne(a => a.Record)
                .WithMany(r => r.Attachments)
                .HasForeignKey(r => r.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.User)
                .WithMany(u => u.Attachments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Entities.EtherealComment>(e =>
        {
            e.ToTable("ethereal_comment");
            e.HasOne(c => c.Record)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}