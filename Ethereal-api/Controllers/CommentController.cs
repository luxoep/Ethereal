using System.Security.Claims;
using Ethereal_api.Dto;
using Ethereal_api.IService;
using Microsoft.AspNetCore.Mvc;

namespace Ethereal_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly IEtherealCommentService _commentService;

    public CommentController(IEtherealCommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetComment(int id)
    {
        return Ok(await _commentService.GetCommentById(id));
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetCommentUserId(int id)
    {
        return Ok(await _commentService.GetCommentById(id));
    }

    [HttpGet("record/{recordId}")]
    public async Task<IActionResult> GetCommentByRecordId(int recordId)
    {
        return Ok(await _commentService.GetCommentById(recordId));
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateComment([FromBody] Dtos.CreateEtherealCommentDto addNewComment)
    {
        int userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        return Ok(await _commentService.CreateEtherealComment(addNewComment, userIdClaim));
    }

    [HttpPost("Update/{id}")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] Dtos.UpdateEtherealCommentDto updateComment)
    {
        return Ok(_commentService.UpdateEtherealComment(id, updateComment));
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        return Ok(_commentService.DeleteComment(id));
    }
}