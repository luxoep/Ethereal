namespace Ethereal_api.Dto;

public class Dtos
{
    public class EtherealUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}