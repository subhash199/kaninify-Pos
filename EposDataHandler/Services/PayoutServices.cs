using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class PayoutServices : IGenericService<Payout>
    {
        private readonly DatabaseInitialization context;
        
        public PayoutServices(DatabaseInitialization databaseInitialization)
        {
            context = databaseInitialization;
        }

        public async Task<IEnumerable<Payout>> GetAllAsync(bool includeMapping)
        {
            if (includeMapping) 
                 return await context.Payouts
                .AsNoTracking()
                .Include(p => p.Created_By)
                .Include(p => p.Last_Modified_By)
                .Include(p => p.Site)
                .Include(p => p.Till)
                .ToListAsync();
            return await context.Payouts
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Payout> GetByIdAsync(int id)
        {
            return await context.Payouts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.Is_Deleted);
        }

        public async Task<Payout> GetByDescriptionAsync(string description)
        {
            return await context.Payouts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Payout_Description == description && !p.Is_Deleted && p.Is_Active);
        }

        public async Task AddAsync(Payout entity)
        {
            entity.Created_Date = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            context.Payouts.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payout entity)
        {
            entity.Last_Modified = DateTime.UtcNow;
            context.Payouts.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payout = await GetByIdAsync(id);
            if (payout != null)
            {
                payout.Is_Deleted = true;
                payout.Last_Modified = DateTime.UtcNow;
                await UpdateAsync(payout);
            }
        }

        public async Task<IEnumerable<Payout>> GetByConditionAsync(Expression<Func<Payout, bool>> expression, bool includeMapping)
        {
            var query = context.Payouts.AsNoTracking().Where(expression);
            
            if (includeMapping)
            {
                query = query.Include(p => p.Created_By)
                           .Include(p => p.Last_Modified_By)
                           .Include(p => p.Site)
                           .Include(p => p.Till);
            }
            
            return await query.ToListAsync();
        }

        public async Task<string> ValidateAsync(Payout entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Payout_Description))
            {
                return "Payout description is required.";
            }

            if (entity.Payout_Description.Length > 255)
            {
                return "Payout description cannot exceed 255 characters.";
            }

            // Check for duplicate description
            var existingPayout = await context.Payouts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Payout_Description == entity.Payout_Description 
                                       && p.Id != entity.Id
                                       && !p.Is_Deleted);
            
            if (existingPayout != null)
            {
                return "A payout with this description already exists.";
            }

            return string.Empty; // Valid
        }

        public async Task<Payout> CreatePayoutAsync(string description, int userId, int? siteId = null, int? tillId = null)
        {
            var payout = new Payout
            {
                Payout_Description = description,
                Is_Active = true,
                Is_Deleted = false,
                Created_Date = DateTime.UtcNow,
                Last_Modified = DateTime.UtcNow,
                Created_By_Id = userId,
                Last_Modified_By_Id = userId,
                Site_Id = siteId,
                Till_Id = tillId
            };

            var validationResult = await ValidateAsync(payout);
            if (!string.IsNullOrEmpty(validationResult))
            {
                throw new ArgumentException(validationResult);
            }

            await AddAsync(payout);
            return payout;
        }

        public async Task<IEnumerable<Payout>> GetActivePayoutsAsync()
        {
            return await context.Payouts
                .AsNoTracking()
                .Where(p => p.Is_Active && !p.Is_Deleted)
                .OrderBy(p => p.Payout_Description)
                .ToListAsync();
        }
    }
}