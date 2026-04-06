using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services
{
    public class DayLogServices : IGenericService<DayLog>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        public DayLogServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }
        // Implementation of IGenericService<DayLog>
        public async Task<DayLog> GetLastDayLog()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.DayLogs.AsNoTracking()
                .OrderByDescending(d => d.DayLog_Start_DateTime)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<DayLog>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext();
            if (includeMapping)
            {
                return await context.DayLogs.AsNoTracking()
                    .Include(d => d.Created_By)
                    .Include(d => d.Last_Modified_By)
                    .Include(d => d.Site)
                    .Include(d => d.Till)
                    .ToListAsync();
            }
            return await context.DayLogs.AsNoTracking().ToListAsync();
        }
        public async Task<DayLog> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.DayLogs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }
        public async Task AddAsync(DayLog entity)
        {
            using var context = _dbFactory.CreateDbContext();
            context.DayLogs.Add(entity);
            await context.SaveChangesAsync();
        }
        public async Task UpdateAsync(DayLog entity)
        {
            using var context = _dbFactory.CreateDbContext();

            context.DayLogs.Update(entity);
            await context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();

            var dayLog = await context.DayLogs.FindAsync(id);
            if (dayLog != null)
            {
                context.DayLogs.Remove(dayLog);
                await context.SaveChangesAsync();
            }
        }

        public Task<IEnumerable<DayLog>> GetByConditionAsync(Expression<Func<DayLog, bool>> expression, bool includeMapping)
        {
            throw new NotImplementedException();
        }

        public Task<string> ValidateAsync(DayLog entity)
        {
            throw new NotImplementedException();
        }
    }
   
}
