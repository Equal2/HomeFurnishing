using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.SqlServer;
using Infrastructure.IO;
using ProjLib.Constants;
using Models.BasicSetup.ViewModels;
using Models.Customize.Models;
using Models.Customize.ViewModels;
using Models.BasicSetup.Models;
using Models.Company.Models;
using Models.Customize.DataBaseViews;
using Components.Logging;
using Services.BasicSetup;
using AutoMapper;
using System.Xml.Linq;
using System.Data;
using ProjLib.DocumentConstants;
using DocumentPrint;

namespace Services.Customize
{
    public interface IJobOrderHeaderService : IDisposable
    {
        JobOrderHeader Create(JobOrderHeader s);
        JobOrderHeaderViewModel Create(JobOrderHeaderViewModel vmJobOrderHeader, string UserName);
        void Delete(int id);
        void Delete(JobOrderHeader s);
        void Delete(ReasonViewModel vm, string UserName);
        JobOrderHeaderViewModel GetJobOrderHeader(int id);
        JobOrderHeader Find(int id);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListByCostCenter(int CostCenterId);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderList(int DocumentTypeId, string Uname);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListPendingToSubmit(int DocumentTypeId, string Uname);
        IQueryable<JobOrderHeaderViewModel> GetJobOrderHeaderListPendingToReview(int DocumentTypeId, string Uname);
        void Update(JobOrderHeader s);
        void Update(JobOrderHeaderViewModel vmJobOrderHeader, string UserName);
        string GetMaxDocNo();
        IEnumerable<ComboBoxList> GetJobWorkerHelpList(int Processid, string term);//PurchaseOrderHeaderId
        string FGetJobOrderCostCenter(int DocTypeId, DateTime DocDate, int DivisionId, int SiteId);
        DateTime AddDueDate(DateTime Base, int DueDays);
        void UpdateProdUidJobWorkers(JobOrderHeader Header);
        string ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName);
        void Submit(int Id, string UserName, string GenGatePass, string UserRemark);
        void Review(int Id, string UserName, string UserRemark);
        void GenerateGatePass(int Id, string UserName, JobOrderHeader pd, string SqlProcGatePass);
        void DeleteGatePass(JobOrderHeader pd, string UserName);
        int NextPrevId(int DocId, int DocTypeId, string UserName, string PrevNextConstants);
        byte[] GetReport(string Ids, int DocTypeId, string UserName);

