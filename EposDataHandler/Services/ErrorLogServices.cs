using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services
{
    public class ErrorLogServices : IGenericService<ErrorLog>
    {
        private readonly DatabaseInitialization context;
        
        public ErrorLogServices(DatabaseInitialization databaseInitialization)
        {
            context = databaseInitialization;
        }

        // Implementation of IGenericService<ErrorLog>
        public async Task<IEnumerable<ErrorLog>> GetAllAsync(bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.ErrorLogs.AsNoTracking()
                    .Include(e => e.User)
                    .Include(e => e.Site)
                    .Include(e => e.Till)
                    .Include(e => e.Resolved_By)
                    .OrderByDescending(e => e.Error_DateTime)
                    .ToListAsync();
            }
            return await context.ErrorLogs.AsNoTracking()
                .OrderByDescending(e => e.Error_DateTime)
                .ToListAsync();
        }

        public async Task<ErrorLog> GetByIdAsync(int id)
        {
            return await context.ErrorLogs.AsNoTracking()
                .Include(e => e.User)
                .Include(e => e.Site)
                .Include(e => e.Till)
                .Include(e => e.Resolved_By)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(ErrorLog entity)
        {
            context.ErrorLogs.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ErrorLog entity)
        {
            context.ErrorLogs.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var errorLog = await context.ErrorLogs.FindAsync(id);
            if (errorLog != null)
            {
                context.ErrorLogs.Remove(errorLog);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ErrorLog>> GetByConditionAsync(Expression<Func<ErrorLog, bool>> expression, bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.ErrorLogs.AsNoTracking()
                    .Include(e => e.User)
                    .Include(e => e.Site)
                    .Include(e => e.Till)
                    .Include(e => e.Resolved_By)
                    .Where(expression)
                    .OrderByDescending(e => e.Error_DateTime)
                    .ToListAsync();
            }
            return await context.ErrorLogs.AsNoTracking()
                .Where(expression)
                .OrderByDescending(e => e.Error_DateTime)
                .ToListAsync();
        }

        public Task<string> ValidateAsync(ErrorLog entity)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(entity.Error_Message))
                errors.Add("Error message is required.");

            if (string.IsNullOrWhiteSpace(entity.Error_Type))
                errors.Add("Error type is required.");

            if (entity.Error_DateTime == default)
                errors.Add("Error date time is required.");

            return Task.FromResult(errors.Any() ? string.Join("; ", errors) : string.Empty);
        }

        // Additional helper methods specific to ErrorLog
        public async Task<IEnumerable<ErrorLog>> GetUnresolvedErrorsAsync()
        {
            return await context.ErrorLogs.AsNoTracking()
                .Include(e => e.User)
                .Include(e => e.Site)
                .Include(e => e.Till)
                .Where(e => !e.Is_Resolved)
                .OrderByDescending(e => e.Error_DateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ErrorLog>> GetErrorsBySeverityAsync(ErrorLogSeverity severity)
        {
            return await context.ErrorLogs.AsNoTracking()
                .Include(e => e.User)
                .Include(e => e.Site)
                .Include(e => e.Till)
                .Where(e => e.Severity_Level == severity)
                .OrderByDescending(e => e.Error_DateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ErrorLog>> GetErrorsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await context.ErrorLogs.AsNoTracking()
                .Include(e => e.User)
                .Include(e => e.Site)
                .Include(e => e.Till)
                .Where(e => e.Error_DateTime >= startDate && e.Error_DateTime <= endDate)
                .OrderByDescending(e => e.Error_DateTime)
                .ToListAsync();
        }

        public async Task<ErrorLog> LogErrorAsync(string errorMessage, string errorType, 
            ErrorLogSeverity severity = ErrorLogSeverity.Error, 
            string sourceMethod = null, string sourceClass = null, 
            string stackTrace = null, string userAction = null,
            int? userId = null, int? siteId = null, int? tillId = null)
        {
            var errorLog = new ErrorLog
            {
                Error_Message = errorMessage,
                Error_Type = errorType,
                Severity_Level = severity,
                Source_Method = sourceMethod,
                Source_Class = sourceClass,
                Stack_Trace = stackTrace,
                User_Action = userAction,
                Error_DateTime = DateTime.Now.ToUniversalTime(),
                Date_Created = DateTime.Now.ToUniversalTime(),
                User_Id = userId,
                Site_Id = siteId,
                Till_Id = tillId,
                Application_Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                Is_Resolved = false
            };

            await AddAsync(errorLog);
            return errorLog;
        }

        public async Task MarkAsResolvedAsync(int errorLogId, int resolvedByUserId, string resolutionNotes = null)
        {
            var errorLog = await context.ErrorLogs.FindAsync(errorLogId);
            if (errorLog != null)
            {
                errorLog.Is_Resolved = true;
                errorLog.Resolved_DateTime = DateTime.Now;
                errorLog.Resolved_By_Id = resolvedByUserId;
                errorLog.Resolution_Notes = resolutionNotes;
                
                await UpdateAsync(errorLog);
            }
        }

        public async Task<int> GetErrorCountBySeverityAsync(ErrorLogSeverity severity, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = context.ErrorLogs.AsNoTracking()
                .Where(e => e.Severity_Level == severity);

            if (startDate.HasValue)
                query = query.Where(e => e.Error_DateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Error_DateTime <= endDate.Value);

            return await query.CountAsync();
        }
    }
}