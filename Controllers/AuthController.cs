using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ReservationApi.Dtos;
using ReservationApi.Models;
using ReservationApi.Services;
using Microsoft.AspNetCore.Mvc;
using Auth = ReservationApi.Dtos.Auth;
using User = ReservationApi.Dtos.User;

namespace ReservationApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<User> Login([FromBody] Auth.LoginRequest request)
    {
        return _authService.Login(request.Email, request.Password);
    }
    
    [HttpPost("register")]
    public ActionResult<User> Register([FromBody] Auth.RegisterRequest request)
    {
        return _authService.Register(request.FirstName, request.LastName, request.Email, request.Password);
    }
}