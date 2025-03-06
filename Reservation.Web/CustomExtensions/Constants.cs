namespace Reservation.Web.CustomExtensions;

public class Constants
{
    public enum Route
    {
        Home,
        Reservation,
        Login,
        Register,
    }
    
    public static readonly Dictionary<Route, string> RouteDictionary = new()
    {
        { Route.Home, "/" },
        { Route.Reservation, "/rezervace" },
        { Route.Login, "/prihlaseni" },
    };
}