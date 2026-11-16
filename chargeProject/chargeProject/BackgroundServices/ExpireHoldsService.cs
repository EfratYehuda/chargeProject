using BLL.Services;

namespace chargeProject.BackgroundServices
{
    // Runs in the background for as long as the server is up.
    // Every minute: bookings that stayed "Held" for more than 15 minutes become "Expired",
    // which frees the station (the unique index only counts Held / Confirmed / Charging).
    public class ExpireHoldsService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpireHoldsService> _logger;

        public ExpireHoldsService(IServiceScopeFactory scopeFactory, ILogger<ExpireHoldsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            do
            {
                try
                {
                    // The booking service is Scoped (one per request), and here there is no request,
                    // so we open a scope of our own for each round.
                    using var scope = _scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IChargeBookingService>();

                    int count = await bookingService.ExpireStaleHoldsAsync();
                    if (count > 0)
                        _logger.LogInformation("Expired {Count} stale holds", count);
                }
                catch (Exception ex)
                {
                    // An error in one round must not stop the service forever.
                    _logger.LogError(ex, "Failed to expire stale holds");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}