using EntityFrameworkDatabaseLibrary.Data;
using DataHandlerLibrary.Models;
using System.Linq.Expressions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class ReceiptPrinterServices : IGenericService<ReceiptPrinter>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        
        public ReceiptPrinterServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Implementation of IGenericService<ReceiptPrinter>
        public async Task<IEnumerable<ReceiptPrinter>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.ReceiptPrinters.AsNoTracking()
                    .Include(p => p.Site)
                    .Include(p => p.Till)
                    .Include(p => p.Created_By)
                    .Include(p => p.Last_Modified_By)
                    .Where(p => !p.Is_Deleted)
                    .ToListAsync();
            }
            return await context.ReceiptPrinters.AsNoTracking()
                .Where(p => !p.Is_Deleted)
                .ToListAsync();
        }

        public async Task<ReceiptPrinter> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.ReceiptPrinters.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.Is_Deleted);
        }

        public async Task AddAsync(ReceiptPrinter entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Deleted = false;
            context.ReceiptPrinters.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ReceiptPrinter entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var trackedEntity = await context.ReceiptPrinters.FindAsync(entity.Id);
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
            var printer = await context.ReceiptPrinters.FindAsync(id);
            if (printer != null)
            {
                // Soft delete
                printer.Is_Deleted = true;
                printer.Is_Active = false;
                printer.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ReceiptPrinter>> GetByConditionAsync(Expression<Func<ReceiptPrinter, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (includeMapping)
            {
                return await context.ReceiptPrinters
                    .AsNoTracking()
                    .Include(p => p.Site)
                    .Include(p => p.Till)
                    .Include(p => p.Created_By)
                    .Include(p => p.Last_Modified_By)
                    .Where(expression)
                    .Where(p => !p.Is_Deleted)
                    .ToListAsync();
            }
            return await context.ReceiptPrinters
                .AsNoTracking()
                .Where(expression)
                .Where(p => !p.Is_Deleted)
                .ToListAsync();
        }

        public Task<string> ValidateAsync(ReceiptPrinter entity)
        {
            if (entity == null)
            {
                return Task.FromResult("Printer cannot be null.");
            }

            if (string.IsNullOrEmpty(entity.Printer_Name))
            {
                return Task.FromResult("Printer name is required.");
            }

            if (!Enum.IsDefined(typeof(PrinterPaperWidth), entity.Paper_Width))
            {
                return Task.FromResult("Paper width must be 58mm or 80mm.");
            }

            if (!Enum.IsDefined(typeof(PrinterType), entity.Printer_Type))
            {
                return Task.FromResult("Printer type is required.");
            }

            if (entity.Printer_Type == PrinterType.Ethernet)
            {
                if (string.IsNullOrWhiteSpace(entity.Printer_IP_Address))
                {
                    return Task.FromResult("IP address is required for Ethernet printers.");
                }

                if (!entity.Printer_Port_Number.HasValue || entity.Printer_Port_Number.Value <= 0)
                {
                    return Task.FromResult("Port number must be greater than 0 for Ethernet printers.");
                }
            }

            if (!string.IsNullOrEmpty(entity.Printer_IP_Address) && entity.Printer_Port_Number <= 0)
            {
                return Task.FromResult("Port number must be greater than 0 when IP address is provided.");
            }

            if (!entity.Site_Id.HasValue)
            {
                return Task.FromResult("Site ID is required.");
            }

            return Task.FromResult(string.Empty);
        }

        // Additional methods specific to ReceiptPrinter
        public async Task<ReceiptPrinter> GetPrimaryPrinterBySiteAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.ReceiptPrinters.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Site_Id == siteId && p.Is_Primary && p.Is_Active && !p.Is_Deleted);
        }

        public async Task<IEnumerable<ReceiptPrinter>> GetActivePrintersBySiteAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.ReceiptPrinters.AsNoTracking()
                .Where(p => p.Site_Id == siteId && p.Is_Active && !p.Is_Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReceiptPrinter>> GetPrintersByTillAsync(int tillId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await context.ReceiptPrinters.AsNoTracking()
                .Where(p => p.Till_Id == tillId && p.Is_Active && !p.Is_Deleted)
                .ToListAsync();
        }

        public async Task SetPrimaryPrinterAsync(int printerId, int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            // First, remove primary status from all printers in the site
            var existingPrimaryPrinters = await context.ReceiptPrinters
                .Where(p => p.Site_Id == siteId && p.Is_Primary)
                .ToListAsync();
            
            foreach (var printer in existingPrimaryPrinters)
            {
                printer.Is_Primary = false;
                printer.Last_Modified = DateTime.UtcNow;
            }
        
            // Set the new primary printer
            var newPrimaryPrinter = await context.ReceiptPrinters.FindAsync(printerId);
            if (newPrimaryPrinter != null && newPrimaryPrinter.Site_Id == siteId)
            {
                newPrimaryPrinter.Is_Primary = true;
                newPrimaryPrinter.Last_Modified = DateTime.UtcNow;
            }
        
            await context.SaveChangesAsync();
        }

        public async Task<bool> TestPrinterConnectionAsync(int printerId)
        {
            var printer = await GetByIdAsync(printerId);
            if (printer == null || !printer.Is_Active)
            {
                return false;
            }

            // TODO: Implement actual connection test logic
            // This could involve pinging the IP address or sending a test command
            try
            {
                if (!string.IsNullOrEmpty(printer.Printer_IP_Address))
                {
                    // Network printer connection test
                    // Implementation would depend on your printer communication library
                    return true; // Placeholder
                }
                else
                {
                    // Local printer connection test
                    return true; // Placeholder
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task BulkUpdateAsync(List<ReceiptPrinter> printersToUpdate)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            await context.BulkUpdateAsync(printersToUpdate).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    throw task.Exception ?? new Exception("An error occurred during bulk update.");
                }
            });
        }
    }
}
