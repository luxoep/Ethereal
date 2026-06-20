using Ethereal_api.Dto;
using Ethereal_api.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ethereal_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly IEtherealRecordService _etherealRecordService;

    public TaskController(IEtherealRecordService etherealRecordService)
    {
        _etherealRecordService = etherealRecordService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetEtherealRecords()
    {
        return Ok(await _etherealRecordService.GetEtherealRecords());
    }

    [HttpGet("record/{id}")]
    public async Task<IActionResult> GetEtherealRecordById(int id)
    {
        return Ok(await _etherealRecordService.GetEtherealRecord(id));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateEtherealRecord([FromBody] Dtos.CreateEtherealRecordDto newRecord)
    {
        return Ok(await _etherealRecordService.CreateEtherealRecord(newRecord));
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateRecord(int id, [FromBody] Dtos.UpdateEtherealRecordDto updateRecord)
    {
        return Ok(await _etherealRecordService.UpdateEtherealRecord(id, updateRecord));
    }

    [HttpPut("updataComplateDate/{id}")]
    public async Task<IActionResult> UpdateComplateDate(int id, [FromBody] Dtos.UpdateCompletedRecordDto updateRecord)
    {
        return Ok(await _etherealRecordService.UpdateCompletedRecord(id, updateRecord));
    }
}