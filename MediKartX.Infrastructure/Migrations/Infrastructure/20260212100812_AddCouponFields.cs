using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediKartX.Infrastructure.Migrations.Infrastructure
{
    /// <inheritdoc />
    public partial class AddCouponFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration runs against databases that may already have the core schema
            // created out-of-band. To avoid attempting to recreate existing tables, add
            // only the new/changed columns and create the dispatch table if missing.

            // Add Coupon new columns if they don't exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('Coupons','DiscountType') IS NULL
BEGIN
    ALTER TABLE Coupons ADD DiscountType NVARCHAR(MAX) NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Coupons','DiscountValue') IS NULL
BEGIN
    ALTER TABLE Coupons ADD DiscountValue DECIMAL(18,2) NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Coupons','MinOrderAmount') IS NULL
BEGIN
    ALTER TABLE Coupons ADD MinOrderAmount DECIMAL(18,2) NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Coupons','MaxUsageCount') IS NULL
BEGIN
    ALTER TABLE Coupons ADD MaxUsageCount INT NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Coupons','MaxUsagePerUser') IS NULL
BEGIN
    ALTER TABLE Coupons ADD MaxUsagePerUser INT NULL;
END
");

            // Make ExpiryDate nullable if it's currently NOT NULL
            migrationBuilder.Sql(@"
IF COL_LENGTH('Coupons','ExpiryDate') IS NOT NULL
BEGIN
    -- Attempt to alter column to nullable (safe if column exists)
    ALTER TABLE Coupons ALTER COLUMN ExpiryDate DATETIME NULL;
END
");

            // Create OrderDispatches table only if it doesn't exist
            migrationBuilder.Sql(@"
IF OBJECT_ID('OrderDispatches') IS NULL
BEGIN
    CREATE TABLE OrderDispatches (
        OrderDispatchId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ShopIdsJson NVARCHAR(MAX) NULL,
        CurrentIndex INT NOT NULL DEFAULT 0,
        StartedAt DATETIME NULL,
        IsCompleted BIT NULL
    );
END
");

            // The rest of the generated migration contains CREATE TABLE statements for
            // the full schema. Many databases already have these tables; to avoid
            // errors when running migrations against an existing database, return
            // now after ensuring the new/changed columns are added.
            return;

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CartItem__488B0B0A1406A2D0", x => x.CartItemId);
                    table.ForeignKey(
                        name: "FK__CartItems__CartI__6FE99F9F",
                        column: x => x.CartId,
                        principalTable: "Cart",
                        principalColumn: "CartId");
                    table.ForeignKey(
                        name: "FK__CartItems__Medic__70DDC3D8",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId");
                });

            migrationBuilder.CreateTable(
                name: "CouponUsage",
                columns: table => new
                {
                    CouponUsageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CouponUs__10AE105D080E3884", x => x.CouponUsageId);
                    table.ForeignKey(
                        name: "FK__CouponUsa__Coupo__0D7A0286",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "CouponId");
                    table.ForeignKey(
                        name: "FK__CouponUsa__Order__0F624AF8",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK__CouponUsa__UserI__0E6E26BF",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "OrderHistories",
                columns: table => new
                {
                    OrderHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistories", x => x.OrderHistoryId);
                    table.ForeignKey(
                        name: "FK_OrderHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderIte__57ED06814EDF382A", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK__OrderItem__Medic__7D439ABD",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId");
                    table.ForeignKey(
                        name: "FK__OrderItem__Order__7C4F7684",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentMode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValue: "Pending"),
                    TransactionReference = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentT__55433A6B550C979E", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK__PaymentTr__Order__1F98B2C1",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    ShipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CourierName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValue: "Pending"),
                    EstimatedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Shipment__5CAD37EDF55DADD9", x => x.ShipmentId);
                    table.ForeignKey(
                        name: "FK__Shipments__Order__3587F3E0",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "RewardTransactions",
                columns: table => new
                {
                    RewardTransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    PointsChanged = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RewardTr__CF7CB7E5450AF238", x => x.RewardTransactionId);
                    table.ForeignKey(
                        name: "FK__RewardTra__Order__19DFD96B",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK__RewardTra__Rewar__18EBB532",
                        column: x => x.RewardId,
                        principalTable: "Rewards",
                        principalColumn: "RewardId");
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    WishlistItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WishlistId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Wishlist__171E21A16A0B1AA4", x => x.WishlistItemId);
                    table.ForeignKey(
                        name: "FK__WishlistI__Medic__2FCF1A8A",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId");
                    table.ForeignKey(
                        name: "FK__WishlistI__Wishl__2EDAF651",
                        column: x => x.WishlistId,
                        principalTable: "Wishlists",
                        principalColumn: "WishlistId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_UserId",
                table: "ApiLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Brands__2206CE9BD4ED64E9",
                table: "Brands",
                column: "BrandName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cart_UserId",
                table: "Cart",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_MedicineId",
                table: "CartItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "UQ__Coupons__A25C5AA763E270A8",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsage_CouponId",
                table: "CouponUsage",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsage_OrderId",
                table: "CouponUsage",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsage_UserId",
                table: "CouponUsage",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_BrandId",
                table: "Medicines",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_CategoryId",
                table: "Medicines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistories_OrderId",
                table: "OrderHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MedicineId",
                table: "OrderItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OTPLogs_UserId",
                table: "OTPLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_OrderId",
                table: "PaymentTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_MedicineId",
                table: "ProductReviews",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_UserId",
                table: "ProductReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_UserId",
                table: "Rewards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardTransactions_OrderId",
                table: "RewardTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardTransactions_RewardId",
                table: "RewardTransactions",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B6160B0120EAE",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OrderId",
                table: "Shipments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockHistory_MedicineId",
                table: "StockHistory",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__250375B141509CB0",
                table: "Users",
                column: "MobileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_MedicineId",
                table: "WishlistItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_WishlistId",
                table: "WishlistItems",
                column: "WishlistId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_UserId",
                table: "Wishlists",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "ApiLogs");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "CouponUsage");

            migrationBuilder.DropTable(
                name: "OrderDispatches");

            migrationBuilder.DropTable(
                name: "OrderHistories");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OTPLogs");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "RewardTransactions");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "StockHistory");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "Rewards");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
