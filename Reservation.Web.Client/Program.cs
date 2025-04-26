using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Reservation.Web.Client.CustomExtensions;
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

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
                config.SnackbarConfiguration.PreventDuplicates = true;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 5000;
                config.SnackbarConfiguration.HideTransitionDuration = 250;
                config.SnackbarConfiguration.ShowTransitionDuration = 250;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddTransient<AuthTokenHandler>();
            builder.Services.AddTransient<TokenRefreshHandler>();

            builder.Services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
                {
                    client.BaseAddress = new Uri(Constants.ApiBaseAddress);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.Timeout = TimeSpan.FromSeconds(15);
                })
                .AddHttpMessageHandler<AuthTokenHandler>()
                .AddHttpMessageHandler<TokenRefreshHandler>();

            builder.Services.AddHttpClient("NoHandlerClient", client =>
            {
                client.BaseAddress = new Uri(Constants.ApiBaseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddSingleton<GlobalState>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddAuthorizationCore();

            await builder.Build().RunAsync();
        }
    }
}