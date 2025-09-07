using LandingApp.Interfaces;
using Microsoft.Extensions.Logging;

namespace LandingApp.Services
{
    public class LeadExportService : ILeadExportService
    {
        private readonly ILeadService _leadService;
        private readonly IGoogleSheetService _googleSheetService;
        private readonly ILogger<LeadExportService> _logger;

        public LeadExportService(
            ILeadService leadService,
            IGoogleSheetService googleSheetService,
            ILogger<LeadExportService> logger)
        {
            _leadService = leadService;
            _googleSheetService = googleSheetService;
            _logger = logger;
        }

        public async Task ExportNewLeadsAsync()
        {
            _logger.LogInformation("🚀 Запуск экспорта новых заявок в Google Sheets...");

            try
            {
                var newLeads = await _leadService.GetUnexportedLeadsAsync();

                if (newLeads == null || !newLeads.Any())
                {
                    _logger.LogInformation("✅ Нет новых заявок для экспорта.");
                    return;
                }

                _logger.LogInformation("🔄 Найдено {Count} новых заявок для экспорта.", newLeads.Count);

                await _googleSheetService.AppendRowsAsync(newLeads);

                foreach (var lead in newLeads)
                {
                    lead.IsExported = true;
                }

                await _leadService.SaveChangesAsync();

                _logger.LogInformation("✅ Успешно экспортировано {Count} заявок в Google Sheets.", newLeads.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при экспорте заявок в Google Sheets.");
                throw;
            }
        }
    }
}
