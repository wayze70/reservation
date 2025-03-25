using Microsoft.AspNetCore.Http.HttpResults;
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
    
    [HttpPost("path")]
    public async Task<ActionResult> Path(PathRequest request, [FromHeader(Name = "Authorization")] string 
        authorization)
    {
        await _accountService.SetPathAsync(request, Utils.GetUserIdFromAuthorizationHeader(authorization));
        return Ok();
    }
}