namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobOrderBomMaterialIssue : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Web.StockBalances", "CostCenterId", "Web.CostCenters");
            DropForeignKey("Web.StockBalances", "Dimension1Id", "Web.Dimension1");
            DropForeignKey("Web.StockBalances", "Dimension2Id", "Web.Dimension2");
            DropForeignKey("Web.StockBalances", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.StockBalances", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.StockBalances", "GodownId", "Web.Godowns");
            DropForeignKey("Web.StockBalances", "ProcessId", "Web.Processes");
            DropForeignKey("Web.StockBalances", "ProductId", "Web.Products");
            DropForeignKey("Web.StockProcessBalances", "CostCenterId", "Web.CostCenters");
            DropForeignKey("Web.StockProcessBalances", "Dimension1Id", "Web.Dimension1");
            DropForeignKey("Web.StockProcessBalances", "Dimension2Id", "Web.Dimension2");
            DropForeignKey("Web.StockProcessBalances", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.StockProcessBalances", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.StockProcessBalances", "GodownId", "Web.Godowns");
            DropForeignKey("Web.StockProcessBalances", "ProcessId", "Web.Processes");
            DropForeignKey("Web.StockProcessBalances", "ProductId", "Web.Products");
            DropForeignKey("Web.ProductCategories", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductDesigns", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductGroups", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductTypeAttributes", "ProductType_ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension1", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension2", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension3", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension4", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductCollections", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductDesignPatterns", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductQualities", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.PromoCodes", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductTypeQaAttributes", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.FinishedProduct", "ProductShapeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductTypes", "ProductNatureId", "Web.ProductNatures");
            DropForeignKey("Web.CostCenters", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.Processes", "AccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.StockHeaders", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.StockHeaders", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.LedgerHeaders", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationFooters", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationFooters", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationHeaderLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationProducts", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationProducts", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.ChargeGroupSettings", "ChargeLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.Ledgers", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.Ledgers", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.LedgerLines", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.TdsRates", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.LedgerAccounts", "LedgerAccountGroupId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PersonSettings", "LedgerAccountGroupId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropIndex("Web.JobReceiveHeaders", new[] { "JobReceiveById" });
            DropIndex("Web.JobReceiveQAHeaders", new[] { "QAById" });
            DropIndex("Web.StockBalances", new[] { "ProductId" });
            DropIndex("Web.StockBalances", new[] { "Dimension1Id" });
            DropIndex("Web.StockBalances", new[] { "Dimension2Id" });
            DropIndex("Web.StockBalances", new[] { "Dimension3Id" });
            DropIndex("Web.StockBalances", new[] { "Dimension4Id" });
            DropIndex("Web.StockBalances", new[] { "ProcessId" });
            DropIndex("Web.StockBalances", new[] { "GodownId" });
            DropIndex("Web.StockBalances", new[] { "CostCenterId" });
            DropIndex("Web.StockProcessBalances", new[] { "ProductId" });
            DropIndex("Web.StockProcessBalances", new[] { "Dimension1Id" });
            DropIndex("Web.StockProcessBalances", new[] { "Dimension2Id" });
            DropIndex("Web.StockProcessBalances", new[] { "Dimension3Id" });
            DropIndex("Web.StockProcessBalances", new[] { "Dimension4Id" });
            DropIndex("Web.StockProcessBalances", new[] { "ProcessId" });
            DropIndex("Web.StockProcessBalances", new[] { "GodownId" });
            DropIndex("Web.StockProcessBalances", new[] { "CostCenterId" });
            DropPrimaryKey("Web.ProductTypes");
            DropPrimaryKey("Web.ProductNatures");
            DropPrimaryKey("Web.LedgerAccounts");
            DropPrimaryKey("Web.LedgerAccountGroups");
            CreateTable(
                "Web.PersonProductUids",
                c => new
                    {
                        PersonProductUidId = c.Int(nullable: false, identity: true),
                        GenDocId = c.Int(),
                        GenDocNo = c.String(),
                        GenDocTypeId = c.Int(),
                        GenLineId = c.Int(),
                        ProductUidName = c.String(nullable: false, maxLength: 50),
                        ProductUidSpecification = c.String(),
                        SaleOrderLineId = c.Int(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.PersonProductUidId)
                .ForeignKey("Web.DocumentTypes", t => t.GenDocTypeId)
                .ForeignKey("Web.SaleOrderLines", t => t.SaleOrderLineId)
                .Index(t => t.GenDocTypeId)
                .Index(t => t.SaleOrderLineId);
            
            CreateTable(
                "Web.JobOrderBomMaterialIssues",
                c => new
                    {
                        JobOrderBomMaterialIssueId = c.Int(nullable: false, identity: true),
                        JobOrderBomId = c.Int(),
                        StockLineId = c.Int(nullable: false),
                        IssueForQty = c.Decimal(nullable: false, precision: 18, scale: 4),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 4),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.JobOrderBomMaterialIssueId)
                .ForeignKey("Web.JobOrderBoms", t => t.JobOrderBomId)
                .ForeignKey("Web.StockLines", t => t.StockLineId)
                .Index(t => t.JobOrderBomId)
                .Index(t => t.StockLineId);
            
            AddColumn("Web.DocumentTypes", "ActionName", c => c.String(maxLength: 50));
            AddColumn("Web.Divisions", "LogoBlob", c => c.String());
            AddColumn("Web.PackingLines", "PersonProductUidId", c => c.Int());
            AddColumn("Web.SaleInvoiceHeaderDetail", "FreightRemark", c => c.String());
            AddColumn("Web.SaleInvoiceHeaderDetail", "InsuranceRemark", c => c.String());
            AddColumn("Web.JobInvoiceHeaders", "GovtInvoiceNo", c => c.String(maxLength: 20));
            AddColumn("Web.JobInvoiceReturnHeaders", "SalesTaxGroupPersonId", c => c.Int());
            AddColumn("Web.JobInvoiceReturnLines", "SalesTaxGroupProductId", c => c.Int());
            AddColumn("Web.JobInvoiceSettings", "isVisibleGovtInvoiceNo", c => c.Boolean());
            AddColumn("Web.SaleEnquiryLineExtendeds", "BuyerSku", c => c.String());
            AddColumn("Web.SaleEnquiryLineExtendeds", "BuyerUpcCode", c => c.String());
            AddColumn("Web.SaleInvoiceSettings", "isVisiblePacking", c => c.Boolean());
            AlterColumn("Web.ProductTypes", "ProductTypeId", c => c.Int(nullable: false));
            AlterColumn("Web.ProductNatures", "ProductNatureId", c => c.Int(nullable: false));
            AlterColumn("Web.LedgerAccounts", "LedgerAccountId", c => c.Int(nullable: false, identity: true));
            AlterColumn("Web.LedgerAccountGroups", "LedgerAccountGroupId", c => c.Int(nullable: false, identity: true));
            AlterColumn("Web.JobReceiveHeaders", "JobReceiveById", c => c.Int());
            AlterColumn("Web.JobReceiveQAHeaders", "QAById", c => c.Int());
            AddPrimaryKey("Web.ProductTypes", "ProductTypeId");
            AddPrimaryKey("Web.ProductNatures", "ProductNatureId");
            AddPrimaryKey("Web.LedgerAccounts", "LedgerAccountId");
            AddPrimaryKey("Web.LedgerAccountGroups", "LedgerAccountGroupId");
            CreateIndex("Web.PackingLines", "PersonProductUidId");
            CreateIndex("Web.JobReceiveHeaders", "JobReceiveById");
            CreateIndex("Web.JobInvoiceReturnHeaders", "SalesTaxGroupPersonId");
            CreateIndex("Web.JobInvoiceReturnLines", "SalesTaxGroupProductId");
            CreateIndex("Web.JobReceiveQAHeaders", "QAById");
            AddForeignKey("Web.PackingLines", "PersonProductUidId", "Web.PersonProductUids", "PersonProductUidId");
            AddForeignKey("Web.JobInvoiceReturnHeaders", "SalesTaxGroupPersonId", "Web.ChargeGroupPersons", "ChargeGroupPersonId");
            AddForeignKey("Web.JobInvoiceReturnLines", "SalesTaxGroupProductId", "Web.ChargeGroupProducts", "ChargeGroupProductId");
            AddForeignKey("Web.ProductCategories", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductDesigns", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductGroups", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductTypeAttributes", "ProductType_ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension1", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension2", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension3", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension4", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductCollections", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductDesignPatterns", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductQualities", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.PromoCodes", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductTypeQaAttributes", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.FinishedProduct", "ProductShapeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductTypes", "ProductNatureId", "Web.ProductNatures", "ProductNatureId");
            AddForeignKey("Web.CostCenters", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.Processes", "AccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.StockHeaders", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.StockHeaders", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.LedgerHeaders", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationFooters", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationFooters", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationHeaderLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationProducts", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationProducts", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.ChargeGroupSettings", "ChargeLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.Ledgers", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.Ledgers", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.LedgerLines", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.TdsRates", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.LedgerAccounts", "LedgerAccountGroupId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PersonSettings", "LedgerAccountGroupId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            DropTable("Web.StockBalances");
            DropTable("Web.StockProcessBalances");
        }
        
        public override void Down()
        {
            CreateTable(
                "Web.StockProcessBalances",
                c => new
                    {
                        StockProcessBalanceId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        Dimension1Id = c.Int(),
                        Dimension2Id = c.Int(),
                        Dimension3Id = c.Int(),
                        Dimension4Id = c.Int(),
                        ProcessId = c.Int(),
                        GodownId = c.Int(),
                        CostCenterId = c.Int(),
                        LotNo = c.String(maxLength: 10),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 4),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.StockProcessBalanceId);
            
            CreateTable(
                "Web.StockBalances",
                c => new
                    {
                        StockBalanceId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        Dimension1Id = c.Int(),
                        Dimension2Id = c.Int(),
                        Dimension3Id = c.Int(),
                        Dimension4Id = c.Int(),
                        ProcessId = c.Int(),
                        GodownId = c.Int(nullable: false),
                        CostCenterId = c.Int(),
                        LotNo = c.String(maxLength: 50),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 4),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.StockBalanceId);
            
            DropForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.PersonSettings", "LedgerAccountGroupId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.LedgerAccounts", "LedgerAccountGroupId", "Web.LedgerAccountGroups");
            DropForeignKey("Web.TdsRates", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.PurchaseInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.LedgerLines", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.Ledgers", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.Ledgers", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.ChargeGroupSettings", "ChargeLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationLineLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationProducts", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationProducts", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationHeaderLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationFooters", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.CalculationFooters", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.LedgerHeaders", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.StockHeaders", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.StockHeaders", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.Processes", "AccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.CostCenters", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.ProductTypes", "ProductNatureId", "Web.ProductNatures");
            DropForeignKey("Web.FinishedProduct", "ProductShapeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductTypeQaAttributes", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.PromoCodes", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductQualities", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductDesignPatterns", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductCollections", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension4", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension3", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension2", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.Dimension1", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductTypeAttributes", "ProductType_ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductGroups", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductDesigns", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.ProductCategories", "ProductTypeId", "Web.ProductTypes");
            DropForeignKey("Web.JobOrderBomMaterialIssues", "StockLineId", "Web.StockLines");
            DropForeignKey("Web.JobOrderBomMaterialIssues", "JobOrderBomId", "Web.JobOrderBoms");
            DropForeignKey("Web.JobInvoiceReturnLines", "SalesTaxGroupProductId", "Web.ChargeGroupProducts");
            DropForeignKey("Web.JobInvoiceReturnHeaders", "SalesTaxGroupPersonId", "Web.ChargeGroupPersons");
            DropForeignKey("Web.PackingLines", "PersonProductUidId", "Web.PersonProductUids");
            DropForeignKey("Web.PersonProductUids", "SaleOrderLineId", "Web.SaleOrderLines");
            DropForeignKey("Web.PersonProductUids", "GenDocTypeId", "Web.DocumentTypes");
            DropIndex("Web.JobReceiveQAHeaders", new[] { "QAById" });
            DropIndex("Web.JobOrderBomMaterialIssues", new[] { "StockLineId" });
            DropIndex("Web.JobOrderBomMaterialIssues", new[] { "JobOrderBomId" });
            DropIndex("Web.JobInvoiceReturnLines", new[] { "SalesTaxGroupProductId" });
            DropIndex("Web.JobInvoiceReturnHeaders", new[] { "SalesTaxGroupPersonId" });
            DropIndex("Web.JobReceiveHeaders", new[] { "JobReceiveById" });
            DropIndex("Web.PersonProductUids", new[] { "SaleOrderLineId" });
            DropIndex("Web.PersonProductUids", new[] { "GenDocTypeId" });
            DropIndex("Web.PackingLines", new[] { "PersonProductUidId" });
            DropPrimaryKey("Web.LedgerAccountGroups");
            DropPrimaryKey("Web.LedgerAccounts");
            DropPrimaryKey("Web.ProductNatures");
            DropPrimaryKey("Web.ProductTypes");
            AlterColumn("Web.JobReceiveQAHeaders", "QAById", c => c.Int(nullable: false));
            AlterColumn("Web.JobReceiveHeaders", "JobReceiveById", c => c.Int(nullable: false));
            AlterColumn("Web.LedgerAccountGroups", "LedgerAccountGroupId", c => c.Int(nullable: false));
            AlterColumn("Web.LedgerAccounts", "LedgerAccountId", c => c.Int(nullable: false));
            AlterColumn("Web.ProductNatures", "ProductNatureId", c => c.Int(nullable: false, identity: true));
            AlterColumn("Web.ProductTypes", "ProductTypeId", c => c.Int(nullable: false, identity: true));
            DropColumn("Web.SaleInvoiceSettings", "isVisiblePacking");
            DropColumn("Web.SaleEnquiryLineExtendeds", "BuyerUpcCode");
            DropColumn("Web.SaleEnquiryLineExtendeds", "BuyerSku");
            DropColumn("Web.JobInvoiceSettings", "isVisibleGovtInvoiceNo");
            DropColumn("Web.JobInvoiceReturnLines", "SalesTaxGroupProductId");
            DropColumn("Web.JobInvoiceReturnHeaders", "SalesTaxGroupPersonId");
            DropColumn("Web.JobInvoiceHeaders", "GovtInvoiceNo");
            DropColumn("Web.SaleInvoiceHeaderDetail", "InsuranceRemark");
            DropColumn("Web.SaleInvoiceHeaderDetail", "FreightRemark");
            DropColumn("Web.PackingLines", "PersonProductUidId");
            DropColumn("Web.Divisions", "LogoBlob");
            DropColumn("Web.DocumentTypes", "ActionName");
            DropTable("Web.JobOrderBomMaterialIssues");
            DropTable("Web.PersonProductUids");
            AddPrimaryKey("Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddPrimaryKey("Web.LedgerAccounts", "LedgerAccountId");
            AddPrimaryKey("Web.ProductNatures", "ProductNatureId");
            AddPrimaryKey("Web.ProductTypes", "ProductTypeId");
            CreateIndex("Web.StockProcessBalances", "CostCenterId");
            CreateIndex("Web.StockProcessBalances", "GodownId");
            CreateIndex("Web.StockProcessBalances", "ProcessId");
            CreateIndex("Web.StockProcessBalances", "Dimension4Id");
            CreateIndex("Web.StockProcessBalances", "Dimension3Id");
            CreateIndex("Web.StockProcessBalances", "Dimension2Id");
            CreateIndex("Web.StockProcessBalances", "Dimension1Id");
            CreateIndex("Web.StockProcessBalances", "ProductId");
            CreateIndex("Web.StockBalances", "CostCenterId");
            CreateIndex("Web.StockBalances", "GodownId");
            CreateIndex("Web.StockBalances", "ProcessId");
            CreateIndex("Web.StockBalances", "Dimension4Id");
            CreateIndex("Web.StockBalances", "Dimension3Id");
            CreateIndex("Web.StockBalances", "Dimension2Id");
            CreateIndex("Web.StockBalances", "Dimension1Id");
            CreateIndex("Web.StockBalances", "ProductId");
            CreateIndex("Web.JobReceiveQAHeaders", "QAById");
            CreateIndex("Web.JobReceiveHeaders", "JobReceiveById");
            AddForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.PersonSettings", "LedgerAccountGroupId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobOrderLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.JobInvoiceLineCharges", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsDrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "filterLedgerAccountGroupsCrId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.LedgerAccounts", "LedgerAccountGroupId", "Web.LedgerAccountGroups", "LedgerAccountGroupId");
            AddForeignKey("Web.TdsRates", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.SaleInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseOrderAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.PurchaseInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.LedgerLines", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.Ledgers", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.Ledgers", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobOrderHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceReturnHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceRateAmendmentLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.JobInvoiceAmendmentHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.ChargeGroupSettings", "ChargeLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationLineLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationProducts", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationProducts", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationHeaderLedgerAccounts", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationHeaderLedgerAccounts", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationFooters", "LedgerAccountDrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CalculationFooters", "LedgerAccountCrId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.LedgerHeaders", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.StockHeaders", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.StockHeaders", "ContraLedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.Processes", "AccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.CostCenters", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.ProductTypes", "ProductNatureId", "Web.ProductNatures", "ProductNatureId");
            AddForeignKey("Web.FinishedProduct", "ProductShapeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductTypeQaAttributes", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.PromoCodes", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductQualities", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductDesignPatterns", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductCollections", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension4", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension3", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension2", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.Dimension1", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductTypeAttributes", "ProductType_ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductGroups", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductDesigns", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.ProductCategories", "ProductTypeId", "Web.ProductTypes", "ProductTypeId");
            AddForeignKey("Web.StockProcessBalances", "ProductId", "Web.Products", "ProductId");
            AddForeignKey("Web.StockProcessBalances", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.StockProcessBalances", "GodownId", "Web.Godowns", "GodownId");
            AddForeignKey("Web.StockProcessBalances", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.StockProcessBalances", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.StockProcessBalances", "Dimension2Id", "Web.Dimension2", "Dimension2Id");
            AddForeignKey("Web.StockProcessBalances", "Dimension1Id", "Web.Dimension1", "Dimension1Id");
            AddForeignKey("Web.StockProcessBalances", "CostCenterId", "Web.CostCenters", "CostCenterId");
            AddForeignKey("Web.StockBalances", "ProductId", "Web.Products", "ProductId");
            AddForeignKey("Web.StockBalances", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.StockBalances", "GodownId", "Web.Godowns", "GodownId");
            AddForeignKey("Web.StockBalances", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.StockBalances", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.StockBalances", "Dimension2Id", "Web.Dimension2", "Dimension2Id");
            AddForeignKey("Web.StockBalances", "Dimension1Id", "Web.Dimension1", "Dimension1Id");
            AddForeignKey("Web.StockBalances", "CostCenterId", "Web.CostCenters", "CostCenterId");
        }
    }
}
