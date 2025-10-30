using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class SupplierServices : IGenericService<Supplier>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public SupplierServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Suppliers
                    .AsNoTracking()
                    .Include(s => s.SupplierItems)
                        .ThenInclude(si => si.Product)
                    .Include(s => s.Created_By)
                    .Include(s => s.Last_Modified_By)
                    .Include(s => s.Site)
                    .Include(s => s.Till)
                    .OrderBy(s => s.Supplier_Name)
                    .ToListAsync();
            }
            else
            {
                return await context.Suppliers
                    .AsNoTracking()
                    .OrderBy(s => s.Supplier_Name)
                    .ToListAsync();
            }
        }

        public async Task<Supplier> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Suppliers
                .AsNoTracking()
                .Include(s => s.SupplierItems)
                    .ThenInclude(si => si.Product)
                .Include(s => s.Created_By)
                .Include(s => s.Last_Modified_By)
                .Include(s => s.Site)
                .Include(s => s.Till)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Supplier>> GetByConditionAsync(Expression<Func<Supplier, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Suppliers
                    .AsNoTracking()
                    .Include(s => s.SupplierItems)
                        .ThenInclude(si => si.Product)
                    .Include(s => s.Created_By)
                    .Include(s => s.Last_Modified_By)
                    .Include(s => s.Site)
                    .Include(s => s.Till)
                    .Where(expression)
                    .ToListAsync();
            }
            else
            {
                return await context.Suppliers
                    .AsNoTracking()
                    .Where(expression)
                    .ToListAsync();
            }
        }

        public async Task<Supplier> CreateAsync(Supplier entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Deleted = false;
            entity.Is_Activated = true;

            context.Suppliers.Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<Supplier> UpdateAsync(Supplier entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var trackedEntry = await context.Suppliers.FindAsync(entity.Id);
            if (trackedEntry != null)
            {
                trackedEntry.Supplier_Email = entity.Supplier_Email;
                trackedEntry.Supplier_Name = entity.Supplier_Name;
                trackedEntry.Supplier_Description = entity.Supplier_Description;
                trackedEntry.Supplier_Address = entity.Supplier_Address;
                trackedEntry.Supplier_Phone = entity.Supplier_Phone;
                trackedEntry.Supplier_Mobile = entity.Supplier_Mobile;
                trackedEntry.Supplier_Website = entity.Supplier_Website;
                trackedEntry.Supplier_Credit_Limit = entity.Supplier_Credit_Limit;
                trackedEntry.Is_Activated = entity.Is_Activated;
                trackedEntry.Last_Modified_By_Id = entity.Last_Modified_By_Id;
                trackedEntry.Site_Id = entity.Site_Id;
                trackedEntry.Till_Id = entity.Till_Id;
                trackedEntry.Last_Modified = DateTime.UtcNow;
                context.Suppliers.Update(trackedEntry);
                await context.SaveChangesAsync();
            }
            return trackedEntry;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var supplier = await context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                supplier.Is_Deleted = true;
                supplier.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliers()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Suppliers
                .AsNoTracking()
                .Where(s => s.Is_Activated == true && s.Is_Deleted == false)
                .OrderBy(s => s.Supplier_Name)
                .ToListAsync();
        }

        public async Task<Supplier> GetSupplierByNameAsync(string supplierName)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Supplier_Name.ToLower() == supplierName.ToLower());
        }

        public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Suppliers
                .AsNoTracking()
                .Where(s =>
                           (s.Supplier_Name.Contains(searchTerm) ||
                            s.Supplier_Description.Contains(searchTerm) ||
                            s.Supplier_Email.Contains(searchTerm)))
                .OrderBy(s => s.Supplier_Name)
                .ToListAsync();
        }

        public Task AddAsync(Supplier entity)
        {
            throw new NotImplementedException();
        }

        Task IGenericService<Supplier>.UpdateAsync(Supplier entity)
        {
            return UpdateAsync(entity);
        }

        Task IGenericService<Supplier>.DeleteAsync(int id)
        {
            return DeleteAsync(id);
        }

        public Task<string> ValidateAsync(Supplier entity)
        {
            throw new NotImplementedException();
        }
    }
}