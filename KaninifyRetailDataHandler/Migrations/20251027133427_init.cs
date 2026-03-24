using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Retailers",
                columns: table => new
                {
                    RetailerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetailName = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    TokenExpiryAt = table.Column<int>(type: "integer", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    ApiUrl = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    FirstLine_Address = table.Column<string>(type: "text", nullable: false),
                    SecondLine_Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: false),
                    County = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Postcode = table.Column<string>(type: "text", nullable: false),
                    Vat_Number = table.Column<string>(type: "text", nullable: true),
                    Business_Registration_Number = table.Column<string>(type: "text", nullable: true),
                    Business_Type = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    Contact_Name = table.Column<string>(type: "text", nullable: true),
                    Contact_Position = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Contact_Number = table.Column<string>(type: "text", nullable: false),
                    Website_Url = table.Column<string>(type: "text", nullable: true),
                    Logo_Path = table.Column<string>(type: "text", nullable: true),
                    Business_Hours = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LicenseKey = table.Column<string>(type: "text", nullable: false),
                    LicenseType = table.Column<string>(type: "text", nullable: true),
                    LicenseIssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    MaxTills = table.Column<int>(type: "integer", nullable: false),
                    IsLicenseValid = table.Column<bool>(type: "boolean", nullable: false),
                    LastLicenseCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SecretKey = table.Column<string>(type: "text", nullable: true),
                    Last_Sign_In_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    SyncVersion = table.Column<int>(type: "integer", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retailers", x => x.RetailerId);
                });

            migrationBuilder.CreateTable(
                name: "SyncedLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: false),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    SyncLocation = table.Column<string>(type: "text", nullable: false),
                    SyncedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncedLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnSyncedLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: false),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SyncError = table.Column<string>(type: "text", nullable: true),
                    SyncRetryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnSyncedLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DayLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    DayLog_Start_DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DayLog_End_DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Opening_Cash_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Closing_Cash_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Cash_Variance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InvoiceVatTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalItemsDelivered = table.Column<int>(type: "integer", nullable: false),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    DayLogId = table.Column<int>(type: "integer", nullable: true),
                    Shift_Id = table.Column<int>(type: "integer", nullable: true),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_DayLogs_DayLogId",
                        column: x => x.DayLogId,
                        principalTable: "DayLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "integer", nullable: false),
                    CostPerCase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SupplierItemId = table.Column<int>(type: "integer", nullable: true),
                    ConfirmedByScanner = table.Column<bool>(type: "boolean", nullable: false),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    DeliveryInvoiceId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_DeliveryInvoices_DeliveryInvoiceId",
                        column: x => x.DeliveryInvoiceId,
                        principalTable: "DeliveryInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_DeliveryInvoices_DeliveryInvoiceId1",
                        column: x => x.DeliveryInvoiceId1,
                        principalTable: "DeliveryInvoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Department_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department_Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Age_Restricted = table.Column<bool>(type: "boolean", nullable: false),
                    Separate_Sales_In_Reports = table.Column<bool>(type: "boolean", nullable: false),
                    Stock_Refill_Print = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Activated = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Allow_Staff_Discount = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrawerLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    OpenedById = table.Column<int>(type: "integer", nullable: false),
                    DrawerOpenDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Shift_Id = table.Column<int>(type: "integer", nullable: true),
                    DayLog_Id = table.Column<int>(type: "integer", nullable: true),
                    DrawerLogType = table.Column<int>(type: "integer", nullable: false),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawerLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrawerLogs_DayLogs_DayLog_Id",
                        column: x => x.DayLog_Id,
                        principalTable: "DayLogs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Error_Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Error_Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Source_Method = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Source_Class = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Stack_Trace = table.Column<string>(type: "text", nullable: true),
                    Severity_Level = table.Column<int>(type: "integer", nullable: true),
                    User_Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Error_DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    User_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Application_Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Is_Resolved = table.Column<bool>(type: "boolean", nullable: false),
                    Resolved_DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resolved_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Resolution_Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Payout_Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PosUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    First_Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Last_Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Passcode = table.Column<int>(type: "integer", nullable: false),
                    User_Type = table.Column<int>(type: "integer", nullable: false),
                    Primary_Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Allowed_Void_Line = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Void_Sale = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_No_Sale = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Returns = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Payout = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Refund = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Change_Price = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Discount = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Override_Price = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Users = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Sites = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Tills = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Products = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Suppliers = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_StockTransfer = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Vat = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Departments = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Orders = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Reports = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Settings = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Tax_Rates = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_Promotions = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Manage_VoidedProducts = table.Column<bool>(type: "boolean", nullable: false),
                    Allowed_Day_End = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Activated = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Site_BusinessName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Site_AddressLine1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Site_City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Site_AddressLine2 = table.Column<string>(type: "text", nullable: true),
                    Site_County = table.Column<string>(type: "text", nullable: true),
                    Site_Country = table.Column<string>(type: "text", nullable: false),
                    Site_Postcode = table.Column<string>(type: "text", nullable: false),
                    Site_ContactNumber = table.Column<string>(type: "text", nullable: true),
                    Site_Email = table.Column<string>(type: "text", nullable: true),
                    Site_VatNumber = table.Column<string>(type: "text", nullable: true),
                    Is_Primary = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sites_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Till_Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Till_IP_Address = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Till_Port_Number = table.Column<int>(type: "integer", maxLength: 10, nullable: true),
                    Till_Password = table.Column<string>(type: "text", nullable: false),
                    Is_Primary = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tills_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Promotion_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Promotion_Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Buy_Quantity = table.Column<int>(type: "integer", nullable: false),
                    Free_Quantity = table.Column<int>(type: "integer", nullable: true),
                    Discount_Percentage = table.Column<decimal>(type: "numeric", nullable: true),
                    Discount_Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    Minimum_Spend_Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    Start_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Promotion_Type = table.Column<int>(type: "integer", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promotions_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Promotions_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Promotions_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Promotions_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceiptPrinters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Printer_Name = table.Column<string>(type: "text", nullable: false),
                    Printer_IP_Address = table.Column<string>(type: "text", nullable: true),
                    Printer_Port_Number = table.Column<int>(type: "integer", nullable: false),
                    Printer_Password = table.Column<string>(type: "text", nullable: true),
                    Paper_Width = table.Column<int>(type: "integer", nullable: false),
                    Print_Receipt = table.Column<bool>(type: "boolean", nullable: false),
                    Print_Label = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Is_Primary = table.Column<bool>(type: "boolean", nullable: false),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptPrinters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptPrinters_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptPrinters_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptPrinters_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptPrinters_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    DayLog_Id = table.Column<int>(type: "integer", nullable: false),
                    PosUser_Id = table.Column<int>(type: "integer", nullable: false),
                    Shift_Start_DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Shift_End_DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Opening_Cash_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Closing_Cash_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Expected_Cash_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Cash_Variance = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Shift_Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Closing_Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_DayLogs_DayLog_Id",
                        column: x => x.DayLog_Id,
                        principalTable: "DayLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shifts_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shifts_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shifts_PosUsers_PosUser_Id",
                        column: x => x.PosUser_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shifts_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shifts_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Supplier_Name = table.Column<string>(type: "text", nullable: false),
                    Supplier_Description = table.Column<string>(type: "text", nullable: true),
                    Supplier_Address = table.Column<string>(type: "text", nullable: true),
                    Supplier_Phone = table.Column<string>(type: "text", nullable: true),
                    Supplier_Mobile = table.Column<string>(type: "text", nullable: true),
                    Supplier_Email = table.Column<string>(type: "text", nullable: true),
                    Supplier_Website = table.Column<string>(type: "text", nullable: true),
                    Supplier_Credit_Limit = table.Column<decimal>(type: "numeric", nullable: true),
                    Is_Activated = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Suppliers_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Suppliers_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Suppliers_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TableTrackers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: false),
                    RecordName = table.Column<string>(type: "text", nullable: false),
                    OldRecord = table.Column<string>(type: "text", nullable: false),
                    NewRecord = table.Column<string>(type: "text", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    PosUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableTrackers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableTrackers_PosUsers_PosUserId",
                        column: x => x.PosUserId,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TableTrackers_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TableTrackers_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserSiteAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    User_Id = table.Column<int>(type: "integer", nullable: false),
                    Site_Id = table.Column<int>(type: "integer", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Granted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Date_Revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSiteAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSiteAccesses_PosUsers_User_Id",
                        column: x => x.User_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSiteAccesses_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSiteAccesses_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VAT_Name = table.Column<string>(type: "text", nullable: false),
                    VAT_Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VAT_Description = table.Column<string>(type: "text", nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vats_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vats_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vats_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vats_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Voucher_Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Voucher_Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Voucher_Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Is_Percentage = table.Column<bool>(type: "boolean", nullable: false),
                    Min_Basket_Total = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Valid_From = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Valid_To = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vouchers_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vouchers_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vouchers_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vouchers_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SalesTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    SaleTransaction_Total_QTY = table.Column<int>(type: "integer", nullable: false),
                    SaleTransaction_Total_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Total_Paid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Cash = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Card = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Refund = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleTransaction_Promotion = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Change = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Payout = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_CashBack = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleTransaction_Card_Charges = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DayLog_Id = table.Column<int>(type: "integer", nullable: false),
                    Sale_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Is_Printed = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sale_Start_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Shift_Id = table.Column<int>(type: "integer", nullable: true),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: false),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: false),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    ShiftId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTransactions_DayLogs_DayLog_Id",
                        column: x => x.DayLog_Id,
                        principalTable: "DayLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesTransactions_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTransactions_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTransactions_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTransactions_Shifts_Shift_Id",
                        column: x => x.Shift_Id,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesTransactions_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesTransactions_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UnknownProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    ProductBarcode = table.Column<string>(type: "text", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaylogId = table.Column<int>(type: "integer", nullable: true),
                    ShiftId = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnknownProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnknownProducts_DayLogs_DaylogId",
                        column: x => x.DaylogId,
                        principalTable: "DayLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UnknownProducts_PosUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UnknownProducts_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UnknownProducts_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UnknownProducts_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    GlobalId = table.Column<Guid>(type: "uuid", nullable: true),
                    Product_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Product_Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Product_Barcode = table.Column<string>(type: "text", nullable: false),
                    Product_Case_Barcode = table.Column<string>(type: "text", nullable: true),
                    ShelfQuantity = table.Column<int>(type: "integer", nullable: false),
                    StockroomQuantity = table.Column<int>(type: "integer", nullable: false),
                    Department_ID = table.Column<int>(type: "integer", nullable: false),
                    VAT_ID = table.Column<int>(type: "integer", nullable: false),
                    Product_Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Product_Selling_Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Profit_On_Return_Percentage = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Product_Size = table.Column<int>(type: "integer", nullable: true),
                    Product_Measurement = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Promotion_Id = table.Column<int>(type: "integer", nullable: true),
                    Brand_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Product_Min_Order = table.Column<int>(type: "integer", nullable: false),
                    Product_Low_Stock_Alert_QTY = table.Column<int>(type: "integer", nullable: false),
                    Product_Min_Stock_Level = table.Column<int>(type: "integer", nullable: false),
                    Product_Unit_Per_Case = table.Column<int>(type: "integer", nullable: false),
                    Product_Cost_Per_Case = table.Column<decimal>(type: "numeric", nullable: false),
                    Expiry_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Is_Activated = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Priced_Changed_On = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Is_Price_Changed = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Allow_Discount = table.Column<bool>(type: "boolean", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Departments_Department_ID",
                        column: x => x.Department_ID,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Promotions_Promotion_Id",
                        column: x => x.Promotion_Id,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Products_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Vats_VAT_ID",
                        column: x => x.VAT_ID,
                        principalTable: "Vats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoucherDepartmentExclusions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoucherId = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    VoucherId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherDepartmentExclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherDepartmentExclusions_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherDepartmentExclusions_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherDepartmentExclusions_Vouchers_VoucherId1",
                        column: x => x.VoucherId1,
                        principalTable: "Vouchers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesItemTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    SaleTransaction_ID = table.Column<int>(type: "integer", nullable: false),
                    Product_ID = table.Column<int>(type: "integer", nullable: false),
                    Product_QTY = table.Column<int>(type: "integer", nullable: false),
                    Product_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Product_Total_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SalesItemTransactionType = table.Column<int>(type: "integer", nullable: true),
                    SalesPayout_ID = table.Column<int>(type: "integer", nullable: true),
                    Promotion_ID = table.Column<int>(type: "integer", nullable: true),
                    Product_Total_Amount_Before_Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount_Percent = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Is_Manual_Weight_Entry = table.Column<bool>(type: "boolean", nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesItemTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesItemTransactions_Payouts_SalesPayout_ID",
                        column: x => x.SalesPayout_ID,
                        principalTable: "Payouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesItemTransactions_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesItemTransactions_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesItemTransactions_Products_Product_ID",
                        column: x => x.Product_ID,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesItemTransactions_Promotions_Promotion_ID",
                        column: x => x.Promotion_ID,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesItemTransactions_SalesTransactions_SaleTransaction_ID",
                        column: x => x.SaleTransaction_ID,
                        principalTable: "SalesTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    StockTransactionType = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DayLogId = table.Column<int>(type: "integer", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Shift_Id = table.Column<int>(type: "integer", nullable: true),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    From_Site_Id = table.Column<int>(type: "integer", nullable: true),
                    To_Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    ShiftId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransactions_DayLogs_DayLogId",
                        column: x => x.DayLogId,
                        principalTable: "DayLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockTransactions_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransactions_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransactions_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransactions_Sites_From_Site_Id",
                        column: x => x.From_Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransactions_Sites_To_Site_Id",
                        column: x => x.To_Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransactions_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Supplier_Product_Code = table.Column<string>(type: "text", nullable: false),
                    Product_OuterCaseBarcode = table.Column<string>(type: "text", nullable: false),
                    Cost_Per_Case = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Cost_Per_Unit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Unit_Per_Case = table.Column<int>(type: "integer", nullable: false),
                    Profit_On_Return = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierItems_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierItems_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierItems_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierItems_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierItems_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoidedProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    Product_ID = table.Column<int>(type: "integer", nullable: false),
                    Voided_Quantity = table.Column<int>(type: "integer", nullable: false),
                    Voided_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Voided_By_User_ID = table.Column<int>(type: "integer", nullable: false),
                    Void_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Additional_Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Shift_Id = table.Column<int>(type: "integer", nullable: true),
                    Daylog_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false),
                    Created_ById = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_ById = table.Column<int>(type: "integer", nullable: true),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    TillId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoidedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoidedProducts_DayLogs_Daylog_Id",
                        column: x => x.Daylog_Id,
                        principalTable: "DayLogs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoidedProducts_PosUsers_Created_ById",
                        column: x => x.Created_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoidedProducts_PosUsers_Last_Modified_ById",
                        column: x => x.Last_Modified_ById,
                        principalTable: "PosUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoidedProducts_PosUsers_Voided_By_User_ID",
                        column: x => x.Voided_By_User_ID,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoidedProducts_Products_Product_ID",
                        column: x => x.Product_ID,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoidedProducts_Shifts_Shift_Id",
                        column: x => x.Shift_Id,
                        principalTable: "Shifts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoidedProducts_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoidedProducts_Tills_TillId",
                        column: x => x.TillId,
                        principalTable: "Tills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoucherProductExclusions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoucherId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    VoucherId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherProductExclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherProductExclusions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherProductExclusions_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherProductExclusions_Vouchers_VoucherId1",
                        column: x => x.VoucherId1,
                        principalTable: "Vouchers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockRefills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Supa_Id = table.Column<long>(type: "bigint", nullable: true),
                    SaleTransaction_Item_ID = table.Column<int>(type: "integer", nullable: false),
                    Refilled_By = table.Column<int>(type: "integer", nullable: true),
                    Refilled_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Confirmed_By_Scanner = table.Column<bool>(type: "boolean", nullable: false),
                    Refill_Quantity = table.Column<int>(type: "integer", nullable: false),
                    Quantity_Refilled = table.Column<int>(type: "integer", nullable: false),
                    Stock_Refilled = table.Column<bool>(type: "boolean", nullable: false),
                    Shift_ID = table.Column<int>(type: "integer", nullable: true),
                    DayLog_ID = table.Column<int>(type: "integer", nullable: true),
                    Created_By_ID = table.Column<int>(type: "integer", nullable: false),
                    Last_Modified_By_ID = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockRefills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockRefills_DayLogs_DayLog_ID",
                        column: x => x.DayLog_ID,
                        principalTable: "DayLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockRefills_PosUsers_Created_By_ID",
                        column: x => x.Created_By_ID,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockRefills_PosUsers_Last_Modified_By_ID",
                        column: x => x.Last_Modified_By_ID,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockRefills_PosUsers_Refilled_By",
                        column: x => x.Refilled_By,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockRefills_SalesItemTransactions_SaleTransaction_Item_ID",
                        column: x => x.SaleTransaction_Item_ID,
                        principalTable: "SalesItemTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockRefills_Shifts_Shift_ID",
                        column: x => x.Shift_ID,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayLog_Status",
                table: "DayLogs",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DayLogs_Created_ById",
                table: "DayLogs",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_DayLogs_Last_Modified_ById",
                table: "DayLogs",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_DayLogs_SiteId",
                table: "DayLogs",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_DayLogs_TillId",
                table: "DayLogs",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Created_By_Id",
                table: "DeliveryInvoices",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_DayLogId",
                table: "DeliveryInvoices",
                column: "DayLogId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Last_Modified_By_Id",
                table: "DeliveryInvoices",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Shift_Id",
                table: "DeliveryInvoices",
                column: "Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Site_Id",
                table: "DeliveryInvoices",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_SupplierId_InvoiceId",
                table: "DeliveryInvoices",
                columns: new[] { "SupplierId", "InvoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Till_Id",
                table: "DeliveryInvoices",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_DeliveryInvoiceId",
                table: "DeliveryItems",
                column: "DeliveryInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_DeliveryInvoiceId1",
                table: "DeliveryItems",
                column: "DeliveryInvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_ProductId",
                table: "DeliveryItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_SupplierItemId",
                table: "DeliveryItems",
                column: "SupplierItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Created_ById",
                table: "Departments",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Last_Modified_ById",
                table: "Departments",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_SiteId",
                table: "Departments",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_TillId",
                table: "Departments",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLog_SyncStatus",
                table: "DrawerLogs",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLogs_Created_By_Id",
                table: "DrawerLogs",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLogs_DayLog_Id",
                table: "DrawerLogs",
                column: "DayLog_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLogs_Last_Modified_By_Id",
                table: "DrawerLogs",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLogs_OpenedById",
                table: "DrawerLogs",
                column: "OpenedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLogs_Shift_Id",
                table: "DrawerLogs",
                column: "Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLogs_Site_Id",
                table: "DrawerLogs",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DrawerLogs_Till_Id",
                table: "DrawerLogs",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLog_Error_DateTime",
                table: "ErrorLogs",
                column: "Error_DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLog_Is_Resolved",
                table: "ErrorLogs",
                column: "Is_Resolved");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLog_Severity_Level",
                table: "ErrorLogs",
                column: "Severity_Level");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLog_SyncStatus",
                table: "ErrorLogs",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Resolved_By_Id",
                table: "ErrorLogs",
                column: "Resolved_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Site_Id",
                table: "ErrorLogs",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Till_Id",
                table: "ErrorLogs",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_User_Id",
                table: "ErrorLogs",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Payout_SyncStatus",
                table: "Payouts",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_Created_ById",
                table: "Payouts",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_Last_Modified_ById",
                table: "Payouts",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_SiteId",
                table: "Payouts",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_TillId",
                table: "Payouts",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_PosUser_SyncStatus",
                table: "PosUsers",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PosUsers_Primary_Site_Id",
                table: "PosUsers",
                column: "Primary_Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PosUsers_Site_Id",
                table: "PosUsers",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PosUsers_Till_Id",
                table: "PosUsers",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_SyncStatus",
                table: "Products",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Created_ById",
                table: "Products",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Department_ID",
                table: "Products",
                column: "Department_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Last_Modified_ById",
                table: "Products",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Product_Barcode",
                table: "Products",
                column: "Product_Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Promotion_Id",
                table: "Products",
                column: "Promotion_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SiteId",
                table: "Products",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TillId",
                table: "Products",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_VAT_ID",
                table: "Products",
                column: "VAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Promotion_SyncStatus",
                table: "Promotions",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Created_ById",
                table: "Promotions",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Last_Modified_ById",
                table: "Promotions",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_SiteId",
                table: "Promotions",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_TillId",
                table: "Promotions",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrinter_SyncStatus",
                table: "ReceiptPrinters",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrinters_Created_ById",
                table: "ReceiptPrinters",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrinters_Last_Modified_ById",
                table: "ReceiptPrinters",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrinters_SiteId",
                table: "ReceiptPrinters",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrinters_TillId",
                table: "ReceiptPrinters",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemTransaction_SyncStatus",
                table: "SalesItemTransactions",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemTransactions_Created_By_Id",
                table: "SalesItemTransactions",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemTransactions_Last_Modified_By_Id",
                table: "SalesItemTransactions",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemTransactions_Product_ID",
                table: "SalesItemTransactions",
                column: "Product_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemTransactions_Promotion_ID",
                table: "SalesItemTransactions",
                column: "Promotion_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemTransactions_SalesPayout_ID",
                table: "SalesItemTransactions",
                column: "SalesPayout_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemTransactions_SaleTransaction_ID",
                table: "SalesItemTransactions",
                column: "SaleTransaction_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransaction_SyncStatus",
                table: "SalesTransactions",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_Created_By_Id",
                table: "SalesTransactions",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_DayLog_Id",
                table: "SalesTransactions",
                column: "DayLog_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_Last_Modified_By_Id",
                table: "SalesTransactions",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_Shift_Id",
                table: "SalesTransactions",
                column: "Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_ShiftId",
                table: "SalesTransactions",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_Site_Id",
                table: "SalesTransactions",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_Till_Id",
                table: "SalesTransactions",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Shift_SyncStatus",
                table: "Shifts",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_Created_ById",
                table: "Shifts",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_DayLog_Id",
                table: "Shifts",
                column: "DayLog_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_Last_Modified_ById",
                table: "Shifts",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_PosUser_Id",
                table: "Shifts",
                column: "PosUser_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_Site_Id",
                table: "Shifts",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_Till_Id",
                table: "Shifts",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Site_SyncStatus",
                table: "Sites",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Created_By_Id",
                table: "Sites",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Last_Modified_By_Id",
                table: "Sites",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefill_Stock_Refilled",
                table: "StockRefills",
                column: "Stock_Refilled");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefill_SyncStatus",
                table: "StockRefills",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefills_Created_By_ID",
                table: "StockRefills",
                column: "Created_By_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefills_DayLog_ID",
                table: "StockRefills",
                column: "DayLog_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefills_Last_Modified_By_ID",
                table: "StockRefills",
                column: "Last_Modified_By_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefills_Refilled_By",
                table: "StockRefills",
                column: "Refilled_By");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefills_SaleTransaction_Item_ID",
                table: "StockRefills",
                column: "SaleTransaction_Item_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StockRefills_Shift_ID",
                table: "StockRefills",
                column: "Shift_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransaction_SyncStatus",
                table: "StockTransactions",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Created_By_Id",
                table: "StockTransactions",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_DayLogId",
                table: "StockTransactions",
                column: "DayLogId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_From_Site_Id",
                table: "StockTransactions",
                column: "From_Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Last_Modified_By_Id",
                table: "StockTransactions",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId",
                table: "StockTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ShiftId",
                table: "StockTransactions",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Till_Id",
                table: "StockTransactions",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_To_Site_Id",
                table: "StockTransactions",
                column: "To_Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_Created_ById",
                table: "SupplierItems",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_Last_Modified_ById",
                table: "SupplierItems",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_ProductId",
                table: "SupplierItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_SiteId",
                table: "SupplierItems",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_SupplierId",
                table: "SupplierItems",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_SyncStatus",
                table: "SupplierItems",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierItems_TillId",
                table: "SupplierItems",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_SyncStatus",
                table: "Suppliers",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Created_ById",
                table: "Suppliers",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Last_Modified_ById",
                table: "Suppliers",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SiteId",
                table: "Suppliers",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TillId",
                table: "Suppliers",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_TableTrackers_PosUserId",
                table: "TableTrackers",
                column: "PosUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TableTrackers_SiteId",
                table: "TableTrackers",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TableTrackers_TillId",
                table: "TableTrackers",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_Till_SyncStatus",
                table: "Tills",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tills_Site_Id",
                table: "Tills",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownProduct_ProductBarcode",
                table: "UnknownProducts",
                column: "ProductBarcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnknownProduct_SyncStatus",
                table: "UnknownProducts",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownProducts_CreatedById",
                table: "UnknownProducts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownProducts_DaylogId",
                table: "UnknownProducts",
                column: "DaylogId");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownProducts_ShiftId",
                table: "UnknownProducts",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownProducts_SiteId",
                table: "UnknownProducts",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownProducts_TillId",
                table: "UnknownProducts",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_UnSyncedLogs_SyncStatus",
                table: "UnSyncedLogs",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserSiteAccess_SyncStatus",
                table: "UserSiteAccesses",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserSiteAccesses_Site_Id",
                table: "UserSiteAccesses",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSiteAccesses_TillId",
                table: "UserSiteAccesses",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSiteAccesses_User_Id",
                table: "UserSiteAccesses",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vats_Created_ById",
                table: "Vats",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Vats_Last_Modified_ById",
                table: "Vats",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Vats_SiteId",
                table: "Vats",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vats_TillId",
                table: "Vats",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProduct_SyncStatus",
                table: "VoidedProducts",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_Created_ById",
                table: "VoidedProducts",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_Daylog_Id",
                table: "VoidedProducts",
                column: "Daylog_Id");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_Last_Modified_ById",
                table: "VoidedProducts",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_Product_ID",
                table: "VoidedProducts",
                column: "Product_ID");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_Shift_Id",
                table: "VoidedProducts",
                column: "Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_SiteId",
                table: "VoidedProducts",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_TillId",
                table: "VoidedProducts",
                column: "TillId");

            migrationBuilder.CreateIndex(
                name: "IX_VoidedProducts_Voided_By_User_ID",
                table: "VoidedProducts",
                column: "Voided_By_User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDepartmentExclusions_DepartmentId",
                table: "VoucherDepartmentExclusions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDepartmentExclusions_VoucherId",
                table: "VoucherDepartmentExclusions",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDepartmentExclusions_VoucherId1",
                table: "VoucherDepartmentExclusions",
                column: "VoucherId1");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherProductExclusions_ProductId",
                table: "VoucherProductExclusions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherProductExclusions_VoucherId",
                table: "VoucherProductExclusions",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherProductExclusions_VoucherId1",
                table: "VoucherProductExclusions",
                column: "VoucherId1");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Created_By_Id",
                table: "Vouchers",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Last_Modified_By_Id",
                table: "Vouchers",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Site_Id",
                table: "Vouchers",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Till_Id",
                table: "Vouchers",
                column: "Till_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayLogs_PosUsers_Created_ById",
                table: "DayLogs",
                column: "Created_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayLogs_PosUsers_Last_Modified_ById",
                table: "DayLogs",
                column: "Last_Modified_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayLogs_Sites_SiteId",
                table: "DayLogs",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayLogs_Tills_TillId",
                table: "DayLogs",
                column: "TillId",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Created_By_Id",
                table: "DeliveryInvoices",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Last_Modified_By_Id",
                table: "DeliveryInvoices",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Shifts_Shift_Id",
                table: "DeliveryInvoices",
                column: "Shift_Id",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Sites_Site_Id",
                table: "DeliveryInvoices",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Suppliers_SupplierId",
                table: "DeliveryInvoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Tills_Till_Id",
                table: "DeliveryInvoices",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryItems_Products_ProductId",
                table: "DeliveryItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryItems_SupplierItems_SupplierItemId",
                table: "DeliveryItems",
                column: "SupplierItemId",
                principalTable: "SupplierItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_PosUsers_Created_ById",
                table: "Departments",
                column: "Created_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_PosUsers_Last_Modified_ById",
                table: "Departments",
                column: "Last_Modified_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Sites_SiteId",
                table: "Departments",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Tills_TillId",
                table: "Departments",
                column: "TillId",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_Created_By_Id",
                table: "DrawerLogs",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_Last_Modified_By_Id",
                table: "DrawerLogs",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_OpenedById",
                table: "DrawerLogs",
                column: "OpenedById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_Shifts_Shift_Id",
                table: "DrawerLogs",
                column: "Shift_Id",
                principalTable: "Shifts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_Sites_Site_Id",
                table: "DrawerLogs",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_Tills_Till_Id",
                table: "DrawerLogs",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_PosUsers_Resolved_By_Id",
                table: "ErrorLogs",
                column: "Resolved_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_PosUsers_User_Id",
                table: "ErrorLogs",
                column: "User_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_Sites_Site_Id",
                table: "ErrorLogs",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_Tills_Till_Id",
                table: "ErrorLogs",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payouts_PosUsers_Created_ById",
                table: "Payouts",
                column: "Created_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payouts_PosUsers_Last_Modified_ById",
                table: "Payouts",
                column: "Last_Modified_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payouts_Sites_SiteId",
                table: "Payouts",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payouts_Tills_TillId",
                table: "Payouts",
                column: "TillId",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Sites_Primary_Site_Id",
                table: "PosUsers",
                column: "Primary_Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Sites_Site_Id",
                table: "PosUsers",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Tills_Till_Id",
                table: "PosUsers",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_PosUsers_Created_By_Id",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_PosUsers_Last_Modified_By_Id",
                table: "Sites");

            migrationBuilder.DropTable(
                name: "DeliveryItems");

            migrationBuilder.DropTable(
                name: "DrawerLogs");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "ReceiptPrinters");

            migrationBuilder.DropTable(
                name: "Retailers");

            migrationBuilder.DropTable(
                name: "StockRefills");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "SyncedLogs");

            migrationBuilder.DropTable(
                name: "TableTrackers");

            migrationBuilder.DropTable(
                name: "UnknownProducts");

            migrationBuilder.DropTable(
                name: "UnSyncedLogs");

            migrationBuilder.DropTable(
                name: "UserSiteAccesses");

            migrationBuilder.DropTable(
                name: "VoidedProducts");

            migrationBuilder.DropTable(
                name: "VoucherDepartmentExclusions");

            migrationBuilder.DropTable(
                name: "VoucherProductExclusions");

            migrationBuilder.DropTable(
                name: "DeliveryInvoices");

            migrationBuilder.DropTable(
                name: "SupplierItems");

            migrationBuilder.DropTable(
                name: "SalesItemTransactions");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Payouts");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "SalesTransactions");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Vats");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "DayLogs");

            migrationBuilder.DropTable(
                name: "PosUsers");

            migrationBuilder.DropTable(
                name: "Tills");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
