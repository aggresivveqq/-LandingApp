using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using LandingApp.Data; // твой DbContext
using LandingApp.Interfaces;
using LandingApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LandingApp.Services
{
    public class GoogleSheetService : IGoogleSheetService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleSheetService> _logger;
        private readonly SheetsService _sheetsService;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _spreadsheetId;
        private readonly string _sheetName;

        public GoogleSheetService(
            IConfiguration configuration,
            ILogger<GoogleSheetService> logger,
            ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;

            var credentialsPath = _configuration["GoogleSheets:CredentialsFilePath"];
            _spreadsheetId = _configuration["GoogleSheets:SpreadsheetId"];
            _sheetName = _configuration["GoogleSheets:SheetName"] ?? "Лист1";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "SmartLanding"
            });
        }

        public async Task ExportUnsentLeadsAsync()
        {
            var unsentLeads = await _dbContext.Leads
                .Where(l => !l.IsExported)
                .ToListAsync();

            if (!unsentLeads.Any())
            {
                _logger.LogInformation("Нет новых лидов для выгрузки.");
                return;
            }

            _logger.LogInformation("Найдено {Count} лидов для выгрузки.", unsentLeads.Count);

            try
            {
                var values = unsentLeads.Select(lead => new List<object>
                {
                    lead.Id,
                    $"'{lead.Name}",
                    $"'{lead.Phone}",
                    $"'{lead.City ?? "Не указан"}",
                    $"'{lead.TariffName}",
                    $"'{lead.Comment ?? "Заявка с лендинга"}",
                    $"'{lead.Need ?? "Тариф"}",
                    $"'{lead.Source ?? "Лендинг"}",
                    lead.CreatedAt.AddHours(5).ToString("yyyy-MM-dd HH:mm:ss")
                }).Cast<IList<object>>().ToList();

                var valueRange = new ValueRange { Values = values };

                var request = _sheetsService.Spreadsheets.Values.Append(
                    valueRange, _spreadsheetId, $"{_sheetName}!A:J");

                request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                await request.ExecuteAsync();

                _logger.LogInformation("✅ Успешно выгружено {Count} лидов в Google Sheets.", values.Count);

                // Помечаем как экспортированные
                foreach (var lead in unsentLeads)
                {
                    lead.IsExported = true;
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("⚡ База обновлена: лиды помечены как экспортированные.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при выгрузке лидов в Google Sheets.");
            }
        }
        public async Task AppendRowsAsync(IEnumerable<LeadModel> leads)
        {
            if (leads == null || !leads.Any())
            {
                _logger.LogWarning("Передан пустой список лидов для выгрузки.");
                return;
            }

            try
            {
                var values = leads.Select(lead => new List<object>
        {
            lead.Id,
            $"'{lead.Name}",
            $"'{lead.Phone}",
            $"'{lead.City ?? "Не указан"}",
            $"'{lead.TariffName}",
            $"'{lead.Comment ?? "Заявка с лендинга"}",
            $"'{lead.Need ?? "Тариф"}",
            $"'{lead.Source ?? "Лендинг"}",
            lead.CreatedAt.AddHours(5).ToString("yyyy-MM-dd HH:mm:ss")
        }).Cast<IList<object>>().ToList();

                var valueRange = new ValueRange { Values = values };

                var request = _sheetsService.Spreadsheets.Values.Append(
                    valueRange, _spreadsheetId, $"{_sheetName}!A:J");

                request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                await request.ExecuteAsync();

                _logger.LogInformation("✅ Успешно выгружено {Count} лидов в Google Sheets.", values.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при выгрузке лидов в Google Sheets.");
                throw;
            }
        }

    }
}
