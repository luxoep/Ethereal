using System.Security.Claims;
using Ethereal_api.Dto;
using Ethereal_api.IService;
using Microsoft.AspNetCore.Mvc;

namespace Ethereal_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentController : ControllerBase
{
    private readonly IEtherealAttachmentService _etherealAttachmentService;

    public AttachmentController(IEtherealAttachmentService etherealAttachmentService)
    {
        _etherealAttachmentService = etherealAttachmentService;
    }

    [HttpGet("attachment/{id}")]
    public async Task<IActionResult> GetAttachment(int id)
    {
        return Ok(await _etherealAttachmentService.GetAttachmentById(id));
    }

    [HttpGet("attachments/{userId}")]
    public async Task<IActionResult> GetAttachmentByUserId(int userId)
    {
        return Ok(await _etherealAttachmentService.GetAttachmentsByUserId(userId));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAttachment(IFormFile file, [FromForm] int recordId)
    {
        if (file.Length == 0)
            return BadRequest("File is empty");

        int userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Dtos.CreateAttachmentDto addAttachmentDto = new Dtos.CreateAttachmentDto()
        {
            RecordId = recordId,
            UserId = userIdClaim,
            File = file
        };

        return Ok(await _etherealAttachmentService.CreateAttachment(addAttachmentDto));
    }

    [HttpPost("upload/batch")]
    public async Task<IActionResult> UploadAttachments(List<IFormFile> files, [FromForm] int recordId)
    {
        if (files.Count == 0)
            return BadRequest("File is empty");

        int userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Dtos.CreateAttachmentsDto addAttachmentsDto = new Dtos.CreateAttachmentsDto()
        {
            RecordId = recordId,
            UserId = userIdClaim,
            Files = files
        };

        return Ok(await _etherealAttachmentService.CreateAttachments(addAttachmentsDto));
    }

    [HttpGet("attachment/{id}/download")]
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        return await _etherealAttachmentService.DownloadAttachment(id);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        return Ok(await _etherealAttachmentService.DeleteAttachment(id));
    }
}