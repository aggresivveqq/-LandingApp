using LandingApp.Data;
using LandingApp.Models;
using LandingApp.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace LandingApp.Repository
{
    public class TariffRepository :ITariffRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TariffRepository> _logger;

        public TariffRepository(ApplicationDbContext context, ILogger<TariffRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TariffModel?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Tariffs.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тарифа по ID: {Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<TariffModel>> GetAllAsync()
        {
            try
            {
                return await _context.Tariffs.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех тарифов");
                return Enumerable.Empty<TariffModel>();
            }
        }

        public async Task AddAsync(TariffModel tariff)
        {
            try
            {
                await _context.Tariffs.AddAsync(tariff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении тарифа");
            }
        }

        public void Update(TariffModel tariff)
        {
            try
            {
                _context.Tariffs.Update(tariff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении тарифа");
            }
        }

        public void Delete(TariffModel tariff)
        {
            try
            {
                _context.Tariffs.Remove(tariff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении тарифа");
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении изменений в базе данных");
            }
        }

        public async Task<TariffModel?> GetByNameAsync(string name)
        {
            try
            {
                return await _context.Tariffs
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске тарифа по имени: {Name}", name);
                return null;
            }
        }
    }
}
