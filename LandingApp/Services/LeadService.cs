using LandingApp.Interfaces;
using LandingApp.Models;
using LandingApp.Repository.IRepository;
using System.Collections.Concurrent;

namespace LandingApp.Services
{
    public class LeadService : ILeadService
    {
        private readonly ILeadRepository _repository;

        private static readonly ConcurrentDictionary<string, (LeadModel Lead, DateTime CachedAt)> _phoneCache = new();
        private static readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);
        private static readonly Timer _cleanupTimer = new Timer(
            CleanupCache,
            null,
            TimeSpan.Zero,            
            TimeSpan.FromMinutes(10)  
        );
        public LeadService(ILeadRepository repository)
        {
            _repository = repository;

        }

        public Task<IEnumerable<LeadModel>> GetAllAsync() => _repository.GetAllAsync();

        public Task<LeadModel?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public async Task AddAsync(LeadModel lead)
        {
            await _repository.AddAsync(lead);
            await _repository.SaveChangesAsync();

            _phoneCache[lead.Phone] = (lead, DateTime.UtcNow);
        }

        public async Task DeleteAsync(int id)
        {
            var lead = await _repository.GetByIdAsync(id);
            if (lead != null)
            {
                await _repository.DeleteAsync(id);
                await _repository.SaveChangesAsync();

                _phoneCache.TryRemove(lead.Phone, out _);
            }
        }

        public async Task<LeadModel?> FindRecentLeadByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;
            if (_phoneCache.TryGetValue(phone, out var cached) &&
                (DateTime.UtcNow - cached.CachedAt) < _cacheLifetime)
            {
                return cached.Lead;
            }

            var lead = await _repository.FindRecentLeadByPhoneAsync(phone);

            if (lead != null)
            {
                _phoneCache[phone] = (lead, DateTime.UtcNow);
            }

            return lead;
        }
        private static void CleanupCache(object? state)
        {
            var now = DateTime.UtcNow;
            foreach (var key in _phoneCache.Keys)
            {
                if (_phoneCache.TryGetValue(key, out var entry) &&
                    (now - entry.CachedAt) > _cacheLifetime)
                {
                    _phoneCache.TryRemove(key, out _);
                }
            }
        }

        public Task<List<LeadModel>> GetUnexportedLeadsAsync() => _repository.GetUnexportedLeadsAsync();

        public Task SaveChangesAsync() => _repository.SaveChangesAsync();
    }
}
