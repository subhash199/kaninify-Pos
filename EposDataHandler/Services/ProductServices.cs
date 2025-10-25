using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class ProductServices : IGenericService<Product>
    {
        private readonly DatabaseInitialization context;
        public ProductServices(DatabaseInitialization sDatabaseInitialization)
        {
            context = sDatabaseInitialization;
        }

        // Implementation of IGenericService<Product>
        public async Task<IEnumerable<Product>> GetAllAsync(bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.Products.AsNoTracking()
                    .Include(p => p.Department)
                    .Include(p => p.Promotion)
                    .Include(p => p.VAT)
                    .ToListAsync();
            }
            return await context.Products.AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product> GetProductByBarcode(string barcode, bool tracked, bool includeMapping)
        {
            if (tracked)
            {
                if (includeMapping)
                {
                    return await context.Products
                        .Include(p => p.Department)
                        .Include(p => p.Promotion)
                        .Include(p => p.VAT)
                        .FirstOrDefaultAsync(p => p.Product_Barcode == barcode);
                }
                return await context.Products.FirstOrDefaultAsync(p => p.Product_Barcode == barcode);
            }
            else
            {
                if (includeMapping)
                {
                    return await context.Products
                        .AsNoTracking()
                        .Include(p => p.Department)
                        .Include(p => p.Promotion)
                        .Include(p => p.VAT)
                        .FirstOrDefaultAsync(p => p.Product_Barcode == barcode);
                }
                return await context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Product_Barcode == barcode);
            }


        }
        public async Task<Product> GetByIdAsync(int id)
        {
            return await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product entity)
        {
            entity.Date_Created = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Activated = true;
            context.Products.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product entity)
        {
            var trackedEntity = await context.Products.FindAsync(entity.Id);
            if (trackedEntity != null)
            {
                context.Entry(trackedEntity).CurrentValues.SetValues(entity);
                trackedEntity.Last_Modified = DateTime.UtcNow;
                trackedEntity.Is_Activated = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product != null)
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetByConditionAsync(Expression<Func<Product, bool>> expression, bool includeMapping)
        {
            if (includeMapping)
            {
                return await context.Products
                    .AsNoTracking()
                    .Include(p => p.Department)
                    .Include(p => p.Promotion)
                    .Include(p => p.VAT)
                    .Where(expression)
                    .ToListAsync();
            }
            return await context.Products
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<Product> AddGenericProduct(string productName, int departmentId, int vatid, int userId, int siteId, int tillId)
        {
            Product product = new Product
            {
                Product_Name = productName,
                Product_Barcode = productName,
                ShelfQuantity = int.MaxValue,
                StockroomQuantity = 0,
                Department_ID = departmentId,
                VAT_ID = vatid,
                Product_Cost = 0,
                Product_Selling_Price = 0,
                Profit_On_Return_Percentage = 0,
                Product_Min_Order = 0,
                Product_Low_Stock_Alert_QTY = 0,
                Product_Min_Stock_Level = 0,
                Product_Unit_Per_Case = 0,
                Product_Cost_Per_Case = 0,
                Expiry_Date = DateTime.MaxValue,
                Is_Activated = true,
                Is_Deleted = false,
                Is_Price_Changed = true,
                Date_Created = DateTime.UtcNow,
                Last_Modified = DateTime.UtcNow,
                Allow_Discount = false,
                Created_By_Id = userId,
                Last_Modified_By_Id = userId,
                Site_Id = siteId,
                Till_Id = tillId
            };

            await context.Products.AddAsync(product);
            return product;
        }

        public Task<string> ValidateAsync(Product entity)
        {

            if (entity == null)
            {
                return Task.FromResult("Product cannot be null.");
            }
            else if (string.IsNullOrEmpty(entity.Product_Name))
            {
                return Task.FromResult("Product name is required.");
            }
            else if (string.IsNullOrEmpty(entity.Product_Barcode))
            {
                return Task.FromResult("Product barcode is required.");
            }
            else if (entity.Product_Selling_Price < 0 || entity.Product_Cost >= entity.Product_Selling_Price)
            {
                return Task.FromResult("Product selling price cannot be 0 or less than product cost.");
            }
            else
            {
                return Task.FromResult(string.Empty);
            }
        }

        public async Task BulkUpdateAsync(List<Product> productsToUpdate)
        {

            await context.BulkUpdateAsync(productsToUpdate).ContinueWith(task =>
             {
                 if (task.IsFaulted)
                 {
                     throw task.Exception ?? new Exception("An error occurred during bulk update.");
                 }
             });
        }

        public async Task<List<Product>> AddRangeAsync(List<Product> productsToAdd)
        {
            await context.Products.AddRangeAsync(productsToAdd);
            await context.SaveChangesAsync();
            return productsToAdd;
        }
    }
}
