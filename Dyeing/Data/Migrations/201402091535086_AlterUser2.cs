namespace Surya.Planning.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterUser2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SalesOrders",
                c => new
                    {
                        SalesOrderId = c.Int(nullable: false, identity: true),
                        OrderNumber = c.String(),
                        OrderDate = c.DateTime(nullable: false),
                        DeliveryDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Buyer_BuyerID = c.Int(),
                    })
                .PrimaryKey(t => t.SalesOrderId)
                .ForeignKey("dbo.Buyers", t => t.Buyer_BuyerID)
                .Index(t => t.Buyer_BuyerID);
            
            CreateTable(
                "dbo.SalesOrderDetails",
                c => new
                    {
                        SalesOrderDetailId = c.Int(nullable: false, identity: true),
                        ShipToParty = c.String(),
                        PartyOrderNumber = c.String(),
                        ReferenceOrderNumber = c.String(),
                        ReferenceOrderDate = c.DateTime(nullable: false),
                        Status = c.String(),
                        PriceMode = c.String(),
                        ReferenceParty = c.String(),
                        Saviourity = c.String(),
                        Priority = c.String(),
                        CompanyFor = c.String(),
                        SignedBy = c.String(),
                        ApprovedBy = c.String(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Product_ProductId = c.Int(),
                        SalesOrder_SalesOrderId = c.Int(),
                    })
                .PrimaryKey(t => t.SalesOrderDetailId)
                .ForeignKey("dbo.Products", t => t.Product_ProductId)
                .ForeignKey("dbo.SalesOrders", t => t.SalesOrder_SalesOrderId)
                .Index(t => t.Product_ProductId)
                .Index(t => t.SalesOrder_SalesOrderId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        ProductName = c.String(),
                        ProductDiscription = c.String(),
                        ProductCode = c.String(),
                        ProductPicture = c.Binary(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        ProductType_ProductTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.ProductId)
                .ForeignKey("dbo.ProductTypes", t => t.ProductType_ProductTypeId)
                .Index(t => t.ProductType_ProductTypeId);
            
            CreateTable(
                "dbo.ProductTypes",
                c => new
                    {
                        ProductTypeId = c.Int(nullable: false, identity: true),
                        ProductTypeName = c.String(),
                        ProductTypeDiscription = c.String(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProductTypeId);
            
            CreateTable(
                "dbo.SalesOrderProducts",
                c => new
                    {
                        SalesOrderProductId = c.Int(nullable: false, identity: true),
                        ProductSKU = c.String(),
                        BuyerSKU = c.String(),
                        BuyerUPC = c.String(),
                        ProductDesign = c.String(),
                        PCs = c.Int(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AreaPerPC = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        SalesOrder_SalesOrderId = c.Int(),
                    })
                .PrimaryKey(t => t.SalesOrderProductId)
                .ForeignKey("dbo.SalesOrders", t => t.SalesOrder_SalesOrderId)
                .Index(t => t.SalesOrder_SalesOrderId);
            
          //  DropTable("dbo.Orders");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        CustomerID = c.String(),
                        EmployeeID = c.Int(),
                        OrderDate = c.DateTime(),
                        RequiredDate = c.DateTime(),
                        ShippedDate = c.DateTime(),
                        ShipVia = c.Int(),
                        Freight = c.Decimal(precision: 18, scale: 2),
                        ShipName = c.String(),
                        ShipAddress = c.String(),
                        ShipCity = c.String(),
                        ShipRegion = c.String(),
                        ShipPostalCode = c.String(),
                        ShipCountry = c.String(),
                    })
                .PrimaryKey(t => t.OrderID);
            
            DropForeignKey("dbo.SalesOrderProducts", "SalesOrder_SalesOrderId", "dbo.SalesOrders");
            DropForeignKey("dbo.SalesOrderDetails", "SalesOrder_SalesOrderId", "dbo.SalesOrders");
            DropForeignKey("dbo.SalesOrderDetails", "Product_ProductId", "dbo.Products");
            DropForeignKey("dbo.Products", "ProductType_ProductTypeId", "dbo.ProductTypes");
            DropForeignKey("dbo.SalesOrders", "Buyer_BuyerID", "dbo.Buyers");
            DropIndex("dbo.SalesOrderProducts", new[] { "SalesOrder_SalesOrderId" });
            DropIndex("dbo.SalesOrderDetails", new[] { "SalesOrder_SalesOrderId" });
            DropIndex("dbo.SalesOrderDetails", new[] { "Product_ProductId" });
            DropIndex("dbo.Products", new[] { "ProductType_ProductTypeId" });
            DropIndex("dbo.SalesOrders", new[] { "Buyer_BuyerID" });
            DropTable("dbo.SalesOrderProducts");
            DropTable("dbo.ProductTypes");
            DropTable("dbo.Products");
            DropTable("dbo.SalesOrderDetails");
            DropTable("dbo.SalesOrders");
        }
    }
}
