using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IAccountService
{
    public Task<ApiResponse<string>> GetPath();
    public Task<ApiResponse<string>> UpdatePath(PathRequest request);
    public Task<ApiResponse<bool>> IsPathTaken(PathRequest request);
}