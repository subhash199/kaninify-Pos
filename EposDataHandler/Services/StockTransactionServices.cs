using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class StockTransactionServices : IGenericService<StockTransaction>
    {
        private readonly DatabaseInitialization context;
        
        public StockTransactionServices(DatabaseInitialization _databaseInitialization)
        {
            context = _databaseInitialization;
        }

        public async Task<IEnumerable<StockTransaction>> GetAllAsync(bool includeMapping)
        {
            if (includeMapping) 
                return await context.StockTransactions.AsNoTracking()
                .Include(st => st.Product)
                .Include(st => st.DayLog)
                .Include(st => st.Shift)
                // .Include(st => st.Created_By)
                // .Include(st => st.Last_Modified_By)
                // .Include(st => st.From_Site)
                // .Include(st => st.To_Site)
                // .Include(st => st.Till)
                .ToListAsync();
            else
                return await context.StockTransactions.AsNoTracking()
                .ToListAsync();
        }

        public async Task<StockTransaction> GetByIdAsync(int id)
        {
            return await context.StockTransactions.AsNoTracking()
                .Include(st => st.Product)
                .Include(st => st.DayLog)
                .Include(st => st.Shift)
                // .Include(st => st.Created_By)
                // .Include(st => st.Last_Modified_By)
                // .Include(st => st.From_Site)
                // .Include(st => st.To_Site)
                // .Include(st => st.Till)
                .FirstOrDefaultAsync(st => st.Id == id);
        }

        public async Task AddBulkEntityAsync(IEnumerable<StockTransaction> entities)
        {
            foreach (var entity in entities)
            {
                entity.DateCreated = DateTime.UtcNow;
                entity.LastModified = DateTime.UtcNow;
                entity.TransactionDate = DateTime.UtcNow;
                entity.Product = null; // Avoid circular reference issues
            }
            await context.StockTransactions.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        public async Task AddAsync(StockTransaction entity)
        {
            entity.DateCreated = DateTime.UtcNow;
            entity.LastModified = DateTime.UtcNow;
            entity.TransactionDate = DateTime.UtcNow;
            entity.Product = null; // Avoid circular reference issues

            context.StockTransactions.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StockTransaction entity)
        {
            entity.LastModified = DateTime.UtcNow;
            
            context.StockTransactions.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var stockTransaction = await context.StockTransactions.FindAsync(id);
            if (stockTransaction != null)
            {
                context.StockTransactions.Remove(stockTransaction);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<StockTransaction>> GetByConditionAsync(Expression<Func<StockTransaction, bool>> expression, bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.StockTransactions.AsNoTracking()
                    .Where(expression)
                    .Include(st => st.Product)
                    .Include(st => st.DayLog)
                    .Include(st => st.Shift)
                    // .Include(st => st.Created_By)
                    // .Include(st => st.Last_Modified_By)
                    // .Include(st => st.From_Site)
                    // .Include(st => st.To_Site)
                    // .Include(st => st.Till)
                    .ToListAsync();
            }
            else
            {
                return await context.StockTransactions.AsNoTracking()
                    .Where(expression)
                    .ToListAsync();
            }
        }

        public Task<string> ValidateAsync(StockTransaction entity)
        {
            var errors = new List<string>();

            // Validate required fields
            if (entity.ProductId <= 0)
                errors.Add("Product ID is required and must be greater than 0.");

            if (entity.DayLogId <= 0)
                errors.Add("DayLog ID is required and must be greater than 0.");

            if (entity.Quantity == 0)
                errors.Add("Quantity cannot be zero.");

            if (entity.TotalAmount < 0)
                errors.Add("Total amount cannot be negative.");

            // Validate stock transfer type specific rules
            if (entity.StockTransactionType == StockTransferType.Transfer)
            {
                if (!entity.From_Site_Id.HasValue || !entity.To_Site_Id.HasValue)
                    errors.Add("Both From Site and To Site are required for transfer transactions.");
                
                if (entity.From_Site_Id == entity.To_Site_Id)
                    errors.Add("From Site and To Site cannot be the same for transfer transactions.");
            }

            return Task.FromResult(errors.Any() ? string.Join("; ", errors) : string.Empty);
        }

        // Additional helper methods specific to stock transactions
        
        /// <summary>
        /// Gets stock transactions by product ID within a date range
        /// </summary>
        public async Task<IEnumerable<StockTransaction>> GetByProductIdAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.StockTransactions.AsNoTracking()
                .Where(st => st.ProductId == productId);

            if (startDate.HasValue)
                query = query.Where(st => st.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(st => st.TransactionDate <= endDate.Value);

            return await query
                .Include(st => st.Product)
                .Include(st => st.DayLog)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets stock transactions by transaction type within a date range
        /// </summary>
        public async Task<IEnumerable<StockTransaction>> GetByTransactionTypeAsync(StockTransferType transactionType, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.StockTransactions.AsNoTracking()
                .Where(st => st.StockTransactionType == transactionType);

            if (startDate.HasValue)
                query = query.Where(st => st.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(st => st.TransactionDate <= endDate.Value);

            return await query
                .Include(st => st.Product)
                .Include(st => st.DayLog)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets stock transactions for a specific site within a date range
        /// </summary>
        public async Task<IEnumerable<StockTransaction>> GetBySiteAsync(int siteId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.StockTransactions.AsNoTracking()
                .Where(st => st.From_Site_Id == siteId || st.To_Site_Id == siteId);

            if (startDate.HasValue)
                query = query.Where(st => st.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(st => st.TransactionDate <= endDate.Value);

            return await query
                .Include(st => st.Product)
                .Include(st => st.From_Site)
                .Include(st => st.To_Site)
                .Include(st => st.DayLog)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets stock transactions by day log ID
        /// </summary>
        public async Task<IEnumerable<StockTransaction>> GetByDayLogAsync(int dayLogId)
        {
            return await context.StockTransactions.AsNoTracking()
                .Where(st => st.DayLogId == dayLogId)
                .Include(st => st.Product)
                .Include(st => st.DayLog)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Calculates total stock movement for a product within a date range
        /// </summary>
        public async Task<decimal> GetTotalStockMovementAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.StockTransactions.AsNoTracking()
                .Where(st => st.ProductId == productId);

            if (startDate.HasValue)
                query = query.Where(st => st.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(st => st.TransactionDate <= endDate.Value);

            return await query.SumAsync(st => st.Quantity);
        }

        /// <summary>
        /// Gets expired products count within a date range
        /// </summary>
        public async Task<int> GetExpiredProductsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.StockTransactions.AsNoTracking()
                .Where(st => st.StockTransactionType == StockTransferType.Expired);

            if (startDate.HasValue)
                query = query.Where(st => st.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(st => st.TransactionDate <= endDate.Value);

            return await query.CountAsync();
        }

        /// <summary>
        /// Gets theft incidents count within a date range
        /// </summary>
        public async Task<int> GetTheftCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.StockTransactions.AsNoTracking()
                .Where(st => st.StockTransactionType == StockTransferType.Theft);

            if (startDate.HasValue)
                query = query.Where(st => st.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(st => st.TransactionDate <= endDate.Value);

            return await query.CountAsync();
        }

        /// <summary>
        /// Gets stock adjustments count within a date range
        /// </summary>
        public async Task<int> GetStockAdjustmentsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.StockTransactions.AsNoTracking()
                .Where(st => st.StockTransactionType == StockTransferType.Adjustment);

            if (startDate.HasValue)
                query = query.Where(st => st.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(st => st.TransactionDate <= endDate.Value);

            return await query.CountAsync();
        }
    }
}