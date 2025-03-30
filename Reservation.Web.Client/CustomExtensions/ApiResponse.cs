using System.Net;

namespace Reservation.Web.Client.CustomExtensions;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}