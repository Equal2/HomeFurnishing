namespace Login.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PhoneNumberTypeChange : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "Mobile", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "Mobile", c => c.Int(nullable: false));
        }
    }
}