        #region Helper Methods
        IQueryable<UnitConversionFor> GetUnitConversionForList();
        void LogDetailInfo(JobOrderHeaderViewModel vm);
        _Menu GetMenu(int Id);
        _Menu GetMenu(string Name);
        _ReportHeader GetReportHeader(string MenuName);
        _ReportLine GetReportLine(string Name, int ReportHeaderId);
        bool CheckForDocNoExists(string docno, int DocTypeId);
        bool CheckForDocNoExists(string docno, int headerid, int DocTypeId);
        #endregion

    }
    public class JobOrderHeaderService : IJobOrderHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderHeader> _jobOrderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Unit> _unitRepository;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        private readonly IStockService _stockService;
        private readonly IStockProcessService _stockProcessService;

        private ActiivtyLogViewModel logVm = new ActiivtyLogViewModel();

        public JobOrderHeaderService(IUnitOfWork unit, IRepository<JobOrderHeader> joborderRepo,
            IStockService StockServ, IStockProcessService StockPRocServ,
            ILogger log, IModificationCheck modificationCheck, IRepository<Product> ProductRepo, IRepository<Unit> UnitRepo)
        {
            _unitOfWork = unit;
            _jobOrderRepository = joborderRepo;
            _stockProcessService = StockPRocServ;
            _stockService = StockServ;
            _logger = log;
            _modificationCheck = modificationCheck;
            _productRepository = ProductRepo;
            _unitRepository = UnitRepo;

            //Log Initialization
            logVm.SessionId = 0;
            logVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            logVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            logVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

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

            return (from p in _jobOrderRepository.Instance
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

            return (from p in _jobOrderRepository.Instance
                    join t in _unitOfWork.Repository<Person>().Instance on p.JobWorkerId equals t.PersonID
                    join dt in _unitOfWork.Repository<DocumentType>().Instance on p.DocTypeId equals dt.DocumentTypeId
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
                                         join prod in _productRepository.Instance on o.ProductId equals prod.ProductId
                                         join u in _unitRepository.Instance on prod.UnitId equals u.UnitId
                                         select u.DecimalPlaces).Max(),
                    });
        }

        public JobOrderHeaderViewModel GetJobOrderHeader(int id)
        {
            return (from p in _jobOrderRepository.Instance
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
                    }
                        ).FirstOrDefault();
        }


        public IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersWithPatternMatch(int JobWorkerId, string term, int Limiter)//Product Id
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            var tem = (from p in _unitOfWork.Repository<ViewJobOrderBalance>().Instance
                       join t in _unitOfWork.Repository<Dimension1>().Instance on p.Dimension1Id equals t.Dimension1Id into table
                       from dim1 in table.DefaultIfEmpty()
                       join t2 in _unitOfWork.Repository<Dimension2>().Instance on p.Dimension2Id equals t2.Dimension2Id into table2
                       from dim2 in table2.DefaultIfEmpty()
                       join t3 in _productRepository.Instance on p.ProductId equals t3.ProductId
                       where p.BalanceQty > 0 && p.JobWorkerId == JobWorkerId
                       && p.DivisionId == DivisionId && p.SiteId == SiteId
                       && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : dim1.Dimension1Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : dim2.Dimension2Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : t3.ProductName.ToLower().Contains(term.ToLower())))
                       orderby p.JobOrderNo
                       select new JobOrderHeaderListViewModel
                       {
                           DocNo = p.JobOrderNo,
                           JobOrderLineId = p.JobOrderLineId,
                           Dimension1Name = dim1.Dimension1Name,
                           Dimension2Name = dim2.Dimension2Name,
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


            var list = (from b in _unitOfWork.Repository<JobWorker>().Instance
                        join bus in _unitOfWork.Repository<BusinessEntity>().Instance on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        join pp in _unitOfWork.Repository<PersonProcess>().Instance on b.PersonID equals pp.PersonId into PersonProcessTable
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

            NewDocNoViewModel NewDocNoViewModel = _unitOfWork.SqlQuery<NewDocNoViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetJobOrderCostCenter @DocTypeId , @DocDate , @DivisionId , @SiteId ", SqlParameterDocTypeId, SqlParameterDocDate, SqlParameterDivisionId, SqlParameterSiteId).FirstOrDefault();

            if (NewDocNoViewModel != null)
            {
                return NewDocNoViewModel.NewDocNo;
            }
            else
            {
                return null;
            }
        }



        public DateTime AddDueDate(DateTime Base, int DueDays)
        {
            DateTime DueDate = Base.AddDays((double)DueDays);
            if (DueDate.DayOfWeek == DayOfWeek.Sunday)
                DueDate = DueDate.AddDays(1);

            return DueDate;
        }

        public void UpdateProdUidJobWorkers(JobOrderHeader Header)
        {
            var Lines = (from p in _unitOfWork.Repository<JobOrderLine>().Instance
                         where p.JobOrderHeaderId == Header.JobOrderHeaderId
                         select p).ToList();

            if (Lines.Count() > 0)
            {
                if (Lines.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    var ProductUids = Lines.Select(m => m.ProductUidId).ToArray();

                    var ProductUidRecords = (from p in _unitOfWork.Repository<ProductUid>().Instance
                                             where ProductUids.Contains(p.ProductUIDId)
                                             && p.LastTransactionDocId == Header.JobOrderHeaderId
                                             select p).ToList();

                    foreach (var item in ProductUidRecords)
                    {
                        item.LastTransactionPersonId = Header.JobWorkerId;
                        item.LastTransactionDocNo = Header.DocNo;
                        item.ObjectState = Model.ObjectState.Modified;

                        _unitOfWork.Repository<ProductUid>().Add(item);
                    }

                }
            }

        }

        public string ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);
            var LedgerAccountId = (from L in _unitOfWork.Repository<LedgerAccount>().Instance
                                   where L.PersonId == JobWorkerId
                                   select L).FirstOrDefault().LedgerAccountId;

            string ValidationMsg = "";

            if (Settings.PersonWiseCostCenter == true)
            {
                var CostCenter = (_unitOfWork.Repository<CostCenter>().Query().Get().Where(m => m.CostCenterName == CostCenterName).FirstOrDefault());
                if (CostCenter != null)
                    if (CostCenter.LedgerAccountId != LedgerAccountId)
                        ValidationMsg += "CostCenter belongs to a different person. ";
            }

            if (Settings.isUniqueCostCenter == true)
            {
                var CostCenter = _unitOfWork.Repository<CostCenter>().Query().Get().Where(m => m.CostCenterName == CostCenterName).FirstOrDefault();
                if (CostCenter != null)
                {
                    var UniqueCostCenter = (from p in _jobOrderRepository.Instance
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


        public JobOrderHeaderViewModel Create(JobOrderHeaderViewModel vmJobOrderHeader, string UserName)
        {
            bool CostCenterGenerated = false;
            JobOrderHeader s = Mapper.Map<JobOrderHeaderViewModel, JobOrderHeader>(vmJobOrderHeader);

            if (!string.IsNullOrEmpty(vmJobOrderHeader.CostCenterName))
            {
                var CostCenter = new CostCenterService(_unitOfWork).Find(vmJobOrderHeader.CostCenterName, vmJobOrderHeader.DivisionId, vmJobOrderHeader.SiteId, vmJobOrderHeader.DocTypeId);
                if (CostCenter != null)
                {
                    s.CostCenterId = CostCenter.CostCenterId;
                    if (s.CostCenterId.HasValue)
                    {
                        var costcen = new CostCenterService(_unitOfWork).Find(s.CostCenterId.Value);
                        costcen.ProcessId = vmJobOrderHeader.ProcessId;
                        costcen.ObjectState = Model.ObjectState.Modified;
                        new CostCenterService(_unitOfWork).Update(costcen);
                    }
                }
                else
                {
                    CostCenter Cs = new CostCenter();
                    Cs.CostCenterName = vmJobOrderHeader.CostCenterName;
                    Cs.DivisionId = vmJobOrderHeader.DivisionId;
                    Cs.SiteId = vmJobOrderHeader.SiteId;
                    Cs.DocTypeId = vmJobOrderHeader.DocTypeId;
                    Cs.ProcessId = vmJobOrderHeader.ProcessId;
                    Cs.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(vmJobOrderHeader.JobWorkerId).LedgerAccountId;
                    Cs.CreatedBy = UserName;
                    Cs.ModifiedBy = UserName;
                    Cs.CreatedDate = DateTime.Now;
                    Cs.ModifiedDate = DateTime.Now;
                    Cs.IsActive = true;
                    Cs.ReferenceDocNo = vmJobOrderHeader.DocNo;
                    Cs.ReferenceDocTypeId = vmJobOrderHeader.DocTypeId;
                    Cs.StartDate = vmJobOrderHeader.DocDate;
                    Cs.ParentCostCenterId = _unitOfWork.Repository<Process>().Find(vmJobOrderHeader.ProcessId).CostCenterId;
                    Cs.ObjectState = Model.ObjectState.Added;

                    new CostCenterService(_unitOfWork).Create(Cs);

                    s.CostCenterId = Cs.CostCenterId;

                    new CostCenterStatusService(_unitOfWork).CreateLineStatus(Cs.CostCenterId);
                    CostCenterGenerated = true;

                }

            }


            s.CreatedDate = DateTime.Now;
            s.ModifiedDate = DateTime.Now;
            s.ActualDueDate = s.DueDate;
            s.ActualDocDate = s.DocDate;
            s.CreatedBy = UserName;
            s.ModifiedBy = UserName;
            s.Status = (int)StatusConstants.Drafted;
            s.ObjectState = Model.ObjectState.Added;
            Create(s);

            JobOrderHeaderStatus Stat = new JobOrderHeaderStatus();
            Stat.JobOrderHeaderId = s.JobOrderHeaderId;
            Stat.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<JobOrderHeaderStatus>().Add(Stat);

            if (vmJobOrderHeader.PerkViewModel != null)
            {
                int perkpid = 0;
                foreach (PerkViewModel item in vmJobOrderHeader.PerkViewModel)
                {
                    JobOrderPerk perk = Mapper.Map<PerkViewModel, JobOrderPerk>(item);
                    perk.CreatedBy = UserName;
                    perk.CreatedDate = DateTime.Now;
                    perk.ModifiedBy = UserName;
                    perk.ModifiedDate = DateTime.Now;
                    perk.JobOrderHeaderId = s.JobOrderHeaderId;
                    perk.JobOrderPerkId = perkpid;
                    perk.ObjectState = Model.ObjectState.Added;
                    _unitOfWork.Repository<JobOrderPerk>().Add(perk);

                    perkpid++;
                }
            }

            _unitOfWork.Save();

            vmJobOrderHeader.JobOrderHeaderId = s.JobOrderHeaderId;

            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = s.DocTypeId,
                DocId = s.JobOrderHeaderId,
                ActivityType = (int)ActivityTypeContants.Added,
                DocNo = s.DocNo,
                DocDate = s.DocDate,
                DocStatus = s.Status,
            }));

            //Update DocId in COstCenter
            if (s.CostCenterId.HasValue && CostCenterGenerated)
            {
                var CC = new CostCenterService(_unitOfWork).Find(s.CostCenterId.Value);
                CC.ReferenceDocId = s.JobOrderHeaderId;
                CC.ObjectState = Model.ObjectState.Modified;
                new CostCenterService(_unitOfWork).Update(CC);
                _unitOfWork.Save();
            }

            return vmJobOrderHeader;
        }


        public void Update(JobOrderHeaderViewModel vmJobOrderHeader, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            JobOrderHeader temp = Find(vmJobOrderHeader.JobOrderHeaderId);

            JobOrderHeader ExRec = Mapper.Map<JobOrderHeader>(temp);

            int status = temp.Status;

            if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                temp.Status = (int)StatusConstants.Modified;


            if (!string.IsNullOrEmpty(vmJobOrderHeader.CostCenterName))
            {
                var CostCenter = new CostCenterService(_unitOfWork).Find(vmJobOrderHeader.CostCenterName, vmJobOrderHeader.DivisionId, vmJobOrderHeader.SiteId, vmJobOrderHeader.DocTypeId);
                if (CostCenter != null)
                {
                    temp.CostCenterId = CostCenter.CostCenterId;
                    if (temp.CostCenterId.HasValue)
                    {
                        var costcen = new CostCenterService(_unitOfWork).Find(temp.CostCenterId.Value);

                        costcen.ProcessId = vmJobOrderHeader.ProcessId;
                        costcen.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(vmJobOrderHeader.JobWorkerId).LedgerAccountId;

                        costcen.ObjectState = Model.ObjectState.Modified;
                        new CostCenterService(_unitOfWork).Update(costcen);
                    }
                }
                else
                {

                    var ExistingCostCenter = new CostCenterService(_unitOfWork).Find(temp.CostCenterId.Value);

                    ExistingCostCenter.CostCenterName = vmJobOrderHeader.CostCenterName;
                    ExistingCostCenter.ObjectState = Model.ObjectState.Modified;

                    new CostCenterService(_unitOfWork).Update(ExistingCostCenter);
                }

            }




            temp.DocDate = vmJobOrderHeader.DocDate;
            temp.DueDate = vmJobOrderHeader.DueDate;
            temp.UnitConversionForId = vmJobOrderHeader.UnitConversionForId;
            temp.ProcessId = vmJobOrderHeader.ProcessId;
            temp.JobWorkerId = vmJobOrderHeader.JobWorkerId;
            temp.MachineId = vmJobOrderHeader.MachineId;
            temp.BillToPartyId = vmJobOrderHeader.BillToPartyId;
            temp.OrderById = vmJobOrderHeader.OrderById;
            temp.ActualDueDate = vmJobOrderHeader.DueDate;
            temp.GodownId = vmJobOrderHeader.GodownId;
            temp.TermsAndConditions = vmJobOrderHeader.TermsAndConditions;
            temp.DocNo = vmJobOrderHeader.DocNo;
            temp.Remark = vmJobOrderHeader.Remark;
            temp.CreditDays = vmJobOrderHeader.CreditDays;
            temp.ModifiedDate = DateTime.Now;
            temp.ModifiedBy = UserName;
            temp.ObjectState = Model.ObjectState.Modified;
            Update(temp);

            if (temp.JobWorkerId != ExRec.JobWorkerId || temp.DocNo != ExRec.DocNo)
            {
                UpdateProdUidJobWorkers(temp);
            }

            if (vmJobOrderHeader.PerkViewModel != null)
            {
                foreach (PerkViewModel item in vmJobOrderHeader.PerkViewModel)
                {

                    if (item.JobOrderPerkId > 0)
                    {
                        JobOrderPerk perk = _unitOfWork.Repository<JobOrderPerk>().Find(item.JobOrderPerkId);

                        perk.Worth = item.Worth;
                        perk.Base = item.Base;
                        perk.ModifiedBy = UserName;
                        perk.ModifiedDate = DateTime.Now;
                        perk.ObjectState = Model.ObjectState.Modified;
                        _unitOfWork.Repository<JobOrderPerk>().Update(perk);
                    }
                    else
                    {
                        JobOrderPerk perkC = Mapper.Map<PerkViewModel, JobOrderPerk>(item);
                        perkC.CreatedBy = UserName;
                        perkC.CreatedDate = DateTime.Now;
                        perkC.ModifiedBy = UserName;
                        perkC.ModifiedDate = DateTime.Now;
                        perkC.JobOrderHeaderId = temp.JobOrderHeaderId;
                        perkC.ObjectState = Model.ObjectState.Added;
                        _unitOfWork.Repository<JobOrderPerk>().Add(perkC);
                    }
                }
            }


            if (temp.StockHeaderId != null)
            {
                StockHeader S = _unitOfWork.Repository<StockHeader>().Find(temp.StockHeaderId);

                //Updating docdate in stock and stockprocess
                #region Updating DocDate in Stock & StockProcess
                if (S.DocDate != temp.DocDate)
                {
                    List<Stock> StockLines = (_unitOfWork.Repository<Stock>().Query().Get().Where(m => m.StockHeaderId == S.StockHeaderId)).ToList();

                    foreach (var item in StockLines)
                    {
                        item.DocDate = temp.DocDate;
                        item.ObjectState = Model.ObjectState.Modified;
                        _unitOfWork.Repository<Stock>().Update(item);
                    }

                    List<StockProcess> StockProcLines = (_unitOfWork.Repository<StockProcess>().Query().Get().Where(m => m.StockHeaderId == temp.StockHeaderId)).ToList();
                    foreach (var item in StockProcLines)
                    {
                        item.DocDate = temp.DocDate;
                        item.ObjectState = Model.ObjectState.Modified;
                        _unitOfWork.Repository<StockProcess>().Update(item);
                    }

                }
                #endregion

                S.DocDate = temp.DocDate;
                S.DocNo = temp.DocNo;
                S.PersonId = temp.JobWorkerId;
                S.GodownId = temp.GodownId;
                S.Remark = temp.Remark;
                S.Status = temp.Status;
                S.ModifiedBy = temp.ModifiedBy;
                S.ModifiedDate = temp.ModifiedDate;
                S.ObjectState = Model.ObjectState.Modified;
                _unitOfWork.Repository<StockHeader>().Update(S);
            }

            LogList.Add(new LogTypeViewModel
            {
                ExObj = ExRec,
                Obj = temp,
            });

            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = temp.DocTypeId,
                DocId = temp.JobOrderHeaderId,
                ActivityType = (int)ActivityTypeContants.Modified,
                DocNo = temp.DocNo,
                xEModifications = Modifications,
                DocDate = temp.DocDate,
                DocStatus = temp.Status,
            }));

        }

        public void Delete(ReasonViewModel vm, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var JobOrderHeader = Find(vm.id);
            GatePassHeader GatePassHEader = new GatePassHeader();

            if (JobOrderHeader.GatePassHeaderId.HasValue)
            {
                GatePassHEader = _unitOfWork.Repository<GatePassHeader>().Find(JobOrderHeader.GatePassHeaderId.Value);

                if (GatePassHEader != null && GatePassHEader.Status == (int)StatusConstants.Submitted)
                {
                    throw new Exception("Cannot delete record because gatepass is submitted.");
                }
            }
            int? StockHeaderId = 0;

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Mapper.Map<JobOrderHeader>(JobOrderHeader),
            });

            StockHeaderId = JobOrderHeader.StockHeaderId;

            //Then find all the Purchase Order Header Line associated with the above ProductType.
            //var JobOrderLine = new JobOrderLineService(_unitOfWork).GetJobOrderLineforDelete(vm.id);
            var JobOrderLine = (_unitOfWork.Repository<JobOrderLine>().Query().Get().Where(m => m.JobOrderHeaderId == vm.id)).ToList();

            var JOLineIds = JobOrderLine.Select(m => m.JobOrderLineId).ToArray();

            var JobOrderLineStatusRecords = _unitOfWork.Repository<JobOrderLineStatus>().Query().Get().Where(m => JOLineIds.Contains(m.JobOrderLineId ?? 0)).ToList();

            var JobOrderLineExtendedRecords = _unitOfWork.Repository<JobOrderLineExtended>().Query().Get().Where(m => JOLineIds.Contains(m.JobOrderLineId)).ToList();

            var LineChargeRecords = _unitOfWork.Repository<JobOrderLineCharge>().Query().Get().Where(m => JOLineIds.Contains(m.LineTableId)).ToList();

            var HeaderChargeRecords = _unitOfWork.Repository<JobOrderHeaderCharge>().Query().Get().Where(m => m.HeaderTableId == vm.id).ToList();

            var CreatedBarCodeRecords = _unitOfWork.Repository<ProductUid>().Query().Get().Where(m => JOLineIds.Contains(m.GenLineId ?? 0) && m.GenDocTypeId == JobOrderHeader.DocTypeId).ToList();

            var ProductUids = JobOrderLine.Select(m => m.ProductUidId).ToArray();

            var BarCodeRecords = _unitOfWork.Repository<ProductUid>().Query().Get().Where(m => ProductUids.Contains(m.ProductUIDId)).ToList();

            var JobOrderBomRecords = _unitOfWork.Repository<JobOrderBom>().Query().Get().Where(m => JOLineIds.Contains(m.JobOrderLineId ?? 0)).ToList();

            var JobOrderHeaderBomRecords = _unitOfWork.Repository<JobOrderBom>().Query().Get().Where(m => m.JobOrderHeaderId == vm.id && m.JobOrderLineId == null).ToList();

            var JobOrderPerks = _unitOfWork.Repository<JobOrderPerk>().Query().Get().Where(m => m.JobOrderHeaderId == vm.id).ToList();


            List<int> StockIdList = new List<int>();
            List<int> StockProcessIdList = new List<int>();

            DeleteProdQtyOnJobOrderMultiple(JobOrderHeader.JobOrderHeaderId);

            foreach (var item in LineChargeRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderLineCharge>().Delete(item);
            }

            foreach (var item in JobOrderLineStatusRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderLineStatus>().Delete(item);
            }

            foreach (var item in JobOrderLineExtendedRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderLineExtended>().Delete(item);
            }


            //Mark ObjectState.Delete to all the Purchase Order Lines. 
            foreach (var item in JobOrderLine)
            {

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobOrderLine>(item),
                });


                if (item.StockId != null)
                {
                    StockIdList.Add((int)item.StockId);
                }

                if (item.StockProcessId != null)
                {
                    StockProcessIdList.Add((int)item.StockProcessId);
                }

                //var linecharges = new JobOrderLineChargeService(_unitOfWork).GetCalculationProductList(item.JobOrderLineId);
                //foreach (var citem in linecharges)
                //    new JobOrderLineChargeService(_unitOfWork).Delete(citem.Id);



                if ((item.ProductUidHeaderId.HasValue) && (item.ProductUidHeaderId.Value > 0))
                {

                    var ProductUid = CreatedBarCodeRecords.Where(m => m.GenLineId == item.JobOrderLineId);

                    foreach (var item2 in ProductUid)
                    {
                        if (item2.LastTransactionDocId == null || (item2.LastTransactionDocId == item2.GenDocId && item2.LastTransactionDocTypeId == item2.GenDocTypeId))
                        //new ProductUidService(_unitOfWork).Delete(item2);
                        {
                            item2.ObjectState = Model.ObjectState.Deleted;
                            _unitOfWork.Repository<ProductUid>().Delete(item2);
                        }
                        else
                        {
                            throw new Exception("Record Cannot be deleted as its Unique Id's are in use by other documents");
                        }
                    }
                }
                else
                {

                    if (item.ProductUidId.HasValue)
                    {
                        ProductUid ProductUid = BarCodeRecords.Where(m => m.ProductUIDId == item.ProductUidId).FirstOrDefault();

                        ProductUid.LastTransactionDocDate = item.ProductUidLastTransactionDocDate;
                        ProductUid.LastTransactionDocId = item.ProductUidLastTransactionDocId;
                        ProductUid.LastTransactionDocNo = item.ProductUidLastTransactionDocNo;
                        ProductUid.LastTransactionDocTypeId = item.ProductUidLastTransactionDocTypeId;
                        ProductUid.LastTransactionPersonId = item.ProductUidLastTransactionPersonId;
                        ProductUid.CurrenctGodownId = item.ProductUidCurrentGodownId;
                        ProductUid.CurrenctProcessId = item.ProductUidCurrentProcessId;
                        ProductUid.Status = item.ProductUidStatus;
                        ProductUid.ObjectState = Model.ObjectState.Modified;
                        _unitOfWork.Repository<ProductUid>().Update(ProductUid);
                    }


                }

                var Boms = JobOrderBomRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId);

                foreach (var item2 in Boms)
                {
                    item2.ObjectState = Model.ObjectState.Deleted;
                    _unitOfWork.Repository<JobOrderBom>().Delete(item2);
                }


                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderLine>().Delete(item);

            }


            _stockService.DeleteStockMultiple(StockIdList);

            _stockProcessService.DeleteStockProcessDBMultiple(StockProcessIdList);


            foreach (var item in HeaderChargeRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderHeaderCharge>().Delete(item);
            }


            foreach (var item in JobOrderPerks)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderPerk>().Delete(item);
            }


            if (JobOrderHeader.GatePassHeaderId.HasValue)
            {

                var GatePassLines = _unitOfWork.Repository<GatePassLine>().Query().Get().Where(m => m.GatePassHeaderId == GatePassHEader.GatePassHeaderId).ToList();

                foreach (var item in GatePassLines)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    _unitOfWork.Repository<GatePassLine>().Delete(item);
                }

                GatePassHEader.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<GatePassHeader>().Delete(GatePassHEader);
            }


            foreach (var Hbom in JobOrderHeaderBomRecords)
            {
                Hbom.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderBom>().Delete(Hbom);
            }

            var JobORderHEaderStatus = _unitOfWork.Repository<JobOrderHeaderStatus>().Find(vm.id);

            JobORderHEaderStatus.ObjectState = Model.ObjectState.Deleted;
            _unitOfWork.Repository<JobOrderHeaderStatus>().Delete(JobORderHEaderStatus);

            // Now delete the Purhcase Order Header
            //_JobOrderHeaderService.Delete(JobOrderHeader);

            int ReferenceDocId = JobOrderHeader.JobOrderHeaderId;
            int ReferenceDocTypeId = JobOrderHeader.DocTypeId;


            JobOrderHeader.ObjectState = Model.ObjectState.Deleted;
            Delete(JobOrderHeader);

            //ForDeleting Generated CostCenter:::

            var GeneratedCostCenter = _unitOfWork.Repository<CostCenter>().Query().Get().Where(m => m.ReferenceDocId == ReferenceDocId && m.ReferenceDocTypeId == ReferenceDocTypeId).FirstOrDefault();

            if (GeneratedCostCenter != null)
            {
                var CostCentrerStatusRecord = _unitOfWork.Repository<CostCenterStatus>().Find(GeneratedCostCenter.CostCenterId);

                if (CostCentrerStatusRecord != null)
                {
                    CostCentrerStatusRecord.ObjectState = Model.ObjectState.Deleted;
                    _unitOfWork.Repository<CostCenterStatus>().Delete(CostCentrerStatusRecord);
                }
                GeneratedCostCenter.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<CostCenter>().Delete(GeneratedCostCenter);
            }

            if (StockHeaderId != null)
            {
                var StockHeader = _unitOfWork.Repository<StockHeader>().Find(StockHeaderId);

                StockHeader.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<StockHeader>().Delete(StockHeader);
            }

            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();


            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = JobOrderHeader.DocTypeId,
                    DocId = JobOrderHeader.JobOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = JobOrderHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = JobOrderHeader.DocDate,
                    DocStatus = JobOrderHeader.Status,
                }));

        }


        public void Submit(int Id, string UserName, string GenGatePass, string UserRemark)
        {
            int Cnt = 0;
            int CountUid = 0;
            var pd = _jobOrderRepository.Find(Id);

            pd.Status = (int)StatusConstants.Submitted;
            int ActivityType = (int)ActivityTypeContants.Submitted;

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);


            if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
            {

                if (!String.IsNullOrEmpty(Settings.SqlProcGatePass))
                {

                    SqlParameter SqlParameterUserId = new SqlParameter("@Id", Id);
                    IEnumerable<GatePassGeneratedViewModel> GatePasses = _unitOfWork.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                    if (pd.GatePassHeaderId == null)
                    {
                        SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                        DocDate.SqlDbType = SqlDbType.DateTime;
                        SqlParameter Godown = new SqlParameter("@GodownId", pd.GodownId);
                        SqlParameter DocType = new SqlParameter("@DocTypeId", new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.GatePass).DocumentTypeId);
                        GatePassHeader GPHeader = new GatePassHeader();
                        GPHeader.CreatedBy = UserName;
                        GPHeader.CreatedDate = DateTime.Now;
                        GPHeader.DivisionId = pd.DivisionId;
                        GPHeader.DocDate = DateTime.Now.Date;
                        GPHeader.DocNo = _unitOfWork.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                        GPHeader.DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.GatePass).DocumentTypeId;
                        GPHeader.ModifiedBy = UserName;
                        GPHeader.ModifiedDate = DateTime.Now;
                        GPHeader.Remark = pd.Remark;
                        GPHeader.PersonId = pd.JobWorkerId;
                        GPHeader.SiteId = pd.SiteId;
                        GPHeader.GodownId = pd.GodownId ?? 0;

                        GPHeader.ObjectState = Model.ObjectState.Added;
                        _unitOfWork.Repository<GatePassHeader>().Add(GPHeader);

                        foreach (GatePassGeneratedViewModel item in GatePasses)
                        {
                            GatePassLine Gline = new GatePassLine();
                            Gline.CreatedBy = UserName;
                            Gline.CreatedDate = DateTime.Now;
                            Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                            Gline.ModifiedBy = UserName;
                            Gline.ModifiedDate = DateTime.Now;
                            Gline.Product = item.ProductName;
                            Gline.Qty = item.Qty;
                            Gline.Specification = item.Specification;
                            Gline.UnitId = item.UnitId;

                            // new GatePassLineService(_unitOfWork).Create(Gline);
                            Gline.ObjectState = Model.ObjectState.Added;
                            _unitOfWork.Repository<GatePassLine>().Add(Gline);
                        }

                        pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                    }
                    else
                    {
                        //List<GatePassLine> LineList = new GatePassLineService(_unitOfWork).GetGatePassLineList(pd.GatePassHeaderId ?? 0).ToList();

                        List<GatePassLine> LineList = _unitOfWork.Repository<GatePassLine>().Query().Get().Where(m => m.GatePassHeaderId == pd.GatePassHeaderId).ToList();

                        foreach (var ittem in LineList)
                        {

                            ittem.ObjectState = Model.ObjectState.Deleted;
                            _unitOfWork.Repository<GatePassLine>().Delete(ittem);
                        }

                        GatePassHeader GPHeader = _unitOfWork.Repository<GatePassHeader>().Find(pd.GatePassHeaderId ?? 0);

                        foreach (GatePassGeneratedViewModel item in GatePasses)
                        {
                            GatePassLine Gline = new GatePassLine();
                            Gline.CreatedBy = UserName;
                            Gline.CreatedDate = DateTime.Now;
                            Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                            Gline.ModifiedBy = UserName;
                            Gline.ModifiedDate = DateTime.Now;
                            Gline.Product = item.ProductName;
                            Gline.Qty = item.Qty;
                            Gline.Specification = item.Specification;
                            Gline.UnitId = item.UnitId;

                            Gline.ObjectState = Model.ObjectState.Added;
                            _unitOfWork.Repository<GatePassLine>().Add(Gline);
                        }

                        pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                    }
                }
            }

            List<string> uids = new List<string>();

            if (!string.IsNullOrEmpty(Settings.SqlProcGenProductUID))
            {

                var lines = _unitOfWork.Repository<JobOrderLine>().Query().Get().Where(m => m.JobOrderHeaderId == pd.JobOrderHeaderId);

                decimal Qty = lines.Where(m => m.ProductUidHeaderId == null).Sum(m => m.Qty);

                using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                {
                    sqlConnection.Open();

                    int TypeId = pd.DocTypeId;

                    SqlCommand Totalf = new SqlCommand("SELECT * FROM " + Settings.SqlProcGenProductUID + "( " + TypeId + ", " + Qty + ")", sqlConnection);

                    SqlDataReader ExcessStockQty = (Totalf.ExecuteReader());
                    while (ExcessStockQty.Read())
                    {
                        uids.Add((string)ExcessStockQty.GetValue(0));
                    }
                }

                //uids = new JobOrderLineService(_unitOfWork).GetProcGenProductUids(pd.DocTypeId, Qty, pd.DivisionId, pd.SiteId);

                foreach (var item in lines.Where(m => m.ProductUidHeaderId == null))
                {
                    if (uids.Count > 0)
                    {
                        ProductUidHeader ProdUidHeader = new ProductUidHeader();

                        ProdUidHeader.ProductUidHeaderId = Cnt;
                        ProdUidHeader.ProductId = item.ProductId;
                        ProdUidHeader.Dimension1Id = item.Dimension1Id;
                        ProdUidHeader.Dimension2Id = item.Dimension2Id;
                        ProdUidHeader.GenDocId = pd.JobOrderHeaderId;
                        ProdUidHeader.GenDocNo = pd.DocNo;
                        ProdUidHeader.GenDocTypeId = pd.DocTypeId;
                        ProdUidHeader.GenDocDate = pd.DocDate;
                        ProdUidHeader.GenPersonId = pd.JobWorkerId;
                        ProdUidHeader.CreatedBy = UserName;
                        ProdUidHeader.CreatedDate = DateTime.Now;
                        ProdUidHeader.ModifiedBy = UserName;
                        ProdUidHeader.ModifiedDate = DateTime.Now;
                        ProdUidHeader.ObjectState = Model.ObjectState.Added;

                        _unitOfWork.Repository<ProductUidHeader>().Add(ProdUidHeader);

                        item.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;


                        int count = 0;
                        //foreach (string UidItem in uids)
                        for (int A = 0; A < item.Qty; A++)
                        {
                            ProductUid ProdUid = new ProductUid();

                            ProdUid.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                            ProdUid.ProductUidName = uids[CountUid];
                            ProdUid.ProductId = item.ProductId;
                            ProdUid.IsActive = true;
                            ProdUid.CreatedBy = UserName;
                            ProdUid.CreatedDate = DateTime.Now;
                            ProdUid.ModifiedBy = UserName;
                            ProdUid.ModifiedDate = DateTime.Now;
                            ProdUid.GenLineId = item.JobOrderLineId;
                            ProdUid.GenDocId = pd.JobOrderHeaderId;
                            ProdUid.GenDocNo = pd.DocNo;
                            ProdUid.GenDocTypeId = pd.DocTypeId;
                            ProdUid.GenDocDate = pd.DocDate;
                            ProdUid.GenPersonId = pd.JobWorkerId;
                            ProdUid.Dimension1Id = item.Dimension1Id;
                            ProdUid.Dimension2Id = item.Dimension2Id;
                            ProdUid.CurrenctProcessId = pd.ProcessId;
                            ProdUid.Status = (!string.IsNullOrEmpty(Settings.BarcodeStatusUpdate) ? Settings.BarcodeStatusUpdate : ProductUidStatusConstants.Issue);
                            ProdUid.LastTransactionDocId = pd.JobOrderHeaderId;
                            ProdUid.LastTransactionDocNo = pd.DocNo;
                            ProdUid.LastTransactionDocTypeId = pd.DocTypeId;
                            ProdUid.LastTransactionDocDate = pd.DocDate;
                            ProdUid.LastTransactionPersonId = pd.JobWorkerId;
                            ProdUid.LastTransactionLineId = item.JobOrderLineId;
                            ProdUid.ProductUIDId = count;
                            ProdUid.ObjectState = Model.ObjectState.Added;
                            _unitOfWork.Repository<ProductUid>().Add(ProdUid);

                            count++;
                            CountUid++;
                        }
                        Cnt++;
                        item.ObjectState = Model.ObjectState.Modified;
                        _unitOfWork.Repository<JobOrderLine>().Update(item);
                    }
                }
            }

            pd.ReviewBy = null;
            pd.ObjectState = Model.ObjectState.Modified;
            _jobOrderRepository.Update(pd);


            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = pd.DocTypeId,
                DocId = pd.JobOrderHeaderId,
                ActivityType = ActivityType,
                UserRemark = UserRemark,
                DocNo = pd.DocNo,
                DocDate = pd.DocDate,
                DocStatus = pd.Status,
            }));


        }

        public void Review(int Id, string UserName, string UserRemark)
        {
            var pd = Find(Id);

            pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
            pd.ReviewBy += UserName + ", ";
            pd.ObjectState = Model.ObjectState.Modified;

            Update(pd);

            _unitOfWork.Save();

            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = pd.DocTypeId,
                DocId = pd.JobOrderHeaderId,
                ActivityType = (int)ActivityTypeContants.Reviewed,
                UserRemark = UserRemark,
                DocNo = pd.DocNo,
                DocDate = pd.DocDate,
                DocStatus = pd.Status,
            }));

        }

        public void GenerateGatePass(int Id, string UserName, JobOrderHeader pd, string SqlProcGatePass)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int PK = 0;

            var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
            string JobHeaderIds = "";

            SqlParameter SqlParameterUserId = new SqlParameter("@Id", Id);
            IEnumerable<GatePassGeneratedViewModel> GatePasses = _unitOfWork.SqlQuery<GatePassGeneratedViewModel>(SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

            if (pd.GatePassHeaderId == null)
            {
                SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                DocDate.SqlDbType = SqlDbType.DateTime;
                SqlParameter Godown = new SqlParameter("@GodownId", pd.GodownId);
                SqlParameter DocType = new SqlParameter("@DocTypeId", GatePassDocTypeID);
                GatePassHeader GPHeader = new GatePassHeader();
                GPHeader.CreatedBy = UserName;
                GPHeader.CreatedDate = DateTime.Now;
                GPHeader.DivisionId = pd.DivisionId;
                GPHeader.DocDate = DateTime.Now.Date;
                GPHeader.DocNo = _unitOfWork.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                GPHeader.DocTypeId = GatePassDocTypeID;
                GPHeader.ModifiedBy = UserName;
                GPHeader.ModifiedDate = DateTime.Now;
                GPHeader.Remark = pd.Remark;
                GPHeader.PersonId = pd.JobWorkerId;
                GPHeader.SiteId = pd.SiteId;
                GPHeader.GodownId = pd.GodownId ?? 0;
                GPHeader.GatePassHeaderId = PK++;
                GPHeader.ObjectState = Model.ObjectState.Added;

                _unitOfWork.Repository<GatePassHeader>().Add(GPHeader);

                foreach (GatePassGeneratedViewModel GatepassLine in GatePasses)
                {
                    GatePassLine Gline = new GatePassLine();
                    Gline.CreatedBy = UserName;
                    Gline.CreatedDate = DateTime.Now;
                    Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                    Gline.ModifiedBy = UserName;
                    Gline.ModifiedDate = DateTime.Now;
                    Gline.Product = GatepassLine.ProductName;
                    Gline.Qty = GatepassLine.Qty;
                    Gline.Specification = GatepassLine.Specification;
                    Gline.UnitId = GatepassLine.UnitId;

                    // new GatePassLineService(_unitOfWork).Create(Gline);
                    Gline.ObjectState = Model.ObjectState.Added;
                    _unitOfWork.Repository<GatePassLine>().Add(Gline);
                }

                pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                pd.ObjectState = Model.ObjectState.Modified;
                Update(pd);
            }

            _unitOfWork.Save();

            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = GatePassDocTypeID,
                ActivityType = (int)ActivityTypeContants.Added,
                Narration = "GatePass created for Job Order " + pd.JobOrderHeaderId,
            }));
        }


        public void DeleteGatePass(JobOrderHeader pd, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var GatePass = _unitOfWork.Repository<GatePassHeader>().Find(pd.GatePassHeaderId);

            if (GatePass.Status != (int)StatusConstants.Submitted)
            {
                pd.GatePassHeaderId = null;
                pd.Status = (int)StatusConstants.Modified;
                pd.ModifiedBy = UserName;
                pd.ModifiedDate = DateTime.Now;
                pd.ObjectState = Model.ObjectState.Modified;
                Update(pd);

                GatePass.Status = (int)StatusConstants.Cancel;
                GatePass.ObjectState = Model.ObjectState.Modified;
                _unitOfWork.Repository<GatePassHeader>().Update(GatePass);

                XElement Modifications = _modificationCheck.CheckChanges(LogList);

                _unitOfWork.Save();

                _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = GatePass.DocTypeId,
                    DocId = GatePass.GatePassHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = GatePass.DocNo,
                    DocDate = GatePass.DocDate,
                    xEModifications = Modifications,
                    DocStatus = GatePass.Status,
                }));

            }
            else
                throw new Exception("Gatepass cannot be deleted because it is already submitted");

        }

        public int NextPrevId(int DocId, int DocTypeId, string UserName, string PrevNext)
        {
            return new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, UserName, "", "Web.JobOrderHeaders", "JobOrderHeaderId", PrevNext);
        }

        public byte[] GetReport(string Ids, int DocTypeId, string UserName)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string ReportSql = "";

            if (!string.IsNullOrEmpty(Settings.DocumentPrint))
                ReportSql = GetReportHeader(Settings.DocumentPrint).ReportSQL;


            List<byte[]> PdfStream = new List<byte[]>();
            foreach (var item in Ids.Split(',').Select(Int32.Parse))
            {

                DirectReportPrint drp = new DirectReportPrint();

                var pd = Find(item);

                byte[] Pdf;

                if (!string.IsNullOrEmpty(ReportSql))
                {
                    Pdf = drp.rsDirectDocumentPrint(ReportSql, UserName, item);
                    PdfStream.Add(Pdf);
                }
                else
                {
                    if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
                    {
                        Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, UserName, item);

                        PdfStream.Add(Pdf);
                    }
                    else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                    {
                        Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, UserName, item);

                        PdfStream.Add(Pdf);
                    }
                    else
                    {
                        Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, UserName, item);
                        PdfStream.Add(Pdf);
                    }
                }

                _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.JobOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Print,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

            }

            PdfMerger pm = new PdfMerger();

            byte[] Merge = pm.MergeFiles(PdfStream);

            return Merge;
        }



        #region Helper Methods

        public IQueryable<UnitConversionFor> GetUnitConversionForList()
        {
            return _unitOfWork.Repository<UnitConversionFor>().Query().Get();
        }

        public void LogDetailInfo(JobOrderHeaderViewModel vm)
        {
            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = vm.DocTypeId,
                DocId = vm.JobOrderHeaderId,
                ActivityType = (int)ActivityTypeContants.Detail,
                DocNo = vm.DocNo,
                DocDate = vm.DocDate,
                DocStatus = vm.Status,
            }));
        }

        public _Menu GetMenu(int Id)
        {
            return _unitOfWork.Repository<_Menu>().Find(Id);
        }

        public _Menu GetMenu(string Name)
        {
            return _unitOfWork.Repository<_Menu>().Query().Get().Where(m => m.MenuName == Name).FirstOrDefault();
        }

        public _ReportHeader GetReportHeader(string MenuName)
        {
            return _unitOfWork.Repository<_ReportHeader>().Query().Get().Where(m => m.ReportName == MenuName).FirstOrDefault();
        }
        public _ReportLine GetReportLine(string Name, int ReportHeaderId)
        {
            return _unitOfWork.Repository<_ReportLine>().Query().Get().Where(m => m.ReportHeaderId == ReportHeaderId && m.FieldName == Name).FirstOrDefault();
        }

        public bool CheckForDocNoExists(string docno, int DocTypeId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _jobOrderRepository.Instance
                        where pr.DocNo == docno && (pr.DocTypeId == DocTypeId) && pr.SiteId == SiteId && pr.DivisionId == DivisionId
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;

        }
        public bool CheckForDocNoExists(string docno, int headerid, int DocTypeId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _jobOrderRepository.Instance
                        where pr.DocNo == docno && pr.JobOrderHeaderId != headerid && (pr.DocTypeId == DocTypeId) && pr.SiteId == SiteId && pr.DivisionId == DivisionId
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        #endregion


        #region ProdOrderLineStatusUpdation

        void DeleteProdQtyOnJobOrderMultiple(int id)
        {
            var LineAndQty = (from t in _unitOfWork.Repository<JobOrderLine>().Instance
                              where t.JobOrderHeaderId == id && t.ProdOrderLineId != null
                              group t by t.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();

            if (LineAndQty != null)
            {
                int[] IsdA2 = null;
                IsdA2 = LineAndQty.Select(m => m.LineId.Value).ToArray();

                var ProdOrderLineStatus = (from p in _unitOfWork.Repository<ProdOrderLineStatus>().Instance
                                           where IsdA2.Contains(p.ProdOrderLineId.Value)
                                           select p
                                            ).ToList();

                foreach (var item in ProdOrderLineStatus)
                {
                    item.JobOrderQty = item.JobOrderQty - (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                    item.ObjectState = Model.ObjectState.Modified;
                    _unitOfWork.Repository<ProdOrderLineStatus>().Update(item);
                }
            }
        }

     

        #endregion

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }

    public class NewDocNoViewModel
    {
        public string NewDocNo { get; set; }
    }
}
