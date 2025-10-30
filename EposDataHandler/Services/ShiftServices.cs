using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class ShiftServices : IGenericService<Shift>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public ShiftServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Shift>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.Shifts.AsNoTracking()
                    .Include(s => s.PosUser)
                    .Include(s => s.Till)
                    .Include(s => s.Site)
                    .Include(s => s.DayLog)
                    .Include(s => s.SalesTransactions)
                    .ToListAsync();
            }
            return await context.Shifts.AsNoTracking()
                .ToListAsync();
        }

        public async Task<Shift> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.Shifts.AsNoTracking()
                .Include(s => s.PosUser)
                .Include(s => s.Till)
                .Include(s => s.Site)
                .Include(s => s.DayLog)
                .Include(s => s.SalesTransactions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Shift entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Active = true;

            context.Shifts.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Shift entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var trackedEntity = await context.Shifts.FindAsync(entity.Id);
            if (trackedEntity != null)
            {
                context.Entry(trackedEntity).CurrentValues.SetValues(entity);
                trackedEntity.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var shift = await context.Shifts.FindAsync(id);
            if (shift != null)
            {
                shift.Is_Active = false;
                shift.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Shift>> GetByConditionAsync(Expression<Func<Shift, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.Shifts
                    .AsNoTracking()
                    .Include(s => s.PosUser)
                    .Include(s => s.Till)
                    .Include(s => s.Site)
                    .Include(s => s.DayLog)
                    .Include(s => s.SalesTransactions)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.Shifts
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<Shift> GetLastShiftLog()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.Shifts.
                AsNoTracking()
                .OrderByDescending(s => s.Shift_Start_DateTime)
                .FirstOrDefaultAsync();
        }

        public Task<string> ValidateAsync(Shift entity)
        {
            if (entity == null)
                return Task.FromResult("Shift cannot be null.");

            if (entity.Till_Id <= 0)
                return Task.FromResult("Valid till must be selected.");

            if (entity.Site_Id <= 0)
                return Task.FromResult("Valid site must be selected.");

            if (entity.Shift_Start_DateTime == default)
                return Task.FromResult("Shift start date/time is required.");

            if (entity.Shift_End_DateTime.HasValue && entity.Shift_End_DateTime <= entity.Shift_Start_DateTime)
                return Task.FromResult("Shift end date/time must be after start date/time.");

            return Task.FromResult(string.Empty);
        }

        public async Task<Shift> GetActiveShiftByUserAsync(int userId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.Shifts.AsNoTracking()
                .Include(s => s.Till)
                .Include(s => s.Site)
                .FirstOrDefaultAsync(s => s.PosUser_Id == userId && s.Is_Active && !s.Shift_End_DateTime.HasValue);
        }

        public async Task<IEnumerable<Shift>> GetShiftsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.Shifts.AsNoTracking()
                .Include(s => s.PosUser)
                .Include(s => s.Till)
                .Include(s => s.Site)
                .Where(s => s.Shift_Start_DateTime >= startDate && s.Shift_Start_DateTime <= endDate)
                .OrderByDescending(s => s.Shift_Start_DateTime)
                .ToListAsync();
        }

        public async Task CloseShiftAsync(int shiftId, decimal closingCashAmount, string closingNotes = null)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var shift = await context.Shifts.FindAsync(shiftId);
            if (shift != null && shift.Is_Active && !shift.Shift_End_DateTime.HasValue)
            {
                shift.Shift_End_DateTime = DateTime.UtcNow;
                shift.Closing_Cash_Amount = closingCashAmount;
                shift.Closing_Notes = closingNotes;
                shift.Cash_Variance = closingCashAmount - (shift.Expected_Cash_Amount ?? 0);
                shift.Last_Modified = DateTime.UtcNow;

                await context.SaveChangesAsync();
            }
        }
    }
}