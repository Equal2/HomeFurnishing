using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.Reports.Models;
using Infrastructure.IO;
using Components.Logging;
using Models.Reports.ViewModels;
using AutoMapper;
using Services.BasicSetup;
using ProjLib.DocumentConstants;
using Models.BasicSetup.ViewModels;
using System.Xml.Linq;

namespace Service
{
    public interface IReportLineService : IDisposable
    {
        ReportLine Create(ReportLine pt);
        ReportLineViewModel Create(ReportLineViewModel pt, string UserName);
        void Delete(int id);
        void Delete(ReportLine pt);
        void Delete(ReportLineViewModel pt, string UserName);
        ReportLine Find(int id);
        void Update(ReportLine pt);
        void Update(ReportLineViewModel pt, string UserName);
        ReportLine Add(ReportLine pt);
        IEnumerable<ReportLineViewModel> GetReportLineList(int id);
        ReportLine GetReportLine(int id);
        ReportLineViewModel GetReportLineViewModel(int id);
        ReportLine GetReportLineByName(string Name, int HeaderID);
        void CopyReport(ReportCopyViewModel vm, string UserName);
    }

    public class ReportLineService : IReportLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IDocumentTypeService _DocumentTypeService;
        private readonly IModificationCheck _modificationCheck;

        IRepository<ReportLine> _ReportLineRepository;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public ReportLineService(IUnitOfWork unitOfWork, IRepository<ReportLine> ReportLineRepo, ILogger log,
            IDocumentTypeService DoctypeServ, IModificationCheck modificationcheck)
        {
            _unitOfWork = unitOfWork;
            _ReportLineRepository = ReportLineRepo;
            _logger = log;
            _DocumentTypeService = DoctypeServ;
            _modificationCheck = modificationcheck;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }
        public ReportLine Find(int id)
        {
            return _unitOfWork.Repository<ReportLine>().Find(id);
        }

        public ReportLine Create(ReportLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ReportLine>().Insert(pt);
            return pt;
        }

        public ReportLineViewModel Create(ReportLineViewModel pt, string UserName)
        {
            ReportLine obj = Mapper.Map<ReportLine>(pt);

            obj.CreatedDate = DateTime.Now;
            obj.ModifiedDate = DateTime.Now;
            obj.CreatedBy = UserName;
            obj.ModifiedBy = UserName;
            obj.ObjectState = Model.ObjectState.Added;
            Create(obj);

            _unitOfWork.Save();

            pt.ReportLineId = obj.ReportLineId;

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = _DocumentTypeService.Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = pt.ReportHeaderId,
                DocLineId = pt.ReportLineId,
                ActivityType = (int)ActivityTypeContants.Added,
            }));

            return pt;

        }
        public ReportLine GetReportLine(int id)
        {
            return ((from p in _ReportLineRepository.Instance
                     where p.ReportLineId == id
                     select p).FirstOrDefault()
                        );
        }

        public ReportLineViewModel GetReportLineViewModel(int id)
        {
            var obj = Find(id);
            return Mapper.Map<ReportLineViewModel>(obj);
        }
        public ReportLine GetReportLineByName(string Name, int HeaderID)
        {
            return (from p in _ReportLineRepository.Instance
                    where p.ReportHeaderId == HeaderID && p.FieldName == Name
                    select p
                        ).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ReportLine>().Delete(id);
        }

        public void Delete(ReportLine pt)
        {
            _unitOfWork.Repository<ReportLine>().Delete(pt);
        }

        public void Delete(ReportLineViewModel pt, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var obj = Find(pt.ReportLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = obj,
            });

            Delete(obj);
            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = _DocumentTypeService.Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = pt.ReportHeaderId,
                DocLineId = pt.ReportLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                xEModifications = Modifications,
            }));

        }

        public void Update(ReportLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ReportLine>().Update(pt);
        }

        public void Update(ReportLineViewModel pt, string UserName)
        {
            var obj = Find(pt.ReportLineId);

            obj.FieldName = pt.FieldName;
            obj.DataType = pt.DataType;
            obj.Type = pt.Type;
            obj.ServiceFuncGet = pt.ServiceFuncGet;
            obj.ServiceFuncSet = pt.ServiceFuncSet;
            obj.CacheKey = pt.CacheKey;
            obj.Serial = pt.Serial;
            obj.IsVisible = pt.IsVisible;

            obj.ModifiedDate = DateTime.Now;
            obj.ModifiedBy = UserName;
            obj.ObjectState = Model.ObjectState.Modified;

            Update(obj);

            _unitOfWork.Save();


            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = _DocumentTypeService.Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = pt.ReportHeaderId,
                DocLineId = pt.ReportLineId,
                ActivityType = (int)ActivityTypeContants.Modified,
            }));

        }

        public IEnumerable<ReportLineViewModel> GetReportLineList(int id)
        {
            var pt = _unitOfWork.Repository<ReportLine>().Query().Get().Where(m => m.ReportHeaderId == id).OrderBy(m => m.Serial).Select(m => new
            ReportLineViewModel
            {

                CacheKey = m.CacheKey,
                DataType = m.DataType,
                DefaultValue = m.DefaultValue,
                DisplayName = m.DisplayName,
                FieldName = m.FieldName,
                IsCollapse = m.IsCollapse,
                IsMandatory = m.IsMandatory,
                IsVisible = m.IsVisible,
                ListItem = m.ListItem,
                NoOfCharToEnter = m.NoOfCharToEnter,
                PlaceHolder = m.PlaceHolder,
                ReportHeaderId = m.ReportHeaderId,
                ReportLineId = m.ReportLineId,
                Serial = m.Serial,
                ServiceFuncGet = m.ServiceFuncGet,
                ServiceFuncSet = m.ServiceFuncSet,
                SqlParameter = m.SqlParameter,
                SqlProcGet = m.SqlProcGet,
                SqlProcGetSet = m.SqlProcGetSet,
                SqlProcSet = m.SqlProcSet,
                ToolTip = m.ToolTip,
                Type = m.Type,
            });

            return pt;
        }

        public ReportLine Add(ReportLine pt)
        {
            _unitOfWork.Repository<ReportLine>().Insert(pt);
            return pt;
        }


        public void CopyReport(ReportCopyViewModel vm, string UserName)
        {
            List<ReportLineViewModel> temp = GetReportLineList(vm.CopyHeaderId).ToList();

            foreach (var item in temp)
            {
                ReportLine line = new ReportLine();
                line.ReportHeaderId = vm.ReportHeaderId;
                line.DataType = item.DataType;
                line.Type = item.Type;
                line.ServiceFuncGet = item.ServiceFuncGet;
                line.ServiceFuncSet = item.ServiceFuncSet;
                line.CacheKey = item.CacheKey;
                line.Serial = item.Serial;
                line.FieldName = item.FieldName;
                line.DisplayName = item.DisplayName;
                line.NoOfCharToEnter = item.NoOfCharToEnter;
                line.CreatedBy = UserName;
                line.CreatedDate = DateTime.Now;
                line.ModifiedBy = UserName;
                line.ModifiedDate = DateTime.Now;

                Create(line);
            }
            _unitOfWork.Save();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
