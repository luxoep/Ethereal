using Ethereal_api.IService;
using System.ComponentModel.DataAnnotations;

namespace Ethereal_api.Dto;

public class Dtos
{
    public class EtherealStatusDto
    {
        public int StatusId { get; set; }

        [Required] [MaxLength(20)] public string Name { get; set; } = null!;
    }

    public class EtherealUserDto
    {
        public int UserId { get; set; }

        [Required] [MaxLength(20)] public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [MaxLength(100)] public string? FullName { get; set; }

        [MaxLength(20)] public string? Role { get; set; }

        [MaxLength(100)] public string? Department { get; set; }

        [MaxLength(30)] public string? Phone { get; set; }

        [MaxLength(100)] public string? Position { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CreateEtherealUserDto
    {
        [Required] [MaxLength(20)] public string Username { get; set; } = null!;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [MaxLength(100)] public string? FullName { get; set; }

        // Member/System Admin/Manager/Owner
        [MaxLength(20)] public string? Role { get; set; }

        [MaxLength(100)] public string? Department { get; set; }

        [MaxLength(100)] public string? Position { get; set; }
    }

    public class UpdateEtherealUserDto
    {
        [EmailAddress] [MaxLength(100)] public string? Email { get; set; }

        [MaxLength(100)] public string? FullName { get; set; }

        [MaxLength(20)] public string? Role { get; set; }

        [MaxLength(100)] public string? Department { get; set; }

        [MaxLength(100)] public string? Position { get; set; }

        public bool IsActive { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required] public string CurrentPassword { get; set; } = null!;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string NewPassword { get; set; } = null!;
    }

    public class EtherealRecordDto
    {
        public int RecordId { get; set; }

        public string? SubNo { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public EtherealStatusDto? Status { get; set; }

        public string? Priority { get; set; }

        public EtherealUserDto? AssignedUser { get; set; }

        public EtherealUserDto? CreatedUser { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int OrderSortNumber { get; set; }
    }

    public class CreateEtherealRecordDto
    {
        [MaxLength(50)] public string? SubNo { get; set; }

        [Required] [MaxLength(300)] public string Title { get; set; } = null!;

        [MaxLength(1000)] public string? Description { get; set; }

        [Required] public int StatusId { get; set; }

        [MaxLength(20)] public string? Priority { get; set; }

        public int? AssigneeUserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public class UpdateEtherealRecordDto
    {
        [MaxLength(50)] public string? SubNo { get; set; }

        [MaxLength(300)] public string? Title { get; set; }

        [MaxLength(1000)] public string? Description { get; set; }

        public int? StatusId { get; set; }

        [MaxLength(20)] public string? Priority { get; set; }

        public int? AssigneeUserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int OrderSortNumber { get; set; }
    }

    public class UpdateCompletedRecordDto
    {
        public DateTime? CompletedAt { get; set; }
    }

    public class MoveEtherealRecordDto
    {
        [Range(0, int.MaxValue)] public int OrderSortNumber { get; set; }

        public DateTime StartDate { get; set; }
    }
}