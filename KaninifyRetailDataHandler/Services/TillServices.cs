using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class TillServices : IGenericService<Till>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public TillServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Till> GetPrimaryTill()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var till = await context.Tills.AsNoTracking().FirstOrDefaultAsync(t => t.Is_Active && t.Is_Primary);

            if (till == null)
            {
                throw new InvalidOperationException("No active primary till found.");
            }
            return till;
        }

        public async Task<IEnumerable<Till>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Tills.AsNoTracking()
                    .Include(t => t.Site)
                    .Include(t => t.Created_By)
                    .Include(t => t.Last_Modified_By)
                    .ToListAsync();
            }
            return await context.Tills.AsNoTracking()
                .ToListAsync();
        }

        public async Task<Till> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Tills.AsNoTracking()
                .Include(t => t.Site)
                .Include(t => t.Created_By)
                .Include(t => t.Last_Modified_By)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(Till entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Active = true;

            context.Tills.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Till entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Last_Modified = DateTime.UtcNow;
            context.Tills.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var till = await context.Tills.FindAsync(id);
            if (till != null)
            {
                till.Is_Active = false;
                till.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Till>> GetByConditionAsync(Expression<Func<Till, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Tills
                    .AsNoTracking()
                    .Include(t => t.Site)
                    .Include(t => t.Created_By)
                    .Include(t => t.Last_Modified_By)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.Tills
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public Task<string> ValidateAsync(Till entity)
        {
            if (entity == null)
                return Task.FromResult("Till cannot be null.");

            if (string.IsNullOrWhiteSpace(entity.Till_Name))
                return Task.FromResult("Till name is required.");

            if (string.IsNullOrWhiteSpace(entity.Till_Password))
                return Task.FromResult("Till password is required.");

            if (entity.Site_Id == null || entity.Site_Id <= 0)
                return Task.FromResult("Valid site must be selected.");

            return Task.FromResult(string.Empty);
        }

        public async Task<IEnumerable<Till>> GetBySiteIdAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Tills.AsNoTracking()
                .Where(t => t.Site_Id == siteId && t.Is_Active)
                .ToListAsync();
        }
    }
}