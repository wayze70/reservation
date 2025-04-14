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
    public async Task<ActionResult<List<ReservationResponse>>> CreateReservations(
        [FromBody] List<ReservationCreateRequest> request,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        int ownerId = Utils.GetUserIdFromBearerToken(authorization);
        return Ok(await _reservationService.CreateReservationsAsync(request, ownerId));
    }

    // 2. Získání detailu rezervace (včetně uživatelů) pro vlastníka
    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<ReservationResponseWithUser>> GetReservationDetailForOwner(
        [FromHeader(Name = "Authorization")] string authorization,
        int reservationId)
    {
        int ownerId = Utils.GetUserIdFromBearerToken(authorization);
        var result = await _reservationService.GetReservationWithUsersAsync(ownerId, reservationId);
        return Ok(result);
    }

    // 3. Získání rezervací pro vlastníka
    // Abychom odlišili tento endpoint od veřejných, přidáváme do routy prefix "owner"
    [HttpGet("owner")]
    public async Task<ActionResult<List<ReservationResponse>>> GetReservationsForOwner(
        [FromHeader(Name = "Authorization")] string authorization)
    {
        int ownerId = Utils.GetUserIdFromBearerToken(authorization);
        var result = await _reservationService.GetReservationsByOwnerAsync(ownerId);
        return Ok(result);
    }

    // 4. Aktualizace rezervace
    [HttpPut("{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> UpdateReservation(
        [FromBody] ReservationCreateRequest request, int reservationId)
    {
        var result = await _reservationService.UpdateReservationAsync(request, reservationId);
        return Ok(result);
    }

    // 5. Smazání rezervace
    [HttpDelete("{reservationId:int}")]
    public async Task<ActionResult<bool>> DeleteReservation(
        [FromHeader(Name = "Authorization")] string authorization,
        int reservationId)
    {
        int ownerId = Utils.GetUserIdFromBearerToken(authorization);
        bool result = await _reservationService.DeleteReservationAsync(ownerId, reservationId);
        return Ok(result);
    }

    [HttpPost("remove-user")]
    public async Task<ActionResult<bool>> RemoveUserFromReservation(
        [FromHeader(Name = "Authorization")] string authorization,
        [FromBody] RemoveUserFromReservationRequest request)
    {
        int ownerId = Utils.GetUserIdFromBearerToken(authorization);
        bool isOwner = await _reservationService.OwnerOwnsReservationAsync(ownerId, request.ReservationId);
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
    public async Task<ActionResult<List<ReservationResponse>>> GetReservationsByPath(string path)
    {
        var result = await _reservationService.GetReservationsByPathAsync(path);
        return Ok(result);
    }

    // 8. Získání rezervace podle cesty a ID (public)
    [AllowAnonymous]
    [HttpGet("public/{path}/{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetReservationByPathAndId(string path, int id)
    {
        var result = await _reservationService.GetReservationByPathAndIdAsync(path, id);
        return Ok(result);
    }

    // 9. Přihlášení se na rezervaci (public)
    [AllowAnonymous]
    [HttpPost("signup/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> SignUpForReservation(
        int reservationId, [FromBody] ReservationSignUpRequest request,
        [FromHeader(Name = "Accept-language")] string acceptLanguage)
    {
        var userCurrentCulture = Shared.Common.Utils.GetUserPreferredCurrentCulture(Shared.Common.Utils.GetUserLanguages(acceptLanguage));
        var result = await _reservationService.SignUpForReservationAsync(reservationId, request, userCurrentCulture);
        return Ok(result);
    }

    // 10. Zrušení rezervace (public)
    [AllowAnonymous]
    [HttpPost("cancel/{reservationId:int}")]
    public async Task<ActionResult<ReservationResponse>> CancelReservation(
        [FromRoute] int reservationId, [FromBody] string cancellationCode,
        [FromHeader(Name = "Accept-language")] string acceptLanguage)
    {
        var userCurrentCulture = Shared.Common.Utils.GetUserPreferredCurrentCulture(Shared.Common.Utils.GetUserLanguages(acceptLanguage));
        var result = await _reservationService.CancelReservationAsync(reservationId, cancellationCode, userCurrentCulture);
        return Ok(result);
    }
}