using System.ComponentModel.DataAnnotations;

namespace Ethereal_api.IService;

public class Entities
{
    public class EtherealStatus
    {
        [Key] public int StatusId { get; set; }
        [MaxLength(20)] public string Name { get; set; } = null!;

        #region 导航属性(一对多关系)

        public ICollection<EtherealRecord> Records { get; set; } = new List<EtherealRecord>();

        #endregion
    }

    public class EtherealUser
    {
        [Key] public int UserId { get; set; }
        [MaxLength(20)] public string Username { get; set; } = null!;
        [MaxLength(256)] public string PasswordHash { get; set; } = null!;
        [MaxLength(100)] public string Email { get; set; } = null!;
        [MaxLength(100)] public string? FullName { get; set; }
        [MaxLength(20)] public string? Role { get; set; } = "Member";
        [MaxLength(100)] public string? Department { get; set; }
        [MaxLength(30)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Position { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(500)] public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        #region 导航属性(一对多关系)

        public ICollection<EtherealRecord> AssignedRecords { get; set; } = new List<EtherealRecord>();
        public ICollection<EtherealRecord> CreatedRecords { get; set; } = new List<EtherealRecord>();
        public ICollection<EtherealAttachment> Attachments { get; set; } = new List<EtherealAttachment>();
        public ICollection<EtherealComment> Comments { get; set; } = new List<EtherealComment>();

        #endregion
    }

    public class EtherealRecord
    {
        [Key] public int RecordId { get; set; }
        [MaxLength(50)] public string? SubNo { get; set; } // e.g. 26aa01-01
        [MaxLength(300)] public string? Title { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }
        public int StatusId { get; set; } = 1;
        [MaxLength(20)] public string Priority { get; set; } = "Medium";
        public int? AssigneeUserId { get; set; }
        public int? CreatorRecordUserId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int OrderSortNumber { get; set; }

        #region 导航属性(多对一关系)

        public EtherealUser? Assignee { get; set; }
        public EtherealUser? Creator { get; set; }
        public EtherealStatus? Status { get; set; }

        #endregion

        #region 导航属性(一对多关系)

        // 一个record有多个评论和附件
        public ICollection<EtherealAttachment> Attachments { get; set; } = new List<EtherealAttachment>();
        public ICollection<EtherealComment> Comments { get; set; } = new List<EtherealComment>();

        #endregion
    }

    public class EtherealAttachment
    {
        [Key] public int Id { get; set; }
        public int RecordId { get; set; }
        public int UserId { get; set; }
        [MaxLength(300)] public string FileName { get; set; } = null!;
        [MaxLength(1000)] public string FilePath { get; set; } = null!;
        public long FileSize { get; set; } = 0;
        [MaxLength(100)] public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        #region 导航属性(多对一关系)

        public EtherealRecord? Record { get; set; }
        public EtherealUser? User { get; set; }

        #endregion
    }

    public class EtherealComment
    {
        [Key] public int Id { get; set; }
        public int RecordId { get; set; }
        public int UserId { get; set; }
        [MaxLength(1000)] public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #region 导航属性(多对一关系)

        public EtherealRecord? Record { get; set; }
        public EtherealUser? User { get; set; }

        #endregion
    }
}