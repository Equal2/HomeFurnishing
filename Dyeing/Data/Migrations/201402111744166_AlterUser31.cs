namespace Surya.Planning.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterUser31 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BuyerAccounts", "ModifiedBy", c => c.String());
            AlterColumn("dbo.Buyers", "ModifiedBy", c => c.String());
            AlterColumn("dbo.BuyerDetails", "ModifiedBy", c => c.String());
            AlterColumn("dbo.SalesOrders", "ModifiedBy", c => c.String());
            AlterColumn("dbo.SalesOrderDetails", "ModifiedBy", c => c.String());
            AlterColumn("dbo.Products", "ModifiedBy", c => c.String());
            AlterColumn("dbo.ProductTypes", "ModifiedBy", c => c.String());
            AlterColumn("dbo.SalesOrderProducts", "ModifiedBy", c => c.String());
        }

        public override void Down()
        {
            AlterColumn("dbo.SalesOrderProducts", "ModifiedBy", c => c.Int(nullable: false));
            AlterColumn("dbo.ProductTypes", "ModifiedBy", c => c.Int(nullable: false));
            AlterColumn("dbo.Products", "ModifiedBy", c => c.Int(nullable: false));
            AlterColumn("dbo.SalesOrderDetails", "ModifiedBy", c => c.Int(nullable: false));
            AlterColumn("dbo.SalesOrders", "ModifiedBy", c => c.Int(nullable: false));
            AlterColumn("dbo.BuyerDetails", "ModifiedBy", c => c.Int(nullable: false));
            AlterColumn("dbo.Buyers", "ModifiedBy", c => c.Int(nullable: false));
            AlterColumn("dbo.BuyerAccounts", "ModifiedBy", c => c.Int(nullable: false));
        }
    }
}
