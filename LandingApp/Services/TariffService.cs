using LandingApp.Interfaces;
using LandingApp.Models;
using LandingApp.Repository.IRepository;
using System.Collections.Concurrent;

namespace LandingApp.Services
{
    public class TariffService : ITariffService
    {
        private readonly ITariffRepository _repository;

        private List<TariffModel>? _cachedTariffs;
        private DateTime _tariffCacheTime;

        private readonly SemaphoreSlim _cacheLock = new(1, 1);

        private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(10);

        public TariffService(ITariffRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

      
        public async Task<IEnumerable<TariffModel>> GetAllAsync()
        {
            if (_cachedTariffs == null || CacheExpired())
            {
                await _cacheLock.WaitAsync();
                try
                {
                    if (_cachedTariffs == null || CacheExpired())
                    {
                        var tariffs = await _repository.GetAllAsync();
                        _cachedTariffs = tariffs?.ToList() ?? new List<TariffModel>();
                        _tariffCacheTime = DateTime.UtcNow;
                    }
                }
                finally
                {
                    _cacheLock.Release();
                }
            }

            return _cachedTariffs.AsReadOnly();
        }

        
        public async Task<TariffModel?> GetByIdAsync(int id)
        {
            return (await GetAllAsync()).FirstOrDefault(t => t.Id == id);
        }

       
        public async Task<TariffModel?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return (await GetAllAsync()).FirstOrDefault(t =>
                t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

   
        public async Task AddAsync(TariffModel tariff)
        {
            if (tariff == null)
                throw new ArgumentNullException(nameof(tariff));

            await _repository.AddAsync(tariff);
            await _repository.SaveChangesAsync();
            InvalidateCache();
        }

       
        public async Task UpdateAsync(TariffModel tariff)
        {
            if (tariff == null)
                throw new ArgumentNullException(nameof(tariff));

            _repository.Update(tariff);
            await _repository.SaveChangesAsync();
            InvalidateCache();
        }

        
        public async Task DeleteAsync(TariffModel tariff)
        {
            if (tariff == null)
                throw new ArgumentNullException(nameof(tariff));

            _repository.Delete(tariff);
            await _repository.SaveChangesAsync();
            InvalidateCache();
        }

        
        private void InvalidateCache()
        {
            _cachedTariffs = null;
            _tariffCacheTime = DateTime.MinValue;
        }

       
        private bool CacheExpired()
        {
            return (DateTime.UtcNow - _tariffCacheTime) > CacheLifetime;
        }
    }
}
