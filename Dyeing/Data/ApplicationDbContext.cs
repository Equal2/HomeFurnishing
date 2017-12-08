using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity.ModelConfiguration.Conventions;
using Infrastructure.IO;
using Models.Customize.Models;
using Models.BasicSetup.Models;
using Models.Company.Models;
using Components.Logging;
using Models.Customize.DataBaseViews;
using TimePlanValidator.Models;
using Models.Company.DatabaseViews;

namespace Data.Customize
{
    public partial class ApplicationDbContext : DataContext
    {
        static string _errors = "";
        static string SchemaName = "Web";

        public string strSchemaName = "Web";

        public ApplicationDbContext()
            : base((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"] ?? "LoginDB", false)
        {
            Configuration.ProxyCreationEnabled = false;
            Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            Database.CommandTimeout = 60;
        }

        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(null); // Existing data, do nothing   
            //Database.SetInitializer(new ApplicationDbContextInitializer()); // Create New database
            //Database.SetInitializer(new LoginDbContextInitializer()); // Create New database
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        //Master Models        

        //User Models

        public DbSet<JobReceiveHeader> JobReceiveHeader { get; set; }

        public DbSet<JobReceiveHeaderExtended> JobReceiveHeaderExtended { get; set; }
        public DbSet<JobReceiveLine> JobReceiveLine { get; set; }
        public DbSet<JobReceiveLineStatus> JobReceiveLineStatus { get; set; }


        public DbSet<JobOrderHeaderCharge> JobOrderHeaderCharge { get; set; }
        public DbSet<JobOrderHeader> JobOrderHeader { get; set; }
        public DbSet<JobOrderHeaderStatus> JobOrderHeaderStatus { get; set; }

        public DbSet<JobOrderHeaderExtended> JobOrderHeaderExtended { get; set; }
        public DbSet<JobOrderPerk> JobOrderPerk { get; set; }
        public DbSet<JobOrderSettings> JobOrderSettings { get; set; }
        public DbSet<JobOrderLine> JobOrderLine { get; set; }
        public DbSet<JobOrderLineStatus> JobOrderLineStatus { get; set; }
        public DbSet<JobOrderLineExtended> JobOrderLinExtended { get; set; }
        public DbSet<StockLineExtended> StockLineExtended { get; set; }
        public DbSet<StockAdj> StockAdj { get; set; }
        public DbSet<JobOrderLineCharge> JobOrderLineCharge { get; set; }
        public DbSet<JobOrderBom> JobOrderBom { get; set; }

        public DbSet<CostCenter> CostCenter { get; set; }
        public DbSet<CostCenterStatus> CostCenterStatus { get; set; }
        public DbSet<LedgerAccount> LedgerAccount { get; set; }
        public DbSet<LedgerAccountGroup> LedgerAccountGroup { get; set; }
        public DbSet<DocumentType> DocumentType { get; set; }
        public DbSet<DocumentCategory> DocumentCategory { get; set; }
        public DbSet<DocumentTypeSite> DocumentTypeSite { get; set; }
        public DbSet<DocumentTypeDivision> DocumentTypeDivision { get; set; }
        public DbSet<ActivityLog> ActivityLog { get; set; }
        public DbSet<Stock> Stock { get; set; }
        public DbSet<StockHeader> StockHeader { get; set; }
        public DbSet<StockLine> StockLine { get; set; }
        public DbSet<StockBalance> StockBalance { get; set; }
        public DbSet<StockProcess> StockProcess { get; set; }
        public DbSet<StockProcessBalance> StockProcessBalance { get; set; }
        public DbSet<Person> Person { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<UnitConversion> UnitConversion { get; set; }
        public DbSet<UnitConversionFor> UnitConversionFor { get; set; }
        public DbSet<ViewJobOrderBalance> ViewJobOrderBalance { get; set; }
        public DbSet<ViewProdOrderBalance> ViewProdOrderBalance { get; set; }
        public DbSet<ViewRecipeBalanceForSubRecipe> ViewRecipeBalanceForSubRecipe { get; set; }
        public DbSet<Dimension1> Dimension1 { get; set; }
        public DbSet<Dimension2> Dimension2 { get; set; }
        public DbSet<JobWorker> JobWorker { get; set; }
        public DbSet<BusinessEntity> BusinessEntity { get; set; }
        public DbSet<PersonProcess> PersonProcess { get; set; }
        public DbSet<ProductUid> ProductUid { get; set; }
        public DbSet<Process> Process { get; set; }
        public DbSet<GatePassHeader> GatePassHeader { get; set; }
        public DbSet<GatePassLine> GatePassLine { get; set; }
        public DbSet<ProductUidHeader> ProductUidHeader { get; set; }
        public DbSet<PerkDocumentType> PerkDocumentType { get; set; }
        public DbSet<Perk> Perk { get; set; }
        public DbSet<_Menu> _Menu { get; set; }
        public DbSet<_ReportHeader> _ReportHeader { get; set; }
        public DbSet<_ReportLine> _ReportLine { get; set; }
        public DbSet<ProdOrderHeader> ProdOrderHeader { get; set; }
        public DbSet<ProdOrderHeaderStatus> ProdOrderHeaderStatus { get; set; }
        public DbSet<ProdOrderLine> ProdOrderLine { get; set; }
        public DbSet<ProdOrderLineStatus> ProdOrderLineStatus { get; set; }
        public DbSet<Site> Site { get; set; }
        public DbSet<DocumentTypeTimeExtension> DocumentTypeTimeExtension { get; set; }
        public DbSet<DocumentTypeTimePlan> DocumentTypeTimePlan { get; set; }
        public DbSet<_Employee> _Employee { get; set; }
        public DbSet<CalculationLineLedgerAccount> CalculationLineLedgerAccount { get; set; }
        public DbSet<CalculationHeaderLedgerAccount> CalculationHeaderLedgerAccount { get; set; }


        //public DbSet<IdentityUser> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.Properties<decimal>().Configure(config => config.HasPrecision(18, 4));

            // Change the name of the table to be Users instead of AspNetUsers
            modelBuilder.Entity<IdentityUser>().ToTable("Users");

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        static void CreateDatabseViews()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string mQry = "";


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".StockVirtual') IS NOT NULL DROP VIEW " + SchemaName + ".StockVirtual  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE VIEW " + SchemaName + ".StockVirtual AS "
                        + "SELECT H.JobOrderHeaderId AS DocHeaderId, L.JobOrderLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "L.ProductId, L.Qty AS Qty_Iss, 0 AS Qty_Rec  "
                        + "FROM  " + SchemaName + ".JobOrderHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".JobOrderLines L ON H.JobOrderHeaderId = L.JobOrderHeaderId  "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE Dc.DocumentCategoryName IN ('Weaving Order','Weaving Finish Order', 'Dyeing Order','Re Dyeing Order','Spinning Order','Dhunai Order', "
                        + "'Spinning Order','Trace Map Order Issue') "
                        + "UNION ALL "
                        + "SELECT H.JobOrderCancelHeaderId AS DocHeaderId, L.JobOrderCancelLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "Ol.ProductId, 0 AS Qty_Iss, - L.Qty AS Qty_Rec  "
                        + "FROM  " + SchemaName + ".JobOrderCancelHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".JobOrderCancelLines L ON H.JobOrderCancelHeaderId = L.JobOrderCancelHeaderId "
                        + "LEFT JOIN " + SchemaName + ".JobOrderLines Ol ON L.JobOrderLineId = Ol.JobOrderLineId  "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE Dc.DocumentCategoryName IN ('Weaving Order Cancel','Dyeing Order Cacel') "
                        + "UNION ALL	 "
                        + "SELECT H.JobReceiveHeaderId AS DocHeaderId, L.JobReceiveLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "Ol.ProductId, 0 AS Qty_Iss, L.Qty AS Qty_Rec  "
                        + "FROM  " + SchemaName + ".JobReceiveHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".JobReceiveLines L ON H.JobReceiveHeaderId = L.JobReceiveHeaderId "
                        + "LEFT JOIN " + SchemaName + ".JobOrderLines Ol ON L.JobOrderLineId = Ol.JobOrderLineId  "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE Dc.DocumentCategoryName IN ('Weaving Receive','Dyeing Receive','Re-Dyeing Receive','Spinning Receive','Trace Map Receive') "
                        + "UNION ALL	 "
                        + "SELECT H.PurchaseOrderHeaderId AS DocHeaderId, L.PurchaseOrderLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId, L.ProductId, L.Qty AS Qty_Iss, 0 AS Qty_Rec  "
                        + "FROM  " + SchemaName + ".PurchaseOrderHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".PurchaseOrderLines L ON H.PurchaseOrderHeaderId = L.PurchaseOrderHeaderId "
                        + "UNION ALL "
                        + "SELECT H.PurchaseOrderCancelHeaderId AS DocHeaderId, L.PurchaseOrderCancelLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "Pol.ProductId, 0 AS Qty_Iss, L.Qty AS Qty_Rec  "
                        + "FROM  " + SchemaName + ".PurchaseOrderCancelHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".PurchaseOrderCancelLines L ON H.PurchaseOrderCancelHeaderId = L.PurchaseOrderCancelHeaderId "
                        + "LEFT JOIN " + SchemaName + ".PurchaseOrderLines Pol ON L.PurchaseOrderLineId = Pol.PurchaseOrderLineId "
                        + "UNION ALL "
                        + "SELECT H.PurchaseGoodsReceiptHeaderId AS DocHeaderId, L.PurchaseGoodsReceiptLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId, L.ProductId, 0 AS Qty_Iss, L.Qty AS Qty_Rec "
                        + "FROM  " + SchemaName + ".PurchaseGoodsReceiptHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".PurchaseGoodsReceiptLines L ON H.PurchaseGoodsReceiptHeaderId = L.PurchaseGoodsReceiptHeaderId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }



            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".StockUid') IS NOT NULL DROP VIEW " + SchemaName + ".StockUid  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE VIEW " + SchemaName + ".StockUid AS "
                        + "SELECT H.JobOrderHeaderId AS DocHeaderId, L.JobOrderLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId, "
                        + "H.GodownId AS GodownId, H.ProcessId, L.ProductUidId, 1 AS Qty_Iss, 0 AS Qty_Rec, L.CreatedDate, L.Remark "
                        + "FROM  " + SchemaName + ".JobOrderHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".JobOrderLines L ON H.JobOrderHeaderId = L.JobOrderHeaderId "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE L.ProductUidId IS NOT NULL "
                        + "AND Dc.DocumentCategoryName = 'Job Order' "
                        + "UNION ALL "
                        + "SELECT H.JobReceiveHeaderId AS DocHeaderId, L.JobReceiveLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "H.GodownId AS GodownId, H.ProcessId, Jol.ProductUidId, 0 AS Qty_Iss, 1 AS Qty_Rec, H.CreatedDate, L.Remark "
                        + "FROM  " + SchemaName + ".JobReceiveHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".JobReceiveLines L ON H.JobReceiveHeaderId = L.JobReceiveHeaderId "
                        + "LEFT JOIN " + SchemaName + ".JobOrderLines Jol ON L.JobOrderLineId = Jol.JobOrderLineId "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE Jol.ProductUidId IS NOT NULL "
                        + "AND Dc.DocumentCategoryName = 'Job Receive' "
                        + "UNION ALL "
                        + "SELECT H.JobOrderCancelHeaderId AS DocHeaderId, L.JobOrderCancelLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "H.GodownId AS GodownId, Joh.ProcessId, Jol.ProductUidId, 0 AS Qty_Iss, 1 AS Qty_Rec, H.CreatedDate, L.Remark "
                        + "FROM  " + SchemaName + ".JobOrderCancelHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".JobOrderCancelLines L ON H.JobOrderCancelHeaderId = L.JobOrderCancelHeaderId "
                        + "LEFT JOIN " + SchemaName + ".JobOrderLines Jol ON L.JobOrderLineId = Jol.JobOrderLineId "
                        + "LEFT JOIN " + SchemaName + ".JobOrderHeaders Joh ON Jol.JobOrderHeaderId = Joh.JobOrderHeaderId "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE Jol.ProductUidId IS NOT NULL "
                        + "AND Dc.DocumentCategoryName = 'Job Order Cancel' "
                        + "UNION ALL "
                        + "SELECT H.PurchaseGoodsReceiptHeaderId AS DocHeaderId, L.PurchaseGoodsReceiptLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "H.GodownId AS GodownId, NULL AS ProcessId, L.ProductUidId, 0 AS Qty_Iss, 1 AS Qty_Rec, H.CreatedDate, H.Remark "
                        + "FROM  " + SchemaName + ".PurchaseGoodsReceiptHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".PurchaseGoodsReceiptLines L ON H.PurchaseGoodsReceiptHeaderId = L.PurchaseGoodsReceiptHeaderId "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE L.ProductUidId IS NOT NULL "
                        + "AND Dc.DocumentCategoryName = 'Purchase Challan' "
                        + "UNION ALL "
                        + "SELECT H.StockHeaderId AS DocHeaderId, L.StockLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "H.FromGodownId AS GodownId, H.ProcessId, L.ProductUidId, 1 AS Qty_Iss, 0 AS Qty_Rec, H.CreatedDate, L.Remark "
                        + "FROM  " + SchemaName + ".StockHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".StockLines L ON H.StockHeaderId = L.StockHeaderId  "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE L.ProductUidId IS NOT NULL "
                        + "AND Dc.DocumentCategoryName = 'Stock Transfer' "
                        + "UNION ALL "
                        + "SELECT H.StockHeaderId AS DocHeaderId, L.StockLineId AS DocLineId, H.DocTypeId, H.DocDate, H.DivisionId, H.SiteId,  "
                        + "H.GodownId AS GodownId, H.ProcessId, L.ProductUidId, 0 AS Qty_Iss, 1 AS Qty_Rec, H.CreatedDate, L.Remark "
                        + "FROM  " + SchemaName + ".StockHeaders H  "
                        + "LEFT JOIN " + SchemaName + ".StockLines L ON H.StockHeaderId = L.StockHeaderId  "
                        + "LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId "
                        + "LEFT JOIN " + SchemaName + ".DocumentCategories Dc ON Dt.DocumentCategoryId = Dc.DocumentCategoryId "
                        + "WHERE L.ProductUidId IS NOT NULL "
                        + "AND Dc.DocumentCategoryName = 'Stock Transfer' ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }



            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewJobOrderHeader') IS NOT NULL DROP VIEW " + SchemaName + ".ViewJobOrderHeader  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewJobOrderHeader AS  "
                        + "SELECT H.JobOrderHeaderId, Max(H.DocTypeId) AS DocTypeId, Max(H.DocDate) AS DocDate, Max(H.DocNo) AS DocNo, Max(H.DivisionId) AS DivisionId,  "
                        + "max(H.SiteId) AS SiteId, Max(H.DueDate) AS DueDate, Max(H.JobWorkerId) AS JobWorkerId, Max(H.BillToPartyId) AS BillToPartyId,  "
                        + "Max(H.OrderById) AS OrderById, Max(H.GodownId) AS GodownId, Max(H.JobInstructionId) AS JobInstructionId, Max(H.TermsAndConditions) AS TermsAndConditions, "
                        + "Max(H.ProcessId) AS ProcessId, Max(H.CostCenterId) AS ConstCenterId, Max(H.MachineId) AS MachineId, "
                        + "max(H.Status) AS Status, max(H.Remark) AS Remark,  "
                        + "Max(H.CreatedBy) AS CreatedBy, Max(H.CreatedDate) AS CreatedDate, max(H.ModifiedBy) AS ModifiedBy, max(H.ModifiedDate) AS ModifiedDate, "
                        + "sum(L.Qty) AS TotalQty, sum(L.Amount) AS TotalAmount "
                        + "FROM  " + SchemaName + ".JobOrderHeaders H "
                        + "LEFT JOIN " + SchemaName + ".JobOrderLines L ON L.JobOrderHeaderId = H.JobOrderHeaderId   "
                        + "GROUP BY H.JobOrderHeaderId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewSaleInvoiceLine') IS NOT NULL DROP VIEW " + SchemaName + ".ViewSaleInvoiceLine  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewSaleInvoiceLine AS "
                        + " SELECT L.SaleInvoiceHeaderId, L.SaleInvoiceLineId, PL.ProductUidId, PL.ProductID, PL.Specification , PL.SaleOrderLineId , PL.Qty, PL.BaleNo, "
                        + "PL.DealQty, PL.DealUnitId, PL.GrossWeight, PL.NetWeight, L.Rate, L.Amount, L.Remark   "
                        + "FROM SaleInvoiceLines L "
                        + "LEFT JOIN " + SchemaName + ".SaleDispatchLines DL ON L.SaleDispatchLineId = DL.SaleDispatchLineId "
                        + "LEFT JOIN " + SchemaName + ".PackingLines PL ON DL.PackingLineId = PL.PackingLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewJobOrderLine') IS NOT NULL DROP VIEW " + SchemaName + ".ViewJobOrderLine  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewJobOrderLine AS "
                        + "SELECT VJobOrder.JobOrderLineId, Max(VJobOrder.JobOrderHeaderId) AS JobOrderHeaderId, "
                        + "IsNull(Sum(VJobOrder.Qty),0) AS OrderQty, IsNull(Sum(VJobOrder.Rate),0) AS Rate, IsNull(Sum(VJobOrder.Qty),0) * IsNull(Sum(VJobOrder.Rate),0) AS OrderAmount,  "
                        + "Max(VJobOrder.ProductId) AS ProductId, Max(VJobOrder.DeliveryUnitId) AS DeliveryUnitId, Max(VJobOrder.Remark) AS Remark "
                        + "FROM  (  "
                        + "SELECT L.JobOrderLineId, L.Qty , L.Rate , L.JobOrderHeaderId, L.ProductId, L.DeliveryUnitId, L.Remark    "
                        + "FROM  " + SchemaName + ".JobOrderLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, - L.Qty, 0 AS Rate, NULL AS JobOrderHeaderId, NULL AS ProductId, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".JobOrderCancelLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, L.Qty, 0 AS Rate , NULL AS JobOrderHeaderId, NULL AS ProductId, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".JobOrderQtyAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, 0 AS Qty, L.Rate , NULL AS JobOrderHeaderId, NULL AS ProductId, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".JobOrderRateAmendmentLines L  ) AS VJobOrder GROUP BY VJobOrder.JobOrderLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewJobOrderBalance') IS NOT NULL DROP VIEW " + SchemaName + ".ViewJobOrderBalance  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewJobOrderBalance AS "
                        + "SELECT VJobOrder.JobOrderLineId, IsNull(Sum(VJobOrder.Qty),0) AS BalanceQty, IsNull(Sum(VJobOrder.Rate),0) AS Rate,  "
                        + "IsNull(Sum(VJobOrder.Qty),0) * IsNull(Sum(VJobOrder.Rate),0) AS BalanceAmount, Max(VJobOrder.JobOrderHeaderId) AS JobOrderHeaderId,  "
                        + "Max(VJobOrder.JobOrderNo) AS JobOrderNo,  Max(VJobOrder.ProductId) AS ProductId, Max(VJobOrder.BuyerId) AS BuyerId,  "
                        + "Max(VJobOrder.DocDate) AS OrderDate  "
                        + "FROM  (  "
                        + "SELECT L.JobOrderLineId, L.Qty , L.Rate , H.JobOrderHeaderId, H.DocNo AS JobOrderNo,  "
                        + "L.ProductId, H.JobWorkerId AS BuyerId, H.DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderLines L   "
                        + "LEFT JOIN " + SchemaName + ".JobOrderHeaders H ON L.JobOrderHeaderId = H.JobOrderHeaderId  "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, - L.Qty, 0 AS Rate, NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderCancelLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, L.Qty, 0 AS Rate , NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderQtyAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, 0 AS Qty, L.Rate , NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderRateAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId,  - L.Qty, 0 AS Rate ,NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate   "
                        + "FROM  " + SchemaName + ".JobReceiveLines L   "
                        + ") AS VJobOrder  "
                        + "GROUP BY VJobOrder.JobOrderLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewJobOrderLine') IS NOT NULL DROP VIEW " + SchemaName + ".ViewJobOrderLine  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewJobOrderLine AS "
                        + "SELECT VJobOrder.JobOrderLineId, IsNull(Sum(VJobOrder.Qty),0) AS BalanceQty, IsNull(Sum(VJobOrder.Rate),0) AS Rate, "
                        + "IsNull(Sum(VJobOrder.Qty),0) * IsNull(Sum(VJobOrder.Rate),0) AS BalanceAmount, Max(VJobOrder.JobOrderHeaderId) AS JobOrderHeaderId,  "
                        + "Max(VJobOrder.JobOrderNo) AS JobOrderNo,  Max(VJobOrder.ProductId) AS ProductId, Max(VJobOrder.BuyerId) AS BuyerId,  "
                        + "Max(VJobOrder.DocDate) AS OrderDate  "
                        + "FROM  (  "
                        + "SELECT L.JobOrderLineId, L.Qty , L.Rate , H.JobOrderHeaderId, H.DocNo AS JobOrderNo,  "
                        + "L.ProductId, H.JobWorkerId AS BuyerId, H.DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderLines L   "
                        + "LEFT JOIN " + SchemaName + ".JobOrderHeaders H ON L.JobOrderHeaderId = H.JobOrderHeaderId  "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, - L.Qty, 0 AS Rate, NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderCancelLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, L.Qty, 0 AS Rate , NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderQtyAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId, 0 AS Qty, L.Rate , NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".JobOrderRateAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.JobOrderLineId,  - L.Qty, 0 AS Rate ,NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL AS JobWorkerId,  "
                        + "NULL AS DocDate   "
                        + "FROM  " + SchemaName + ".JobReceiveLines L   "
                        + ") AS VJobOrder  "
                        + "GROUP BY VJobOrder.JobOrderLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewPurchaseOrderHeader') IS NOT NULL DROP VIEW " + SchemaName + ".ViewPurchaseOrderHeader  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewPurchaseOrderHeader AS "
                        + "SELECT H.PurchaseOrderHeaderId, Max(H.DocTypeId) AS DocTypeId, Max(H.DocDate) AS DocDate, Max(H.DocNo) AS DocNo, Max(H.DivisionId) AS DivisionId,max(H.SiteId) AS SiteId,  "
                        + "Max(H.DueDate) AS DueDate, Max(H.SupplierId) AS SupplierId,  "
                        + "Max(H.ShipMethodId) AS ShipMethodId, Max(H.DeliveryTermsId) AS DeliveryTermsId, "
                        + "Max(H.SupplierShipDate) AS SupplierShipDate, Max(H.SupplierRemark) AS SupplierRemark, "
                        + "Max(H.CreditDays) AS CreditDays, Max(H.ProgressPer) AS ProgressPer,  "
                        + "max(H.Status) AS Status, max(H.Remark) AS Remark,  "
                        + "Max(H.CreatedBy) AS CreatedBy, Max(H.CreatedDate) AS CreatedDate, max(H.ModifiedBy) AS ModifiedBy, max(H.ModifiedDate) AS ModifiedDate, "
                        + "sum(L.Qty) AS TotalQty, sum(L.Amount) AS TotalAmount "
                        + "FROM  " + SchemaName + ".PurchaseOrderHeaders H "
                        + "LEFT JOIN " + SchemaName + ".PurchaseOrderLines L ON L.PurchaseOrderHeaderId = H.PurchaseOrderHeaderId   "
                        + "GROUP BY H.PurchaseOrderHeaderId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }



            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewPurchaseOrderLine') IS NOT NULL DROP VIEW " + SchemaName + ".ViewPurchaseOrderLine  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewPurchaseOrderLine AS "
                        + "SELECT VPurchaseOrder.PurchaseOrderLineId, Max(VPurchaseOrder.PurchaseOrderHeaderId) AS PurchaseOrderHeaderId, "
                        + "IsNull(Sum(VPurchaseOrder.Qty),0) AS OrderQty, IsNull(Sum(VPurchaseOrder.Rate),0) AS Rate, IsNull(Sum(VPurchaseOrder.Qty),0) * IsNull(Sum(VPurchaseOrder.Rate),0) AS OrderAmount,  "
                        + "Max(VPurchaseOrder.ProductId) AS ProductId, Max(VPurchaseOrder.DeliveryUnitId) AS DeliveryUnitId, Max(VPurchaseOrder.Remark) AS Remark "
                        + "FROM  (  "
                        + "SELECT L.PurchaseOrderLineId, L.Qty , L.Rate , L.PurchaseOrderHeaderId, L.ProductId, L.DeliveryUnitId, L.Remark    "
                        + "FROM  " + SchemaName + ".PurchaseOrderLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseOrderLineId, - L.Qty, 0 AS Rate, NULL AS PurchaseOrderHeaderId, NULL AS ProductId, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".PurchaseOrderCancelLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseOrderLineId, L.Qty, 0 AS Rate , NULL AS PurchaseOrderHeaderId, NULL AS ProductId, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".PurchaseOrderQtyAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseOrderLineId, 0 AS Qty, L.Rate , NULL AS PurchaseOrderHeaderId, NULL AS ProductId, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".PurchaseOrderRateAmendmentLines L   "
                        + ") AS VPurchaseOrder  "
                        + "GROUP BY VPurchaseOrder.PurchaseOrderLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewPurchaseOrderBalance') IS NOT NULL DROP VIEW " + SchemaName + ".ViewPurchaseOrderBalance  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewPurchaseOrderBalance AS "
                        + "SELECT VPurchaseOrder.PurchaseOrderLineId, IsNull(Sum(VPurchaseOrder.Qty),0) AS BalanceQty, IsNull(Sum(VPurchaseOrder.Rate),0) AS Rate,  "
                        + "IsNull(Sum(VPurchaseOrder.Qty),0) * IsNull(Sum(VPurchaseOrder.Rate),0) AS BalanceAmount, Max(VPurchaseOrder.PurchaseOrderHeaderId) AS PurchaseOrderHeaderId,  "
                        + "Max(VPurchaseOrder.PurchaseOrderNo) AS PurchaseOrderNo,  Max(VPurchaseOrder.ProductId) AS ProductId, Max(VPurchaseOrder.SupplierId) AS SupplierId,  "
                        + "Max(VPurchaseOrder.DocDate) AS OrderDate  "
                        + "FROM  (  "
                        + "SELECT L.PurchaseOrderLineId, L.Qty , L.Rate , H.PurchaseOrderHeaderId, H.DocNo AS PurchaseOrderNo,  "
                        + "L.ProductId, H.SupplierId AS SupplierId, H.DocDate  "
                        + "FROM  " + SchemaName + ".PurchaseOrderLines L   "
                        + "LEFT JOIN " + SchemaName + ".PurchaseOrderHeaders H ON L.PurchaseOrderHeaderId = H.PurchaseOrderHeaderId  "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseOrderLineId, - L.Qty, 0 AS Rate, NULL AS PurchaseOrderHeaderId, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".PurchaseOrderCancelLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseOrderLineId, L.Qty, 0 AS Rate , NULL AS PurchaseOrderHeaderId, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".PurchaseOrderQtyAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseOrderLineId, 0 AS Qty, L.Rate , NULL AS PurchaseOrderHeaderId, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".PurchaseOrderRateAmendmentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseOrderLineId,  - L.Qty, 0 AS Rate ,NULL AS PurchaseOrderHeaderId, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  "
                        + "NULL AS DocDate   "
                        + "FROM  " + SchemaName + ".PurchaseGoodsReceiptLines L   "
                        + ") AS VPurchaseOrder  "
                        + "GROUP BY VPurchaseOrder.PurchaseOrderLineId  ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewSaleOrderHeader') IS NOT NULL DROP VIEW " + SchemaName + ".ViewSaleOrderHeader  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewSaleOrderHeader AS "
                        + " SELECT H.SaleOrderHeaderId, Max(H.DocTypeId) AS DocTypeId, Max(H.DocDate) AS DocDate, Max(H.DocNo) AS DocNo, Max(H.DivisionId) AS DivisionId, "
                        + " max(H.SiteId) AS SiteId, Max(H.DueDate) AS DueDate, Max(H.SaleToBuyerId) AS SaleToBuyerId, Max(H.BillToBuyerId) AS BillToBuyerId,  "
                        + " max(CurrencyId) AS CurrencyId, max(Priority) AS Priority, max(ShipMethodId) AS ShipMethodId, Max(ShipAddress) AS ShipAddress,  "
                        + " Max(H.DeliveryTermsId) AS DeliveryTermsId, max(H.Status) AS Status, max(H.Remark) AS Remark, max(H.BuyerOrderNo) AS BuyerOrderNo,  "
                        + " Max(H.CreditDays) AS CreditDays, Max(H.TermsAndConditions) AS TermsAndConditions, "
                        + " Max(H.CreatedBy) AS CreatedBy, Max(H.CreatedDate) AS CreatedDate, max(H.ModifiedBy) AS ModifiedBy, max(H.ModifiedDate) AS ModifiedDate, "
                        + " sum(L.Qty) AS TotalQty, sum(L.Amount) AS TotalAmount "
                        + " FROM  " + SchemaName + ".SaleOrderHeaders H "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderLines L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId "
                        + " GROUP BY H.SaleOrderHeaderId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewSaleOrderLine') IS NOT NULL DROP VIEW " + SchemaName + ".ViewSaleOrderLine  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewSaleOrderLine AS "
                        + " SELECT VSaleOrder.SaleOrderLineId, Max(VSaleOrder.SaleOrderHeaderId) AS SaleOrderHeaderId, "
                        + " IsNull(Sum(VSaleOrder.Qty),0) AS OrderQty, IsNull(Sum(VSaleOrder.CancelQty),0) AS CancelQty, IsNull(Sum(VSaleOrder.Rate),0) AS Rate, Round(IsNull(Sum(VSaleOrder.Qty),0) * IsNull(Sum(VSaleOrder.Rate),0),2) AS OrderAmount, Round(IsNull(Sum(VSaleOrder.CancelQty),0)* IsNull(Sum(VSaleOrder.Rate),0),2) AS CancelAmount, "
                        + " Max(VSaleOrder.ProductId) AS ProductId, Max(VSaleOrder.DueDate) AS DueDate, Max(VSaleOrder.Specification) AS Specification, Max(VSaleOrder.DeliveryUnitId) AS DeliveryUnitId, Max(VSaleOrder.Remark) AS Remark "
                        + " FROM  (  "
                        + " SELECT L.SaleOrderLineId, L.Qty,  0 AS CancelQty, L.DueDate, L.Rate, L.SaleOrderHeaderId, L.ProductId, L.Specification, L.DeliveryUnitId, L.Remark "
                        + " FROM  " + SchemaName + ".SaleOrderLines L   "
                        + " UNION ALL  "
                        + " SELECT L.SaleOrderLineId, 0 AS Qty, sum(L.Qty) AS CancelQty, NULL AS DueDate, 0 AS Rate, NULL AS SaleOrderHeaderId, NULL AS ProductId, NULL AS Specification, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + " FROM  " + SchemaName + ".SaleOrderCancelLines L   "
                        + " GROUP BY L.SaleOrderLineId  "
                        + " UNION  "
                        + " ALL SELECT L.SaleOrderLineId, sum(L.Qty) AS Qty, 0 AS CancelQty, NULL AS DueDate,  0 AS Rate , NULL AS SaleOrderHeaderId, NULL AS ProductId, NULL AS Specification, NULL AS DeliveryUnitId, NULL AS Remark "
                        + " FROM  " + SchemaName + ".SaleOrderQtyAmendmentLines L   "
                        + " GROUP BY L.SaleOrderLineId  "
                        + " UNION ALL  "
                        + " SELECT L.SaleOrderLineId, 0 AS Qty,0 AS CancelQty, NULL AS DueDate, Sum(L.Rate) AS Rate , NULL AS SaleOrderHeaderId, NULL AS ProductId, NULL AS Specification, NULL AS DeliveryUnitId, NULL AS Remark  "
                        + " FROM  " + SchemaName + ".SaleOrderRateAmendmentLines L   "
                        + " GROUP BY L.SaleOrderLineId  "
                        + " ) AS VSaleOrder GROUP BY VSaleOrder.SaleOrderLineId ";


                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewSaleOrderLineTheir') IS NOT NULL DROP VIEW " + SchemaName + ".ViewSaleOrderLineTheir  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewSaleOrderLineTheir AS "
                        + " SELECT VSaleOrder.SaleOrderLineId, Max(VSaleOrder.SaleOrderHeaderId) AS SaleOrderHeaderId, "
                        + " IsNull(Sum(VSaleOrder.Qty),0) AS OrderQty, IsNull(Sum(VSaleOrder.DeliveryQty),0) AS DeliveryQty, IsNull(Sum(VSaleOrder.Rate),0) AS Rate, Round(IsNull(Sum(VSaleOrder.Qty),0) * IsNull(Sum(VSaleOrder.Rate),0),2) AS OrderAmount,  "
                        + " Max(VSaleOrder.ProductId) AS ProductId, CASE WHEN max(VSaleOrder.DueDate)='01/01/1900 12:00:00 AM' THEN NULL ELSE  max(VSaleOrder.DueDate) END AS DueDate, Max(VSaleOrder.Specification) AS Specification, Max(VSaleOrder.DeliveryUnitId) AS DeliveryUnitId, Max(VSaleOrder.Remark) AS Remark, "
                        + " IsNull(Sum(VSaleOrder.CancelCount),0) AS CancelCount, IsNull(Sum(VSaleOrder.AmendmentCount),0) AS AmendmentCount, IsNull(Sum(VSaleOrder.RateAmdCount),0) AS RateAmdCount "
                        + " FROM  (  "
                        + " SELECT L.SaleOrderLineId, L.Qty, L.DeliveryQty, L.DueDate, L.Rate, L.SaleOrderHeaderId, L.ProductId, L.Specification, L.DeliveryUnitId, L.Remark, 0 AS CancelCount,  0 AS AmendmentCount , 0 AS RateAmdCount    "
                        + " FROM  " + SchemaName + ".SaleOrderLines L   "
                        + " UNION ALL  "
                        + " SELECT L.SaleOrderLineId, - sum(L.Qty) AS Qty,  NULL AS DueDate, 0 AS DeliveryQty, 0 AS Rate, NULL AS SaleOrderHeaderId, NULL AS ProductId, NULL AS Specification, NULL AS DeliveryUnitId, NULL AS Remark , count(L.SaleOrderLineId)  AS CancelCount, 0 AS AmendmentCount , 0 AS RateAmdCount "
                        + " FROM  " + SchemaName + ".SaleOrderCancelLines L   "
                        + " GROUP BY L.SaleOrderLineId  "
                        + " UNION  "
                        + " ALL SELECT L.SaleOrderLineId, sum(L.Qty) AS Qty, NULL AS DueDate, 0 AS DeliveryQty, 0 AS Rate , NULL AS SaleOrderHeaderId, NULL AS ProductId, NULL AS Specification, NULL AS DeliveryUnitId, NULL AS Remark, 0 AS CancelCount, count(L.SaleOrderLineId) AS AmendmentCount, 0 AS RateAmdCount "
                        + " FROM  " + SchemaName + ".SaleOrderQtyAmendmentLines L  "
                        + " GROUP BY L.SaleOrderLineId  "
                        + " UNION ALL  "
                        + " SELECT L.SaleOrderLineId, 0 AS Qty, 0 AS DeliveryQty, NULL AS DueDate, Sum(L.Rate) AS Rate , NULL AS SaleOrderHeaderId, NULL AS ProductId, NULL AS Specification, NULL AS DeliveryUnitId, NULL AS Remark , 0 AS CancelCount, 0 AS AmendmentCount, 0 AS RateAmdCount "
                        + " FROM  " + SchemaName + ".SaleOrderRateAmendmentLines L   "
                        + " GROUP BY L.SaleOrderLineId  "
                        + " ) AS VSaleOrder GROUP BY VSaleOrder.SaleOrderLineId ";


                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewSaleInvoiceHeader') IS NOT NULL DROP VIEW " + SchemaName + ".ViewSaleInvoiceHeader  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewSaleInvoiceHeader AS "
                        + " SELECT H.SaleInvoiceHeaderId, Max(H.DocTypeId) AS DocTypeId, Max(H.DocDate) AS DocDate, Max(H.DocNo) AS DocNo, Max(H.DivisionId) AS DivisionId, "
                        + " max(H.SiteId) AS SiteId,  Max(H.BillToBuyerId) AS BillToBuyerId, max(CurrencyId) AS CurrencyId,  "
                        + " max(H.Status) AS Status, max(SIHD.BaleNoSeries) AS BaleNoStr, max(H.Remark) AS Remark,  "
                        + " max(SIHD.BLNo) AS LrNo,	max(SIHD.BLDate) AS LrDate,	max(SIHD.PrivateMark) AS PrivateMark,	max(SIHD.PortOfLoading) AS PortOfLoading,   "
                        + " max(SIHD.DestinationPort) AS DestinationPort,	max(SIHD.FinalPlaceOfDelivery) AS FinalPlaceOfDelivery,	max(SIHD.PreCarriageBy) AS PreCarriageBy, "
                        + " max(SIHD.PlaceOfPreCarriage) AS PlaceOfPreCarriage,	max(SIHD.CircularNo) AS CircularNo,	max(SIHD.CircularDate) AS CircularDate,	max(SIHD.OrderNo) AS OrderNo,	max(SIHD.OrderDate) AS OrderDate,   "
                        + " 	max(PL.BaleNo) AS RollNo,	max(SIHD.DescriptionOfGoods) AS DescriptionOfGoods,	max(SIHD.KindsOfackages) AS KindsOfackages, "
                        + " max(SIHD.Compositions) AS Compositions,	max(SIHD.OtherRefrence) AS OtherRefrence,	max(SIHD.TermsOfSale) AS TermsOfSale,	max(SIHD.NotifyParty) AS NotifyParty, "
                        + " 	max(SIHD.TransporterInformation) AS TransporterInformation,	 "
                        + " Max(H.CreatedBy) AS CreatedBy, Max(H.CreatedDate) AS CreatedDate, max(H.ModifiedBy) AS ModifiedBy, max(H.ModifiedDate) AS ModifiedDate, "
                        + " sum(PL.Qty) AS TotalQty,  "
                        + " sum(L.Amount) AS TotalAmount "
                        + " FROM  " + SchemaName + ".SaleInvoiceHeaders H "
                        + " LEFT JOIN " + SchemaName + ".SaleInvoiceHeaderDetail SIHD ON  SIHD.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".SaleInvoiceLines L ON L.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId   "
                        + " LEFT JOIN " + SchemaName + ".SaleDispatchLines SDL ON SDL.SaleDispatchLineId = L.SaleDispatchLineId "
                        + " LEFT JOIN " + SchemaName + ".PackingLines PL ON PL.PackingLineId  = SDL.PackingLineId  "
                        + " GROUP BY H.SaleInvoiceHeaderId ";


                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewSaleInvoiceLineX') IS NOT NULL DROP VIEW " + SchemaName + ".ViewSaleInvoiceLineX  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewSaleInvoiceLineX AS "
                        + " SELECT H.SaleInvoiceHeaderId, PL.SaleOrderLineId, Max(H.DocTypeId) AS DocTypeId, Max(H.DocDate) AS DocDate, Max(H.DocNo) AS DocNo, Max(H.DivisionId) AS DivisionId, "
                        + " max(H.SiteId) AS SiteId,  Max(H.BillToBuyerId) AS BillToBuyerId, max(CurrencyId) AS CurrencyId,  "
                        + " max(H.Status) AS Status, max(SIHD.BaleNoSeries) AS BaleNoStr, max(H.Remark) AS Remark,  "
                        + " max(SIHD.BLNo) AS LrNo,	max(SIHD.BLDate) AS LrDate,	max(SIHD.PrivateMark) AS PrivateMark,	max(SIHD.PortOfLoading) AS PortOfLoading,	 "
                        + " max(SIHD.DestinationPort) AS DestinationPort,	max(SIHD.FinalPlaceOfDelivery) AS FinalPlaceOfDelivery,	max(SIHD.PreCarriageBy) AS PreCarriageBy, "
                        + " max(SIHD.PlaceOfPreCarriage) AS PlaceOfPreCarriage,	max(SIHD.CircularNo) AS CircularNo,	max(SIHD.CircularDate) AS CircularDate,	max(SIHD.OrderNo) AS OrderNo,	max(SIHD.OrderDate) AS OrderDate,   "
                        + " 	max(PL.BaleNo) AS RollNo,	max(SIHD.DescriptionOfGoods) AS DescriptionOfGoods,	max(SIHD.KindsOfackages) AS KindsOfackages, "
                        + " max(SIHD.Compositions) AS Compositions,	max(SIHD.OtherRefrence) AS OtherRefrence,	max(SIHD.TermsOfSale) AS TermsOfSale,	max(SIHD.NotifyParty) AS NotifyParty,	 "
                        + " max(SIHD.TransporterInformation) AS TransporterInformation,	 "
                        + " Max(H.CreatedBy) AS CreatedBy, Max(H.CreatedDate) AS CreatedDate, max(H.ModifiedBy) AS ModifiedBy, max(H.ModifiedDate) AS ModifiedDate, "
                        + " sum(PL.Qty) AS Qty, sum(L.Amount) AS Amount "
                        + " FROM  " + SchemaName + ".SaleInvoiceHeaders H "
                        + " LEFT JOIN " + SchemaName + ".SaleInvoiceHeaderDetail SIHD ON SIHD.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".SaleInvoiceLines L ON L.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId   "
                        + " LEFT JOIN " + SchemaName + ".SaleDispatchLines SDL ON SDL.SaleDispatchLineId = L.SaleDispatchLineId  "
                        + " LEFT JOIN " + SchemaName + ".PackingLines PL ON PL.PackingLineId  = SDL.PackingLineId  "
                        + " GROUP BY H.SaleInvoiceHeaderId, PL.SaleOrderLineId ";


                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewRugSize') IS NOT NULL DROP VIEW " + SchemaName + ".ViewRugSize  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE VIEW " + SchemaName + ".[ViewRugSize] "
                        + "AS   "
                        + "SELECT P.ProductId, SS.SizeId AS StandardSizeID, SS.SizeName AS StandardSizeName, "
                        + "SS.Area AS StandardSizeArea,  SM.SizeId AS ManufaturingSizeID, SM.SizeName AS ManufaturingSizeName,  "
                        + "SM.Area AS ManufaturingSizeArea,SF.SizeId AS FinishingSizeID, SF.SizeName AS FinishingSizeName, "
                        + "SF.Area AS FinishingSizeArea   "
                        + "FROM " + SchemaName + ".Products P   "
                        + "LEFT JOIN ( "
                        + "    SELECT PSS.ProductId, PSS.SizeId  "
                        + "    FROM " + SchemaName + ".ProductSizes PSS  "
                        + "    WHERE  PSS.ProductSizeTypeId = (SELECT ProductsizetypeID FROM " + SchemaName + ".ProductSizeTypes WHERE ProductsizetypeName = 'Standard')   "
                        + ") AS PSS ON P.ProductId = PSS.ProductId "
                        + "LEFT JOIN ( "
                        + "    SELECT PSM.ProductId, PSM.SizeId  "
                        + "    FROM " + SchemaName + ".ProductSizes PSM  "
                        + "    WHERE PSM.ProductSizeTypeId = (SELECT ProductsizetypeID FROM " + SchemaName + ".ProductSizeTypes WHERE ProductsizetypeName = 'Manufaturing Size')   "
                        + ") AS PSM ON P.ProductId = PSM.ProductId "
                        + "LEFT JOIN ( "
                        + "    SELECT PSF.ProductId, PSF.SizeId  "
                        + "    FROM " + SchemaName + ".ProductSizes PSF  "
                        + "    WHERE PSF.ProductSizeTypeId = ( SELECT ProductsizetypeID FROM " + SchemaName + ".ProductSizeTypes WHERE ProductsizetypeName = 'Finishing Size')   "
                        + ") AS PSF ON P.ProductId = PSF.ProductId "
                        + "LEFT JOIN " + SchemaName + ".Sizes SS ON SS.SizeId = PSS.SizeId   "
                        + "LEFT JOIN " + SchemaName + ".Sizes SM ON SM.SizeId = PSM.SizeId   "
                        + "LEFT JOIN " + SchemaName + ".Sizes SF ON SF.SizeId = PSF.SizeId   "
                        + "LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = P.ProductCategoryId ";


                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewSaleOrderBalance') IS NOT NULL DROP VIEW " + SchemaName + ".ViewSaleOrderBalance  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewSaleOrderBalance "
                        + "AS  "
                        + "SELECT VSaleOrder.SaleOrderLineId, IsNull(Sum(VSaleOrder.Qty),0) AS BalanceQty, IsNull(Sum(VSaleOrder.Rate),0) AS Rate, "
                        + "IsNull(Sum(VSaleOrder.Qty),0) * IsNull(Sum(VSaleOrder.Rate),0) AS BalanceAmount, "
                        + "Max(VSaleOrder.SaleOrderHeaderId) AS SaleOrderHeaderId, Max(VSaleOrder.SaleOrderNo) AS SaleOrderNo,  "
                        + "Max(VSaleOrder.ProductId) AS ProductId, Max(VSaleOrder.BuyerId) AS BuyerId, Max(VSaleOrder.DocDate) AS OrderDate "
                        + "FROM  ( "
                        + "SELECT L.SaleOrderLineId, L.Qty , L.Rate , "
                        + "H.SaleOrderHeaderId, H.DocNo AS SaleOrderNo, L.ProductId, H.SaleToBuyerId AS BuyerId, H.DocDate "
                        + "FROM  " + SchemaName + ".SaleOrderLines L  "
                        + "LEFT JOIN " + SchemaName + ".SaleOrderHeaders H ON L.SaleOrderHeaderId = H.SaleOrderHeaderId "
                        + "UNION ALL "
                        + "SELECT L.SaleOrderLineId, - L.Qty, 0 AS Rate, "
                        + "NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate "
                        + "FROM  " + SchemaName + ".SaleOrderCancelLines L  "
                        + "UNION ALL "
                        + "SELECT L.SaleOrderLineId, L.Qty, 0 AS Rate , "
                        + "NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate "
                        + "FROM  " + SchemaName + ".SaleOrderQtyAmendmentLines L  "
                        + "UNION ALL "
                        + "SELECT L.SaleOrderLineId, 0 AS Qty, L.Rate , "
                        + "NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate "
                        + "FROM  " + SchemaName + ".SaleOrderRateAmendmentLines L  "
                        + "UNION ALL "
                        + "SELECT Pl.SaleOrderLineId,  - Pl.Qty, 0 AS Rate ,NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".SaleDispatchLines L  "
                        + "LEFT JOIN " + SchemaName + ".PackingLines Pl ON L.PackingLineId = Pl.PackingLineId  "
                        + ") AS VSaleOrder "
                        + "GROUP BY VSaleOrder.SaleOrderLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewPurchaseIndentHeader') IS NOT NULL DROP VIEW " + SchemaName + ".ViewPurchaseIndentHeader  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewPurchaseIndentHeader AS "
                        + "SELECT H.PurchaseIndentHeaderId, Max(H.DocTypeId) AS DocTypeId, Max(H.DocDate) AS DocDate, Max(H.DocNo) AS DocNo, Max(H.DivisionId) AS DivisionId,max(H.SiteId) AS SiteId,  "
                        + "max(H.Status) AS Status, max(H.Remark) AS Remark,  "
                        + "Max(H.CreatedBy) AS CreatedBy, Max(H.CreatedDate) AS CreatedDate, max(H.ModifiedBy) AS ModifiedBy, max(H.ModifiedDate) AS ModifiedDate, "
                        + "sum(L.Qty) AS TotalQty "
                        + "FROM  " + SchemaName + ".PurchaseIndentHeaders H "
                        + "LEFT JOIN " + SchemaName + ".PurchaseIndentLines L ON L.PurchaseIndentHeaderId = H.PurchaseIndentHeaderId   "
                        + "GROUP BY H.PurchaseIndentHeaderId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewPurchaseIndentLine') IS NOT NULL DROP VIEW " + SchemaName + ".ViewPurchaseIndentLine  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewPurchaseIndentLine AS "
                        + "SELECT VPurchaseIndent.PurchaseIndentLineId, Max(VPurchaseIndent.PurchaseIndentHeaderId) AS PurchaseIndentHeaderId, "
                        + "IsNull(Sum(VPurchaseIndent.Qty),0) AS Qty,  "
                        + "Max(VPurchaseIndent.ProductId) AS ProductId, Max(VPurchaseIndent.Remark) AS Remark "
                        + "FROM  (  "
                        + "SELECT L.PurchaseIndentLineId, L.Qty , L.PurchaseIndentHeaderId, L.ProductId, L.Remark    "
                        + "FROM  " + SchemaName + ".PurchaseIndentLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseIndentLineId, - L.Qty, NULL AS PurchaseIndentHeaderId, NULL AS ProductId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".PurchaseIndentCancelLines L   "
                        + ") AS VPurchaseIndent  "
                        + "GROUP BY VPurchaseIndent.PurchaseIndentLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewPurchaseIndentBalance') IS NOT NULL DROP VIEW " + SchemaName + ".ViewPurchaseIndentBalance  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewPurchaseIndentBalance AS "
                        + "SELECT VPurchaseIndent.PurchaseIndentLineId, IsNull(Sum(VPurchaseIndent.Qty),0) AS BalanceQty,  "
                        + "Max(VPurchaseIndent.PurchaseIndentHeaderId) AS PurchaseIndentHeaderId,  "
                        + "Max(VPurchaseIndent.PurchaseIndentNo) AS PurchaseIndentNo,  Max(VPurchaseIndent.ProductId) AS ProductId, "
                        + "Max(VPurchaseIndent.DocDate) AS IndentDate  "
                        + "FROM  (  "
                        + "SELECT L.PurchaseIndentLineId, L.Qty , H.PurchaseIndentHeaderId, H.DocNo AS PurchaseIndentNo,  "
                        + "L.ProductId, H.DocDate  "
                        + "FROM  " + SchemaName + ".PurchaseIndentLines L   "
                        + "LEFT JOIN " + SchemaName + ".PurchaseIndentHeaders H ON L.PurchaseIndentHeaderId = H.PurchaseIndentHeaderId  "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseIndentLineId, - L.Qty, NULL AS PurchaseIndentHeaderId, NULL AS PurchaseIndentNo, NULL AS ProductId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".PurchaseIndentCancelLines L   "
                        + "UNION ALL  "
                        + "SELECT L.PurchaseIndentLineId,  - L.Qty, NULL AS PurchaseIndentHeaderId, NULL AS PurchaseIndentNo, NULL AS ProductId,  "
                        + "NULL AS DocDate   "
                        + "FROM  " + SchemaName + ".PurchaseOrderLines L   "
                        + ") AS VPurchaseIndent  "
                        + "GROUP BY VPurchaseIndent.PurchaseIndentLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewProdOrderHeader') IS NOT NULL DROP VIEW " + SchemaName + ".ViewProdOrderHeader  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewProdOrderHeader AS "
                        + "SELECT H.ProdOrderHeaderId, Max(H.DocTypeId) AS DocTypeId, Max(H.DocDate) AS DocDate, Max(H.DocNo) AS DocNo, Max(H.DivisionId) AS DivisionId,max(H.SiteId) AS SiteId, "
                        + "max(H.DueDate) AS DueDate,  "
                        + "max(H.Status) AS Status, max(H.Remark) AS Remark,  "
                        + "Max(H.CreatedBy) AS CreatedBy, Max(H.CreatedDate) AS CreatedDate, max(H.ModifiedBy) AS ModifiedBy, max(H.ModifiedDate) AS ModifiedDate, "
                        + "sum(L.Qty) AS TotalQty "
                        + "FROM  " + SchemaName + ".ProdOrderHeaders H "
                        + "LEFT JOIN " + SchemaName + ".ProdOrderLines L ON L.ProdOrderHeaderId = H.ProdOrderHeaderId   "
                        + "GROUP BY H.ProdOrderHeaderId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewProdOrderLine') IS NOT NULL DROP VIEW " + SchemaName + ".ViewProdOrderLine  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewProdOrderLine AS "
                        + "SELECT VProdOrder.ProdOrderLineId, Max(VProdOrder.ProdOrderHeaderId) AS ProdOrderHeaderId, "
                        + "IsNull(Sum(VProdOrder.Qty),0) AS Qty,  "
                        + "Max(VProdOrder.ProductId) AS ProductId, Max(VProdOrder.Remark) AS Remark "
                        + "FROM  (  "
                        + "SELECT L.ProdOrderLineId, L.Qty , L.ProdOrderHeaderId, L.ProductId, L.Remark    "
                        + "FROM  " + SchemaName + ".ProdOrderLines L   "
                        + "UNION ALL  "
                        + "SELECT L.ProdOrderLineId, - L.Qty, NULL AS ProdOrderHeaderId, NULL AS ProductId, NULL AS Remark  "
                        + "FROM  " + SchemaName + ".ProdOrderCancelLines L   "
                        + ") AS VProdOrder  "
                        + "GROUP BY VProdOrder.ProdOrderLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }


            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewProdOrderBalance') IS NOT NULL DROP VIEW " + SchemaName + ".ViewProdOrderBalance  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewProdOrderBalance AS "
                        + "SELECT VProdOrder.ProdOrderLineId, IsNull(Sum(VProdOrder.Qty),0) AS BalanceQty, "
                        + "Max(VProdOrder.ProdOrderHeaderId) AS ProdOrderHeaderId,  "
                        + "Max(VProdOrder.ProdOrderNo) AS ProdOrderNo,  Max(VProdOrder.ProductId) AS ProductId, "
                        + "Max(VProdOrder.DocDate) AS IndentDate  "
                        + "FROM  (  "
                        + "SELECT L.ProdOrderLineId, L.Qty , H.ProdOrderHeaderId, H.DocNo AS ProdOrderNo,  "
                        + "L.ProductId, H.DocDate  "
                        + "FROM  " + SchemaName + ".ProdOrderLines L   "
                        + "LEFT JOIN " + SchemaName + ".ProdOrderHeaders H ON L.ProdOrderHeaderId = H.ProdOrderHeaderId  "
                        + "UNION ALL  "
                        + "SELECT L.ProdOrderLineId, - L.Qty, NULL AS ProdOrderHeaderId, NULL AS ProdOrderNo, NULL AS ProductId,  "
                        + "NULL AS DocDate  "
                        + "FROM  " + SchemaName + ".ProdOrderCancelLines L   "
                        + "UNION ALL  "
                        + "SELECT L.ProdOrderLineId,  - L.Qty, NULL AS ProdOrderHeaderId, NULL AS ProdOrderNo, NULL AS ProductId,  "
                        + "NULL AS DocDate   "
                        + "FROM  " + SchemaName + ".JobOrderLines L   "
                        + ") AS VProdOrder  "
                        + "GROUP BY VProdOrder.ProdOrderLineId ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            #region ViewRugArea
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ViewRugArea') IS NOT NULL DROP VIEW " + SchemaName + ".ViewRugArea  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = "CREATE VIEW " + SchemaName + ".ViewRugArea AS "
                        + " SELECT P.ProductId, P.ProductName, S.SizeId, S.ProductShapeId, S.SizeName, S.Area, S.UnitId, "
                        + " (SELECT SqYard FROM  " + SchemaName + ".[FuncConvertSqFeetToSqYard] (S.Area) ) AS SqYardPerPcs "
                        + " FROM  " + SchemaName + ".Products P "
                        + " LEFT JOIN " + SchemaName + ".ProductSizes PS ON PS.ProductId = P.ProductId AND PS.ProductSizeTypeId =1 "
                        + " LEFT JOIN " + SchemaName + ".Sizes S ON S.SizeId = PS.SizeId  "
                        + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = P.ProductCategoryId  "
                        + " WHERE PC.ProductTypeId =1";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            #endregion

        }

