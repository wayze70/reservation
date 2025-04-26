using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.Services;
using Reservation.Shared.Authorization;
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
    [Authorize(Roles = $"{nameof(Role.Admin)}, {nameof(Role.Reservationist)}")]
    public async Task<ActionResult<List<ReservationResponse>>> CreateReservations(
        [FromBody] List<ReservationCreateRequest> request)
    {
        return Ok(await _reservationService.CreateReservationsAsync(
            request, HttpContext.GetAccountIdFromBearer()));
    }

    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<ReservationResponseWithCustomers>> GetReservationDetailForOwner(
        [FromRoute] int reservationId)
    {
        return Ok(await _reservationService.GetReservationWithUsersAsync(HttpContext.GetAccountIdFromBearer(),
            reservationId));
    }

    [HttpGet("owner")]
    public async Task<ActionResult<List<ReservationResponse>>> GetReservationsForOwner()
    {
        return Ok(await _reservationService.GetReservationsByAccountAsync(HttpContext.GetAccountIdFromBearer()));
    }

    [HttpPut("{reservationId:int}")]
    [Authorize(Roles = $"{nameof(Role.Admin)}, {nameof(Role.Reservationist)}")]
    public async Task<ActionResult<ReservationResponse>> UpdateReservation(
        [FromBody] ReservationCreateRequest request, [FromRoute] int reservationId)
    {
        return Ok(await _reservationService.UpdateReservationAsync(request, reservationId));
    }

    [HttpDelete("{reservationId:int}")]
    [Authorize(Roles = $"{nameof(Role.Admin)}, {nameof(Role.Reservationist)}")]
    public async Task<ActionResult<bool>> DeleteReservation([FromRoute] int reservationId)
    {
        return Ok(await _reservationService.DeleteReservationAsync(HttpContext.GetAccountIdFromBearer(),
            reservationId));
    }

    [HttpPost("remove-user")]
    [Authorize(Roles = $"{nameof(Role.Admin)}, {nameof(Role.Reservationist)}")]
    public async Task<ActionResult<bool>> RemoveUserFromReservation(
        [FromBody] RemoveCustomerFromReservationRequest request)
    {
        if (!await _reservationService.AccountOwnsReservationAsync(HttpContext.GetAccountIdFromBearer(),
                request.ReservationId))
        {
            throw new CustomHttpException(HttpStatusCode.Forbidden, "Nejste vlastníkem události");
        }

        return Ok(await _reservationService.RemoveUserFromReservationAsync(request.ReservationId, request.UserEmail,
            HttpContext.GetUserPreferredCurrentCulture()));
    }

    [AllowAnonymous]
    [HttpGet("public/{path}")]
    public async Task<ActionResult<List<ReservationResponse>>> GetActiveReservationsByPath([FromRoute] string path)
    {
        return Ok(await _reservationService.GetActiveReservationsByPathAsync(path));
    }

    [AllowAnonymous]
    [HttpGet("public/{path}/{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetReservationByPathAndId([FromRoute] string path,
        [FromRoute] int id)
    {
        return Ok(await _reservationService.GetReservationByPathAndIdAsync(path, id));
    }

    [AllowAnonymous]
    [HttpPost("signup/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> SignUpForReservation(
        [FromRoute] int reservationId, [FromBody] ReservationSignUpRequest request)
    {
        return Ok(await _reservationService.SignUpForReservationAsync(reservationId, request,
            HttpContext.GetUserPreferredCurrentCulture()));
    }

    [AllowAnonymous]
    [HttpPost("cancel/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> CancelReservation(
        [FromRoute] int reservationId, [FromBody] string cancellationCode)
    {
        return Ok(await _reservationService.CancelReservationAsync(reservationId, cancellationCode,
            HttpContext.GetUserPreferredCurrentCulture()));
    }
}