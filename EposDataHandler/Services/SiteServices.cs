using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class SiteServices : IGenericService<Site>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public SiteServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Site> GetPrimarySite()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var site = await context.Sites.AsNoTracking()
                .Include(s => s.UserAccesses)
                .ThenInclude(ua => ua.User)
                .Include(s => s.PrimaryUsers)
                .Include(s => s.Tills)
                .FirstOrDefaultAsync(s => s.Is_Active && s.Is_Primary);

            if (site == null)
            {
                throw new InvalidOperationException("No active primary site found.");
            }
            return site;


        }
        public async Task<IEnumerable<Site>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.Sites.AsNoTracking()
                    .Include(s => s.UserAccesses)
                        .ThenInclude(ua => ua.User)
                    .Include(s => s.PrimaryUsers)
                    .Include(s => s.Tills)
                    .ToListAsync();
            }
            return await context.Sites.AsNoTracking()
                .ToListAsync();
        }

        public async Task<Site> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.Sites.AsNoTracking()
                .Include(s => s.UserAccesses)
                    .ThenInclude(ua => ua.User)
                .Include(s => s.PrimaryUsers)
                .Include(s => s.Tills)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Site entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            context.Sites.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Site entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var site = await context.Sites.FindAsync(entity.Id);
            if (site == null)
                throw new KeyNotFoundException("Site not found for update.");

            // Update properties
            context.Entry(site).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var site = await context.Sites.FindAsync(id);
            if (site != null)
            {
                context.Sites.Remove(site);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Site>> GetByConditionAsync(Expression<Func<Site, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.Sites
                    .AsNoTracking()
                    .Include(s => s.UserAccesses)
                        .ThenInclude(ua => ua.User)
                    .Include(s => s.PrimaryUsers)
                    .Include(s => s.Tills)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.Sites
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public Task<string> ValidateAsync(Site entity)
        {
            if (entity == null)
                return Task.FromResult("Site cannot be null.");

            if (string.IsNullOrWhiteSpace(entity.Site_BusinessName))
                return Task.FromResult("Business name is required.");

            if (string.IsNullOrWhiteSpace(entity.Site_AddressLine1))
                return Task.FromResult("Address line 1 is required.");

            if (string.IsNullOrWhiteSpace(entity.Site_City))
                return Task.FromResult("City is required.");

            if (string.IsNullOrWhiteSpace(entity.Site_Country))
                return Task.FromResult("Country is required.");

            if (string.IsNullOrWhiteSpace(entity.Site_Postcode))
                return Task.FromResult("Postcode is required.");

            if (entity.Is_Active == false && entity.Is_Primary)
                return Task.FromResult("A site cannot be inactive and primary at the same time.");

            if (entity.Is_Active && entity.Is_Primary == false)
                return Task.FromResult("A site must be primary if it is active.");

            return Task.FromResult(string.Empty);
        }
    }
}