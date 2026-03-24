using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;
using EFCore.BulkExtensions;

namespace DataHandlerLibrary.Services
{
    public class SupplierItemsServices : IGenericService<SupplierItem>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public SupplierItemsServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<SupplierItem>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.SupplierItems
                    .AsNoTracking()
                    .Include(si => si.Supplier)
                    .Include(si => si.Product)
                    .Include(si => si.Created_By)
                    .Include(si => si.Last_Modified_By)
                    .Include(si => si.Site)
                    .Include(si => si.Till)
                    .Where(si => !si.Is_Deleted)
                    .OrderBy(si => si.Supplier.Supplier_Name)
                    .ThenBy(si => si.Product.Product_Name)
                    .ToListAsync();
            }
            else
            {
                return await context.SupplierItems
                    .AsNoTracking()
                    .Where(si => !si.Is_Deleted)
                    .OrderBy(si => si.SupplierId)
                    .ThenBy(si => si.ProductId)
                    .ToListAsync();
            }
        }

        public async Task<SupplierItem> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Supplier)
                .Include(si => si.Product)
                .Include(si => si.Created_By)
                .Include(si => si.Last_Modified_By)
                .Include(si => si.Site)
                .Include(si => si.Till)
                .FirstOrDefaultAsync(si => si.Id == id && !si.Is_Deleted);
        }

        public async Task<IEnumerable<SupplierItem>> GetByConditionAsync(Expression<Func<SupplierItem, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.SupplierItems
                    .AsNoTracking()
                    .Include(si => si.Supplier)
                    .Include(si => si.Product)
                    .Include(si => si.Created_By)
                    .Include(si => si.Last_Modified_By)
                    .Include(si => si.Site)
                    .Include(si => si.Till)
                    .Where(expression)
                    .ToListAsync();
            }
            else
            {
                return await context.SupplierItems
                    .AsNoTracking()
                    .Where(expression)
                    .ToListAsync();
            }
        }

        public async Task<SupplierItem> CreateAsync(SupplierItem entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Deleted = false;
            entity.Is_Active = true;

            context.SupplierItems.Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<SupplierItem> UpdateAsync(SupplierItem entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Last_Modified = DateTime.UtcNow;
            context.SupplierItems.Update(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var supplierItem = await context.SupplierItems.FindAsync(id);
            if (supplierItem != null)
            {
                supplierItem.Is_Deleted = true;
                supplierItem.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<SupplierItem>> GetItemsBySupplierId(int supplierId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Product)
                .Where(si => si.SupplierId == supplierId && !si.Is_Deleted && si.Is_Active)
                .OrderBy(si => si.Product.Product_Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupplierItem>> GetItemsByProductId(int productId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Supplier)
                .Where(si => si.ProductId == productId && !si.Is_Deleted && si.Is_Active)
                .OrderBy(si => si.Cost_Per_Unit)
                .ToListAsync();
        }

        public async Task<SupplierItem> GetBySupplierAndProductAsync(int supplierId, int productId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Supplier)
                .Include(si => si.Product)
                .FirstOrDefaultAsync(si => si.SupplierId == supplierId && 
                                          si.ProductId == productId && 
                                          !si.Is_Deleted);
        }

        public async Task<SupplierItem> GetBySupplierProductCodeAsync(string supplierProductCode)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Supplier)
                .Include(si => si.Product)
                .FirstOrDefaultAsync(si => si.Supplier_Product_Code == supplierProductCode && 
                                          !si.Is_Deleted && 
                                          si.Is_Active);
        }

        public async Task<SupplierItem> GetByOuterCaseBarcodeAsync(string outerCaseBarcode)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Supplier)
                .Include(si => si.Product)
                .FirstOrDefaultAsync(si => si.Product_OuterCaseBarcode == outerCaseBarcode && 
                                          !si.Is_Deleted && 
                                          si.Is_Active);
        }

        public async Task<IEnumerable<SupplierItem>> GetActiveSupplierItems()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Supplier)
                .Include(si => si.Product)
                .Where(si => si.Is_Active && !si.Is_Deleted)
                .OrderBy(si => si.Supplier.Supplier_Name)
                .ThenBy(si => si.Product.Product_Name)
                .ToListAsync();
        }

        public async Task<decimal> GetLowestCostForProductAsync(int productId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var lowestCostItem = await context.SupplierItems
                .AsNoTracking()
                .Where(si => si.ProductId == productId && !si.Is_Deleted && si.Is_Active)
                .OrderBy(si => si.Cost_Per_Unit)
                .FirstOrDefaultAsync();

            return lowestCostItem?.Cost_Per_Unit ?? 0;
        }

        public async Task<SupplierItem> GetSupplierItemByProductIdAsync(int productId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.SupplierItems
                .AsNoTracking()
                .Include(si => si.Supplier)
                .Include(si => si.Product)
                .FirstOrDefaultAsync(si => si.ProductId == productId && !si.Is_Deleted && si.Is_Active);
        }

        public Task BulkInsertUpdateAsync(List<SupplierItem> entities)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return context.BulkInsertOrUpdateAsync(entities);
        }

        public Task AddAsync(SupplierItem entity)
        {
            throw new NotImplementedException();
        }

        Task IGenericService<SupplierItem>.UpdateAsync(SupplierItem entity)
        {
            return UpdateAsync(entity);
        }

        Task IGenericService<SupplierItem>.DeleteAsync(int id)
        {
            return DeleteAsync(id);
        }

        public Task<string> ValidateAsync(SupplierItem entity)
        {
            throw new NotImplementedException();
        }
    }
}