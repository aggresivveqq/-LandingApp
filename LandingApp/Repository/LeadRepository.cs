using LandingApp.Data;
using LandingApp.Models;
using LandingApp.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace LandingApp.Repository
{
    public class LeadRepository : ILeadRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeadRepository> _logger;

        public LeadRepository(ApplicationDbContext context, ILogger<LeadRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<LeadModel>> GetAllAsync()
        {
            try
            {
                return await _context.Leads.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка лидов");
                return Enumerable.Empty<LeadModel>();
            }
        }

        public async Task<LeadModel?> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("ID должен быть больше нуля", nameof(id));
                }

                return await _context.Leads.FindAsync(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Передан неверный ID: {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении лида по ID: {Id}", id);
                return null;
            }
        }

        public async Task AddAsync(LeadModel lead)
        {
            try
            {
                await _context.Leads.AddAsync(lead);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении лида в базу данных");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при добавлении лида");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var lead = await _context.Leads.FindAsync(id);
                if (lead != null)
                {
                    _context.Leads.Remove(lead);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("Лид с ID {Id} не найден для удаления", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении лида с ID: {Id}", id);
                throw;
            }
        }
        public async Task<LeadModel?> FindRecentLeadByPhoneAsync(string phone)
        {
            return await _context.Leads
                .Where(l => l.Phone == phone)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }
        public async Task<List<LeadModel>> GetUnexportedLeadsAsync()
        {
            return await _context.Leads
                .Where(l => !l.IsExported)
                .ToListAsync();
        }
        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении изменений в базе данных");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при сохранении изменений в базе данных");
                throw;
            }
        }
    }
}
