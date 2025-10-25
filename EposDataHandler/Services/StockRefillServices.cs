using DataHandlerLibrary.Interfaces;
using EFCore.BulkExtensions;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class StockRefillServices : IGenericService<StockRefill>
    {
        private readonly DatabaseInitialization context;
        
        public StockRefillServices(DatabaseInitialization databaseInitialization)
        {
            context = databaseInitialization;
        }

        public async Task AddAsync(StockRefill entity)
        {
            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;

            context.StockRefills.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<StockRefill> entities)
        {
            foreach (var entity in entities)
            {
                entity.Date_Created = DateTime.UtcNow;
                entity.Last_Modified = DateTime.UtcNow;

                // Detach navigation properties to avoid EF tracking conflicts
                entity.SalesItemTransaction = null;
                entity.Refilled_By_User = null;
                entity.Shift = null;
                entity.DayLog = null;
                entity.Created_By_User = null;
                entity.Last_Modified_By_User = null;
            }

            context.StockRefills.AddRange(entities);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockRefill>> GetAllAsync(bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.StockRefills.AsNoTracking()
                    .Include(sr => sr.SalesItemTransaction)
                        .ThenInclude(sit => sit.Product)
                    .Include(sr => sr.Refilled_By_User)
                    .Include(sr => sr.Shift)
                    .Include(sr => sr.DayLog)
                    .Include(sr => sr.Created_By_User)
                    .Include(sr => sr.Last_Modified_By_User)
                    .ToListAsync();
            }
            else
            {
                return await context.StockRefills.AsNoTracking().ToListAsync();
            }
        }

        public async Task<StockRefill?> GetByIdAsync(int id)
        {
            return await context.StockRefills.AsNoTracking()
                .Include(sr => sr.SalesItemTransaction)
                    .ThenInclude(sit => sit.Product)
                .Include(sr => sr.Refilled_By_User)
                .Include(sr => sr.Shift)
                .Include(sr => sr.DayLog)
                .Include(sr => sr.Created_By_User)
                .Include(sr => sr.Last_Modified_By_User)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<IEnumerable<StockRefill>> GetByConditionAsync(Expression<Func<StockRefill, bool>> expression, bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.StockRefills.AsNoTracking()
                    .Where(expression)
                    .Include(sr => sr.SalesItemTransaction)
                        .ThenInclude(sit => sit.Product)
                            .ThenInclude(p => p.Department)
                    .Include(sr => sr.Refilled_By_User)
                    .Include(sr => sr.Shift)
                    .Include(sr => sr.DayLog)
                    .Include(sr => sr.Created_By_User)
                    .Include(sr => sr.Last_Modified_By_User)
                    .ToListAsync();
            }
            else
            {
                return await context.StockRefills.AsNoTracking()
                    .Where(expression)
                    .ToListAsync();
            }
        }

        public async Task UpdateAsync(StockRefill entity)
        {
            entity.Last_Modified = DateTime.UtcNow;
            
            // Detach navigation properties to avoid EF tracking conflicts
            entity.SalesItemTransaction = null;
            entity.Refilled_By_User = null;
            entity.Shift = null;
            entity.DayLog = null;
            entity.Created_By_User = null;
            entity.Last_Modified_By_User = null;

            context.StockRefills.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<StockRefill> entities)
        {
            foreach (var entity in entities)
            {
                entity.Last_Modified = DateTime.UtcNow;
                // Detach navigation properties to avoid EF tracking conflicts
                entity.SalesItemTransaction = null;
                entity.Refilled_By_User = null;
                entity.Shift = null;
                entity.DayLog = null;
                entity.Created_By_User = null;
                entity.Last_Modified_By_User = null;
            }
            context.BulkUpdate(entities);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await context.StockRefills.FindAsync(id);
            if (entity != null)
            {
                context.StockRefills.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<string> ValidateAsync(StockRefill entity)
        {
            var errors = new List<string>();

            // Validate required fields
            if (entity.SaleTransaction_Item_ID <= 0)
                errors.Add("Sale Transaction Item ID is required.");

            if (entity.Refilled_By <= 0)
                errors.Add("Refilled By user is required.");

            if (entity.Created_By_ID <= 0)
                errors.Add("Created By user is required.");

            if (entity.Refill_Quantity <= 0)
                errors.Add("Refill Quantity must be greater than 0.");

            if (entity.Quantity_Refilled < 0)
                errors.Add("Quantity Refilled cannot be negative.");

            if (entity.Quantity_Refilled > entity.Refill_Quantity)
                errors.Add("Quantity Refilled cannot exceed Refill Quantity.");

            // Validate that the SalesItemTransaction exists
            var salesItemExists = await context.SalesItemTransactions
                .AnyAsync(sit => sit.Id == entity.SaleTransaction_Item_ID);
            if (!salesItemExists)
                errors.Add("Referenced Sales Item Transaction does not exist.");

            // Validate that the users exist
            var refilledByUserExists = await context.PosUsers
                .AnyAsync(u => u.Id == entity.Refilled_By);
            if (!refilledByUserExists)
                errors.Add("Refilled By user does not exist.");

            var createdByUserExists = await context.PosUsers
                .AnyAsync(u => u.Id == entity.Created_By_ID);
            if (!createdByUserExists)
                errors.Add("Created By user does not exist.");

            if (entity.Last_Modified_By_ID.HasValue)
            {
                var lastModifiedByUserExists = await context.PosUsers
                    .AnyAsync(u => u.Id == entity.Last_Modified_By_ID.Value);
                if (!lastModifiedByUserExists)
                    errors.Add("Last Modified By user does not exist.");
            }

            return errors.Any() ? string.Join("; ", errors) : string.Empty;
        }

        // Additional business logic methods specific to StockRefill
        public async Task<IEnumerable<StockRefill>> GetPendingRefillsAsync()
        {
            return await GetByConditionAsync(sr => !sr.Stock_Refilled, true);
        }

        public async Task<IEnumerable<StockRefill>> GetRefillsByUserAsync(int userId)
        {
            return await GetByConditionAsync(sr => sr.Refilled_By == userId, true);
        }

        public async Task<IEnumerable<StockRefill>> GetRefillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetByConditionAsync(sr => sr.Refilled_Date >= startDate && sr.Refilled_Date <= endDate, true);
        }

        public async Task<IEnumerable<StockRefill>> GetRefillsByShiftAsync(int shiftId)
        {
            return await GetByConditionAsync(sr => sr.Shift_ID == shiftId, true);
        }

        public async Task<IEnumerable<StockRefill>> GetRefillsByDayLogAsync(int dayLogId)
        {
            return await GetByConditionAsync(sr => sr.DayLog_ID == dayLogId, true);
        }

        public async Task MarkAsCompleteAsync(int stockRefillId, int completedByUserId)
        {
            var stockRefill = await context.StockRefills.FindAsync(stockRefillId);
            if (stockRefill != null)
            {
                stockRefill.Stock_Refilled = true;
                stockRefill.Quantity_Refilled = stockRefill.Refill_Quantity;
                stockRefill.Last_Modified_By_ID = completedByUserId;
                stockRefill.Last_Modified = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateRefillProgressAsync(int stockRefillId, int quantityRefilled, int modifiedByUserId)
        {
            var stockRefill = await context.StockRefills.FindAsync(stockRefillId);
            if (stockRefill != null)
            {
                stockRefill.Quantity_Refilled = Math.Min(quantityRefilled, stockRefill.Refill_Quantity);
                stockRefill.Stock_Refilled = stockRefill.Quantity_Refilled >= stockRefill.Refill_Quantity;
                stockRefill.Last_Modified_By_ID = modifiedByUserId;
                stockRefill.Last_Modified = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
            }
        }
    }
}