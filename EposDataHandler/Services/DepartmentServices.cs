using DataHandlerLibrary.Interfaces;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services
{
    public class DepartmentServices : IGenericService<Department>
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        public DepartmentServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
             _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Department>> GetAllAsync(bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Departments
                    .AsNoTracking()
                    .Include(d => d.Products)
                    .Include(d => d.Created_By)
                    .Include(d => d.Last_Modified_By)
                    .ToListAsync();
            }
            else
            {
                // Return all departments without tracking
                return await context.Departments.AsNoTracking().ToListAsync();
            }

        }

        public async Task<Department> GetDepartmentByName(string departmentName)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Department_Name == departmentName);
        }

        public async Task<Department> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Departments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Department entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            context.Departments.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department entity)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            context.Departments.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task<Department> AddGenericDepartment(string departmentName, int userId, int siteId, int tillId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            Department department = new Department
            {
                Department_Name = departmentName,
                Department_Description = departmentName,
                Age_Restricted = false,
                Separate_Sales_In_Reports = false,
                Stock_Refill_Print = true,
                Is_Activated = true,
                Is_Deleted = false,
                Allow_Staff_Discount = false,
                Date_Created = DateTime.UtcNow,
                Last_Modified = DateTime.UtcNow,
                Created_By_Id = userId,
                Last_Modified_By_Id = userId,
                Site_Id = siteId,
                Till_Id = tillId

            };
            await context.Departments.AddAsync(department);
            return department;

        }

        public async Task DeleteAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            context.Departments.Remove(new Department { Id = id });
            context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Department>> GetByConditionAsync(Expression<Func<Department, bool>> expression, bool includeMapping)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (includeMapping)
            {
                return await context.Departments
                     .AsNoTracking()
                     .Include(p => p.Products)
                     .Where(expression)
                     .ToListAsync();
            }

            return await context.Departments
                .AsNoTracking()
                .Where(expression)
                .ToListAsync();

        }

        public Task<string> ValidateAsync(Department entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Department> GetDefaultDepartment()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            Department department = await context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Department_Name == "Default");
            if (department == null)
            {
                await AddAsync(new Department
                {
                    Department_Name = "Default",
                    Department_Description = "Default Department",
                    Age_Restricted = false,
                    Separate_Sales_In_Reports = true,
                    Stock_Refill_Print = true,
                    Date_Created = DateTime.UtcNow,
                    Last_Modified = DateTime.UtcNow
                });
            }
            return await context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Department_Name == "Default");
        }

        public async Task<List<Department>> AddRangeAsync(List<Department> departments)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
            return departments;
        }
    }
}
