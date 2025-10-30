using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class UnknownProductServices : IGenericService<UnknownProduct>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public UnknownProductServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<UnknownProduct>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.UnknownProducts
                    .AsNoTracking()
                    .Include(up => up.Site)
                    .Include(up => up.Till)
                    .Include(up => up.CreatedBy)
                    .Include(up => up.Daylog)
                    .Include(up => up.Shift)
                    .ToListAsync();
            }
            return await context.UnknownProducts
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UnknownProduct> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.UnknownProducts
                .AsNoTracking()
                .Include(up => up.Site)
                .Include(up => up.Till)
                .Include(up => up.CreatedBy)
                .Include(up => up.Daylog)
                .Include(up => up.Shift)
                .FirstOrDefaultAsync(up => up.Id == id);
        }

        public async Task AddAsync(UnknownProduct entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            // Ensure base fields
            entity.DateCreated = DateTime.UtcNow;
            entity.LastModified = DateTime.UtcNow;
            entity.IsResolved = false;
            entity.SyncStatus = SyncStatus.Pending;

            // Avoid duplicate pending entries for the same barcode
            var existing = await context.UnknownProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.ProductBarcode == entity.ProductBarcode && !up.IsResolved);

            if (existing != null)
                return;

            await context.UnknownProducts.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UnknownProduct entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var tracked = await context.UnknownProducts.FindAsync(entity.Id);
            if (tracked != null)
            {
                context.Entry(tracked).CurrentValues.SetValues(entity);
                tracked.LastModified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var entity = await context.UnknownProducts.FindAsync(id);
            if (entity != null)
            {
                context.UnknownProducts.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UnknownProduct>> GetByConditionAsync(Expression<Func<UnknownProduct, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            IQueryable<UnknownProduct> query = context.UnknownProducts.AsNoTracking();
            if (includeMapping)
            {
                query = query
                    .Include(up => up.Site)
                    .Include(up => up.Till)
                    .Include(up => up.CreatedBy)
                    .Include(up => up.Daylog)
                    .Include(up => up.Shift);
            }
            return await query.Where(expression).ToListAsync();
        }

        public Task<string> ValidateAsync(UnknownProduct entity)
        {
            if (entity == null)
                return Task.FromResult("Unknown product cannot be null.");

            if (string.IsNullOrWhiteSpace(entity.ProductBarcode))
                return Task.FromResult("Product barcode is required.");

            return Task.FromResult(string.Empty);
        }
    }
}