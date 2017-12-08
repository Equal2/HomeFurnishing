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
    public interface IRecipeLineService : IDisposable
    {
        StockLine Create(StockLine s);
        RecipeLineViewModel Create(RecipeLineViewModel svm, string UserName);
        void Delete(int id);
        void Delete(StockLine s);
        void Delete(RecipeLineViewModel vm, string UserName);
        RecipeLineViewModel GetStockLine(int id);
        StockLine Find(int id);
        void Update(StockLine s);
        void Update(RecipeLineViewModel svm, string UserName);
        IQueryable<RecipeLineViewModel> GetStockLineListForIndex(int StockHeaderId);
        IEnumerable<RecipeLineViewModel> GetStockLineforDelete(int headerid);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
        IEnumerable<StockLine> GetStockLineListForHeader(int HeaderId);
        IEnumerable<Dimension1RecipeViewModel> GetRecipes(int Dimension1Id);
        IEnumerable<RecipeDetailViewModel> GetRecipeDetail(int JobOrderHeaderId);
        LastValues GetLastValues(int JobOrderHeaderId);
        bool IsDuplicateLine(int StockHeaderId, int ProductId, Decimal? DyeingRatio, int? StockLineId);
        decimal GetExcessStock(int ProductId, int? Dim1, int? Dim2, int? ProcId, string Lot, int StockHeaderId, string ProcName);

        #region Helper Methods
        ProductViewModel GetProduct(int ProductId);

        #endregion
    }

    public class RecipeLineService : IRecipeLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<StockLine> _StockLineRepository;
        private readonly IRepository<StockHeader> _StockHeaderRepository;
        private readonly IStockLineExtendedService _StockLineExtendedService;
        private readonly IStockService _stockService;
        private readonly IStockProcessService _stockProcessService;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        //private readonly IJobReceiveSettingsService _jobreceiveSettingsService;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public RecipeLineService(IUnitOfWork unitOfWork, IRepository<StockLine> StockLineRepo, IRepository<StockHeader> StockHeaderRepo,
             ILogger log
            , IStockLineExtendedService StockLineExtendedService
            , IStockService stockSErv, IStockProcessService stockProcServ,
            IModificationCheck modificationCheck
            //, IJobReceiveSettingsService JobReceiveSettingsServ
            )
        {
            _unitOfWork = unitOfWork;
            _StockLineRepository = StockLineRepo;
            _StockHeaderRepository = StockHeaderRepo;
            _stockProcessService = stockProcServ;
            _StockLineExtendedService = StockLineExtendedService;
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

        public StockLine Create(StockLine S)
        {
            S.ObjectState = ObjectState.Added;
            _StockLineRepository.Add(S);
            return S;
        }

        public void Delete(int id)
        {
            _StockLineRepository.Delete(id);
        }

        public void Delete(StockLine s)
        {
            _StockLineRepository.Delete(s);
        }

        public void Update(StockLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _StockLineRepository.Update(s);
        }

        public ProductViewModel GetProduct(int ProductId)
        {
            return new ProductService(_unitOfWork).GetProduct(ProductId);
        }


        public RecipeLineViewModel GetStockLine(int id)
        {
            var temp = (from p in _StockLineRepository.Instance
                        join H  in _unitOfWork.Repository<JobOrderHeader>().Instance on p.StockHeaderId equals H.StockHeaderId into JobOrderHeaderTable from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                        join Se in _unitOfWork.Repository<StockLineExtended>().Instance on p.StockLineId equals Se.StockLineId into StockLineExtendedTable
                        from StockLineExtendedTab in StockLineExtendedTable.DefaultIfEmpty()
                        where p.StockLineId == id
                        select new RecipeLineViewModel
                        {

                            ProductId = p.ProductId,
                            Qty = p.Qty,
                            Remark = p.Remark,
                            StockHeaderId = p.StockHeaderId,
                            JobOrderHeaderId = JobOrderHeaderTab.JobOrderHeaderId,
                            StockLineId = p.StockLineId,
                            ProductName = p.Product.ProductName,
                            LockReason = p.LockReason,
                            DyeingRatio = StockLineExtendedTab.DyeingRatio,
                            TestingQty = StockLineExtendedTab.TestingQty,
                            DocQty = StockLineExtendedTab.DocQty,
                            ExcessQty = StockLineExtendedTab.ExcessQty,
                            Rate = p.Rate,
                            Amount = p.Amount,
                            UnitId = p.Product.UnitId,
                            UnitName = p.Product.Unit.UnitName,
                            UnitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                        }).FirstOrDefault();



            return temp;
        }
        public StockLine Find(int id)
        {
            return _StockLineRepository.Find(id);
        }



        public IQueryable<RecipeLineViewModel> GetStockLineListForIndex(int StockHeaderId)
        {
            var temp = from p in _StockLineRepository.Instance
                       join t in _unitOfWork.Repository<Dimension1>().Instance on p.Dimension1Id equals t.Dimension1Id into table
                       from Dim1 in table.DefaultIfEmpty()
                       join t1 in _unitOfWork.Repository<Dimension2>().Instance on p.Dimension2Id equals t1.Dimension2Id into table1
                       from Dim2 in table1.DefaultIfEmpty()
                       join Se in _unitOfWork.Repository<StockLineExtended>().Instance on p.StockLineId equals Se.StockLineId into StockLineExtendedTable
                       from StockLineExtendedTab in StockLineExtendedTable.DefaultIfEmpty()
                       where p.StockHeaderId == StockHeaderId
                       orderby p.Sr
                       select new RecipeLineViewModel
                       {
                           Rate = p.Rate,
                           Amount = p.Amount,
                           UnitId = p.Product.UnitId,
                           UnitName = p.Product.Unit.UnitName,
                           UnitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                           Remark = p.Remark,
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           Qty = p.Qty,
                           DyeingRatio = StockLineExtendedTab.DyeingRatio,
                           TestingQty = StockLineExtendedTab.TestingQty,
                           DocQty = StockLineExtendedTab.DocQty,
                           ExcessQty = StockLineExtendedTab.ExcessQty,
                           StockHeaderId = p.StockHeaderId,
                           StockLineId = p.StockLineId,
                       };
            return temp;
        }

        public IEnumerable<RecipeLineViewModel> GetStockLineforDelete(int headerid)
        {
            return (from p in _StockLineRepository.Instance
                    where p.StockHeaderId == headerid
                    select new RecipeLineViewModel
                    {
                        StockLineId = p.StockLineId,
                        StockId = p.StockId,
                        StockProcessId = p.StockProcessId,
                    }
                        );
        }


        
        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var Stock = _StockHeaderRepository.Find(Id);


            var list = (from p in _unitOfWork.Repository<Product>().Instance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        group new { p } by p.ProductId into g
                        select new ComboBoxList
                        {
                            PropFirst = g.Max(m => m.p.ProductName),
                            Id = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in _StockLineRepository.Instance
                       where p.StockHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }



        public RecipeLineViewModel Create(RecipeLineViewModel svm, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            StockHeader temp = _StockHeaderRepository.Find(svm.StockHeaderId);

            StockLine s = Mapper.Map<RecipeLineViewModel, StockLine>(svm);

            StockViewModel StockViewModel = new StockViewModel();
            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
            
            //Posting in Stock
            StockViewModel.StockHeaderId = temp.StockHeaderId;
            StockViewModel.DocHeaderId = temp.StockHeaderId;
            StockViewModel.DocLineId = s.StockLineId;
            StockViewModel.DocTypeId = temp.DocTypeId;
            StockViewModel.StockHeaderDocDate = temp.DocDate;
            StockViewModel.StockDocDate = DateTime.Now.Date;
            StockViewModel.DocNo = temp.DocNo;
            StockViewModel.DivisionId = temp.DivisionId;
            StockViewModel.SiteId = temp.SiteId;
            StockViewModel.CurrencyId = null;
            StockViewModel.HeaderProcessId = null;
            StockViewModel.PersonId = temp.PersonId;
            StockViewModel.ProductId = s.ProductId;
            StockViewModel.HeaderFromGodownId = null;
            StockViewModel.HeaderGodownId = temp.GodownId;
            StockViewModel.GodownId = temp.GodownId ?? 0;
            StockViewModel.ProcessId = s.FromProcessId;
            StockViewModel.LotNo = s.LotNo;
            StockViewModel.CostCenterId = temp.CostCenterId;
            StockViewModel.Qty_Iss = s.Qty;
            StockViewModel.Qty_Rec = 0;
            StockViewModel.Rate = s.Rate;
            StockViewModel.ExpiryDate = null;
            StockViewModel.Specification = s.Specification;
            StockViewModel.Dimension1Id = s.Dimension1Id;
            StockViewModel.Dimension2Id = s.Dimension2Id;
            StockViewModel.Remark = s.Remark;
            StockViewModel.ProductUidId = s.ProductUidId;
            StockViewModel.Status = temp.Status;
            StockViewModel.CreatedBy = temp.CreatedBy;
            StockViewModel.CreatedDate = DateTime.Now;
            StockViewModel.ModifiedBy = temp.ModifiedBy;
            StockViewModel.ModifiedDate = DateTime.Now;

            string StockPostingError = "";
            StockPostingError = _stockService.StockPostDB(ref StockViewModel);

            s.StockId = StockViewModel.StockId;

            if (temp.StockHeaderId == null)
            {
                temp.StockHeaderId = StockViewModel.StockHeaderId;
            }


            StockProcessViewModel.StockHeaderId = (int)temp.StockHeaderId;
            StockProcessViewModel.DocHeaderId = temp.StockHeaderId;
            StockProcessViewModel.DocLineId = s.StockLineId;
            StockProcessViewModel.DocTypeId = temp.DocTypeId;
            StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
            StockProcessViewModel.StockProcessDocDate = DateTime.Now.Date;
            StockProcessViewModel.DocNo = temp.DocNo;
            StockProcessViewModel.DivisionId = temp.DivisionId;
            StockProcessViewModel.SiteId = temp.SiteId;
            StockProcessViewModel.CurrencyId = null;
            StockProcessViewModel.HeaderProcessId = null;
            StockProcessViewModel.PersonId = temp.PersonId;
            StockProcessViewModel.ProductId = s.ProductId;
            StockProcessViewModel.HeaderFromGodownId = null;
            StockProcessViewModel.HeaderGodownId = temp.GodownId ?? 0;
            StockProcessViewModel.GodownId = temp.GodownId ?? 0;
            StockProcessViewModel.ProcessId = temp.ProcessId;
            StockProcessViewModel.LotNo = s.LotNo;
            StockProcessViewModel.CostCenterId = temp.CostCenterId;
            StockProcessViewModel.Qty_Iss = 0;
            StockProcessViewModel.Qty_Rec = s.Qty;
            StockProcessViewModel.Rate = s.Rate;
            StockProcessViewModel.ExpiryDate = null;
            StockProcessViewModel.Specification = s.Specification;
            StockProcessViewModel.Dimension1Id = s.Dimension1Id;
            StockProcessViewModel.Dimension2Id = s.Dimension2Id;
            StockProcessViewModel.Remark = s.Remark;
            StockProcessViewModel.Status = temp.Status;
            StockProcessViewModel.ProductUidId = s.ProductUidId;
            StockProcessViewModel.CreatedBy = temp.CreatedBy;
            StockProcessViewModel.CreatedDate = DateTime.Now;
            StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
            StockProcessViewModel.ModifiedDate = DateTime.Now;

            string StockProcessPostingError = "";
            StockProcessPostingError = _stockProcessService.StockProcessPostDB(ref StockProcessViewModel);

            s.StockProcessId = StockProcessViewModel.StockProcessId;



            s.CreatedDate = DateTime.Now;
            s.ModifiedDate = DateTime.Now;
            s.CreatedBy = UserName;
            s.Sr = GetMaxSr(s.StockHeaderId);
            s.ModifiedBy = UserName;
            s.ObjectState = Model.ObjectState.Added;

            
            Create(s);

            StockLineExtended LineExtended = new StockLineExtended();
            LineExtended.StockLineId = s.StockLineId;
            LineExtended.DyeingRatio = svm.DyeingRatio;
            LineExtended.TestingQty = svm.TestingQty;
            LineExtended.DocQty = svm.DocQty;
            LineExtended.ExcessQty = svm.ExcessQty;
            LineExtended.ObjectState = Model.ObjectState.Added;
            _StockLineExtendedService.Create(LineExtended);

            

            //StockHeader header = new StockHeaderService(_unitOfWork).Find(s.StockHeaderId);
            if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
            {
                temp.Status = (int)StatusConstants.Modified;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = UserName;
            }

            temp.ObjectState = Model.ObjectState.Modified;
            _StockHeaderRepository.Update(temp);

            
            _unitOfWork.Save();

            svm.StockLineId = s.StockLineId;

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = temp.DocTypeId,
                DocId = temp.StockHeaderId,
                DocLineId = s.StockLineId,
                ActivityType = (int)ActivityTypeContants.Added,
                DocNo = temp.DocNo,
                DocDate = temp.DocDate,
                DocStatus = temp.Status,
            }));

            return svm;

        }

        public void Update(RecipeLineViewModel svm, string UserName)
        {
            StockLine s = Mapper.Map<RecipeLineViewModel, StockLine>(svm);

            StockHeader temp = _StockHeaderRepository.Find(svm.StockHeaderId);

            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            StockLine templine = Find(s.StockLineId);

            StockLine ExTempLine = new StockLine();
            ExTempLine = Mapper.Map<StockLine>(templine);

            if (templine.StockId != null)
            {
                StockViewModel StockViewModel = new StockViewModel();
                StockViewModel.StockHeaderId = temp.StockHeaderId;
                StockViewModel.StockId = templine.StockId ?? 0;
                StockViewModel.DocHeaderId = templine.StockHeaderId;
                StockViewModel.DocLineId = templine.StockLineId;
                StockViewModel.DocTypeId = temp.DocTypeId;
                StockViewModel.StockHeaderDocDate = temp.DocDate;
                StockViewModel.StockDocDate = templine.CreatedDate.Date;
                StockViewModel.DocNo = temp.DocNo;
                StockViewModel.DivisionId = temp.DivisionId;
                StockViewModel.SiteId = temp.SiteId;
                StockViewModel.CurrencyId = null;
                StockViewModel.HeaderProcessId = temp.ProcessId;
                StockViewModel.PersonId = temp.PersonId;
                StockViewModel.ProductId = s.ProductId;
                StockViewModel.HeaderFromGodownId = null;
                StockViewModel.HeaderGodownId = temp.GodownId;
                StockViewModel.GodownId = temp.GodownId ?? 0;
                StockViewModel.ProcessId = templine.FromProcessId;
                StockViewModel.LotNo = templine.LotNo;
                StockViewModel.CostCenterId = temp.CostCenterId;
                StockViewModel.Qty_Iss = s.Qty;
                StockViewModel.Qty_Rec = 0;
                StockViewModel.Rate = templine.Rate;
                StockViewModel.ExpiryDate = null;
                StockViewModel.Specification = templine.Specification;
                StockViewModel.Dimension1Id = templine.Dimension1Id;
                StockViewModel.Dimension2Id = templine.Dimension2Id;
                StockViewModel.Remark = s.Remark;
                StockViewModel.ProductUidId = s.ProductUidId;
                StockViewModel.Status = temp.Status;
                StockViewModel.CreatedBy = templine.CreatedBy;
                StockViewModel.CreatedDate = templine.CreatedDate;
                StockViewModel.ModifiedBy = UserName;
                StockViewModel.ModifiedDate = DateTime.Now;

                string StockPostingError = "";
                StockPostingError = _stockService.StockPostDB(ref StockViewModel);
            }


            if (templine.StockProcessId != null)
            {
                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                StockProcessViewModel.StockHeaderId = temp.StockHeaderId;
                StockProcessViewModel.StockProcessId = templine.StockProcessId ?? 0;
                StockProcessViewModel.DocHeaderId = templine.StockHeaderId;
                StockProcessViewModel.DocLineId = templine.StockLineId;
                StockProcessViewModel.DocTypeId = temp.DocTypeId;
                StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                StockProcessViewModel.StockProcessDocDate = templine.CreatedDate.Date;
                StockProcessViewModel.DocNo = temp.DocNo;
                StockProcessViewModel.DivisionId = temp.DivisionId;
                StockProcessViewModel.SiteId = temp.SiteId;
                StockProcessViewModel.CurrencyId = null;
                StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                StockProcessViewModel.PersonId = temp.PersonId;
                StockProcessViewModel.ProductId = s.ProductId;
                StockProcessViewModel.HeaderFromGodownId = null;
                StockProcessViewModel.HeaderGodownId = temp.GodownId;
                StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                StockProcessViewModel.ProcessId = temp.ProcessId;
                StockProcessViewModel.LotNo = templine.LotNo;
                StockProcessViewModel.CostCenterId = temp.CostCenterId;
                StockProcessViewModel.Qty_Iss = 0;
                StockProcessViewModel.Qty_Rec = s.Qty;
                StockProcessViewModel.Rate = templine.Rate;
                StockProcessViewModel.ExpiryDate = null;
                StockProcessViewModel.Specification = templine.Specification;
                StockProcessViewModel.Dimension1Id = templine.Dimension1Id;
                StockProcessViewModel.Dimension2Id = templine.Dimension2Id;
                StockProcessViewModel.Remark = s.Remark;
                StockProcessViewModel.ProductUidId = s.ProductUidId;
                StockProcessViewModel.Status = temp.Status;
                StockProcessViewModel.CreatedBy = templine.CreatedBy;
                StockProcessViewModel.CreatedDate = templine.CreatedDate;
                StockProcessViewModel.ModifiedBy = UserName;
                StockProcessViewModel.ModifiedDate = DateTime.Now;

                string StockProcessPostingError = "";
                StockProcessPostingError = _stockProcessService.StockProcessPostDB(ref StockProcessViewModel);

            }


            templine.ProductId = s.ProductId;
            templine.ProductUidId = s.ProductUidId;
            templine.LotNo = s.LotNo;
            templine.FromProcessId = s.FromProcessId;
            templine.Rate = s.Rate;
            templine.Amount = s.Amount;
            templine.Remark = s.Remark;
            templine.Qty = s.Qty;
            templine.Remark = s.Remark;
            templine.Dimension1Id = s.Dimension1Id;
            templine.Dimension2Id = s.Dimension2Id;
            templine.Specification = s.Specification;
            templine.ModifiedDate = DateTime.Now;
            templine.ModifiedBy = UserName;
            templine.ObjectState = Model.ObjectState.Modified;
            Update(templine);

            StockLineExtended LineExtended = _StockLineExtendedService.Find(templine.StockLineId);
            LineExtended.StockLineId = templine.StockLineId;
            LineExtended.DyeingRatio = svm.DyeingRatio;
            LineExtended.TestingQty = svm.TestingQty;
            LineExtended.DocQty = svm.DocQty;
            LineExtended.ExcessQty = svm.ExcessQty;
            LineExtended.ObjectState = Model.ObjectState.Modified;
            _StockLineExtendedService.Update(LineExtended);

            int Status = 0;
            if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
            {
                Status = temp.Status;
                temp.Status = (int)StatusConstants.Modified;
                temp.ModifiedBy = UserName;
                temp.ModifiedDate = DateTime.Now;
            }

            temp.ObjectState = Model.ObjectState.Modified;
            _StockHeaderRepository.Update(temp);


            LogList.Add(new LogTypeViewModel
            {
                ExObj = ExTempLine,
                Obj = templine
            });


            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = temp.DocTypeId,
                DocId = templine.StockHeaderId,
                DocLineId = templine.StockLineId,
                ActivityType = (int)ActivityTypeContants.Modified,
                DocNo = temp.DocNo,
                xEModifications = Modifications,
                DocDate = temp.DocDate,
                DocStatus = temp.Status,
            }));
        }

        public void Delete(RecipeLineViewModel vm, string UserName)
        {

            int? StockId = 0;
            int? StockProcessId = 0;
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            StockLine StockLine = Find(vm.StockLineId);
            StockHeader header = _StockHeaderRepository.Find(StockLine.StockHeaderId);



            StockId = StockLine.StockId;
            StockProcessId = StockLine.StockProcessId;

            LogList.Add(new LogTypeViewModel
            {
                Obj = Mapper.Map<StockLine>(StockLine),
            });


            _StockLineExtendedService.Delete(StockLine.StockLineId);

            //_RecipeLineService.Delete(StockLine);
            StockLine.ObjectState = Model.ObjectState.Deleted;
            Delete(StockLine);

            if (StockId != null)
            {
                _stockService.DeleteStock((int)StockId);
            }

            if (StockProcessId != null)
            {
                _stockProcessService.DeleteStockProcessDB((int)StockProcessId);
            }

            


            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedDate = DateTime.Now;
                header.ModifiedBy = UserName;
                _StockHeaderRepository.Update(header);
            }


           


            
            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            //Saving the Activity Log

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = header.DocTypeId,
                DocId = header.StockHeaderId,
                DocLineId = StockLine.StockLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));

        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }




        public IEnumerable<StockLine> GetStockLineListForHeader(int HeaderId)
        {
            return (from p in _StockLineRepository.Instance
                    where p.StockHeaderId == HeaderId
                    select p);
        }

        public IEnumerable<Dimension1RecipeViewModel> GetRecipes(int Dimension1Id)
        {
            SqlParameter SqlParameterDimension1Id = new SqlParameter("@Dimension1Id", Dimension1Id);

            IEnumerable<Dimension1RecipeViewModel> RecipeList = _unitOfWork.SqlQuery<Dimension1RecipeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetRecipeListForShade @Dimension1Id", SqlParameterDimension1Id).ToList();

            return RecipeList;

        }

        public IEnumerable<RecipeDetailViewModel> GetRecipeDetail(int JobOrderHeaderId)
        {
            SqlParameter SqlParameterJobOrderHeaderId = new SqlParameter("@JobOrderHeaderId", JobOrderHeaderId);

            IEnumerable<RecipeDetailViewModel> RecipeDetail = _unitOfWork.SqlQuery<RecipeDetailViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetRecipeDetail @JobOrderHeaderId", SqlParameterJobOrderHeaderId).ToList();

            return RecipeDetail;
        }


        public LastValues GetLastValues(int JobOrderHeaderId)
        {
            var temp = (from H in _unitOfWork.Repository<JobOrderHeader>().Instance
                        join L in _unitOfWork.Repository<StockLine>().Instance on H.StockHeaderId equals L.StockHeaderId into StockLineTable
                        from StockLineTab in StockLineTable.DefaultIfEmpty()
                        join Le in _unitOfWork.Repository<StockLineExtended>().Instance on StockLineTab.StockLineId equals Le.StockLineId into StockLineExtendedTable
                        from StockLineExtendedTab in StockLineExtendedTable.DefaultIfEmpty()
                        where H.JobOrderHeaderId == JobOrderHeaderId
                        orderby StockLineExtendedTab.StockLineId descending
                        select new LastValues
                        {
                            DyeingRatio = StockLineExtendedTab.DyeingRatio
                        }).FirstOrDefault();

            return temp;
        }

        public bool IsDuplicateLine(int StockHeaderId, int ProductId, Decimal? DyeingRatio, int? StockLineId)
        {
            var temp = (from L in _unitOfWork.Repository<StockLine>().Instance
                        join Le in _unitOfWork.Repository<StockLineExtended>().Instance on L.StockLineId equals Le.StockLineId into StockLineExtendedTable from StockLineExtendedTab in StockLineExtendedTable.DefaultIfEmpty()
                        where L.StockHeaderId == StockHeaderId && L.ProductId == ProductId && StockLineExtendedTab.DyeingRatio == DyeingRatio && L.StockLineId != StockLineId
                        select L).FirstOrDefault();

            if (temp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public decimal GetExcessStock(int ProductId, int? Dim1, int? Dim2, int? ProcId, string Lot, int StockHeaderId, string ProcName)
        {
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(StockHeaderId);
            decimal EXS = 0;

            using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
            {
                sqlConnection.Open();

                SqlCommand Totalf = new SqlCommand("SELECT " + (string.IsNullOrEmpty(ProcName) ? "Web.FStockBalance" : ProcName) + "( " + ProductId + ", " + (!Dim1.HasValue ? "NULL" : "" + Dim1 + "") + ", " + (!Dim2.HasValue ? "NULL" : "" + Dim2 + "") + ", " + (!ProcId.HasValue ? "NULL" : "" + ProcId + "") + ", " + (string.IsNullOrEmpty(Lot) ? "NULL" : Lot) + ", " + Header.SiteId + ", NULL" + ", " + (!Header.GodownId.HasValue ? "NULL" : "" + Header.GodownId + "") + ")", sqlConnection);

                EXS = Convert.ToDecimal(Totalf.ExecuteScalar() == DBNull.Value ? 0 : Totalf.ExecuteScalar());
            }

            return EXS;
        }
    }

    public class Dimension1RecipeViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string ProductName { get; set; }
        public string RecipeNo { get; set; }
        public string RecipeDate { get; set; }
        public string LotNo { get; set; }
    }

    public class RecipeDetailViewModel
    {
        public int StockLineId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public Decimal? DyeingRatio { get; set; }
        public Decimal? TestingQty { get; set; }
        public Decimal? DocQty { get; set; }
        public Decimal? ExcessQty { get; set; }
        public Decimal Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }

    }
}