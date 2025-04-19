using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.Services;
using Reservation.Shared.Dtos;
using Reservation.Shared.Authorization;

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
        return Ok(await _reservationService.CreateReservationsAsync(request, HttpContext.GetAccountIdFromBearer()));
    }

    // 2. Získání detailu rezervace (včetně uživatelů) pro vlastníka
    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<ReservationResponseWithUser>> GetReservationDetailForOwner(
        [FromRoute] int reservationId)
    {
        var result = await _reservationService.GetReservationWithUsersAsync(HttpContext.GetAccountIdFromBearer(), reservationId);
        return Ok(result);
    }

    // 3. Získání rezervací pro vlastníka
    // Abychom odlišili tento endpoint od veřejných, přidáváme do routy prefix "owner"
    [HttpGet("owner")]
    public async Task<ActionResult<List<ReservationResponse>>> GetReservationsForOwner()
    {
        var result = await _reservationService.GetReservationsByAccountAsync(HttpContext.GetAccountIdFromBearer());
        return Ok(result);
    }

    // 4. Aktualizace rezervace
    [HttpPut("{reservationId:int}")]
    [Authorize(Roles = $"{nameof(Role.Admin)}, {nameof(Role.Reservationist)}")]
    public async Task<ActionResult<ReservationResponse>> UpdateReservation(
        [FromBody] ReservationCreateRequest request, [FromRoute] int reservationId)
    {
        var result = await _reservationService.UpdateReservationAsync(request, reservationId);
        return Ok(result);
    }

    // 5. Smazání rezervace
    [HttpDelete("{reservationId:int}")]
    [Authorize(Roles = $"{nameof(Role.Admin)}, {nameof(Role.Reservationist)}")]
    public async Task<ActionResult<bool>> DeleteReservation([FromRoute] int reservationId)
    {
        bool result = await _reservationService.DeleteReservationAsync(HttpContext.GetAccountIdFromBearer(), reservationId);
        return Ok(result);
    }

    [HttpPost("remove-user")]
    [Authorize(Roles = $"{nameof(Role.Admin)}, {nameof(Role.Reservationist)}")]
    public async Task<ActionResult<bool>> RemoveUserFromReservation(
        [FromBody] RemoveUserFromReservationRequest request)
    {
        bool isOwner =
            await _reservationService.AccountOwnsReservationAsync(HttpContext.GetAccountIdFromBearer(), request.ReservationId);
        if (!isOwner)
        {
            throw new CustomHttpException(HttpStatusCode.Forbidden, "Nejste vlastníkem rezervace");
        }

        bool result =
            await _reservationService.RemoveUserFromReservationAsync(request.ReservationId, request.UserEmail);
        return Ok(result);
    }

    // 7. Získání rezervací podle cesty (public)
    // Přidáváme prefix "public", aby nedošlo ke kolizi s endpointy pro vlastníka
    [AllowAnonymous]
    [HttpGet("public/{path}")]
    public async Task<ActionResult<List<ReservationResponse>>> GetReservationsByPath([FromRoute] string path)
    {
        var result = await _reservationService.GetReservationsByPathAsync(path);
        return Ok(result);
    }

    // 8. Získání rezervace podle cesty a ID (public)
    [AllowAnonymous]
    [HttpGet("public/{path}/{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetReservationByPathAndId([FromRoute] string path,
        [FromRoute] int id)
    {
        var result = await _reservationService.GetReservationByPathAndIdAsync(path, id);
        return Ok(result);
    }

    // 9. Přihlášení se na rezervaci (public)
    [AllowAnonymous]
    [HttpPost("signup/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> SignUpForReservation(
        [FromRoute] int reservationId, [FromBody] ReservationSignUpRequest request)
    {
        var result = await _reservationService.SignUpForReservationAsync(reservationId, request,
            HttpContext.GetUserPreferredCurrentCulture());
        return Ok(result);
    }

    // 10. Zrušení rezervace (public)
    [AllowAnonymous]
    [HttpPost("cancel/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> CancelReservation(
        [FromRoute] int reservationId, [FromBody] string cancellationCode)
    {
        var result = await _reservationService.CancelReservationAsync(reservationId, cancellationCode,
            HttpContext.GetUserPreferredCurrentCulture());
        return Ok(result);
    }
}