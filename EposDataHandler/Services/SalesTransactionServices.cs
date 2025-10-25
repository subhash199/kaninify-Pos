using DataHandlerLibrary.Interfaces;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class SalesTransactionServices : IGenericService<SalesTransaction>
    {
        private readonly DatabaseInitialization context;
        public SalesTransactionServices(DatabaseInitialization _databaseInitialization)
        {
            context = _databaseInitialization;
        }

        public async Task<IEnumerable<SalesTransaction>> GetAllAsync(bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.SalesTransactions.AsNoTracking()
                    .Include(s => s.SalesItemTransactions)
                        .ThenInclude(s => s.Product)
                    .ToListAsync();
            }
            return await context.SalesTransactions.AsNoTracking()
                .ToListAsync();
        }

        public async Task<SalesTransaction> GetByIdAsync(int id)
        {
            return await context.SalesTransactions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(SalesTransaction entity)
        {
            var validationResult = await ValidateAsync(entity);
            if (!string.IsNullOrEmpty(validationResult))
            {
                throw new ArgumentException($"Validation failed: {validationResult}");
            }

            context.SalesTransactions.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SalesTransaction entity)
        {
            context.SalesTransactions.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var salesTransaction = await context.SalesTransactions.FindAsync(id);
            if (salesTransaction != null)
            {
                context.SalesTransactions.Remove(salesTransaction);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SalesTransaction>> GetByConditionAsync(Expression<Func<SalesTransaction, bool>> expression, bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.SalesTransactions.AsNoTracking().Where(expression)
                    .Include(s => s.SalesItemTransactions)
                        .ThenInclude(sit => sit.Product)
                    .Include(s => s.SalesItemTransactions)
                        .ThenInclude(sit => sit.Promotion)
                    .Include(s => s.SalesItemTransactions)
                        .ThenInclude(sit => sit.SalesPayout)
                    .Include(s => s.Created_By)
                    .ToListAsync();
            }
            else
            {
                return await context.SalesTransactions.AsNoTracking().Where(expression).ToListAsync();
            }
        }

        public async Task<SalesTransaction> GetLastTransaction()
        {
            return await context.SalesTransactions.AsNoTracking()
             .OrderByDescending(s => s.Id)
             .Include(s => s.SalesItemTransactions)
                .ThenInclude(s => s.Product)
                .ThenInclude(p => p.Promotion)
             .FirstOrDefaultAsync();
        }
        public Task<string> ValidateAsync(SalesTransaction entity)
        {
            var errors = new List<string>();

            if (entity.DayLog_Id <= 0)
            {
                errors.Add("DayLog_Id is required and must be greater than 0.");
            }

            if (entity.Created_By_Id <= 0)
            {
                errors.Add("Created_By_Id is required and must be greater than 0.");
            }

            if (entity.Last_Modified_By_Id <= 0)
            {
                errors.Add("Last_Modified_By_Id is required and must be greater than 0.");
            }

            return Task.FromResult(errors.Any() ? string.Join("; ", errors) : string.Empty);
        }
    }
}
