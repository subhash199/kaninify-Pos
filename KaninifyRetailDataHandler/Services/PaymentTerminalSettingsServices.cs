using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class PaymentTerminalSettingsServices : IGenericService<PaymentTerminalSetting>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public PaymentTerminalSettingsServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<PaymentTerminalSetting>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext();
            var query = context.PaymentTerminalSettings.AsNoTracking();

            if (includeMapping)
            {
                query = query
                    .Include(x => x.Site)
                    .Include(x => x.Till)
                    .Include(x => x.Created_By)
                    .Include(x => x.Last_Modified_By);
            }

            return await query
                .OrderBy(x => x.Provider)
                .ThenBy(x => x.Site_Id)
                .ThenBy(x => x.Till_Id)
                .ToListAsync();
        }

        public async Task<PaymentTerminalSetting> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.PaymentTerminalSettings.AsNoTracking()
                .Include(x => x.Site)
                .Include(x => x.Till)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(PaymentTerminalSetting entity)
        {
            using var context = _dbFactory.CreateDbContext();
            context.PaymentTerminalSettings.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PaymentTerminalSetting entity)
        {
            using var context = _dbFactory.CreateDbContext();
            var existing = await context.PaymentTerminalSettings.FindAsync(entity.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException("Payment terminal setting not found for update.");
            }

            context.Entry(existing).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var existing = await context.PaymentTerminalSettings.FindAsync(id);
            if (existing != null)
            {
                context.PaymentTerminalSettings.Remove(existing);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PaymentTerminalSetting>> GetByConditionAsync(Expression<Func<PaymentTerminalSetting, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext();
            var query = context.PaymentTerminalSettings.AsNoTracking().Where(expression);

            if (includeMapping)
            {
                query = query
                    .Include(x => x.Site)
                    .Include(x => x.Till)
                    .Include(x => x.Created_By)
                    .Include(x => x.Last_Modified_By);
            }

            return await query.ToListAsync();
        }

        public Task<string> ValidateAsync(PaymentTerminalSetting entity)
        {
            if (entity == null)
            {
                return Task.FromResult("Payment terminal setting cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(entity.Provider))
            {
                return Task.FromResult("Provider is required.");
            }

            if (entity.Is_Enabled)
            {
                if (string.IsNullOrWhiteSpace(entity.Store_Id))
                {
                    return Task.FromResult("A store must be selected before enabling card payments.");
                }

                if (string.IsNullOrWhiteSpace(entity.Terminal_Id))
                {
                    return Task.FromResult("A terminal must be selected before enabling card payments.");
                }

                if (string.IsNullOrWhiteSpace(entity.Access_Token) || string.IsNullOrWhiteSpace(entity.Refresh_Token))
                {
                    return Task.FromResult("Merchant authentication must be completed before enabling card payments.");
                }
            }

            return Task.FromResult(string.Empty);
        }

        public async Task<PaymentTerminalSetting?> GetEffectiveSettingAsync(int? siteId, int? tillId, string provider = "Teya")
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.PaymentTerminalSettings.AsNoTracking()
                .Where(x => x.Provider == provider && x.Site_Id == siteId && (x.Till_Id == tillId || x.Till_Id == null))
                .OrderByDescending(x => x.Till_Id == tillId)
                .ThenByDescending(x => x.Is_Enabled)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTerminalSetting> UpsertForScopeAsync(PaymentTerminalSetting entity)
        {
            using var context = _dbFactory.CreateDbContext();

            var existing = await context.PaymentTerminalSettings.FirstOrDefaultAsync(x =>
                x.Provider == entity.Provider &&
                x.Site_Id == entity.Site_Id &&
                x.Till_Id == entity.Till_Id);

            if (existing == null)
            {
                context.PaymentTerminalSettings.Add(entity);
                await context.SaveChangesAsync();
                return entity;
            }

            entity.Id = existing.Id;
            context.Entry(existing).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
            return existing;
        }
    }
}
