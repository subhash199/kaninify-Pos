using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class UserSiteAccessServices : IGenericService<UserSiteAccess>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public UserSiteAccessServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<UserSiteAccess>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.UserSiteAccesses.AsNoTracking()
                    .Include(usa => usa.User)
                    .Include(usa => usa.Site)
                    .Include(usa => usa.Created_By)
                    .Include(usa => usa.Last_Modified_By)
                    .ToListAsync();
            }
            return await context.UserSiteAccesses.AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserSiteAccess> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.UserSiteAccesses.AsNoTracking()
                .Include(usa => usa.User)
                .Include(usa => usa.Site)
                .Include(usa => usa.Created_By)
                .Include(usa => usa.Last_Modified_By)
                .FirstOrDefaultAsync(usa => usa.Id == id);
        }

        public async Task AddAsync(UserSiteAccess entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Date_Granted = DateTime.UtcNow;
            entity.Is_Active = true;

            context.UserSiteAccesses.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserSiteAccess entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var existingEntity = await context.UserSiteAccesses.FindAsync(entity.Id);
            if (existingEntity != null)
            {
                // Update only the properties that can change
                existingEntity.Is_Active = entity.Is_Active;
                existingEntity.Date_Revoked = entity.Date_Revoked;
                existingEntity.Last_Modified_By_Id = entity.Last_Modified_By_Id;
                existingEntity.Last_Modified = DateTime.UtcNow;

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var access = await context.UserSiteAccesses.FindAsync(id);
            if (access != null)
            {
                access.Is_Active = false;
                access.Date_Revoked = DateTime.UtcNow;
                access.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UserSiteAccess>> GetByConditionAsync(Expression<Func<UserSiteAccess, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.UserSiteAccesses
                    .AsNoTracking()
                    .Include(usa => usa.User)
                    .Include(usa => usa.Site)
                    .Include(usa => usa.Created_By)
                    .Include(usa => usa.Last_Modified_By)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.UserSiteAccesses
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public Task<string> ValidateAsync(UserSiteAccess entity)
        {
            if (entity == null)
                return Task.FromResult("User site access cannot be null.");

            if (entity.User_Id <= 0)
                return Task.FromResult("Valid user must be selected.");

            if (entity.Site_Id <= 0)
                return Task.FromResult("Valid site must be selected.");

            return Task.FromResult(string.Empty);
        }

        public async Task<IEnumerable<UserSiteAccess>> GetByUserIdAsync(int userId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.UserSiteAccesses.AsNoTracking()
                .Include(usa => usa.Site)
                .Where(usa => usa.User_Id == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserSiteAccess>> GetBySiteIdAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.UserSiteAccesses.AsNoTracking()
                .Include(usa => usa.User)
                .Where(usa => usa.Site_Id == siteId)
                .ToListAsync();
        }

        public async Task<bool> HasUserAccessToSiteAsync(int userId, int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.UserSiteAccesses.AsNoTracking()
                .AnyAsync(usa => usa.User_Id == userId && usa.Site_Id == siteId && usa.Is_Active);
        }

        public async Task UpsertAsync(UserSiteAccess entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            // Check if a UserSiteAccess record already exists for this user and site
            var existingAccess = await context.UserSiteAccesses
                .FirstOrDefaultAsync(usa => usa.User_Id == entity.User_Id && usa.Site_Id == entity.Site_Id);

            if (existingAccess != null)
            {
                // Update existing record
                existingAccess.Is_Active = entity.Is_Active;
                existingAccess.Date_Revoked = entity.Is_Active ? null : DateTime.UtcNow;
                existingAccess.Last_Modified_By_Id = entity.Last_Modified_By_Id;
                existingAccess.Last_Modified = DateTime.UtcNow;

                // If reactivating, update the date granted
                if (entity.Is_Active && !existingAccess.Is_Active)
                {
                    existingAccess.Date_Granted = DateTime.UtcNow;
                }
            }
            else
            {
                // Create new record
                entity.Date_Created = DateTime.UtcNow;
                entity.Last_Modified = DateTime.UtcNow;
                entity.Date_Granted = DateTime.UtcNow;
                entity.Is_Active = true;
                entity.Date_Revoked = null;

                context.UserSiteAccesses.Add(entity);
            }

            await context.SaveChangesAsync();
        }
    }
}