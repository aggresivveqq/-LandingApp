using LandingApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LandingApp.Services
{
    public class LeadExportWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LeadExportWorker> _logger;
        private readonly TimeSpan _interval;

        public LeadExportWorker(IServiceProvider serviceProvider, ILogger<LeadExportWorker> logger, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            _interval = TimeSpan.FromSeconds(config.GetValue<int>("LeadExport:IntervalSeconds", 30));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LeadExportWorker запущен с интервалом {Interval} сек.", _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExportLeadsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(" LeadExportWorker остановлен по запросу.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при экспорте заявок. Следующая попытка через 60 сек.");
                    await SafeDelay(TimeSpan.FromSeconds(60), stoppingToken);
                }

                await SafeDelay(_interval, stoppingToken);
            }

            _logger.LogInformation("LeadExportWorker полностью остановлен.");
        }

        private async Task ExportLeadsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var exportService = scope.ServiceProvider.GetRequiredService<ILeadExportService>();

            _logger.LogInformation(" Экспорт новых заявок начат.");
            await exportService.ExportNewLeadsAsync();
            _logger.LogInformation("Экспорт новых заявок завершён.");
        }

        private static async Task SafeDelay(TimeSpan delay, CancellationToken token)
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
