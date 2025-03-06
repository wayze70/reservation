using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.Dtos;
using Reservation.Api.JWT;
using Reservation.Api.Services;

namespace Reservation.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReservationController
{
    private readonly IReservationService _reservationService;
    
    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
    
    [HttpPost("create")]
    public ActionResult<ReservationResponse> Create([FromBody] ReservationCreateRequest request, 
    [FromHeader(Name = 
        "Authorization")] string authorization)
    {
        string bearerToken = JwtTokenHelper.GetBearerToken(authorization);
        
        if (string.IsNullOrEmpty(bearerToken))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Bearer token is required");
        }
        
        var claims = JwtTokenHelper.GetClaims(bearerToken);
        
        int userId = int.Parse(JwtTokenHelper.GetClaimValue(claims, ReservationClaimNames.Sub));
        
        return _reservationService.Create(request, userId);
    }
 
    [HttpGet("{ownerId:int}")]
    public ActionResult<List<ReservationResponse>> Get(int ownerId)
    {
        return _reservationService.Get(ownerId);
    }

    [AllowAnonymous]
    [HttpPut("signup/{reservationId:int}")]
    public ActionResult<ReservationSignUpResponse> SignUp(int reservationId, ReservationSignUpRequest request)
    {
        return _reservationService.SignUp(reservationId, request);
    }
}