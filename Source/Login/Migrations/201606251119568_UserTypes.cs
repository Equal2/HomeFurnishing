namespace Login.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTypes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        NotificationSubjectId = c.Int(nullable: false),
                        NotificationText = c.String(nullable: false),
                        NotificationUrl = c.String(),
                        UrlKey = c.String(),
                        ExpiryDate = c.DateTime(),
                        ReadDate = c.DateTime(),
                        SeenDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.NotificationSubjects", t => t.NotificationSubjectId, cascadeDelete: true)
                .Index(t => t.NotificationSubjectId);
            
            CreateTable(
                "dbo.NotificationSubjects",
                c => new
                    {
                        NotificationSubjectId = c.Int(nullable: false, identity: true),
                        NotificationSubjectName = c.String(nullable: false, maxLength: 50),
                        IconName = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.NotificationSubjectId)
                .Index(t => t.NotificationSubjectName, unique: true, name: "IX_NotificationSubject_NotificationSubjectName");
            
            CreateTable(
                "dbo.NotificationUsers",
                c => new
                    {
                        NotificationUserId = c.Int(nullable: false, identity: true),
                        NotificationId = c.Int(nullable: false),
                        UserName = c.String(maxLength: 128),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.NotificationUserId);
            
            CreateTable(
                "dbo.UserTypes",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Name);
            
            AddColumn("dbo.AspNetUsers", "UserType", c => c.String(nullable: false));
            AddColumn("dbo.AspNetUsers", "Company", c => c.String());
            AddColumn("dbo.AspNetUsers", "City", c => c.String(nullable: false));
            AddColumn("dbo.AspNetUsers", "Mobile", c => c.Int(nullable: false));
            //AlterColumn("dbo.AspNetUsers", "FirstName", c => c.String(nullable: false));
            //AlterColumn("dbo.AspNetUsers", "LastName", c => c.String(nullable: false));
            DropTable("dbo.Employees");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        EmployeeId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.EmployeeId);
            
            DropForeignKey("dbo.Notifications", "NotificationSubjectId", "dbo.NotificationSubjects");
            DropIndex("dbo.NotificationSubjects", "IX_NotificationSubject_NotificationSubjectName");
            DropIndex("dbo.Notifications", new[] { "NotificationSubjectId" });
            AlterColumn("dbo.AspNetUsers", "LastName", c => c.String());
            AlterColumn("dbo.AspNetUsers", "FirstName", c => c.String());
            DropColumn("dbo.AspNetUsers", "Mobile");
            DropColumn("dbo.AspNetUsers", "City");
            DropColumn("dbo.AspNetUsers", "Company");
            DropColumn("dbo.AspNetUsers", "UserType");
            DropTable("dbo.UserTypes");
            DropTable("dbo.NotificationUsers");
            DropTable("dbo.NotificationSubjects");
            DropTable("dbo.Notifications");
        }
    }
}