        static void CreateDatabseProcedures()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string mQry = "";

            // Proc For Sale Print
            #region ProcSaleOrderPrint
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderPrint') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderPrint  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE Procedure  [" + SchemaName + "].[ProcSaleOrderPrint](@Id INT) "
                        + " As "
                        + " BEGIN "
                        + " SELECT H.SaleOrderHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, H.DueDate, H.BuyerOrderNo, H.Remark, "
                        + " P.Name AS SaleToBuyer, BP.Name AS BillToBuyer, "
                        + " H.ShipAddress, C.Name AS Currency, SM.ShipMethodName, DET.DeliveryTermsName,  "
                        + " CASE WHEN H.Priority = 10 THEN 'High' WHEN H.Priority = -10 THEN 'Low' ELSE 'Normal' END AS Priority, H.CreditDays, H.TermsAndConditions,    "
                        + " L.SaleOrderLineId, PD.ProductName, L.OrderQty AS Qty, L.DeliveryQty, L.DueDate AS LineDueDate, L.Rate, L.OrderAmount AS Amount, L.Remark AS LineRemark,  "
                        + " U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, UD.UnitName AS DeliveryUnit, isnull(UD.DecimalPlaces,0) AS DeliveryUnitDecimalPlace,  "
                        + " H.CreatedBy, H.CreatedDate,  "
                        + " CASE WHEN L.AmendmentCount > 0 THEN 'A ' ELSE '' END + CASE WHEN L.CancelCount >0  THEN 'C ' ELSE '' END + CASE WHEN L.RateAmdCount > 0 THEN 'R ' ELSE '' END AS CurrentStatus, "
                        + " H.ModifiedBy +' ' + replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, H.ModifiedDate,  "
                        + " AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy,  "
                        + " AL.ApproveDate      "
                        + " FROM  " + SchemaName + ".SaleOrderHeaders H "
                        + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                        + " LEFT JOIN " + SchemaName + ".ViewSaleOrderLineTheir L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = H.SaleToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = L.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                        + " LEFT JOIN " + SchemaName + ".Units UD ON UD.UnitId = PD.UnitId  "
                        + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                        + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = H.ShipMethodId "
                        + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = H.DeliveryTermsId  "
                        + " LEFT JOIN "
                        + " ( "
                        + "     SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate   "
                        + "     FROM  " + SchemaName + ".ActivityLogs AL "
                        + "     WHERE AL.ActivityType =2 "
                        + "     GROUP BY AL.DocTypeId, AL.DocId "
                        + " ) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.SaleOrderHeaderId "
                        + " WHERE H.SaleOrderHeaderId	= @Id "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            #endregion

