using System.Net;
using System.Text.Json;

namespace Reservation.Api.CustomException;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CustomHttpException ex)
        {
            await HandleCustomHttpExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static Task HandleCustomHttpExceptionAsync(HttpContext context, CustomHttpException ex)
    {
        context.Response.StatusCode = (int)ex.StatusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(ex.Message));
    }
    
    private static Task HandleGenericExceptionAsync(HttpContext context, System.Exception ex)
    {
        context.Response.ContentType = "application/json";

        var innerExceptions = new List<string>();
        var currentException = ex.InnerException;

        // Iterace přes všechny vnitřní výjimky
        while (currentException is not null)
        {
            innerExceptions.Add(currentException.Message);
            currentException = currentException.InnerException;
        }

        // Vytvoření odpovědi s detailními informacemi
        var response = new
        {
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            innerExceptions = innerExceptions
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

}