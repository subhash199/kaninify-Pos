using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataHandlerLibrary.Services
{
    public class RetailerServices : IGenericService<Retailer>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public RetailerServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Retailer>> GetAllAsync(bool includeMapping)
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await _context.Retailers.AsNoTracking().ToListAsync();
        }

        public async Task<Retailer> GetByIdAsync(int id)
        {
            // Note: Retailer uses Guid as key, not int, so this method won't work directly
            // This is implemented to satisfy the interface
            throw new NotImplementedException("Use GetByGuidAsync instead for Retailer objects");
        }

        public async Task<Retailer> GetByGuidAsync(Guid retailerId)
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await _context.Retailers.FindAsync(retailerId);
        }

        public async Task<Retailer> GetFirstActiveRetailerAsync()
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await _context.Retailers
                .AsNoTracking()
                .Where(r => r.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Retailer entity)
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            await _context.Retailers.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Retailer entity)
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Try to get a tracked instance from the context (or database)
            var existingEntity = await _context.Retailers.FindAsync(entity.RetailerId);

            if (existingEntity != null)
            {
                // existingEntity is tracked by the context.
                // Copy values from the incoming entity into the tracked instance so EF Core can detect changes.
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                // Not tracked / not found: attach the incoming entity and mark as modified so EF will update it.
                _context.Retailers.Update(entity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            var allRetailers = await _context.Retailers.ToListAsync();
            _context.Retailers.RemoveRange(allRetailers);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            // Note: Retailer uses Guid as key, not int
            throw new NotImplementedException("Use DeleteByGuidAsync instead for Retailer objects");
        }

        public async Task DeleteByGuidAsync(Guid retailerId)
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            var retailer = await GetByGuidAsync(retailerId);
            if (retailer != null)
            {
                _context.Retailers.Remove(retailer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Retailer>> GetByConditionAsync(Expression<Func<Retailer, bool>> expression, bool includeMapping)
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await _context.Retailers.AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<string> ValidateAsync(Retailer entity)
        {
            // Implement validation logic for Retailer
            if (string.IsNullOrEmpty(entity.RetailName))
                return "Retail name is required";

            if (string.IsNullOrEmpty(entity.FirstLine_Address))
                return "First line of address is required";

            if (string.IsNullOrEmpty(entity.City))
                return "City is required";

            if (string.IsNullOrEmpty(entity.Country))
                return "Country is required";

            if (string.IsNullOrEmpty(entity.Postcode))
                return "Postcode is required";

            if (string.IsNullOrEmpty(entity.Contact_Number))
                return "Contact number is required";

            return string.Empty;
        }

        public async Task<Retailer> GetByEmailAsync(string email)
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            return await _context.Retailers
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Email == email);
        }
    }
}