using DataHandlerLibrary.Interfaces;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class SalesItemTransactionServices : IGenericService<SalesItemTransaction>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        public SalesItemTransactionServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
             _dbFactory = dbFactory;
        }

        public async Task AddAsync(SalesItemTransaction entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;

            context.SalesItemTransactions.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<SalesItemTransaction> entities)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var entity in entities)
            {
                entity.Date_Created = DateTime.UtcNow;
                entity.Last_Modified = DateTime.UtcNow;

                // Detach Product navigation property to avoid EF tracking conflicts
                // Only store the Product_ID foreign key
                entity.Product = null;
            }

            context.SalesItemTransactions.AddRange(entities);
            await context.SaveChangesAsync();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SalesItemTransaction>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return context.SalesItemTransactions.AsNoTracking()
                     .Include(s => s.SalesTransaction)
                     .Include(s => s.Product)
                        .ThenInclude(p => p.Department)
                     .Include(s => s.SalesPayout)
                     .Include(s => s.Promotion)
                     .Include(s => s.Created_By)
                     .Include(s => s.Last_Modified_By);
            }
            else
            {
                return context.SalesItemTransactions.AsNoTracking();

            }
        }

        public async Task<IEnumerable<SalesItemTransaction>> GetByConditionAsync(Expression<Func<SalesItemTransaction, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.SalesItemTransactions.AsNoTracking()
                    .Where(expression)
                    .Include(s => s.SalesTransaction)
                    .Include(s => s.Product)
                        .ThenInclude(p => p.Department)
                    .Include(s => s.SalesPayout)
                    .Include(s => s.Promotion)
                    .Include(s => s.Created_By)
                    .Include(s => s.Last_Modified_By)
                    .ToListAsync();

            }
            else
            {
                return await context.SalesItemTransactions.AsNoTracking()
                    .Where(expression)
                    .ToListAsync();
            }
        }

        public Task<SalesItemTransaction> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(SalesItemTransaction entity)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateRangeAsync(IEnumerable<SalesItemTransaction> entities)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var item in entities)
            {
                item.SalesTransaction = null;
                item.Product = null;
                item.SalesPayout = null;
                item.Promotion = null;
                item.Last_Modified = DateTime.UtcNow;
            }
            context.SalesItemTransactions.UpdateRange(entities);
            await context.SaveChangesAsync();
        }

        public Task<string> ValidateAsync(SalesItemTransaction entity)
        {
            throw new NotImplementedException();
        }
    }
}
