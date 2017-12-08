using System.Collections.Generic;
using System.Linq;
using System;
using Models.Reports.Models;
using Infrastructure.IO;
using Models.Reports.DatabaseViews;
using Components.Logging;
using Models.Reports.ViewModels;
using AutoMapper;
using ProjLib.DocumentConstants;
using Services.BasicSetup;
using System.Xml.Linq;
using Models.BasicSetup.ViewModels;

namespace Service
{
    public interface IReportHeaderService : IDisposable
    {
        ReportHeader Create(ReportHeader pt);
        ReportHeaderViewModel Create(ReportHeaderViewModel pt, string UserName);
        void Delete(int id);
        void Delete(ReportHeader pt);
        void Delete(ReasonViewModel vm, string UserName);
        ReportHeader Find(int id);
        void Update(ReportHeader pt);
        void Update(ReportHeaderViewModel pt, string UserName);
        ReportHeader Add(ReportHeader pt);
        IEnumerable<ReportHeaderViewModel> GetReportHeaderList();
        ReportHeader GetReportHeader(int id);
        ReportHeaderViewModel GetReportHeaderViewModel(int id);
        ReportHeader GetReportHeaderByName(string name);
        IEnumerable<ReportHeader> GetReportHeaderListForCopy(int id);
        List<string> GetSubReportProcList(int id);
        string GetMenuName(int Id);
        string GetReportNameFromProcedure(string Query);
    }

    public class ReportHeaderService : IReportHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ReportHeader> _ReportHeaderRepository;
        private readonly ILogger _logger;
        private readonly IDocumentTypeService _DocumentTypeService;
        private readonly IReportLineService _reportLineService;
        private readonly IModificationCheck _modificationCheck;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public ReportHeaderService(IUnitOfWork unitOfWork, IRepository<ReportHeader> ReportHeaderRepo, ILogger log, IDocumentTypeService DoctypeServ
            , IReportLineService reportlineServ, IModificationCheck modificationcheck)
        {
            _unitOfWork = unitOfWork;
            _ReportHeaderRepository = ReportHeaderRepo;
            _logger = log;
            _DocumentTypeService = DoctypeServ;
            _reportLineService = reportlineServ;
            _modificationCheck = modificationcheck;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ReportHeader Find(int id)
        {
            return _unitOfWork.Repository<ReportHeader>().Find(id);
        }

        public ReportHeader Create(ReportHeader pt)
        {
            pt.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<ReportHeader>().Insert(pt);
            return pt;
        }

        public ReportHeaderViewModel Create(ReportHeaderViewModel pt, string UserName)
        {
            ReportHeader obj = Mapper.Map<ReportHeader>(pt);
            obj.CreatedDate = DateTime.Now;
            obj.ModifiedDate = DateTime.Now;
            obj.CreatedBy = UserName;
            obj.ModifiedBy = UserName;
            obj.ObjectState = Model.ObjectState.Added;
            Create(obj);

            _unitOfWork.Save();

            pt.ReportHeaderId = obj.ReportHeaderId;


            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = _DocumentTypeService.FindByName(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = pt.ReportHeaderId,
                ActivityType = (int)ActivityTypeContants.Added,
            }));

            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ReportHeader>().Delete(id);
        }

        public void Delete(ReportHeader pt)
        {
            _unitOfWork.Repository<ReportHeader>().Delete(pt);
        }

        public void Delete(ReasonViewModel vm, string UserName)
        {

            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var temp = Find(vm.id);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = temp,
            });

            var line = _reportLineService.GetReportLineList(vm.id).ToList();


            foreach (var item in line)
            {

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = item,
                });


                _reportLineService.Delete(item.ReportLineId);
            }

            Delete(temp);

            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = _DocumentTypeService.FindByName(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = temp.ReportHeaderId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                UserRemark = vm.Reason,
                xEModifications = Modifications,
            }));
        }

        public ReportHeader GetReportHeader(int id)
        {
            return ((from p in _ReportHeaderRepository.Instance
                     where p.ReportHeaderId == id
                     select p
                         ).FirstOrDefault());
        }

        public ReportHeaderViewModel GetReportHeaderViewModel(int id)
        {
            var obj = Find(id);
            return Mapper.Map<ReportHeaderViewModel>(obj);
        }

        public ReportHeader GetReportHeaderByName(string name)
        {
            return ((from p in _ReportHeaderRepository.Instance
                     where p.ReportName == name
                     select p
                         ).FirstOrDefault());
        }

        public void Update(ReportHeader pt)
        {
            pt.ObjectState = Model.ObjectState.Modified;
            _unitOfWork.Repository<ReportHeader>().Update(pt);
        }
        public void Update(ReportHeaderViewModel pt, string UserName)
        {
            var rh = Find(pt.ReportHeaderId);

            rh.Controller = pt.Controller;
            rh.Action = pt.Action;
            rh.SqlProc = pt.SqlProc;
            rh.ModifiedDate = DateTime.Now;
            rh.ModifiedBy = UserName;
            rh.ObjectState = Model.ObjectState.Modified;
            Update(rh);

            _unitOfWork.Save();


            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = _DocumentTypeService.FindByName(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = pt.ReportHeaderId,
                ActivityType = (int)ActivityTypeContants.Modified,
            }));
        }

        public IEnumerable<ReportHeaderViewModel> GetReportHeaderList()
        {
            var pt = _unitOfWork.Repository<ReportHeader>().Query().Get().Select(m => new ReportHeaderViewModel
            {
                Action = m.Action,
                Controller = m.Controller,
                Notes = m.Notes,
                ParentReportHeaderId = m.ParentReportHeaderId,
                ReportHeaderId = m.ReportHeaderId,
                ReportName = m.ReportName,
                ReportSQL = m.ReportSQL,
                SqlProc = m.SqlProc,
            });

            return pt;
        }

        public IEnumerable<ReportHeader> GetReportHeaderListForCopy(int id)
        {
            var pt = _unitOfWork.Repository<ReportHeader>().Query().Get().Where(m => m.ReportHeaderId != id);

            return pt;
        }

        public ReportHeader Add(ReportHeader pt)
        {
            _unitOfWork.Repository<ReportHeader>().Insert(pt);
            return pt;
        }

        public List<string> GetSubReportProcList(int id)
        {
            var pt = _unitOfWork.Repository<ReportHeader>().Query().Get().Where(m => m.ParentReportHeaderId == id).Select(m => m.SqlProc).ToList();

            return pt;
        }

        public string GetMenuName(int Id)
        {
            return _unitOfWork.Repository<_Menus>().Find(Id).MenuName;
        }


        public string GetReportNameFromProcedure(string Query)
        {
            return _unitOfWork.SqlQuery<string>(Query).FirstOrDefault();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
