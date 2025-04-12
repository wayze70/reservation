using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.JWT;
using Reservation.Api.Services;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("path")]
    public async Task<ActionResult<string>> GetPath([FromHeader(Name = "Authorization")] string
        authorization)
    {
        return Ok((await _accountService.GetPathAsync(Utils.GetUserIdFromAuthorizationHeader(authorization))) ??
                  string.Empty);
    }

    [HttpPost("path")]
    public async Task<ActionResult<string>> SetPath([FromBody] PathRequest request, [FromHeader(Name = "Authorization")]
        string authorization)
    {
        return Ok(await _accountService.SetPathAsync(request, Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }

    [HttpPost("path/taken")]
    public async Task<ActionResult<bool>> IsPathTaken([FromBody] PathRequest request)
    {
        return Ok(await _accountService.IsPathTakenAsync(request.Path));
    }

    [HttpGet("account-info")]
    public async Task<ActionResult<AccountInfoResponse>> GetAccountInfo(
        [FromHeader(Name = "Authorization")] string authorization)
    {
        return Ok(await _accountService.GetAccountInfoAsync(Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }

    [HttpPut("account-info")]
    public async Task<ActionResult<AccountInfoResponse>> UpdatePath([FromBody] UpdateAccountInfoRequest request,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        return Ok(await _accountService.UpdateAccountInfoAsync(request, Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }
    
    [HttpPut("update-password")]
    public async Task<ActionResult<bool>> UpdatePassword([FromBody] UpdatePasswordRequest request,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        return Ok(await _accountService.UpdatePasswordAsync(request, Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }
        
    [AllowAnonymous]
    [HttpGet("{path}")]
    public async Task<ActionResult<AccountDescriptionResponse>> GetAccountDescription([FromRoute] string path)
    {
        return Ok(await _accountService.GetAccountDescriptionAsync(path));
    }
}