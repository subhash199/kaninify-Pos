
using DataHandlerLibrary.Interfaces;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services
{
    public class VatServices : IGenericService<Vat>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        public VatServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Vat> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Vats.AsNoTracking()
                 .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task UpdateAsync(Vat entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            context.Vats.Update(entity);
            await context.SaveChangesAsync();
        }
        public async Task AddAsync(Vat entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            context.Vats.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var vat = await context.Vats.FindAsync(id);
            if (vat != null)
            {
                context.Vats.Remove(vat);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Vat>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Vats.AsNoTracking()
                    .ToListAsync();
            }
            return await context.Vats.AsNoTracking()
                .ToListAsync();
        }

        public Task<IEnumerable<Vat>> GetByConditionAsync(Expression<Func<Vat, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return Task.FromResult(context.Vats.AsNoTracking()
                    .Where(expression).AsEnumerable());
            }
            else
            {
                return Task.FromResult(context.Vats.AsNoTracking()
                    .Where(expression).AsEnumerable());
            }

        }

        public Task<string> ValidateAsync(Vat entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Vat> GetDefaultVatAsync()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            Vat defaultVat = await context.Vats
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VAT_Value == 0);
            if (defaultVat == null)
            {
                defaultVat = new Vat
                {
                    VAT_Name = "Zero VAT",
                    VAT_Value = 0,
                    VAT_Description = "Zero VAT",
                    Date_Created = DateTime.UtcNow,
                    Last_Modified = DateTime.UtcNow
                };
                await AddAsync(defaultVat);
            }
            return defaultVat;

        }

        public async Task<List<Vat>> AddRangeAsync(List<Vat> entities)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            await context.Vats.AddRangeAsync(entities);
            await context.SaveChangesAsync();
            return entities;
        }
    }

}