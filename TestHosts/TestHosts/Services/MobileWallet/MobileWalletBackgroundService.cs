namespace TestHosts.Services.MobileWallet
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class MobileWalletBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory ScopeFactory;
        private readonly ILogger<MobileWalletBackgroundService> Logger;

        public MobileWalletBackgroundService(IServiceScopeFactory scopeFactory, ILogger<MobileWalletBackgroundService> logger)
        {
            this.ScopeFactory = scopeFactory;
            this.Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    using IServiceScope scope = this.ScopeFactory.CreateScope();
                    MobileWalletService service = scope.ServiceProvider.GetRequiredService<MobileWalletService>();
                    await service.ProcessPendingTransactionsAsync(stoppingToken);
                    await service.ProcessPendingReversalsAsync(stoppingToken);
                    await service.DeliverPendingWebhooksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Mobile wallet background processing failed.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
