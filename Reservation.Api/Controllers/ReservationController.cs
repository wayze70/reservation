using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.JWT;
using Reservation.Api.Services;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    
    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
    
    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create([FromBody] ReservationCreateRequest request, 
    [FromHeader(Name = 
        "Authorization")] string authorization)
    {
        return Ok(await _reservationService.CreateAsync(request, Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }
 
    [HttpGet("{ownerId:int}")]
    public async Task<ActionResult<List<ReservationResponse>>> Get(int ownerId)
    {
        return Ok(await _reservationService.GetAsync(ownerId));
    }

    [AllowAnonymous]
    [HttpPut("signup/{reservationId:int}")]
    public async Task<ActionResult<ReservationSignUpResponse>> SignUp(int reservationId, ReservationSignUpRequest request)
    {
        return Ok (await _reservationService.SignUpAsync(reservationId, request));
    }
}