            #region ProcSaleOrderAmendmentPrint
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderAmendmentPrint') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderAmendmentPrint  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE Procedure  [" + SchemaName + "].[ProcSaleOrderAmendmentPrint](@Id INT) "
                        + " As "
                        + " BEGIN "
                        + " SELECT H.SaleOrderAmendmentHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, R.ReasonName, H.Remark, "
                        + " L.SaleOrderQtyAmendmentLineId , SOH.DocNo AS SaleOrderNo, P.Name AS BuyerName, PD.ProductName, L.Qty, L.Remark AS LineRemark, "
                        + " U.UnitName, U.DecimalPlaces,H.CreatedBy, H.CreatedDate,  "
                        + " H.ModifiedBy +' ' + replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, H.ModifiedDate,  "
                        + " AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy,  "
                        + " AL.ApproveDate   "
                        + " FROM  " + SchemaName + ".SaleOrderAmendmentHeaders H "
                        + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                        + " LEFT JOIN " + SchemaName + ".Reasons R ON R.ReasonId = H.ReasonId  "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderQtyAmendmentLines L ON L.SaleOrderAmendmentHeaderId  = H.SaleOrderAmendmentHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = SOH.SaleToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = SOL.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                        + " LEFT JOIN "
                        + " ( "
                        + "     SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate   "
                        + "     FROM  " + SchemaName + ".ActivityLogs AL "
                        + "     WHERE AL.ActivityType =2 "
                        + "     GROUP BY AL.DocTypeId, AL.DocId "
                        + " ) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.SaleOrderAmendmentHeaderId "
                        + " WHERE H.SaleOrderAmendmentHeaderId	= @Id "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion

            #region ProcSaleOrderCancelPrint
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderCancelPrint') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderCancelPrint  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE Procedure  [" + SchemaName + "].[ProcSaleOrderCancelPrint](@Id INT) "
                        + " As "
                        + " BEGIN "
                        + " SELECT H.SaleOrderCancelHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, R.ReasonName, H.Remark, "
                        + " L.SaleOrderCancelLineId, SOH.DocNo AS SaleOrderNo, P.Name AS BuyerName, PD.ProductName, L.Qty, L.Remark AS LineRemark, "
                        + " U.UnitName, U.DecimalPlaces,H.CreatedBy, H.CreatedDate, H.ModifiedBy, H.ModifiedDate, AL.ApproveBy, AL.ApproveDate "
                        + " FROM  " + SchemaName + ".SaleOrderCancelHeaders H "
                        + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                        + " LEFT JOIN " + SchemaName + ".Reasons R ON R.ReasonId = H.ReasonId  "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderCancelLines L ON L.SaleOrderCancelHeaderId = H.SaleOrderCancelHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = SOH.SaleToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = SOL.ProductId "
                        + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                        + " LEFT JOIN "
                        + " ( "
                        + "     SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate "
                        + "     FROM  " + SchemaName + ".ActivityLogs AL "
                        + "     WHERE AL.ActivityType =2 "
                        + "     GROUP BY AL.DocTypeId, AL.DocId "
                        + " ) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.SaleOrderCancelHeaderId "
                        + " WHERE H.SaleOrderCancelHeaderId	= @Id "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion

