using LandingApp.Models;

namespace LandingApp.Interfaces
{
    public interface IGoogleSheetService
    {
        /// <summary>
        /// Выгружает в Google Sheets все лиды, у которых IsExported = false.
        /// </summary>
        Task ExportUnsentLeadsAsync();

        /// <summary>
        /// Выгружает указанный список лидов в Google Sheets.
        /// </summary>
        /// <param name="leads">Список лидов для экспорта</param>
        Task AppendRowsAsync(IEnumerable<LeadModel> leads);
    }
}
