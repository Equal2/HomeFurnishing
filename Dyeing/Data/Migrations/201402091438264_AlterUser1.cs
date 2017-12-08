namespace Surya.Planning.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterUser1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BuyerAccounts",
                c => new
                    {
                        BuyerAccountID = c.Int(nullable: false, identity: true),
                        BankName = c.String(),
                        Currency = c.String(),
                        BankAccount = c.String(),
                        BankAddress1 = c.String(),
                        BankAddress2 = c.String(),
                        BankAddress3 = c.String(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Buyer_BuyerID = c.Int(),
                    })
                .PrimaryKey(t => t.BuyerAccountID)
                .ForeignKey("dbo.Buyers", t => t.Buyer_BuyerID)
                .Index(t => t.Buyer_BuyerID);
            
            CreateTable(
                "dbo.Buyers",
                c => new
                    {
                        BuyerID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Discription = c.String(),
                        BusinessType = c.String(),
                        AddressDetails1 = c.String(),
                        AddressDetails2 = c.String(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BuyerID);
            
            CreateTable(
                "dbo.BuyerDetails",
                c => new
                    {
                        BuyerDetailID = c.Int(nullable: false, identity: true),
                        Country = c.String(),
                        City = c.String(),
                        State = c.String(),
                        PostalCode = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        ContectNumber1 = c.String(),
                        AdditionalContectNo = c.String(),
                        ContectPerson = c.String(),
                        FaxNo = c.String(),
                        EmailAddr1 = c.String(),
                        EmailAddr2 = c.String(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Buyer_BuyerID = c.Int(),
                    })
                .PrimaryKey(t => t.BuyerDetailID)
                .ForeignKey("dbo.Buyers", t => t.Buyer_BuyerID)
                .Index(t => t.Buyer_BuyerID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BuyerDetails", "Buyer_BuyerID", "dbo.Buyers");
            DropForeignKey("dbo.BuyerAccounts", "Buyer_BuyerID", "dbo.Buyers");
            DropIndex("dbo.BuyerDetails", new[] { "Buyer_BuyerID" });
            DropIndex("dbo.BuyerAccounts", new[] { "Buyer_BuyerID" });
            DropTable("dbo.BuyerDetails");
            DropTable("dbo.Buyers");
            DropTable("dbo.BuyerAccounts");
        }
    }
}
