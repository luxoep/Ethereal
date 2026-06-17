using System.Security.Claims;
using Ethereal_api.Dto;
using Ethereal_api.IService;
using Microsoft.AspNetCore.Mvc;

namespace Ethereal_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IEtherealUserService _userService;

    public UsersController(IEtherealUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        return Ok(await _userService.GetEtherealUsers());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> UserId(int id)
    {
        return Ok(await _userService.GetEtherealUser(id));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        int userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        return Ok(await _userService.GetEtherealUser(userIdClaim));
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewUser([FromBody] Dtos.CreateEtherealUserDto createEtherealUserDto)
    {
        if (createEtherealUserDto == null)
            throw new KeyNotFoundException("New user is illegal.");
        return Ok(await _userService.CreateEtherealUser(createEtherealUserDto));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ChangeUser(int id, [FromBody] Dtos.UpdateEtherealUserDto updateEtherealUserDto)
    {
        if (updateEtherealUserDto == null)
            throw new KeyNotFoundException("Invalid User");

        return Ok(await _userService.UpdateEtherealUser(id, updateEtherealUserDto));
    }

    [HttpDelete("ChangePassWord")]
    public async Task<IActionResult> ChangePassWord(Dtos.ChangePasswordDto changePasswordDto)
    {
        int userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (changePasswordDto == null)
            throw new KeyNotFoundException("Invalid password");

        return Ok(await _userService.ChangePassword(userIdClaim, changePasswordDto));
    }
}