using Reservation.Api.Services;

namespace Reservation.Api.JWT;

public class RefreshTokenCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupWorker> _logger;

    public RefreshTokenCleanupWorker(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                await authService.CleanupInvalidRefreshTokensAsync();
                _logger.LogInformation("Cleanup of invalid refresh tokens completed at {Time}", DateTimeOffset.Now);

                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddHours(3);
                var delay = nextRun - now;

                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during refresh token cleanup");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}