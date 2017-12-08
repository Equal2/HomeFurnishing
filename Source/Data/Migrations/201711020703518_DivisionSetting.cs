namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DivisionSetting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Web.DivisionSettings",
                c => new
                    {
                        DivisionSettingId = c.Int(nullable: false, identity: true),
                        DivisionId = c.Int(nullable: false),
                        Dimension1Caption = c.String(maxLength: 50),
                        Dimension2Caption = c.String(maxLength: 50),
                        Dimension3Caption = c.String(maxLength: 50),
                        Dimension4Caption = c.String(maxLength: 50),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DivisionSettingId)
                .ForeignKey("Web.Divisions", t => t.DivisionId)
                .Index(t => t.DivisionId);
            
            AddColumn("Web.Units", "GSTUnit", c => c.String(maxLength: 50));
            AddColumn("Web.SaleInvoiceHeaderDetail", "Deduction", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.PromoCodes", "ProcessId", c => c.Int(nullable: false));
            AddColumn("Web.PromoCodes", "MinQty", c => c.Decimal(precision: 18, scale: 4));
            CreateIndex("Web.PromoCodes", "ProcessId");
            AddForeignKey("Web.PromoCodes", "ProcessId", "Web.Processes", "ProcessId");
        }
        
        public override void Down()
        {
            DropForeignKey("Web.DivisionSettings", "DivisionId", "Web.Divisions");
            DropForeignKey("Web.PromoCodes", "ProcessId", "Web.Processes");
            DropIndex("Web.DivisionSettings", new[] { "DivisionId" });
            DropIndex("Web.PromoCodes", new[] { "ProcessId" });
            DropColumn("Web.PromoCodes", "MinQty");
            DropColumn("Web.PromoCodes", "ProcessId");
            DropColumn("Web.SaleInvoiceHeaderDetail", "Deduction");
            DropColumn("Web.Units", "GSTUnit");
            DropTable("Web.DivisionSettings");
        }
    }
}
