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
    public interface IDyeingService : IDisposable
    {
        JobReceiveHeader Create(JobReceiveHeader s);
        DyeingViewModel Create(DyeingViewModel vmDyeing, string UserName);
        void Delete(int id);
        void Delete(JobReceiveHeader s);
        void Delete(ReasonViewModel vm, string UserName);
        DyeingViewModel GetDyeing(int id);
        JobReceiveHeader Find(int id);
        IQueryable<DyeingViewModel> GetDyeingList(int DocumentTypeId, string Uname);
        IQueryable<DyeingViewModel> GetDyeingListPendingToSubmit(int DocumentTypeId, string Uname);
        IQueryable<DyeingViewModel> GetDyeingListPendingToReview(int DocumentTypeId, string Uname);
        void Update(JobReceiveHeader s);
        void Update(DyeingViewModel vmDyeing, string UserName);
        string GetMaxDocNo();
        IEnumerable<ComboBoxList> GetJobWorkerHelpList(int Processid, string term);//PurchaseOrderHeaderId
        void Submit(int Id, string UserName, string GenGatePass, string UserRemark);
        void Review(int Id, string UserName, string UserRemark);
        int NextPrevId(int DocId, int DocTypeId, string UserName, string PrevNextConstants);
        byte[] GetReport(string Ids, int DocTypeId, string UserName);

        IQueryable<ComboBoxResult> GetJobOrderHelpListForProduct(int filter, string term);
        ComboBoxResult GetJobOrderLine(int Ids);
        JobOrderDetail GetJobOrderDetail(int JobOrderLineId);
        LastValues GetLastValues(int DocTypeId);

        #region Helper Methods
        IQueryable<UnitConversionFor> GetUnitConversionForList();
        void LogDetailInfo(DyeingViewModel vm);
        _Menu GetMenu(int Id);
        _Menu GetMenu(string Name);
        _ReportHeader GetReportHeader(string MenuName);
        _ReportLine GetReportLine(string Name, int ReportHeaderId);
        bool CheckForDocNoExists(string docno, int DocTypeId);
        bool CheckForDocNoExists(string docno, int headerid, int DocTypeId);
        #endregion

    }
    public class DyeingService : IDyeingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobReceiveHeader> _DyeingRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Unit> _unitRepository;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        private readonly IJobReceiveLineService _JobReceiveLineService;
        private readonly IJobReceiveLineStatusService _JobReceiveLineStatusService;
        private readonly IJobReceiveHeaderExtendedService _JobReceiveHeaderExtendedService;
        private readonly IStockService _stockService;
        private readonly IStockProcessService _stockProcessService;


        private ActiivtyLogViewModel logVm = new ActiivtyLogViewModel();

        public DyeingService(IUnitOfWork unit, IRepository<JobReceiveHeader> RecipeRepo,
            IJobReceiveLineService JobReceiveLineService,
            IJobReceiveLineStatusService JobReceiveLineStatusService,
            IJobReceiveHeaderExtendedService JobReceiveHeaderExtendedService,
            IStockService StockServ, IStockProcessService StockPRocServ,
            ILogger log, IModificationCheck modificationCheck, IRepository<Product> ProductRepo, IRepository<Unit> UnitRepo)
        {
            _unitOfWork = unit;
            _DyeingRepository = RecipeRepo;
            _stockProcessService = StockPRocServ;
            _stockService = StockServ;
            _logger = log;
            _modificationCheck = modificationCheck;
            _productRepository = ProductRepo;
            _unitRepository = UnitRepo;
            _JobReceiveLineService = JobReceiveLineService;
            _JobReceiveLineStatusService = JobReceiveLineStatusService;
            _JobReceiveHeaderExtendedService = JobReceiveHeaderExtendedService;

            //Log Initialization
            logVm.SessionId = 0;
            logVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            logVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            logVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        public JobReceiveHeader Create(JobReceiveHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveHeader>().Delete(id);
        }
        public void Delete(JobReceiveHeader s)
        {
            _unitOfWork.Repository<JobReceiveHeader>().Delete(s);
        }
        public void Update(JobReceiveHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveHeader>().Update(s);
        }


        public JobReceiveHeader Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveHeader>().Find(id);
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<JobReceiveHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }


        public IQueryable<DyeingViewModel> GetDyeingList(int DocumentTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in _DyeingRepository.Instance
                    join l in _unitOfWork.Repository<JobReceiveLine>().Instance on p.JobReceiveHeaderId equals l.JobReceiveHeaderId
                    join t in _unitOfWork.Repository<Person>().Instance on p.JobWorkerId equals t.PersonID
                    join dt in _unitOfWork.Repository<DocumentType>().Instance on p.DocTypeId equals dt.DocumentTypeId
                    orderby p.DocDate descending, p.DocNo descending
                    where p.DocTypeId == DocumentTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select new DyeingViewModel
                    {
                        DocTypeName = dt.DocumentTypeName,
                        JobWorkerName = t.Name + "," + t.Suffix,
                        MachineName = p.Machine.ProductName,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        Status = p.Status,
                        JobReceiveHeaderId = p.JobReceiveHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        Dimension1Name = l.JobOrderLine.Dimension1.Dimension1Name
                    });
        }

        public DyeingViewModel GetDyeing(int id)
        {
            return (from p in _DyeingRepository.Instance
                    join L in _unitOfWork.Repository<JobReceiveLine>().Instance on p.JobReceiveHeaderId equals L.JobReceiveHeaderId
                    join He in _unitOfWork.Repository<JobReceiveHeaderExtended>().Instance on p.JobReceiveHeaderId equals He.JobReceiveHeaderId
                    where p.JobReceiveHeaderId == id
                    select new DyeingViewModel
                    {
                        JobReceiveHeaderId = p.JobReceiveHeaderId,
                        StockHeaderId = (int) p.StockHeaderId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Status = p.Status,
                        DocTypeId = p.DocTypeId,
                        ProcessId = p.ProcessId,
                        JobWorkerId = p.JobWorkerId,
                        MachineId = p.MachineId,
                        GodownId = (int) p.GodownId,
                        JobReceiveById = p.JobReceiveById,
                        JobOrderLineId = (int)L.JobOrderLineId,
                        ProductId = L.JobOrderLine.ProductId,
                        Dimension1Id = L.JobOrderLine.Dimension1Id,
                        Dimension2Id = L.JobOrderLine.Dimension2Id,
                        LotNo = L.LotNo,
                        JobOrderNo = L.JobOrderLine.JobOrderHeader.DocNo,
                        Qty = L.Qty,
                        DivisionId = p.DivisionId,
                        SiteId = p.SiteId,
                        LockReason = p.LockReason,
                        StartDateTime = He.StartDateTime,
                        CompletedDateTime = He.CompletedDateTime,
                        LoadingTime = He.LoadingTime,
                        IsQCRequired = He.IsQCRequired,
                        DyeingType = He.DyeingType,
                        Remark = p.Remark,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                    }
                        ).FirstOrDefault();
        }


        public IEnumerable<RecipeLineListViewModel> GetPendingRecipesWithPatternMatch(int JobWorkerId, string term, int Limiter)//Product Id
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
                       select new RecipeLineListViewModel
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

        public IQueryable<DyeingViewModel> GetDyeingListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobReceiveHeader = GetDyeingList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobReceiveHeader
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


        public IQueryable<DyeingViewModel> GetDyeingListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobReceiveHeader = GetDyeingList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobReceiveHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }





        public DyeingViewModel Create(DyeingViewModel vmDyeing, string UserName)
        {
            JobReceiveHeader s = Mapper.Map<DyeingViewModel, JobReceiveHeader>(vmDyeing);

            int? PersonId = (from L in _unitOfWork.Repository<JobOrderLine>().Instance
                        join He in _unitOfWork.Repository<JobOrderHeaderExtended>().Instance on L.JobOrderHeaderId equals He.JobOrderHeaderId
                        where L.JobOrderLineId == vmDyeing.JobOrderLineId
                        select He).FirstOrDefault().PersonId;


            
            s.CreatedDate = DateTime.Now;
            s.ModifiedDate = DateTime.Now;
            s.CreatedBy = UserName;
            s.ModifiedBy = UserName;
            s.Status = (int)StatusConstants.Drafted;



            StockViewModel StockViewModel = new StockViewModel();

            //Posting in Stock
            StockViewModel.StockHeaderId = s.StockHeaderId ?? 0;
            StockViewModel.DocHeaderId = s.JobReceiveHeaderId;
            StockViewModel.DocLineId = null;
            StockViewModel.DocTypeId = s.DocTypeId;
            StockViewModel.StockHeaderDocDate = s.DocDate;
            StockViewModel.StockDocDate = DateTime.Now.Date;
            StockViewModel.DocNo = s.DocNo;
            StockViewModel.DivisionId = s.DivisionId;
            StockViewModel.SiteId = s.SiteId;
            StockViewModel.CurrencyId = null;
            StockViewModel.HeaderProcessId = null;
            StockViewModel.PersonId = PersonId;
            StockViewModel.ProductId = vmDyeing.ProductId;
            StockViewModel.HeaderFromGodownId = null;
            StockViewModel.HeaderGodownId = null;
            StockViewModel.GodownId = vmDyeing.GodownId ;
            StockViewModel.ProcessId = null;
            StockViewModel.LotNo = vmDyeing.LotNo;
            StockViewModel.CostCenterId = null;
            StockViewModel.Qty_Iss = 0;
            StockViewModel.Qty_Rec = vmDyeing.Qty;
            StockViewModel.Rate = 0;
            StockViewModel.ExpiryDate = null;
            StockViewModel.Specification = null;
            StockViewModel.Dimension1Id = vmDyeing.Dimension1Id;
            StockViewModel.Dimension2Id = vmDyeing.Dimension2Id;
            StockViewModel.Remark = s.Remark;
            StockViewModel.ProductUidId = null;
            StockViewModel.Status = 0;
            StockViewModel.CreatedBy = UserName;
            StockViewModel.CreatedDate = DateTime.Now;
            StockViewModel.ModifiedBy = UserName;
            StockViewModel.ModifiedDate = DateTime.Now;

            string StockPostingError = "";
            StockPostingError = _stockService.StockPostDB(ref StockViewModel);

            

            if (s.StockHeaderId == null)
            {
                s.StockHeaderId = StockViewModel.StockHeaderId;
            }


            s.ObjectState = Model.ObjectState.Added;
            Create(s);
            //Line Save


            JobReceiveLine line = new JobReceiveLine();

            line.JobReceiveHeaderId = s.JobReceiveHeaderId;
            line.JobOrderLineId = vmDyeing.JobOrderLineId;
            line.Qty = vmDyeing.Qty;
            line.PassQty = vmDyeing.Qty;
            line.Sr = 1;
            line.LotNo = vmDyeing.LotNo;
            line.LossQty = 0;
            line.DealQty = vmDyeing.Qty;
            line.DealUnitId = vmDyeing.UnitId;
            line.UnitConversionMultiplier = 1;
            line.CreatedDate = DateTime.Now;
            line.ModifiedDate = DateTime.Now;
            line.CreatedBy = UserName;
            line.ModifiedBy = UserName;
            line.StockId = StockViewModel.StockId;
            line.ObjectState = Model.ObjectState.Added;

            _JobReceiveLineService.Create(line);



            _JobReceiveLineStatusService.CreateLineStatus(line.JobReceiveLineId);

            JobReceiveHeaderExtended HeaderExtended = new JobReceiveHeaderExtended();
            HeaderExtended.JobReceiveHeaderId = s.JobReceiveHeaderId;
            HeaderExtended.StartDateTime = vmDyeing.StartDateTime.Value.AddHours(vmDyeing.StartDateTimeHour).AddMinutes(vmDyeing.StartDateTimeMinute);
            if (vmDyeing.CompletedDateTime != null)
            {
                HeaderExtended.CompletedDateTime = vmDyeing.CompletedDateTime.Value.AddHours(vmDyeing.CompletedDateTimeHour).AddMinutes(vmDyeing.CompletedDateTimeMinute);
            }
            HeaderExtended.LoadingTime = vmDyeing.LoadingTime;
            HeaderExtended.IsQCRequired = vmDyeing.IsQCRequired;
            HeaderExtended.DyeingType = vmDyeing.DyeingType;
            HeaderExtended.ObjectState = Model.ObjectState.Added;
            _JobReceiveHeaderExtendedService.Create(HeaderExtended);


            //End Line Save


            _unitOfWork.Save();

            vmDyeing.JobReceiveHeaderId = s.JobReceiveHeaderId;

            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = s.DocTypeId,
                DocId = s.JobReceiveHeaderId,
                ActivityType = (int)ActivityTypeContants.Added,
                DocNo = s.DocNo,
                DocDate = s.DocDate,
                DocStatus = s.Status,
            }));

            

            return vmDyeing;
        }


        public void Update(DyeingViewModel vmDyeing, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            JobReceiveHeader temp = Find(vmDyeing.JobReceiveHeaderId);
            JobReceiveLine line = _JobReceiveLineService.GetJobReceiveLineListForHeader(vmDyeing.JobReceiveHeaderId).FirstOrDefault();

            JobReceiveHeader ExRec = Mapper.Map<JobReceiveHeader>(temp);

            int? PersonId = (from L in _unitOfWork.Repository<JobOrderLine>().Instance
                             join He in _unitOfWork.Repository<JobOrderHeaderExtended>().Instance on L.JobOrderHeaderId equals He.JobOrderHeaderId
                             where L.JobOrderLineId == vmDyeing.JobOrderLineId
                             select He).FirstOrDefault().PersonId;

            int status = temp.Status;

            if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                temp.Status = (int)StatusConstants.Modified;


            temp.DocDate = vmDyeing.DocDate;
            temp.ProcessId = vmDyeing.ProcessId;
            temp.JobWorkerId = vmDyeing.JobWorkerId;
            temp.MachineId = vmDyeing.MachineId;
            temp.JobReceiveById = vmDyeing.JobReceiveById;
            temp.DocNo = vmDyeing.DocNo;
            temp.Remark = vmDyeing.Remark;
            temp.ModifiedDate = DateTime.Now;
            temp.ModifiedBy = UserName;
            temp.ObjectState = Model.ObjectState.Modified;
            Update(temp);



            StockViewModel StockViewModel = new StockViewModel();

            //Posting in Stock
            StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
            StockViewModel.StockId = line.StockId ?? 0;
            StockViewModel.DocHeaderId = temp.JobReceiveHeaderId;
            StockViewModel.DocLineId = null;
            StockViewModel.DocTypeId = temp.DocTypeId;
            StockViewModel.StockHeaderDocDate = temp.DocDate;
            StockViewModel.StockDocDate = temp.DocDate;
            StockViewModel.DocNo = temp.DocNo;
            StockViewModel.DivisionId = temp.DivisionId;
            StockViewModel.SiteId = temp.SiteId;
            StockViewModel.CurrencyId = null;
            StockViewModel.HeaderProcessId = null;
            StockViewModel.PersonId = PersonId;
            StockViewModel.ProductId = vmDyeing.ProductId;
            StockViewModel.HeaderFromGodownId = null;
            StockViewModel.HeaderGodownId = null;
            StockViewModel.GodownId = vmDyeing.GodownId;
            StockViewModel.ProcessId = null;
            StockViewModel.LotNo = vmDyeing.LotNo;
            StockViewModel.CostCenterId = null;
            StockViewModel.Qty_Iss = 0;
            StockViewModel.Qty_Rec = vmDyeing.Qty;
            StockViewModel.Rate = 0;
            StockViewModel.ExpiryDate = null;
            StockViewModel.Specification = null;
            StockViewModel.Dimension1Id = vmDyeing.Dimension1Id;
            StockViewModel.Dimension2Id = vmDyeing.Dimension2Id;
            StockViewModel.Remark = temp.Remark;
            StockViewModel.ProductUidId = null;
            StockViewModel.Status = 0;
            StockViewModel.CreatedBy = UserName;
            StockViewModel.CreatedDate = DateTime.Now;
            StockViewModel.ModifiedBy = UserName;
            StockViewModel.ModifiedDate = DateTime.Now;

            string StockPostingError = "";
            StockPostingError = _stockService.StockPostDB(ref StockViewModel);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = ExRec,
                Obj = temp,
            });

            if (temp.StockHeaderId != null && temp.StockHeaderId != 0)
            {
                StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find((int)temp.StockHeaderId);
                StockHeader.DocDate = temp.DocDate;
                StockHeader.DocNo = temp.DocNo;
                new StockHeaderService(_unitOfWork).Update(StockHeader);
            }


            

            line.JobReceiveHeaderId = temp.JobReceiveHeaderId;
            line.JobOrderLineId = vmDyeing.JobOrderLineId;
            line.JobOrderLineId = vmDyeing.JobOrderLineId;
            line.Qty = vmDyeing.Qty;
            line.Sr = 1;
            line.LossQty = 0;
            line.DealQty = vmDyeing.Qty;
            line.DealUnitId = vmDyeing.UnitId;
            line.UnitConversionMultiplier = 1;
            line.CreatedDate = DateTime.Now;
            line.ModifiedDate = DateTime.Now;
            line.CreatedBy = UserName;
            line.ModifiedBy = UserName;
            line.ObjectState = Model.ObjectState.Modified;

            _JobReceiveLineService.Update(line);


            JobReceiveHeaderExtended HeaderExtended = _JobReceiveHeaderExtendedService.Find(temp.JobReceiveHeaderId);
            HeaderExtended.JobReceiveHeaderId = temp.JobReceiveHeaderId;
            HeaderExtended.StartDateTime = vmDyeing.StartDateTime.Value.AddHours(vmDyeing.StartDateTimeHour).AddMinutes(vmDyeing.StartDateTimeMinute);
            if (vmDyeing.CompletedDateTime != null)
            {
                HeaderExtended.CompletedDateTime = vmDyeing.CompletedDateTime.Value.AddHours(vmDyeing.CompletedDateTimeHour).AddMinutes(vmDyeing.CompletedDateTimeMinute);
            }
            HeaderExtended.LoadingTime = vmDyeing.LoadingTime;
            HeaderExtended.IsQCRequired = vmDyeing.IsQCRequired;
            HeaderExtended.DyeingType = vmDyeing.DyeingType;
            HeaderExtended.ObjectState = Model.ObjectState.Modified;
            _JobReceiveHeaderExtendedService.Update(HeaderExtended);

            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = temp.DocTypeId,
                DocId = temp.JobReceiveHeaderId,
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

            var JobReceiveHeader = Find(vm.id);
            
            int? StockHeaderId = 0;

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Mapper.Map<JobReceiveHeader>(JobReceiveHeader),
            });

            StockHeaderId = JobReceiveHeader.StockHeaderId;

            //Then find all the Job Order Header Line associated with the above ProductType.
            //var JobReceiveLine = new JobReceiveLineService(_unitOfWork).GetJobReceiveLineforDelete(vm.id);
            var Stock = (_unitOfWork.Repository<Stock>().Query().Get().Where(m => m.StockHeaderId == StockHeaderId)).ToList();

            var StockProcess = (_unitOfWork.Repository<StockProcess>().Query().Get().Where(m => m.StockHeaderId == StockHeaderId)).ToList();

            var JobReceiveLine = (_unitOfWork.Repository<JobReceiveLine>().Query().Get().Where(m => m.JobReceiveHeaderId == vm.id)).ToList();

            var JOLineIds = JobReceiveLine.Select(m => m.JobReceiveLineId).ToArray();

            var JobReceiveLineStatusRecords = _unitOfWork.Repository<JobReceiveLineStatus>().Query().Get().Where(m => JOLineIds.Contains(m.JobReceiveLineId ?? 0)).ToList();

            var JobReceiveHeaderExtendedRecords = _unitOfWork.Repository<JobReceiveHeaderExtended>().Query().Get().Where(m => m.JobReceiveHeaderId == JobReceiveHeader.JobReceiveHeaderId).ToList();


            List<int> StockIdList = new List<int>();
            List<int> StockProcessIdList = new List<int>();


            
            foreach (var item in JobReceiveLineStatusRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobReceiveLineStatus>().Delete(item);
            }

            foreach (var item in JobReceiveHeaderExtendedRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobReceiveHeaderExtended>().Delete(item);
            }


            //Mark ObjectState.Delete to all the Job Order Lines. 
            foreach (var item in JobReceiveLine)
            {

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveLine>(item),
                });


                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobReceiveLine>().Delete(item);

            }

            foreach (var item in Stock)
            {
                if (item.StockId != null)
                {
                    StockIdList.Add((int)item.StockId);

                    IEnumerable<StockAdj> StockAdj = (from L in _unitOfWork.Repository<StockAdj>().Instance
                                   where L.StockInId == item.StockId 
                                   select L).ToList();

                    foreach (var StockAdjitem in StockAdj)
                    {
                        if (StockAdjitem != null)
                        {
                            StockAdjitem.ObjectState = Model.ObjectState.Deleted;
                            _unitOfWork.Repository<StockAdj>().Delete(StockAdjitem);
                        }
                    }
                }
            }

            foreach (var item in StockProcess)
            {
                if (item.StockProcessId != null)
                {
                    StockProcessIdList.Add((int)item.StockProcessId);
                }
            }



            _stockService.DeleteStockMultiple(StockIdList);

            _stockProcessService.DeleteStockProcessDBMultiple(StockProcessIdList);


            // Now delete the Purhcase Order Header
            //_DyeingService.Delete(JobReceiveHeader);

            int ReferenceDocId = JobReceiveHeader.JobReceiveHeaderId;
            int ReferenceDocTypeId = JobReceiveHeader.DocTypeId;


            JobReceiveHeader.ObjectState = Model.ObjectState.Deleted;
            Delete(JobReceiveHeader);



            

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
                    DocTypeId = JobReceiveHeader.DocTypeId,
                    DocId = JobReceiveHeader.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = JobReceiveHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = JobReceiveHeader.DocDate,
                    DocStatus = JobReceiveHeader.Status,
                }));

        }


        public void Submit(int Id, string UserName, string GenGatePass, string UserRemark)
        {
            var pd = _DyeingRepository.Find(Id);

            pd.Status = (int)StatusConstants.Submitted;
            int ActivityType = (int)ActivityTypeContants.Submitted;


            pd.ReviewBy = null;
            pd.ObjectState = Model.ObjectState.Modified;
            _DyeingRepository.Update(pd);


            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = pd.DocTypeId,
                DocId = pd.JobReceiveHeaderId,
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
                DocId = pd.JobReceiveHeaderId,
                ActivityType = (int)ActivityTypeContants.Reviewed,
                UserRemark = UserRemark,
                DocNo = pd.DocNo,
                DocDate = pd.DocDate,
                DocStatus = pd.Status,
            }));

        }

        public int NextPrevId(int DocId, int DocTypeId, string UserName, string PrevNext)
        {
            return new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, UserName, "", "Web.JobReceiveHeaders", "JobReceiveHeaderId", PrevNext);
        }

        public byte[] GetReport(string Ids, int DocTypeId, string UserName)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, DivisionId, SiteId);

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
                    DocId = pd.JobReceiveHeaderId,
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

        public void LogDetailInfo(DyeingViewModel vm)
        {
            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = vm.DocTypeId,
                DocId = vm.JobReceiveHeaderId,
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

            var temp = (from pr in _DyeingRepository.Instance
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

            var temp = (from pr in _DyeingRepository.Instance
                        where pr.DocNo == docno && pr.JobReceiveHeaderId != headerid && (pr.DocTypeId == DocTypeId) && pr.SiteId == SiteId && pr.DivisionId == DivisionId
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        #endregion


        

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        public IQueryable<ComboBoxResult> GetJobOrderHelpListForProduct(int filter, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = (from p in _unitOfWork.Repository<ViewJobOrderBalance>().Instance
                        join t in _unitOfWork.Repository<JobOrderHeader>().Instance on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        join t2 in _unitOfWork.Repository<JobOrderLine>().Instance on p.JobOrderLineId equals t2.JobOrderLineId
                        join pt in _unitOfWork.Repository <Product>().Instance on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in _unitOfWork.Repository <Dimension1>().Instance on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in _unitOfWork.Repository<Dimension2>().Instance on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where p.BalanceQty > 0
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower()))
                        && p.SiteId == CurrentSiteId
                        && p.DivisionId == CurrentDivisionId
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = t.DocType.DocumentTypeName + "-" + p.JobOrderNo,
                            id = p.JobOrderLineId.ToString(),
                            TextProp1 = "Product: " + ProductTab.ProductName.ToString(),
                            TextProp2 = "Qty: " + p.BalanceQty.ToString(),
                            AProp1 = Dimension1Tab.Dimension1Name,
                            AProp2 = Dimension2Tab.Dimension2Name
                        });

            return list;
        }

        public ComboBoxResult GetJobOrderLine(int Ids)
        {
            var JobOrderLine = (from L in _unitOfWork.Repository<JobOrderLine>().Instance
                                join H in _unitOfWork.Repository<JobOrderHeader>().Instance on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                                from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                                where L.JobOrderLineId == Ids
                                 select new ComboBoxResult
                                {
                                    id = L.JobOrderLineId.ToString(),
                                    text = JobOrderHeaderTab.DocNo
                                }).FirstOrDefault();

            return JobOrderLine;
        }

        public JobOrderDetail GetJobOrderDetail(int JobOrderLineId)
        {
            var temp = (from L in _unitOfWork.Repository<ViewJobOrderBalance>().Instance
                        join Dl in _unitOfWork.Repository<JobOrderLine>().Instance on L.JobOrderLineId equals Dl.JobOrderLineId into JobOrderLineTable
                        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                        join Dh in _unitOfWork.Repository<JobOrderHeader>().Instance on L.JobOrderHeaderId equals Dh.JobOrderHeaderId into JobOrderHeaderTable
                        from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                        where L.JobOrderLineId == JobOrderLineId
                        select new JobOrderDetail
                        {
                            JobOrderHeaderDocNo = L.JobOrderNo,
                            ProductId = L.ProductId,
                            ProductName = L.Product.ProductName,
                            Dimension1Id = L.Dimension1Id,
                            Dimension1Name = L.Dimension1.Dimension1Name,
                            Dimension2Id = L.Dimension2Id,
                            Dimension2Name = L.Dimension2.Dimension2Name,
                            MachineId = JobOrderHeaderTab.MachineId,
                            MachineName = JobOrderHeaderTab.Machine.ProductName,
                            Qty = L.BalanceQty,
                            BalanceQty = L.BalanceQty,
                            LotNo = JobOrderLineTab.LotNo,
                            UnitId = JobOrderLineTab.UnitId
                        }).FirstOrDefault();

            return temp;
        }

        public LastValues GetLastValues(int DocTypeId)
        {
            var temp = (from H in _unitOfWork.Repository<JobReceiveHeader>().Instance
                        where H.DocTypeId == DocTypeId
                        orderby H.JobReceiveHeaderId descending
                        select new LastValues
                        {
                            GodownId = H.GodownId,
                            JobWorkerId = H.JobWorkerId,
                            JobReceiveById = H.JobReceiveById
                        }).FirstOrDefault();

            return temp;
        }
    }

    public class JobOrderDetail
    {
        public string JobOrderHeaderDocNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public int? MachineId { get; set; }
        public string MachineName { get; set; }
        public string LotNo { get; set; }
        public Decimal? Qty { get; set; }
        public Decimal? BalanceQty { get; set; }
        public string UnitId { get; set; }

    }

    public class LastValues
    {
        public int? JobWorkerId { get; set; }
        public int? GodownId { get; set; }
        public int? JobReceiveById { get; set; }

        public int? OrderById { get; set; }
        public Decimal? TestingQty { get; set; }

        public Decimal? DyeingRatio { get; set; }

    }
}