            #region ProcSaleInvoicePrint
            try
            {
                mQry = " IF OBJECT_ID ('dbo.ProcSaleInvoicePrint') IS NOT NULL DROP PROCEDURE dbo.ProcSaleInvoicePrint  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE PROCEDURE [dbo].[ProcSaleInvoicePrint](@Id INT) "
                       + " AS  "
                       + " BEGIN "
                       + " SELECT H.SaleInvoiceHeaderId, H.DocDate, H.DocNo, P.Name AS BillToBuyerName, VAddress.BillToBuyerAddress, "
                       + " VAddress.BIllToPartyCity, VAddress.BillToPartyCountry, "
                       + " C.Name AS CurrencyName,  "
                       + " Hd.BLNo, Hd.BLDate, Hd.PrivateMark, Hd.PortOfLoading, Hd.DestinationPort, Hd.FinalPlaceOfDelivery, Hd.PreCarriageBy,  "
                       + " Hd.PlaceOfPreCarriage, Hd.CircularNo, Hd.CircularDate, Hd.OrderNo, Hd.OrderDate, Hd.BaleNoSeries, Hd.DescriptionOfGoods,  "
                       + " Hd.PackingMaterialDescription, Hd.KindsOfackages,  "
                       + " Hd.Compositions, Hd.OtherRefrence, Hd.TermsOfSale, Hd.NotifyParty, Hd.TransporterInformation,  "
                       + " Sm.ShipMethodName, Dt.DeliveryTermsName,  "
                       + " Prod.ProductName AS ProductName, Prod.ProductSpecification, Pg.ProductGroupName AS ProductDesignName, PCol.ColourName AS ProductColourName,  "
                       + " Pt.ProductTypeName, VProductArea.SizeName AS ProductSizeName, Pig.ProductInvoiceGroupName,  "
                       + " Soh.DocNo AS SaleOrderNo, "
                       + " Pig.ItcHsCode, Pig.Knots, Pl.BaleNo, Try_Parse(Pl.BaleNo AS INT) AS BaleNoToSort, Pl.Qty, L.Rate, L.Amount,  "
                       + " Pl.Qty * IsNull(VProductArea.SqFeetArea,0) AS SqFeetArea, Pl.Qty * IsNull(VProductArea.SqMeterArea,0) AS SqMeterArea  , "
                       + " Pl.GrossWeight, Pl.NetWeight, "
                       + " VSaleInvoice.TotalRugGrossWeight,VSaleInvoice.TotalFinishedProductGrossWeight, "
                       + " VSaleInvoice.TotalGrossWeight,VSaleInvoice.TotalRugNetWeight,VSaleInvoice.TotalFinishedProductNetWeight, "
                       + " VSaleInvoice.TotalNetWeight,VSaleInvoice.TotalRugQty,VSaleInvoice.TotalFinishedProductQty, VSaleInvoice.TotalQty, "
                       + " VBaleCount.TotalRugRolls, VBaleCount.TotalFinishedProductRolls, VBaleCount.TotalRolls, "
                       + " CASE WHEN VBaleCount.TotalRugRolls <> 0 AND VBaleCount.TotalFinishedProductRolls <> 0  "
                       + "      THEN Convert(NVARCHAR,VBaleCount.TotalRugRolls) +  ' + '  + Convert(NVARCHAR,VBaleCount.TotalFinishedProductRolls) + ' = ' + Convert(NVARCHAR,VBaleCount.TotalRolls) "
                       + "      WHEN VBaleCount.TotalRugRolls <> 0 AND VBaleCount.TotalFinishedProductRolls = 0  "
                       + "      THEN Convert(NVARCHAR, VBaleCount.TotalRugRolls) 	 "
                       + "      WHEN VBaleCount.TotalRugRolls = 0 AND VBaleCount.TotalFinishedProductRolls <> 0  "
                       + "      THEN Convert(NVARCHAR,VBaleCount.TotalFinishedProductRolls) 	 "
                       + " END AS TotalRoleText, "
                       + " CASE WHEN VSaleInvoice.TotalRugQty <> 0 AND VSaleInvoice.TotalFinishedProductQty <> 0  "
                       + "      THEN Convert(NVARCHAR, Convert(INT,VSaleInvoice.TotalRugQty)) +  ' + '  + Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalFinishedProductQty)) + ' = ' + Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalQty))  "
                       + "      WHEN VSaleInvoice.TotalRugQty <> 0 AND VSaleInvoice.TotalFinishedProductQty = 0   "
                       + "      THEN Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalRugQty))   "
                       + "      WHEN VSaleInvoice.TotalRugQty = 0 AND VSaleInvoice.TotalFinishedProductQty <> 0   "
                       + "      THEN Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalFinishedProductQty))     "
                       + " END AS TotalQtyText,  "
                       + " CASE WHEN VSaleInvoice.TotalRugGrossWeight <> 0 AND VSaleInvoice.TotalFinishedProductGrossWeight <> 0   "
                       + "      THEN Convert(NVARCHAR, VSaleInvoice.TotalRugGrossWeight) +  ' + '  + Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductGrossWeight) + ' = ' + Convert(NVARCHAR,VSaleInvoice.TotalGrossWeight)  "
                       + "      WHEN VSaleInvoice.TotalRugGrossWeight <> 0 AND VSaleInvoice.TotalFinishedProductGrossWeight = 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalRugGrossWeight)   "
                       + "      WHEN VSaleInvoice.TotalRugGrossWeight = 0 AND VSaleInvoice.TotalFinishedProductGrossWeight <> 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductGrossWeight)     "
                       + " END AS TotalGrossWeightText,  "
                       + " CASE WHEN VSaleInvoice.TotalRugNetWeight <> 0 AND VSaleInvoice.TotalFinishedProductNetWeight <> 0   "
                       + "      THEN Convert(NVARCHAR, VSaleInvoice.TotalRugNetWeight) +  ' + '  + Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductNetWeight) + ' = ' + Convert(NVARCHAR,VSaleInvoice.TotalNetWeight) "
                       + "      WHEN VSaleInvoice.TotalRugNetWeight <> 0 AND VSaleInvoice.TotalFinishedProductNetWeight = 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalRugNetWeight)   "
                       + "      WHEN VSaleInvoice.TotalRugNetWeight = 0 AND VSaleInvoice.TotalFinishedProductNetWeight <> 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductNetWeight)     "
                       + " END AS TotalNetWeightText  "
                       + " FROM SaleInvoiceHeaders H   "
                       + " LEFT JOIN SaleInvoiceHeaderDetail Hd ON H.SaleInvoiceHeaderId = Hd.SaleInvoiceHeaderId  "
                       + " LEFT JOIN SaleDispatchHeaders Dh ON Hd.SaleDispatchHeaderId = Dh.SaleDispatchHeaderId  "
                       + " LEFT JOIN People P ON H.BillToBuyerId = P.PersonID  "
                       + " LEFT JOIN ( "
                       + "     SELECT H.PersonId, Max(H.Address) AS BillToBuyerAddress, Max(C.CityName) AS BillToPartyCity, Max(Con.CountryName) AS BillToPartyCountry  "
                       + "     FROM PersonAddresses H   "
                       + "     LEFT JOIN Cities C ON H.CityId = C.CityId  "
                       + "     LEFT JOIN States S ON C.StateId = S.StateId  "
                       + "     LEFT JOIN Countries Con ON S.CountryId = Con.CountryId  "
                       + "     GROUP BY H.PersonId  "
                       + " ) AS VAddress ON P.PersonId = VAddress.PersonId  "
                       + " LEFT JOIN Currencies C ON H.CurrencyId = C.ID  "
                       + " LEFT JOIN DeliveryTerms Dt ON Dh.DeliveryTermsId = Dt.DeliveryTermsId  "
                       + " LEFT JOIN ShipMethods Sm ON Dh.ShipMethodId = Sm.ShipMethodId  "
                       + " LEFT JOIN SaleInvoiceLines L  ON H.SaleInvoiceHeaderId = L.SaleInvoiceHeaderId  "
                       + " LEFT JOIN SaleDispatchLines Dl ON L.SaleDispatchLineId = Dl.SaleDispatchLineId  "
                       + " LEFT JOIN PackingLines Pl ON Dl.PackingLineId = Pl.PackingLineId   "
                       + " LEFT JOIN SaleOrderLines Sol ON Pl.SaleOrderLineId = Sol.SaleOrderLineId  "
                       + " LEFT JOIN SaleOrderHeaders Soh ON Sol.SaleOrderHeaderId = Soh.SaleOrderHeaderId  "
                       + " LEFT JOIN Products Prod ON Pl.ProductId = Prod.ProductId  "
                       + " LEFT JOIN FinishedProduct Fp ON Prod.ProductId = Fp.ProductId  "
                       + " LEFT JOIN ProductGroups Pg ON Prod.ProductGroupId = Pg.ProductGroupId  "
                       + " LEFT JOIN Colours PCol ON Fp.ColourId = PCol.ColourId  "
                       + " LEFT JOIN ProductCategories Pc ON Prod.ProductCategoryId = Pc.ProductCategoryId  "
                       + " LEFT JOIN ProductTypes Pt ON Pc.ProductTypeId = Pt.ProductTypeId   "
                       + " LEFT JOIN ProductInvoiceGroups Pig ON Fp.ProductInvoiceGroupId = Pig.ProductInvoiceGroupId  "
                       + " LEFT JOIN (  "
                       + "     SELECT Ps.ProductId, S.SizeName, S.Area SqFeetArea, IsNull(S.Area,0) * 0.092903 AS SqMeterArea  "
                       + "     FROM ProductSizes Ps    "
                       + "     LEFT JOIN Sizes S ON Ps.SizeId = S.SizeId  "
                       + "     LEFT JOIN ProductSizeTypes Pst ON Ps.ProductSizeTypeId = Pst.ProductSizeTypeId  "
                       + "     WHERE Pst.ProductSizeTypeName = 'Standard'  "
                       + " ) AS VProductArea ON Prod.ProductId = VProductArea.ProductId  "
                       + " LEFT JOIN (  "
                       + "     SELECT L.SaleInvoiceHeaderId,   "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName = 'Rug'  THEN Pl.GrossWeight END),0) AS TotalRugGrossWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName <> 'Rug'  THEN Pl.GrossWeight END),0) AS TotalFinishedProductGrossWeight,  "
                       + "     IsNull(Sum(Pl.GrossWeight),0) AS TotalGrossWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName = 'Rug'  THEN Pl.NetWeight END),0) AS TotalRugNetWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName <> 'Rug'  THEN Pl.NetWeight END),0) AS TotalFinishedProductNetWeight,  "
                       + "     IsNull(Sum(Pl.NetWeight),0) AS TotalNetWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName = 'Rug'  THEN Pl.Qty END),0) AS TotalRugQty,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName <> 'Rug'  THEN Pl.Qty END),0) AS TotalFinishedProductQty,  "
                       + "     IsNull(Sum(Pl.Qty),0) AS TotalQty,  "
                       + "     IsNull(Sum(L.Amount),0) AS TotalAmount  "
                       + "     FROM SaleInvoiceLines L   "
                       + "     LEFT JOIN SaleDispatchLines Dl ON L.SaleDispatchLineId = Dl.SaleDispatchLineId  "
                       + "     LEFT JOIN PackingLines Pl ON PL.PackingLineId = Dl.PackingLineId  "
                       + "     LEFT JOIN Products P ON PL.ProductId = P.ProductId  "
                       + "     LEFT JOIN ProductCategories Pc ON P.ProductCategoryId = Pc.ProductCategoryId  "
                       + "     LEFT JOIN ProductTypes Pt ON Pc.ProductTypeId = Pt.ProductTypeId  "
                       + "     GROUP BY L.SaleInvoiceHeaderId  "
                       + " ) AS VSaleInvoice ON L.SaleInvoiceHeaderId = VSaleInvoice.SaleInvoiceHeaderId  "
                       + " LEFT JOIN (  "
                       + "     SELECT H.SaleInvoiceHeaderId,   "
                       + "     IsNull(Sum(VBales.RugRolls),0) AS TotalRugRolls,  "
                       + "     IsNull(Sum(VBales.FinishedRolls),0) AS TotalFinishedProductRolls,  "
                       + "     IsNull(Sum(VBales.TotalRolls),0) AS TotalRolls  "
                       + "     FROM SaleInvoiceHeaders H   "
                       + "     LEFT JOIN (  "
                       + "         SELECT DISTINCT  L.SaleInvoiceHeaderId, Pl.BaleNo,  "
                       + "         CASE WHEN Pt.ProductTypeName = 'Rug' THEN 1 ELSE 0 END  AS RugRolls,  "
                       + "         CASE WHEN Pt.ProductTypeName <> 'Rug' THEN 1 ELSE 0 END  AS FinishedRolls,  "
                       + "         1 AS TotalRolls  "
                       + "         FROM SaleInvoiceLines L   "
                       + "         LEFT JOIN SaleDispatchLines DL ON L.SaleDispatchLineId = Dl.SaleDispatchLineId  "
                       + "         LEFT JOIN PackingLines PL ON Dl.PackingLineId = Pl.PackingLineId  "
                       + "         LEFT JOIN Products P ON Pl.ProductId = P.ProductId  "
                       + "         LEFT JOIN ProductCategories Pc ON P.ProductCategoryId = Pc.ProductCategoryId  "
                       + "         LEFT JOIN ProductTypes Pt ON Pc.ProductTypeId = Pt.ProductTypeId  "
                       + "     ) AS VBales ON H.SaleInvoiceHeaderId = VBales.SaleInvoiceHeaderId  "
                       + "     GROUP BY H.SaleInvoiceHeaderId  "
                       + " ) AS VBaleCount ON H.SaleInvoiceHeaderId = VBaleCount.SaleInvoiceHeaderId  "
                       + " WHERE H.SaleInvoiceHeaderId	= @Id  "
                       + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { }
            #endregion



            // Proc For Sale Reports
            #region ProcSaleOrderReport
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderReport') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderReport  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE  procedure   [" + SchemaName + "].[ProcSaleOrderReport] "
                        + "     @Site VARCHAR(20) = NULL, "
                        + "     @Division VARCHAR(20) = NULL, "
                        + "     @FromDate VARCHAR(20) = NULL, "
                        + "     @ToDate VARCHAR(20) = NULL, "
                        + "     @DocumentType VARCHAR(20) = NULL,	 "
                        + "     @Buyer VARCHAR(Max) = NULL, "
                        + "     @Currency VARCHAR(Max) = NULL, "
                        + "     @ProductNature VARCHAR(Max) = NULL, "
                        + "     @ProductCategory VARCHAR(Max) = NULL, "
                        + "     @ProductType VARCHAR(Max) = NULL, "
                        + "     @ProductCollection VARCHAR(Max) = NULL, "
                        + "     @ProductQuality VARCHAR(Max) = NULL, "
                        + "     @ProductGroup VARCHAR(Max) = NULL, "
                        + "     @ProductStyle VARCHAR(Max) = NULL, "
                        + "     @ProductDesign VARCHAR(Max) = NULL, "
                        + "     @ProductShape VARCHAR(Max) = NULL, "
                        + "     @ProductSize VARCHAR(Max) = NULL, "
                        + "     @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                        + "     @ProductCustomGroup VARCHAR(Max) = NULL, "
                        + "     @ProductTag VARCHAR(Max) = NULL, "
                        + "     @Product VARCHAR(Max) = NULL,   "
                        + "     @SaleOrderHeaderId VARCHAR(Max) = NULL "
                        + " as "
                        + " Begin "
                        + " SELECT H.SaleOrderHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, H.DueDate, H.BuyerOrderNo, H.Remark, "
                        + " P.Name AS SaleToBuyer, BP.Name AS BillToBuyer, "
                        + " H.ShipAddress, C.Name AS Currency, SM.ShipMethodName, DET.DeliveryTermsName,  "
                        + " CASE WHEN H.Priority = 10 THEN 'High' WHEN H.Priority = -10 THEN 'Low' ELSE 'Narmal' END AS Priority, "
                        + "  H.CreditDays, H.TermsAndConditions,    "
                        + " L.SaleOrderLineId, PD.ProductName, L.Qty AS Qty,  "
                        + " L.Qty*isnull(UC.SqYardPerPcs,0) AS DeliveryQty, "
                        + " 'Sq. Yard' AS DeliveryUnit, "
                        + " L.DueDate AS LineDueDate, L.Rate, L.Amount AS Amount, L.Remark AS LineRemark,  "
                        + " U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,  "
                        + " 4 AS DeliveryUnitDecimalPlace, "
                        + " H.CreatedBy, H.CreatedDate, H.ModifiedDate, "
                        + " CASE WHEN isnull(L.Amount,0) <> 0 THEN L.Amount/L.Qty ELSE 0 END AS RatePerPcs "
                        + " FROM  " + SchemaName + ".ViewSaleOrderHeader H "
                        + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderLines L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId  "
                        + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = H.SaleToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = L.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".FinishedProduct  FPD ON FPD.ProductId = PD.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                        + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                        + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = H.ShipMethodId "
                        + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = H.DeliveryTermsId  "
                        + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = L.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                        + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                        + " WHERE 1=1 "
                        + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                        + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                        + " AND ( @FromDate is null or @FromDate <= H.DocDate )  "
                        + " AND ( @ToDate is null or @ToDate >= H.DocDate )  "
                        + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                        + " AND ( @Buyer is null or H.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                        + " AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                        + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                        + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                        + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                        + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                        + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                        + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                        + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                        + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                        + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                        + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                        + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                        + " AND ( @Product is null or L.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                        + " AND ( @SaleOrderHeaderId is null or H.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }



            #endregion

            #region ProcSaleOrderCancelReport
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderCancelReport') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderCancelReport  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE  procedure   [" + SchemaName + "].[ProcSaleOrderCancelReport] "
                            + "     @Site VARCHAR(20) = NULL, "
                            + "     @Division VARCHAR(20) = NULL, "
                            + "     @FromDate VARCHAR(20) = NULL, "
                            + "     @ToDate VARCHAR(20) = NULL, "
                            + "     @FilterDateOn VARCHAR(20) = NULL, "
                            + "     @DocumentType VARCHAR(20) = NULL,	 "
                            + "     @Buyer VARCHAR(Max) = NULL, "
                            + "     @ProductNature VARCHAR(Max) = NULL, "
                            + "     @ProductCategory VARCHAR(Max) = NULL, "
                            + "     @ProductType VARCHAR(Max) = NULL, "
                            + "     @ProductCollection VARCHAR(Max) = NULL, "
                            + "     @ProductQuality VARCHAR(Max) = NULL, "
                            + "     @ProductGroup VARCHAR(Max) = NULL, "
                            + "     @ProductStyle VARCHAR(Max) = NULL, "
                            + "     @ProductDesign VARCHAR(Max) = NULL, "
                            + "     @ProductShape VARCHAR(Max) = NULL, "
                            + "     @ProductSize VARCHAR(Max) = NULL, "
                            + "     @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                            + "     @ProductCustomGroup VARCHAR(Max) = NULL, "
                            + "     @ProductTag VARCHAR(Max) = NULL, "
                            + "     @Product VARCHAR(Max) = NULL,   "
                            + "     @SaleOrderHeaderId VARCHAR(Max) = NULL, "
                            + "     @SaleOrderCancelHeaderId VARCHAR(Max) = NULL "
                            + " as "
                            + " Begin "
                            + " SELECT H.SaleOrderCancelHeaderId, Dt.DocumentTypeName, H.DocDate, H.DocNo, H.Remark, R.ReasonName, "
                            + " L.SaleOrderCancelLineId, SOH.DocNo AS SaleOrderNo, PD.ProductName,  L.Qty, L.Remark AS LineRemark,  "
                            + " B.Name AS BuyerName, U.UnitName, U.DecimalPlaces "
                            + " FROM  " + SchemaName + ".SaleOrderCancelHeaders H "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderCancelLines L ON L.SaleOrderCancelHeaderId  = H.SaleOrderCancelHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON Dt.DocumentTypeId = H.DocTypeId  "
                            + " LEFT JOIN " + SchemaName + ".Reasons R ON R.ReasonId = H.ReasonId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".People B ON B.PersonID = SOH.SaleToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                            + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                            + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                            + " WHERE 1=1 "
                            + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                            + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                            + " AND ( @FromDate is null or @FromDate <= CASE WHEN @FilterDateOn = 'Sale Order Date' THEN SOH.DocDate ELSE H.DocDate END )  "
                            + " AND ( @ToDate is null or @ToDate >= CASE WHEN @FilterDateOn = 'Sale Order Date' THEN SOH.DocDate ELSE H.DocDate END )  "
                            + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                            + " AND ( @Buyer is null or SOH.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                            + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                            + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                            + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                            + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                            + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                            + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                            + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                            + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                            + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                            + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                            + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                            + " AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                            + " AND ( @SaleOrderHeaderId is null or SOL.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                            + " AND ( @SaleOrderCancelHeaderId is null or  H.SaleOrderCancelHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderCancelHeaderId, ',')))  "
                            + " END ";

                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion

            #region ProcSaleOrderAmendmentReport
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderAmendmentReport') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderAmendmentReport  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE  procedure   [" + SchemaName + "].[ProcSaleOrderAmendmentReport] "
                        + "     @Site VARCHAR(20) = NULL, "
                            + "     @Division VARCHAR(20) = NULL, "
                            + "     @FromDate VARCHAR(20) = NULL, "
                            + "     @ToDate VARCHAR(20) = NULL, "
                            + "     @FilterDateOn VARCHAR(20) = NULL, "
                            + "     @DocumentType VARCHAR(20) = NULL,	 "
                            + "     @Buyer VARCHAR(Max) = NULL, "
                            + "     @ProductNature VARCHAR(Max) = NULL, "
                            + "     @ProductCategory VARCHAR(Max) = NULL, "
                            + "     @ProductType VARCHAR(Max) = NULL, "
                            + "     @ProductCollection VARCHAR(Max) = NULL, "
                            + "     @ProductQuality VARCHAR(Max) = NULL, "
                            + "     @ProductGroup VARCHAR(Max) = NULL, "
                            + "     @ProductStyle VARCHAR(Max) = NULL, "
                            + "     @ProductDesign VARCHAR(Max) = NULL, "
                            + "     @ProductShape VARCHAR(Max) = NULL, "
                            + "     @ProductSize VARCHAR(Max) = NULL, "
                            + "     @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                            + "     @ProductCustomGroup VARCHAR(Max) = NULL, "
                            + "     @ProductTag VARCHAR(Max) = NULL, "
                            + "     @Product VARCHAR(Max) = NULL,   "
                            + "     @SaleOrderHeaderId VARCHAR(Max) = NULL, "
                        + "     @SaleOrderAmendmentHeaderId VARCHAR(Max) = NULL "
                            + " as "
                            + " Begin "
                        + " SELECT H.SaleOrderAmendmentHeaderId, Dt.DocumentTypeName, H.DocDate, H.DocNo, H.Remark, R.ReasonName, "
                        + " L.SaleOrderQtyAmendmentLineId, SOH.DocNo AS SaleOrderNo, PD.ProductName,  L.Qty, L.Remark AS LineRemark,  "
                            + " B.Name AS BuyerName, U.UnitName, U.DecimalPlaces "
                        + " FROM  " + SchemaName + ".SaleOrderAmendmentHeaders H "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderQtyAmendmentLines L ON L.SaleOrderAmendmentHeaderId = H.SaleOrderAmendmentHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".DocumentTypes Dt ON Dt.DocumentTypeId = H.DocTypeId  "
                            + " LEFT JOIN " + SchemaName + ".Reasons R ON R.ReasonId = H.ReasonId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".People B ON B.PersonID = SOH.SaleToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = SOL.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                            + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                            + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                            + " WHERE 1=1 "
                            + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                            + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                            + " AND ( @FromDate is null or @FromDate <= CASE WHEN @FilterDateOn = 'Sale Order Date' THEN SOH.DocDate ELSE H.DocDate END )  "
                            + " AND ( @ToDate is null or @ToDate >= CASE WHEN @FilterDateOn = 'Sale Order Date' THEN SOH.DocDate ELSE H.DocDate END )  "
                            + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                            + " AND ( @Buyer is null or SOH.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                            + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                            + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                            + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                        + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                        + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                            + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                        + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                        + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                            + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                            + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                        + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                            + " AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                            + " AND ( @SaleOrderHeaderId is null or SOL.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                        + " AND ( @SaleOrderAmendmentHeaderId is null or  H.SaleOrderAmendmentHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderAmendmentHeaderId, ',')))  "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion

            #region ProcSaleOrderSummary
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderSummary') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderSummary  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE  procedure   [" + SchemaName + "].[ProcSaleOrderSummary] "
                            + "     @GroupTitle VARCHAR(20) = NULL, "
                    + "     @Site VARCHAR(20) = NULL, "
                    + "     @Division VARCHAR(20) = NULL, "
                    + "     @FromDate VARCHAR(20) = NULL, "
                    + "     @ToDate VARCHAR(20) = NULL, "
                    + "     @DocumentType VARCHAR(20) = NULL,	 "
                    + "     @Buyer VARCHAR(Max) = NULL, "
                    + "     @Currency VARCHAR(Max) = NULL, "
                    + "     @ProductNature VARCHAR(Max) = NULL, "
                    + "     @ProductCategory VARCHAR(Max) = NULL, "
                    + "     @ProductType VARCHAR(Max) = NULL, "
                    + "     @ProductCollection VARCHAR(Max) = NULL, "
                    + "     @ProductQuality VARCHAR(Max) = NULL, "
                    + "     @ProductGroup VARCHAR(Max) = NULL, "
                    + "     @ProductStyle VARCHAR(Max) = NULL, "
                    + "     @ProductDesign VARCHAR(Max) = NULL, "
                    + "     @ProductShape VARCHAR(Max) = NULL, "
                    + "     @ProductSize VARCHAR(Max) = NULL, "
                    + "     @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                    + "     @ProductCustomGroup VARCHAR(Max) = NULL, "
                    + "     @ProductTag VARCHAR(Max) = NULL, "
                    + "     @Product VARCHAR(Max) = NULL,   "
                            + "     @SaleOrderHeaderId VARCHAR(Max) = NULL "
                    + " as "
                    + " Begin "
                            + " SELECT @GroupTitle AS GroupOnTitle,  @GroupTitle, "
                            + " CASE WHEN @GroupTitle ='Product' THEN ProductName WHEN  @GroupTitle ='Buyer' THEN BuyerName  WHEN @GroupTitle ='Month' THEN Substring(convert(nvarchar,DocDate,11),0,6) ELSE '' END  AS GroupOnValue, "
                            + " CASE WHEN @GroupTitle ='Product' THEN ProductName WHEN  @GroupTitle ='Buyer' THEN BuyerName  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), DocDate, 106),4,11),' ','/') ELSE '' END  AS GroupOnValueDesc, "
                            + " UnitName, sum(Qty) AS Qty, sum(CancelQty) AS CancelQty, Max(DecimalPlaces) AS DecimalPlaces, "
                            + " DeliveryUnit, sum(DeliveryQty) AS DeliveryQty, sum(CancelDeliveryQty) AS CancelDeliveryQty, max(DeliveryUnitDecimalPlace) AS DeliveryUnitDecimalPlace, "
                            + " Currency, sum(Amount) AS Amount, sum(CancelAmount) AS CancelAmount "
                            + " FROM  "
                            + " ( "
                            + " SELECT PD.ProductName AS ProductName, P.Name AS BuyerName, H.DocDate, "
                            + " L.Qty AS Qty, 0 AS CancelQty, U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,  "
                            + " L.Qty * isnull(UC.SqYardPerPcs,0) AS DeliveryQty, 0 AS CancelDeliveryQty, "
                            + " 'Sq. Yard' AS DeliveryUnit,4 AS DeliveryUnitDecimalPlace, "
                            + " L.Amount AS Amount, 0 AS CancelAmount, C.Name AS Currency "
                            + " FROM  " + SchemaName + ".ViewSaleOrderHeader H "
                    + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderLines L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = H.SaleToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = L.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId  "
                    + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                            + " LEFT JOIN " + SchemaName + ".Units UD ON UD.UnitId = L.DeliveryUnitId  "
                    + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                            + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = H.ShipMethodId "
                            + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = H.DeliveryTermsId  "
                            + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = L.ProductId  "
                    + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                    + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                    + " WHERE 1=1 "
                    + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                    + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                    + " AND ( @FromDate is null or @FromDate <= H.DocDate )  "
                    + " AND ( @ToDate is null or @ToDate >= H.DocDate )  "
                    + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                            + " AND ( @Buyer is null or H.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                            + " AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                    + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                    + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                    + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                            + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                            + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                    + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                            + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                            + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                    + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                    + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                            + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                            + " AND ( @Product is null or L.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                            + " AND ( @SaleOrderHeaderId is null or H.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                            + " UNION ALL  "
                            + " SELECT PD.ProductName AS ProductName, P.Name AS BuyerName, SOH.DocDate, "
                            + " SOCL.Qty AS Qty, 0 AS CancelQty, U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,  "
                            + " UC.SqYardPerPcs*SOCL.Qty AS DeliveryQty, 0 AS CancelDeliveryQty,  "
                            + " 'Sq. Yard'  AS DeliveryUnit,4 AS DeliveryUnitDecimalPlace, "
                            + " 0 AS Amount, SOL.Amount AS CancelAmount, C.Name AS Currency "
                            + " FROM  " + SchemaName + ".SaleOrderAmendmentHeaders SOCH "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderQtyAmendmentLines  SOCL ON SOCL.SaleOrderAmendmentHeaderId  = SOCH.SaleOrderAmendmentHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId  = SOCL.SaleOrderLineId   "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId "
                            + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = SOCH.DocTypeId  "
                            + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = SOH.SaleToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = SOH.BillToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                            + " LEFT JOIN " + SchemaName + ".Units UD ON UD.UnitId = SOL.DeliveryUnitId  "
                            + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = SOH.CurrencyId  "
                            + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = SOH.ShipMethodId "
                            + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = SOH.DeliveryTermsId  "
                            + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                            + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                            + " WHERE 1=1 "
                            + " AND ( @Site is null or SOH.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                            + " AND ( @Division is null or SOH.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                            + " AND ( @FromDate is null or @FromDate <= SOCH.DocDate )  "
                            + " AND ( @ToDate is null or @ToDate >= SOCH.DocDate )  "
                            + " AND ( @DocumentType is null or SOCH.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                            + " AND ( @Buyer is null or SOH.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                            + " AND ( @Currency is null or SOH.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                            + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                            + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                            + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                            + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                            + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                            + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                            + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                            + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                            + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                            + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                            + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                            + " AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                            + " AND ( @SaleOrderHeaderId is null or SOH.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                            + " UNION ALL  "
                            + " SELECT PD.ProductName AS ProductName, P.Name AS BuyerName, SOH.DocDate, "
                            + " SOCL.Qty AS Qty, 0 AS CancelQty, U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,  "
                            + " UC.SqYardPerPcs*SOCL.Qty AS DeliveryQty, 0 AS CancelDeliveryQty,  "
                            + " 'Sq. Yard'  AS DeliveryUnit,4 AS DeliveryUnitDecimalPlace, "
                            + " 0 AS Amount, SOL.Amount AS CancelAmount, C.Name AS Currency "
                            + " FROM  " + SchemaName + ".SaleOrderAmendmentHeaders SOCH "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderRateAmendmentLines  SOCL ON SOCL.SaleOrderAmendmentHeaderId  = SOCH.SaleOrderAmendmentHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId  = SOCL.SaleOrderLineId   "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId "
                            + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = SOCH.DocTypeId  "
                            + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = SOH.SaleToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = SOH.BillToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = SOL.ProductId "
                            + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId   "
                            + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                            + " LEFT JOIN " + SchemaName + ".Units UD ON UD.UnitId = SOL.DeliveryUnitId  "
                            + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = SOH.CurrencyId  "
                            + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = SOH.ShipMethodId "
                            + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = SOH.DeliveryTermsId  "
                            + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                            + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                            + " WHERE 1=1 "
                            + " AND ( @Site is null or SOH.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                            + " AND ( @Division is null or SOH.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                            + " AND ( @FromDate is null or @FromDate <= SOCH.DocDate )  "
                            + " AND ( @ToDate is null or @ToDate >= SOCH.DocDate )  "
                            + " AND ( @DocumentType is null or SOCH.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                            + " AND ( @Buyer is null or SOH.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                            + " AND ( @Currency is null or SOH.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                            + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                            + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                            + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                            + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                            + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                            + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                            + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                            + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                            + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                            + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                            + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                            + " AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                            + " AND ( @SaleOrderHeaderId is null or SOH.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                            + " UNION ALL  "
                            + " SELECT PD.ProductName AS ProductName, P.Name AS BuyerName, SOH.DocDate, "
                            + " 0 AS Qty, SOCL.Qty AS CancelQty, U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,  "
                            + " 0 AS DeliveryQty, UC.SqYardPerPcs*SOCL.Qty AS CancelDeliveryQty,  "
                            + " 'Sq. Yard'  AS DeliveryUnit,4 AS DeliveryUnitDecimalPlace, "
                            + " 0 AS Amount, SOL.Amount AS CancelAmount, C.Name AS Currency "
                            + " FROM  " + SchemaName + ".SaleOrderCancelHeaders SOCH "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderCancelLines SOCL ON SOCL.SaleOrderCancelHeaderId = SOCH.SaleOrderCancelHeaderId  "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId  = SOCL.SaleOrderLineId   "
                            + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId "
                            + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = SOCH.DocTypeId  "
                            + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = SOH.SaleToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = SOH.BillToBuyerId  "
                            + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId   "
                            + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                            + " LEFT JOIN " + SchemaName + ".Units UD ON UD.UnitId = SOL.DeliveryUnitId  "
                            + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = SOH.CurrencyId  "
                            + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = SOH.ShipMethodId "
                            + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = SOH.DeliveryTermsId  "
                            + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = SOL.ProductId  "
                            + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                            + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                            + " WHERE 1=1 "
                            + " AND ( @Site is null or SOH.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                            + " AND ( @Division is null or SOH.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                            + " AND ( @FromDate is null or @FromDate <= SOCH.DocDate )  "
                            + " AND ( @ToDate is null or @ToDate >= SOCH.DocDate )  "
                            + " AND ( @DocumentType is null or SOCH.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                            + " AND ( @Buyer is null or SOH.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                            + " AND ( @Currency is null or SOH.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                            + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                            + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                            + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                            + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                            + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                            + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                            + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                            + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                            + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                            + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                            + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                            + " AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                            + " AND ( @SaleOrderHeaderId is null or SOH.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                            + " ) H "
                            + " GROUP BY  UnitName, DeliveryUnit, Currency, "
                            + " CASE WHEN @GroupTitle ='Product' THEN ProductName WHEN  @GroupTitle ='Buyer' THEN BuyerName WHEN @GroupTitle ='Month' THEN Substring(convert(nvarchar,DocDate,11),0,6) ELSE '' END , "
                            + " CASE WHEN @GroupTitle ='Product' THEN ProductName WHEN  @GroupTitle ='Buyer' THEN BuyerName  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), DocDate, 106),4,11),' ','/') ELSE '' END  "
                            + " ORDER BY CASE WHEN @GroupTitle ='Product' THEN ProductName WHEN  @GroupTitle ='Buyer' THEN BuyerName  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), DocDate, 106),4,11),' ','/') ELSE '' END  "
                            + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            #endregion

            #region ProcSaleOrderStatusReport
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderStatusReport') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderStatusReport  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE procedure   [" + SchemaName + "].[ProcSaleOrderStatusReport] "
                    + "     @Site VARCHAR(20) = NULL, "
                    + "     @Division VARCHAR(20) = NULL, "
                    + "     @FromDate VARCHAR(20) = NULL, "
                    + "     @ToDate VARCHAR(20) = NULL, "
                    + "     @StatusOnDate VARCHAR(20) = NULL, "
                        + "     @DocumentType VARCHAR(20) = NULL,	 "
                    + "     @Buyer VARCHAR(Max) = NULL, "
                    + "     @Currency VARCHAR(Max) = NULL, "
                    + "     @ProductNature VARCHAR(Max) = NULL, "
                    + "     @ProductCategory VARCHAR(Max) = NULL, "
                    + "     @ProductType VARCHAR(Max) = NULL, "
                    + "     @ProductCollection VARCHAR(Max) = NULL, "
                    + "     @ProductQuality VARCHAR(Max) = NULL, "
                    + "     @ProductGroup VARCHAR(Max) = NULL, "
                    + "     @ProductStyle VARCHAR(Max) = NULL, "
                    + "     @ProductDesign VARCHAR(Max) = NULL, "
                    + "     @ProductShape VARCHAR(Max) = NULL, "
                    + "     @ProductSize VARCHAR(Max) = NULL, "
                    + "     @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                    + "     @ProductCustomGroup VARCHAR(Max) = NULL, "
                    + "     @ProductTag VARCHAR(Max) = NULL, "
                    + "     @Product VARCHAR(Max) = NULL,   "
                    + "     @SaleOrderHeaderId VARCHAR(Max) = NULL, "
                    + "     @ReportFor VARCHAR(Max) = NULL "
                    + " as "
                    + " Begin "
                    + " SELECT H.SaleOrderHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, H.DueDate, H.BuyerOrderNo, H.Remark, "
                    + " P.Name AS SaleToBuyer, BP.Name AS BillToBuyer,H.ShipAddress, C.Name AS Currency, SM.ShipMethodName,  "
                    + " DET.DeliveryTermsName,  H.CreditDays, H.TermsAndConditions,    "
                    + " L.SaleOrderLineId, PD.ProductName, L.OrderQty AS OrderQty, L.CancelQty, Vinv.InvoiceQty AS InvoiceQty, L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0) AS BalQty, "
                    + " L.OrderQty*isnull(UC.SqYardPerPcs,0) AS OrderDeliveryQty, L.CancelQty*isnull(UC.SqYardPerPcs,0) AS CancelDeliveryQty, Vinv.InvoiceQty*isnull(UC.SqYardPerPcs,0) AS InvoiceDeliveryQty, (L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0))*isnull(UC.SqYardPerPcs,0) AS BalDeliveryQty, 'Sq. Yard' AS DeliveryUnit, L.DueDate AS LineDueDate,  "
                    + " L.Rate, L.OrderQty*L.Rate  AS OrderAmount, L.CancelQty*L.Rate AS CancelAmount, Vinv.InvoiceQty*L.Rate AS InvoiceAmount,(L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0))*L.Rate AS BalAmount, "
                    + " L.Remark AS LineRemark,  "
                    + " U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 4 AS DeliveryUnitDecimalPlace, H.CreatedBy, H.CreatedDate, H.ModifiedDate, 0 AS RatePerPcs "
                    + " FROM  " + SchemaName + ".ViewSaleOrderHeader H "
                    + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                    + " LEFT JOIN " + SchemaName + ".ViewSaleOrderLine L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId  "
                    + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = H.SaleToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId  "
                    + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = L.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON FPD.ProductId = PD.ProductId  "
                    + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                    + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                    + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = H.ShipMethodId "
                    + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = H.DeliveryTermsId  "
                    + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = L.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                    + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                    + " LEFT JOIN "
                    + " ( "
                    + " SELECT PL.SaleOrderLineId, Sum(PL.Qty) AS InvoiceQty  "
                    + " FROM  " + SchemaName + ".ViewSaleInvoiceLineX PL "
                    + " WHERE 1=1  "
                    + " AND ( @StatusOnDate is null or @StatusOnDate >= PL.DocDate )  "
                    + " GROUP BY PL.SaleOrderLineId "
                    + " ) VInv ON VInv.SaleOrderLineId = L.SaleOrderLineId "
                        + " WHERE 1=1 "
                    + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                    + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                    + " AND ( @FromDate is null or @FromDate <= H.DocDate )  "
                    + " AND ( @ToDate is null or @ToDate >= H.DocDate )  "
                    + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                    + " AND ( @Buyer is null or H.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                        + " AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                    + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                    + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                    + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                        + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                        + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                    + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                        + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                        + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                    + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                    + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                        + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                    + " AND ( @Product is null or L.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                    + " AND ( @SaleOrderHeaderId is null or H.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                        + " AND ( @ReportFor is null or (L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0)) > 0 )  "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            #endregion

            #region ProcSaleOrderBalanceReport
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderBalanceReport') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderBalanceReport  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE  procedure   [" + SchemaName + "].[ProcSaleOrderBalanceReport] "
                    + "     @Site VARCHAR(20) = NULL, "
                    + "     @Division VARCHAR(20) = NULL, "
                    + "     @FromDate VARCHAR(20) = NULL, "
                    + "     @ToDate VARCHAR(20) = NULL, "
                    + "     @StatusOnDate VARCHAR(20) = NULL, "
                    + "     @DocumentType VARCHAR(20) = NULL,	 "
                    + "     @Buyer VARCHAR(Max) = NULL, "
                    + "     @Currency VARCHAR(Max) = NULL, "
                    + "     @ProductNature VARCHAR(Max) = NULL, "
                    + "     @ProductCategory VARCHAR(Max) = NULL, "
                    + "     @ProductType VARCHAR(Max) = NULL, "
                    + "     @ProductCollection VARCHAR(Max) = NULL, "
                    + "     @ProductQuality VARCHAR(Max) = NULL, "
                    + "     @ProductGroup VARCHAR(Max) = NULL, "
                    + "     @ProductStyle VARCHAR(Max) = NULL, "
                    + "     @ProductDesign VARCHAR(Max) = NULL, "
                    + "     @ProductShape VARCHAR(Max) = NULL, "
                    + "     @ProductSize VARCHAR(Max) = NULL, "
                    + "     @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                    + "     @ProductCustomGroup VARCHAR(Max) = NULL, "
                    + "     @ProductTag VARCHAR(Max) = NULL, "
                    + "     @Product VARCHAR(Max) = NULL,   "
                    + "     @SaleOrderHeaderId VARCHAR(Max) = NULL "
                    + " as "
                    + " Begin "
                    + " SELECT H.SaleOrderHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, H.DueDate, H.BuyerOrderNo, H.Remark, "
                    + " P.Name AS SaleToBuyer, BP.Name AS BillToBuyer,H.ShipAddress, C.Name AS Currency, SM.ShipMethodName,  "
                    + " DET.DeliveryTermsName,  H.CreditDays, H.TermsAndConditions,    "
                    + " L.SaleOrderLineId, PD.ProductName, L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0) AS BalQty, "
                    + " (L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0))*isnull(UC.SqYardPerPcs,0) AS BalDeliveryQty, 'Sq. Yard' AS DeliveryUnit, L.DueDate AS LineDueDate,  "
                    + " L.Rate,(L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0))*L.Rate AS BalAmount, "
                    + " L.Remark AS LineRemark,  "
                    + " U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 4 AS DeliveryUnitDecimalPlace, H.CreatedBy, H.CreatedDate, H.ModifiedDate, 0 AS RatePerPcs "
                    + " FROM  " + SchemaName + ".ViewSaleOrderHeader H "
                    + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                    + " LEFT JOIN " + SchemaName + ".ViewSaleOrderLine L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId  "
                    + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = H.SaleToBuyerId  "
                    + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId  "
                    + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = L.ProductId  "
                    + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON FPD.ProductId = PD.ProductId  "
                    + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                    + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                    + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = H.ShipMethodId "
                    + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = H.DeliveryTermsId  "
                    + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = L.ProductId  "
                    + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                    + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                    + " LEFT JOIN "
                    + " ( "
                    + " SELECT PL.SaleOrderLineId, Sum(PL.Qty) AS InvoiceQty  "
                    + " FROM  " + SchemaName + ".ViewSaleInvoiceLineX PL "
                    + " WHERE 1=1  "
                    + " AND ( @StatusOnDate is null or @StatusOnDate >= PL.DocDate )  "
                    + " GROUP BY PL.SaleOrderLineId "
                    + " ) VInv ON VInv.SaleOrderLineId = L.SaleOrderLineId "
                    + " WHERE 1=1 "
                    + " AND (L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0)) > 0  "
                    + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                    + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                    + " AND ( @FromDate is null or @FromDate <= H.DocDate )  "
                    + " AND ( @ToDate is null or @ToDate >= H.DocDate )  "
                    + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                    + " AND ( @Buyer is null or H.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                    + " AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                    + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                    + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                    + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                    + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                    + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                    + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                    + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                    + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                    + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                    + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                    + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                    + " AND ( @Product is null or L.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                    + " AND ( @SaleOrderHeaderId is null or H.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                    + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            #endregion

            #region ProcSaleInvoiceReport
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleInvoiceReport') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleInvoiceReport  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE  procedure  [" + SchemaName + "].[ProcSaleInvoiceReport] "
                        + " @Site VARCHAR(20) = NULL,  "
                        + " @Division VARCHAR(20) = NULL,       "
                        + " @FromDate VARCHAR(20) = NULL,       "
                        + " @ToDate VARCHAR(20) = NULL,       "
                        + " @DocumentType VARCHAR(20) = NULL,	  "
                        + " @Buyer VARCHAR(Max) = NULL,       "
                        + " @Currency VARCHAR(Max) = NULL,      "
                        + " @ProductNature VARCHAR(Max) = NULL,   "
                        + " @ProductCategory VARCHAR(Max) = NULL,   "
                        + " @ProductType VARCHAR(Max) = NULL,       "
                        + " @ProductCollection VARCHAR(Max) = NULL,   "
                        + " @ProductQuality VARCHAR(Max) = NULL,       "
                        + " @ProductGroup VARCHAR(Max) = NULL,       "
                        + " @ProductStyle VARCHAR(Max) = NULL,       "
                        + " @ProductDesign VARCHAR(Max) = NULL,       "
                        + " @ProductShape VARCHAR(Max) = NULL,       "
                        + " @ProductSize VARCHAR(Max) = NULL,       "
                        + " @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                        + " @ProductCustomGroup VARCHAR(Max) = NULL,    "
                        + " @ProductTag VARCHAR(Max) = NULL,       "
                        + " @Product VARCHAR(Max) = NULL,         "
                        + " @SaleOrderHeaderId VARCHAR(Max) = NULL, "
                        + " @SaleInvoiceHeaderId VARCHAR(Max) = NULL  "
                        + " as   "
                        + " Begin  "
                        + " SELECT H.SaleInvoiceHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, H.Remark,   H.LrNo,	H.LrDate,	H.PrivateMark,	H.PortOfLoading,	H.DestinationPort,	H.FinalPlaceOfDelivery,  H.PreCarriageBy,	H.PlaceOfPreCarriage,	H.CircularNo,	H.CircularDate,	H.OrderNo,	H.OrderDate,	H.RollNo,  H.DescriptionOfGoods,	H.KindsOfackages,	H.Compositions,	H.OtherRefrence,	 "
                        + " H.TermsOfSale,	H.NotifyParty,  H.TransporterInformation,	BP.Name AS BillToBuyer, SOH.DocNo AS SaleOrderNo, C.Name AS Currency,  L.SaleInvoiceLineId, PD.ProductName,   PL.Qty AS Qty, PL.Qty*isnull(UC.SqYardPerPcs,0) AS DeliveryQty, 'Sq. Yard' AS DeliveryUnit,  L.Rate, L.Amount AS Amount, L.Remark AS LineRemark,   U.UnitName,  "
                        + " isnull(U.DecimalPlaces,0) AS DecimalPlaces,   4 AS DeliveryUnitDecimalPlace, H.CreatedBy, H.CreatedDate, H.ModifiedDate,  "
                        + " CASE WHEN isnull(L.Amount,0) <> 0 THEN L.Amount/PL.Qty ELSE 0 END AS RatePerPcs   "
                        + " FROM  " + SchemaName + ".ViewSaleInvoiceHeader H   "
                        + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId    "
                        + " LEFT JOIN " + SchemaName + ".SaleInvoiceLines L ON L.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId    "
                        + " LEFT JOIN " + SchemaName + ".SaleDispatchLines SDL ON SDL.SaleDispatchLineId = L.SaleDispatchLineId   "
                        + " LEFT JOIN " + SchemaName + ".PackingLines PL ON PL.PackingLineId  = SDL.PackingLineId    "
                        + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId     "
                        + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = PL.ProductId    "
                        + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId   "
                        + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId    "
                        + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                        + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = PL.ProductId    "
                        + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId    "
                        + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId    "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId = PL.SaleOrderLineId    "
                        + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId    "
                        + " WHERE 1=1   "
                        + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))    "
                        + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))    "
                        + " AND ( @FromDate is null or @FromDate <= H.DocDate )   AND ( @ToDate is null or @ToDate >= H.DocDate )    "
                        + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))    "
                        + " AND ( @Buyer is null or H.BillToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))    "
                        + " AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ',')))   "
                        + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))    "
                        + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))    "
                        + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))    "
                        + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))    "
                        + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))    "
                        + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))    "
                        + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))    "
                        + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ',')))   "
                        + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))    "
                        + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))    "
                        + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))    "
                        + " AND ( @Product is null or PL.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))    "
                        + " AND ( @SaleOrderHeaderId is null or SOL.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))    "
                        + " AND ( @SaleInvoiceHeaderId is null or H.SaleInvoiceHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleInvoiceHeaderId, ',')))  "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }

            #endregion

            #region ProcSaleInvoiceSummary
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleInvoiceSummary') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleInvoiceSummary  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE  procedure  [" + SchemaName + "].[ProcSaleInvoiceSummary] "
                    + " @GroupTitle VARCHAR(20) = NULL,   "
                    + " @Site VARCHAR(20) = NULL,       "
                    + " @Division VARCHAR(20) = NULL,     "
                    + " @FromDate VARCHAR(20) = NULL,       "
                    + " @ToDate VARCHAR(20) = NULL,       "
                    + " @DocumentType VARCHAR(20) = NULL,	  "
                    + " @Buyer VARCHAR(Max) = NULL,       "
                    + " @Currency VARCHAR(Max) = NULL,      "
                    + " @ProductNature VARCHAR(Max) = NULL,   "
                    + " @ProductCategory VARCHAR(Max) = NULL,   "
                    + " @ProductType VARCHAR(Max) = NULL,       "
                    + " @ProductCollection VARCHAR(Max) = NULL,   "
                    + " @ProductQuality VARCHAR(Max) = NULL,       "
                    + " @ProductGroup VARCHAR(Max) = NULL,       "
                    + " @ProductStyle VARCHAR(Max) = NULL,       "
                    + " @ProductDesign VARCHAR(Max) = NULL,       "
                    + " @ProductShape VARCHAR(Max) = NULL,       "
                    + " @ProductSize VARCHAR(Max) = NULL,       "
                    + " @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                    + " @ProductCustomGroup VARCHAR(Max) = NULL,    "
                    + " @ProductTag VARCHAR(Max) = NULL,       "
                    + " @Product VARCHAR(Max) = NULL,         "
                    + " @SaleOrderHeaderId VARCHAR(Max) = NULL, "
                    + " @SaleInvoiceHeaderId VARCHAR(Max) = NULL  "
                    + " as  Begin   "
                    + " SELECT U.UnitName, isnull(max(U.DecimalPlaces),0) AS DecimalPlaces, 'Sq. Yard' AS DeliveryUnit, 4 AS DeliveryUnitDecimalPlace, C.Name AS Currency, @GroupTitle AS GroupTitle, "
                    + " CASE WHEN @GroupTitle ='Product' THEN PD.ProductName WHEN @GroupTitle ='Product_Group' THEN PG.ProductGroupName WHEN @GroupTitle ='Product_Category' THEN PC.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN PT.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN BP.Name  WHEN @GroupTitle ='Month' THEN Substring(convert(nvarchar,H.DocDate,11),0,6) ELSE '' END  AS GroupOnValue, "
                    + " CASE WHEN @GroupTitle ='Product' THEN PD.ProductName WHEN @GroupTitle ='Product_Group' THEN PG.ProductGroupName WHEN @GroupTitle ='Product_Category' THEN PC.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN PT.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN BP.Name  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), H.DocDate, 106),4,11),' ','/') ELSE '' END  AS GroupOnValueDesc, "
                    + " sum(PL.Qty) AS Qty, sum(PL.Qty * isnull(UC.SqYardPerPcs,0)) AS DeliveryQty, sum(L.Amount) AS Amount "
                    + " FROM  " + SchemaName + ".ViewSaleInvoiceHeader H   "
                    + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId    "
                    + " LEFT JOIN " + SchemaName + ".SaleInvoiceLines L ON L.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId    "
                    + " LEFT JOIN " + SchemaName + ".SaleDispatchLines SDL ON SDL.SaleDispatchLineId = L.SaleDispatchLineId   "
                    + " LEFT JOIN " + SchemaName + ".PackingLines PL ON PL.PackingLineId  = SDL.PackingLineId    "
                    + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId     "
                    + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = PL.ProductId    "
                    + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON PD.ProductId = FPD.ProductId   "
                    + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId    "
                    + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                    + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = PL.ProductId    "
                    + " LEFT JOIN " + SchemaName + ".ProductGroups PG ON PG.ProductGroupId = PD.ProductGroupId  "
                    + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON  PC.ProductCategoryId = PD.ProductCategoryId    "
                    + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId    "
                    + " LEFT JOIN " + SchemaName + ".SaleOrderLines SOL ON SOL.SaleOrderLineId = PL.SaleOrderLineId    "
                    + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId    "
                    + " WHERE 1=1   "
                    + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))    "
                    + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))    "
                    + " AND ( @FromDate is null or @FromDate <= H.DocDate )   AND ( @ToDate is null or @ToDate >= H.DocDate )    "
                    + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))    "
                    + " AND ( @Buyer is null or H.BillToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))    "
                    + " AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ',')))   "
                    + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))    "
                    + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))    "
                    + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))    "
                    + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))    "
                    + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))    "
                    + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))    "
                    + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))    "
                    + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ',')))   "
                    + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))    "
                    + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))    "
                    + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))    "
                    + " AND ( @Product is null or PL.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))    "
                    + " AND ( @SaleOrderHeaderId is null or SOL.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))    "
                    + " AND ( @SaleInvoiceHeaderId is null or H.SaleInvoiceHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleInvoiceHeaderId, ',')))  "
                    + " GROUP BY  U.UnitName, C.Name , "
                    + " CASE WHEN @GroupTitle ='Product' THEN PD.ProductName WHEN @GroupTitle ='Product_Group' THEN PG.ProductGroupName  WHEN @GroupTitle ='Product_Category' THEN PC.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN PT.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN BP.Name WHEN @GroupTitle ='Month' THEN Substring(convert(nvarchar,H.DocDate,11),0,6) ELSE '' END , "
                    + " CASE WHEN @GroupTitle ='Product' THEN PD.ProductName WHEN @GroupTitle ='Product_Group' THEN PG.ProductGroupName  WHEN @GroupTitle ='Product_Category' THEN PC.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN PT.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN BP.Name  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), H.DocDate, 106),4,11),' ','/') ELSE '' END  "
                    + " ORDER BY CASE WHEN @GroupTitle ='Product' THEN PD.ProductName WHEN @GroupTitle ='Product_Group' THEN PG.ProductGroupName WHEN @GroupTitle ='Product_Category' THEN PC.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN PT.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN BP.Name  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), H.DocDate, 106),4,11),' ','/') ELSE '' END  "
                    + " END ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion

            #region ProcSaleOrderStatusSummary
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleOrderStatusSummary') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleOrderStatusSummary  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE procedure   [" + SchemaName + "].[ProcSaleOrderStatusSummary] "
                        + "     @GroupTitle VARCHAR(20) = NULL,  "
                    + "     @Site VARCHAR(20) = NULL, "
                    + "     @Division VARCHAR(20) = NULL, "
                    + "     @FromDate VARCHAR(20) = NULL, "
                    + "     @ToDate VARCHAR(20) = NULL, "
                        + "     @StatusOnDate VARCHAR(20) = NULL, "
                    + "     @DocumentType VARCHAR(20) = NULL,	 "
                    + "     @Buyer VARCHAR(Max) = NULL, "
                    + "     @Currency VARCHAR(Max) = NULL, "
                    + "     @ProductNature VARCHAR(Max) = NULL, "
                    + "     @ProductCategory VARCHAR(Max) = NULL, "
                    + "     @ProductType VARCHAR(Max) = NULL, "
                    + "     @ProductCollection VARCHAR(Max) = NULL, "
                    + "     @ProductQuality VARCHAR(Max) = NULL, "
                    + "     @ProductGroup VARCHAR(Max) = NULL, "
                    + "     @ProductStyle VARCHAR(Max) = NULL, "
                    + "     @ProductDesign VARCHAR(Max) = NULL, "
                    + "     @ProductShape VARCHAR(Max) = NULL, "
                    + "     @ProductSize VARCHAR(Max) = NULL, "
                    + "     @ProductInvoiceGroup VARCHAR(Max) = NULL, "
                    + "     @ProductCustomGroup VARCHAR(Max) = NULL, "
                    + "     @ProductTag VARCHAR(Max) = NULL, "
                    + "     @Product VARCHAR(Max) = NULL,   "
                        + "     @SaleOrderHeaderId VARCHAR(Max) = NULL, "
                        + "     @ReportFor VARCHAR(Max) = NULL "
                    + " as "
                    + " Begin "
                        + " SELECT VMain.UnitName, isnull(max(VMain.DecimalPlaces),0) AS DecimalPlaces, 'Sq. Yard' AS DeliveryUnit, 4 AS DeliveryUnitDecimalPlace, VMain.Currency AS Currency, @GroupTitle AS GroupTitle, "
                        + " CASE WHEN @GroupTitle ='Product' THEN VMain.ProductName WHEN @GroupTitle ='Product_Group' THEN VMain.ProductGroupName WHEN @GroupTitle ='Product_Category' THEN VMain.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN VMain.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN VMain.SaleToBuyer  WHEN @GroupTitle ='Month' THEN Substring(convert(nvarchar,VMain.DocDate,11),0,6) ELSE '' END  AS GroupOnValue, "
                        + " CASE WHEN @GroupTitle ='Product' THEN VMain.ProductName WHEN @GroupTitle ='Product_Group' THEN VMain.ProductGroupName WHEN @GroupTitle ='Product_Category' THEN VMain.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN VMain.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN VMain.SaleToBuyer  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), VMain.DocDate, 106),4,11),' ','/') ELSE '' END  AS GroupOnValueDesc, "
                        + " sum(VMain.OrderQty) AS OrderQty, sum(VMain.OrderDeliveryQty) AS OrderDeliveryQty, sum(VMain.OrderAmount) AS OrderAmount, "
                        + " sum(VMain.CancelQty) AS CancelQty, sum(VMain.CancelDeliveryQty) AS CancelDeliveryQty, sum(VMain.CancelAmount) AS CancelAmount, "
                        + " sum(VMain.InvoiceQty) AS InvoiceQty, sum(VMain.InvoiceDeliveryQty) AS InvoiceDeliveryQty, sum(VMain.InvoiceAmount) AS InvoiceAmount, "
                        + " sum(VMain.BalQty) AS BalanceQty, sum(VMain.BalDeliveryQty) AS BalanceDeliveryQty, sum(VMain.BalAmount) AS BalanceAmount "
                        + " FROM  "
                    + " ( "
                        + " SELECT H.SaleOrderHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, H.DueDate, H.BuyerOrderNo, H.Remark, "
                        + " P.Name AS SaleToBuyer, BP.Name AS BillToBuyer,H.ShipAddress, C.Name AS Currency, SM.ShipMethodName,  "
                        + " DET.DeliveryTermsName,  H.CreditDays, H.TermsAndConditions,   PG.ProductGroupName ,  PC.ProductCategoryName, PT.ProductTypeName, "
                        + " L.SaleOrderLineId, PD.ProductName, L.OrderQty AS OrderQty, L.CancelQty, Vinv.InvoiceQty AS InvoiceQty, L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0) AS BalQty, "
                        + " L.OrderQty*isnull(UC.SqYardPerPcs,0) AS OrderDeliveryQty, L.CancelQty*isnull(UC.SqYardPerPcs,0) AS CancelDeliveryQty, Vinv.InvoiceQty*isnull(UC.SqYardPerPcs,0) AS InvoiceDeliveryQty, (L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0))*isnull(UC.SqYardPerPcs,0) AS BalDeliveryQty, 'Sq. Yard' AS DeliveryUnit, L.DueDate AS LineDueDate,  "
                        + " L.Rate, L.OrderQty*L.Rate  AS OrderAmount, L.CancelQty*L.Rate AS CancelAmount, Vinv.InvoiceQty*L.Rate AS InvoiceAmount,(L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0))*L.Rate AS BalAmount, "
                        + " L.Remark AS LineRemark,  "
                        + " U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 4 AS DeliveryUnitDecimalPlace, H.CreatedBy, H.CreatedDate, H.ModifiedDate, 0 AS RatePerPcs "
                    + " FROM  " + SchemaName + ".ViewSaleOrderHeader H "
                    + " LEFT JOIN " + SchemaName + ".DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId  "
                        + " LEFT JOIN " + SchemaName + ".ViewSaleOrderLine L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId  "
                    + " LEFT JOIN " + SchemaName + ".People P ON P.PersonID = H.SaleToBuyerId  "
                        + " LEFT JOIN " + SchemaName + ".People BP ON BP.PersonID = H.BillToBuyerId  "
                    + " LEFT JOIN " + SchemaName + ".Products PD ON PD.ProductId = L.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".FinishedProduct FPD ON FPD.ProductId = PD.ProductId  "
                    + " LEFT JOIN " + SchemaName + ".Units U ON U.UnitId = PD.UnitId  "
                    + " LEFT JOIN " + SchemaName + ".Currencies C ON C.ID = H.CurrencyId  "
                    + " LEFT JOIN " + SchemaName + ".ShipMethods SM ON SM.ShipMethodId  = H.ShipMethodId "
                    + " LEFT JOIN " + SchemaName + ".DeliveryTerms DET ON DET.DeliveryTermsId = H.DeliveryTermsId  "
                    + " LEFT JOIN " + SchemaName + ".ViewRugArea UC ON UC.ProductId = L.ProductId  "
                        + " LEFT JOIN " + SchemaName + ".ProductGroups PG ON PG.ProductGroupId = PD.ProductGroupId  "
                        + " LEFT JOIN " + SchemaName + ".ProductCategories PC ON PC.ProductCategoryId = PD.ProductCategoryId  "
                    + " LEFT JOIN " + SchemaName + ".ProductTypes PT ON PT.ProductTypeId = PC.ProductTypeId  "
                        + " LEFT JOIN "
                        + " ( "
                        + " SELECT PL.SaleOrderLineId, Sum(PL.Qty) AS InvoiceQty  "
                        + " FROM  " + SchemaName + ".ViewSaleInvoiceLineX PL "
                        + " WHERE 1=1  "
                        + " AND ( @StatusOnDate is null or @StatusOnDate >= PL.DocDate )  "
                        + " GROUP BY PL.SaleOrderLineId "
                        + " ) VInv ON VInv.SaleOrderLineId = L.SaleOrderLineId "
                        + " WHERE 1=1 "
                    + " AND ( @Site is null or H.SiteId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Site, ',')))  "
                    + " AND ( @Division is null or H.DivisionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Division, ',')))  "
                    + " AND ( @FromDate is null or @FromDate <= H.DocDate )  "
                    + " AND ( @ToDate is null or @ToDate >= H.DocDate )  "
                    + " AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@DocumentType, ',')))  "
                    + " AND ( @Buyer is null or H.SaleToBuyerId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Buyer, ',')))  "
                        + " AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Currency, ','))) "
                    + " AND ( @ProductNature is null or PT.ProductNatureId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductNature, ',')))  "
                    + " AND ( @ProductCategory is null or PC.ProductCategoryId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCategory, ',')))  "
                    + " AND ( @ProductType is null OR PC.ProductTypeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductType, ',')))  "
                        + " AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductCollection, ',')))  "
                        + " AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductQuality, ',')))  "
                    + " AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductGroup, ',')))  "
                        + " AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductStyle, ',')))  "
                        + " AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductDesign, ','))) "
                    + " AND ( @ProductShape is null or UC.ProductShapeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductShape, ',')))  "
                    + " AND ( @ProductSize is null or UC.SizeId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductSize, ',')))  "
                        + " AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@ProductInvoiceGroup, ',')))  "
                    + " AND ( @Product is null or L.ProductId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@Product, ',')))  "
                    + " AND ( @SaleOrderHeaderId is null or H.SaleOrderHeaderId IN (SELECT Items FROM  " + SchemaName + ".[Split] (@SaleOrderHeaderId, ',')))  "
                        + " AND ( @ReportFor is null or (L.OrderQty - isnull(L.CancelQty,0) - isnull(Vinv.InvoiceQty,0)) > 0 )  "
                        + " )  VMain "
                        + " GROUP BY  VMain.UnitName, VMain.Currency , "
                        + " CASE WHEN @GroupTitle ='Product' THEN VMain.ProductName WHEN @GroupTitle ='Product_Group' THEN VMain.ProductGroupName  WHEN @GroupTitle ='Product_Category' THEN VMain.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN VMain.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN VMain.SaleToBuyer WHEN @GroupTitle ='Month' THEN Substring(convert(nvarchar,VMain.DocDate,11),0,6) ELSE '' END , "
                        + " CASE WHEN @GroupTitle ='Product' THEN VMain.ProductName WHEN @GroupTitle ='Product_Group' THEN VMain.ProductGroupName  WHEN @GroupTitle ='Product_Category' THEN VMain.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN VMain.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN VMain.SaleToBuyer  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), VMain.DocDate, 106),4,11),' ','/') ELSE '' END  "
                        + " ORDER BY CASE WHEN @GroupTitle ='Product' THEN VMain.ProductName WHEN @GroupTitle ='Product_Group' THEN VMain.ProductGroupName WHEN @GroupTitle ='Product_Category' THEN VMain.ProductCategoryName WHEN @GroupTitle ='Product_Type' THEN VMain.ProductTypeName WHEN  @GroupTitle ='Buyer' THEN VMain.SaleToBuyer  WHEN @GroupTitle ='Month' THEN replace(substring(CONVERT(VARCHAR(11), VMain.DocDate, 106),4,11),' ','/') ELSE '' END  "
                        + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion



            // Proc For Packing Reports
            #region ProcSaleOrderReport
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcSaleInvoicePrint') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcSaleInvoicePrint  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " CREATE PROCEDURE [" + SchemaName + "].[ProcSaleInvoicePrint](@Id INT) "
                       + " AS  "
                       + " BEGIN "
                       + " SELECT H.SaleInvoiceHeaderId, H.DocDate, H.DocNo, P.Name AS BillToBuyerName, VAddress.BillToBuyerAddress, "
                       + " VAddress.BIllToPartyCity, VAddress.BillToPartyCountry, "
                       + " C.Name AS CurrencyName,  "
                       + " Hd.BLNo, Hd.BLDate, Hd.PrivateMark, Hd.PortOfLoading, Hd.DestinationPort, Hd.FinalPlaceOfDelivery, Hd.PreCarriageBy,  "
                       + " Hd.PlaceOfPreCarriage, Hd.CircularNo, Hd.CircularDate, Hd.OrderNo, Hd.OrderDate, Hd.BaleNoSeries, Hd.DescriptionOfGoods,  "
                       + " Hd.PackingMaterialDescription, Hd.KindsOfackages,  "
                       + " Hd.Compositions, Hd.OtherRefrence, Hd.TermsOfSale, Hd.NotifyParty, Hd.TransporterInformation,  "
                       + " Sm.ShipMethodName, Dt.DeliveryTermsName,  "
                       + " Prod.ProductName AS ProductName, Prod.ProductSpecification, Pg.ProductGroupName AS ProductDesignName, PCol.ColourName AS ProductColourName,  "
                       + " Pt.ProductTypeName, VProductArea.SizeName AS ProductSizeName, Pig.ProductInvoiceGroupName,  "
                       + " Soh.DocNo AS SaleOrderNo, "
                       + " Pig.ItcHsCode, Pig.Knots, Pl.BaleNo, Try_Parse(Pl.BaleNo AS INT) AS BaleNoToSort, Pl.Qty, L.Rate, L.Amount,  "
                       + " Pl.Qty * IsNull(VProductArea.SqFeetArea,0) AS SqFeetArea, Pl.Qty * IsNull(VProductArea.SqMeterArea,0) AS SqMeterArea  , "
                       + " Pl.GrossWeight, Pl.NetWeight, "
                       + " VSaleInvoice.TotalRugGrossWeight,VSaleInvoice.TotalFinishedProductGrossWeight, "
                       + " VSaleInvoice.TotalGrossWeight,VSaleInvoice.TotalRugNetWeight,VSaleInvoice.TotalFinishedProductNetWeight, "
                       + " VSaleInvoice.TotalNetWeight,VSaleInvoice.TotalRugQty,VSaleInvoice.TotalFinishedProductQty, VSaleInvoice.TotalQty, "
                       + " VBaleCount.TotalRugRolls, VBaleCount.TotalFinishedProductRolls, VBaleCount.TotalRolls, "
                       + " CASE WHEN VBaleCount.TotalRugRolls <> 0 AND VBaleCount.TotalFinishedProductRolls <> 0  "
                       + "      THEN Convert(NVARCHAR,VBaleCount.TotalRugRolls) +  ' + '  + Convert(NVARCHAR,VBaleCount.TotalFinishedProductRolls) + ' = ' + Convert(NVARCHAR,VBaleCount.TotalRolls) "
                       + "      WHEN VBaleCount.TotalRugRolls <> 0 AND VBaleCount.TotalFinishedProductRolls = 0  "
                       + "      THEN Convert(NVARCHAR, VBaleCount.TotalRugRolls) 	 "
                       + "      WHEN VBaleCount.TotalRugRolls = 0 AND VBaleCount.TotalFinishedProductRolls <> 0  "
                       + "      THEN Convert(NVARCHAR,VBaleCount.TotalFinishedProductRolls) 	 "
                       + " END AS TotalRoleText, "
                       + " CASE WHEN VSaleInvoice.TotalRugQty <> 0 AND VSaleInvoice.TotalFinishedProductQty <> 0  "
                       + "      THEN Convert(NVARCHAR, Convert(INT,VSaleInvoice.TotalRugQty)) +  ' + '  + Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalFinishedProductQty)) + ' = ' + Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalQty))  "
                       + "      WHEN VSaleInvoice.TotalRugQty <> 0 AND VSaleInvoice.TotalFinishedProductQty = 0   "
                       + "      THEN Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalRugQty))   "
                       + "      WHEN VSaleInvoice.TotalRugQty = 0 AND VSaleInvoice.TotalFinishedProductQty <> 0   "
                       + "      THEN Convert(NVARCHAR,Convert(INT,VSaleInvoice.TotalFinishedProductQty))     "
                       + " END AS TotalQtyText,  "
                       + " CASE WHEN VSaleInvoice.TotalRugGrossWeight <> 0 AND VSaleInvoice.TotalFinishedProductGrossWeight <> 0   "
                       + "      THEN Convert(NVARCHAR, VSaleInvoice.TotalRugGrossWeight) +  ' + '  + Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductGrossWeight) + ' = ' + Convert(NVARCHAR,VSaleInvoice.TotalGrossWeight)  "
                       + "      WHEN VSaleInvoice.TotalRugGrossWeight <> 0 AND VSaleInvoice.TotalFinishedProductGrossWeight = 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalRugGrossWeight)   "
                       + "      WHEN VSaleInvoice.TotalRugGrossWeight = 0 AND VSaleInvoice.TotalFinishedProductGrossWeight <> 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductGrossWeight)     "
                       + " END AS TotalGrossWeightText,  "
                       + " CASE WHEN VSaleInvoice.TotalRugNetWeight <> 0 AND VSaleInvoice.TotalFinishedProductNetWeight <> 0   "
                       + "      THEN Convert(NVARCHAR, VSaleInvoice.TotalRugNetWeight) +  ' + '  + Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductNetWeight) + ' = ' + Convert(NVARCHAR,VSaleInvoice.TotalNetWeight) "
                       + "      WHEN VSaleInvoice.TotalRugNetWeight <> 0 AND VSaleInvoice.TotalFinishedProductNetWeight = 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalRugNetWeight)   "
                       + "      WHEN VSaleInvoice.TotalRugNetWeight = 0 AND VSaleInvoice.TotalFinishedProductNetWeight <> 0   "
                       + "      THEN Convert(NVARCHAR,VSaleInvoice.TotalFinishedProductNetWeight)     "
                       + " END AS TotalNetWeightText  "
                       + " FROM  " + SchemaName + ".SaleInvoiceHeaders H   "
                       + " LEFT JOIN " + SchemaName + ".SaleInvoiceHeaderDetail Hd ON H.SaleInvoiceHeaderId = Hd.SaleInvoiceHeaderId  "
                       + " LEFT JOIN " + SchemaName + ".SaleDispatchHeaders Dh ON Hd.SaleDispatchHeaderId = Dh.SaleDispatchHeaderId  "
                       + " LEFT JOIN " + SchemaName + ".People P ON H.BillToBuyerId = P.PersonID  "
                       + " LEFT JOIN ( "
                       + "     SELECT H.PersonId, Max(H.Address) AS BillToBuyerAddress, Max(C.CityName) AS BillToPartyCity, Max(Con.CountryName) AS BillToPartyCountry  "
                       + "     FROM  " + SchemaName + ".PersonAddresses H   "
                       + "     LEFT JOIN " + SchemaName + ".Cities C ON H.CityId = C.CityId  "
                       + "     LEFT JOIN " + SchemaName + ".States S ON C.StateId = S.StateId  "
                       + "     LEFT JOIN " + SchemaName + ".Countries Con ON S.CountryId = Con.CountryId  "
                       + "     GROUP BY H.PersonId  "
                       + " ) AS VAddress ON P.PersonId = VAddress.PersonId  "
                       + " LEFT JOIN " + SchemaName + ".Currencies C ON H.CurrencyId = C.ID  "
                       + " LEFT JOIN " + SchemaName + ".DeliveryTerms Dt ON Dh.DeliveryTermsId = Dt.DeliveryTermsId  "
                       + " LEFT JOIN " + SchemaName + ".ShipMethods Sm ON Dh.ShipMethodId = Sm.ShipMethodId  "
                       + " LEFT JOIN " + SchemaName + ".SaleInvoiceLines L  ON H.SaleInvoiceHeaderId = L.SaleInvoiceHeaderId  "
                       + " LEFT JOIN " + SchemaName + ".SaleDispatchLines Dl ON L.SaleDispatchLineId = Dl.SaleDispatchLineId  "
                       + " LEFT JOIN " + SchemaName + ".PackingLines Pl ON Dl.PackingLineId = Pl.PackingLineId   "
                       + " LEFT JOIN " + SchemaName + ".SaleOrderLines Sol ON Pl.SaleOrderLineId = Sol.SaleOrderLineId  "
                       + " LEFT JOIN " + SchemaName + ".SaleOrderHeaders Soh ON Sol.SaleOrderHeaderId = Soh.SaleOrderHeaderId  "
                       + " LEFT JOIN " + SchemaName + ".Products Prod ON Pl.ProductId = Prod.ProductId  "
                       + " LEFT JOIN " + SchemaName + ".FinishedProduct Fp ON Prod.ProductId = Fp.ProductId  "
                       + " LEFT JOIN " + SchemaName + ".ProductGroups Pg ON Prod.ProductGroupId = Pg.ProductGroupId  "
                       + " LEFT JOIN " + SchemaName + ".Colours PCol ON Fp.ColourId = PCol.ColourId  "
                       + " LEFT JOIN " + SchemaName + ".ProductCategories Pc ON Prod.ProductCategoryId = Pc.ProductCategoryId  "
                       + " LEFT JOIN " + SchemaName + ".ProductTypes Pt ON Pc.ProductTypeId = Pt.ProductTypeId   "
                       + " LEFT JOIN " + SchemaName + ".ProductInvoiceGroups Pig ON Fp.ProductInvoiceGroupId = Pig.ProductInvoiceGroupId  "
                       + " LEFT JOIN (  "
                       + "     SELECT Ps.ProductId, S.SizeName, S.Area SqFeetArea, IsNull(S.Area,0) * 0.092903 AS SqMeterArea  "
                       + "     FROM  " + SchemaName + ".ProductSizes Ps    "
                       + "     LEFT JOIN " + SchemaName + ".Sizes S ON Ps.SizeId = S.SizeId  "
                       + "     LEFT JOIN " + SchemaName + ".ProductSizeTypes Pst ON Ps.ProductSizeTypeId = Pst.ProductSizeTypeId  "
                       + "     WHERE Pst.ProductSizeTypeName = 'Standard'  "
                       + " ) AS VProductArea ON Prod.ProductId = VProductArea.ProductId  "
                       + " LEFT JOIN (  "
                       + "     SELECT L.SaleInvoiceHeaderId,   "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName = 'Rug'  THEN Pl.GrossWeight END),0) AS TotalRugGrossWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName <> 'Rug'  THEN Pl.GrossWeight END),0) AS TotalFinishedProductGrossWeight,  "
                       + "     IsNull(Sum(Pl.GrossWeight),0) AS TotalGrossWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName = 'Rug'  THEN Pl.NetWeight END),0) AS TotalRugNetWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName <> 'Rug'  THEN Pl.NetWeight END),0) AS TotalFinishedProductNetWeight,  "
                       + "     IsNull(Sum(Pl.NetWeight),0) AS TotalNetWeight,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName = 'Rug'  THEN Pl.Qty END),0) AS TotalRugQty,  "
                       + "     IsNull(Sum(CASE WHEN Pt.ProductTypeName <> 'Rug'  THEN Pl.Qty END),0) AS TotalFinishedProductQty,  "
                       + "     IsNull(Sum(Pl.Qty),0) AS TotalQty,  "
                       + "     IsNull(Sum(L.Amount),0) AS TotalAmount  "
                       + "     FROM  " + SchemaName + ".SaleInvoiceLines L   "
                       + "     LEFT JOIN " + SchemaName + ".SaleDispatchLines Dl ON L.SaleDispatchLineId = Dl.SaleDispatchLineId  "
                       + "     LEFT JOIN " + SchemaName + ".PackingLines Pl ON PL.PackingLineId = Dl.PackingLineId  "
                       + "     LEFT JOIN " + SchemaName + ".Products P ON PL.ProductId = P.ProductId  "
                       + "     LEFT JOIN " + SchemaName + ".ProductCategories Pc ON P.ProductCategoryId = Pc.ProductCategoryId  "
                       + "     LEFT JOIN " + SchemaName + ".ProductTypes Pt ON Pc.ProductTypeId = Pt.ProductTypeId  "
                       + "     GROUP BY L.SaleInvoiceHeaderId  "
                       + " ) AS VSaleInvoice ON L.SaleInvoiceHeaderId = VSaleInvoice.SaleInvoiceHeaderId  "
                       + " LEFT JOIN (  "
                       + "     SELECT H.SaleInvoiceHeaderId,   "
                       + "     IsNull(Sum(VBales.RugRolls),0) AS TotalRugRolls,  "
                       + "     IsNull(Sum(VBales.FinishedRolls),0) AS TotalFinishedProductRolls,  "
                       + "     IsNull(Sum(VBales.TotalRolls),0) AS TotalRolls  "
                       + "     FROM  " + SchemaName + ".SaleInvoiceHeaders H   "
                       + "     LEFT JOIN (  "
                       + "         SELECT DISTINCT  L.SaleInvoiceHeaderId, Pl.BaleNo,  "
                       + "         CASE WHEN Pt.ProductTypeName = 'Rug' THEN 1 ELSE 0 END  AS RugRolls,  "
                       + "         CASE WHEN Pt.ProductTypeName <> 'Rug' THEN 1 ELSE 0 END  AS FinishedRolls,  "
                       + "         1 AS TotalRolls  "
                       + "         FROM  " + SchemaName + ".SaleInvoiceLines L   "
                       + "         LEFT JOIN " + SchemaName + ".SaleDispatchLines DL ON L.SaleDispatchLineId = Dl.SaleDispatchLineId  "
                       + "         LEFT JOIN " + SchemaName + ".PackingLines PL ON Dl.PackingLineId = Pl.PackingLineId  "
                       + "         LEFT JOIN " + SchemaName + ".Products P ON Pl.ProductId = P.ProductId  "
                       + "         LEFT JOIN " + SchemaName + ".ProductCategories Pc ON P.ProductCategoryId = Pc.ProductCategoryId  "
                       + "         LEFT JOIN " + SchemaName + ".ProductTypes Pt ON Pc.ProductTypeId = Pt.ProductTypeId  "
                       + "     ) AS VBales ON H.SaleInvoiceHeaderId = VBales.SaleInvoiceHeaderId  "
                       + "     GROUP BY H.SaleInvoiceHeaderId  "
                       + " ) AS VBaleCount ON H.SaleInvoiceHeaderId = VBaleCount.SaleInvoiceHeaderId  "
                       + " WHERE H.SaleInvoiceHeaderId	= @Id  "
                       + " End ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion












            #region ProcCreateMenu
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".ProcCreateMenu') IS NOT NULL DROP PROCEDURE " + SchemaName + ".ProcCreateMenu  ";
                db.Database.ExecuteSqlCommand(mQry);


                mQry = "CREATE PROCEDURE " + SchemaName + ".[ProcCreateMenu]       "
                       + "     @Module VARCHAR(50) ,       "
                       + "     @SubModule VARCHAR(50) ,      "
                       + "     @Controller VARCHAR(100) ,      "
                       + "     @Action VARCHAR(100) ,       "
                       + "     @Menu VARCHAR(50)   ,  "
                       + "     @Srl     VARCHAR(5),   "
                       + "     @Description  VARCHAR(100) "
                       + " As  BEGIN   "
                       + " IF (NOT EXISTS(SELECT * FROM  " + SchemaName + ".MenuModules  WHERE ModuleName = @Module))    "
                       + " BEGIN        "
                       + "     INSERT INTO " + SchemaName + ".MenuModules	(ModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)       "
                       + "     VALUES (@Module, 'glyphicon glyphicon-gift', 1, 'admin', 'admin', GetDate(), GetDate())   "
                       + " END  "
                       + " IF (NOT EXISTS(SELECT * FROM  " + SchemaName + ".MenuSubModules  WHERE SubModuleName  = @SubModule))    "
                       + " BEGIN        "
                       + "     INSERT INTO " + SchemaName + ".MenuSubModules(SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)       "
                       + "     VALUES(@SubModule,	'glyphicon glyphicon-road', 1,	'admin', 'admin', GetDate(), GetDate())   "
                       + " END    "
                       + " IF (NOT EXISTS(SELECT * FROM  " + SchemaName + ".ControllerActions  WHERE ControllerName = @Controller AND ActionName = @Action))    "
                       + " BEGIN      "
                       + "     INSERT INTO " + SchemaName + "ControllerActions(ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)    "
                       + "     VALUES(@Controller,	@Action, 1,	'admin', 'admin', GetDate(), GetDate())   "
                       + " END    "
                       + " IF (NOT EXISTS(SELECT * FROM  " + SchemaName + ".Menus LEFT JOIN " + SchemaName + ".MenuSubModules ON Menus.SubModuleId = MenuSubModules.SubModuleId LEFT JOIN " + SchemaName + ".MenuModules ON Menus.ModuleId = MenuModules.ModuleId   "
                       + "                 WHERE MenuName  = @Menu AND ModuleName = @Module AND SubModuleName = @SubModule  ))    "
                       + " BEGIN       "
                       + "     INSERT INTO Menus(MenuName, IconName, ModuleId, SubModuleId, ControllerActionId, Srl, Description, IsActive, CreatedBy, ModifiedBy, CreatedDate,ModifiedDate)       "
                       + "     SELECT @Menu,	'glyphicon glyphicon-book',        "
                       + "     (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = @Module),      (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules  "
                       + "     WHERE SubModuleName  = @SubModule),      (SELECT ControllerActionId  FROM  " + SchemaName + ".ControllerActions   "
                       + "     WHERE ControllerName  = @Controller AND ActionName = @Action), @Srl, @Description,     1,	'admin', 'admin', GetDate(), GetDate()   "
                       + " END   "
                       + " END  ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion




        }

        static void CreateDatabseFunctions()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string mQry = "";

            #region Split
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".Split') IS NOT NULL DROP FUNCTION " + SchemaName + ".Split  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " Create  FUNCTION " + SchemaName + ".Split(@String nvarchar(4000), @Delimiter char(1)) "
                        + "RETURNS @Results TABLE (Items nvarchar(4000)) "
                        + "AS "
                        + "BEGIN "
                        + "DECLARE @INDEX INT "
                        + "DECLARE @SLICE nvarchar(4000) "
                        + "SELECT @INDEX = 1 "
                        + "WHILE @INDEX !=0 "
                        + "BEGIN "
                        + "SELECT @INDEX = CHARINDEX(@Delimiter,@STRING) "
                        + "IF @INDEX !=0 "
                        + "SELECT @SLICE = LEFT(@STRING,@INDEX - 1) "
                        + "ELSE "
                        + "SELECT @SLICE = @STRING "
                        + "INSERT INTO @Results(Items) VALUES(@SLICE) "
                        + "SELECT @STRING = RIGHT(@STRING,LEN(@STRING) - @INDEX) "
                        + "IF LEN(@STRING) = 0 BREAK "
                        + "END "
                        + "RETURN "
                        + "END ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion

            #region FunConvertSqFeetToSqYard
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".FuncConvertSqFeetToSqYard') IS NOT NULL DROP FUNCTION " + SchemaName + ".FuncConvertSqFeetToSqYard  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " Create  FUNCTION " + SchemaName + ".FuncConvertSqFeetToSqYard(@SqFeets FLOAT) "
                         + " RETURNS @Results TABLE (SqYard FLOAT) "
                         + " AS "
                         + " BEGIN "
                         + " DECLARE @MyInt INT  "
                         + " DECLARE @MyFrcation FLOAT "
                         + " DECLARE @TempSqYard FLOAT "
                         + " SET @TempSqYard = Round(@SqFeets * 0.111111111, 2) "
                         + " SET @MyInt = Convert(INT,@TempSqYard) "
                         + " SET @MyFrcation = @TempSqYard - @MyInt "
                         + " If (@MyFrcation * 16) > 1  "
                         + "    begin  "
                         + "    SET @MyFrcation = @MyFrcation * 16 "
                         + "    SET @MyFrcation = convert(INT,@MyFrcation) "
                         + "    SET @MyFrcation = @MyFrcation / 16 "
                         + "    end "
                         + " Else IF @MyInt > 0  "
                         + "     BEGIN  "
                         + "      SET @MyFrcation = 0 "
                         + "     END  "
                         + " SET @SqFeets = @MyInt + @MyFrcation "
                         + " INSERT INTO @Results(SqYard) VALUES(@SqFeets) "
                         + " RETURN "
                         + " END ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion

            #region FGetExcessStock
            try
            {
                mQry = " IF OBJECT_ID ('" + SchemaName + ".FGetExcessStock') IS NOT NULL DROP FUNCTION " + SchemaName + ".FGetExcessStock  ";
                db.Database.ExecuteSqlCommand(mQry);

                mQry = " ALTER FUNCTION '" + SchemaName + ".FGetExcessStock (@ProductId Integer) "
                         + " Returns Float AS "
                         + " BEGIN  "
                         + " DECLARE @ExcessStockQty Float  "
                         + " SET @ExcessStockQty =  "
                         + " (SELECT isnull(sum(V.StockQty),0)+isnull(sum(V.PendingPurchaseOrderQty),0)+isnull(sum(V.PendingProdOrderQty),0)+isnull( sum(V.PendingWeavingOrderQty),0) - isnull( sum(V.PendingSaleOrderQty),0) AS ExcessStockQty  "
                         + " FROM "
                         + " ( "
                         + " SELECT H.ProductId, sum(H.Qty_Rec) - sum(H.Qty_Iss)  AS StockQty, 0 AS PendingPurchaseOrderQty , 0  AS PendingProdOrderQty, 0  AS PendingWeavingOrderQty,  0 AS PendingSaleOrderQty "
                         + " FROM Web.Stocks H WITH (Nolock) "
                         + " WHERE H.ProductId = @ProductId "
                         + " GROUP BY H.ProductId  "
                         + " HAVING sum(H.Qty_Rec) - sum(H.Qty_Iss) > 0 "
                         + " UNION ALL "
                         + " SELECT H.ProductId, 0  AS StockQty, sum(H.BalanceQty)  AS PendingPurchaseOrderQty , 0 AS PendingProdOrderQty, 0  AS PendingWeavingOrderQty,  0 AS PendingSaleOrderQty "
                         + " FROM Web.ViewPurchaseOrderBalance H WITH (Nolock) "
                         + " WHERE H.ProductId = @ProductId "
                         + " GROUP BY H.ProductId  "
                         + " UNION ALL "
                         + " SELECT H.ProductId, 0  AS StockQty, 0  AS PendingPurchaseOrderQty , sum(H.BalanceQty)  AS PendingProdOrderQty, 0  AS PendingWeavingOrderQty,  0 AS PendingSaleOrderQty "
                         + " FROM Web.ViewProdOrderBalance H WITH (Nolock) "
                         + " WHERE H.ProductId = @ProductId "
                         + " GROUP BY H.ProductId  "
                         + " UNION ALL "
                         + " SELECT H.ProductId, 0  AS StockQty, 0  AS PendingPurchaseOrderQty , 0  AS PendingProdOrderQty , sum(H.BalanceQty)  AS PendingWeavingOrderQty, 0 AS PendingSaleOrderQty "
                         + " FROM Web.ViewJobOrderBalance H WITH (Nolock) "
                         + " LEFT JOIN Web.JobOrderLines JOL ON JOL.JobOrderLineId = H.JobOrderLineId  "
                         + " LEFT JOIN Web.JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId  "
                         + " WHERE H.ProductId = @ProductId AND JOH.ProcessId = 41 "
                         + " GROUP BY H.ProductId  "
                         + " UNION ALL "
                         + " SELECT H.ProductId, 0  AS StockQty, 0  AS PendingPurchaseOrderQty , 0  AS PendingProdOrderQty, 0  AS PendingWeavingOrderQty, sum(H.BalanceQty)  AS PendingSaleOrderQty "
                         + " FROM Web.ViewSaleOrderBalance H WITH (Nolock) "
                         + " WHERE H.ProductId = @ProductId "
                         + " GROUP BY H.ProductId  "
                         + " ) V "
                         + " GROUP BY V.ProductId) "
                         + " RETURN @ExcessStockQty; "
                         + " END ";
                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
            #endregion


        }




        static void CreateMenuModules()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string mQry = "";

            try
            {
                mQry = " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sales', 1, 'glyphicon glyphicon-globe', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Planning', 2, 'glyphicon glyphicon-list-alt', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Purchase', 3, 'glyphicon glyphicon-shopping-cart', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Dyeing', 4, 'glyphicon glyphicon-random', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Weaving', 5, 'glyphicon glyphicon-tower', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Finishing', 6, 'glyphicon glyphicon-transfer', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Production', 7, 'glyphicon glyphicon-tasks', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Inventory', 8, 'glyphicon glyphicon-stats', 1, 'admin', 'admin', '2015-01-17 11:41:28', '2015-01-17 11:41:28') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Accounts', 10, 'glyphicon glyphicon-file', 1, 'admin', 'admin', '2015-01-17 09:57:33', '2015-01-17 09:57:33') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Human Resource', 11, 'glyphicon glyphicon-user', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Gate', 12, 'glyphicon glyphicon-sound-dolby', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product', 13, 'glyphicon glyphicon-tag', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Spinning', 14, 'glyphicon glyphicon-retweet', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Basic Setup', 15, 'glyphicon glyphicon-gift', 1, 'admin', 'admin', '2015-01-20 11:12:41', '2015-01-20 11:12:41') " +


                " INSERT INTO " + SchemaName + ".MenuModules (ModuleName, Srl, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Company', 16, 'glyphicon glyphicon-home', 1, 'admin', 'admin', '2015-01-20 10:36:58', '2015-01-20 10:36:58') ";

                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
        }

        static void CreateMenuSubModules()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string mQry = "";

            try
            {
                mQry = " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Documents', 'glyphicon glyphicon-folder-open', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Setup', 'glyphicon glyphicon-cog', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Standard Reports', 'glyphicon glyphicon-file', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Summary Reports', 'glyphicon glyphicon-road', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Rugs', 'glyphicon glyphicon-th-large', 1, 'admin', 'admin', '2015-01-17 09:38:56', '2015-01-17 09:38:56') " +


                " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Finished Products', 'glyphicon glyphicon-hdd', 1, 'admin', 'admin', '2015-01-16 14:02:27', '2015-01-16 14:02:27') " +


                " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Raw Materials', 'glyphicon glyphicon-picture', 1, 'admin', 'admin', '2014-12-10', '2014-12-10') " +


                " INSERT INTO " + SchemaName + ".MenuSubModules (SubModuleName, IconName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Other Materials', 'glyphicon glyphicon-cutlery', 1, 'admin', 'admin', '2014-12-10', '2014-12-10') ";



                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
        }


        static void CreateControllerAction()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string mQry = "";

            try
            {
                mQry = " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Agent', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Buyer', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('CarpetMaster', 'Index', 1, 'admin', 'admin', '2015-01-17 09:50:10', '2015-01-17 09:50:10') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('City', 'Index', 1, 'admin', 'admin', '2015-01-20 11:12:41', '2015-01-20 11:12:41') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Colour', 'Index', 1, 'admin', 'admin', '2015-01-17 09:39:02', '2015-01-17 09:39:02') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('CostCenter', 'Index', 1, 'admin', 'admin', '2015-01-17 09:57:36', '2015-01-17 09:57:36') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Country', 'Index', 1, 'admin', 'admin', '2015-01-20 11:12:41', '2015-01-20 11:12:41') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Currency', 'Index', 1, 'admin', 'admin', '2015-01-17 10:08:38', '2015-01-17 10:08:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DeliveryTerms', 'Index', 1, 'admin', 'admin', '2015-01-17 10:08:38', '2015-01-17 10:08:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Department', 'Index', 1, 'admin', 'admin', '2015-01-17 10:13:10', '2015-01-17 10:13:10') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DescriptionOfGoods', 'Index', 1, 'admin', 'admin', '2015-01-17 10:08:38', '2015-01-17 10:08:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Designation', 'Index', 1, 'admin', 'admin', '2015-01-17 10:13:10', '2015-01-17 10:13:10') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DesignConsumptionHeader', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DispatchWaybillHeader', 'Index', 1, 'admin', 'admin', '2015-01-28 14:29:34', '2015-01-28 14:29:34') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Division', 'Index', 1, 'admin', 'admin', '2015-01-20 10:42:33', '2015-01-20 10:42:33') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DocumentCategory', 'Index', 1, 'admin', 'admin', '2015-01-20 10:36:58', '2015-01-20 10:36:58') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DocumentType', 'Index', 1, 'admin', 'admin', '2015-01-20 10:36:58', '2015-01-20 10:36:58') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DrawBackTariffHead', 'Index', 1, 'admin', 'admin', '2015-01-17 10:08:38', '2015-01-17 10:08:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Employee', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('FinishedProduct', 'ProductTypeIndex', 1, 'admin', 'admin', '2015-01-16 14:02:27', '2015-01-16 14:02:27') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Godown', 'Index', 1, 'admin', 'admin', '2015-01-17 11:41:28', '2015-01-17 11:41:28') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('JobWorker', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('LedgerAccount', 'Index', 1, 'admin', 'admin', '2015-01-17 09:57:35', '2015-01-17 09:57:35') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('LedgerAccountGroup', 'Index', 1, 'admin', 'admin', '2015-01-17 09:57:33', '2015-01-17 09:57:33') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Manufacturer', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('PackingHeader', 'Index', 1, 'admin', 'admin', '2015-01-17 11:47:41', '2015-01-17 11:47:41') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('PersonRateGroup', 'Index', 1, 'admin', 'admin', '2015-01-20 11:12:41', '2015-01-20 11:12:41') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Process', 'Index', 1, 'admin', 'admin', '2015-01-20 11:06:24', '2015-01-20 11:06:24') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProcessSequenceHeader', 'Index', 1, 'admin', 'admin', '2015-01-20 11:06:24', '2015-01-20 11:06:24') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product', 'ProductCategoryMaterialIndex', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductCategory', 'Index', 1, 'admin', 'admin', '2015-01-17 09:05:15', '2015-01-17 09:05:15') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductCollection', 'Index', 1, 'admin', 'admin', '2015-01-17 09:39:04', '2015-01-17 09:39:04') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductContentHeader', 'Index', 1, 'admin', 'admin', '2015-01-17 09:39:06', '2015-01-17 09:39:06') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductCustomGroupHeader', 'Index', 1, 'admin', 'admin', '2015-01-20 11:02:38', '2015-01-20 11:02:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductDesign', 'Index', 1, 'admin', 'admin', '2015-01-20 10:46:38', '2015-01-20 10:46:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductDesignPattern', 'Index', 1, 'admin', 'admin', '2015-01-20 10:48:25', '2015-01-20 10:48:25') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductGroup', 'Index', 1, 'admin', 'admin', '2015-01-17 09:03:46', '2015-01-17 09:03:46') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductNature', 'Index', 1, 'admin', 'admin', '2015-01-20 11:08:32', '2015-01-20 11:08:32') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductQuality', 'Index', 1, 'admin', 'admin', '2015-01-17 09:39:07', '2015-01-17 09:39:07') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductShape', 'Index', 1, 'admin', 'admin', '2015-01-17 09:39:09', '2015-01-17 09:39:09') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductStyle', 'Index', 1, 'admin', 'admin', '2015-01-20 10:52:44', '2015-01-20 10:52:44') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductType', 'Index', 1, 'admin', 'admin', '2015-01-20 11:08:32', '2015-01-20 11:08:32') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductTypeAttribute', 'Index', 1, 'admin', 'admin', '2015-01-20 11:02:38', '2015-01-20 11:02:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ProductInvoiceGroup', 'Index', 1, 'admin', 'admin', '2015-01-20 11:02:38', '2015-01-20 11:02:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('PurchaseIndentHeader', 'Index', 1, 'admin', 'admin', '2015-10-12', '2015-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('PurchaseGoodsReceiptHeader', 'Index', 1, 'admin', 'admin', '2015-10-12', '2015-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('PurchaseOrderHeader', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_PackingRegister', 'PackingRegister', 1, 'admin', 'admin', '2015-01-29 15:08:46', '2015-01-29 15:08:46') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleInvoiceReport', 'SaleInvoiceReport', 1, 'admin', 'admin', '2015-01-27 14:41:35', '2015-01-27 14:41:35') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleInvoiceSummary', 'SaleInvoiceSummary', 1, 'admin', 'admin', '2015-01-27 15:39:38', '2015-01-27 15:39:38') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleOrderAmendmentReport', 'SaleOrderAmendmentReport', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleOrderBalance', 'SaleOrderBalance', 1, 'admin', 'admin', '2015-01-27 14:24:07', '2015-01-27 14:24:07') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleOrderCancelReport', 'SaleOrderCancelReport', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleOrderReport', 'SaleOrderReport', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleOrderStatus', 'SaleOrderStatus', 1, 'admin', 'admin', '2015-01-27 12:21:12', '2015-01-27 12:21:12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleOrderStatusSummary', 'SaleOrderStatusSummary', 1, 'admin', 'admin', '2015-01-28 10:53:03', '2015-01-28 10:53:03') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Report_SaleOrderSummary', 'SaleOrderSummary', 1, 'admin', 'admin', '2015-01-27 12:06:08', '2015-01-27 12:06:08') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Route', 'Index', 1, 'admin', 'admin', '2015-01-28 09:58:06', '2015-01-28 09:58:06') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('SaleInvoiceHeader', 'Index', 1, 'admin', 'admin', '2015-01-20 10:14:05', '2015-01-20 10:14:05') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('SaleOrderAmendmentHeader', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('SaleOrderCancelHeader', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('SaleOrderHeader', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('SalesTaxGroupParty', 'Index', 1, 'admin', 'admin', '2015-01-20 10:27:10', '2015-01-20 10:27:10') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('SalesTaxGroupProduct', 'Index', 1, 'admin', 'admin', '2015-01-20 10:27:11', '2015-01-20 10:27:11') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ServiceTaxCategory', 'Index', 1, 'admin', 'admin', '2015-01-20 10:32:06', '2015-01-20 10:32:06') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('ShipMethod', 'Index', 1, 'admin', 'admin', '2015-02-06 14:15:19', '2015-02-06 14:15:19') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Site', 'Index', 1, 'admin', 'admin', '2015-01-20 10:42:29', '2015-01-20 10:42:29') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Size', 'Index', 1, 'admin', 'admin', '2015-02-06 09:32:29', '2015-02-06 09:32:29') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('State', 'Index', 1, 'admin', 'admin', '2015-01-20 11:12:41', '2015-01-20 11:12:41') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Supplier', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('TdsCategory', 'Index', 1, 'admin', 'admin', '2015-01-20 10:21:06', '2015-01-20 10:21:06') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('TdsGroup', 'Index', 1, 'admin', 'admin', '2015-01-20 10:21:06', '2015-01-20 10:21:06') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Transporter', 'Index', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".ControllerActions (ControllerName, ActionName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Unit', 'Index', 1, 'admin', 'admin', '2015-02-06 10:04:24', '2015-02-06 10:04:24') ";



                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
        }


        static void CreateMenus()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string mQry = "";

            try
            {
                mQry = " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order', 'Documents', '1.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SaleOrderHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Cancel', 'Documents', '1.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SaleOrderCancelHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Amendment', 'Documents', '1.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SaleOrderAmendmentHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Packing', 'Documents', '1.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'PackingHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Invoice', 'Documents', '1.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SaleInvoiceHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Dispatch Waybill', 'Documents', '1.25', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'DispatchWaybillHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Buyer', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Buyer' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Manufacturer', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Manufacturer' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Agent', 'Setup', '2.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Agent' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Currency', 'Setup', '2.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Currency' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('DeliveryTerms', 'Setup', '2.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'DeliveryTerms' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Description Of Goods', 'Setup', '2.25', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'DescriptionOfGoods' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Draw Back Tariff', 'Setup', '2.30', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'DrawBackTariffHead' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sales Tax Group Party', 'Setup', '2.35', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SalesTaxGroupParty' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sales Tax Group Product', 'Setup', '2.40', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SalesTaxGroupProduct' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Ship Method', 'Setup', '2.45', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ShipMethod' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2015-02-06 14:15:19', '2015-02-06 14:15:19') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Route', 'Setup', '2.50', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Route' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Invoice Group', 'Setup', '2.55', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductInvoiceGroup' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Report', 'Report', '3.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Standard Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleOrderReport' And ActionName = 'SaleOrderReport'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Cancel Report', 'Report', '3.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Standard Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleOrderCancelReport' And ActionName = 'SaleOrderCancelReport'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Amendment Report', 'Report', '3.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Standard Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleOrderAmendmentReport' And ActionName = 'SaleOrderAmendmentReport'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Packing Register', 'Report', '3.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Standard Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_PackingRegister' And ActionName = 'PackingRegister'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Invoice Report', 'Report', '3.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Standard Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleInvoiceReport' And ActionName = 'SaleInvoiceReport'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Summary', 'Report', '4.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Summary Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleOrderSummary' And ActionName = 'SaleOrderSummary'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Status', 'Report', '4.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Summary Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleOrderStatus' And ActionName = 'SaleOrderStatus'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Balance', 'Report', '4.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Summary Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleOrderBalance' And ActionName = 'SaleOrderBalance'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Order Status Summary', 'Report', '4.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Summary Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleOrderStatusSummary' And ActionName = 'SaleOrderStatusSummary'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sale Invoice Summary', 'Report', '4.25', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Sales'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Summary Reports'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Report_SaleInvoiceSummary' And ActionName = 'SaleInvoiceSummary'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Purchase Indent', 'Documents', '1.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Purchase'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'PurchaseIndentHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Purchase Order', 'Documents', '1.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Purchase'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'PurchaseOrderHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Purchase Receipt', 'Documents', '1.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Purchase'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Documents'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'PurchaseGoodsReceiptHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Supplier', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Purchase'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Supplier' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Transporter', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Purchase'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Transporter' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sales Tax Group Party', 'Setup', '2.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Purchase'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SalesTaxGroupParty' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Sales Tax Group Product', 'Setup', '2.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Purchase'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'SalesTaxGroupProduct' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Job Worker', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Weaving'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'JobWorker' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Design Consumption', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Weaving'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'DesignConsumptionHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Process', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Finishing'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Process' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Process Sequence', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Finishing'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProcessSequenceHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Godown', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Inventory'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Godown' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Ledger Account Group', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Accounts'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'LedgerAccountGroup' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Ledger Account', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Accounts'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'LedgerAccount' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Cost Center', 'Setup', '2.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Accounts'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'CostCenter' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Tds Category', 'Setup', '2.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Accounts'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'TdsCategory' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Tds Group', 'Setup', '2.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Accounts'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'TdsGroup' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Service Tax Category', 'Setup', '2.25', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Accounts'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ServiceTaxCategory' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Employee', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Human Resource'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Employee' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Departmenr', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Human Resource'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Department' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Designation', 'Setup', '2.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Human Resource'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Designation' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Carpet', 'Rugs', '1.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'CarpetMaster' And ActionName = 'Index'), 0, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Size', 'Rugs', '1.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Size' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2015-02-06 09:32:29', '2015-02-06 09:32:29') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Quality', 'Rugs', '1.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductQuality' And ActionName = 'Index'), '1', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Collection', 'Rugs', '1.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductCollection' And ActionName = 'Index'), '1', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Style', 'Rugs', '1.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductStyle' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Shape', 'Rugs', '1.25', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductShape' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Content', 'Rugs', '1.30', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductContentHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Design Pattern', 'Rugs', '1.35', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductDesignPattern' And ActionName = 'Index'), '1', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Colour', 'Rugs', '1.40', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Colour' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Construction', 'Rugs', '1.45', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductCategory' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Colour Ways', 'Rugs', '1.55', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Rugs'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductDesign' And ActionName = 'Index'), '1', 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Finished Products', 'Finished Products', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Finished Products'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'FinishedProduct' And ActionName = 'ProductTypeIndex'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Group', 'Finished Products', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Finished Products'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductGroup' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Category', 'Finished Products', '2.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Finished Products'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductCategory' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Raw Materials', 'Raw Materials', '3.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Raw Materials'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Product' And ActionName = 'ProductCategoryMaterialIndex'), 1, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Group', 'Raw Materials', '3.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Raw Materials'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductGroup' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Category', 'Raw Materials', '3.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Raw Materials'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductCategory' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Other Materials', 'Other Materials', '4.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Other Materials'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Product' And ActionName = 'ProductCategoryMaterialIndex'), 3, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Group', 'Other Materials', '4.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Other Materials'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductGroup' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Category', 'Other Materials', '4.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Other Materials'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductCategory' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Nature', 'Setup', '5.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductNature' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Type', 'Setup', '5.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductType' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Type Attribute', 'Setup', '5.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductTypeAttribute' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Unit', 'Setup', '5.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Unit' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2015-02-06 10:04:24', '2015-02-06 10:04:24') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Product Custom Group', 'Setup', '5.25', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Product'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'ProductCustomGroupHeader' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Buyer', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Buyer' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Supplier', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Supplier' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Job Worker', 'Setup', '2.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'JobWorker' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Manufacturer', 'Setup', '2.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Manufacturer' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Employee', 'Setup', '2.20', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Employee' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Transporter', 'Setup', '2.25', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Transporter' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Agent', 'Setup', '2.30', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Agent' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('City', 'Setup', '2.35', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'City' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('State', 'Setup', '2.40', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'State' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Country', 'Setup', '2.45', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Country' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Person Rate Group', 'Setup', '2.50', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Basic Setup'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'PersonRateGroup' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Document Category', 'Setup', '2.00', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Company'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'DocumentCategory' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Document Type', 'Setup', '2.05', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Company'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'DocumentType' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Site', 'Setup', '2.10', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Company'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Site' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') " +


                " INSERT INTO " + SchemaName + ".Menus (MenuName, Description, Srl, IconName, ModuleId, SubModuleId, ControllerActionId, RouteId, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) " +
                " VALUES ('Division', 'Setup', '2.15', 'glyphicon glyphicon-book', (SELECT ModuleId FROM  " + SchemaName + ".MenuModules WHERE ModuleName = 'Company'), (SELECT SubModuleId FROM  " + SchemaName + ".MenuSubModules WHERE SubModuleName = 'Setup'), (SELECT ControllerActionId FROM  " + SchemaName + ".ControllerActions WHERE ControllerName = 'Division' And ActionName = 'Index'), NULL, 1, 'admin', 'admin', '2014-10-12', '2014-10-12') ";




                db.Database.ExecuteSqlCommand(mQry);
            }
            catch (Exception e) { _errors = _errors + e.Message; }
        }
    }

}