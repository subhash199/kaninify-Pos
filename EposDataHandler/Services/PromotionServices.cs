using EntityFrameworkDatabaseLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using DataHandlerLibrary.Models;
using System.Linq.Expressions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Services
{
    public class PromotionServices : IGenericService<Promotion>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        
        public PromotionServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
             _dbFactory = dbFactory;
        }

        // Implementation of IGenericService<Promotion>
        public async Task<IEnumerable<Promotion>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Promotions.AsNoTracking()
                    .Include(p => p.Created_By)
                    .Include(p => p.Last_Modified_By)
                    .Include(p => p.Site)
                    .Include(p => p.Till)
                    .Include(p => p.Products)
                    .Where(p => !p.Is_Deleted)
                    .OrderByDescending(p => p.Created_Date)
                    .ToListAsync();
            }
            return await context.Promotions.AsNoTracking()              
                .ToListAsync();
        }

        public async Task<Promotion> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Promotions.AsNoTracking()
               .Include(p => p.Products)
                .FirstOrDefaultAsync(p => p.Id == id && !p.Is_Deleted);
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var currentDate = DateTime.UtcNow;
            return await context.Promotions.AsNoTracking()
                .Where(p => !p.Is_Deleted && 
                           p.Is_Active &&
                           p.Start_Date <= currentDate && 
                           p.End_Date >= currentDate)
                .OrderBy(p => p.Promotion_Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsByTypeAsync(PromotionType promotionType)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Promotions.AsNoTracking()
                .Where(p => !p.Is_Deleted && p.Promotion_Type == promotionType)
                .OrderByDescending(p => p.Created_Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsByProductAsync(int productId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            // With the new direct relationship, get the promotion directly from the product
            var product = await context.Products.AsNoTracking()
                .Include(p => p.Promotion)
                .FirstOrDefaultAsync(p => p.Id == productId && !p.Is_Deleted);
            
            if (product?.Promotion != null && !product.Promotion.Is_Deleted)
            {
                return new List<Promotion> { product.Promotion };
            }
            
            return new List<Promotion>();
        }

        public async Task AddAsync(Promotion entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.End_Date = entity.End_Date.ToUniversalTime();
            entity.Start_Date = entity.Start_Date.ToUniversalTime();
            entity.Created_Date = DateTime.UtcNow;
            entity.Last_Modified = DateTime.UtcNow;
            entity.Is_Deleted = false;
            
            context.Promotions.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Promotion entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            entity.Last_Modified = DateTime.UtcNow;
            context.Promotions.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var promotion = await context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                promotion.Is_Deleted = true;
                promotion.Last_Modified = DateTime.UtcNow;
                
                // Remove promotion reference from all products that use this promotion
                var productsWithPromotion = await context.Products
                    .Where(p => p.Promotion_Id == id)
                    .ToListAsync();
                
                foreach (var product in productsWithPromotion)
                {
                    product.Promotion_Id = null;
                    product.Last_Modified = DateTime.UtcNow;
                }
                
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Promotion>> GetByConditionAsync(Expression<Func<Promotion, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var query = context.Promotions.AsNoTracking().Where(expression);
            
            if (includeMapping)
            {
                query = query.Include(p => p.Created_By)
                    .Include(p => p.Last_Modified_By)
                    .Include(p => p.Site)
                    .Include(p => p.Till);
            }
            
            return await query.ToListAsync();
        }

        public async Task<string> ValidateAsync(Promotion entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var errors = new List<string>();

            // Required field validations
            if (string.IsNullOrWhiteSpace(entity.Promotion_Name))
                errors.Add("Promotion name is required.");

            if (entity.Start_Date == default)
                errors.Add("Start date is required.");

            if (entity.End_Date == default)
                errors.Add("End date is required.");

            // Business logic validations
            if (entity.End_Date <= entity.Start_Date)
                errors.Add("End date must be after start date.");

            // Check for duplicate promotion names (excluding current promotion if updating)
            var existingPromotion = await context.Promotions
                .FirstOrDefaultAsync(p => p.Promotion_Name.ToLower() == entity.Promotion_Name.ToLower() && 
                                         p.Id != entity.Id && 
                                         !p.Is_Deleted);
            
            if (existingPromotion != null)
                errors.Add("A promotion with this name already exists.");

            // Validate promotion type specific fields
            switch (entity.Promotion_Type)
            {
                case PromotionType.Discount:
                    if (entity.Discount_Percentage <= 0 && entity.Discount_Amount <= 0)
                        errors.Add("Discount percentage or amount must be greater than 0 for discount promotions.");
                    if (entity.Discount_Percentage > 100)
                        errors.Add("Discount percentage cannot exceed 100%.");
                    break;
                    
                case PromotionType.BuyXGetXFree:
                    if (entity.Buy_Quantity <= 0)
                        errors.Add("Buy quantity must be greater than 0 for Buy X Get X Free promotions.");
                    if (entity.Free_Quantity <= 0)
                        errors.Add("Free quantity must be greater than 0 for Buy X Get X Free promotions.");
                    break;
                    
                case PromotionType.MultiBuy:
                    if (entity.Buy_Quantity <= 1)
                        errors.Add("Buy quantity must be greater than 1 for MultiBuy promotions.");
                    if (entity.Discount_Percentage <= 0 && entity.Discount_Amount <= 0)
                        errors.Add("Discount percentage or amount must be greater than 0 for MultiBuy promotions.");
                    if (entity.Discount_Percentage > 100)
                        errors.Add("Discount percentage cannot exceed 100%.");
                    break;
                    
                case PromotionType.BundleBuy:
                    if (entity.Discount_Percentage <= 0 && entity.Discount_Amount <= 0)
                        errors.Add("Discount percentage or amount must be greater than 0 for Bundle promotions.");
                    if (entity.Discount_Percentage > 100)
                        errors.Add("Discount percentage cannot exceed 100%.");
                    // Note: Bundle products validation should be done separately as it requires database access
                    break;
            }

            return errors.Any() ? string.Join("; ", errors) : string.Empty;
        }

        // Additional methods for managing product-promotion relationships
        public async Task AssignPromotionToProductAsync(int promotionId, int productId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var product = await context.Products.FindAsync(productId);
            if (product != null && !product.Is_Deleted)
            {
                product.Promotion_Id = promotionId;
                product.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task RemovePromotionFromProductAsync(int productId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var product = await context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Promotion_Id = null;
                product.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetProductsWithPromotionAsync(int promotionId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Products.AsNoTracking()
                .Include(p => p.Department)
                .Include(p => p.VAT)
                .Where(p => p.Promotion_Id == promotionId && !p.Is_Deleted)
                .OrderBy(p => p.Product_Name)
                .ToListAsync();
        }
    }
}