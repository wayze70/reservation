using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.Services;
using Reservation.Shared.Authorization;
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

    [AllowAnonymous]
    [HttpPost("accounts-by-email")]
    public async Task<ActionResult<List<AccountInfoResponse>>> GetAccountsByEmail([FromBody] AccountsByEmailRequest
        request)
    {

        return Ok(await _accountService.GetAccountsByEmailAsync(request.Email));
    }

    [HttpGet("path")]
    public async Task<ActionResult<string>> GetPath()
    {
        return Ok((await _accountService.GetPathAsync(HttpContext.GetAccountIdFromBearer())) ??
                  string.Empty);
    }

    [HttpPost("path")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<string>> SetPath([FromBody] PathRequest request)
    {
        return Ok(await _accountService.SetPathAsync(request, HttpContext.GetAccountIdFromBearer()));
    }

    [AllowAnonymous]
    [HttpPost("path/taken")]
    public async Task<ActionResult<bool>> IsPathTaken([FromBody] PathRequest request)
    {
        return Ok(await _accountService.IsPathTakenAsync(request.Path));
    }

    [HttpGet("account-info")]
    public async Task<ActionResult<AccountInfoResponse>> GetAccountInfo()
    {
        return Ok(await _accountService.GetAccountInfoAsync(HttpContext.GetAccountIdFromBearer()));
    }

    [HttpPut("account-info")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<AccountInfoResponse>> UpdatePath([FromBody] UpdateAccountInfoRequest request)
    {
        return Ok(await _accountService.UpdateAccountInfoAsync(request, HttpContext.GetAccountIdFromBearer()));
    }

    [HttpPut("update-password")]
    public async Task<ActionResult<bool>> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        return Ok(await _accountService.UpdatePasswordAsync(request, HttpContext.GetUserIdFromBearer()));
    }

    [AllowAnonymous]
    [HttpGet("{path}")]
    public async Task<ActionResult<AccountDescriptionResponse>> GetAccountDescription([FromRoute] string path)
    {
        return Ok(await _accountService.GetAccountDescriptionAsync(path));
    }

    [HttpPost("delete")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<bool>> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        return Ok(await _accountService.DeleteAccountAsync(request, HttpContext.GetAccountIdFromBearer(), HttpContext.GetUserIdFromBearer()));
    }
}