namespace Login.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mobilenumremoved : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "Mobile");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Mobile", c => c.String(nullable: false));
        }
    }
}
