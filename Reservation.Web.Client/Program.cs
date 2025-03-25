using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Reservation.Web.Client.Services;

namespace Reservation.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddAuthorizationCore();

            // Registrace handlerů
            builder.Services.AddTransient<AuthTokenHandler>();
            builder.Services.AddTransient<TokenRefreshHandler>();

            // Registrace HttpClientu s našimi handlery pro běžné požadavky
            builder.Services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7045");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddHttpMessageHandler<AuthTokenHandler>()
            .AddHttpMessageHandler<TokenRefreshHandler>();

            // Registrace pojmenovaného HttpClientu bez handlerů, který se použije v refresh logice
            builder.Services.AddHttpClient("NoHandlerClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7045");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            // Registrace ostatních služeb
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}
