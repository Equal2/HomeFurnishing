using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Data.SqlClient;
using System.Configuration;
using Infrastructure.IO;
using Models.Customize.Models;
using Models.Customize.ViewModels;
using Models.BasicSetup.ViewModels;
using Models.BasicSetup.Models;
using Models.Customize.DataBaseViews;
using Models.Company.Models;
using Components.Logging;
using ProjLib.Constants;
using AutoMapper;
using Models.Company.ViewModels;
using Services.BasicSetup;
using ProjLib;
using System.Xml.Linq;

namespace Services.Customize
{
    public interface IJobReceiveLineService : IDisposable
    {
        JobReceiveLine Create(JobReceiveLine s);
        void Delete(int id);
        void Delete(JobReceiveLine s);
        JobReceiveLine Find(int id);
        void Update(JobReceiveLine s);
        int GetMaxSr(int id);
        IEnumerable<JobReceiveLine> GetJobReceiveLineListForHeader(int HeaderId);


        #region Helper Methods
        IEnumerable<Unit> GetUnitList();
        Unit FindUnit(string UnitId);
        ProductViewModel GetProduct(int ProductId);
        UnitConversion GetUnitConversion(int Id, string UnitId, int conversionForId, string DealUnitId);
        ProductUid FindProductUid(int Id);
        ProductUid FindProductUid(string Name);

        #endregion
    }

    public class JobReceiveLineService : IJobReceiveLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobReceiveLine> _JobReceiveLineRepository;
        private readonly IRepository<JobReceiveHeader> _JobReceiveHeaderRepository;
        private readonly IStockService _stockService;
        private readonly IStockProcessService _stockProcessService;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        //private readonly IJobReceiveSettingsService _jobreceiveSettingsService;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public JobReceiveLineService(IUnitOfWork unitOfWork, IRepository<JobReceiveLine> JobReceiveLineRepo, IRepository<JobReceiveHeader> JobReceiveHeaderRepo,
             ILogger log
            , IStockService stockSErv, IStockProcessService stockProcServ,
            IModificationCheck modificationCheck
            //, IJobReceiveSettingsService JobReceiveSettingsServ
            )
        {
            _unitOfWork = unitOfWork;
            _JobReceiveLineRepository = JobReceiveLineRepo;
            _JobReceiveHeaderRepository = JobReceiveHeaderRepo;
            _stockProcessService = stockProcServ;
            _stockService = stockSErv;
            _modificationCheck = modificationCheck;
            _logger = log;
            // _jobreceiveSettingsService = JobReceiveSettingsServ;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public JobReceiveLine Create(JobReceiveLine S)
        {
            S.ObjectState = ObjectState.Added;
            _JobReceiveLineRepository.Add(S);
            return S;
        }

        public void Delete(int id)
        {
            _JobReceiveLineRepository.Delete(id);
        }

        public void Delete(JobReceiveLine s)
        {
            _JobReceiveLineRepository.Delete(s);
        }

        public void Update(JobReceiveLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _JobReceiveLineRepository.Update(s);
        }


        public JobReceiveLine Find(int id)
        {
            return _JobReceiveLineRepository.Find(id);
        }






        public int GetMaxSr(int id)
        {
            var Max = (from p in _JobReceiveLineRepository.Instance
                       where p.JobReceiveHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }







        public void Dispose()
        {
            _unitOfWork.Dispose();
        }



        public IEnumerable<JobReceiveLine> GetJobReceiveLineListForHeader(int HeaderId)
        {
            return (from p in _JobReceiveLineRepository.Instance
                    where p.JobReceiveHeaderId == HeaderId
                    select p);
        }


        #region Helper Methods

        public IEnumerable<Unit> GetUnitList()
        {
            return new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        public ProductViewModel GetProduct(int ProductId)
        {
            return new ProductService(_unitOfWork).GetProduct(ProductId);
        }

        public Unit FindUnit(string UnitId)
        {
            return new UnitService(_unitOfWork).Find(UnitId);
        }

        public UnitConversion GetUnitConversion(int Id, string UnitId, int conversionForId, string DealUnitId)
        {
            return new UnitConversionService(_unitOfWork).GetUnitConversion(Id, UnitId, conversionForId, DealUnitId);
        }

        public ProductUid FindProductUid(int Id)
        {
            return new ProductUidService(_unitOfWork).Find(Id);
        }

        public ProductUid FindProductUid(string Name)
        {
            return new ProductUidService(_unitOfWork).Find(Name);
        }

        public bool CheckProductUidProcessDone(string ProductUidName, int ProcessID)
        {
            return new ProductUidService(_unitOfWork).IsProcessDone(ProductUidName, ProcessID);
        }


        #endregion



    }
    
}