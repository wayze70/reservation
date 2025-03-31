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
    public async Task<ActionResult<string>> Path([FromHeader(Name = "Authorization")] string
        authorization)
    {
        return Ok((await _accountService.GetPathAsync(Utils.GetUserIdFromAuthorizationHeader(authorization))) ??
                  string.Empty);
    }

    [HttpPost("path")]
    public async Task<ActionResult<string>> Path([FromBody] PathRequest request, [FromHeader(Name = "Authorization")]
        string
            authorization)
    {
        return Ok(await _accountService.SetPathAsync(request, Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }

    [HttpPost("path/taken")]
    public async Task<ActionResult<bool>> Path([FromBody] PathRequest request)
    {
        return Ok(await _accountService.IsPathTakenAsync(request.Path));
    }
}