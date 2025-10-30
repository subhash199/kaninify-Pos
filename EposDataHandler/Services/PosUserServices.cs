using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class PosUserServices : IGenericService<PosUser>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public PosUserServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> CheckIfAnyUsersExist()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.PosUsers.AnyAsync();
        }
        public async Task<IEnumerable<PosUser>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.PosUsers.AsNoTracking()
                    .Include(u => u.PrimarySite)
                    .Include(u => u.SiteAccesses)
                        .ThenInclude(sa => sa.Site)
                    .ToListAsync();
            }

            return await context.PosUsers.AsNoTracking()                
                .ToListAsync();
        }

        public async Task<PosUser> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.PosUsers.AsNoTracking()
                .Include(u => u.PrimarySite)
                .Include(u => u.SiteAccesses)
                    .ThenInclude(sa => sa.Site)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<PosUser> GetByPasscodeAsync(int passcode)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.PosUsers.AsNoTracking()
                .Include(u => u.PrimarySite)
                .FirstOrDefaultAsync(u => u.Passcode == passcode && u.Is_Activated && !u.Is_Deleted);
        }

        public async Task AddAsync(PosUser entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Activated = true;
            entity.Is_Deleted = false;

            context.PosUsers.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PosUser entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Last_Modified = DateTime.UtcNow;
            context.PosUsers.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var user = await context.PosUsers.FindAsync(id);
            if (user != null)
            {
                user.Is_Deleted = true;
                user.Is_Activated = false;
                user.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PosUser>> GetByConditionAsync(Expression<Func<PosUser, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.PosUsers
                    .AsNoTracking()
                    .Include(u => u.PrimarySite)
                    .Include(u => u.SiteAccesses)
                        .ThenInclude(sa => sa.Site)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.PosUsers
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public Task<string> ValidateAsync(PosUser entity)
        {
            if (entity == null)
                return Task.FromResult("User cannot be null.");

            if (string.IsNullOrWhiteSpace(entity.First_Name))
                return Task.FromResult("First name is required.");

            if (string.IsNullOrWhiteSpace(entity.Last_Name))
                return Task.FromResult("Last name is required.");

            var passcodeLength = entity.Passcode.ToString().Length;
            if (entity.Passcode <= 0 || passcodeLength < 4 || passcodeLength > 10)
                return Task.FromResult("Passcode must be between 4 and 10 digits.");

            return Task.FromResult(string.Empty);
        }

        public async Task<bool> IsPasscodeUniqueAsync(int passcode, int? excludeUserId = null)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var query = context.PosUsers.Where(u => u.Passcode == passcode && !u.Is_Deleted);
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync();
        }
    }
}