using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class VoidedProductServices : IGenericService<VoidedProduct>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public VoidedProductServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<VoidedProduct>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.VoidedProducts.AsNoTracking()
            .Include(vp => vp.Product)
            .Include(vp => vp.VoidedByUser)
            .Include(vp => vp.Created_By)
            .Include(vp => vp.Last_Modified_By)
            .ToListAsync();
            }
            return await context.VoidedProducts.AsNoTracking()
                .ToListAsync();

        }

        public async Task<VoidedProduct> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.VoidedProducts.AsNoTracking()
                .Include(vp => vp.Product)
                .Include(vp => vp.VoidedByUser)
                .Include(vp => vp.Created_By)
                .Include(vp => vp.Last_Modified_By)
                .FirstOrDefaultAsync(vp => vp.Id == id);
        }

        public async Task AddAsync(VoidedProduct entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Void_Date = DateTime.UtcNow;

            context.VoidedProducts.Add(entity);
            await context.SaveChangesAsync();
        }
        public async Task AddListAsync(List<VoidedProduct> entitys)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            foreach (var entity in entitys)
            {
                entity.Date_Created = DateTime.UtcNow;
                entity.Last_Modified = DateTime.UtcNow;
                entity.Void_Date = DateTime.UtcNow;

                context.VoidedProducts.Add(entity);
            }
            await context.SaveChangesAsync();
        }
        public async Task UpdateAsync(VoidedProduct entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            entity.Last_Modified = DateTime.UtcNow;
            context.VoidedProducts.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var voidedProduct = await context.VoidedProducts.FindAsync(id);
            if (voidedProduct != null)
            {
                context.VoidedProducts.Remove(voidedProduct);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<VoidedProduct>> GetByConditionAsync(Expression<Func<VoidedProduct, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.VoidedProducts
                    .AsNoTracking()
                    .Include(vp => vp.Product)
                    .Include(vp => vp.VoidedByUser)
                    .Include(vp => vp.Created_By)
                    .Include(vp => vp.Last_Modified_By)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.VoidedProducts
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public Task<string> ValidateAsync(VoidedProduct entity)
        {
            if (entity == null)
                return Task.FromResult("Voided product cannot be null.");

            if (entity.Product_ID <= 0)
                return Task.FromResult("Valid product must be selected.");

            if (entity.Voided_Quantity <= 0)
                return Task.FromResult("Voided quantity must be greater than 0.");

            if (entity.Voided_Amount <= 0)
                return Task.FromResult("Voided amount must be greater than 0.");

            if (entity.Voided_By_User_ID <= 0)
                return Task.FromResult("Valid user who voided the product must be specified.");

            return Task.FromResult(string.Empty);
        }

        public async Task<IEnumerable<VoidedProduct>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.VoidedProducts.AsNoTracking()
                .Include(vp => vp.Product)
                .Include(vp => vp.VoidedByUser)
                .Where(vp => vp.Void_Date >= startDate && vp.Void_Date <= endDate)
                .OrderByDescending(vp => vp.Void_Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<VoidedProduct>> GetByUserAsync(int userId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.VoidedProducts.AsNoTracking()
                .Include(vp => vp.Product)
                .Where(vp => vp.Voided_By_User_ID == userId)
                .OrderByDescending(vp => vp.Void_Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalVoidedAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.VoidedProducts
                .Where(vp => vp.Void_Date >= startDate && vp.Void_Date <= endDate)
                .SumAsync(vp => vp.Voided_Amount);
        }
    }
}