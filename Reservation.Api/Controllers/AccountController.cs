using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.JWT;
using Reservation.Api.Services;
using Reservation.Shared.Authorization;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAuthService _authService;
    
    public AccountController(IAccountService accountService, IAuthService authService)
    {
        _accountService = accountService;
        _authService = authService;
    }
    
    [AllowAnonymous]
    [HttpPost("accounts-by-email")]
    public async Task<ActionResult<List<AccountInfoResponse>>> GetAccountsByEmail([FromBody] AccountsByEmailRequest 
        request)
    {
        return Ok(await _accountService.GetAccountsByEmailAsync(request.Email));
    }

    [HttpGet("path")]
    public async Task<ActionResult<string>> GetPath([FromHeader(Name = "Authorization")] string
        authorization)
    {
        HttpContext.Request.Headers.Authorization = authorization;
        return Ok((await _accountService.GetPathAsync(Utils.GetAccountIdFromBearerToken(authorization))) ??
                  string.Empty);
    }
    
    [HttpPost("path")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<string>> SetPath([FromBody] PathRequest request, [FromHeader(Name = "Authorization")]
        string authorization)
    {
        return Ok(await _accountService.SetPathAsync(request, Utils.GetAccountIdFromBearerToken(authorization)));
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
        return Ok(await _accountService.GetAccountInfoAsync(Utils.GetAccountIdFromBearerToken(authorization)));
    }

    [HttpPut("account-info")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<AccountInfoResponse>> UpdatePath([FromBody] UpdateAccountInfoRequest request,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        return Ok(await _accountService.UpdateAccountInfoAsync(request, Utils.GetAccountIdFromBearerToken
            (authorization)));
    }
    
    [HttpPut("update-password")]
    public async Task<ActionResult<bool>> UpdatePassword([FromBody] UpdatePasswordRequest request,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        return Ok(await _accountService.UpdatePasswordAsync(request, Utils.GetUserIdFromBearerToken(authorization)));
    }
        
    [AllowAnonymous]
    [HttpGet("{path}")]
    public async Task<ActionResult<AccountDescriptionResponse>> GetAccountDescription([FromRoute] string path)
    {
        return Ok(await _accountService.GetAccountDescriptionAsync(path));
    }

    [HttpPost("delete")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<bool>> DeleteAccount([FromHeader(Name = "Authorization")] string authorization, 
    [FromBody] DeleteAccountRequest request)
    {
        return Ok(await _accountService.DeleteAccountAsync(request, Utils.GetAccountIdFromBearerToken(authorization)));
    }
}