using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DataHandlerLibrary.Services
{
    public class MigrateDataServices
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        private GeneralServices _generalServices;
        private List<Department> _departments;
        private List<Vat> _vats;
        public MigrateDataServices(IDbContextFactory<DatabaseInitialization> dbFactory, GeneralServices generalServices)
        {
            _dbFactory = dbFactory;
            _generalServices = generalServices;
        }

        private void InitializeData()
        {
            using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
            _departments = _context.Departments.ToList();
            _vats = _context.Vats.ToList();
        }
        public async Task<int> InsertDepartmentsFromCsvAsync(string csvFilePath, int? createdByUserId, int? siteId = null, int? tillId = null)
        {
            try
            {
                var departments = new List<Department>();
                var lines = await File.ReadAllLinesAsync(csvFilePath);
                
                // Skip header row if exists, process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    if (values.Length < 11) continue; // Skip invalid rows

                    var department = new Department
                    {
                        Department_Name = _generalServices.CapitalizeWords(values[1]?.Trim()) ?? string.Empty,
                        Department_Description = _generalServices.CapitalizeWords(values[2]?.Trim()),
                        Age_Restricted = values[3] == "1",
                        Separate_Sales_In_Reports = values[4] == "1",
                        Allow_Staff_Discount = values[6] == "1",
                        Is_Activated = values[7] == "1",
                        Is_Deleted = values[8] == "1",
                        Date_Created = DateTime.UtcNow,
                        Last_Modified = DateTime.UtcNow,
                        Created_By_Id = createdByUserId,
                        Last_Modified_By_Id = createdByUserId,
                        Site_Id = siteId,
                        Till_Id = tillId
                    };

                    departments.Add(department);
                }
                using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
                await _context.Departments.AddRangeAsync(departments);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing departments from CSV: {ex.Message}", ex);
            }
        }

        public async Task<int> InsertVatsFromCsvAsync(string csvFilePath, int? createdByUserId, int? siteId = null, int? tillId = null)
        {
            try
            {
                var vats = new List<Vat>();
                var lines = await File.ReadAllLinesAsync(csvFilePath);
                
                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    if (values.Length < 6) continue; // Skip invalid rows

                    var vat = new Vat
                    {
                        VAT_Name = _generalServices.CapitalizeWords(values[1]?.Trim()) ?? string.Empty,
                        VAT_Value = decimal.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal vatValue) ? vatValue : 0m,
                        VAT_Description = _generalServices.CapitalizeWords(values[3]?.Trim()),
                        Date_Created = DateTime.UtcNow,
                        Last_Modified = DateTime.UtcNow,
                        Created_By_Id = createdByUserId,
                        Last_Modified_By_Id = createdByUserId,
                        Site_Id = siteId,
                        Till_Id = tillId
                    };

                    vats.Add(vat);
                }
                using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
                await _context.Vats.AddRangeAsync(vats);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing VATs from CSV: {ex.Message}", ex);
            }
        }

        public async Task<int> InsertProductsFromCsvAsync(string csvFilePath, int? createdByUserId, int? siteId = null, int? tillId = null)
        {
            try
            {
                var products = new List<Product>();
                var lines = await File.ReadAllLinesAsync(csvFilePath);
                
                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    if (values.Length < 27) continue; // Skip invalid rows (minimum required fields)
                    
                    var product = new Product
                    {
                        Product_Name = _generalServices.CapitalizeWords(values[1]?.Trim()) ?? string.Empty,
                        Product_Description = _generalServices.CapitalizeWords(values[2]?.Trim()),
                        Product_Barcode = values[3]?.Trim() ?? string.Empty,
                        Product_Case_Barcode = string.IsNullOrEmpty(values[4]?.Trim()) ? null : values[4]?.Trim(),
                        ShelfQuantity = 0,
                        StockroomQuantity = int.TryParse(values[5], out int stockQty) ? stockQty : 0,
                        Department_ID = int.TryParse(values[6], out int deptId) ? deptId : 1,
                        VAT_ID = int.TryParse(values[7], out int vatId) ? vatId : 1,
                        Product_Cost = decimal.TryParse(values[8], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cost) ? cost : 0m,
                        Product_Selling_Price = decimal.TryParse(values[9], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) ? price : 0m,
                        Profit_On_Return_Percentage = decimal.TryParse(values[10], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal profit) ? profit : 0m,
                        Product_Size = values[11] != "NULL" && int.TryParse(values[11], out int size) ? size : null,
                        Product_Measurement = values[12] == "NULL" ? null : values[12]?.Trim(),
                        Brand_Name = values[13] == "NULL" ? null : _generalServices.CapitalizeWords(values[13]?.Trim()),
                        Product_Min_Order = int.TryParse(values[14], out int minOrder) ? minOrder : 0,
                        Product_Low_Stock_Alert_QTY = int.TryParse(values[15], out int lowStock) ? lowStock : 0,
                        Product_Min_Stock_Level = int.TryParse(values[16], out int minStock) ? minStock : 0,
                        Product_Unit_Per_Case = int.TryParse(values[17], out int unitsPerCase) ? unitsPerCase : 0,
                        Product_Cost_Per_Case = decimal.TryParse(values[18], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costPerCase) ? costPerCase : 0m,
                        Is_Activated = values[21] == "1",
                        Is_Deleted = values[22] == "1",
                        Priced_Changed_On = DateTime.UtcNow,
                        Date_Created = DateTime.UtcNow,
                        Last_Modified = DateTime.UtcNow,
                        Created_By_Id = createdByUserId,
                        Last_Modified_By_Id = createdByUserId,
                        Site_Id = siteId,
                        Till_Id = tillId
                    };

                    DateTime expiry;
                    if (DateTime.TryParse(values[19], out expiry))
                    {
                        product.Expiry_Date = expiry.Date < DateTime.UtcNow.AddDays(-100).Date
                            ? DateTime.UtcNow
                            : expiry.ToUniversalTime();
                    }
                    else
                    {
                        product.Expiry_Date = DateTime.UtcNow.AddYears(100); // Or use _generalServices.GetDefaultExpiryDate();
                    }

                    if (!products.Exists(p => p.Product_Barcode == product.Product_Barcode))
                    {
                        if (_departments == null || _vats == null)
                        {
                            InitializeData();
                        }
                        product.Department_ID = _departments?.FirstOrDefault(d => d.Id == product.Department_ID)?.Id
                            ?? _departments?.FirstOrDefault(d => string.Equals(d.Department_Name, "Default", StringComparison.OrdinalIgnoreCase))?.Id
                            ?? 1;
                        product.VAT_ID = _vats?.FirstOrDefault(v => v.Id == product.VAT_ID)?.Id
                            ?? _vats?.FirstOrDefault(v => string.Equals(v.VAT_Name, "Zero Rate", StringComparison.OrdinalIgnoreCase))?.Id
                            ?? 1;
                        products.Add(product);
                    }

                }
                using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
                await _context.Products.AddRangeAsync(products);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing products from CSV: {ex.Message}", ex);
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentField = new System.Text.StringBuilder();

            for (int i = 1; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result.ToArray();
        }

        // Utility method to import all data in correct order
        public async Task<(int departments, int vats, int products)> ImportAllDataFromCsvAsync(
            int createdByUserId, 
            int? siteId = null, 
            int? tillId = null,
            string departmentsCsvPath = "EposRetailData/Departments.csv",
            string vatsCsvPath = "EposRetailData/Vats.csv",
            string productsCsvPath = "EposRetailData/Products.csv")
        {
            try
            {
                // Import in correct order (dependencies first)
                int departmentsCount = await InsertDepartmentsFromCsvAsync(departmentsCsvPath, createdByUserId, siteId, tillId);
                int vatsCount = await InsertVatsFromCsvAsync(vatsCsvPath, createdByUserId, siteId, tillId);
                int productsCount = await InsertProductsFromCsvAsync(productsCsvPath, createdByUserId, siteId, tillId);

                return (departmentsCount, vatsCount, productsCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing all data from CSV files: {ex.Message}", ex);
            }
        }

        // Utility method to clear existing data before import
        public async Task ClearAllDataAsync()
        {
            try
            {
                // Clear in reverse dependency order
                using var _context = _dbFactory.CreateDbContext(); // fresh DbContext
                _context.Products.RemoveRange(_context.Products);
                _context.Vats.RemoveRange(_context.Vats);
                _context.Departments.RemoveRange(_context.Departments);
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error clearing existing data: {ex.Message}", ex);
            }
        }
    }
}
