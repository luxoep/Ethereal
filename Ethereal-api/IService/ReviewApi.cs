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
                throw new KeyNotFoundException("id is not found");

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
                .FirstOrDefaultAsync(u =>
                    u.Email == addEtherealUser.Email ||
                    u.UserName == addEtherealUser.UserName);

            if (modelData != null)
                throw new KeyNotFoundException("Username or email already exists.");

            Entities.EtherealUser entity = new Entities.EtherealUser()
            {
                UserName = addEtherealUser.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(addEtherealUser.Password),
                Email = addEtherealUser.Email,
                FullName = addEtherealUser.FullName,
                Role = addEtherealUser.Role,
                Department = addEtherealUser.Department,
                Phone = addEtherealUser.Phone,
                Position = addEtherealUser.Position,
                CreatedAt = DateTime.UtcNow,
            };

            await _appDbContext.ethereal_user.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();

            return new Dtos.EtherealUserDto
            {
                UserId = entity.UserId,
                UserName = entity.UserName,
                Email = entity.Email,
                FullName = entity.FullName,
                Role = entity.Role,
                Department = entity.Department,
                Phone = entity.Phone,
                Position = entity.Position,
            };
        }

        public async Task<Dtos.EtherealUserDto> UpdateEtherealUser(int id,
            Dtos.UpdateEtherealUserDto updateEtherealUser)
        {
            Entities.EtherealUser? modelData = await _appDbContext.ethereal_user.FindAsync(id);

            if (modelData == null)
                throw new KeyNotFoundException("User is not found");

            modelData.UserName = updateEtherealUser.UserName;
            if (updateEtherealUser.Email != null) modelData.Email = updateEtherealUser.Email;
            if (updateEtherealUser.FullName != null) modelData.FullName = updateEtherealUser.FullName;
            modelData.Role = updateEtherealUser.Role;
            modelData.Department = updateEtherealUser.Department;
            modelData.Phone = updateEtherealUser.Phone;
            modelData.Position = updateEtherealUser.Position;
            modelData.IsActive = updateEtherealUser.IsActive;

            await _appDbContext.SaveChangesAsync();

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
                throw new KeyNotFoundException("User is not found");

            if (string.IsNullOrWhiteSpace(changePassword.CurrentPassword) ||
                string.IsNullOrWhiteSpace(changePassword.NewPassword))
            {
                return new Response.ApiErrorResponse<string>("Password cannot be empty.");
            }

            if (!BCrypt.Net.BCrypt.Verify(changePassword.CurrentPassword, modelData.PasswordHash))
            {
                return new Response.ApiErrorResponse<string>("Current password is incorrect.");
            }

            modelData.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePassword.NewPassword);
            await _appDbContext.SaveChangesAsync();

            return new Response.ApiSuccessResponse<string>("Password changed successfully");
        }
    }

    public class EtherealRecordApi(AppDbContext appDbContext) : IEtherealRecordService
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        private async Task<Dtos.EtherealRecordDto> ResponseRecord(int recordId)
        {
            Entities.EtherealRecord? record = await _appDbContext.ethereal_record
                .AsNoTracking()
                .Include(r => r.Status)
                .Include(r => r.Assignee)
                .Include(r => r.Creator)
                .FirstOrDefaultAsync(r => r.RecordId == recordId);

            if (record == null)
                throw new KeyNotFoundException("Record not found");

            return new Dtos.EtherealRecordDto()
            {
                RecordId = record.RecordId,
                SubNo = record.SubNo,
                Title = record.Title,
                Description = record.Description,
                Status = record.Status == null
                    ? null
                    : new Dtos.EtherealStatusDto()
                    {
                        StatusId = record.Status.StatusId,
                        Name = record.Status.Name
                    },
                Priority = record.Priority,
                AssignedUser = record.Assignee == null
                    ? null
                    : new Dtos.EtherealUserDto()
                    {
                        UserId = record.Assignee.UserId,
                        UserName = record.Assignee.UserName,
                        Email = record.Assignee.Email,
                        FullName = record.Assignee.FullName,
                        Role = record.Assignee.Role,
                        Department = record.Assignee.Department,
                        Position = record.Assignee.Position,
                        IsActive = record.Assignee.IsActive,
                    },
                CreatedUser = record.Creator == null
                    ? null
                    : new Dtos.EtherealUserDto()
                    {
                        UserId = record.Creator.UserId,
                        UserName = record.Creator.UserName,
                        Email = record.Creator.Email,
                        FullName = record.Creator.FullName,
                        Role = record.Creator.Role,
                        Department = record.Creator.Department,
                        Position = record.Creator.Position,
                        IsActive = record.Creator.IsActive,
                    },
                StartDate = record.StartDate,
                DueDate = record.DueDate,
                CompletedAt = record.CompletedAt,
                UpdatedAt = record.UpdatedAt,
                OrderSortNumber = record.OrderSortNumber,
            };
        }

        public async Task<List<Dtos.EtherealRecordDto>> GetEtherealRecords()
        {
            List<Dtos.EtherealRecordDto> query = new List<Dtos.EtherealRecordDto>();

            // Include 导航属性并映射数据
            List<Entities.EtherealRecord> records = await _appDbContext.ethereal_record
                .Include(r => r.Status)
                .Include(r => r.Assignee)
                .Include(r => r.Creator)
                .ToListAsync();

            foreach (Entities.EtherealRecord record in records)
            {
                query.Add(new Dtos.EtherealRecordDto()
                {
                    RecordId = record.RecordId,
                    SubNo = record.SubNo,
                    Title = record.Title,
                    Description = record.Description,

                    Status = record.Status == null
                        ? null
                        : new Dtos.EtherealStatusDto()
                        {
                            StatusId = record.Status.StatusId,
                            Name = record.Status.Name
                        },

                    Priority = record.Priority,

                    AssignedUser = record.Assignee == null
                        ? null
                        : new Dtos.EtherealUserDto()
                        {
                            UserId = record.Assignee.UserId,
                            UserName = record.Assignee.UserName,
                            Email = record.Assignee.Email,
                            FullName = record.Assignee.FullName,
                            Role = record.Assignee.Role,
                            Department = record.Assignee.Department,
                            Position = record.Assignee.Position,
                            IsActive = record.Assignee.IsActive,
                        },

                    CreatedUser = record.Creator == null
                        ? null
                        : new Dtos.EtherealUserDto()
                        {
                            UserId = record.Creator.UserId,
                            UserName = record.Creator.UserName,
                            Email = record.Creator.Email,
                            FullName = record.Creator.FullName,
                            Role = record.Creator.Role,
                            Department = record.Creator.Department,
                            Position = record.Creator.Position,
                            IsActive = record.Creator.IsActive,
                        },

                    StartDate = record.StartDate,
                    DueDate = record.DueDate,
                    CompletedAt = record.CompletedAt,
                    UpdatedAt = record.UpdatedAt,
                    OrderSortNumber = record.OrderSortNumber,
                });
            }

            return query;
        }

        public async Task<Dtos.EtherealRecordDto> GetEtherealRecord(int id)
        {
            return await ResponseRecord(id);
        }

        public async Task<Dtos.EtherealRecordDto> CreateEtherealRecord(Dtos.CreateEtherealRecordDto addEtherealRecord)
        {
            if (addEtherealRecord == null)
                throw new KeyNotFoundException("Create Record is not found");

            Entities.EtherealRecord etherealRecord = new Entities.EtherealRecord()
            {
                SubNo = addEtherealRecord.SubNo,
                Title = addEtherealRecord.Title,
                Description = addEtherealRecord.Description,
                StatusId = addEtherealRecord.StatusId,
                Priority = addEtherealRecord.Priority!,
                AssigneeUserId = addEtherealRecord.AssigneeUserId,
                StartDate = addEtherealRecord.StartDate,
                DueDate = addEtherealRecord.DueDate,
            };

            await _appDbContext.ethereal_record.AddAsync(etherealRecord);
            await _appDbContext.SaveChangesAsync();

            return await ResponseRecord(etherealRecord.RecordId);
        }

        public async Task<Dtos.EtherealRecordDto> UpdateEtherealRecord(int id,
            Dtos.UpdateEtherealRecordDto updateEtherealRecord)
        {
            Entities.EtherealRecord? record = await _appDbContext.ethereal_record
                .FirstOrDefaultAsync(r => r.RecordId == id);

            if (record == null)
                throw new KeyNotFoundException("Update Record is not found");

            record.SubNo = updateEtherealRecord.SubNo;
            record.Title = updateEtherealRecord.Title;
            record.Description = updateEtherealRecord.Description;
            record.StatusId = updateEtherealRecord.StatusId;
            record.Priority = updateEtherealRecord.Priority!;
            record.AssigneeUserId = updateEtherealRecord.AssigneeUserId;
            record.StartDate = updateEtherealRecord.StartDate;
            record.DueDate = updateEtherealRecord.DueDate;
            record.UpdatedAt = updateEtherealRecord.UpdatedAt;
            record.OrderSortNumber = updateEtherealRecord.OrderSortNumber;

            await _appDbContext.SaveChangesAsync();

            return await ResponseRecord(record.RecordId);
        }

        public async Task<Dtos.EtherealRecordDto> UpdateCompletedRecord(int id, Dtos.UpdateCompletedRecordDto dto)
        {
            Entities.EtherealRecord? record = await _appDbContext.ethereal_record
                .FirstOrDefaultAsync(r => r.RecordId == id);

            if (record == null)
                throw new KeyNotFoundException("Update Record is not found");

            record.UpdatedAt = dto.UpdatedAt;
            record.StatusId = dto.StatusId;
            record.CompletedAt = dto.CompletedAt;

            await _appDbContext.SaveChangesAsync();

            return await ResponseRecord(record.RecordId);
        }

        public async Task<Dtos.EtherealRecordDto> MoveEtherealRecord(int id,
            Dtos.MoveEtherealRecordDto moveEtherealRecordDto)
        {
            Entities.EtherealRecord? record = await _appDbContext.ethereal_record
                .FirstOrDefaultAsync(r => r.RecordId == id);

            if (record == null)
                throw new KeyNotFoundException("Update Record is not found");

            record.OrderSortNumber = moveEtherealRecordDto.OrderSortNumber;
            record.UpdatedAt = moveEtherealRecordDto.UpdatedAt;
            record.StartDate = moveEtherealRecordDto.StartDate;

            await _appDbContext.SaveChangesAsync();

            return await ResponseRecord(record.RecordId);
        }
    }

    public class EtherealAttachmentApi(AppDbContext appDbContext) : IEtherealAttachmentService
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        private const string UploadFolderUrl = @"J:\Ethereal\Files";

        private const long MaxFileSize = 10 * 1024 * 1024;

        private IEtherealAttachmentService _etherealAttachmentServiceImplementation;

        // 清理非法字符
        private string GetSafePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return "Default";

            foreach (char c in Path.GetInvalidPathChars())
            {
                relativePath = relativePath.Replace(c, '_');
            }

            return relativePath.Replace("..", "_");
        }

        private async Task<Dtos.EtherealAttachmentDto> ResponseAttachment(int id)
        {
            Entities.EtherealAttachment? attachment = await _appDbContext.ethereal_attachment
                .AsNoTracking()
                .Include(u => u.User)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (attachment == null)
                throw new KeyNotFoundException("Attachment is not found");

            return new Dtos.EtherealAttachmentDto()
            {
                Id = attachment.Id,
                RecordId = attachment.RecordId,
                UserId = attachment.UserId,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                FileVersion = attachment.FileVersion,
                FileStatus = attachment.FileStatus,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt,
                User = attachment.User == null
                    ? null
                    : new Dtos.EtherealUserDto()
                    {
                        UserId = attachment.User.UserId,
                        UserName = attachment.User.UserName,
                        Email = attachment.User.Email,
                        FullName = attachment.User.FullName,
                        Role = attachment.User.Role,
                        Department = attachment.User.Department,
                        Position = attachment.User.Position,
                        IsActive = attachment.User.IsActive,
                    }
            };
        }

        public async Task<Dtos.EtherealAttachmentDto> GetAttachmentById(int id)
        {
            return await ResponseAttachment(id);
        }

        public async Task<List<Dtos.EtherealAttachmentDto>> GetAttachmentsByUserId(int userId)
        {
            List<Entities.EtherealAttachment> attachments = await _appDbContext.ethereal_attachment
                .AsNoTracking()
                .Include(u => u.User)
                .ToListAsync();

            List<Dtos.EtherealAttachmentDto> query = new List<Dtos.EtherealAttachmentDto>();

            foreach (Entities.EtherealAttachment item in attachments)
            {
                if (item.UserId == userId)
                {
                    query.Add(new Dtos.EtherealAttachmentDto()
                    {
                        Id = item.Id,
                        RecordId = item.RecordId,
                        UserId = item.UserId,
                        FileName = item.FileName,
                        FileSize = item.FileSize,
                        FileVersion = item.FileVersion,
                        FileStatus = item.FileStatus,
                        ContentType = item.ContentType,
                        UploadedAt = item.UploadedAt,
                        User = item.User == null
                            ? null
                            : new Dtos.EtherealUserDto()
                            {
                                UserName = item.User.UserName,
                                Email = item.User.Email,
                                FullName = item.User.FullName,
                                Role = item.User.Role,
                            }
                    });
                }
            }

            return query;
        }

        public Task<List<Dtos.EtherealAttachmentDto>> GetAttachmentsByRecordId(int recordId)
        {
            return _etherealAttachmentServiceImplementation.GetAttachmentsByRecordId(recordId);
        }

        public Task<Dtos.EtherealAttachmentDto> CreateAttachment(Dtos.CreateAttachmentDto addAttachmentDto)
        {
            return _etherealAttachmentServiceImplementation.CreateAttachment(addAttachmentDto);
        }

        public Task<Response.ApiResponse<string>> DeleteAttachment(int id)
        {
            return _etherealAttachmentServiceImplementation.DeleteAttachment(id);
        }
    }
}