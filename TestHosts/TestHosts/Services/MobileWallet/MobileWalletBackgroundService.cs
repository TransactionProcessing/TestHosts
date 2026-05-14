namespace TestHosts.Services.MobileWallet
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class MobileWalletBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory ScopeFactory;

        public MobileWalletBackgroundService(IServiceScopeFactory scopeFactory)
        {
            this.ScopeFactory = scopeFactory;
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
                catch
                {
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
