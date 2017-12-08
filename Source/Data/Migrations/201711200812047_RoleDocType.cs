namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoleDocType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Web.RolesDocTypes",
                c => new
                    {
                        RolesDocTypeId = c.Int(nullable: false, identity: true),
                        RoleId = c.String(maxLength: 128),
                        DocTypeId = c.Int(nullable: false),
                        ConsotollerName = c.String(),
                        ActionName = c.String(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RolesDocTypeId)
                .ForeignKey("Web.DocumentTypes", t => t.DocTypeId)
                .ForeignKey("Web.AspNetRoles", t => t.RoleId)
                .Index(t => t.RoleId)
                .Index(t => t.DocTypeId);
            
            CreateTable(
                "Web.RolesDocTypeProcesses",
                c => new
                    {
                        RolesDocTypeProcessId = c.Int(nullable: false, identity: true),
                        RolesDocTypeId = c.Int(nullable: false),
                        ProcessId = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RolesDocTypeProcessId)
                .ForeignKey("Web.Processes", t => t.ProcessId)
                .ForeignKey("Web.RolesDocTypes", t => t.RolesDocTypeId)
                .Index(t => t.RolesDocTypeId)
                .Index(t => t.ProcessId);
            
            AddColumn("Web.JobInvoiceReturnLines", "CostCenterId", c => c.Int());
            AddColumn("Web.JobReceiveSettings", "isVisibleProcessHeader", c => c.Boolean());
            AddColumn("Web.PackingSettings", "isVisibleShipMethod", c => c.Boolean());
            AddColumn("Web.UserRoles", "DivisionId", c => c.Int(nullable: false));
            AddColumn("Web.UserRoles", "SiteId", c => c.Int(nullable: false));
            AddColumn("Web.UserRoles", "CreatedBy", c => c.String());
            AddColumn("Web.UserRoles", "ModifiedBy", c => c.String());
            AddColumn("Web.UserRoles", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("Web.UserRoles", "ModifiedDate", c => c.DateTime(nullable: false));
            CreateIndex("Web.JobInvoiceReturnLines", "CostCenterId");
            CreateIndex("Web.UserRoles", "DivisionId");
            CreateIndex("Web.UserRoles", "SiteId");
            AddForeignKey("Web.JobInvoiceReturnLines", "CostCenterId", "Web.CostCenters", "CostCenterId");
            //AddForeignKey("Web.UserRoles", "DivisionId", "Web.Divisions", "DivisionId");
            //AddForeignKey("Web.UserRoles", "SiteId", "Web.Sites", "SiteId");
        }
        
        public override void Down()
        {
            //DropForeignKey("Web.UserRoles", "SiteId", "Web.Sites");
            //DropForeignKey("Web.UserRoles", "DivisionId", "Web.Divisions");
            DropForeignKey("Web.RolesDocTypeProcesses", "RolesDocTypeId", "Web.RolesDocTypes");
            DropForeignKey("Web.RolesDocTypeProcesses", "ProcessId", "Web.Processes");
            DropForeignKey("Web.RolesDocTypes", "RoleId", "Web.AspNetRoles");
            DropForeignKey("Web.RolesDocTypes", "DocTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.JobInvoiceReturnLines", "CostCenterId", "Web.CostCenters");
            DropIndex("Web.UserRoles", new[] { "SiteId" });
            DropIndex("Web.UserRoles", new[] { "DivisionId" });
            DropIndex("Web.RolesDocTypeProcesses", new[] { "ProcessId" });
            DropIndex("Web.RolesDocTypeProcesses", new[] { "RolesDocTypeId" });
            DropIndex("Web.RolesDocTypes", new[] { "DocTypeId" });
            DropIndex("Web.RolesDocTypes", new[] { "RoleId" });
            DropIndex("Web.JobInvoiceReturnLines", new[] { "CostCenterId" });
            DropColumn("Web.UserRoles", "ModifiedDate");
            DropColumn("Web.UserRoles", "CreatedDate");
            DropColumn("Web.UserRoles", "ModifiedBy");
            DropColumn("Web.UserRoles", "CreatedBy");
            DropColumn("Web.UserRoles", "SiteId");
            DropColumn("Web.UserRoles", "DivisionId");
            DropColumn("Web.PackingSettings", "isVisibleShipMethod");
            DropColumn("Web.JobReceiveSettings", "isVisibleProcessHeader");
            DropColumn("Web.JobInvoiceReturnLines", "CostCenterId");
            DropTable("Web.RolesDocTypeProcesses");
            DropTable("Web.RolesDocTypes");
        }
    }
}
