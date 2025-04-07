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
    public async Task<ActionResult<List<ReservationResponse>>> Create([FromBody] List<ReservationCreateRequest> request, 
        [FromHeader(Name = 
            "Authorization")] string authorization)
    {
        return Ok(await _reservationService.CreateAsync(request, Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }
    
    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<ReservationResponseWithUser>> GetDetailByOwner([FromHeader(Name = "Authorization")] string
        authorization, int reservationId)
    {
        return Ok(await _reservationService.GetWithUserAsync(Utils.GetUserIdFromAuthorizationHeader(authorization), 
            reservationId));
    }
    
    
    [HttpGet]
    public async Task<ActionResult<List<ReservationResponse>>> GetByOwner([FromHeader(Name = "Authorization")] string
        authorization)
    {
        return Ok(await _reservationService.GetAsync(Utils.GetUserIdFromAuthorizationHeader(authorization)));
    }
    
    [HttpDelete("{reservationId:int}")]
    public async Task<ActionResult<bool>> Delete([FromHeader(Name = "Authorization")] string authorization, 
        int reservationId)
    {
        return Ok(await _reservationService.DeleteAsync(Utils.GetUserIdFromAuthorizationHeader(authorization), reservationId));
    }
    
    [HttpPut("{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> Update([FromBody] ReservationCreateRequest request, int reservationId)
    {
        return Ok(await _reservationService.UpdateAsync(request, reservationId));
    }
    
    [HttpPost("remove-user")]
    public async Task<ActionResult<bool>> RemoveUserFromReservation([FromHeader(Name = "Authorization")] string authorization, RemoveUserFromReservationRequest request)
    {
        bool isOwner = await _reservationService.OwnerOwnsReservation(Utils.GetUserIdFromAuthorizationHeader(authorization), request
            .ReservationId);
        
        if (!isOwner)
        {
            throw new CustomHttpException(HttpStatusCode.Forbidden, "Nejste vlastníkem rezervace");
        }
        
        return Ok(await _reservationService.RemoveUserFromReservationAsync(request.ReservationId, request.UserEmail));
    }
 
    [AllowAnonymous]
    [HttpGet("{path}")]
    public async Task<ActionResult<List<ReservationResponse>>> Get(string path)
    {
        return Ok(await _reservationService.GetAsync(path));
    }
    
    [AllowAnonymous]
    [HttpGet("{path}/{id:int}")]
    public async Task<ActionResult<List<ReservationResponse>>> Get(string path, int id)
    {
        return Ok(await _reservationService.GetAsync(path, id));
    }

    [AllowAnonymous]
    [HttpPost("signup/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> SignUp(int reservationId, ReservationSignUpRequest request)
    {
        return Ok(await _reservationService.SignUpAsync(reservationId, request));
    }
    
    [AllowAnonymous]
    [HttpPost("cancel/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> CancelReservation(int reservationId, string cancellationCode)
    {
        return Ok(await _reservationService.CancelReservationAsync(reservationId, cancellationCode));
    }
}