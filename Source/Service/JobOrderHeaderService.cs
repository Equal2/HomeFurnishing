using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Data.Linq;
using Model.DatabaseViews;
using Model.ViewModels;
using AutoMapper;

namespace Service
{
    public interface IJobOrderHeaderService : IDisposable
    {
        JobOrderHeader Create(JobOrderHeader s);
        void Delete(int id);
        void Delete(JobOrderHeader s);
        JobOrderHeaderViewModel GetJobOrderHeader(int id);
        JobOrderHeader Find(int id);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListByCostCenter(int CostCenterId);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderList(int DocumentTypeId, string Uname);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListPendingToSubmit(int DocumentTypeId, string Uname);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListPendingToReview(int DocumentTypeId, string Uname);
        void Update(JobOrderHeader s);
        string GetMaxDocNo();
        IEnumerable<ComboBoxList> GetJobWorkerHelpList(int Processid, string term);//PurchaseOrderHeaderId
        string FGetJobOrderCostCenter(int DocTypeId, DateTime DocDate, int DivisionId, int SiteId);
        IEnumerable<WeavingOrderWizardViewModel> GetProdOrdersForWeavingWizard(int DocTypeId);
        DateTime AddDueDate(DateTime Base, int DueDays);
        void UpdateProdUidJobWorkers(ref ApplicationDbContext context, JobOrderHeader Header);
        string ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName);
        JobOrderLineProgressViewModel GetLineProgressDetail(int JobOrderLineId);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
        IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id);
    }
    public class JobOrderHeaderService : IJobOrderHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobOrderHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public JobOrderHeader Create(JobOrderHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderHeader>().Delete(id);
        }
        public void Delete(JobOrderHeader s)
        {
            _unitOfWork.Repository<JobOrderHeader>().Delete(s);
        }
        public void Update(JobOrderHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderHeader>().Update(s);
        }


        public JobOrderHeader Find(int id)
        {
            return _unitOfWork.Repository<JobOrderHeader>().Find(id);
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<JobOrderHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListByCostCenter(int CostCenterId)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.JobOrderHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.CostCenterId == CostCenterId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select new JobOrderHeaderViewModel
                    {
                        DueDate = p.DueDate,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        Status = p.Status,
                        JobOrderHeaderId = p.JobOrderHeaderId,
                        ModifiedBy = p.ModifiedBy,
                    });
        }

        public IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderList(int DocumentTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];      


            
            return (from p in db.JobOrderHeader
                    join t in db.Persons on p.JobWorkerId equals t.PersonID
                    join dt in db.DocumentType on p.DocTypeId equals dt.DocumentTypeId
                    orderby p.DocDate descending, p.DocNo descending
                    where p.DocTypeId == DocumentTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select new JobOrderHeaderViewModel
                    {
                        DocTypeName = dt.DocumentTypeName,
                        DueDate = p.DueDate,
                        JobWorkerName = t.Name + "," + t.Suffix,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        CostCenterName = p.CostCenter.CostCenterName,
                        Remark = p.Remark,
                        Status = p.Status,
                        JobOrderHeaderId = p.JobOrderHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        GodownName = p.Godown.GodownName,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        GatePassDocNo = p.GatePassHeader.DocNo,
                        GatePassHeaderId = p.GatePassHeaderId,
                        GatePassDocDate = p.GatePassHeader.DocDate,
                        GatePassStatus = (p.GatePassHeaderId != null ? p.GatePassHeader.Status : 0),
                        TotalQty = p.JobOrderLines.Sum(m => m.Qty),
                        DecimalPlaces = (from o in p.JobOrderLines
                                         join prod in db.Product on o.ProductId equals prod.ProductId
                                         join u in db.Units on prod.UnitId equals u.UnitId
                                         select u.DecimalPlaces).Max(),
                       


                    });
        }

        public JobOrderHeaderViewModel GetJobOrderHeader(int id)
        {
            return (from p in db.JobOrderHeader
                    where p.JobOrderHeaderId == id
                    select new JobOrderHeaderViewModel
                    {
                        DocTypeName = p.DocType.DocumentTypeName,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        JobOrderHeaderId = p.JobOrderHeaderId,
                        Status = p.Status,
                        DocTypeId = p.DocTypeId,
                        DueDate = p.DueDate,
                        ProcessId = p.ProcessId,
                        JobWorkerId = p.JobWorkerId,
                        MachineId = p.MachineId,
                        BillToPartyId = p.BillToPartyId,
                        OrderById = p.OrderById,
                        GodownId = p.GodownId,
                        UnitConversionForId = p.UnitConversionForId,
                        CostCenterId = p.CostCenterId,
                        TermsAndConditions = p.TermsAndConditions,
                        CreditDays = p.CreditDays,
                        DivisionId = p.DivisionId,
                        SiteId = p.SiteId,
                        LockReason = p.LockReason,
                        CostCenterName = p.CostCenter.CostCenterName,
                        ModifiedBy = p.ModifiedBy,
                        GatePassHeaderId = p.GatePassHeaderId,
                        GatePassDocNo = p.GatePassHeader.DocNo,
                        GatePassStatus = (p.GatePassHeader == null ? 0 : p.GatePassHeader.Status),
                        GatePassDocDate = p.GatePassHeader.DocDate,
                        CreatedDate = p.CreatedDate,
                        DeliveryTermsId = p.DeliveryTermsId,
                        ShipToAddressId = p.ShipToAddressId,
                        CurrencyId = p.CurrencyId,
                        SalesTaxGroupPersonId = p.SalesTaxGroupPersonId,
                        ShipMethodId = p.ShipMethodId,
                        DocumentShipMethodId = p.DocumentShipMethodId,
                        TransporterId = p.TransporterId,
                        AgentId= p.AgentId,
                        FinancierId = p.FinancierId,
                        SalesExecutiveId = p.SalesExecutiveId,
                        IsDoorDelivery = p.IsDoorDelivery ?? false,
                        PayTermAdvancePer= p.PayTermAdvancePer,
                        PayTermOnDeliveryPer = p.PayTermOnDeliveryPer,
                        PayTermOnDueDatePer= p.PayTermOnDueDatePer,
                        PayTermCashPer = p.PayTermCashPer,
                        PayTermBankPer= p.PayTermBankPer,
                    }).FirstOrDefault();
        }


        public IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersWithPatternMatch(int JobWorkerId, string term, int Limiter)//Product Id
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            var tem = (from p in db.ViewJobOrderBalance
                       join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                       from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                       join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                       from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                       join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                       from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                       join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                       from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                       join t3 in db.Product on p.ProductId equals t3.ProductId
                       join Jol in db.JobOrderLine on p.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                       where p.BalanceQty > 0 && p.JobWorkerId == JobWorkerId
                       && p.DivisionId == DivisionId && p.SiteId == SiteId
                       && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension3Tab.Dimension3Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension4Tab.Dimension4Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : t3.ProductName.ToLower().Contains(term.ToLower())))
                       orderby p.JobOrderNo
                       select new JobOrderHeaderListViewModel
                       {
                           DocNo = p.JobOrderNo,
                           JobOrderLineId = p.JobOrderLineId,
                           Dimension1Name = Dimension1Tab.Dimension1Name,
                           Dimension2Name = Dimension2Tab.Dimension2Name,
                           Dimension3Name = Dimension3Tab.Dimension3Name,
                           Dimension4Name = Dimension4Tab.Dimension4Name,
                           ProductName = t3.ProductName,
                           ProductUidId = JobOrderLineTab.ProductUidId,
                           ProductUidName = JobOrderLineTab.ProductUid.ProductUidName,
                           LotNo = JobOrderLineTab.LotNo,
                           BalanceQty = p.BalanceQty,
                       }).Take(Limiter);

            return (tem);
        }

        public IQueryable<JobOrderHeaderListViewModel> GetPendingJobOrdersWithPatternMatchTraceMapReceive(int JobWorkerId, int ProcessId, string term, int Limiter)//Product Id
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var tem = (from p in db.ViewJobOrderBalanceFromStatus
                       join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                       from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                       join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                       from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                       join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                       from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                       join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                       from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                       join t4 in db.JobOrderHeader on p.JobOrderHeaderId equals t4.JobOrderHeaderId
                       join t3 in db.Product on p.ProductId equals t3.ProductId
                       where p.BalanceQty > 0 && p.JobWorkerId == JobWorkerId && t4.ProcessId == ProcessId
                       && p.DivisionId == DivisionId && p.SiteId == SiteId
                       && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension3Tab.Dimension3Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension4Tab.Dimension4Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : t3.ProductName.ToLower().Contains(term.ToLower())))
                       orderby p.JobOrderNo
                       select new JobOrderHeaderListViewModel
                       {
                           DocNo = p.JobOrderNo,
                           JobOrderLineId = p.JobOrderLineId,
                           Dimension1Name = Dimension1Tab.Dimension1Name,
                           Dimension2Name = Dimension2Tab.Dimension2Name,
                           Dimension3Name = Dimension3Tab.Dimension3Name,
                           Dimension4Name = Dimension4Tab.Dimension4Name,
                           ProductName = t3.ProductName,
                           BalanceQty = p.BalanceQty,

                       }).Take(Limiter);

            return (tem);
        }

        public IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }


        public IEnumerable<ComboBoxList> GetJobWorkerHelpList(int Processid, string term)
        {


            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";


            var list = (from b in db.JobWorker
                        join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on b.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == Processid
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : PersonTab.Name.ToLower().Contains(term.ToLower()))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                        orderby PersonTab.Name
                        select new ComboBoxList
                        {
                            Id = b.PersonID,
                            PropFirst = PersonTab.Name
                            //PropSecond  = BusinessEntityTab.SiteIds.IndexOf(SiteId).ToString() 
                        }
              ).Take(20);

            return list.ToList();
        }


        public IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public string FGetJobOrderCostCenter(int DocTypeId, DateTime DocDate, int DivisionId, int SiteId)
        {
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", DocDate);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);

            NewDocNoViewModel NewDocNoViewModel = db.Database.SqlQuery<NewDocNoViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetJobOrderCostCenter @DocTypeId , @DocDate , @DivisionId , @SiteId ", SqlParameterDocTypeId, SqlParameterDocDate, SqlParameterDivisionId, SqlParameterSiteId).FirstOrDefault();

            if (NewDocNoViewModel != null)
            {
                return NewDocNoViewModel.NewDocNo;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<WeavingOrderWizardViewModel> GetProdOrdersForWeavingWizard(int DocTypeId)//DocTypeId
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] ProductCategories = null;
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { ProductCategories = settings.filterProductCategories.Split(",".ToCharArray()); }
            else { ProductCategories = new string[] { "NA" }; }

            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocumentTypeId", DocTypeId);

            IEnumerable<WeavingOrderWizardViewModel> PendingProdOrderList = db.Database.SqlQuery<WeavingOrderWizardViewModel>("Web.ProcWeavingOrderWizard @SiteId, @DivisionId, @DocumentTypeId", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDocTypeId).ToList();

            IEnumerable<WeavingOrderWizardViewModel> temp = (from p in PendingProdOrderList
                                                             where (string.IsNullOrEmpty(settings.filterProductCategories) ? 1 == 1 : ProductCategories.Contains(p.ProductCategoryId.ToString()))
                                                             orderby p.ProdOrderLineId
                                                             select p).ToList();




            //var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            //string[] ContraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { ContraSites = new string[] { "NA" }; }

            //string[] ContraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { ContraDivisions = new string[] { "NA" }; }

            //string[] ContraDocTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            //else { ContraDocTypes = new string[] { "NA" }; }

            //var temp = from p in db.ViewProdOrderBalance
            //           where (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == SiteId : ContraSites.Contains(p.SiteId.ToString()))
            //           && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId== DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
            //           && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(p.DocTypeId.ToString()))
            //           join t in db.FinishedProduct on p.ProductId equals t.ProductId into table
            //           from tab in table.DefaultIfEmpty()
            //           join t2 in db.ViewRugSize on p.ProductId equals t2.ProductId into table2
            //           from tab2 in table2.DefaultIfEmpty()
            //           join t3 in db.ProdOrderHeader on p.ProdOrderHeaderId equals t3.ProdOrderHeaderId into table3
            //           from tab3 in table3.DefaultIfEmpty()
            //           select new WeavingOrderWizardViewModel
            //           {
            //               Area = tab2.ManufaturingSizeArea * p.BalanceQty,
            //               BalanceQty = p.BalanceQty,
            //               BuyerId = tab3.BuyerId,
            //               BuyerName = tab3.Buyer.Person.Name,
            //               DesignName = tab.ProductGroup.ProductGroupName,
            //               Date = p.IndentDate.ToString(),
            //               DocNo = p.ProdOrderNo,
            //               DueDate = tab3.DueDate.ToString(),
            //               Qty = p.BalanceQty,
            //               Quality = tab.ProductQuality.ProductQualityName,
            //               Size = tab2.ManufaturingSizeName,
            //               ProdOrderLineId=p.ProdOrderLineId,
            //               RefDocLineId=p.ReferenceDocLineId,
            //               RefDocTypeId=p.ReferenceDocTypeId,
            //               DesignPatternName=tab.ProductDesign.ProductDesignName,
            //           };
            return temp;

        }

        public DateTime AddDueDate(DateTime Base, int DueDays)
        {
            DateTime DueDate = Base.AddDays((double)DueDays);
            if (DueDate.DayOfWeek == DayOfWeek.Sunday)
                DueDate = DueDate.AddDays(1);

            return DueDate;
        }

        public void UpdateProdUidJobWorkers(ref ApplicationDbContext context, JobOrderHeader Header)
        {
            var Lines = (from p in context.JobOrderLine
                         where p.JobOrderHeaderId == Header.JobOrderHeaderId
                         select p).ToList();

            if (Lines.Count() > 0)
            {
                if (Lines.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    var ProductUids = Lines.Select(m => m.ProductUidId).ToArray();

                    var ProductUidRecords = (from p in context.ProductUid
                                             where ProductUids.Contains(p.ProductUIDId)
                                             && p.LastTransactionDocId == Header.JobOrderHeaderId
                                             select p).ToList();

                    foreach (var item in ProductUidRecords)
                    {
                        item.LastTransactionPersonId = Header.JobWorkerId;
                        item.LastTransactionDocNo = Header.DocNo;
                        item.LastTransactionDocDate = Header.DocDate;
                        item.ObjectState = Model.ObjectState.Modified;

                        context.ProductUid.Add(item);
                    }

                }
            }

        }

        public string ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);
            var LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(JobWorkerId).LedgerAccountId;

            string ValidationMsg = "";

            if (Settings.PersonWiseCostCenter == true)
            {
                var CostCenter = (db.CostCenter.AsNoTracking().Where(m => m.CostCenterName == CostCenterName
                    && m.ReferenceDocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault());
                if (CostCenter != null)
                    if (CostCenter.LedgerAccountId != LedgerAccountId)
                        ValidationMsg += "CostCenter belongs to a different person. ";
            }

            if (Settings.isUniqueCostCenter == true)
            {
                var CostCenter = db.CostCenter.AsNoTracking().Where(m => m.CostCenterName == CostCenterName
                    && m.ReferenceDocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();
                if (CostCenter != null)
                {
                    var UniqueCostCenter = (from p in db.JobOrderHeader
                                            where p.CostCenterId == CostCenter.CostCenterId && p.JobOrderHeaderId != HeaderId && p.DocTypeId == DocTypeId
                                            && p.SiteId == SiteId && p.DivisionId == DivisionId
                                            select p
                                         ).FirstOrDefault();
                    if (UniqueCostCenter != null)
                        ValidationMsg += "CostCenter Already exists";
                }
            }

            return ValidationMsg;

        }

        public JobOrderLineProgressViewModel GetLineProgressDetail(int JobOrderLineId)
        {
            JobOrderLineProgressViewModel pd = new JobOrderLineProgressViewModel();

            var RecProgress = (from p in db.JobReceiveLine
                               join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                               join l in db.JobOrderLine on p.JobOrderLineId equals l.JobOrderLineId
                               join prod in db.Product on l.ProductId equals prod.ProductId
                               join u in db.Units on prod.UnitId equals u.UnitId
                               where p.JobOrderLineId == JobOrderLineId
                               group new { t, prod, p, u } by new { p.JobReceiveHeaderId, p.JobOrderLineId } into g
                               select
                               new ProgressDetail
                               {
                                   DocDate = g.Max(m => m.t.DocDate),
                                   DocNo = g.Max(m => m.t.DocNo),
                                   Qty = g.Sum(m => m.p.Qty),
                                   SKU = g.Max(m => m.prod.ProductName),
                                   DocId = g.Max(m => m.t.JobReceiveHeaderId),
                                   DocTypeId = g.Max(m => m.t.DocTypeId),
                                   DecimalPlaces = g.Max(m => m.u.DecimalPlaces),
                               }).ToList();

            pd.JobReceievs = RecProgress;

            var CancelProgress = (from p in db.JobOrderCancelLine
                                  join t in db.JobOrderCancelHeader on p.JobOrderCancelHeaderId equals t.JobOrderCancelHeaderId
                                  join l in db.JobOrderLine on p.JobOrderLineId equals l.JobOrderLineId
                                  join prod in db.Product on l.ProductId equals prod.ProductId
                                  join u in db.Units on prod.UnitId equals u.UnitId
                                  where p.JobOrderLineId == JobOrderLineId
                                  group new { t, prod, p, u } by new { p.JobOrderCancelHeaderId, p.JobOrderLineId } into g
                                  select
                                  new ProgressDetail
                                  {
                                      DocDate = g.Max(m => m.t.DocDate),
                                      DocNo = g.Max(m => m.t.DocNo),
                                      Qty = g.Sum(m => m.p.Qty),
                                      SKU = g.Max(m => m.prod.ProductName),
                                      DocId = g.Max(m => m.t.JobOrderCancelHeaderId),
                                      DocTypeId = g.Max(m => m.t.DocTypeId),
                                      DecimalPlaces = g.Max(m => m.u.DecimalPlaces),
                                  }).ToList();

            pd.JobCancels = CancelProgress;

            var AmendmentProgress = (from p in db.JobOrderQtyAmendmentLine
                                     join t in db.JobOrderAmendmentHeader on p.JobOrderAmendmentHeaderId equals t.JobOrderAmendmentHeaderId
                                     join l in db.JobOrderLine on p.JobOrderLineId equals l.JobOrderLineId
                                     join prod in db.Product on l.ProductId equals prod.ProductId
                                     join u in db.Units on prod.UnitId equals u.UnitId
                                     where p.JobOrderLineId == JobOrderLineId
                                     group new { t, prod, p, u } by new { p.JobOrderAmendmentHeaderId, p.JobOrderLineId } into g
                                     select
                                     new ProgressDetail
                                     {
                                         DocDate = g.Max(m => m.t.DocDate),
                                         DocNo = g.Max(m => m.t.DocNo),
                                         Qty = g.Sum(m => m.p.Qty),
                                         SKU = g.Max(m => m.prod.ProductName),
                                         DocId = g.Max(m => m.t.JobOrderAmendmentHeaderId),
                                         DocTypeId = g.Max(m => m.t.DocTypeId),
                                         DecimalPlaces = g.Max(m => m.u.DecimalPlaces),
                                     }).ToList();

            pd.JobAmendment = AmendmentProgress;

            return pd;

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] PersonRoles = null;
            if (!string.IsNullOrEmpty(settings.filterPersonRoles)) { PersonRoles = settings.filterPersonRoles.Split(",".ToCharArray()); }
            else { PersonRoles = new string[] { "NA" }; }

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            var list = (from p in db.Persons
                        join bus in db.BusinessEntity on p.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.Name.ToLower().Contains(term.ToLower()) || p.Code.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterPersonRoles) ? 1 == 1 : PersonRoles.Contains(PersonRoleTab.RoleDocTypeId.ToString()))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivIdStr) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteIdStr) != -1
                        && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                        group new { p } by new { p.PersonID } into Result
                        orderby Result.Max(m => m.p.Name)
                        select new ComboBoxResult
                        {
                            id = Result.Key.PersonID.ToString(),
                            text = Result.Max(m => m.p.Name + ", " + m.p.Suffix + " [" + m.p.Code + "]"),
                        }
              );

            return list;
        }


        public IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id)
        {
            var Header = db.JobOrderHeader.Find(id);

            var temp = from Dta in db.DocumentTypeHeaderAttribute
                       join Ha in db.JobOrderHeaderAttributes on Dta.DocumentTypeHeaderAttributeId equals Ha.DocumentTypeHeaderAttributeId into HeaderAttributeTable
                       from HeaderAttributeTab in HeaderAttributeTable.Where(m => m.HeaderTableId == id).DefaultIfEmpty()
                       where (Dta.DocumentTypeId == Header.DocTypeId)
                       select new DocumentTypeHeaderAttributeViewModel
                       {
                           ListItem = Dta.ListItem,
                           DataType = Dta.DataType,
                           Value = HeaderAttributeTab.Value,
                           Name = Dta.Name,
                           DocumentTypeHeaderAttributeId = Dta.DocumentTypeHeaderAttributeId,
                       };

            return temp;
        }

        public void Dispose()
        {
        }
    }
}
