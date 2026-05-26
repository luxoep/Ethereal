using Ethereal_api.Dto;
using Microsoft.EntityFrameworkCore;

namespace Ethereal_api.IService;

public class ReviewApi
{
    public class EtherealUserApi(AppDbContext appDbContext) : IEtherealUserService
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<List<Dtos.EtherealUserDto>> GetEtherealUsers()
        {
            List<Dtos.EtherealUserDto> query = new List<Dtos.EtherealUserDto>();

            List<Entities.EtherealUser> modelData = await _appDbContext.ethereal_user.ToListAsync();
            foreach (Entities.EtherealUser user in modelData)
            {
                query.Add(new Dtos.EtherealUserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Department = user.Department,
                    Phone = user.Phone,
                    Position = user.Position,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                });
            }

            return query;
        }

        public async Task<Dtos.EtherealUserDto> GetEtherealUser(int id)
        {
            Entities.EtherealUser? modelData = await _appDbContext.ethereal_user.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (modelData == null)
                throw new UnauthorizedAccessException("id is not found");

            return new Dtos.EtherealUserDto
            {
                UserId = modelData.UserId,
                UserName = modelData.UserName,
                Email = modelData.Email,
                FullName = modelData.FullName,
                Role = modelData.Role,
                Department = modelData.Department,
                Phone = modelData.Phone,
                Position = modelData.Position,
                IsActive = modelData.IsActive,
                CreatedAt = modelData.CreatedAt,
            };
        }

        public async Task<Dtos.EtherealUserDto> CreateEtherealUser(Dtos.CreateEtherealUserDto addEtherealUser)
        {
            Entities.EtherealUser? modelData = await _appDbContext.ethereal_user
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == addEtherealUser.Email);

            if (modelData != null)
                throw new UnauthorizedAccessException("An email address already exists.");

            Dtos.EtherealUserDto newUser = new Dtos.EtherealUserDto()
            {
                UserName = addEtherealUser.UserName,
                Email = addEtherealUser.Email,
                FullName = addEtherealUser.FullName,
                Role = addEtherealUser.Role,
                Department = addEtherealUser.Department,
                Phone = addEtherealUser.Phone,
                Position = addEtherealUser.Position,
            };

            _appDbContext.Add(newUser);
            await _appDbContext.SaveChangesAsync();

            return newUser;
        }

        public async Task<Dtos.EtherealUserDto> UpdateEtherealUser(int id,
            Dtos.UpdateEtherealUserDto updateEtherealUser)
        {
            Entities.EtherealUser? modelData = await _appDbContext.ethereal_user.FindAsync(id);

            if (modelData == null)
                throw new UnauthorizedAccessException("User is not found");

            if (updateEtherealUser.Email != null) modelData.Email = updateEtherealUser.Email;
            modelData.FullName = updateEtherealUser.FullName;
            modelData.Role = updateEtherealUser.Role;
            modelData.Department = updateEtherealUser.Department;
            modelData.Phone = updateEtherealUser.Phone;
            modelData.Position = updateEtherealUser.Position;
            modelData.IsActive = updateEtherealUser.IsActive;

            return new Dtos.EtherealUserDto()
            {
                UserName = modelData.UserName,
                Email = modelData.Email,
                FullName = modelData.FullName,
                Role = modelData.Role,
                Department = modelData.Department,
                Phone = modelData.Phone,
                Position = modelData.Position,
                IsActive = modelData.IsActive,
            };
        }

        public async Task<Response.ApiResponse<string>> ChangePassword(int id, Dtos.ChangePasswordDto changePassword)
        {
            Entities.EtherealUser? modelData = await _appDbContext.ethereal_user.FindAsync(id);

            if (modelData == null)
                throw new UnauthorizedAccessException("User is not found");

            if (string.IsNullOrEmpty(changePassword.NewPassword) ||
                !BCrypt.Net.BCrypt.Verify(changePassword.CurrentPassword, changePassword.NewPassword))
            {
                return new Response.ApiErrorResponse<string>("Current password is incorrect.");
            }

            modelData.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePassword.NewPassword);
            await _appDbContext.SaveChangesAsync();

            return new Response.ApiSuccessResponse<string>("Password changed successfully");
        }
    }
}