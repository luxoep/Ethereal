using System.Security.Claims;
using Ethereal_api.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

        public async Task<List<Dtos.EtherealAttachmentDto>> GetAttachmentsByRecordId(int recordId)
        {
            List<Entities.EtherealAttachment> attachments = await _appDbContext.ethereal_attachment
                .AsNoTracking()
                .Include(u => u.User)
                .ToListAsync();

            List<Dtos.EtherealAttachmentDto> query = new List<Dtos.EtherealAttachmentDto>();

            foreach (Entities.EtherealAttachment item in attachments)
            {
                if (item.RecordId == recordId)
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

        public async Task<Dtos.EtherealAttachmentDto> CreateAttachment(Dtos.CreateAttachmentDto addAttachmentDto)
        {
            if (addAttachmentDto.File == null)
                throw new KeyNotFoundException("Invalid File");

            if (addAttachmentDto.File.Length == 0)
                throw new Exception("File is empty");

            if (addAttachmentDto.File.Length > MaxFileSize)
                return new Dtos.EtherealAttachmentDto()
                {
                    FileName =
                        $"{addAttachmentDto.File.FileName} Size Exceeded {addAttachmentDto.File.Length / 1024.0 / 1024.0:F2}MB",
                };
            // 根据RecordId创建文件夹
            string safeOrder = GetSafePath(addAttachmentDto.RecordId.ToString());
            // 拼接完整文件夹地址
            string folderPath = Path.Combine(UploadFolderUrl, safeOrder);
            // 自动判断文件夹是否存在
            Directory.CreateDirectory(folderPath);
            // 获取上传文件名
            string originalFileName = Path.GetFileName(addAttachmentDto.File.FileName);
            // 获取上传不带扩展名的文件名
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            // 获取上传文件带扩展名的文件名
            string extension = Path.GetExtension(originalFileName);
            // 查询已有附件
            List<Entities.EtherealAttachment> attachments = await _appDbContext.ethereal_attachment.ToListAsync();

            int version = 1;

            foreach (Entities.EtherealAttachment item in attachments)
            {
                if (item.RecordId == addAttachmentDto.RecordId && item.FileName == originalFileName)
                {
                    if (item.FileVersion >= version)
                    {
                        version = item.FileVersion + 1;
                    }

                    item.FileStatus = false;
                }
            }

            await _appDbContext.SaveChangesAsync();

            // 实际存储文件名
            string versionFileName = $"{fileNameWithoutExtension}_{version}{extension}";
            // 检查文件名合法性
            versionFileName = GetSafePath(versionFileName);
            // 拼接完整文件地址
            string filePath = Path.Combine(folderPath, versionFileName);

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await addAttachmentDto.File.CopyToAsync(fs);
            }

            // 写入数据库记录
            Entities.EtherealAttachment attachment = new Entities.EtherealAttachment()
            {
                RecordId = addAttachmentDto.RecordId,
                UserId = addAttachmentDto.UserId,
                FileName = originalFileName,
                FilePath = filePath,
                FileSize = addAttachmentDto.File.Length,
                FileVersion = version,
                FileStatus = true,
                ContentType = addAttachmentDto.File.ContentType,
                UploadedAt = DateTime.UtcNow
            };

            await _appDbContext.ethereal_attachment.AddAsync(attachment);
            await _appDbContext.SaveChangesAsync();

            return await ResponseAttachment(attachment.Id);
        }

        public async Task<List<Dtos.UploadAttachmentResultDto>> CreateAttachmentsDto(
            Dtos.CreateAttachmentsDto addAttachmentsDto)
        {
            if (addAttachmentsDto.Files == null || addAttachmentsDto.Files.Count == 0)
                throw new KeyNotFoundException("Invalid Files");

            List<Dtos.UploadAttachmentResultDto> resFile = new List<Dtos.UploadAttachmentResultDto>();
            // 记录成功写入磁盘的文件路径
            List<string> tempSavedFiles = new List<string>();
            // 根据RecordId创建文件夹
            string safeOrder = GetSafePath(addAttachmentsDto.RecordId.ToString());
            // 拼接完整文件夹地址
            string folderPath = Path.Combine(UploadFolderUrl, safeOrder);
            // 自动判断文件夹是否存在
            Directory.CreateDirectory(folderPath);

            // 开启数据库事务
            await using IDbContextTransaction transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (IFormFile item in addAttachmentsDto.Files)
                {
                    Dtos.UploadAttachmentResultDto result = new Dtos.UploadAttachmentResultDto()
                    {
                        FileName = item.FileName,
                    };

                    if (item.Length == 0)
                    {
                        result.Success = false;
                        result.Message = "File is empty";
                        resFile.Add(result);
                        continue;
                    }

                    if (item.Length > MaxFileSize)
                    {
                        result.Success = false;
                        result.Message = $"File exceeds {MaxFileSize / 1024 / 1024}MB limit";

                        resFile.Add(result);
                        continue;
                    }

                    // 获取上传文件名
                    string originalFileName = Path.GetFileName(item.FileName);
                    // 获取文件完整路径
                    string filePath = Path.Combine(folderPath, originalFileName);

                    // 重复检查
                    bool exists = await _appDbContext.ethereal_attachment
                        .AnyAsync(f =>
                            f.RecordId == addAttachmentsDto.RecordId &&
                            f.FileName == originalFileName);

                    if (exists)
                    {
                        result.Success = false;
                        result.Message = $"File {item.FileName} is Duplicate file";
                        resFile.Add(result);
                        continue;
                    }

                    // 写入磁盘
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await item.CopyToAsync(fs);
                    }

                    // 追踪以写文件
                    tempSavedFiles.Add(filePath);

                    // 写入数据库记录
                    Entities.EtherealAttachment newFile = new Entities.EtherealAttachment()
                    {
                        RecordId = addAttachmentsDto.RecordId,
                        UserId = addAttachmentsDto.UserId,
                        FileName = item.FileName,
                        FilePath = filePath,
                        FileSize = item.Length,
                        FileVersion = 1,
                        FileStatus = true,
                        ContentType = item.ContentType,
                        UploadedAt = DateTime.UtcNow
                    };
                    await _appDbContext.ethereal_attachment.AddAsync(newFile);

                    result.Success = true;
                    result.Message = "Upload Success";

                    result.Attachment = new Dtos.EtherealAttachmentDto()
                    {
                        RecordId = newFile.RecordId,
                        UserId = newFile.UserId,
                        FileName = newFile.FileName,
                        FileSize = newFile.FileSize,
                        FileVersion = newFile.FileVersion,
                        FileStatus = newFile.FileStatus,
                        ContentType = newFile.ContentType,
                        UploadedAt = newFile.UploadedAt
                    };
                    resFile.Add(result);
                }

                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return resFile;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                foreach (string path in tempSavedFiles)
                {
                    if (File.Exists(path)) File.Delete(path);
                }

                throw;
            }
        }

        public async Task<FileStreamResult> DownloadAttachment(int id)
        {
            Entities.EtherealAttachment? attachment =
                await _appDbContext.ethereal_attachment.FirstOrDefaultAsync(f => f.Id == id);

            if (attachment == null)
                throw new KeyNotFoundException("Attachment not found");

            if (!File.Exists(attachment.FilePath))
                throw new FileNotFoundException("Physical file missing");

            FileStream stream = new FileStream(attachment.FilePath, FileMode.Open, FileAccess.Read);

            return new FileStreamResult(stream, attachment.ContentType ?? "application/octet-stream")
            {
                FileDownloadName = attachment.FileName,
            };
        }

        public async Task<Response.ApiResponse<string>> DeleteAttachment(int id)
        {
            Entities.EtherealAttachment? attachment =
                await _appDbContext.ethereal_attachment.FirstOrDefaultAsync(f => f.Id == id);

            if (attachment == null)
                throw new KeyNotFoundException("Attachment not found");

            // 开启数据库事务
            await using IDbContextTransaction transaction =
                await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                if (!string.IsNullOrEmpty(attachment.FilePath) && File.Exists(attachment.FilePath))
                    File.Delete(attachment.FilePath);

                _appDbContext.ethereal_attachment.Remove(attachment);

                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new Response.ApiSuccessResponse<string>("Deleted Attachment Access");
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                return new Response.ApiErrorResponse<string>($"Delete failed: {e.Message}");
            }
        }
    }

    public class EtherealCommentApi(AppDbContext appDbContext) : IEtherealCommentService
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        private async Task<(Entities.EtherealComment, Entities.EtherealUser)> ResponseComment(int id)
        {
            Entities.EtherealComment? comment = await _appDbContext.ethereal_comment
                .AsNoTracking()
                .Include(u => u.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (comment == null)
                throw new KeyNotFoundException("Comment does not exist");

            if (comment.User == null)
                throw new KeyNotFoundException("User not found");

            return (comment, comment.User);
        }

        private Dtos.EtherealUserDto MapUserDto(Entities.EtherealUser? user)
        {
            return new Dtos.EtherealUserDto()
            {
                UserId = user!.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Department = user.Department,
                Position = user.Position,
                IsActive = user.IsActive,
            };
        }

        public async Task<Dtos.EtherealCommentDto> GetCommentById(int id)
        {
            (Entities.EtherealComment, Entities.EtherealUser) commentTask = await ResponseComment(id);

            Entities.EtherealComment? comment = commentTask.Item1;

            return new Dtos.EtherealCommentDto()
            {
                Id = comment.Id,
                RecordId = comment.RecordId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                User = MapUserDto(comment.User),
            };
        }

        public async Task<List<Dtos.EtherealCommentDto>> GetCommentsByUserId(int userId)
        {
            List<Dtos.EtherealCommentDto> query = new List<Dtos.EtherealCommentDto>();

            List<Entities.EtherealComment> comments = await _appDbContext.ethereal_comment
                .Include(r => r.Record)
                .Include(u => u.User)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            foreach (Entities.EtherealComment comment in comments)
            {
                query.Add(new Dtos.EtherealCommentDto()
                {
                    Id = comment.Id,
                    RecordId = comment.RecordId,
                    UserId = comment.UserId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    User = MapUserDto(comment.User)
                });
            }

            return query;
        }

        public async Task<List<Dtos.EtherealCommentDto>> GetCommentsByRecordId(int recordId)
        {
            List<Dtos.EtherealCommentDto> query = new List<Dtos.EtherealCommentDto>();

            List<Entities.EtherealComment> comments = await _appDbContext.ethereal_comment
                .Include(r => r.Record)
                .Include(u => u.User)
                .Where(f => f.RecordId == recordId)
                .ToListAsync();

            foreach (Entities.EtherealComment comment in comments)
            {
                query.Add(new Dtos.EtherealCommentDto()
                {
                    Id = comment.Id,
                    RecordId = comment.RecordId,
                    UserId = comment.UserId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    User = MapUserDto(comment.User)
                });
            }

            return query;
        }

        public async Task<Dtos.EtherealCommentDto> CreateEtherealComment(Dtos.CreateEtherealCommentDto addCommentDto,
            int userIdClaim)
        {
            if (addCommentDto == null)
                throw new ArgumentNullException(nameof(addCommentDto));

            Entities.EtherealComment newComment = new Entities.EtherealComment()
            {
                RecordId = addCommentDto.RecordId,
                UserId = userIdClaim,
                Content = addCommentDto.Content,
                CreatedAt = DateTime.UtcNow,
            };

            await _appDbContext.ethereal_comment.AddAsync(newComment);
            await _appDbContext.SaveChangesAsync();

            (Entities.EtherealComment, Entities.EtherealUser) commentTask = await ResponseComment(newComment.Id);

            Entities.EtherealComment? comment = commentTask.Item1;

            return new Dtos.EtherealCommentDto()
            {
                Id = comment.Id,
                RecordId = comment.RecordId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                User = MapUserDto(comment.User)
            };
        }

        public async Task<Dtos.EtherealCommentDto> UpdateEtherealComment(int id,
            Dtos.UpdateEtherealCommentDto updateCommentDto)
        {
            if (updateCommentDto == null)
                throw new ArgumentNullException(nameof(updateCommentDto));

            (Entities.EtherealComment, Entities.EtherealUser) commentTask = await ResponseComment(id);

            Entities.EtherealComment? comment = commentTask.Item1;

            comment.Content = updateCommentDto.Content;

            await _appDbContext.SaveChangesAsync();

            return await GetCommentById(comment.Id);
        }

        public async Task<Response.ApiResponse<string>> DeleteComment(int id)
        {
            (Entities.EtherealComment, Entities.EtherealUser) commentTask = await ResponseComment(id);

            Entities.EtherealComment? comment = commentTask.Item1;

            _appDbContext.ethereal_comment.Remove(comment);

            await _appDbContext.SaveChangesAsync();

            return new Response.ApiSuccessResponse<string>("Operation successful");
        }
    }
}