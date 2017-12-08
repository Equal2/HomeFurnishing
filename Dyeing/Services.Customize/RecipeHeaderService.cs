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
    public interface IRecipeHeaderService : IDisposable
    {
        JobOrderHeader Create(JobOrderHeader s);
        RecipeHeaderViewModel Create(RecipeHeaderViewModel vmRecipeHeader, string UserName);
        void Delete(int id);
        void Delete(JobOrderHeader s);
        void Delete(ReasonViewModel vm, string UserName);
        RecipeHeaderViewModel GetRecipeHeader(int id);
        JobOrderHeader Find(int id);
        IQueryable<RecipeHeaderViewModel> GetRecipeHeaderList(int DocumentTypeId, string Uname);
        IQueryable<RecipeHeaderViewModel> GetRecipeHeaderListPendingToSubmit(int DocumentTypeId, string Uname);
        IQueryable<RecipeHeaderViewModel> GetRecipeHeaderListPendingToReview(int DocumentTypeId, string Uname);
        void Update(JobOrderHeader s);
        void Update(RecipeHeaderViewModel vmRecipeHeader, string UserName);
        string GetMaxDocNo();
        IEnumerable<ComboBoxList> GetJobWorkerHelpList(int Processid, string term);//PurchaseOrderHeaderId
        DateTime AddDueDate(DateTime Base, int DueDays);
        void Submit(int Id, string UserName, string GenGatePass, string UserRemark);
        void Review(int Id, string UserName, string UserRemark);
        int NextPrevId(int DocId, int DocTypeId, string UserName, string PrevNextConstants);
        byte[] GetReport(string Ids, int DocTypeId, string UserName);

        IQueryable<ComboBoxResult> GetProdOrderHelpListForProduct(int filter, string term);
        ComboBoxResult GetProdOrderLine(int Ids);
        ProdOrderDetail GetProdOrderDetail(int ProdOrderLineId);

        LastValues GetLastValues(int DocTypeId);

        void CreateProdOrder(int RecipeHeaderId, string UserName, Decimal? SubRecipeQty);


        #region Helper Methods
        IQueryable<UnitConversionFor> GetUnitConversionForList();
        void LogDetailInfo(RecipeHeaderViewModel vm);
        _Menu GetMenu(int Id);
        _Menu GetMenu(string Name);
        _ReportHeader GetReportHeader(string MenuName);
        _ReportLine GetReportLine(string Name, int ReportHeaderId);
        bool CheckForDocNoExists(string docno, int DocTypeId);
        bool CheckForDocNoExists(string docno, int headerid, int DocTypeId);
        #endregion

    }
    public class RecipeHeaderService : IRecipeHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderHeader> _RecipeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Unit> _unitRepository;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        private readonly IStockService _stockService;
        private readonly IStockProcessService _stockProcessService;
        private readonly IStockAdjService _stockAdjService;
        private readonly IJobOrderLineService _JobOrderLineService;
        private readonly IJobOrderLineStatusService _JobOrderLineStatusService;
        private readonly IJobOrderLineExtendedService _JobOrderLineExtendedService;
        private readonly IDocumentTypeService _DocumentTypeService;

        private ActiivtyLogViewModel logVm = new ActiivtyLogViewModel();

        public RecipeHeaderService(IUnitOfWork unit, IRepository<JobOrderHeader> RecipeRepo,
            IStockService StockServ, IStockProcessService StockPRocServ, IStockAdjService StockAdjServ, 
            IJobOrderLineService JobOrderLineService,
            IJobOrderLineStatusService JobOrderLineStatusService,
            IJobOrderLineExtendedService JobOrderLineExtendedService,
            IDocumentTypeService DocumentTypeService,
            ILogger log, IModificationCheck modificationCheck, IRepository<Product> ProductRepo, IRepository<Unit> UnitRepo)
        {
            _unitOfWork = unit;
            _RecipeRepository = RecipeRepo;
            _stockProcessService = StockPRocServ;
            _stockService = StockServ;
            _stockAdjService = StockAdjServ;
            _JobOrderLineService = JobOrderLineService;
            _JobOrderLineStatusService = JobOrderLineStatusService;
            _JobOrderLineExtendedService = JobOrderLineExtendedService;
            _DocumentTypeService = DocumentTypeService;
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


        public IQueryable<RecipeHeaderViewModel> GetRecipeHeaderList(int DocumentTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in _RecipeRepository.Instance
                    join l in _unitOfWork.Repository<JobOrderLine>().Instance on p.JobOrderHeaderId equals l.JobOrderHeaderId
                    join t in _unitOfWork.Repository<Person>().Instance on p.JobWorkerId equals t.PersonID
                    join dt in _unitOfWork.Repository<DocumentType>().Instance on p.DocTypeId equals dt.DocumentTypeId
                    orderby p.DocDate descending, p.DocNo descending
                    where p.DocTypeId == DocumentTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select new RecipeHeaderViewModel
                    {
                        DocTypeName = dt.DocumentTypeName,
                        DueDate = p.DueDate,
                        JobWorkerName = t.Name + "," + t.Suffix,
                        MachineName = p.Machine.ProductName,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        Status = p.Status,
                        JobOrderHeaderId = p.JobOrderHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        Dimension1Name = l.Dimension1.Dimension1Name,
                        Dimension2Name=l.Dimension2.Dimension2Name

                    });
        }

        public RecipeHeaderViewModel GetRecipeHeader(int id)
        {
            return (from p in _RecipeRepository.Instance
                    join t in _unitOfWork.Repository<JobOrderHeaderExtended>().Instance on p.JobOrderHeaderId equals t.JobOrderHeaderId
                    join L in _unitOfWork.Repository<JobOrderLine>().Instance on p.JobOrderHeaderId equals L.JobOrderHeaderId
                    join Le in _unitOfWork.Repository<JobOrderLineExtended>().Instance on L.JobOrderLineId equals Le.JobOrderLineId
                    where p.JobOrderHeaderId == id
                    select new RecipeHeaderViewModel
                    {
                        JobOrderHeaderId = p.JobOrderHeaderId,
                        StockHeaderId = (int) p.StockHeaderId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Status = p.Status,
                        DocTypeId = p.DocTypeId,
                        PersonId = t.PersonId,
                        DueDate = p.DueDate,
                        ProcessId = p.ProcessId,
                        JobWorkerId = p.JobWorkerId,
                        MachineId = p.MachineId,
                        GodownId = (int) p.GodownId,
                        OrderById = p.OrderById,
                        ProductId = L.ProductId,
                        Dimension1Id = (int)L.Dimension1Id,
                        Dimension2Id = (int)L.Dimension2Id,
                        ProdOrderLineId = (int)L.ProdOrderLineId,
                        LotNo = L.LotNo,
                        ProdOrderNo = L.ProdOrderLine.ProdOrderHeader.DocNo,
                        Qty = L.Qty,
                        TestingQty = Le.TestingQty,
                        SubRecipeQty = Le.SubRecipeQty,
                        CreditDays = p.CreditDays,
                        DivisionId = p.DivisionId,
                        SiteId = p.SiteId,
                        LockReason = p.LockReason,
                        Remark = p.Remark,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        UnitId=L.UnitId,
                    }
                        ).FirstOrDefault();
        }


        public IEnumerable<RecipeHeaderListViewModel> GetPendingRecipesWithPatternMatch(int JobWorkerId, string term, int Limiter)//Product Id
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
                       select new RecipeHeaderListViewModel
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

        public IQueryable<RecipeHeaderViewModel> GetRecipeHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetRecipeHeaderList(id, Uname).AsQueryable();

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


        public IQueryable<RecipeHeaderViewModel> GetRecipeHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetRecipeHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }




        public DateTime AddDueDate(DateTime Base, int DueDays)
        {
            DateTime DueDate = Base.AddDays((double)DueDays);
            if (DueDate.DayOfWeek == DayOfWeek.Sunday)
                DueDate = DueDate.AddDays(1);

            return DueDate;
        }

        public RecipeHeaderViewModel Create(RecipeHeaderViewModel vmRecipeHeader, string UserName)
        {
            JobOrderHeader s = Mapper.Map<RecipeHeaderViewModel, JobOrderHeader>(vmRecipeHeader);
            DocumentType D = _DocumentTypeService.Find(s.DocTypeId);

            s.BillToPartyId = vmRecipeHeader.JobWorkerId;
            s.ActualDocDate = s.DocDate;
            s.DueDate = s.DocDate;
            s.ActualDueDate = s.DocDate;
            s.CreatedDate = DateTime.Now;
            s.ModifiedDate = DateTime.Now;
            s.ActualDueDate = s.DueDate;
            s.ActualDocDate = s.DocDate;
            s.CreatedBy = UserName;
            s.ModifiedBy = UserName;
            s.Status = (int)StatusConstants.Drafted;


            JobOrderHeaderStatus Stat = new JobOrderHeaderStatus();
            Stat.JobOrderHeaderId = s.JobOrderHeaderId;
            Stat.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<JobOrderHeaderStatus>().Add(Stat);

            JobOrderHeaderExtended Extend = new JobOrderHeaderExtended();
            Extend.JobOrderHeaderId = s.JobOrderHeaderId;
            Extend.PersonId = vmRecipeHeader.PersonId;
            Extend.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<JobOrderHeaderExtended>().Add(Extend);





            StockViewModel StockViewModel = new StockViewModel();

            //Posting in Stock
            StockViewModel.StockHeaderId = s.StockHeaderId ?? 0;
            StockViewModel.DocHeaderId = s.JobOrderHeaderId;
            StockViewModel.DocLineId = null;
            StockViewModel.DocTypeId = s.DocTypeId;
            StockViewModel.StockHeaderDocDate = s.DocDate;
            StockViewModel.StockDocDate = DateTime.Now.Date;
            StockViewModel.DocNo = s.DocNo;
            StockViewModel.DivisionId = s.DivisionId;
            StockViewModel.SiteId = s.SiteId;
            StockViewModel.CurrencyId = null;
            StockViewModel.HeaderProcessId = null;
            StockViewModel.PersonId = vmRecipeHeader.PersonId;
            StockViewModel.ProductId = vmRecipeHeader.ProductId;
            StockViewModel.HeaderFromGodownId = null;
            StockViewModel.HeaderGodownId = vmRecipeHeader.GodownId;
            StockViewModel.GodownId = vmRecipeHeader.GodownId;
            StockViewModel.ProcessId = null;
            StockViewModel.LotNo = vmRecipeHeader.LotNo;
            StockViewModel.CostCenterId = null;
            StockViewModel.Qty_Iss = vmRecipeHeader.Qty;
            StockViewModel.Qty_Rec = 0;
            StockViewModel.Rate = 0;
            StockViewModel.ExpiryDate = null;
            StockViewModel.Specification = null;
            if (D.DocumentTypeShortName == "RCPQC" || D.DocumentTypeShortName == "SRCPE" || D.DocumentTypeShortName == "RDRCP")
            {
                StockViewModel.Dimension1Id = vmRecipeHeader.Dimension1Id;
                StockViewModel.Dimension2Id = vmRecipeHeader.Dimension2Id;
            }
            StockViewModel.Remark = s.Remark;
            StockViewModel.ProductUidId = null;
            StockViewModel.Status = 0;
            StockViewModel.CreatedBy = UserName;
            StockViewModel.CreatedDate = DateTime.Now;
            StockViewModel.ModifiedBy = UserName;
            StockViewModel.ModifiedDate = DateTime.Now;

            string StockPostingError = "";
            StockPostingError = _stockService.StockPostDB(ref StockViewModel);





            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

            //Posting in StockProcess
            StockProcessViewModel.StockHeaderId = -1;
            StockProcessViewModel.DocHeaderId = s.JobOrderHeaderId;
            StockProcessViewModel.DocLineId = null;
            StockProcessViewModel.DocTypeId = s.DocTypeId;
            StockProcessViewModel.StockHeaderDocDate = s.DocDate;
            StockProcessViewModel.StockProcessDocDate = DateTime.Now.Date;
            StockProcessViewModel.DocNo = s.DocNo;
            StockProcessViewModel.DivisionId = s.DivisionId;
            StockProcessViewModel.SiteId = s.SiteId;
            StockProcessViewModel.CurrencyId = null;
            StockProcessViewModel.HeaderProcessId = null;
            StockProcessViewModel.PersonId = vmRecipeHeader.PersonId;
            StockProcessViewModel.ProductId = vmRecipeHeader.ProductId;
            StockProcessViewModel.HeaderFromGodownId = null;
            StockProcessViewModel.HeaderGodownId = vmRecipeHeader.GodownId;
            StockProcessViewModel.GodownId = vmRecipeHeader.GodownId;
            StockProcessViewModel.ProcessId = null;
            StockProcessViewModel.LotNo = vmRecipeHeader.LotNo;
            StockProcessViewModel.CostCenterId = null;
            StockProcessViewModel.Qty_Iss = 0;
            StockProcessViewModel.Qty_Rec = vmRecipeHeader.Qty;
            StockProcessViewModel.Rate = 0;
            StockProcessViewModel.ExpiryDate = null;
            StockProcessViewModel.Specification = null;
            StockProcessViewModel.Remark = s.Remark;
            StockProcessViewModel.ProductUidId = null;
            StockProcessViewModel.Status = 0;
            StockProcessViewModel.CreatedBy = UserName;
            StockProcessViewModel.CreatedDate = DateTime.Now;
            StockProcessViewModel.ModifiedBy = UserName;
            StockProcessViewModel.ModifiedDate = DateTime.Now;

            string StockProcessPostingError = "";
            StockProcessPostingError = _stockProcessService.StockProcessPostDB(ref StockProcessViewModel);




            if (s.StockHeaderId == null)
            {
                s.StockHeaderId = StockViewModel.StockHeaderId;
            }

            s.StockHeaderId = StockViewModel.StockHeaderId;
            s.ObjectState = Model.ObjectState.Added;
            Create(s);


            string DocTypeName = (from DT in _unitOfWork.Repository<DocumentType>().Instance
                                  where DT.DocumentTypeId == vmRecipeHeader.DocTypeId
                                  select DT).FirstOrDefault().DocumentTypeName;

            if (DocTypeName == "Sub-Recipe")
            {
                SqlParameter SqlParameterProdOrderLineId = new SqlParameter("@ProdOrderLineId", vmRecipeHeader.ProdOrderLineId);
                IEnumerable<StockInDetail> StockInDetail = _unitOfWork.SqlQuery<StockInDetail>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetStockInForSubRecipe @ProdOrderLineId", SqlParameterProdOrderLineId).ToList();

                if (StockInDetail != null)
                {
                    if (StockInDetail.FirstOrDefault().StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)StockInDetail.FirstOrDefault().StockInId;
                        Adj_IssQty.StockOutId = (int)StockViewModel.StockId;
                        Adj_IssQty.DivisionId = vmRecipeHeader.DivisionId;
                        Adj_IssQty.SiteId = vmRecipeHeader.SiteId;
                        Adj_IssQty.AdjustedQty = vmRecipeHeader.Qty;
                        Adj_IssQty.ObjectState = ObjectState.Added;

                        _stockAdjService.Create(Adj_IssQty);
                    }
                    else
                    {
                        throw new Exception("Recipe Production is not done yet.");
                    }
                }
                else
                {
                    throw new Exception("Recipe Production is not done yet.");
                }
            }


            

            
            //Line Save
            JobOrderLine line = new JobOrderLine();
            line.JobOrderHeaderId = s.JobOrderHeaderId;
            line.ProdOrderLineId = vmRecipeHeader.ProdOrderLineId;
            line.ProductId = vmRecipeHeader.ProductId;
            line.Dimension1Id = vmRecipeHeader.Dimension1Id;
            line.Dimension2Id = vmRecipeHeader.Dimension2Id;
            line.Qty = vmRecipeHeader.Qty;
            line.UnitId = vmRecipeHeader.UnitId;
            line.Sr = 1;
            line.LotNo = vmRecipeHeader.LotNo;
            line.LossQty = 0;
            line.NonCountedQty = 0;
            line.Rate = 0;
            line.DealQty = vmRecipeHeader.Qty;
            line.DealUnitId = vmRecipeHeader.UnitId;
            line.Amount = 0;
            line.UnitConversionMultiplier = 1;
            line.CreatedDate = DateTime.Now;
            line.ModifiedDate = DateTime.Now;
            line.CreatedBy = UserName;
            line.ModifiedBy = UserName;
            line.StockId = StockViewModel.StockId;
            line.StockProcessId = StockProcessViewModel.StockProcessId;
            line.ObjectState = Model.ObjectState.Added;

            _JobOrderLineService.Create(line);

            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            if (line.ProdOrderLineId.HasValue)
                LineStatus.Add(line.ProdOrderLineId.Value, line.Qty);


            _JobOrderLineStatusService.CreateLineStatus(line.JobOrderLineId);

            JobOrderLineExtended LineExtended = new JobOrderLineExtended();
            LineExtended.JobOrderLineId = line.JobOrderLineId;
            LineExtended.SubRecipeQty = vmRecipeHeader.SubRecipeQty;
            LineExtended.TestingQty = vmRecipeHeader.TestingQty;
            LineExtended.ObjectState = Model.ObjectState.Added;
            _JobOrderLineExtendedService.Create(LineExtended);












            //End Line Save


            _unitOfWork.Save();

            vmRecipeHeader.JobOrderHeaderId = s.JobOrderHeaderId;

            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = s.DocTypeId,
                DocId = s.JobOrderHeaderId,
                ActivityType = (int)ActivityTypeContants.Added,
                DocNo = s.DocNo,
                DocDate = s.DocDate,
                DocStatus = s.Status,
            }));

            

            return vmRecipeHeader;
        }


        public void Update(RecipeHeaderViewModel vmRecipeHeader, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            JobOrderHeader temp = Find(vmRecipeHeader.JobOrderHeaderId);

            JobOrderHeader ExRec = Mapper.Map<JobOrderHeader>(temp);

            DocumentType D = _DocumentTypeService.Find(temp.DocTypeId);

            int status = temp.Status;

            if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                temp.Status = (int)StatusConstants.Modified;


            temp.DocDate = vmRecipeHeader.DocDate;
            temp.ProcessId = vmRecipeHeader.ProcessId;
            temp.JobWorkerId = vmRecipeHeader.JobWorkerId;
            temp.MachineId = vmRecipeHeader.MachineId;
            temp.OrderById = vmRecipeHeader.OrderById;
            temp.DocNo = vmRecipeHeader.DocNo;
            temp.Remark = vmRecipeHeader.Remark;
            temp.CreditDays = vmRecipeHeader.CreditDays;
            temp.ModifiedDate = DateTime.Now;
            temp.ModifiedBy = UserName;
            temp.ObjectState = Model.ObjectState.Modified;
            Update(temp);

            
            if (temp.StockHeaderId != null && temp.StockHeaderId != 0 )
            {
                StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find((int)temp.StockHeaderId);
                StockHeader.DocDate = temp.DocDate;
                StockHeader.DocNo = temp.DocNo;
                new StockHeaderService(_unitOfWork).Update(StockHeader);
            }
            




            LogList.Add(new LogTypeViewModel
            {
                ExObj = ExRec,
                Obj = temp,
            });


            JobOrderLine line = _JobOrderLineService.GetJobOrderLineListForHeader(vmRecipeHeader.JobOrderHeaderId).FirstOrDefault();

            line.JobOrderHeaderId = temp.JobOrderHeaderId;
            line.ProdOrderLineId = vmRecipeHeader.ProdOrderLineId;
            line.ProductId = vmRecipeHeader.ProductId;
            line.Dimension1Id = vmRecipeHeader.Dimension1Id;
            line.Dimension2Id = vmRecipeHeader.Dimension2Id;
            line.Qty = vmRecipeHeader.Qty;
            line.UnitId = vmRecipeHeader.UnitId;
            line.Sr = 1;
            line.LotNo = vmRecipeHeader.LotNo;
            line.LossQty = 0;
            line.NonCountedQty = 0;
            line.Rate = 0;
            line.DealQty = vmRecipeHeader.Qty;
            line.DealUnitId = vmRecipeHeader.UnitId;
            line.Amount = 0;
            line.UnitConversionMultiplier = 1;
            line.CreatedDate = DateTime.Now;
            line.ModifiedDate = DateTime.Now;
            line.CreatedBy = UserName;
            line.ModifiedBy = UserName;
            line.ObjectState = Model.ObjectState.Modified;

            _JobOrderLineService.Update(line);






            if (line.StockId != null)
            {
                StockViewModel StockViewModel = new StockViewModel();
                StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                StockViewModel.StockId = line.StockId ?? 0;
                StockViewModel.DocHeaderId = line.JobOrderHeaderId;
                StockViewModel.DocLineId = line.JobOrderLineId;
                StockViewModel.DocTypeId = temp.DocTypeId;
                StockViewModel.StockHeaderDocDate = temp.DocDate;
                StockViewModel.StockDocDate = temp.DocDate;
                StockViewModel.DocNo = temp.DocNo;
                StockViewModel.DivisionId = temp.DivisionId;
                StockViewModel.SiteId = temp.SiteId;
                StockViewModel.CurrencyId = null;
                StockViewModel.HeaderProcessId = temp.ProcessId;
                StockViewModel.PersonId = vmRecipeHeader.PersonId;
                StockViewModel.ProductId = line.ProductId;
                StockViewModel.HeaderFromGodownId = null;
                StockViewModel.HeaderGodownId = temp.GodownId;
                StockViewModel.GodownId = temp.GodownId ?? 0;
                StockViewModel.ProcessId = line.FromProcessId;
                StockViewModel.LotNo = line.LotNo;
                StockViewModel.CostCenterId = temp.CostCenterId;
                StockViewModel.Qty_Iss = line.Qty;
                StockViewModel.Qty_Rec = 0;
                StockViewModel.Rate = line.Rate;
                StockViewModel.ExpiryDate = null;
                StockViewModel.Specification = line.Specification;
                if (D.DocumentTypeShortName == "RCPQC" || D.DocumentTypeShortName == "SRCPE" || D.DocumentTypeShortName == "RDRCP")
                {
                    StockViewModel.Dimension1Id = vmRecipeHeader.Dimension1Id;
                    StockViewModel.Dimension2Id = vmRecipeHeader.Dimension2Id;
                }
                StockViewModel.Remark = line.Remark;
                StockViewModel.ProductUidId = line.ProductUidId;
                StockViewModel.Status = temp.Status;
                StockViewModel.CreatedBy = line.CreatedBy;
                StockViewModel.CreatedDate = line.CreatedDate;
                StockViewModel.ModifiedBy = UserName;
                StockViewModel.ModifiedDate = DateTime.Now;

                string StockPostingError = "";
                StockPostingError = _stockService.StockPostDB(ref StockViewModel);
            }


            if (line.StockProcessId != null)
            {
                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                StockProcessViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                StockProcessViewModel.StockProcessId = line.StockProcessId ?? 0;
                StockProcessViewModel.DocHeaderId = line.JobOrderHeaderId;
                StockProcessViewModel.DocLineId = line.JobOrderLineId;
                StockProcessViewModel.DocTypeId = temp.DocTypeId;
                StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                StockProcessViewModel.StockProcessDocDate = line.CreatedDate.Date;
                StockProcessViewModel.DocNo = temp.DocNo;
                StockProcessViewModel.DivisionId = temp.DivisionId;
                StockProcessViewModel.SiteId = temp.SiteId;
                StockProcessViewModel.CurrencyId = null;
                StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                StockProcessViewModel.PersonId = vmRecipeHeader.PersonId;
                StockProcessViewModel.ProductId = line.ProductId;
                StockProcessViewModel.HeaderFromGodownId = null;
                StockProcessViewModel.HeaderGodownId = temp.GodownId;
                StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                StockProcessViewModel.ProcessId = temp.ProcessId;
                StockProcessViewModel.LotNo = line.LotNo;
                StockProcessViewModel.CostCenterId = temp.CostCenterId;
                StockProcessViewModel.Qty_Iss = 0;
                StockProcessViewModel.Qty_Rec = line.Qty;
                StockProcessViewModel.Rate = line.Rate;
                StockProcessViewModel.ExpiryDate = null;
                StockProcessViewModel.Specification = line.Specification;
                StockProcessViewModel.Remark = line.Remark;
                StockProcessViewModel.ProductUidId = line.ProductUidId;
                StockProcessViewModel.Status = temp.Status;
                StockProcessViewModel.CreatedBy = line.CreatedBy;
                StockProcessViewModel.CreatedDate = line.CreatedDate;
                StockProcessViewModel.ModifiedBy = UserName;
                StockProcessViewModel.ModifiedDate = DateTime.Now;

                string StockProcessPostingError = "";
                StockProcessPostingError = _stockProcessService.StockProcessPostDB(ref StockProcessViewModel);

            }


            if (line.ProdOrderLineId.HasValue)
                _JobOrderLineService.UpdateProdQtyOnJobOrder(line.ProdOrderLineId.Value, line.JobOrderLineId, temp.DocDate, line.Qty);


            JobOrderLineExtended LineExtended = _JobOrderLineExtendedService.Find(line.JobOrderLineId);
            LineExtended.JobOrderLineId = line.JobOrderLineId;
            LineExtended.SubRecipeQty = vmRecipeHeader.SubRecipeQty;
            LineExtended.TestingQty = vmRecipeHeader.TestingQty;
            LineExtended.ObjectState = Model.ObjectState.Modified;
            _JobOrderLineExtendedService.Update(LineExtended);

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

            int? StockHeaderId = 0;

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Mapper.Map<JobOrderHeader>(JobOrderHeader),
            });

            StockHeaderId = JobOrderHeader.StockHeaderId;

            //Then find all the Job Order Header Line associated with the above ProductType.
            //var JobOrderLine = new JobOrderLineService(_unitOfWork).GetJobOrderLineforDelete(vm.id);
            var ProdOrderHeader = (_unitOfWork.Repository<ProdOrderHeader>().Query().Get().Where(m => m.ReferenceDocId == JobOrderHeader.JobOrderHeaderId && m.ReferenceDocTypeId == JobOrderHeader.DocTypeId)).FirstOrDefault();

            if (ProdOrderHeader != null)
            {
                var ProdOrderLine = (_unitOfWork.Repository<ProdOrderLine>().Query().Get().Where(m => m.ProdOrderHeaderId == ProdOrderHeader.ProdOrderHeaderId)).ToList();

                foreach (var item in ProdOrderLine)
                {
                    var Prodorderlinestatus = (_unitOfWork.Repository<ProdOrderLineStatus>().Query().Get().Where(m => m.ProdOrderLineId == item.ProdOrderLineId)).FirstOrDefault();
                    if (Prodorderlinestatus != null)
                    {
                        Prodorderlinestatus.ObjectState = Model.ObjectState.Deleted;
                        _unitOfWork.Repository<ProdOrderLineStatus>().Delete(Prodorderlinestatus);
                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    _unitOfWork.Repository<ProdOrderLine>().Delete(item);
                }

                ProdOrderHeader.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<ProdOrderHeader>().Delete(ProdOrderHeader);
            }

            var Stock = (_unitOfWork.Repository<Stock>().Query().Get().Where(m => m.StockHeaderId == StockHeaderId)).ToList();

            var StockAdj = (_unitOfWork.Repository<StockAdj>().Query().Get().Where(m => m.StockOut.StockHeaderId == StockHeaderId)).ToList();

            var StockLine = (_unitOfWork.Repository<StockLine>().Query().Get().Where(m => m.StockHeaderId == StockHeaderId)).ToList();

            var StockLineIds = StockLine.Select(m => m.StockLineId).ToArray();

            var StockLineExtendedRecords = _unitOfWork.Repository<StockLineExtended>().Query().Get().Where(m => StockLineIds.Contains(m.StockLineId)).ToList();

            var StockProcess = (_unitOfWork.Repository<StockProcess>().Query().Get().Where(m => m.StockHeaderId == StockHeaderId)).ToList();

            var JobOrderLine = (_unitOfWork.Repository<JobOrderLine>().Query().Get().Where(m => m.JobOrderHeaderId == vm.id)).ToList();

            var JOLineIds = JobOrderLine.Select(m => m.JobOrderLineId).ToArray();

            var JobOrderHeaderExtendedRecords = _unitOfWork.Repository<JobOrderHeaderExtended>().Query().Get().Where(m => m.JobOrderHeaderId == JobOrderHeader.JobOrderHeaderId).ToList();

            var JobOrderLineStatusRecords = _unitOfWork.Repository<JobOrderLineStatus>().Query().Get().Where(m => JOLineIds.Contains(m.JobOrderLineId ?? 0)).ToList();

            var JobOrderLineExtendedRecords = _unitOfWork.Repository<JobOrderLineExtended>().Query().Get().Where(m => JOLineIds.Contains(m.JobOrderLineId)).ToList();


            List<int> StockIdList = new List<int>();
            List<int> StockProcessIdList = new List<int>();

            DeleteProdQtyOnRecipeMultiple(JobOrderHeader.JobOrderHeaderId);

            foreach (var item in StockAdj)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<StockAdj>().Delete(item);
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

            foreach (var item in JobOrderHeaderExtendedRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderHeaderExtended>().Delete(item);
            }


            //Mark ObjectState.Delete to all the Job Order Lines. 
            foreach (var item in JobOrderLine)
            {

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobOrderLine>(item),
                });


                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderLine>().Delete(item);

            }

            foreach (var item in StockLineExtendedRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<StockLineExtended>().Delete(item);
            }

            foreach (var item in StockLine)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<StockLine>().Delete(item);

            }

            foreach (var item in Stock)
            {
                StockIdList.Add((int)item.StockId);
            }

            foreach (var item in StockProcess)
            {
                StockProcessIdList.Add((int)item.StockProcessId);
            }





            _stockService.DeleteStockMultiple(StockIdList);

            _stockProcessService.DeleteStockProcessDBMultiple(StockProcessIdList);



            var JobOrderHeaderStatus = _unitOfWork.Repository<JobOrderHeaderStatus>().Find(vm.id);

            JobOrderHeaderStatus.ObjectState = Model.ObjectState.Deleted;
            _unitOfWork.Repository<JobOrderHeaderStatus>().Delete(JobOrderHeaderStatus);

            // Now delete the Purhcase Order Header
            //_RecipeHeaderService.Delete(JobOrderHeader);

            int ReferenceDocId = JobOrderHeader.JobOrderHeaderId;
            int ReferenceDocTypeId = JobOrderHeader.DocTypeId;


            JobOrderHeader.ObjectState = Model.ObjectState.Deleted;
            Delete(JobOrderHeader);





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
            var pd = _RecipeRepository.Find(Id);

            pd.Status = (int)StatusConstants.Submitted;
            int ActivityType = (int)ActivityTypeContants.Submitted;


            pd.ReviewBy = null;
            pd.ObjectState = Model.ObjectState.Modified;
            _RecipeRepository.Update(pd);


            RecipeHeaderViewModel vmRecipeHeader = GetRecipeHeader(Id);
            if (vmRecipeHeader.SubRecipeQty > 0)
            {
                CreateProdOrder(Id, UserName, vmRecipeHeader.SubRecipeQty);
            }

            


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

        public void LogDetailInfo(RecipeHeaderViewModel vm)
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

            var temp = (from pr in _RecipeRepository.Instance
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

            var temp = (from pr in _RecipeRepository.Instance
                        where pr.DocNo == docno && pr.JobOrderHeaderId != headerid && (pr.DocTypeId == DocTypeId) && pr.SiteId == SiteId && pr.DivisionId == DivisionId
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        #endregion


        #region ProdOrderLineStatusUpdation

        void DeleteProdQtyOnRecipeMultiple(int id)
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

        public IQueryable<ComboBoxResult> GetProdOrderHelpListForProduct(int filter, string term)
        {

            int DocTypeId = filter;
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, CurrentDivisionId, CurrentSiteId);

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { ContraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }


            var list = (from p in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                        join t in _unitOfWork.Repository<ProdOrderHeader>().Instance on p.ProdOrderHeaderId equals t.ProdOrderHeaderId
                        join t2 in _unitOfWork.Repository<ProdOrderLine>().Instance on p.ProdOrderLineId equals t2.ProdOrderLineId
                        join pt in _unitOfWork.Repository <Product>().Instance on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in _unitOfWork.Repository <Dimension1>().Instance on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in _unitOfWork.Repository<Dimension2>().Instance on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower()))
                        && (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(p.DocTypeId.ToString()))
                        && p.SiteId == CurrentSiteId
                        && p.DivisionId == CurrentDivisionId
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = Dimension1Tab.Dimension1Name,
                            id = p.ProdOrderLineId.ToString(),
                            TextProp1 = "Order No: " + p.ProdOrderNo.ToString() + ", Party: " + t.Buyer.Code,
                            TextProp2 = "BalQty: " + p.BalanceQty.ToString(),
                            AProp1 = ProductTab.ProductName,
                            AProp2 = Dimension2Tab.Dimension2Name
                        });

            return list;
        }

        public ComboBoxResult GetProdOrderLine(int Ids)
        {
            var ProdOrderLine = (from L in _unitOfWork.Repository<ProdOrderLine>().Instance
                                join H in _unitOfWork.Repository<ProdOrderHeader>().Instance on L.ProdOrderHeaderId equals H.ProdOrderHeaderId into ProdOrderHeaderTable
                                from ProdOrderHeaderTab in ProdOrderHeaderTable.DefaultIfEmpty()
                                where L.ProdOrderLineId == Ids
                                 select new ComboBoxResult
                                {
                                    id = L.ProdOrderLineId.ToString(),
                                    text = L.Dimension1.Dimension1Name
                                }).FirstOrDefault();

            return ProdOrderLine;
        }

        public ProdOrderDetail GetProdOrderDetail(int ProdOrderLineId)
        {
            var temp = (from L in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                        join Dl in _unitOfWork.Repository<ProdOrderLine>().Instance on L.ProdOrderLineId equals Dl.ProdOrderLineId into ProdOrderLineTable
                        from ProdOrderLineTab in ProdOrderLineTable.DefaultIfEmpty()
                        where L.ProdOrderLineId == ProdOrderLineId
                        select new ProdOrderDetail
                        {
                            ProdOrderHeaderDocNo = L.ProdOrderNo,
                            ProductId = L.ProductId,
                            ProductName = L.Product.ProductName,
                            Dimension1Id = L.Dimension1Id,
                            Dimension1Name = L.Dimension1.Dimension1Name,
                            Dimension2Id = L.Dimension2Id,
                            Dimension2Name = L.Dimension2.Dimension2Name,
                            Qty = L.BalanceQty,
                            BalanceQty = L.BalanceQty,
                            UnitId = ProdOrderLineTab.Product.UnitId,
                            PersonId = ProdOrderLineTab.ProdOrderHeader.BuyerId,
                            PersonName = ProdOrderLineTab.ProdOrderHeader.Buyer.Name
                        }).FirstOrDefault();

            return temp;
        }

        public void CreateProdOrder(int RecipeHeaderId, string UserName, Decimal? SubRecipeQty)
        {
            RecipeHeaderViewModel vmRecipeHeader = GetRecipeHeader(RecipeHeaderId);
            var JobOrderLine = (_unitOfWork.Repository<JobOrderLine>().Query().Get().Where(m => m.JobOrderHeaderId == vmRecipeHeader.JobOrderHeaderId)).FirstOrDefault();

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(vmRecipeHeader.DocTypeId, vmRecipeHeader.DivisionId, vmRecipeHeader.SiteId);

            ProdOrderHeader ProdOrderHeader = new ProdOrderHeader();
            ProdOrderHeader.DocTypeId = (int)Settings.DocTypeProductionOrderId;
            ProdOrderHeader.DocDate = vmRecipeHeader.DocDate;
            ProdOrderHeader.DocNo = vmRecipeHeader.DocNo;
            ProdOrderHeader.DivisionId = vmRecipeHeader.DivisionId;
            ProdOrderHeader.SiteId = vmRecipeHeader.SiteId;
            ProdOrderHeader.DueDate = vmRecipeHeader.DueDate;
            ProdOrderHeader.ReferenceDocTypeId =vmRecipeHeader.DocTypeId;
            ProdOrderHeader.ReferenceDocId =vmRecipeHeader.JobOrderHeaderId;
            ProdOrderHeader.Status =vmRecipeHeader.DocTypeId;
            ProdOrderHeader.Remark = vmRecipeHeader.Remark;
            ProdOrderHeader.BuyerId = vmRecipeHeader.PersonId;
            ProdOrderHeader.CreatedBy = UserName;
            ProdOrderHeader.CreatedDate = DateTime.Now;
            ProdOrderHeader.ModifiedBy = UserName;
            ProdOrderHeader.ModifiedDate = DateTime.Now;
            ProdOrderHeader.LockReason = "Prod order automatically generated from recipe.";
            ProdOrderHeader.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<ProdOrderHeader>().Add(ProdOrderHeader);


            ProdOrderHeaderStatus ProdOrderHeaderStatus = new ProdOrderHeaderStatus();
            ProdOrderHeaderStatus.ProdOrderHeaderId = ProdOrderHeader.ProdOrderHeaderId;
            _unitOfWork.Repository<ProdOrderHeaderStatus>().Add(ProdOrderHeaderStatus);


            ProdOrderLine ProdOrderLine = new ProdOrderLine();
            ProdOrderLine.ProdOrderHeaderId = ProdOrderHeader.ProdOrderHeaderId;
            ProdOrderLine.ProductId = vmRecipeHeader.ProductId;
            ProdOrderLine.Dimension1Id = vmRecipeHeader.Dimension1Id;
            ProdOrderLine.Dimension2Id = vmRecipeHeader.Dimension2Id;
            ProdOrderLine.Specification = null;
            ProdOrderLine.ProcessId = vmRecipeHeader.ProcessId;
            ProdOrderLine.ReferenceDocTypeId = vmRecipeHeader.DocTypeId;
            ProdOrderLine.ReferenceDocLineId = JobOrderLine.JobOrderLineId;
            ProdOrderLine.Sr = 1;
            ProdOrderLine.Qty = SubRecipeQty ?? 0;
            ProdOrderLine.Remark = vmRecipeHeader.Remark;
            ProdOrderLine.CreatedBy = UserName;
            ProdOrderLine.ModifiedBy = UserName;
            ProdOrderLine.CreatedDate = DateTime.Now;
            ProdOrderLine.ModifiedDate = DateTime.Now;
            ProdOrderLine.LockReason = "Prod order automatically generated from recipe.";
            ProdOrderLine.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<ProdOrderLine>().Add(ProdOrderLine);

            ProdOrderLineStatus ProdOrderLineStatus = new ProdOrderLineStatus();
            ProdOrderLineStatus.ProdOrderLineId = ProdOrderLine.ProdOrderLineId;
            ProdOrderLineStatus.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<ProdOrderLineStatus>().Add(ProdOrderLineStatus);
        }

        public LastValues GetLastValues(int DocTypeId)
        {
            var temp = (from H in _unitOfWork.Repository<JobOrderHeader>().Instance
                        join L in _unitOfWork.Repository<JobOrderLine>().Instance on H.JobOrderHeaderId equals L.JobOrderHeaderId into JobOrderLineTable
                        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                        join Le in _unitOfWork.Repository<JobOrderLineExtended>().Instance  on JobOrderLineTab.JobOrderLineId equals Le.JobOrderLineId into JobOrderLineExtendedTable
                        from JobOrderLineExtendedTab in JobOrderLineExtendedTable.DefaultIfEmpty()
                        where H.DocTypeId == DocTypeId
                        orderby H.JobOrderHeaderId descending
                        select new LastValues
                        {
                            GodownId = H.GodownId,
                            JobWorkerId = H.JobWorkerId,
                            OrderById = H.OrderById,
                            TestingQty = JobOrderLineExtendedTab.TestingQty
                        }).FirstOrDefault();

            return temp;
        }
    }

    public class ProdOrderDetail
    {
        public string ProdOrderHeaderDocNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public Decimal? Qty { get; set; }
        public Decimal? BalanceQty { get; set; }
        public string UnitId { get; set; }
        public int? PersonId { get; set; }
        public string PersonName { get; set; }
    }

    public class StockInDetail
    {
        public int? StockInId { get; set; }
    }
}


