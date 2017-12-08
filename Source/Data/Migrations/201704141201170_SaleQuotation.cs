namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SaleQuotation : DbMigration
    {
        public override void Up()
        {
            DropIndex("Web.JobOrderLines", "IX_JobOrderLine_Unique");
            DropIndex("Web.LedgerAdjs", new[] { "LedgerId" });
            CreateTable(
                "Web.DocumentShipMethods",
                c => new
                    {
                        DocumentShipMethodId = c.Int(nullable: false, identity: true),
                        DocumentShipMethodName = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.DocumentShipMethodId)
                .Index(t => t.DocumentShipMethodName, unique: true, name: "IX_DocumentShipMethod_DocumentShipMethodName");
            
            CreateTable(
                "Web.BinLocations",
                c => new
                    {
                        BinLocationId = c.Int(nullable: false, identity: true),
                        GodownId = c.Int(nullable: false),
                        BinLocationCode = c.String(),
                        BinLocationName = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BinLocationId)
                .ForeignKey("Web.Godowns", t => t.GodownId)
                .Index(t => t.GodownId)
                .Index(t => t.BinLocationName, unique: true, name: "IX_BinLocation_BinLocation");
            
            CreateTable(
                "Web.SaleQuotationHeaders",
                c => new
                    {
                        SaleQuotationHeaderId = c.Int(nullable: false, identity: true),
                        DocTypeId = c.Int(nullable: false),
                        DocDate = c.DateTime(nullable: false),
                        DocNo = c.String(maxLength: 20),
                        DivisionId = c.Int(nullable: false),
                        SiteId = c.Int(nullable: false),
                        ParentSaleQuotationHeaderId = c.Int(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                        ProcessId = c.Int(nullable: false),
                        CostCenterId = c.Int(),
                        SaleToBuyerId = c.Int(nullable: false),
                        CurrencyId = c.Int(nullable: false),
                        TermsAndConditions = c.String(),
                        Status = c.Int(nullable: false),
                        UnitConversionForId = c.Byte(nullable: false),
                        SalesTaxGroupPersonId = c.Int(),
                        Remark = c.String(),
                        LockReason = c.String(),
                        ReviewCount = c.Int(),
                        ReviewBy = c.String(),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        ModifiedDate = c.DateTime(nullable: false),
                        ReferenceDocTypeId = c.Int(),
                        ReferenceDocId = c.Int(),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.SaleQuotationHeaderId)
                .ForeignKey("Web.CostCenters", t => t.CostCenterId)
                .ForeignKey("Web.Currencies", t => t.CurrencyId)
                .ForeignKey("Web.Divisions", t => t.DivisionId)
                .ForeignKey("Web.DocumentTypes", t => t.DocTypeId)
                .ForeignKey("Web.SaleQuotationHeaders", t => t.ParentSaleQuotationHeaderId)
                .ForeignKey("Web.Processes", t => t.ProcessId)
                .ForeignKey("Web.DocumentTypes", t => t.ReferenceDocTypeId)
                .ForeignKey("Web.ChargeGroupPersons", t => t.SalesTaxGroupPersonId)
                .ForeignKey("Web.People", t => t.SaleToBuyerId)
                .ForeignKey("Web.Sites", t => t.SiteId)
                .ForeignKey("Web.UnitConversionFors", t => t.UnitConversionForId)
                .Index(t => t.DocTypeId)
                .Index(t => t.DivisionId)
                .Index(t => t.SiteId)
                .Index(t => t.ParentSaleQuotationHeaderId)
                .Index(t => t.ProcessId)
                .Index(t => t.CostCenterId)
                .Index(t => t.SaleToBuyerId)
                .Index(t => t.CurrencyId)
                .Index(t => t.UnitConversionForId)
                .Index(t => t.SalesTaxGroupPersonId)
                .Index(t => t.ReferenceDocTypeId);
            
            CreateTable(
                "Web.SaleQuotationHeaderCharges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HeaderTableId = c.Int(nullable: false),
                        Sr = c.Int(nullable: false),
                        ChargeId = c.Int(nullable: false),
                        AddDeduct = c.Byte(),
                        AffectCost = c.Boolean(nullable: false),
                        ChargeTypeId = c.Int(),
                        ProductChargeId = c.Int(),
                        CalculateOnId = c.Int(),
                        PersonID = c.Int(),
                        LedgerAccountDrId = c.Int(),
                        LedgerAccountCrId = c.Int(),
                        ContraLedgerAccountId = c.Int(),
                        CostCenterId = c.Int(),
                        RateType = c.Byte(nullable: false),
                        IncludedInBase = c.Boolean(nullable: false),
                        ParentChargeId = c.Int(),
                        Rate = c.Decimal(precision: 18, scale: 4),
                        Amount = c.Decimal(precision: 18, scale: 4),
                        IsVisible = c.Boolean(nullable: false),
                        IncludedCharges = c.String(),
                        IncludedChargesCalculation = c.String(),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Web.Charges", t => t.CalculateOnId)
                .ForeignKey("Web.Charges", t => t.ChargeId)
                .ForeignKey("Web.ChargeTypes", t => t.ChargeTypeId)
                .ForeignKey("Web.LedgerAccounts", t => t.ContraLedgerAccountId)
                .ForeignKey("Web.CostCenters", t => t.CostCenterId)
                .ForeignKey("Web.LedgerAccounts", t => t.LedgerAccountCrId)
                .ForeignKey("Web.LedgerAccounts", t => t.LedgerAccountDrId)
                .ForeignKey("Web.Charges", t => t.ParentChargeId)
                .ForeignKey("Web.People", t => t.PersonID)
                .ForeignKey("Web.Charges", t => t.ProductChargeId)
                .ForeignKey("Web.SaleQuotationHeaders", t => t.HeaderTableId)
                .Index(t => t.HeaderTableId)
                .Index(t => t.ChargeId)
                .Index(t => t.ChargeTypeId)
                .Index(t => t.ProductChargeId)
                .Index(t => t.CalculateOnId)
                .Index(t => t.PersonID)
                .Index(t => t.LedgerAccountDrId)
                .Index(t => t.LedgerAccountCrId)
                .Index(t => t.ContraLedgerAccountId)
                .Index(t => t.CostCenterId)
                .Index(t => t.ParentChargeId);
            
            CreateTable(
                "Web.SaleQuotationHeaderDetails",
                c => new
                    {
                        SaleQuotationHeaderId = c.Int(nullable: false, identity: true),
                        Priority = c.Int(nullable: false),
                        ShipMethodId = c.Int(nullable: false),
                        IsDoorDelivery = c.Boolean(),
                        DeliveryTermsId = c.Int(nullable: false),
                        CreditDays = c.Int(nullable: false),
                        FinancierId = c.Int(),
                        SalesExecutiveId = c.Int(),
                        AgentId = c.Int(),
                        PayTermAdvancePer = c.Decimal(precision: 18, scale: 4),
                        PayTermOnDeliveryPer = c.Decimal(precision: 18, scale: 4),
                        PayTermOnDueDatePer = c.Decimal(precision: 18, scale: 4),
                        PayTermCashPer = c.Decimal(precision: 18, scale: 4),
                        PayTermBankPer = c.Decimal(precision: 18, scale: 4),
                        PayTermDescription = c.String(),
                    })
                .PrimaryKey(t => t.SaleQuotationHeaderId)
                .ForeignKey("Web.People", t => t.AgentId)
                .ForeignKey("Web.DeliveryTerms", t => t.DeliveryTermsId)
                .ForeignKey("Web.People", t => t.FinancierId)
                .ForeignKey("Web.People", t => t.SalesExecutiveId)
                .ForeignKey("Web.ShipMethods", t => t.ShipMethodId)
                .Index(t => t.ShipMethodId)
                .Index(t => t.DeliveryTermsId)
                .Index(t => t.FinancierId)
                .Index(t => t.SalesExecutiveId)
                .Index(t => t.AgentId);
            
            CreateTable(
                "Web.SaleQuotationLines",
                c => new
                    {
                        SaleQuotationLineId = c.Int(nullable: false, identity: true),
                        SaleQuotationHeaderId = c.Int(nullable: false),
                        SaleEnquiryLineId = c.Int(),
                        ProductId = c.Int(nullable: false),
                        Dimension1Id = c.Int(),
                        Dimension2Id = c.Int(),
                        Dimension3Id = c.Int(),
                        Dimension4Id = c.Int(),
                        Specification = c.String(maxLength: 50),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 4),
                        DealUnitId = c.String(maxLength: 3),
                        DealQty = c.Decimal(nullable: false, precision: 18, scale: 4),
                        UnitConversionMultiplier = c.Decimal(precision: 18, scale: 4),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 4),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 4),
                        DiscountPer = c.Decimal(precision: 18, scale: 4),
                        ReferenceDocTypeId = c.Int(),
                        ReferenceDocLineId = c.Int(),
                        Sr = c.Int(nullable: false),
                        Remark = c.String(),
                        LockReason = c.String(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.SaleQuotationLineId)
                .ForeignKey("Web.Units", t => t.DealUnitId)
                .ForeignKey("Web.Dimension1", t => t.Dimension1Id)
                .ForeignKey("Web.Dimension2", t => t.Dimension2Id)
                .ForeignKey("Web.Dimension3", t => t.Dimension3Id)
                .ForeignKey("Web.Dimension4", t => t.Dimension4Id)
                .ForeignKey("Web.Products", t => t.ProductId)
                .ForeignKey("Web.DocumentTypes", t => t.ReferenceDocTypeId)
                .ForeignKey("Web.SaleEnquiryLines", t => t.SaleEnquiryLineId)
                .ForeignKey("Web.SaleQuotationHeaders", t => t.SaleQuotationHeaderId)
                .Index(t => new { t.SaleQuotationHeaderId, t.ProductId }, unique: true, name: "IX_SaleQuotationLine_SaleQuotationHeaderProductDueDate")
                .Index(t => t.SaleEnquiryLineId)
                .Index(t => t.Dimension1Id)
                .Index(t => t.Dimension2Id)
                .Index(t => t.Dimension3Id)
                .Index(t => t.Dimension4Id)
                .Index(t => t.DealUnitId)
                .Index(t => t.ReferenceDocTypeId);
            
            CreateTable(
                "Web.SaleQuotationLineCharges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LineTableId = c.Int(nullable: false),
                        HeaderTableId = c.Int(nullable: false),
                        Sr = c.Int(nullable: false),
                        ChargeId = c.Int(nullable: false),
                        AddDeduct = c.Byte(),
                        AffectCost = c.Boolean(nullable: false),
                        ChargeTypeId = c.Int(),
                        CalculateOnId = c.Int(),
                        PersonID = c.Int(),
                        LedgerAccountDrId = c.Int(),
                        LedgerAccountCrId = c.Int(),
                        ContraLedgerAccountId = c.Int(),
                        CostCenterId = c.Int(),
                        RateType = c.Byte(nullable: false),
                        IncludedInBase = c.Boolean(nullable: false),
                        ParentChargeId = c.Int(),
                        Rate = c.Decimal(precision: 18, scale: 4),
                        Amount = c.Decimal(precision: 18, scale: 4),
                        DealQty = c.Decimal(precision: 18, scale: 4),
                        IsVisible = c.Boolean(nullable: false),
                        IncludedCharges = c.String(),
                        IncludedChargesCalculation = c.String(),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Web.Charges", t => t.CalculateOnId)
                .ForeignKey("Web.Charges", t => t.ChargeId)
                .ForeignKey("Web.ChargeTypes", t => t.ChargeTypeId)
                .ForeignKey("Web.LedgerAccounts", t => t.ContraLedgerAccountId)
                .ForeignKey("Web.CostCenters", t => t.CostCenterId)
                .ForeignKey("Web.LedgerAccounts", t => t.LedgerAccountCrId)
                .ForeignKey("Web.LedgerAccounts", t => t.LedgerAccountDrId)
                .ForeignKey("Web.Charges", t => t.ParentChargeId)
                .ForeignKey("Web.People", t => t.PersonID)
                .ForeignKey("Web.SaleQuotationLines", t => t.LineTableId)
                .Index(t => t.LineTableId)
                .Index(t => t.ChargeId)
                .Index(t => t.ChargeTypeId)
                .Index(t => t.CalculateOnId)
                .Index(t => t.PersonID)
                .Index(t => t.LedgerAccountDrId)
                .Index(t => t.LedgerAccountCrId)
                .Index(t => t.ContraLedgerAccountId)
                .Index(t => t.CostCenterId)
                .Index(t => t.ParentChargeId);
            
            CreateTable(
                "Web.SaleQuotationSettings",
                c => new
                    {
                        SaleQuotationSettingsId = c.Int(nullable: false, identity: true),
                        DocTypeId = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        ShipMethodId = c.Int(nullable: false),
                        CurrencyId = c.Int(nullable: false),
                        DeliveryTermsId = c.Int(nullable: false),
                        SiteId = c.Int(nullable: false),
                        DivisionId = c.Int(nullable: false),
                        UnitConversionForId = c.Byte(nullable: false),
                        ProcessId = c.Int(nullable: false),
                        TermsAndConditions = c.String(),
                        isVisibleCurrency = c.Boolean(),
                        isVisibleShipMethod = c.Boolean(),
                        isVisibleSalesTaxGroupPerson = c.Boolean(),
                        isVisibleDoorDelivery = c.Boolean(),
                        isVisibleCreditDays = c.Boolean(),
                        isVisibleCostCenter = c.Boolean(),
                        isVisibleDeliveryTerms = c.Boolean(),
                        isVisiblePriority = c.Boolean(),
                        isVisibleDimension1 = c.Boolean(),
                        isVisibleDimension2 = c.Boolean(),
                        isVisibleDimension3 = c.Boolean(),
                        isVisibleDimension4 = c.Boolean(),
                        isVisibleDealUnit = c.Boolean(),
                        isVisibleSpecification = c.Boolean(),
                        isVisibleProductCode = c.Boolean(),
                        isVisibleUnitConversionFor = c.Boolean(),
                        isVisibleFinancier = c.Boolean(),
                        isVisibleSalesExecutive = c.Boolean(),
                        isVisiblePaymentTerms = c.Boolean(),
                        isUniqueCostCenter = c.Boolean(),
                        IsPersonWiseCostCenter = c.Boolean(),
                        isVisibleFromSaleEnquiry = c.Boolean(nullable: false),
                        isVisibleAgent = c.Boolean(nullable: false),
                        filterLedgerAccountGroups = c.String(),
                        filterLedgerAccounts = c.String(),
                        filterProductTypes = c.String(),
                        filterProductGroups = c.String(),
                        filterProducts = c.String(),
                        filterPersonRoles = c.String(),
                        filterContraDocTypes = c.String(),
                        filterContraSites = c.String(),
                        filterContraDivisions = c.String(),
                        FilterProductDivision = c.String(),
                        filterProductCategories = c.String(),
                        DocumentPrint = c.String(maxLength: 100),
                        NoOfPrintCopies = c.Int(),
                        DealUnitId = c.String(maxLength: 3),
                        SqlProcDocumentPrint = c.String(maxLength: 100),
                        SqlProcDocumentPrint_AfterSubmit = c.String(maxLength: 100),
                        SqlProcDocumentPrint_AfterApprove = c.String(maxLength: 100),
                        ImportMenuId = c.Int(),
                        WizardMenuId = c.Int(),
                        ExportMenuId = c.Int(),
                        CalculationId = c.Int(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        OMSId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.SaleQuotationSettingsId)
                .ForeignKey("Web.Calculations", t => t.CalculationId)
                .ForeignKey("Web.Currencies", t => t.CurrencyId)
                .ForeignKey("Web.Units", t => t.DealUnitId)
                .ForeignKey("Web.DeliveryTerms", t => t.DeliveryTermsId)
                .ForeignKey("Web.Divisions", t => t.DivisionId)
                .ForeignKey("Web.DocumentTypes", t => t.DocTypeId)
                .ForeignKey("Web.Menus", t => t.ExportMenuId)
                .ForeignKey("Web.Menus", t => t.ImportMenuId)
                .ForeignKey("Web.Processes", t => t.ProcessId)
                .ForeignKey("Web.ShipMethods", t => t.ShipMethodId)
                .ForeignKey("Web.Sites", t => t.SiteId)
                .ForeignKey("Web.UnitConversionFors", t => t.UnitConversionForId)
                .ForeignKey("Web.Menus", t => t.WizardMenuId)
                .Index(t => t.DocTypeId)
                .Index(t => t.ShipMethodId)
                .Index(t => t.CurrencyId)
                .Index(t => t.DeliveryTermsId)
                .Index(t => t.SiteId)
                .Index(t => t.DivisionId)
                .Index(t => t.UnitConversionForId)
                .Index(t => t.ProcessId)
                .Index(t => t.DealUnitId)
                .Index(t => t.ImportMenuId)
                .Index(t => t.WizardMenuId)
                .Index(t => t.ExportMenuId)
                .Index(t => t.CalculationId);
            
            CreateTable(
                "Web.ViewSizeinCms",
                c => new
                    {
                        SizeId = c.Int(nullable: false, identity: true),
                        SizeName = c.String(),
                        Length = c.Decimal(precision: 18, scale: 4),
                        Width = c.Decimal(precision: 18, scale: 4),
                        Area = c.Decimal(precision: 18, scale: 4),
                    })
                .PrimaryKey(t => t.SizeId);
            
            AddColumn("Web.Products", "SaleRate", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.Products", "ProfitMargin", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.Products", "CarryingCost", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.JobOrderBoms", "Dimension3Id", c => c.Int());
            AddColumn("Web.JobOrderBoms", "Dimension4Id", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "FinancierId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "SalesExecutiveId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "DeliveryTermsId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "ShipToAddressId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "CurrencyId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "SalesTaxGroupPersonId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "ShipMethodId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "DocumentShipMethodId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "TransporterId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "IsDoorDelivery", c => c.Boolean());
            AddColumn("Web.JobOrderHeaders", "AgentId", c => c.Int());
            AddColumn("Web.JobOrderHeaders", "PayTermAdvancePer", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.JobOrderHeaders", "PayTermOnDeliveryPer", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.JobOrderHeaders", "PayTermOnDueDatePer", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.JobOrderHeaders", "PayTermCashPer", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.JobOrderHeaders", "PayTermBankPer", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.JobOrderLines", "Dimension3Id", c => c.Int());
            AddColumn("Web.JobOrderLines", "Dimension4Id", c => c.Int());
            AddColumn("Web.ProdOrderLines", "DueDate", c => c.DateTime());
            AddColumn("Web.ProductUids", "ProductUidSpecification", c => c.String());
            AddColumn("Web.ProductUids", "Dimension3Id", c => c.Int());
            AddColumn("Web.ProductUids", "Dimension4Id", c => c.Int());
            AddColumn("Web.PackingLines", "SealNo", c => c.String());
            AddColumn("Web.PackingLines", "RateRemark", c => c.String());
            AddColumn("Web.PackingLines", "SaleQuotationLine_SaleQuotationLineId", c => c.Int());
            AddColumn("Web.LedgerHeaders", "ReferenceDocTypeId", c => c.Int());
            AddColumn("Web.LedgerHeaders", "ReferenceDocId", c => c.Int());
            AddColumn("Web.SaleOrderHeaders", "FinancierId", c => c.Int());
            AddColumn("Web.SaleOrderHeaders", "SalesExecutiveId", c => c.Int());
            AddColumn("Web.ProductUidHeaders", "Dimension3Id", c => c.Int());
            AddColumn("Web.ProductUidHeaders", "Dimension4Id", c => c.Int());
            AddColumn("Web.SaleInvoiceHeaders", "FinancierId", c => c.Int());
            AddColumn("Web.SaleInvoiceHeaders", "SalesExecutiveId", c => c.Int());
            AddColumn("Web.SaleInvoiceHeaderDetail", "Insurance", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.SaleInvoiceLines", "RateRemark", c => c.String());
            AddColumn("Web.DocumentTypeSettings", "CostCenterCaption", c => c.String(maxLength: 50));
            AddColumn("Web.DocumentTypeSettings", "SpecificationCaption", c => c.String(maxLength: 50));
            AddColumn("Web.ExcessMaterialSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.JobInvoiceHeaders", "FinancierId", c => c.Int());
            AddColumn("Web.JobInvoiceLines", "RateDiscountPer", c => c.Decimal(precision: 18, scale: 4));
            AddColumn("Web.JobInvoiceLines", "MfgDate", c => c.DateTime());
            AddColumn("Web.JobInvoiceSettings", "isVisibleIncentive", c => c.Boolean());
            AddColumn("Web.JobInvoiceSettings", "isVisiblePenalty", c => c.Boolean());
            AddColumn("Web.JobInvoiceSettings", "isVisibleRateDiscountPer", c => c.Boolean());
            AddColumn("Web.JobInvoiceSettings", "isVisibleFinancier", c => c.Boolean());
            AddColumn("Web.JobInvoiceSettings", "isVisibleMfgDate", c => c.Boolean());
            AddColumn("Web.JobInvoiceSettings", "isLedgerPostingLineWise", c => c.Boolean());
            AddColumn("Web.JobInvoiceSettings", "isGenerateProductUid", c => c.Boolean());
            AddColumn("Web.JobInvoiceSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.JobInvoiceSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.JobOrderCancelBoms", "Dimension3Id", c => c.Int());
            AddColumn("Web.JobOrderCancelBoms", "Dimension4Id", c => c.Int());
            AddColumn("Web.JobOrderInspectionRequestSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.JobOrderInspectionRequestSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.JobOrderInspectionSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.JobOrderInspectionSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.JobOrderSettings", "isVisibleDeliveryTerms", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleShipToAddress", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleCurrency", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleSalesTaxGroupPerson", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleShipMethod", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleDocumentShipMethod", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleTransporter", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleAgent", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleDoorDelivery", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisiblePaymentTerms", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleFinancier", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "isVisibleSalesExecutive", c => c.Boolean());
            AddColumn("Web.JobOrderSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.JobOrderSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.JobOrderSettings", "NonCountedQtyCaption", c => c.String(maxLength: 50));
            AddColumn("Web.JobOrderSettings", "TermsAndConditions", c => c.String());
            AddColumn("Web.JobReceiveBoms", "Dimension3Id", c => c.Int());
            AddColumn("Web.JobReceiveBoms", "Dimension4Id", c => c.Int());
            AddColumn("Web.JobReceiveByProducts", "Dimension3Id", c => c.Int());
            AddColumn("Web.JobReceiveByProducts", "Dimension4Id", c => c.Int());
            AddColumn("Web.JobReceiveQASettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.JobReceiveQASettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.JobReceiveSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.JobReceiveSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.JobReceiveSettings", "ConsumptionDimension3Caption", c => c.String(maxLength: 50));
            AddColumn("Web.JobReceiveSettings", "ConsumptionDimension4Caption", c => c.String(maxLength: 50));
            AddColumn("Web.JobReceiveSettings", "ByProductDimension3Caption", c => c.String(maxLength: 50));
            AddColumn("Web.JobReceiveSettings", "ByProductDimension4Caption", c => c.String(maxLength: 50));
            AddColumn("Web.JobReceiveSettings", "isVisibleConsumptionDimension3", c => c.Boolean());
            AddColumn("Web.JobReceiveSettings", "isVisibleConsumptionDimension4", c => c.Boolean());
            AddColumn("Web.JobReceiveSettings", "isVisibleByProductDimension3", c => c.Boolean());
            AddColumn("Web.JobReceiveSettings", "isVisibleByProductDimension4", c => c.Boolean());
            AddColumn("Web.Ledgers", "ReferenceDocTypeId", c => c.Int());
            AddColumn("Web.Ledgers", "ReferenceDocLineId", c => c.Int());
            AddColumn("Web.StockHeaderSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.StockHeaderSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.MaterialPlanSettings", "isVisibleProdPlanQty", c => c.Boolean());
            AddColumn("Web.MaterialPlanSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.MaterialPlanSettings", "ImportMenuId", c => c.Int());
            AddColumn("Web.MaterialPlanSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.MaterialPlanSettings", "DocumentPrintReportHeaderId", c => c.Int());
            AddColumn("Web.MaterialReceiveSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.MaterialRequestSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.MaterialRequestSettings", "ProcessId", c => c.Int());
            AddColumn("Web.MaterialRequestSettings", "ImportMenuId", c => c.Int());
            AddColumn("Web.MaterialRequestSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.PackingSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.ProdOrderSettings", "isVisibleBuyer", c => c.Boolean());
            AddColumn("Web.ProdOrderSettings", "isVisibleLineDueDate", c => c.Boolean());
            AddColumn("Web.ProdOrderSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.ProdOrderSettings", "ImportMenuId", c => c.Int());
            AddColumn("Web.ProdOrderSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.ProdOrderSettings", "WizardMenuId", c => c.Int());
            AddColumn("Web.ProductionOrderSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.ProductionOrderSettings", "ImportMenuId", c => c.Int());
            AddColumn("Web.ProductionOrderSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.ProductionOrderSettings", "WizardMenuId", c => c.Int());
            AddColumn("Web.ProductSiteDetails", "BinLocationId", c => c.Int());
            AddColumn("Web.ProductTypeSettings", "isVisibleProductDescription", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleProductSpecification", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleProductCategory", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleSalesTaxGroup", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleSaleRate", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleStandardCost", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleTags", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleMinimumOrderQty", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleReOrderLevel", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleGodownId", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleBinLocationId", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleProfitMargin", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleCarryingCost", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleLotManagement", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleConsumptionDetail", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "isVisibleProductProcessDetail", c => c.Boolean());
            AddColumn("Web.ProductTypeSettings", "ProductNameCaption", c => c.String());
            AddColumn("Web.ProductTypeSettings", "ProductCodeCaption", c => c.String());
            AddColumn("Web.ProductTypeSettings", "ProductDescriptionCaption", c => c.String());
            AddColumn("Web.ProductTypeSettings", "ProductSpecificationCaption", c => c.String());
            AddColumn("Web.ProductTypeSettings", "ProductGroupCaption", c => c.String());
            AddColumn("Web.ProductTypeSettings", "ProductCategoryCaption", c => c.String());
            AddColumn("Web.PurchaseGoodsReceiptSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.PurchaseGoodsReceiptSettings", "ProcessId", c => c.Int());
            AddColumn("Web.PurchaseInvoiceSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.PurchaseInvoiceSettings", "ProcessId", c => c.Int());
            AddColumn("Web.PurchaseOrderSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.PurchaseOrderSettings", "ProcessId", c => c.Int());
            AddColumn("Web.RateConversionSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.RequisitionSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.RequisitionSettings", "ProcessId", c => c.Int());
            AddColumn("Web.RequisitionSettings", "ImportMenuId", c => c.Int());
            AddColumn("Web.RequisitionSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.SaleDeliveryOrderSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.SaleDeliveryOrderSettings", "ProcessId", c => c.Int());
            AddColumn("Web.SaleDispatchSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.SaleDispatchSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.SaleEnquiryLines", "Dimension3Id", c => c.Int());
            AddColumn("Web.SaleEnquiryLines", "Dimension4Id", c => c.Int());
            AddColumn("Web.SaleEnquirySettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.SaleInvoiceSettings", "isVisibleFinancier", c => c.Boolean());
            AddColumn("Web.SaleInvoiceSettings", "isVisibleSalesExecutive", c => c.Boolean());
            AddColumn("Web.SaleInvoiceSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.SaleInvoiceSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.SaleOrderSettings", "isVisibleFinancier", c => c.Boolean());
            AddColumn("Web.SaleOrderSettings", "isVisibleSalesExecutive", c => c.Boolean());
            AddColumn("Web.SaleOrderSettings", "isMandatoryRate", c => c.Boolean());
            AddColumn("Web.SaleOrderSettings", "filterPersonRoles", c => c.String());
            AddColumn("Web.SaleOrderSettings", "ExportMenuId", c => c.Int());
            AddColumn("Web.TdsRates", "LedgerAccountId", c => c.Int());
            AddColumn("Web.ViewJobOrderBalance", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalance", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceForInspection", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceForInspection", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceForInvoice", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceForInvoice", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceFromStatus", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobOrderBalanceFromStatus", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobOrderInspectionRequestBalance", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobOrderInspectionRequestBalance", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobReceiveBalance", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobReceiveBalance", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobReceiveBalanceForInvoice", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobReceiveBalanceForInvoice", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewJobReceiveBalanceForQA", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewJobReceiveBalanceForQA", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewProdOrderBalance", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewProdOrderBalance", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewSaleEnquiryBalance", "Dimension3Id", c => c.Int());
            AddColumn("Web.ViewSaleEnquiryBalance", "Dimension4Id", c => c.Int());
            AddColumn("Web.ViewSaleEnquiryBalance", "DocTypeId", c => c.Int());
            AddColumn("Web.ViewSaleEnquiryBalance", "DocType_DocumentTypeId", c => c.Int());
            AddColumn("Web.WeavingRetensions", "PersonId", c => c.Int());
            AlterColumn("Web.LedgerAdjs", "LedgerId", c => c.Int());
            CreateIndex("Web.JobOrderBoms", "Dimension3Id");
            CreateIndex("Web.JobOrderBoms", "Dimension4Id");
            CreateIndex("Web.JobOrderHeaders", "FinancierId");
            CreateIndex("Web.JobOrderHeaders", "SalesExecutiveId");
            CreateIndex("Web.JobOrderHeaders", "DeliveryTermsId");
            CreateIndex("Web.JobOrderHeaders", "ShipToAddressId");
            CreateIndex("Web.JobOrderHeaders", "CurrencyId");
            CreateIndex("Web.JobOrderHeaders", "SalesTaxGroupPersonId");
            CreateIndex("Web.JobOrderHeaders", "ShipMethodId");
            CreateIndex("Web.JobOrderHeaders", "DocumentShipMethodId");
            CreateIndex("Web.JobOrderHeaders", "TransporterId");
            CreateIndex("Web.JobOrderHeaders", "AgentId");
            CreateIndex("Web.JobOrderLines", new[] { "JobOrderHeaderId", "ProductUidId", "ProductId", "Dimension1Id", "Dimension2Id", "Dimension3Id", "Dimension4Id", "ProdOrderLineId", "Specification", "LotNo", "FromProcessId" }, unique: true, name: "IX_JobOrderLine_Unique");
            CreateIndex("Web.ProductUids", "Dimension3Id");
            CreateIndex("Web.ProductUids", "Dimension4Id");
            CreateIndex("Web.PackingLines", "SaleQuotationLine_SaleQuotationLineId");
            CreateIndex("Web.LedgerHeaders", "ReferenceDocTypeId");
            CreateIndex("Web.SaleOrderHeaders", "FinancierId");
            CreateIndex("Web.SaleOrderHeaders", "SalesExecutiveId");
            CreateIndex("Web.ProductUidHeaders", "Dimension3Id");
            CreateIndex("Web.ProductUidHeaders", "Dimension4Id");
            CreateIndex("Web.SaleInvoiceHeaders", "FinancierId");
            CreateIndex("Web.SaleInvoiceHeaders", "SalesExecutiveId");
            CreateIndex("Web.ExcessMaterialSettings", "ExportMenuId");
            CreateIndex("Web.JobInvoiceHeaders", "FinancierId");
            CreateIndex("Web.JobInvoiceSettings", "ExportMenuId");
            CreateIndex("Web.JobOrderCancelBoms", "Dimension3Id");
            CreateIndex("Web.JobOrderCancelBoms", "Dimension4Id");
            CreateIndex("Web.JobOrderInspectionRequestSettings", "ExportMenuId");
            CreateIndex("Web.JobOrderInspectionSettings", "ExportMenuId");
            CreateIndex("Web.JobOrderSettings", "ExportMenuId");
            CreateIndex("Web.JobReceiveBoms", "Dimension3Id");
            CreateIndex("Web.JobReceiveBoms", "Dimension4Id");
            CreateIndex("Web.JobReceiveByProducts", "Dimension3Id");
            CreateIndex("Web.JobReceiveByProducts", "Dimension4Id");
            CreateIndex("Web.JobReceiveQASettings", "ExportMenuId");
            CreateIndex("Web.JobReceiveSettings", "ExportMenuId");
            CreateIndex("Web.Ledgers", "ReferenceDocTypeId");
            CreateIndex("Web.LedgerAdjs", "LedgerId");
            CreateIndex("Web.StockHeaderSettings", "ExportMenuId");
            CreateIndex("Web.MaterialPlanSettings", "ImportMenuId");
            CreateIndex("Web.MaterialPlanSettings", "ExportMenuId");
            CreateIndex("Web.MaterialRequestSettings", "ProcessId");
            CreateIndex("Web.MaterialRequestSettings", "ImportMenuId");
            CreateIndex("Web.MaterialRequestSettings", "ExportMenuId");
            CreateIndex("Web.ProdOrderSettings", "ImportMenuId");
            CreateIndex("Web.ProdOrderSettings", "ExportMenuId");
            CreateIndex("Web.ProdOrderSettings", "WizardMenuId");
            CreateIndex("Web.ProductionOrderSettings", "ImportMenuId");
            CreateIndex("Web.ProductionOrderSettings", "ExportMenuId");
            CreateIndex("Web.ProductionOrderSettings", "WizardMenuId");
            CreateIndex("Web.ProductSiteDetails", "BinLocationId");
            CreateIndex("Web.PurchaseGoodsReceiptSettings", "ProcessId");
            CreateIndex("Web.PurchaseInvoiceSettings", "ProcessId");
            CreateIndex("Web.PurchaseOrderSettings", "ProcessId");
            CreateIndex("Web.RequisitionSettings", "ProcessId");
            CreateIndex("Web.RequisitionSettings", "ImportMenuId");
            CreateIndex("Web.RequisitionSettings", "ExportMenuId");
            CreateIndex("Web.SaleDeliveryOrderSettings", "ProcessId");
            CreateIndex("Web.SaleDispatchSettings", "ExportMenuId");
            CreateIndex("Web.SaleEnquiryLines", "Dimension3Id");
            CreateIndex("Web.SaleEnquiryLines", "Dimension4Id");
            CreateIndex("Web.SaleInvoiceSettings", "ExportMenuId");
            CreateIndex("Web.SaleOrderSettings", "ExportMenuId");
            CreateIndex("Web.TdsRates", "LedgerAccountId");
            CreateIndex("Web.ViewJobOrderBalance", "Dimension3Id");
            CreateIndex("Web.ViewJobOrderBalance", "Dimension4Id");
            CreateIndex("Web.ViewJobOrderBalanceForInspection", "Dimension3Id");
            CreateIndex("Web.ViewJobOrderBalanceForInspection", "Dimension4Id");
            CreateIndex("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension3Id");
            CreateIndex("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension4Id");
            CreateIndex("Web.ViewJobOrderBalanceForInvoice", "Dimension3Id");
            CreateIndex("Web.ViewJobOrderBalanceForInvoice", "Dimension4Id");
            CreateIndex("Web.ViewJobOrderBalanceFromStatus", "Dimension3Id");
            CreateIndex("Web.ViewJobOrderBalanceFromStatus", "Dimension4Id");
            CreateIndex("Web.ViewJobOrderInspectionRequestBalance", "Dimension3Id");
            CreateIndex("Web.ViewJobOrderInspectionRequestBalance", "Dimension4Id");
            CreateIndex("Web.ViewJobReceiveBalance", "Dimension3Id");
            CreateIndex("Web.ViewJobReceiveBalance", "Dimension4Id");
            CreateIndex("Web.ViewJobReceiveBalanceForInvoice", "Dimension3Id");
            CreateIndex("Web.ViewJobReceiveBalanceForInvoice", "Dimension4Id");
            CreateIndex("Web.ViewProdOrderBalance", "Dimension3Id");
            CreateIndex("Web.ViewProdOrderBalance", "Dimension4Id");
            CreateIndex("Web.ViewSaleEnquiryBalance", "Dimension3Id");
            CreateIndex("Web.ViewSaleEnquiryBalance", "Dimension4Id");
            CreateIndex("Web.ViewSaleEnquiryBalance", "DocType_DocumentTypeId");
            CreateIndex("Web.WeavingRetensions", "PersonId");
            AddForeignKey("Web.JobOrderBoms", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.JobOrderBoms", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.JobOrderHeaders", "AgentId", "Web.People", "PersonID");
            AddForeignKey("Web.JobOrderHeaders", "CurrencyId", "Web.Currencies", "ID");
            AddForeignKey("Web.JobOrderHeaders", "DeliveryTermsId", "Web.DeliveryTerms", "DeliveryTermsId");
            AddForeignKey("Web.JobOrderHeaders", "DocumentShipMethodId", "Web.DocumentShipMethods", "DocumentShipMethodId");
            AddForeignKey("Web.JobOrderHeaders", "FinancierId", "Web.People", "PersonID");
            AddForeignKey("Web.JobOrderLines", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.JobOrderLines", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ProductUids", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ProductUids", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.LedgerHeaders", "ReferenceDocTypeId", "Web.DocumentTypes", "DocumentTypeId");
            AddForeignKey("Web.SaleOrderHeaders", "FinancierId", "Web.People", "PersonID");
            AddForeignKey("Web.SaleOrderHeaders", "SalesExecutiveId", "Web.People", "PersonID");
            AddForeignKey("Web.ProductUidHeaders", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ProductUidHeaders", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.JobOrderHeaders", "SalesExecutiveId", "Web.People", "PersonID");
            AddForeignKey("Web.JobOrderHeaders", "SalesTaxGroupPersonId", "Web.ChargeGroupPersons", "ChargeGroupPersonId");
            AddForeignKey("Web.JobOrderHeaders", "ShipMethodId", "Web.ShipMethods", "ShipMethodId");
            AddForeignKey("Web.JobOrderHeaders", "ShipToAddressId", "Web.PersonAddresses", "PersonAddressID");
            AddForeignKey("Web.JobOrderHeaders", "TransporterId", "Web.People", "PersonID");
            AddForeignKey("Web.SaleInvoiceHeaders", "FinancierId", "Web.People", "PersonID");
            AddForeignKey("Web.SaleInvoiceHeaders", "SalesExecutiveId", "Web.People", "PersonID");
            AddForeignKey("Web.ExcessMaterialSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.JobInvoiceHeaders", "FinancierId", "Web.People", "PersonID");
            AddForeignKey("Web.JobInvoiceSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.JobOrderCancelBoms", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.JobOrderCancelBoms", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.JobOrderInspectionRequestSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.JobOrderInspectionSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.JobOrderSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.JobReceiveBoms", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.JobReceiveBoms", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.JobReceiveByProducts", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.JobReceiveByProducts", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.JobReceiveQASettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.JobReceiveSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.Ledgers", "ReferenceDocTypeId", "Web.DocumentTypes", "DocumentTypeId");
            AddForeignKey("Web.StockHeaderSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.MaterialPlanSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.MaterialPlanSettings", "ImportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.MaterialRequestSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.MaterialRequestSettings", "ImportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.MaterialRequestSettings", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.ProdOrderSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.ProdOrderSettings", "ImportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.ProdOrderSettings", "WizardMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.ProductionOrderSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.ProductionOrderSettings", "ImportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.ProductionOrderSettings", "WizardMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.ProductSiteDetails", "BinLocationId", "Web.BinLocations", "BinLocationId");
            AddForeignKey("Web.PurchaseGoodsReceiptSettings", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.PurchaseInvoiceSettings", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.PurchaseOrderSettings", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.RequisitionSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.RequisitionSettings", "ImportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.RequisitionSettings", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.SaleDeliveryOrderSettings", "ProcessId", "Web.Processes", "ProcessId");
            AddForeignKey("Web.SaleDispatchSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.SaleEnquiryLines", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.SaleEnquiryLines", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.SaleInvoiceSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.SaleOrderSettings", "ExportMenuId", "Web.Menus", "MenuId");
            AddForeignKey("Web.PackingLines", "SaleQuotationLine_SaleQuotationLineId", "Web.SaleQuotationLines", "SaleQuotationLineId");
            AddForeignKey("Web.TdsRates", "LedgerAccountId", "Web.LedgerAccounts", "LedgerAccountId");
            AddForeignKey("Web.ViewJobOrderBalance", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobOrderBalance", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewJobOrderBalanceForInspection", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobOrderBalanceForInspection", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewJobOrderBalanceForInvoice", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobOrderBalanceForInvoice", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewJobOrderBalanceFromStatus", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobOrderBalanceFromStatus", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewJobOrderInspectionRequestBalance", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobOrderInspectionRequestBalance", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewJobReceiveBalance", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobReceiveBalance", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewJobReceiveBalanceForInvoice", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewJobReceiveBalanceForInvoice", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewProdOrderBalance", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewProdOrderBalance", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewSaleEnquiryBalance", "Dimension3Id", "Web.Dimension3", "Dimension3Id");
            AddForeignKey("Web.ViewSaleEnquiryBalance", "Dimension4Id", "Web.Dimension4", "Dimension4Id");
            AddForeignKey("Web.ViewSaleEnquiryBalance", "DocType_DocumentTypeId", "Web.DocumentTypes", "DocumentTypeId");
            AddForeignKey("Web.WeavingRetensions", "PersonId", "Web.People", "PersonID");
            DropColumn("Web.ProductSiteDetails", "BinLocation");
        }
        
        public override void Down()
        {
            AddColumn("Web.ProductSiteDetails", "BinLocation", c => c.String(maxLength: 20));
            DropForeignKey("Web.WeavingRetensions", "PersonId", "Web.People");
            DropForeignKey("Web.ViewSaleEnquiryBalance", "DocType_DocumentTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.ViewSaleEnquiryBalance", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewSaleEnquiryBalance", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewProdOrderBalance", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewProdOrderBalance", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobReceiveBalanceForInvoice", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobReceiveBalanceForInvoice", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobReceiveBalance", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobReceiveBalance", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobOrderInspectionRequestBalance", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobOrderInspectionRequestBalance", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobOrderBalanceFromStatus", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobOrderBalanceFromStatus", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobOrderBalanceForInvoice", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobOrderBalanceForInvoice", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobOrderBalanceForInspection", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobOrderBalanceForInspection", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.ViewJobOrderBalance", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ViewJobOrderBalance", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.TdsRates", "LedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationSettings", "WizardMenuId", "Web.Menus");
            DropForeignKey("Web.SaleQuotationSettings", "UnitConversionForId", "Web.UnitConversionFors");
            DropForeignKey("Web.SaleQuotationSettings", "SiteId", "Web.Sites");
            DropForeignKey("Web.SaleQuotationSettings", "ShipMethodId", "Web.ShipMethods");
            DropForeignKey("Web.SaleQuotationSettings", "ProcessId", "Web.Processes");
            DropForeignKey("Web.SaleQuotationSettings", "ImportMenuId", "Web.Menus");
            DropForeignKey("Web.SaleQuotationSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.SaleQuotationSettings", "DocTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.SaleQuotationSettings", "DivisionId", "Web.Divisions");
            DropForeignKey("Web.SaleQuotationSettings", "DeliveryTermsId", "Web.DeliveryTerms");
            DropForeignKey("Web.SaleQuotationSettings", "DealUnitId", "Web.Units");
            DropForeignKey("Web.SaleQuotationSettings", "CurrencyId", "Web.Currencies");
            DropForeignKey("Web.SaleQuotationSettings", "CalculationId", "Web.Calculations");
            DropForeignKey("Web.SaleQuotationLineCharges", "LineTableId", "Web.SaleQuotationLines");
            DropForeignKey("Web.SaleQuotationLineCharges", "PersonID", "Web.People");
            DropForeignKey("Web.SaleQuotationLineCharges", "ParentChargeId", "Web.Charges");
            DropForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "CostCenterId", "Web.CostCenters");
            DropForeignKey("Web.SaleQuotationLineCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationLineCharges", "ChargeTypeId", "Web.ChargeTypes");
            DropForeignKey("Web.SaleQuotationLineCharges", "ChargeId", "Web.Charges");
            DropForeignKey("Web.SaleQuotationLineCharges", "CalculateOnId", "Web.Charges");
            DropForeignKey("Web.SaleQuotationLines", "SaleQuotationHeaderId", "Web.SaleQuotationHeaders");
            DropForeignKey("Web.SaleQuotationLines", "SaleEnquiryLineId", "Web.SaleEnquiryLines");
            DropForeignKey("Web.SaleQuotationLines", "ReferenceDocTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.SaleQuotationLines", "ProductId", "Web.Products");
            DropForeignKey("Web.PackingLines", "SaleQuotationLine_SaleQuotationLineId", "Web.SaleQuotationLines");
            DropForeignKey("Web.SaleQuotationLines", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.SaleQuotationLines", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.SaleQuotationLines", "Dimension2Id", "Web.Dimension2");
            DropForeignKey("Web.SaleQuotationLines", "Dimension1Id", "Web.Dimension1");
            DropForeignKey("Web.SaleQuotationLines", "DealUnitId", "Web.Units");
            DropForeignKey("Web.SaleQuotationHeaderDetails", "ShipMethodId", "Web.ShipMethods");
            DropForeignKey("Web.SaleQuotationHeaderDetails", "SalesExecutiveId", "Web.People");
            DropForeignKey("Web.SaleQuotationHeaderDetails", "FinancierId", "Web.People");
            DropForeignKey("Web.SaleQuotationHeaderDetails", "DeliveryTermsId", "Web.DeliveryTerms");
            DropForeignKey("Web.SaleQuotationHeaderDetails", "AgentId", "Web.People");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "HeaderTableId", "Web.SaleQuotationHeaders");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "ProductChargeId", "Web.Charges");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "PersonID", "Web.People");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "ParentChargeId", "Web.Charges");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountDrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "LedgerAccountCrId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "CostCenterId", "Web.CostCenters");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "ContraLedgerAccountId", "Web.LedgerAccounts");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "ChargeTypeId", "Web.ChargeTypes");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "ChargeId", "Web.Charges");
            DropForeignKey("Web.SaleQuotationHeaderCharges", "CalculateOnId", "Web.Charges");
            DropForeignKey("Web.SaleQuotationHeaders", "UnitConversionForId", "Web.UnitConversionFors");
            DropForeignKey("Web.SaleQuotationHeaders", "SiteId", "Web.Sites");
            DropForeignKey("Web.SaleQuotationHeaders", "SaleToBuyerId", "Web.People");
            DropForeignKey("Web.SaleQuotationHeaders", "SalesTaxGroupPersonId", "Web.ChargeGroupPersons");
            DropForeignKey("Web.SaleQuotationHeaders", "ReferenceDocTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.SaleQuotationHeaders", "ProcessId", "Web.Processes");
            DropForeignKey("Web.SaleQuotationHeaders", "ParentSaleQuotationHeaderId", "Web.SaleQuotationHeaders");
            DropForeignKey("Web.SaleQuotationHeaders", "DocTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.SaleQuotationHeaders", "DivisionId", "Web.Divisions");
            DropForeignKey("Web.SaleQuotationHeaders", "CurrencyId", "Web.Currencies");
            DropForeignKey("Web.SaleQuotationHeaders", "CostCenterId", "Web.CostCenters");
            DropForeignKey("Web.SaleOrderSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.SaleInvoiceSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.SaleEnquiryLines", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.SaleEnquiryLines", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.SaleDispatchSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.SaleDeliveryOrderSettings", "ProcessId", "Web.Processes");
            DropForeignKey("Web.RequisitionSettings", "ProcessId", "Web.Processes");
            DropForeignKey("Web.RequisitionSettings", "ImportMenuId", "Web.Menus");
            DropForeignKey("Web.RequisitionSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.PurchaseOrderSettings", "ProcessId", "Web.Processes");
            DropForeignKey("Web.PurchaseInvoiceSettings", "ProcessId", "Web.Processes");
            DropForeignKey("Web.PurchaseGoodsReceiptSettings", "ProcessId", "Web.Processes");
            DropForeignKey("Web.ProductSiteDetails", "BinLocationId", "Web.BinLocations");
            DropForeignKey("Web.ProductionOrderSettings", "WizardMenuId", "Web.Menus");
            DropForeignKey("Web.ProductionOrderSettings", "ImportMenuId", "Web.Menus");
            DropForeignKey("Web.ProductionOrderSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.ProdOrderSettings", "WizardMenuId", "Web.Menus");
            DropForeignKey("Web.ProdOrderSettings", "ImportMenuId", "Web.Menus");
            DropForeignKey("Web.ProdOrderSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.MaterialRequestSettings", "ProcessId", "Web.Processes");
            DropForeignKey("Web.MaterialRequestSettings", "ImportMenuId", "Web.Menus");
            DropForeignKey("Web.MaterialRequestSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.MaterialPlanSettings", "ImportMenuId", "Web.Menus");
            DropForeignKey("Web.MaterialPlanSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.StockHeaderSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.Ledgers", "ReferenceDocTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.JobReceiveSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.JobReceiveQASettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.JobReceiveByProducts", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.JobReceiveByProducts", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.JobReceiveBoms", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.JobReceiveBoms", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.JobOrderSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.JobOrderInspectionSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.JobOrderInspectionRequestSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.JobOrderCancelBoms", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.JobOrderCancelBoms", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.JobInvoiceSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.JobInvoiceHeaders", "FinancierId", "Web.People");
            DropForeignKey("Web.ExcessMaterialSettings", "ExportMenuId", "Web.Menus");
            DropForeignKey("Web.SaleInvoiceHeaders", "SalesExecutiveId", "Web.People");
            DropForeignKey("Web.SaleInvoiceHeaders", "FinancierId", "Web.People");
            DropForeignKey("Web.BinLocations", "GodownId", "Web.Godowns");
            DropForeignKey("Web.JobOrderHeaders", "TransporterId", "Web.People");
            DropForeignKey("Web.JobOrderHeaders", "ShipToAddressId", "Web.PersonAddresses");
            DropForeignKey("Web.JobOrderHeaders", "ShipMethodId", "Web.ShipMethods");
            DropForeignKey("Web.JobOrderHeaders", "SalesTaxGroupPersonId", "Web.ChargeGroupPersons");
            DropForeignKey("Web.JobOrderHeaders", "SalesExecutiveId", "Web.People");
            DropForeignKey("Web.ProductUidHeaders", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ProductUidHeaders", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.SaleOrderHeaders", "SalesExecutiveId", "Web.People");
            DropForeignKey("Web.SaleOrderHeaders", "FinancierId", "Web.People");
            DropForeignKey("Web.LedgerHeaders", "ReferenceDocTypeId", "Web.DocumentTypes");
            DropForeignKey("Web.ProductUids", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.ProductUids", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.JobOrderLines", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.JobOrderLines", "Dimension3Id", "Web.Dimension3");
            DropForeignKey("Web.JobOrderHeaders", "FinancierId", "Web.People");
            DropForeignKey("Web.JobOrderHeaders", "DocumentShipMethodId", "Web.DocumentShipMethods");
            DropForeignKey("Web.JobOrderHeaders", "DeliveryTermsId", "Web.DeliveryTerms");
            DropForeignKey("Web.JobOrderHeaders", "CurrencyId", "Web.Currencies");
            DropForeignKey("Web.JobOrderHeaders", "AgentId", "Web.People");
            DropForeignKey("Web.JobOrderBoms", "Dimension4Id", "Web.Dimension4");
            DropForeignKey("Web.JobOrderBoms", "Dimension3Id", "Web.Dimension3");
            DropIndex("Web.WeavingRetensions", new[] { "PersonId" });
            DropIndex("Web.ViewSaleEnquiryBalance", new[] { "DocType_DocumentTypeId" });
            DropIndex("Web.ViewSaleEnquiryBalance", new[] { "Dimension4Id" });
            DropIndex("Web.ViewSaleEnquiryBalance", new[] { "Dimension3Id" });
            DropIndex("Web.ViewProdOrderBalance", new[] { "Dimension4Id" });
            DropIndex("Web.ViewProdOrderBalance", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobReceiveBalanceForInvoice", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobReceiveBalanceForInvoice", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobReceiveBalance", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobReceiveBalance", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobOrderInspectionRequestBalance", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobOrderInspectionRequestBalance", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobOrderBalanceFromStatus", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobOrderBalanceFromStatus", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobOrderBalanceForInvoice", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobOrderBalanceForInvoice", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobOrderBalanceForInspectionRequest", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobOrderBalanceForInspectionRequest", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobOrderBalanceForInspection", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobOrderBalanceForInspection", new[] { "Dimension3Id" });
            DropIndex("Web.ViewJobOrderBalance", new[] { "Dimension4Id" });
            DropIndex("Web.ViewJobOrderBalance", new[] { "Dimension3Id" });
            DropIndex("Web.TdsRates", new[] { "LedgerAccountId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "CalculationId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "ExportMenuId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "WizardMenuId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "ImportMenuId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "DealUnitId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "ProcessId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "UnitConversionForId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "DivisionId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "SiteId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "DeliveryTermsId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "CurrencyId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "ShipMethodId" });
            DropIndex("Web.SaleQuotationSettings", new[] { "DocTypeId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "ParentChargeId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "CostCenterId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "ContraLedgerAccountId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "LedgerAccountCrId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "LedgerAccountDrId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "PersonID" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "CalculateOnId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "ChargeTypeId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "ChargeId" });
            DropIndex("Web.SaleQuotationLineCharges", new[] { "LineTableId" });
            DropIndex("Web.SaleQuotationLines", new[] { "ReferenceDocTypeId" });
            DropIndex("Web.SaleQuotationLines", new[] { "DealUnitId" });
            DropIndex("Web.SaleQuotationLines", new[] { "Dimension4Id" });
            DropIndex("Web.SaleQuotationLines", new[] { "Dimension3Id" });
            DropIndex("Web.SaleQuotationLines", new[] { "Dimension2Id" });
            DropIndex("Web.SaleQuotationLines", new[] { "Dimension1Id" });
            DropIndex("Web.SaleQuotationLines", new[] { "SaleEnquiryLineId" });
            DropIndex("Web.SaleQuotationLines", "IX_SaleQuotationLine_SaleQuotationHeaderProductDueDate");
            DropIndex("Web.SaleQuotationHeaderDetails", new[] { "AgentId" });
            DropIndex("Web.SaleQuotationHeaderDetails", new[] { "SalesExecutiveId" });
            DropIndex("Web.SaleQuotationHeaderDetails", new[] { "FinancierId" });
            DropIndex("Web.SaleQuotationHeaderDetails", new[] { "DeliveryTermsId" });
            DropIndex("Web.SaleQuotationHeaderDetails", new[] { "ShipMethodId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "ParentChargeId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "CostCenterId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "ContraLedgerAccountId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "LedgerAccountCrId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "LedgerAccountDrId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "PersonID" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "CalculateOnId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "ProductChargeId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "ChargeTypeId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "ChargeId" });
            DropIndex("Web.SaleQuotationHeaderCharges", new[] { "HeaderTableId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "ReferenceDocTypeId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "SalesTaxGroupPersonId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "UnitConversionForId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "CurrencyId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "SaleToBuyerId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "CostCenterId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "ProcessId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "ParentSaleQuotationHeaderId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "SiteId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "DivisionId" });
            DropIndex("Web.SaleQuotationHeaders", new[] { "DocTypeId" });
            DropIndex("Web.SaleOrderSettings", new[] { "ExportMenuId" });
            DropIndex("Web.SaleInvoiceSettings", new[] { "ExportMenuId" });
            DropIndex("Web.SaleEnquiryLines", new[] { "Dimension4Id" });
            DropIndex("Web.SaleEnquiryLines", new[] { "Dimension3Id" });
            DropIndex("Web.SaleDispatchSettings", new[] { "ExportMenuId" });
            DropIndex("Web.SaleDeliveryOrderSettings", new[] { "ProcessId" });
            DropIndex("Web.RequisitionSettings", new[] { "ExportMenuId" });
            DropIndex("Web.RequisitionSettings", new[] { "ImportMenuId" });
            DropIndex("Web.RequisitionSettings", new[] { "ProcessId" });
            DropIndex("Web.PurchaseOrderSettings", new[] { "ProcessId" });
            DropIndex("Web.PurchaseInvoiceSettings", new[] { "ProcessId" });
            DropIndex("Web.PurchaseGoodsReceiptSettings", new[] { "ProcessId" });
            DropIndex("Web.ProductSiteDetails", new[] { "BinLocationId" });
            DropIndex("Web.ProductionOrderSettings", new[] { "WizardMenuId" });
            DropIndex("Web.ProductionOrderSettings", new[] { "ExportMenuId" });
            DropIndex("Web.ProductionOrderSettings", new[] { "ImportMenuId" });
            DropIndex("Web.ProdOrderSettings", new[] { "WizardMenuId" });
            DropIndex("Web.ProdOrderSettings", new[] { "ExportMenuId" });
            DropIndex("Web.ProdOrderSettings", new[] { "ImportMenuId" });
            DropIndex("Web.MaterialRequestSettings", new[] { "ExportMenuId" });
            DropIndex("Web.MaterialRequestSettings", new[] { "ImportMenuId" });
            DropIndex("Web.MaterialRequestSettings", new[] { "ProcessId" });
            DropIndex("Web.MaterialPlanSettings", new[] { "ExportMenuId" });
            DropIndex("Web.MaterialPlanSettings", new[] { "ImportMenuId" });
            DropIndex("Web.StockHeaderSettings", new[] { "ExportMenuId" });
            DropIndex("Web.LedgerAdjs", new[] { "LedgerId" });
            DropIndex("Web.Ledgers", new[] { "ReferenceDocTypeId" });
            DropIndex("Web.JobReceiveSettings", new[] { "ExportMenuId" });
            DropIndex("Web.JobReceiveQASettings", new[] { "ExportMenuId" });
            DropIndex("Web.JobReceiveByProducts", new[] { "Dimension4Id" });
            DropIndex("Web.JobReceiveByProducts", new[] { "Dimension3Id" });
            DropIndex("Web.JobReceiveBoms", new[] { "Dimension4Id" });
            DropIndex("Web.JobReceiveBoms", new[] { "Dimension3Id" });
            DropIndex("Web.JobOrderSettings", new[] { "ExportMenuId" });
            DropIndex("Web.JobOrderInspectionSettings", new[] { "ExportMenuId" });
            DropIndex("Web.JobOrderInspectionRequestSettings", new[] { "ExportMenuId" });
            DropIndex("Web.JobOrderCancelBoms", new[] { "Dimension4Id" });
            DropIndex("Web.JobOrderCancelBoms", new[] { "Dimension3Id" });
            DropIndex("Web.JobInvoiceSettings", new[] { "ExportMenuId" });
            DropIndex("Web.JobInvoiceHeaders", new[] { "FinancierId" });
            DropIndex("Web.ExcessMaterialSettings", new[] { "ExportMenuId" });
            DropIndex("Web.SaleInvoiceHeaders", new[] { "SalesExecutiveId" });
            DropIndex("Web.SaleInvoiceHeaders", new[] { "FinancierId" });
            DropIndex("Web.BinLocations", "IX_BinLocation_BinLocation");
            DropIndex("Web.BinLocations", new[] { "GodownId" });
            DropIndex("Web.ProductUidHeaders", new[] { "Dimension4Id" });
            DropIndex("Web.ProductUidHeaders", new[] { "Dimension3Id" });
            DropIndex("Web.SaleOrderHeaders", new[] { "SalesExecutiveId" });
            DropIndex("Web.SaleOrderHeaders", new[] { "FinancierId" });
            DropIndex("Web.LedgerHeaders", new[] { "ReferenceDocTypeId" });
            DropIndex("Web.PackingLines", new[] { "SaleQuotationLine_SaleQuotationLineId" });
            DropIndex("Web.ProductUids", new[] { "Dimension4Id" });
            DropIndex("Web.ProductUids", new[] { "Dimension3Id" });
            DropIndex("Web.JobOrderLines", "IX_JobOrderLine_Unique");
            DropIndex("Web.DocumentShipMethods", "IX_DocumentShipMethod_DocumentShipMethodName");
            DropIndex("Web.JobOrderHeaders", new[] { "AgentId" });
            DropIndex("Web.JobOrderHeaders", new[] { "TransporterId" });
            DropIndex("Web.JobOrderHeaders", new[] { "DocumentShipMethodId" });
            DropIndex("Web.JobOrderHeaders", new[] { "ShipMethodId" });
            DropIndex("Web.JobOrderHeaders", new[] { "SalesTaxGroupPersonId" });
            DropIndex("Web.JobOrderHeaders", new[] { "CurrencyId" });
            DropIndex("Web.JobOrderHeaders", new[] { "ShipToAddressId" });
            DropIndex("Web.JobOrderHeaders", new[] { "DeliveryTermsId" });
            DropIndex("Web.JobOrderHeaders", new[] { "SalesExecutiveId" });
            DropIndex("Web.JobOrderHeaders", new[] { "FinancierId" });
            DropIndex("Web.JobOrderBoms", new[] { "Dimension4Id" });
            DropIndex("Web.JobOrderBoms", new[] { "Dimension3Id" });
            AlterColumn("Web.LedgerAdjs", "LedgerId", c => c.Int(nullable: false));
            DropColumn("Web.WeavingRetensions", "PersonId");
            DropColumn("Web.ViewSaleEnquiryBalance", "DocType_DocumentTypeId");
            DropColumn("Web.ViewSaleEnquiryBalance", "DocTypeId");
            DropColumn("Web.ViewSaleEnquiryBalance", "Dimension4Id");
            DropColumn("Web.ViewSaleEnquiryBalance", "Dimension3Id");
            DropColumn("Web.ViewProdOrderBalance", "Dimension4Id");
            DropColumn("Web.ViewProdOrderBalance", "Dimension3Id");
            DropColumn("Web.ViewJobReceiveBalanceForQA", "Dimension4Id");
            DropColumn("Web.ViewJobReceiveBalanceForQA", "Dimension3Id");
            DropColumn("Web.ViewJobReceiveBalanceForInvoice", "Dimension4Id");
            DropColumn("Web.ViewJobReceiveBalanceForInvoice", "Dimension3Id");
            DropColumn("Web.ViewJobReceiveBalance", "Dimension4Id");
            DropColumn("Web.ViewJobReceiveBalance", "Dimension3Id");
            DropColumn("Web.ViewJobOrderInspectionRequestBalance", "Dimension4Id");
            DropColumn("Web.ViewJobOrderInspectionRequestBalance", "Dimension3Id");
            DropColumn("Web.ViewJobOrderBalanceFromStatus", "Dimension4Id");
            DropColumn("Web.ViewJobOrderBalanceFromStatus", "Dimension3Id");
            DropColumn("Web.ViewJobOrderBalanceForInvoice", "Dimension4Id");
            DropColumn("Web.ViewJobOrderBalanceForInvoice", "Dimension3Id");
            DropColumn("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension4Id");
            DropColumn("Web.ViewJobOrderBalanceForInspectionRequest", "Dimension3Id");
            DropColumn("Web.ViewJobOrderBalanceForInspection", "Dimension4Id");
            DropColumn("Web.ViewJobOrderBalanceForInspection", "Dimension3Id");
            DropColumn("Web.ViewJobOrderBalance", "Dimension4Id");
            DropColumn("Web.ViewJobOrderBalance", "Dimension3Id");
            DropColumn("Web.TdsRates", "LedgerAccountId");
            DropColumn("Web.SaleOrderSettings", "ExportMenuId");
            DropColumn("Web.SaleOrderSettings", "filterPersonRoles");
            DropColumn("Web.SaleOrderSettings", "isMandatoryRate");
            DropColumn("Web.SaleOrderSettings", "isVisibleSalesExecutive");
            DropColumn("Web.SaleOrderSettings", "isVisibleFinancier");
            DropColumn("Web.SaleInvoiceSettings", "ExportMenuId");
            DropColumn("Web.SaleInvoiceSettings", "filterPersonRoles");
            DropColumn("Web.SaleInvoiceSettings", "isVisibleSalesExecutive");
            DropColumn("Web.SaleInvoiceSettings", "isVisibleFinancier");
            DropColumn("Web.SaleEnquirySettings", "filterPersonRoles");
            DropColumn("Web.SaleEnquiryLines", "Dimension4Id");
            DropColumn("Web.SaleEnquiryLines", "Dimension3Id");
            DropColumn("Web.SaleDispatchSettings", "ExportMenuId");
            DropColumn("Web.SaleDispatchSettings", "filterPersonRoles");
            DropColumn("Web.SaleDeliveryOrderSettings", "ProcessId");
            DropColumn("Web.SaleDeliveryOrderSettings", "filterPersonRoles");
            DropColumn("Web.RequisitionSettings", "ExportMenuId");
            DropColumn("Web.RequisitionSettings", "ImportMenuId");
            DropColumn("Web.RequisitionSettings", "ProcessId");
            DropColumn("Web.RequisitionSettings", "filterPersonRoles");
            DropColumn("Web.RateConversionSettings", "filterPersonRoles");
            DropColumn("Web.PurchaseOrderSettings", "ProcessId");
            DropColumn("Web.PurchaseOrderSettings", "filterPersonRoles");
            DropColumn("Web.PurchaseInvoiceSettings", "ProcessId");
            DropColumn("Web.PurchaseInvoiceSettings", "filterPersonRoles");
            DropColumn("Web.PurchaseGoodsReceiptSettings", "ProcessId");
            DropColumn("Web.PurchaseGoodsReceiptSettings", "filterPersonRoles");
            DropColumn("Web.ProductTypeSettings", "ProductCategoryCaption");
            DropColumn("Web.ProductTypeSettings", "ProductGroupCaption");
            DropColumn("Web.ProductTypeSettings", "ProductSpecificationCaption");
            DropColumn("Web.ProductTypeSettings", "ProductDescriptionCaption");
            DropColumn("Web.ProductTypeSettings", "ProductCodeCaption");
            DropColumn("Web.ProductTypeSettings", "ProductNameCaption");
            DropColumn("Web.ProductTypeSettings", "isVisibleProductProcessDetail");
            DropColumn("Web.ProductTypeSettings", "isVisibleConsumptionDetail");
            DropColumn("Web.ProductTypeSettings", "isVisibleLotManagement");
            DropColumn("Web.ProductTypeSettings", "isVisibleCarryingCost");
            DropColumn("Web.ProductTypeSettings", "isVisibleProfitMargin");
            DropColumn("Web.ProductTypeSettings", "isVisibleBinLocationId");
            DropColumn("Web.ProductTypeSettings", "isVisibleGodownId");
            DropColumn("Web.ProductTypeSettings", "isVisibleReOrderLevel");
            DropColumn("Web.ProductTypeSettings", "isVisibleMinimumOrderQty");
            DropColumn("Web.ProductTypeSettings", "isVisibleTags");
            DropColumn("Web.ProductTypeSettings", "isVisibleStandardCost");
            DropColumn("Web.ProductTypeSettings", "isVisibleSaleRate");
            DropColumn("Web.ProductTypeSettings", "isVisibleSalesTaxGroup");
            DropColumn("Web.ProductTypeSettings", "isVisibleProductCategory");
            DropColumn("Web.ProductTypeSettings", "isVisibleProductSpecification");
            DropColumn("Web.ProductTypeSettings", "isVisibleProductDescription");
            DropColumn("Web.ProductSiteDetails", "BinLocationId");
            DropColumn("Web.ProductionOrderSettings", "WizardMenuId");
            DropColumn("Web.ProductionOrderSettings", "ExportMenuId");
            DropColumn("Web.ProductionOrderSettings", "ImportMenuId");
            DropColumn("Web.ProductionOrderSettings", "filterPersonRoles");
            DropColumn("Web.ProdOrderSettings", "WizardMenuId");
            DropColumn("Web.ProdOrderSettings", "ExportMenuId");
            DropColumn("Web.ProdOrderSettings", "ImportMenuId");
            DropColumn("Web.ProdOrderSettings", "filterPersonRoles");
            DropColumn("Web.ProdOrderSettings", "isVisibleLineDueDate");
            DropColumn("Web.ProdOrderSettings", "isVisibleBuyer");
            DropColumn("Web.PackingSettings", "filterPersonRoles");
            DropColumn("Web.MaterialRequestSettings", "ExportMenuId");
            DropColumn("Web.MaterialRequestSettings", "ImportMenuId");
            DropColumn("Web.MaterialRequestSettings", "ProcessId");
            DropColumn("Web.MaterialRequestSettings", "filterPersonRoles");
            DropColumn("Web.MaterialReceiveSettings", "filterPersonRoles");
            DropColumn("Web.MaterialPlanSettings", "DocumentPrintReportHeaderId");
            DropColumn("Web.MaterialPlanSettings", "ExportMenuId");
            DropColumn("Web.MaterialPlanSettings", "ImportMenuId");
            DropColumn("Web.MaterialPlanSettings", "filterPersonRoles");
            DropColumn("Web.MaterialPlanSettings", "isVisibleProdPlanQty");
            DropColumn("Web.StockHeaderSettings", "ExportMenuId");
            DropColumn("Web.StockHeaderSettings", "filterPersonRoles");
            DropColumn("Web.Ledgers", "ReferenceDocLineId");
            DropColumn("Web.Ledgers", "ReferenceDocTypeId");
            DropColumn("Web.JobReceiveSettings", "isVisibleByProductDimension4");
            DropColumn("Web.JobReceiveSettings", "isVisibleByProductDimension3");
            DropColumn("Web.JobReceiveSettings", "isVisibleConsumptionDimension4");
            DropColumn("Web.JobReceiveSettings", "isVisibleConsumptionDimension3");
            DropColumn("Web.JobReceiveSettings", "ByProductDimension4Caption");
            DropColumn("Web.JobReceiveSettings", "ByProductDimension3Caption");
            DropColumn("Web.JobReceiveSettings", "ConsumptionDimension4Caption");
            DropColumn("Web.JobReceiveSettings", "ConsumptionDimension3Caption");
            DropColumn("Web.JobReceiveSettings", "ExportMenuId");
            DropColumn("Web.JobReceiveSettings", "filterPersonRoles");
            DropColumn("Web.JobReceiveQASettings", "filterPersonRoles");
            DropColumn("Web.JobReceiveQASettings", "ExportMenuId");
            DropColumn("Web.JobReceiveByProducts", "Dimension4Id");
            DropColumn("Web.JobReceiveByProducts", "Dimension3Id");
            DropColumn("Web.JobReceiveBoms", "Dimension4Id");
            DropColumn("Web.JobReceiveBoms", "Dimension3Id");
            DropColumn("Web.JobOrderSettings", "TermsAndConditions");
            DropColumn("Web.JobOrderSettings", "NonCountedQtyCaption");
            DropColumn("Web.JobOrderSettings", "ExportMenuId");
            DropColumn("Web.JobOrderSettings", "filterPersonRoles");
            DropColumn("Web.JobOrderSettings", "isVisibleSalesExecutive");
            DropColumn("Web.JobOrderSettings", "isVisibleFinancier");
            DropColumn("Web.JobOrderSettings", "isVisiblePaymentTerms");
            DropColumn("Web.JobOrderSettings", "isVisibleDoorDelivery");
            DropColumn("Web.JobOrderSettings", "isVisibleAgent");
            DropColumn("Web.JobOrderSettings", "isVisibleTransporter");
            DropColumn("Web.JobOrderSettings", "isVisibleDocumentShipMethod");
            DropColumn("Web.JobOrderSettings", "isVisibleShipMethod");
            DropColumn("Web.JobOrderSettings", "isVisibleSalesTaxGroupPerson");
            DropColumn("Web.JobOrderSettings", "isVisibleCurrency");
            DropColumn("Web.JobOrderSettings", "isVisibleShipToAddress");
            DropColumn("Web.JobOrderSettings", "isVisibleDeliveryTerms");
            DropColumn("Web.JobOrderInspectionSettings", "filterPersonRoles");
            DropColumn("Web.JobOrderInspectionSettings", "ExportMenuId");
            DropColumn("Web.JobOrderInspectionRequestSettings", "filterPersonRoles");
            DropColumn("Web.JobOrderInspectionRequestSettings", "ExportMenuId");
            DropColumn("Web.JobOrderCancelBoms", "Dimension4Id");
            DropColumn("Web.JobOrderCancelBoms", "Dimension3Id");
            DropColumn("Web.JobInvoiceSettings", "ExportMenuId");
            DropColumn("Web.JobInvoiceSettings", "filterPersonRoles");
            DropColumn("Web.JobInvoiceSettings", "isGenerateProductUid");
            DropColumn("Web.JobInvoiceSettings", "isLedgerPostingLineWise");
            DropColumn("Web.JobInvoiceSettings", "isVisibleMfgDate");
            DropColumn("Web.JobInvoiceSettings", "isVisibleFinancier");
            DropColumn("Web.JobInvoiceSettings", "isVisibleRateDiscountPer");
            DropColumn("Web.JobInvoiceSettings", "isVisiblePenalty");
            DropColumn("Web.JobInvoiceSettings", "isVisibleIncentive");
            DropColumn("Web.JobInvoiceLines", "MfgDate");
            DropColumn("Web.JobInvoiceLines", "RateDiscountPer");
            DropColumn("Web.JobInvoiceHeaders", "FinancierId");
            DropColumn("Web.ExcessMaterialSettings", "ExportMenuId");
            DropColumn("Web.DocumentTypeSettings", "SpecificationCaption");
            DropColumn("Web.DocumentTypeSettings", "CostCenterCaption");
            DropColumn("Web.SaleInvoiceLines", "RateRemark");
            DropColumn("Web.SaleInvoiceHeaderDetail", "Insurance");
            DropColumn("Web.SaleInvoiceHeaders", "SalesExecutiveId");
            DropColumn("Web.SaleInvoiceHeaders", "FinancierId");
            DropColumn("Web.ProductUidHeaders", "Dimension4Id");
            DropColumn("Web.ProductUidHeaders", "Dimension3Id");
            DropColumn("Web.SaleOrderHeaders", "SalesExecutiveId");
            DropColumn("Web.SaleOrderHeaders", "FinancierId");
            DropColumn("Web.LedgerHeaders", "ReferenceDocId");
            DropColumn("Web.LedgerHeaders", "ReferenceDocTypeId");
            DropColumn("Web.PackingLines", "SaleQuotationLine_SaleQuotationLineId");
            DropColumn("Web.PackingLines", "RateRemark");
            DropColumn("Web.PackingLines", "SealNo");
            DropColumn("Web.ProductUids", "Dimension4Id");
            DropColumn("Web.ProductUids", "Dimension3Id");
            DropColumn("Web.ProductUids", "ProductUidSpecification");
            DropColumn("Web.ProdOrderLines", "DueDate");
            DropColumn("Web.JobOrderLines", "Dimension4Id");
            DropColumn("Web.JobOrderLines", "Dimension3Id");
            DropColumn("Web.JobOrderHeaders", "PayTermBankPer");
            DropColumn("Web.JobOrderHeaders", "PayTermCashPer");
            DropColumn("Web.JobOrderHeaders", "PayTermOnDueDatePer");
            DropColumn("Web.JobOrderHeaders", "PayTermOnDeliveryPer");
            DropColumn("Web.JobOrderHeaders", "PayTermAdvancePer");
            DropColumn("Web.JobOrderHeaders", "AgentId");
            DropColumn("Web.JobOrderHeaders", "IsDoorDelivery");
            DropColumn("Web.JobOrderHeaders", "TransporterId");
            DropColumn("Web.JobOrderHeaders", "DocumentShipMethodId");
            DropColumn("Web.JobOrderHeaders", "ShipMethodId");
            DropColumn("Web.JobOrderHeaders", "SalesTaxGroupPersonId");
            DropColumn("Web.JobOrderHeaders", "CurrencyId");
            DropColumn("Web.JobOrderHeaders", "ShipToAddressId");
            DropColumn("Web.JobOrderHeaders", "DeliveryTermsId");
            DropColumn("Web.JobOrderHeaders", "SalesExecutiveId");
            DropColumn("Web.JobOrderHeaders", "FinancierId");
            DropColumn("Web.JobOrderBoms", "Dimension4Id");
            DropColumn("Web.JobOrderBoms", "Dimension3Id");
            DropColumn("Web.Products", "CarryingCost");
            DropColumn("Web.Products", "ProfitMargin");
            DropColumn("Web.Products", "SaleRate");
            DropTable("Web.ViewSizeinCms");
            DropTable("Web.SaleQuotationSettings");
            DropTable("Web.SaleQuotationLineCharges");
            DropTable("Web.SaleQuotationLines");
            DropTable("Web.SaleQuotationHeaderDetails");
            DropTable("Web.SaleQuotationHeaderCharges");
            DropTable("Web.SaleQuotationHeaders");
            DropTable("Web.BinLocations");
            DropTable("Web.DocumentShipMethods");
            CreateIndex("Web.LedgerAdjs", "LedgerId");
            CreateIndex("Web.JobOrderLines", new[] { "JobOrderHeaderId", "ProductUidId", "ProductId", "Dimension1Id", "Dimension2Id", "ProdOrderLineId", "FromProcessId", "LotNo", "Specification" }, unique: true, name: "IX_JobOrderLine_Unique");
        }
    }
}
