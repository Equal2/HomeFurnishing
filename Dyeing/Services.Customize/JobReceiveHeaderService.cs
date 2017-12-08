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
    public interface IJobReceiveHeaderService : IDisposable
    {
        JobReceiveHeader Create(JobReceiveHeader s);
        void Delete(int id);
        void Delete(JobReceiveHeader s);
        void Delete(ReasonViewModel vm, string UserName);
        JobReceiveHeader Find(int id);
        void Update(JobReceiveHeader s);
        string GetMaxDocNo();
        int NextPrevId(int DocId, int DocTypeId, string UserName, string PrevNextConstants);

        #region Helper Methods
        IQueryable<UnitConversionFor> GetUnitConversionForList();
        _Menu GetMenu(int Id);
        _Menu GetMenu(string Name);
        _ReportHeader GetReportHeader(string MenuName);
        _ReportLine GetReportLine(string Name, int ReportHeaderId);
        bool CheckForDocNoExists(string docno, int DocTypeId);
        bool CheckForDocNoExists(string docno, int headerid, int DocTypeId);
        #endregion

    }
    public class JobReceiveHeaderService : IJobReceiveHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobReceiveHeader> _JobReceiveRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Unit> _unitRepository;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        private readonly IStockService _stockService;
        private readonly IStockProcessService _stockProcessService;

        private ActiivtyLogViewModel logVm = new ActiivtyLogViewModel();

        public JobReceiveHeaderService(IUnitOfWork unit, IRepository<JobReceiveHeader> JobReceiveRepo,
            IStockService StockServ, IStockProcessService StockPRocServ,
            ILogger log, IModificationCheck modificationCheck, IRepository<Product> ProductRepo, IRepository<Unit> UnitRepo)
        {
            _unitOfWork = unit;
            _JobReceiveRepository = JobReceiveRepo;
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



        public void Delete(ReasonViewModel vm, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var JobReceiveHeader = Find(vm.id);
            GatePassHeader GatePassHEader = new GatePassHeader();

            
            int? StockHeaderId = 0;

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Mapper.Map<JobReceiveHeader>(JobReceiveHeader),
            });

            StockHeaderId = JobReceiveHeader.StockHeaderId;

            //Then find all the Purchase Order Header Line associated with the above ProductType.
            //var JobReceiveLine = new JobReceiveLineService(_unitOfWork).GetJobReceiveLineforDelete(vm.id);
            var JobReceiveLine = (_unitOfWork.Repository<JobReceiveLine>().Query().Get().Where(m => m.JobReceiveHeaderId == vm.id)).ToList();

            var JOLineIds = JobReceiveLine.Select(m => m.JobReceiveLineId).ToArray();

            var JobReceiveLineStatusRecords = _unitOfWork.Repository<JobReceiveLineStatus>().Query().Get().Where(m => JOLineIds.Contains(m.JobReceiveLineId ?? 0)).ToList();

            var ProductUids = JobReceiveLine.Select(m => m.ProductUidId).ToArray();

            var BarCodeRecords = _unitOfWork.Repository<ProductUid>().Query().Get().Where(m => ProductUids.Contains(m.ProductUIDId)).ToList();


            List<int> StockIdList = new List<int>();
            List<int> StockProcessIdList = new List<int>();

            foreach (var item in JobReceiveLineStatusRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobReceiveLineStatus>().Delete(item);
            }


            //Mark ObjectState.Delete to all the Purchase Order Lines. 
            foreach (var item in JobReceiveLine)
            {

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveLine>(item),
                });


                if (item.StockId != null)
                {
                    StockIdList.Add((int)item.StockId);
                }

                if (item.StockProcessId != null)
                {
                    StockProcessIdList.Add((int)item.StockProcessId);
                }

                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobReceiveLine>().Delete(item);

            }


            _stockService.DeleteStockMultiple(StockIdList);

            _stockProcessService.DeleteStockProcessDBMultiple(StockProcessIdList);


            // Now delete the Purhcase Order Header
            //_JobReceiveHeaderService.Delete(JobReceiveHeader);

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




        public int NextPrevId(int DocId, int DocTypeId, string UserName, string PrevNext)
        {
            return new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, UserName, "", "Web.JobReceiveHeaders", "JobReceiveHeaderId", PrevNext);
        }


        #region Helper Methods

        public IQueryable<UnitConversionFor> GetUnitConversionForList()
        {
            return _unitOfWork.Repository<UnitConversionFor>().Query().Get();
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

            var temp = (from pr in _JobReceiveRepository.Instance
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

            var temp = (from pr in _JobReceiveRepository.Instance
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
    }
}
