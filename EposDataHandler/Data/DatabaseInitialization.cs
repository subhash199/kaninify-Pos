using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkDatabaseLibrary.Data
{
    public class DatabaseInitialization : DbContext
    {
        public DbSet<Site> Sites { get; set; }
        public DbSet<DayLog> DayLogs { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Payout> Payouts { get; set; }
        public DbSet<PosUser> PosUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<SalesItemTransaction> SalesItemTransactions { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierItem> SupplierItems { get; set; }
        public DbSet<Till> Tills { get; set; }
        public DbSet<Vat> Vats { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<UserSiteAccess> UserSiteAccesses { get; set; }
        public DbSet<VoidedProduct> VoidedProducts { get; set; }
        public DbSet<ReceiptPrinter> ReceiptPrinters { get; set; }
        public DbSet<DrawerLog> DrawerLogs { get; set; }
        public DbSet<StockRefill> StockRefills { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Retailer> Retailers { get; set; }
        public DbSet<UnknownProduct> UnknownProducts { get; set; }
        public DbSet<UnSyncedLog> UnSyncedLogs { get; set; }
        public DbSet<SyncedLog> SyncedLogs { get; set; }
        public DbSet<TableTracker> TableTrackers { get; set; }
        public DbSet<DeliveryInvoice> DeliveryInvoices { get; set; }
        public DbSet<DeliveryItem> DeliveryItems { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherProductExclusion> VoucherProductExclusions { get; set; }
        public DbSet<VoucherDepartmentExclusion> VoucherDepartmentExclusions { get; set; }
        string ConnectionString { get; set; }



        public DatabaseInitialization(string decryptedString)
        {
            //"Data Source=(localdb)\\MSSqlLocalDB;Initial " +
            //    "Catalog=RetailEposMaui;Integrated Security=True;Pooling=true;Min Pool Size=10;Max Pool Size=100;Connection Timeout=30;";
            ConnectionString = decryptedString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // indexes

            modelBuilder.Entity<UnSyncedLog>()
            .HasIndex(usl => usl.SyncStatus);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Product_Barcode)
                .IsUnique();


            // end of indexes

            modelBuilder.Entity<DeliveryInvoice>()
           .HasOne(di => di.DayLog)
           .WithMany()
           .HasForeignKey(di => di.DayLogId)
           .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeliveryInvoice>()
           .HasOne(di => di.Shift)
           .WithMany()
           .HasForeignKey(di => di.Shift_Id)
           .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeliveryInvoice>()
                .HasOne(di => di.Created_By)
                .WithMany()
                .HasForeignKey(di => di.Created_By_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeliveryInvoice>()
                .HasOne(di => di.Last_Modified_By)
                .WithMany()
                .HasForeignKey(di => di.Last_Modified_By_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeliveryInvoice>()
                .HasOne(di => di.Till)
                .WithMany()
                .HasForeignKey(di => di.Till_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeliveryInvoice>()
                .HasOne(di => di.Site)
                .WithMany()
                .HasForeignKey(di => di.Site_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeliveryInvoice>()
                  .HasMany(di => di.Items)
                  .WithOne(i => i.DeliveryInvoice)
                  .HasForeignKey(i => i.DeliveryInvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeliveryInvoice>()
                .HasOne(di => di.Supplier)
                .WithMany()
                .HasForeignKey(di => di.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeliveryItem>()
                .HasOne(di => di.DeliveryInvoice)
                .WithMany()
                .HasForeignKey(di => di.DeliveryInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeliveryItem>()
               .HasOne(di => di.Product)
               .WithMany()
               .HasForeignKey(di => di.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeliveryItem>()
               .HasOne(di => di.SupplierItem)
               .WithMany()
               .HasForeignKey(di => di.SupplierItemId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Voucher>()
             .HasOne(v => v.Created_By)
             .WithMany()
             .HasForeignKey(v => v.Created_By_Id)
             .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Voucher>()
             .HasOne(v => v.Last_Modified_By)
             .WithMany()
             .HasForeignKey(v => v.Last_Modified_By_Id)
             .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Voucher>()
             .HasOne(v => v.Site)
             .WithMany()
             .HasForeignKey(v => v.Site_Id)
             .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Voucher>()
             .HasOne(v => v.Till)
             .WithMany()
             .HasForeignKey(v => v.Till_Id)
             .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Voucher>()
             .HasMany(v => v.ExcludedDepartments)
             .WithOne(vd => vd.Voucher)
             .HasForeignKey(vd => vd.VoucherId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Voucher>()
             .HasMany(v => v.ExcludedProducts)
             .WithOne(vp => vp.Voucher)
             .HasForeignKey(vp => vp.VoucherId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherDepartmentExclusion>()
                .HasOne(vd => vd.Voucher)
                .WithMany()
                .HasForeignKey(vd => vd.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherDepartmentExclusion>()
                .HasOne(vd => vd.Department)
                .WithMany()
                .HasForeignKey(vd => vd.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherProductExclusion>()
                .HasOne(vp => vp.Voucher)
                .WithMany()
                .HasForeignKey(vp => vp.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherProductExclusion>()
                .HasOne(vp => vp.Product)
                .WithMany()
                .HasForeignKey(vp => vp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            

            // Configure PosUser audit fields - ignore navigation properties without foreign keys
            modelBuilder.Entity<PosUser>()
                .Ignore(u => u.Created_By)
                .Ignore(u => u.Last_Modified_By);


            // Configure PosUser -> UserSiteAccess relationship
            modelBuilder.Entity<PosUser>()
                .HasMany(u => u.SiteAccesses)
                .WithOne(usa => usa.User)
                .HasForeignKey(usa => usa.User_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Site -> UserSiteAccess relationship
            modelBuilder.Entity<Site>()
                .HasMany(s => s.UserAccesses)
                .WithOne(usa => usa.Site)
                .HasForeignKey(usa => usa.Site_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserSiteAccess audit fields - ignore navigation properties without foreign keys
            modelBuilder.Entity<UserSiteAccess>()
                .Ignore(usa => usa.Created_By)
                .Ignore(usa => usa.Last_Modified_By);

            // Configure PosUser -> Site (Primary Site) relationship
            modelBuilder.Entity<PosUser>()
                .HasOne(u => u.PrimarySite)
                .WithMany(s => s.PrimaryUsers)
                .HasForeignKey(u => u.Primary_Site_Id)
                .OnDelete(DeleteBehavior.NoAction); // Changed from SetNull to NoAction

            // Configure Till -> Site relationship
            modelBuilder.Entity<Till>()
                .HasOne(t => t.Site)
                .WithMany(s => s.Tills)
                .HasForeignKey(t => t.Site_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Till audit fields - ignore navigation properties without foreign keys
            modelBuilder.Entity<Till>()
                .Ignore(t => t.Created_By)
                .Ignore(t => t.Last_Modified_By);

            // Existing Shift configurations
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Till)
                .WithMany()
                .HasForeignKey(s => s.Till_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Site)
                .WithMany()
                .HasForeignKey(s => s.Site_Id)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Shift>()
                .HasOne(s => s.DayLog)
                .WithMany()
                .HasForeignKey(s => s.DayLog_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SalesTransaction navigation properties
            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.Till)
                .WithMany()
                .HasForeignKey(st => st.Till_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.DayLog)
                .WithMany()
                .HasForeignKey(st => st.DayLog_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.Shift)
                .WithMany()
                .HasForeignKey(st => st.Shift_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.Created_By)
                .WithMany()
                .HasForeignKey(st => st.Created_By_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.Last_Modified_By)
                .WithMany()
                .HasForeignKey(st => st.Last_Modified_By_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.Site)
                .WithMany()
                .HasForeignKey(st => st.Site_Id)
                .OnDelete(DeleteBehavior.SetNull);

            // StockTransaction foreign key configurations
            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.Till)
                .WithMany()
                .HasForeignKey(st => st.Till_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.From_Site)
                .WithMany()
                .HasForeignKey(st => st.From_Site_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.To_Site)
                .WithMany()
                .HasForeignKey(st => st.To_Site_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.Product)
                .WithMany()
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.DayLog)
                .WithMany()
                .HasForeignKey(st => st.DayLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure audit trail relationships
            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.Created_By)
                .WithMany()
                .HasForeignKey(st => st.Created_By_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.Last_Modified_By)
                .WithMany()
                .HasForeignKey(st => st.Last_Modified_By_Id)
                .OnDelete(DeleteBehavior.NoAction);
            // Configure PosUser -> Site (regular Site) relationship
            modelBuilder.Entity<PosUser>()
                .HasOne(u => u.Site)
                .WithMany()
                .HasForeignKey(u => u.Site_Id)
                .OnDelete(DeleteBehavior.NoAction); // Changed from SetNull to NoAction

            // Configure PosUser -> Shift relationship
            modelBuilder.Entity<PosUser>()
                .HasMany(u => u.Shifts)
                .WithOne(s => s.PosUser)
                .HasForeignKey(s => s.PosUser_Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure PosUser -> Till relationship
            modelBuilder.Entity<PosUser>()
                .HasOne(u => u.Till)
                .WithMany()
                .HasForeignKey(u => u.Till_Id)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Site audit trail relationships
            modelBuilder.Entity<Site>()
                .HasOne(s => s.Created_By)
                .WithMany()
                .HasForeignKey(s => s.Created_By_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Site>()
                .HasOne(s => s.Last_Modified_By)
                .WithMany()
                .HasForeignKey(s => s.Last_Modified_By_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesTransaction>()
                .HasMany(st => st.SalesItemTransactions)
                .WithOne(sit => sit.SalesTransaction)
                .HasForeignKey(sit => sit.SaleTransaction_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SalesItemTransaction navigation properties
            modelBuilder.Entity<SalesItemTransaction>()
                .HasOne(sit => sit.Product)
                .WithMany()
                .HasForeignKey(sit => sit.Product_ID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesItemTransaction>()
                .HasOne(sit => sit.SalesPayout)
                .WithMany()
                .HasForeignKey(sit => sit.SalesPayout_ID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesItemTransaction>()
                .HasOne(sit => sit.Promotion)
                .WithMany()
                .HasForeignKey(sit => sit.Promotion_ID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesItemTransaction>()
                .HasOne(sit => sit.Created_By)
                .WithMany()
                .HasForeignKey(sit => sit.Created_By_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesItemTransaction>()
                .HasOne(sit => sit.Last_Modified_By)
                .WithMany()
                .HasForeignKey(sit => sit.Last_Modified_By_Id)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Product -> Promotion relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Promotion)
                .WithMany(pr => pr.Products)
                .HasForeignKey(p => p.Promotion_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DrawerLog>()
                .HasOne(dl => dl.OpenedBy)
                .WithMany()
                .HasForeignKey(dl => dl.OpenedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DrawerLog>()
                .HasOne(dl => dl.Created_By)
                .WithMany()
                .HasForeignKey(dl => dl.Created_By_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DrawerLog>()
                .HasOne(dl => dl.Last_Modified_By)
                .WithMany()
                .HasForeignKey(dl => dl.Last_Modified_By_Id)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<DrawerLog>()
                .HasOne(dl => dl.Site)
                .WithMany()
                .HasForeignKey(dl => dl.Site_Id)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<DrawerLog>()
                .HasOne(dl => dl.Till)
                .WithMany()
                .HasForeignKey(dl => dl.Till_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StockRefill>()
                .HasOne(sr => sr.SalesItemTransaction)
                .WithMany()
                .HasForeignKey(sr => sr.SaleTransaction_Item_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockRefill>()
                .HasOne(sr => sr.Refilled_By_User)
                .WithMany()
                .HasForeignKey(sr => sr.Refilled_By)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<StockRefill>()
                .HasOne(sr => sr.Created_By_User)
                .WithMany()
                .HasForeignKey(sr => sr.Created_By_ID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<StockRefill>()
                .HasOne(sr => sr.Last_Modified_By_User)
                .WithMany()
                .HasForeignKey(sr => sr.Last_Modified_By_ID)
                .OnDelete(DeleteBehavior.SetNull); // Allow setting to null

            modelBuilder.Entity<StockRefill>()
                .HasOne(sr => sr.Shift)
                .WithMany()
                .HasForeignKey(sr => sr.Shift_ID)
                .OnDelete(DeleteBehavior.Cascade); // Prevent cascade delete

            modelBuilder.Entity<StockRefill>()
                .HasOne(sr => sr.DayLog)
                .WithMany()
                .HasForeignKey(sr => sr.DayLog_ID)
                .OnDelete(DeleteBehavior.Cascade); // Prevent cascade delete

            // Create index on Stock_Refilled field for better query performance
            modelBuilder.Entity<StockRefill>()
                .HasIndex(sr => sr.Stock_Refilled)
                .HasDatabaseName("IX_StockRefill_Stock_Refilled");

            // Configure ErrorLog relationships
            modelBuilder.Entity<ErrorLog>()
                .HasOne(el => el.User)
                .WithMany()
                .HasForeignKey(el => el.User_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ErrorLog>()
                .HasOne(el => el.Site)
                .WithMany()
                .HasForeignKey(el => el.Site_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ErrorLog>()
                .HasOne(el => el.Till)
                .WithMany()
                .HasForeignKey(el => el.Till_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ErrorLog>()
                .HasOne(el => el.Resolved_By)
                .WithMany()
                .HasForeignKey(el => el.Resolved_By_Id)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure UnknownProduct relationships
            modelBuilder.Entity<UnknownProduct>()
                .HasIndex(up => up.ProductBarcode)
                .IsUnique();
            modelBuilder.Entity<UnknownProduct>()
                .HasOne(up => up.Site)
                .WithMany()
                .HasForeignKey(up => up.SiteId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<UnknownProduct>()
                .HasOne(up => up.Till)
                .WithMany()
                .HasForeignKey(up => up.TillId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<UnknownProduct>()
                .HasOne(up => up.CreatedBy)
                .WithMany()
                .HasForeignKey(up => up.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<UnknownProduct>()
                .HasOne(up => up.Daylog)
                .WithMany()
                .HasForeignKey(up => up.DaylogId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<UnknownProduct>()
                .HasOne(up => up.Shift)
                .WithMany()
                .HasForeignKey(up => up.ShiftId)
                .OnDelete(DeleteBehavior.SetNull);

            // Create indexes for better query performance
            modelBuilder.Entity<ErrorLog>()
                .HasIndex(el => el.Error_DateTime)
                .HasDatabaseName("IX_ErrorLog_Error_DateTime");

            modelBuilder.Entity<ErrorLog>()
                .HasIndex(el => el.Severity_Level)
                .HasDatabaseName("IX_ErrorLog_Severity_Level");

            modelBuilder.Entity<ErrorLog>()
                .HasIndex(el => el.Is_Resolved)
                .HasDatabaseName("IX_ErrorLog_Is_Resolved");
            modelBuilder.Entity<UnknownProduct>()
                .HasIndex(up => up.ProductBarcode)
                .HasDatabaseName("IX_UnknownProduct_ProductBarcode");

            // Index Status field for quick lookups
            modelBuilder.Entity<DayLog>()
                .HasIndex(up => up.SyncStatus)
                .HasDatabaseName("IX_DayLog_Status");
            modelBuilder.Entity<DrawerLog>()
                .HasIndex(dl => dl.SyncStatus)
                .HasDatabaseName("IX_DrawerLog_SyncStatus");
            modelBuilder.Entity<ErrorLog>()
                .HasIndex(el => el.SyncStatus)
                .HasDatabaseName("IX_ErrorLog_SyncStatus");
            modelBuilder.Entity<Payout>()
                .HasIndex(p => p.SyncStatus)
                .HasDatabaseName("IX_Payout_SyncStatus");
            modelBuilder.Entity<PosUser>()
                .HasIndex(u => u.SyncStatus)
                .HasDatabaseName("IX_PosUser_SyncStatus");
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SyncStatus)
                .HasDatabaseName("IX_Product_SyncStatus");
            modelBuilder.Entity<Promotion>()
                .HasIndex(p => p.SyncStatus)
                .HasDatabaseName("IX_Promotion_SyncStatus");
            modelBuilder.Entity<ReceiptPrinter>()
                .HasIndex(rp => rp.SyncStatus)
                .HasDatabaseName("IX_ReceiptPrinter_SyncStatus");
            modelBuilder.Entity<SalesItemTransaction>()
                .HasIndex(sit => sit.SyncStatus)
                .HasDatabaseName("IX_SalesItemTransaction_SyncStatus");
            modelBuilder.Entity<SalesTransaction>()
                .HasIndex(st => st.SyncStatus)
                .HasDatabaseName("IX_SalesTransaction_SyncStatus");
            modelBuilder.Entity<Shift>()
                .HasIndex(s => s.SyncStatus)
                .HasDatabaseName("IX_Shift_SyncStatus");
            modelBuilder.Entity<Site>()
                .HasIndex(s => s.SyncStatus)
                .HasDatabaseName("IX_Site_SyncStatus");
            modelBuilder.Entity<StockRefill>()
                .HasIndex(sr => sr.SyncStatus)
                .HasDatabaseName("IX_StockRefill_SyncStatus");
            modelBuilder.Entity<StockTransaction>()
                .HasIndex(st => st.SyncStatus)
                .HasDatabaseName("IX_StockTransaction_SyncStatus");
            modelBuilder.Entity<Supplier>()
                .HasIndex(s => s.SyncStatus)
                .HasDatabaseName("IX_Supplier_SyncStatus");
            modelBuilder.Entity<SupplierItem>()
                .HasIndex(si => si.SyncStatus)
                .HasDatabaseName("IX_SupplierItems_SyncStatus");
            modelBuilder.Entity<Till>()
                .HasIndex(t => t.SyncStatus)
                .HasDatabaseName("IX_Till_SyncStatus");
            modelBuilder.Entity<UnknownProduct>()
                .HasIndex(up => up.SyncStatus)
                .HasDatabaseName("IX_UnknownProduct_SyncStatus");
            modelBuilder.Entity<UserSiteAccess>()
                .HasIndex(usa => usa.SyncStatus)
                .HasDatabaseName("IX_UserSiteAccess_SyncStatus");
            modelBuilder.Entity<VoidedProduct>()
                .HasIndex(vp => vp.SyncStatus)
                .HasDatabaseName("IX_VoidedProduct_SyncStatus");


            // Sync status to string
            modelBuilder.Entity<DayLog>()
                .Property(e => e.SyncStatus)
                .HasConversion<string>();

            modelBuilder.Entity<DrawerLog>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<ErrorLog>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();
            modelBuilder.Entity<Payout>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<PosUser>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<Product>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<Promotion>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<ReceiptPrinter>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<SalesItemTransaction>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();


            modelBuilder.Entity<SalesTransaction>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<Shift>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<Site>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<StockRefill>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<StockTransaction>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<Supplier>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<SupplierItem>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<SyncedLog>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<Till>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<UnknownProduct>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<UnSyncedLog>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<UserSiteAccess>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

            modelBuilder.Entity<VoidedProduct>()
               .Property(e => e.SyncStatus)
               .HasConversion<string>();

        }
    }
}



