using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services
{
    public class ProductLookUpServices
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        private readonly DatabaseInitialization _dbContext;
        public ProductLookUpServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
            _dbContext = _dbFactory.CreateDbContext();
        }

        private readonly Func<DatabaseInitialization, string, Task<Product?>> _compiledNoTrackingIncludes =
            EF.CompileAsyncQuery((DatabaseInitialization ctx, string barcode) =>
                ctx.Products
                    .AsNoTracking()
                    .Include(p => p.Department)
                    .Include(p => p.Promotion)
                    .Include(p => p.VAT)
                    .FirstOrDefault(p => p.Product_Barcode == barcode));

        private static readonly Func<DatabaseInitialization, string, Task<Product?>> _compiledNoTracking =
          EF.CompileAsyncQuery((DatabaseInitialization ctx, string barcode) =>
              ctx.Products
                  .AsNoTracking()
                  .FirstOrDefault(p => p.Product_Barcode == barcode));
        public async Task<Product> GetProductByBarcode(string barcode, bool includeMapping)
        {
            if (includeMapping)
            {
                return await _compiledNoTrackingIncludes(_dbContext, barcode);
            }
            return await _compiledNoTracking(_dbContext, barcode);
        }

        public async Task<Product> GetFirstOrDefaultProduct()
        {
            return await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync();
        }
    }
}
