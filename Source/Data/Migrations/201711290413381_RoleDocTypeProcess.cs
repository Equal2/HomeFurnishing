namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoleDocTypeProcess : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Web.RolesDocTypeProcesses", "RolesDocTypeId", "Web.RolesDocTypes");
            DropIndex("Web.RolesDocTypeProcesses", new[] { "RolesDocTypeId" });
            AddColumn("Web.ControllerActions", "DisplayName", c => c.String());
            AddColumn("Web.RolesDocTypes", "ControllerName", c => c.String(nullable: false));
            AddColumn("Web.RolesDocTypeProcesses", "RoleId", c => c.String(maxLength: 128));
            AddColumn("Web.RolesDocTypeProcesses", "DocTypeId", c => c.Int(nullable: false));
            AlterColumn("Web.RolesDocTypes", "ActionName", c => c.String(nullable: false));
            CreateIndex("Web.RolesDocTypeProcesses", "RoleId");
            CreateIndex("Web.RolesDocTypeProcesses", "DocTypeId");
            AddForeignKey("Web.RolesDocTypeProcesses", "DocTypeId", "Web.DocumentTypes", "DocumentTypeId");
            AddForeignKey("Web.RolesDocTypeProcesses", "RoleId", "Web.AspNetRoles", "Id");
            DropColumn("Web.RolesDocTypes", "ConsotollerName");
            DropColumn("Web.RolesDocTypeProcesses", "RolesDocTypeId");
        }
        
        public override void Down()
        {
            AddColumn("Web.RolesDocTypeProcesses", "RolesDocTypeId", c => c.Int(nullable: false));
            AddColumn("Web.RolesDocTypes", "ConsotollerName", c => c.String());
            DropForeignKey("Web.RolesDocTypeProcesses", "RoleId", "Web.AspNetRoles");
            DropForeignKey("Web.RolesDocTypeProcesses", "DocTypeId", "Web.DocumentTypes");
            DropIndex("Web.RolesDocTypeProcesses", new[] { "DocTypeId" });
            DropIndex("Web.RolesDocTypeProcesses", new[] { "RoleId" });
            AlterColumn("Web.RolesDocTypes", "ActionName", c => c.String());
            DropColumn("Web.RolesDocTypeProcesses", "DocTypeId");
            DropColumn("Web.RolesDocTypeProcesses", "RoleId");
            DropColumn("Web.RolesDocTypes", "ControllerName");
            DropColumn("Web.ControllerActions", "DisplayName");
            CreateIndex("Web.RolesDocTypeProcesses", "RolesDocTypeId");
            AddForeignKey("Web.RolesDocTypeProcesses", "RolesDocTypeId", "Web.RolesDocTypes", "RolesDocTypeId");
        }
    }
}
