using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class DrawerLogServices : IGenericService<DrawerLog>
    {
        private readonly DatabaseInitialization context;
        
        public DrawerLogServices(DatabaseInitialization databaseInitialization)
        {
            context = databaseInitialization;
        }

        public async Task<IEnumerable<DrawerLog>> GetAllAsync(bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.DrawerLogs.AsNoTracking()
                    .Include(dl => dl.OpenedBy)
                    .Include(dl => dl.Created_By)
                    .Include(dl => dl.Last_Modified_By)
                    .Include(dl => dl.Site)
                    .Include(dl => dl.Till)
                    .ToListAsync();
            }
            return await context.DrawerLogs.AsNoTracking()
                .ToListAsync();
        }

        public async Task<DrawerLog> GetByIdAsync(int id)
        {
            return await context.DrawerLogs.AsNoTracking()
                .Include(dl => dl.OpenedBy)
                .Include(dl => dl.Created_By)
                .Include(dl => dl.Last_Modified_By)
                .Include(dl => dl.Site)
                .Include(dl => dl.Till)
                .FirstOrDefaultAsync(dl => dl.Id == id);
        }

        public async Task AddAsync(DrawerLog entity)
        {
            context.DrawerLogs.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DrawerLog entity)
        {
            context.DrawerLogs.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var drawerLog = await context.DrawerLogs.FindAsync(id);
            if (drawerLog != null)
            {
                context.DrawerLogs.Remove(drawerLog);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<DrawerLog>> GetByConditionAsync(Expression<Func<DrawerLog, bool>> expression, bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.DrawerLogs
                    .AsNoTracking()
                    .Include(dl => dl.OpenedBy)
                    .Include(dl => dl.Created_By)
                    .Include(dl => dl.Last_Modified_By)
                    .Include(dl => dl.Site)
                    .Include(dl => dl.Till)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.DrawerLogs
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<string> ValidateAsync(DrawerLog entity)
        {
            if (entity.OpenedById <= 0)
                return "OpenedById is required.";
            
            if (entity.DrawerOpenDateTime == default)
                return "DrawerOpenDateTime is required.";

            return string.Empty;
        }
    }
}