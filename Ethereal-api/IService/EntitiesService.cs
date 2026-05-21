using Ethereal_api.Dto;

namespace Ethereal_api.IService;

public interface IEtherealUserService
{
    Task<List<Dtos.EtherealUserDto>> GetEtherealUsers();
    Task<Dtos.EtherealUserDto> GetEtherealUser(int id);
    Task<Dtos.EtherealUserDto> CreateEtherealUser(Dtos.CreateEtherealUserDto addEtherealUser);
    Task<Dtos.EtherealUserDto> UpdateEtherealUser(int id, Dtos.UpdateEtherealUserDto updateEtherealUser);
    Task<Response.ApiResponse<string>> ChangePassword(int id, Dtos.ChangePasswordDto changePasswordDto);
}

public interface IEtherealRecordService
{
    Task<List<Dtos.EtherealRecordDto>> GetEtherealRecords();
    Task<Dtos.EtherealRecordDto> GetEtherealRecord(int id);
    Task<Dtos.EtherealRecordDto> CreateEtherealRecord(Dtos.CreateEtherealRecordDto addEtherealRecord);
    Task<Dtos.EtherealRecordDto> UpdateEtherealRecord(int id, Dtos.UpdateEtherealRecordDto updateEtherealRecord);
    Task<Dtos.EtherealRecordDto> UpdateCompletedRecord(int id, Dtos.UpdateCompletedRecordDto dto);
    Task<Dtos.EtherealRecordDto> MoveEtherealRecord(int id, Dtos.MoveEtherealRecordDto moveEtherealRecordDto);
}

public interface IEtherealAttachmentService
{
    Task<Dtos.EtherealAttachmentDto> GetAttachmentById(int id);
    Task<List<Dtos.EtherealAttachmentDto>> GetAttachmentsByUserId(int userId);
    Task<List<Dtos.EtherealAttachmentDto>> GetAttachmentsByRecordId(int recordId);
    Task<Dtos.EtherealAttachmentDto> CreateAttachment(Dtos.CreateAttachmentDto addAttachmentDto);
    Task<Response.ApiResponse<string>> DeleteAttachment(int id);
}

public interface IEtherealCommentService
{
    Task<Dtos.EtherealCommentDto> GetCommentById(int id);
    Task<List<Dtos.EtherealCommentDto>> GetCommentsByUserId(int userId);
    Task<List<Dtos.EtherealCommentDto>> GetCommentsByRecordId(int recordId);
    Task<Dtos.EtherealCommentDto> CreateEtherealComment(Dtos.CreateEtherealCommentDto addCommentDto);
    Task<Dtos.EtherealCommentDto> UpdateEtherealComment(int id, Dtos.UpdateEtherealCommentDto updateCommentDto);
    Task<Response.ApiResponse<string>> DeleteComment(int id);
}