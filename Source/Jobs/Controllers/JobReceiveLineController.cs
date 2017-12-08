using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Jobs.Helpers;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using JobReceiveDocumentEvents;
using Reports.Controllers;
using System.Reflection;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobReceiveLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;

        IJobReceiveLineService _JobReceiveLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobReceiveLineController(IJobReceiveLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveLineService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }


        #region ByProductTransactions

        public ActionResult ByProduct(int id)//ReceiveHeaderId
        {
            JobReceiveByProductViewModel vm = new JobReceiveByProductViewModel();
            vm.JobReceiveHeaderId = id;
            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(id);
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            return PartialView("ByProduct", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ByProductPost(JobReceiveByProductViewModel vm)
        {

            if (ModelState.IsValid)
            {
                //Create Logic
                if (vm.JobReceiveByProductId <= 0)
                {
                    JobReceiveByProduct ByProduct = Mapper.Map<JobReceiveByProductViewModel, JobReceiveByProduct>(vm);
                    ByProduct.CreatedBy = User.Identity.Name;
                    ByProduct.CreatedDate = DateTime.Now;
                    ByProduct.ModifiedBy = User.Identity.Name;
                    ByProduct.ModifiedDate = DateTime.Now;
                    ByProduct.ObjectState = Model.ObjectState.Added;
                    db.JobReceiveByProduct.Add(ByProduct);
                    //new JobReceiveByProductService(_unitOfWork).Create(ByProduct);

                    try
                    {
                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("ByProduct", vm);

                    }

                    return RedirectToAction("ByProduct", new { id = vm.JobReceiveHeaderId });

                }
                else//Edit Logic
                {

                    JobReceiveByProduct temp = new JobReceiveByProductService(_unitOfWork).Find(vm.JobReceiveByProductId);
                    temp.ProductId = vm.ProductId;
                    temp.Dimension1Id = vm.Dimension1Id;
                    temp.Dimension2Id = vm.Dimension2Id;
                    temp.Dimension3Id = vm.Dimension3Id;
                    temp.Dimension4Id = vm.Dimension4Id;
                    temp.LotNo = vm.LotNo;
                    temp.Qty = vm.Qty;
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveByProduct.Add(temp);

                    //new JobReceiveByProductService(_unitOfWork).Update(temp);

                    try
                    {
                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("ByProduct", vm);

                    }

                    return Json(new { success = true });

                }
            }

            return PartialView("ByProduct", vm);

        }

        public ActionResult EditByProduct(int id)//ReceiveHeaderId
        {

            JobReceiveByProductViewModel vm = new JobReceiveByProductService(_unitOfWork).GetJobReceiveByProduct(id);

            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(id);
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            if (vm == null)
            {
                return HttpNotFound();
            }
            return PartialView("ByProduct", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteByProduct(JobReceiveByProductViewModel vm)
        {
            JobReceiveByProduct temp = (from p in db.JobReceiveByProduct
                                        where p.JobReceiveByProductId == vm.JobReceiveByProductId
                                        select p).FirstOrDefault();
            JobReceiveHeader header = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);

            //new JobReceiveByProductService(_unitOfWork).Delete(temp);

            temp.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveByProduct.Remove(temp);

            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedBy = User.Identity.Name;
                header.ModifiedDate = DateTime.Now;
            }
            header.ObjectState = Model.ObjectState.Modified;
            db.JobReceiveHeader.Add(header);

            try
            {
                db.SaveChanges();
                //_unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("ByProduct", vm);

            }
            return Json(new { success = true });
        }




        #endregion


        #region ConsumptionTransactions

        public ActionResult Consumption(int id)//ReceiveHeaderId
        {
            JobReceiveBomViewModel vm = new JobReceiveBomViewModel();
            vm.JobReceiveHeaderId = id;
            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(id);
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            return PartialView("Consumption", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConsumptionPost(JobReceiveBomViewModel vm)
        {

            if (ModelState.IsValid)
            {
                //Create Logic
                if (vm.JobReceiveBomId <= 0)
                {
                    JobReceiveBom Consumption = Mapper.Map<JobReceiveBomViewModel, JobReceiveBom>(vm);
                    Consumption.CreatedBy = User.Identity.Name;
                    Consumption.CreatedDate = DateTime.Now;
                    Consumption.ModifiedBy = User.Identity.Name;
                    Consumption.ModifiedDate = DateTime.Now;

                    JobReceiveHeader JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();


                    if (JobReceiveHeader.StockHeaderId == null)
                    {
                        StockProcessBomViewModel.StockHeaderId = 0;
                    }
                    else
                    {
                        StockProcessBomViewModel.StockHeaderId = (int)JobReceiveHeader.StockHeaderId;
                    }

                    StockProcessBomViewModel.DocHeaderId = JobReceiveHeader.JobReceiveHeaderId;
                    StockProcessBomViewModel.DocLineId = Consumption.JobReceiveBomId;
                    StockProcessBomViewModel.DocTypeId = JobReceiveHeader.DocTypeId;
                    StockProcessBomViewModel.StockHeaderDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.StockProcessDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.DocNo = JobReceiveHeader.DocNo;
                    StockProcessBomViewModel.DivisionId = JobReceiveHeader.DivisionId;
                    StockProcessBomViewModel.SiteId = JobReceiveHeader.SiteId;
                    StockProcessBomViewModel.CurrencyId = null;
                    StockProcessBomViewModel.HeaderProcessId = null;
                    StockProcessBomViewModel.PersonId = JobReceiveHeader.JobWorkerId;
                    StockProcessBomViewModel.ProductId = Consumption.ProductId;
                    StockProcessBomViewModel.HeaderFromGodownId = null;
                    StockProcessBomViewModel.HeaderGodownId = null;
                    StockProcessBomViewModel.GodownId = JobReceiveHeader.GodownId;
                    StockProcessBomViewModel.ProcessId = JobReceiveHeader.ProcessId;
                    StockProcessBomViewModel.LotNo = Consumption.LotNo;
                    StockProcessBomViewModel.CostCenterId = Consumption.CostCenterId;
                    StockProcessBomViewModel.Qty_Iss = Consumption.Qty;
                    StockProcessBomViewModel.Qty_Rec = 0;
                    StockProcessBomViewModel.Rate = 0;
                    StockProcessBomViewModel.ExpiryDate = null;
                    StockProcessBomViewModel.Specification = null;
                    StockProcessBomViewModel.Dimension1Id = null;
                    StockProcessBomViewModel.Dimension2Id = null;
                    StockProcessBomViewModel.Dimension3Id = null;
                    StockProcessBomViewModel.Dimension4Id = null;
                    StockProcessBomViewModel.Remark = null;
                    StockProcessBomViewModel.Status = JobReceiveHeader.Status;
                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                    string StockProcessPostingError = "";
                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                    if (StockProcessPostingError != "")
                    {
                        ModelState.AddModelError("", StockProcessPostingError);
                        return PartialView("_Create", vm);
                    }

                    Consumption.StockProcessId = StockProcessBomViewModel.StockProcessId;
                    Consumption.ObjectState = Model.ObjectState.Added;
                    db.JobReceiveBom.Add(Consumption);


                    if (!JobReceiveHeader.StockHeaderId.HasValue || JobReceiveHeader.StockHeaderId == 0)
                    {
                        JobReceiveHeader.StockHeaderId = StockProcessBomViewModel.StockHeaderId;

                        JobReceiveHeader.ObjectState = Model.ObjectState.Modified;
                        db.JobReceiveHeader.Add(JobReceiveHeader);
                    }


                    try
                    {
                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("Consumption", vm);
                    }

                    return RedirectToAction("Consumption", new { id = vm.JobReceiveHeaderId });

                }
                else//Edit Logic
                {

                    JobReceiveBom temp = new JobReceiveBomService(_unitOfWork).Find(vm.JobReceiveBomId);
                    temp.ProductId = vm.ProductId;
                    temp.Dimension1Id = vm.Dimension1Id;
                    temp.Dimension2Id = vm.Dimension2Id;
                    temp.Dimension3Id = vm.Dimension3Id;
                    temp.Dimension4Id = vm.Dimension4Id;
                    temp.LotNo = vm.LotNo;
                    temp.Qty = vm.Qty;


                    JobReceiveHeader JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();

                    StockProcessBomViewModel.StockHeaderId = JobReceiveHeader.StockHeaderId ?? 0;
                    StockProcessBomViewModel.StockProcessId = temp.StockProcessId ?? 0;
                    StockProcessBomViewModel.DocHeaderId = JobReceiveHeader.JobReceiveHeaderId;
                    StockProcessBomViewModel.DocLineId = temp.JobReceiveBomId;
                    StockProcessBomViewModel.DocTypeId = JobReceiveHeader.DocTypeId;
                    StockProcessBomViewModel.StockHeaderDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.StockProcessDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.DocNo = JobReceiveHeader.DocNo;
                    StockProcessBomViewModel.DivisionId = JobReceiveHeader.DivisionId;
                    StockProcessBomViewModel.SiteId = JobReceiveHeader.SiteId;
                    StockProcessBomViewModel.CurrencyId = null;
                    StockProcessBomViewModel.HeaderProcessId = null;
                    StockProcessBomViewModel.PersonId = JobReceiveHeader.JobWorkerId;
                    StockProcessBomViewModel.ProductId = temp.ProductId;
                    StockProcessBomViewModel.HeaderFromGodownId = null;
                    StockProcessBomViewModel.HeaderGodownId = null;
                    StockProcessBomViewModel.GodownId = JobReceiveHeader.GodownId;
                    StockProcessBomViewModel.ProcessId = JobReceiveHeader.ProcessId;
                    StockProcessBomViewModel.LotNo = temp.LotNo;
                    StockProcessBomViewModel.CostCenterId = temp.CostCenterId;
                    StockProcessBomViewModel.Qty_Iss = temp.Qty;
                    StockProcessBomViewModel.Qty_Rec = 0;
                    StockProcessBomViewModel.Rate = 0;
                    StockProcessBomViewModel.ExpiryDate = null;
                    StockProcessBomViewModel.Specification = null;
                    StockProcessBomViewModel.Dimension1Id = null;
                    StockProcessBomViewModel.Dimension2Id = null;
                    StockProcessBomViewModel.Dimension3Id = null;
                    StockProcessBomViewModel.Dimension4Id = null;
                    StockProcessBomViewModel.Remark = null;
                    StockProcessBomViewModel.Status = JobReceiveHeader.Status;
                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                    string StockProcessPostingError = "";
                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                    if (StockProcessPostingError != "")
                    {
                        ModelState.AddModelError("", StockProcessPostingError);
                        return PartialView("_Create", vm);
                    }



                    temp.StockProcessId = StockProcessBomViewModel.StockProcessId;
                    temp.ObjectState = Model.ObjectState.Modified;

                    db.JobReceiveBom.Add(temp);

                    //new JobReceiveBomService(_unitOfWork).Update(temp);

                    try
                    {
                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("Consumption", vm);

                    }

                    return Json(new { success = true });

                }
            }

            return PartialView("Consumption", vm);

        }

        public ActionResult EditConsumption(int id)//ReceiveHeaderId
        {
            JobReceiveBomViewModel vm = new JobReceiveBomService(_unitOfWork).GetJobReceiveBom(id);

            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            if (vm == null)
            {
                return HttpNotFound();
            }
            return PartialView("Consumption", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConsumption(JobReceiveBomViewModel vm)
        {
            JobReceiveBom temp = (from p in db.JobReceiveBom
                                  where p.JobReceiveBomId == vm.JobReceiveBomId
                                  select p).FirstOrDefault();

            JobReceiveHeader header = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);

            if (temp.StockProcessId != null)
            {
                var StockProcess = (from p in db.StockProcess
                                    where p.StockProcessId == temp.StockProcessId
                                    select p).FirstOrDefault();
                StockProcess.ObjectState = Model.ObjectState.Deleted;
                db.StockProcess.Remove(StockProcess);
            }

            temp.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveBom.Remove(temp);
            //new JobReceiveBomService(_unitOfWork).Delete(temp);

            if (header.Status != (int)StatusConstants.Drafted)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedBy = User.Identity.Name;
                header.ModifiedDate = DateTime.Now;
            }


            header.ObjectState = Model.ObjectState.Modified;
            db.JobReceiveHeader.Add(header);

            try
            {
                db.SaveChanges();
                //_unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("Consumption", vm);
            }
            return Json(new { success = true });
        }




        #endregion


        public ActionResult _ForOrder(int id, int sid)
        {
            JobReceiveLineFilterViewModel vm = new JobReceiveLineFilterViewModel();
            vm.JobReceiveHeaderId = id;
            JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);


            vm.DocTypeId = Header.DocTypeId;
            vm.JobWorkerId = sid;
            return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobReceiveLineFilterViewModel vm)
        {
            List<JobReceiveLineViewModel> temp = _JobReceiveLineService.GetJobOrdersForFilters(vm).ToList();
            JobReceiveMasterDetailModel svm = new JobReceiveMasterDetailModel();
            //svm.JobReceiveLineViewModel = temp.Where(m => m.JobOrderUidHeaderId == null && m.ProductUidId == null).ToList();
            svm.JobReceiveLineViewModel = temp.ToList();
            JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
            JobReceiveSettings settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            if (temp.Where(m => m.JobOrderUidHeaderId != null).Any())
            {
                List<JobReceiveLineViewModel> Sequence = new List<JobReceiveLineViewModel>();
                Sequence = temp.Where(m => m.JobOrderUidHeaderId != null && m.ProductUidId == null).ToList();
                foreach (var item in Sequence)
                    item.JobReceiveType = JobReceiveTypeConstants.ProductUIdHeaderId;

                HttpContext.Session["UIQHID" + vm.JobReceiveHeaderId] = Sequence;
            }

            if (temp.Where(m => m.ProductUidId != null).Any())
            {
                List<JobReceiveLineViewModel> Sequence = new List<JobReceiveLineViewModel>();
                Sequence = temp.Where(m => m.ProductUidId != null && m.JobOrderUidHeaderId == null).ToList();
                foreach (var item in Sequence)
                    item.JobReceiveType = JobReceiveTypeConstants.ProductUid;

                HttpContext.Session["UIQUID" + vm.JobReceiveHeaderId] = Sequence;
            }

            if (!temp.Where(m => m.JobOrderUidHeaderId == null && m.ProductUidId == null).Any())
            {
                if (temp.Where(m => m.JobOrderUidHeaderId != null).Count() > 0)
                {

                    List<BarCodeSequenceViewModelForReceive> Sequence = new List<BarCodeSequenceViewModelForReceive>();

                    foreach (var item in temp.Where(m => m.ProductUidId == null))
                        Sequence.Add(new BarCodeSequenceViewModelForReceive
                        {
                            JobReceiveHeaderId = item.JobReceiveHeaderId,
                            JobOrderLineId = item.JobOrderLineId ?? 0,
                            ProductName = item.ProductName,
                            Qty = item.OrderBalanceQty,
                            BalanceQty = item.OrderBalanceQty,
                            CostCenterName = item.CostCenterName,
                            JobReceiveType = JobReceiveTypeConstants.ProductUIdHeaderId,
                            // FirstBarCode = _JobReceiveLineService.GetFirstBarCodeForCancel(item.JobOrderLineId),
                        });

                    BarCodeSequenceListViewModelForReceive SquenceList = new BarCodeSequenceListViewModelForReceive();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;
                    HttpContext.Session.Remove("UIQHID" + vm.JobReceiveHeaderId);

                    return PartialView("_Sequence", SquenceList);

                }
                else if (temp.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    List<BarCodeSequenceViewModelForReceive> Sequence = new List<BarCodeSequenceViewModelForReceive>();

                    var Grouping = (from p in temp.Where(m => m.ProductUidId != null && m.JobOrderUidHeaderId == null)
                                    group p by new { p.JobOrderHeaderId, p.ProductId, p.ProdOrderLineId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForReceive
                        {
                            JobReceiveHeaderId = item.Max(m => m.JobReceiveHeaderId),
                            //JobOrderLineId = item.JobOrderLineId,
                            JobOrdLineIds = string.Join(",", item.Select(m => m.JobOrderLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.OrderBalanceQty),
                            BalanceQty = item.Sum(m => m.OrderBalanceQty),
                            CostCenterName = item.Max(m => m.CostCenterName),
                            JobReceiveType = JobReceiveTypeConstants.ProductUid,
                            JobOrderHeaderId = item.Max(m => m.JobOrderHeaderId),
                            // FirstBarCode = _JobReceiveLineService.GetFirstBarCodeForCancel(item.JobOrderLineId),
                        });

                    BarCodeSequenceListViewModelForReceive SquenceList = new BarCodeSequenceListViewModelForReceive();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;
                    HttpContext.Session.Remove("UIQHID" + vm.JobReceiveHeaderId);
                    HttpContext.Session.Remove("UIQUID" + vm.JobReceiveHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

            }

            if (svm.JobReceiveSettings.isVisibleLoss == true && svm.JobReceiveSettings.IsVisiblePassQty == false  && svm.JobReceiveSettings.isVisibleLotNo == false)
            {
                return PartialView("_ResultsWithLossQty", svm);
            }
            if (svm.JobReceiveSettings.isVisibleLoss == false && svm.JobReceiveSettings.IsVisiblePassQty == false && svm.JobReceiveSettings.isVisibleLotNo == false)
            {
                return PartialView("_ResultsWithQty", svm);
            }
            if (svm.JobReceiveSettings.isVisibleLoss == false && svm.JobReceiveSettings.IsVisiblePassQty == false && svm.JobReceiveSettings.isVisibleLotNo == true)
            {
                return PartialView("_ResultsWithQtyLotNo", svm);
            }
            else
            {
                return PartialView("_Results", svm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobReceiveMasterDetailModel vm)
        {
            int Cnt = 0;
            int Cnt1 = 0;
            int Serial = _JobReceiveLineService.GetMaxSr(vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobReceiveLineViewModel> BarCodeBased = new List<JobReceiveLineViewModel>();
            List<JobReceiveLineViewModel> BarCodeBasedBranch = new List<JobReceiveLineViewModel>();

            BarCodeBasedBranch = (List<JobReceiveLineViewModel>)HttpContext.Session["UIQUID" + vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId];

            BarCodeBased = (List<JobReceiveLineViewModel>)HttpContext.Session["UIQHID" + vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId];

            HttpContext.Session.Remove("UIQHID" + vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId);


            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId);

                JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                var JobOrderLineIds = vm.JobReceiveLineViewModel.Select(m => m.JobOrderLineId).ToArray();

                var JobOrderLineCostCenterRecords = (from p in db.JobOrderLine
                                                     join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                                     select new { CostCenterId = t.CostCenterId, Rate = p.Rate, JoborderLineId = p.JobOrderLineId }).ToList();

                var JobOrderLineBalanceQtyRecords = (from p in db.ViewJobOrderBalance
                                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                                     select new { BalQty = p.BalanceQty, LineId = p.JobOrderLineId }).ToList();

                var JobORderLineRecords = (from p in db.JobOrderLine
                                           where JobOrderLineIds.Contains(p.JobOrderLineId)
                                           select p).ToList();

                var JobOrderHeaderIds = JobORderLineRecords.Select(m => m.JobOrderHeaderId).ToArray();

                var JobOrderHeaderRecords = (from p in db.JobOrderHeader
                                             where JobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                             select p).ToList();

                var ProductUids = JobORderLineRecords.Select(m => m.ProductUidId).ToArray();

                var ProductUidsRecords = (from p in db.ProductUid
                                          where ProductUids.Contains(p.ProductUIDId)
                                          select p).ToList();

                foreach (var item in vm.JobReceiveLineViewModel.Where(m => m.ReceiveQty + m.LossQty > 0))
                {
                    //var temp = (from L in db.JobOrderLine
                    //            join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                    //            from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                    //            where L.JobOrderLineId == item.JobOrderLineId
                    //            select new { CostCenterId = JobOrderHeaderTab.CostCenterId, Rate = L.Rate }).FirstOrDefault();

                    //var balqty = (from p in db.ViewJobOrderBalance
                    //              where p.JobOrderLineId == item.JobOrderLineId
                    //              select p.BalanceQty).FirstOrDefault();


                    if (item.JobOrderLineId != null)
                    {
                        if (item.OrderBalanceQty < item.ReceiveQty)
                        {
                            Decimal? ExcessAllowedQty = new JobOrderLineService(_unitOfWork).GetExcessReceiveAllowedAgainstOrderQty((int)item.JobOrderLineId);
                            if (ExcessAllowedQty != null)
                            {
                                Decimal TotalReceiveQty = 0;
                                var ReceiveLinesList = (from L in db.JobReceiveLine
                                                        where L.JobOrderLineId == item.JobOrderLineId
                                                        group new { L } by new { L.JobOrderLineId } into Result
                                                        select new
                                                        {
                                                            Qty = Result.Sum(m => m.L.Qty)
                                                        }).FirstOrDefault();
                                if (ReceiveLinesList != null)
                                    TotalReceiveQty = ReceiveLinesList.Qty;

                                TotalReceiveQty = TotalReceiveQty + item.ReceiveQty;
                                var OrderLine = new JobOrderLineService(_unitOfWork).Find((int)item.JobOrderLineId);
                                Decimal ExcessQty = TotalReceiveQty - OrderLine.Qty;

                                if (ExcessQty > ExcessAllowedQty)
                                {
                                    string message = "Qty exceeding allowed excess receive qty for product.";
                                    ModelState.AddModelError("", message);
                                    if (vm.JobReceiveSettings.isVisibleLoss == true && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                    {
                                        return PartialView("_ResultsWithLossQty", vm);
                                    }
                                    if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                    {
                                        return PartialView("_ResultsWithQty", vm);
                                    }
                                    if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == true)
                                    {
                                        return PartialView("_ResultsWithQtyLotNo", vm);
                                    }
                                    else
                                    {
                                        return PartialView("_Results", vm);
                                    }
                                }
                            }
                        }

                    }

                    var temp = JobOrderLineCostCenterRecords.Where(m => m.JoborderLineId == item.JobOrderLineId).FirstOrDefault();

                    var balqty = JobOrderLineBalanceQtyRecords.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().BalQty;

                    JobOrderLine JobOrderLine = JobORderLineRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault();
                    JobOrderHeader JobOrderHeader = JobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();

                    //if (JobOrderLine.ProductUidHeaderId != null && item.DocQty > 0)
                    //{
                    //    item.OrderBalanceQty = balqty;
                    //    BarCodeBased.Add(item);
                    //}

                    //if (JobOrderLine.ProductUidHeaderId == null && item.DocQty > 0 && JobOrderLine.ProductUidId!=null)
                    //{
                    //    item.OrderBalanceQty = balqty;
                    //    BarCodeBasedBranch.Add(item);
                    //}


                    //if (item.DocQty > 0 && item.DocQty <= balqty && JobOrderLine.ProductUidHeaderId == null && JobOrderLine.ProductUidId == null)
                    if (item.DocQty > 0 && JobOrderLine.ProductUidHeaderId == null && JobOrderLine.ProductUidId == null)
                    {

                        JobReceiveLine line = new JobReceiveLine();
                        line.JobReceiveLineId = -Cnt;
                        line.JobReceiveHeaderId = item.JobReceiveHeaderId;
                        line.JobOrderLineId = item.JobOrderLineId;
                        line.ProductUidId = JobOrderLine.ProductUidId;
                        line.ProductId = JobOrderLine.ProductId;
                        line.Dimension1Id = JobOrderLine.Dimension1Id;
                        line.Dimension2Id = JobOrderLine.Dimension2Id;
                        line.Dimension3Id = JobOrderLine.Dimension3Id;
                        line.Dimension4Id = JobOrderLine.Dimension4Id;
                        line.Qty = item.ReceiveQty;
                        line.LossQty = item.LossQty;
                        line.PassQty = item.PassQty;
                        line.LotNo = item.LotNo;
                        line.UnitConversionMultiplier = JobOrderLine.UnitConversionMultiplier;
                        line.DealQty = (line.UnitConversionMultiplier * line.Qty);

                        //if (item.DealQty != null && item.DealQty != 0)
                        //    line.DealQty = item.DealQty;
                        //else
                        //    line.DealQty = (line.UnitConversionMultiplier * line.Qty);

                        line.DealUnitId = JobOrderLine.DealUnitId;
                        line.LotNo = item.LotNo;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        if (line.JobOrderLineId != null)
                        {
                            LineStatus.Add((int)line.JobOrderLineId, (line.Qty + line.LossQty));
                        }


                        if (Settings.isPostedInStock ?? false)
                        {
                            StockViewModel StockViewModel = new StockViewModel();

                            if (Cnt == 0)
                            {
                                StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                            }
                            else
                            {
                                if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
                                {
                                    StockViewModel.StockHeaderId = (int)Header.StockHeaderId;
                                }
                                else
                                {
                                    StockViewModel.StockHeaderId = -1;
                                }
                            }

                            StockViewModel.StockId = -Cnt;
                            StockViewModel.DocHeaderId = Header.JobReceiveHeaderId;
                            StockViewModel.DocLineId = line.JobReceiveLineId;
                            StockViewModel.DocTypeId = Header.DocTypeId;
                            StockViewModel.StockHeaderDocDate = Header.DocDate;
                            StockViewModel.StockDocDate = Header.DocDate;
                            StockViewModel.DocNo = Header.DocNo;
                            StockViewModel.DivisionId = Header.DivisionId;
                            StockViewModel.SiteId = Header.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.PersonId = Header.JobWorkerId;
                            StockViewModel.ProductId = JobOrderLine.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = Header.GodownId;
                            StockViewModel.HeaderProcessId = Header.ProcessId;
                            StockViewModel.GodownId = (int)Header.GodownId;
                            StockViewModel.Remark = Header.Remark;
                            StockViewModel.Status = Header.Status;
                            StockViewModel.ProcessId = Header.ProcessId;
                            StockViewModel.LotNo = line.LotNo;

                            if (temp != null)
                            {
                                StockViewModel.CostCenterId = temp.CostCenterId;
                                StockViewModel.Rate = temp.Rate;
                            }
                            StockViewModel.Qty_Iss = 0;
                            StockViewModel.Qty_Rec = line.Qty;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = JobOrderLine.Specification;
                            StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockViewModel.ProductUidId = line.ProductUidId;
                            StockViewModel.CreatedBy = User.Identity.Name;
                            StockViewModel.CreatedDate = DateTime.Now;
                            StockViewModel.ModifiedBy = User.Identity.Name;
                            StockViewModel.ModifiedDate = DateTime.Now;

                            string StockPostingError = "";
                            StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                            if (StockPostingError != "")
                            {
                                string message = StockPostingError;
                                ModelState.AddModelError("", message);
                                //return PartialView("_Results", vm);
                                if (vm.JobReceiveSettings.isVisibleLoss == true && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                {
                                    return PartialView("_ResultsWithLossQty", vm);
                                }
                                if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                {
                                    return PartialView("_ResultsWithQty", vm);
                                }
                                if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == true)
                                {
                                    return PartialView("_ResultsWithQtyLotNo", vm);
                                }
                                else
                                {
                                    return PartialView("_Results", vm);
                                }
                            }

                            if (Cnt == 0)
                            {
                                Header.StockHeaderId = StockViewModel.StockHeaderId;
                            }
                            line.StockId = StockViewModel.StockId;
                        }


                        if (Settings.isPostedInStockProcess ?? false)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else if (Settings.isPostedInStock ?? false)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                            {
                                StockProcessViewModel.StockHeaderId = -1;
                            }
                            else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                            {
                                StockProcessViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessViewModel.StockHeaderId = 0;
                            }

                            StockProcessViewModel.StockProcessId = -Cnt;
                            StockProcessViewModel.DocHeaderId = Header.JobReceiveHeaderId;
                            StockProcessViewModel.DocLineId = line.JobReceiveLineId;
                            StockProcessViewModel.DocTypeId = Header.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                            StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                            StockProcessViewModel.DocNo = Header.DocNo;
                            StockProcessViewModel.DivisionId = Header.DivisionId;
                            StockProcessViewModel.SiteId = Header.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.PersonId = Header.JobWorkerId;
                            StockProcessViewModel.ProductId = JobOrderLine.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = Header.GodownId;
                            StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                            StockProcessViewModel.GodownId = (int)Header.GodownId;
                            StockProcessViewModel.Remark = Header.Remark;
                            StockProcessViewModel.Status = Header.Status;
                            StockProcessViewModel.ProcessId = Header.ProcessId;
                            StockProcessViewModel.LotNo = line.LotNo;

                            if (temp != null)
                            {
                                StockProcessViewModel.CostCenterId = temp.CostCenterId;
                                StockProcessViewModel.Rate = temp.Rate;
                            }
                            StockProcessViewModel.Qty_Iss = line.Qty + line.LossQty;
                            StockProcessViewModel.Qty_Rec = 0;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = JobOrderLine.Specification;
                            StockProcessViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockProcessViewModel.ProductUidId = line.ProductUidId;
                            StockProcessViewModel.CreatedBy = User.Identity.Name;
                            StockProcessViewModel.CreatedDate = DateTime.Now;
                            StockProcessViewModel.ModifiedBy = User.Identity.Name;
                            StockProcessViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                string message = StockProcessPostingError;
                                ModelState.AddModelError("", message);
                                //return PartialView("_Results", vm);
                                if (vm.JobReceiveSettings.isVisibleLoss == true && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                {
                                    return PartialView("_ResultsWithLossQty", vm);
                                }
                                if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                {
                                    return PartialView("_ResultsWithQty", vm);
                                }
                                if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == true)
                                {
                                    return PartialView("_ResultsWithQtyLotNo", vm);
                                }
                                else
                                {
                                    return PartialView("_Results", vm);
                                }
                            }

                            if ((Settings.isPostedInStock ?? false) == false)
                            {
                                if (Cnt == 0)
                                {
                                    Header.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                }
                            }

                            line.StockProcessId = StockProcessViewModel.StockProcessId;
                        }



                        line.ObjectState = Model.ObjectState.Added;
                        db.JobReceiveLine.Add(line);

                        new JobReceiveLineStatusService(_unitOfWork).CreateLineStatus(line.JobReceiveLineId, ref db, true);


                        #region BomPost

                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        if (!string.IsNullOrEmpty(Settings.SqlProcConsumption))
                        {
                            var BomPostList = _JobReceiveLineService.GetBomPostingDataForWeaving(item.ProductId, item.Dimension1Id, item.Dimension2Id, item.Dimension3Id, item.Dimension4Id, Header.ProcessId, item.PassQty, Header.DocTypeId, Settings.SqlProcConsumption, line.JobOrderLineId, line.Weight).ToList();

                            foreach (var BomItem in BomPostList)
                            {
                                JobReceiveBom BomPost = new JobReceiveBom();
                                BomPost.JobReceiveBomId = -Cnt1;
                                BomPost.JobReceiveHeaderId = Header.JobReceiveHeaderId;
                                BomPost.JobReceiveLineId = line.JobReceiveLineId;
                                BomPost.CreatedBy = User.Identity.Name;
                                BomPost.CreatedDate = DateTime.Now;
                                BomPost.ModifiedBy = User.Identity.Name;
                                BomPost.ModifiedDate = DateTime.Now;
                                BomPost.ProductId = BomItem.ProductId;
                                BomPost.Qty = Convert.ToDecimal(BomItem.Qty);


                                StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
                                if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                {
                                    StockProcessBomViewModel.StockHeaderId = (int)Header.StockHeaderId;
                                }
                                else if (Settings.isPostedInStock ?? false)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                                {
                                    StockProcessBomViewModel.StockHeaderId = -1;
                                }
                                else if (Cnt1 > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                                {
                                    StockProcessBomViewModel.StockHeaderId = -1;
                                }
                                else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                {
                                    StockProcessBomViewModel.StockHeaderId = 0;
                                }

                                StockProcessBomViewModel.StockProcessId = -Cnt1;
                                StockProcessBomViewModel.DocHeaderId = Header.JobReceiveHeaderId;
                                StockProcessBomViewModel.DocLineId = line.JobReceiveLineId;
                                StockProcessBomViewModel.DocTypeId = Header.DocTypeId;
                                StockProcessBomViewModel.StockHeaderDocDate = Header.DocDate;
                                StockProcessBomViewModel.StockProcessDocDate = Header.DocDate;
                                StockProcessBomViewModel.DocNo = Header.DocNo;
                                StockProcessBomViewModel.DivisionId = Header.DivisionId;
                                StockProcessBomViewModel.SiteId = Header.SiteId;
                                StockProcessBomViewModel.CurrencyId = null;
                                StockProcessBomViewModel.HeaderProcessId = null;
                                StockProcessBomViewModel.PersonId = Header.JobWorkerId;
                                StockProcessBomViewModel.ProductId = BomItem.ProductId;
                                StockProcessBomViewModel.HeaderFromGodownId = null;
                                StockProcessBomViewModel.HeaderGodownId = null;
                                StockProcessBomViewModel.GodownId = Header.GodownId;
                                StockProcessBomViewModel.ProcessId = Header.ProcessId;
                                StockProcessBomViewModel.LotNo = null;
                                StockProcessBomViewModel.CostCenterId = JobOrderHeader.CostCenterId;
                                StockProcessBomViewModel.Qty_Iss = BomItem.Qty;
                                StockProcessBomViewModel.Qty_Rec = 0;
                                StockProcessBomViewModel.Rate = 0;
                                StockProcessBomViewModel.ExpiryDate = null;
                                StockProcessBomViewModel.Specification = null;
                                StockProcessBomViewModel.Dimension1Id = BomItem.Dimension1Id;
                                StockProcessBomViewModel.Dimension2Id = BomItem.Dimension2Id;
                                StockProcessBomViewModel.Dimension3Id = BomItem.Dimension3Id;
                                StockProcessBomViewModel.Dimension4Id = BomItem.Dimension4Id;
                                StockProcessBomViewModel.Remark = null;
                                StockProcessBomViewModel.Status = Header.Status;
                                StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                                StockProcessBomViewModel.CreatedDate = DateTime.Now;
                                StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                                StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                                string StockProcessPostingError = "";
                                StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                                if (StockProcessPostingError != "")
                                {
                                    ModelState.AddModelError("", StockProcessPostingError);
                                    //return PartialView("_Results", vm);
                                    if (vm.JobReceiveSettings.isVisibleLoss == true && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                    {
                                        return PartialView("_ResultsWithLossQty", vm);
                                    }
                                    if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                                    {
                                        return PartialView("_ResultsWithQty", vm);
                                    }
                                    if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == true)
                                    {
                                        return PartialView("_ResultsWithQtyLotNo", vm);
                                    }
                                    else
                                    {
                                        return PartialView("_Results", vm);
                                    }
                                }

                                BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;
                                BomPost.ObjectState = Model.ObjectState.Added;
                                db.JobReceiveBom.Add(BomPost);
                                //new JobReceiveBomService(_unitOfWork).Create(BomPost);


                                if (Settings.isPostedInStock == false && Settings.isPostedInStockProcess == false)
                                {
                                    if (Header.StockHeaderId == null)
                                    {
                                        Header.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
                                    }
                                }

                                Cnt1 = Cnt1 + 1;
                            }
                        }

                        #endregion


                        Cnt = Cnt + 1;

                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                Header.ObjectState = Model.ObjectState.Modified;
                db.JobReceiveHeader.Add(Header);
                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobReceiveDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                try
                {
                    if (EventException)
                    { throw new Exception(); }

                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    //return PartialView("_Results", vm);
                    if (vm.JobReceiveSettings.isVisibleLoss == true && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                    {
                        return PartialView("_ResultsWithLossQty", vm);
                    }
                    if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
                    {
                        return PartialView("_ResultsWithQty", vm);
                    }
                    if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == true)
                    {
                        return PartialView("_ResultsWithQtyLotNo", vm);
                    }
                    else
                    {
                        return PartialView("_Results", vm);
                    }
                }


                try
                {
                    JobReceiveDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                if (BarCodeBased != null && BarCodeBased.Count() > 0)
                {
                    List<BarCodeSequenceViewModelForReceive> Sequence = new List<BarCodeSequenceViewModelForReceive>();

                    foreach (var item in BarCodeBased)
                        Sequence.Add(new BarCodeSequenceViewModelForReceive
                        {
                            JobReceiveHeaderId = item.JobReceiveHeaderId,
                            JobOrderLineId = item.JobOrderLineId ?? 0,
                            ProductName = item.ProductName,
                            Qty = item.DocQty,
                            BalanceQty = item.OrderBalanceQty,
                            JobReceiveType = JobReceiveTypeConstants.ProductUIdHeaderId,
                            //FirstBarCode = _JobReceiveLineService.GetFirstBarCodeForCancel(item.JobOrderLineId),
                        });

                    BarCodeSequenceListViewModelForReceive SquenceList = new BarCodeSequenceListViewModelForReceive();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    HttpContext.Session.Remove("UIQHID" + vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId);

                    return PartialView("_Sequence", SquenceList);
                }

                if (BarCodeBasedBranch != null && BarCodeBasedBranch.Count() > 0)
                {
                    List<BarCodeSequenceViewModelForReceive> Sequence = new List<BarCodeSequenceViewModelForReceive>();

                    var Grouping = (from p in BarCodeBasedBranch.Where(m => m.ProductUidId != null && m.JobOrderUidHeaderId == null)
                                    group p by new { p.JobOrderHeaderId, p.ProductId, p.ProdOrderLineId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForReceive
                        {
                            JobReceiveHeaderId = item.Max(m => m.JobReceiveHeaderId),
                            //JobOrderLineId = item.JobOrderLineId,
                            JobOrdLineIds = string.Join(",", item.Select(m => m.JobOrderLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.OrderBalanceQty),
                            BalanceQty = item.Sum(m => m.OrderBalanceQty),
                            CostCenterName = item.Max(m => m.CostCenterName),
                            JobReceiveType = JobReceiveTypeConstants.ProductUid,
                            JobOrderHeaderId = item.Max(m => m.JobOrderHeaderId),
                        });

                    BarCodeSequenceListViewModelForReceive SquenceList = new BarCodeSequenceListViewModelForReceive();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    HttpContext.Session.Remove("UIQHID" + vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId);
                    HttpContext.Session.Remove("UIQUID" + vm.JobReceiveLineViewModel.FirstOrDefault().JobReceiveHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });
            }
            //return PartialView("_Results", vm);
            if (vm.JobReceiveSettings.isVisibleLoss == true && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
            {
                return PartialView("_ResultsWithLossQty", vm);
            }
            if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == false)
            {
                return PartialView("_ResultsWithQty", vm);
            }
            if (vm.JobReceiveSettings.isVisibleLoss == false && vm.JobReceiveSettings.IsVisiblePassQty == false && vm.JobReceiveSettings.isVisibleLotNo == true)
            {
                return PartialView("_ResultsWithQtyLotNo", vm);
            }
            else
            {
                return PartialView("_Results", vm);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _SequencePost(BarCodeSequenceListViewModelForReceive vm)
        {
            List<BarCodeSequenceViewModelForReceive> Seq = new List<BarCodeSequenceViewModelForReceive>();
            BarCodeSequenceListViewModelForReceive SquLis = new BarCodeSequenceListViewModelForReceive();

            JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobReceiveHeaderId);



            List<JobReceiveLineViewModel> BarCodeBasedBranch = new List<JobReceiveLineViewModel>();

            BarCodeBasedBranch = (List<JobReceiveLineViewModel>)HttpContext.Session["UIQUID" + vm.BarCodeSequenceViewModel.FirstOrDefault().JobReceiveHeaderId];

            HttpContext.Session.Remove("UIQUID" + vm.BarCodeSequenceViewModel.FirstOrDefault().JobReceiveHeaderId);


            if (BarCodeBasedBranch != null && BarCodeBasedBranch.Count() > 0)
            {

                HttpContext.Session["UIQHID" + vm.BarCodeSequenceViewModel.FirstOrDefault().JobReceiveHeaderId] = vm;


                List<BarCodeSequenceViewModelForReceive> Sequence = new List<BarCodeSequenceViewModelForReceive>();

                var Grouping = (from p in BarCodeBasedBranch.Where(m => m.ProductUidId != null && m.JobOrderUidHeaderId == null)
                                group p by new { p.JobOrderHeaderId, p.ProductId, p.ProdOrderLineId } into g
                                select g).ToList();

                foreach (var item in Grouping)
                    Sequence.Add(new BarCodeSequenceViewModelForReceive
                    {
                        JobReceiveHeaderId = item.Max(m => m.JobReceiveHeaderId),
                        JobOrdLineIds = string.Join(",", item.Select(m => m.JobOrderLineId).ToList()),
                        ProductName = item.Max(m => m.ProductName),
                        Qty = item.Sum(m => m.OrderBalanceQty),
                        BalanceQty = item.Sum(m => m.OrderBalanceQty),
                        CostCenterName = item.Max(m => m.CostCenterName),
                        JobReceiveType = JobReceiveTypeConstants.ProductUid,
                        JobOrderHeaderId = item.Max(m => m.JobOrderHeaderId),
                    });

                BarCodeSequenceListViewModelForReceive SquenceList = new BarCodeSequenceListViewModelForReceive();
                SquenceList.BarCodeSequenceViewModel = Sequence;


                return PartialView("_Sequence2", SquenceList);
            }




            var LineIds = vm.BarCodeSequenceViewModel.Select(m => m.JobOrderLineId).ToArray();

            foreach (var item in vm.BarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                int jOLID = item.JobOrderLineId;

                BarCodeSequenceViewModelForReceive SeqLi = new BarCodeSequenceViewModelForReceive();

                SeqLi.JobReceiveHeaderId = item.JobReceiveHeaderId;
                SeqLi.JobOrderLineId = item.JobOrderLineId;
                SeqLi.ProductName = item.ProductName;
                SeqLi.JobReceiveType = item.JobReceiveType;
                SeqLi.Qty = item.Qty;

                if (!string.IsNullOrEmpty(item.ProductUidIdName))
                {
                    var BarCodes = _JobReceiveLineService.GetPendingBarCodesList(SeqLi.JobOrderLineId);

                    var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SeqLi.ProductUidIds = Ids;
                }
                else
                {
                    var BarCodes = _JobReceiveLineService.GetPendingBarCodesList(SeqLi.JobOrderLineId);

                    var temp = BarCodes.Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SeqLi.ProductUidIds = Ids;
                }

                Seq.Add(SeqLi);

            }
            SquLis.BarCodeSequenceViewModelPost = Seq;
            return PartialView("_BarCodes", SquLis);
        }




        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _SequencePost2(BarCodeSequenceListViewModelForReceive vm)
        {
            List<BarCodeSequenceViewModelForReceive> Seq = new List<BarCodeSequenceViewModelForReceive>();
            BarCodeSequenceListViewModelForReceive SquLis = new BarCodeSequenceListViewModelForReceive();

            JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobReceiveHeaderId);


            List<BarCodeSequenceViewModelForReceive> BarCodeBased = new List<BarCodeSequenceViewModelForReceive>();

            BarCodeBased = (List<BarCodeSequenceViewModelForReceive>)HttpContext.Session["UIQHID" + vm.BarCodeSequenceViewModel.FirstOrDefault().JobReceiveHeaderId];


            var LineIds = vm.BarCodeSequenceViewModel.Select(m => m.JobOrderLineId).ToArray();

            if (BarCodeBased != null && BarCodeBased.Count() > 0)
                vm.BarCodeSequenceViewModel.AddRange(BarCodeBased);

            foreach (var item in vm.BarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                if (item.JobReceiveType == JobReceiveTypeConstants.ProductUIdHeaderId)
                {
                    int jOLID = item.JobOrderLineId;

                    BarCodeSequenceViewModelForReceive SeqLi = new BarCodeSequenceViewModelForReceive();

                    SeqLi.JobReceiveHeaderId = item.JobReceiveHeaderId;
                    SeqLi.JobOrderLineId = item.JobOrderLineId;
                    SeqLi.ProductName = item.ProductName;
                    SeqLi.JobReceiveType = item.JobReceiveType;
                    SeqLi.Qty = item.Qty;
                    SeqLi.JobOrderHeaderId = item.JobOrderHeaderId;

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = _JobReceiveLineService.GetPendingBarCodesList(SeqLi.JobOrderLineId);

                        var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    else
                    {
                        var BarCodes = _JobReceiveLineService.GetPendingBarCodesList(SeqLi.JobOrderLineId);

                        var temp = BarCodes.Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }

                    Seq.Add(SeqLi);
                }
                else if (item.JobReceiveType == JobReceiveTypeConstants.ProductUid)
                {
                    string jOLIDs = item.JobOrdLineIds;

                    BarCodeSequenceViewModelForReceive SeqLi = new BarCodeSequenceViewModelForReceive();

                    SeqLi.JobReceiveHeaderId = item.JobReceiveHeaderId;
                    SeqLi.JobOrdLineIds = item.JobOrdLineIds;
                    SeqLi.ProductName = item.ProductName;
                    SeqLi.Qty = item.Qty;
                    SeqLi.JobReceiveType = item.JobReceiveType;
                    SeqLi.JobOrderHeaderId = item.JobOrderHeaderId;

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = _JobReceiveLineService.GetPendingBarCodesList(SeqLi.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    else
                    {
                        var BarCodes = _JobReceiveLineService.GetPendingBarCodesList(SeqLi.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }

                    Seq.Add(SeqLi);

                }


            }
            SquLis.BarCodeSequenceViewModelPost = Seq;
            return PartialView("_BarCodes", SquLis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _BarCodesPost(BarCodeSequenceListViewModelForReceive vm)
        {

            int Cnt = 0;
            int Cnt1 = 0;
            int Pk = 0;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            int Serial = _JobReceiveLineService.GetMaxSr(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveHeaderId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            var JobOrderLineIds = vm.BarCodeSequenceViewModelPost.Select(m => m.JobOrderLineId).ToArray();

            var JobOrderLineRecords = (from p in db.JobOrderLine
                                       where JobOrderLineIds.Contains(p.JobOrderLineId)
                                       select p).ToList();

            var ProductUidJobOrderLineIds = string.Join(",", vm.BarCodeSequenceViewModelPost.Where(m => !string.IsNullOrEmpty(m.JobOrdLineIds)).Select(m => m.JobOrdLineIds));

            int[] ProdUidJobOrderLineIds;

            ProdUidJobOrderLineIds = string.IsNullOrEmpty(ProductUidJobOrderLineIds) ? new int[0] : ProductUidJobOrderLineIds.Split(',').Select(Int32.Parse).ToArray();

            var ProdUidJobOrderLineRecords = (from p in db.ViewJobOrderBalance
                                              join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                              where ProdUidJobOrderLineIds.Contains(p.JobOrderLineId)
                                              select t).ToList();
            var ProdUidJobOrderHeaderIds = ProdUidJobOrderLineRecords.GroupBy(m => m.JobOrderHeaderId).Select(m => m.Key).ToArray();

            var ProdUidJobOrderHeaderRecords = (from p in db.JobOrderHeader
                                                where ProdUidJobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                                select p).ToList();

            var JobOrderHeaderIds = JobOrderLineRecords.GroupBy(m => m.JobOrderHeaderId).Select(m => m.Key).ToArray();

            var JobOrderHeaderRecords = (from p in db.JobOrderHeader
                                         where JobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                         select p).ToList();

            var SProductUids = string.Join(",", vm.BarCodeSequenceViewModelPost.Select(m => m.ProductUidIds));

            var ProductUids = SProductUids.Split(',').Select(Int32.Parse).ToArray();

            var ProductUidRecords = (from p in db.ProductUid
                                     where ProductUids.Contains(p.ProductUIDId)
                                     select p).ToList();

            var BalCostCenterRecords = (from L in db.JobOrderLine
                                        join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                                        from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                                        where JobOrderLineIds.Contains(L.JobOrderLineId) || ProdUidJobOrderLineIds.Contains(L.JobOrderLineId)
                                        select new { CostCenterId = JobOrderHeaderTab.CostCenterId, Rate = L.Rate, JobOrderLineId = L.JobOrderLineId }).ToList();

            var BalanceQtyRecords = (from p in db.ViewJobOrderBalance
                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                     select new { BalQty = p.BalanceQty, JobOrderLineId = p.JobOrderLineId }).ToList();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveHeaderId);
                JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);


                foreach (var item in vm.BarCodeSequenceViewModelPost)
                {

                    JobOrderLine JobOrderLine = new JobOrderLine();
                    JobOrderHeader JobOrderHeader = new JobOrderHeader();
                    if (item.JobReceiveType == JobReceiveTypeConstants.ProductUIdHeaderId)
                    {
                        JobOrderLine = JobOrderLineRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault();
                        JobOrderHeader = JobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();
                    }

                    decimal balqty = 0;

                    if (item.JobReceiveType == JobReceiveTypeConstants.ProductUIdHeaderId)
                    {
                        balqty = (from L in BalanceQtyRecords
                                  where L.JobOrderLineId == item.JobOrderLineId
                                  select L.BalQty).FirstOrDefault();
                    }
                    else if (item.JobReceiveType == JobReceiveTypeConstants.ProductUid)
                    {
                        var LineIds = item.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray();
                        balqty = (from L in ProdUidJobOrderLineRecords
                                  where LineIds.Contains(L.JobOrderLineId)
                                  select L.Qty).Sum();
                    }


                    if (!string.IsNullOrEmpty(item.ProductUidIds) && item.ProductUidIds.Split(',').Count() <= balqty)
                    {

                        foreach (var BarCodes in item.ProductUidIds.Split(',').Select(Int32.Parse).ToList())
                        {
                            if (item.JobReceiveType == JobReceiveTypeConstants.ProductUid)
                            {
                                JobOrderLine = ProdUidJobOrderLineRecords.Where(m => m.ProductUidId == BarCodes && m.JobOrderHeaderId == item.JobOrderHeaderId).FirstOrDefault();
                                JobOrderHeader = ProdUidJobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();
                            }

                            var temp = (from p in BalCostCenterRecords
                                        where p.JobOrderLineId == JobOrderLine.JobOrderLineId
                                        select p).FirstOrDefault();

                            JobReceiveLine line = new JobReceiveLine();

                            line.JobReceiveHeaderId = item.JobReceiveHeaderId;
                            if (item.JobReceiveType == JobReceiveTypeConstants.ProductUIdHeaderId)
                            { line.JobOrderLineId = item.JobOrderLineId; }
                            else if (item.JobReceiveType == JobReceiveTypeConstants.ProductUid)
                            {
                                line.JobOrderLineId = JobOrderLine.JobOrderLineId;
                            }
                            line.ProductUidId = BarCodes;
                            line.ProductId = JobOrderLine.ProductId;
                            line.Dimension1Id = JobOrderLine.Dimension1Id;
                            line.Dimension2Id = JobOrderLine.Dimension2Id;
                            line.Dimension3Id = JobOrderLine.Dimension3Id;
                            line.Dimension4Id = JobOrderLine.Dimension4Id;
                            line.Qty = 1;
                            line.PassQty = line.Qty;
                            line.DealUnitId = JobOrderLine.DealUnitId;
                            line.UnitConversionMultiplier = JobOrderLine.UnitConversionMultiplier;
                            line.DealQty = (line.Qty * line.UnitConversionMultiplier);
                            line.Sr = Serial++;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobReceiveLineId = Pk++;

                            if (line.JobOrderLineId != null)
                            {
                                if (LineStatus.ContainsKey((int)line.JobOrderLineId))
                                {
                                    LineStatus[(int)line.JobOrderLineId] = LineStatus[(int)line.JobOrderLineId] + 1;
                                }
                                else
                                {
                                    LineStatus.Add((int)line.JobOrderLineId, line.Qty + line.LossQty);
                                }
                            }


                            if (JobOrderLine.ProductUidHeaderId.HasValue && JobOrderLine.ProductUidHeaderId.Value > 0)
                            {

                                ProductUid Uid = ProductUidRecords.Where(m => m.ProductUIDId == BarCodes).FirstOrDefault();

                                line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                                line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                                line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                                line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                                line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                                line.ProductUidStatus = Uid.Status;
                                line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                                line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                                if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId == 17)
                                {

                                    Uid.LastTransactionDocId = Header.JobReceiveHeaderId;
                                    Uid.LastTransactionDocNo = Header.DocNo;
                                    Uid.LastTransactionDocTypeId = Header.DocTypeId;
                                    Uid.LastTransactionDocDate = Header.DocDate;
                                    Uid.LastTransactionPersonId = Header.JobWorkerId;
                                    Uid.CurrenctGodownId = Header.GodownId;
                                    Uid.Status = (!string.IsNullOrEmpty(Settings.BarcodeStatusUpdate) ? Settings.BarcodeStatusUpdate : ProductUidStatusConstants.Receive);
                                    if (Uid.ProcessesDone == null)
                                    {
                                        Uid.ProcessesDone = "|" + Header.ProcessId.ToString() + "|";
                                    }
                                    else
                                    {
                                        Uid.ProcessesDone = Uid.ProcessesDone + ",|" + Header.ProcessId.ToString() + "|";
                                    }

                                    Uid.ObjectState = Model.ObjectState.Modified;
                                    db.ProductUid.Add(Uid);
                                }
                                else
                                {
                                    var MainJobRec = (from p in db.JobReceiveLine
                                                      join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                                                      join d in db.DocumentType on t.DocTypeId equals d.DocumentTypeId
                                                      where p.ProductUidId == BarCodes && t.SiteId != Header.SiteId && d.DocumentTypeName == TransactionDoctypeConstants.WeavingBazar
                                                      select p).ToList().LastOrDefault();

                                    if (MainJobRec != null)
                                    {
                                        MainJobRec.LockReason = "Received in Branch";
                                        MainJobRec.ObjectState = Model.ObjectState.Modified;
                                        db.JobReceiveLine.Add(MainJobRec);
                                    }
                                }


                            }

                            if (JobOrderLine.ProductUidId.HasValue && JobOrderLine.ProductUidId.Value > 0)
                            {

                                ProductUid Uid = ProductUidRecords.Where(m => m.ProductUIDId == BarCodes).FirstOrDefault();

                                line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                                line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                                line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                                line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                                line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                                line.ProductUidStatus = Uid.Status;
                                line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                                line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;

                                if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId == 17)
                                {

                                    Uid.LastTransactionDocId = Header.JobReceiveHeaderId;
                                    Uid.LastTransactionDocNo = Header.DocNo;
                                    Uid.LastTransactionDocTypeId = Header.DocTypeId;
                                    Uid.LastTransactionDocDate = Header.DocDate;
                                    Uid.LastTransactionPersonId = Header.JobWorkerId;
                                    Uid.CurrenctGodownId = Header.GodownId;
                                    Uid.CurrenctProcessId = Header.ProcessId;
                                    Uid.Status = ProductUidStatusConstants.Receive;
                                    if (Uid.ProcessesDone == null)
                                    {
                                        Uid.ProcessesDone = "|" + Header.ProcessId.ToString() + "|";
                                    }
                                    else
                                    {
                                        Uid.ProcessesDone = Uid.ProcessesDone + ",|" + Header.ProcessId.ToString() + "|";
                                    }

                                    Uid.ObjectState = Model.ObjectState.Modified;
                                    db.ProductUid.Add(Uid);

                                }
                                else
                                {
                                    var MainJobRec = (from p in db.JobReceiveLine
                                                      join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                                                      join d in db.DocumentType on t.DocTypeId equals d.DocumentTypeId
                                                      where p.ProductUidId == BarCodes && t.SiteId != Header.SiteId && d.DocumentTypeName == TransactionDoctypeConstants.WeavingBazar
                                                      select p).ToList().LastOrDefault();

                                    if (MainJobRec != null)
                                    {
                                        MainJobRec.LockReason = "Received in Branch";
                                        MainJobRec.ObjectState = Model.ObjectState.Modified;
                                        db.JobReceiveLine.Add(MainJobRec);
                                    }
                                }

                            }


                            if (Settings.isPostedInStock ?? false)
                            {
                                StockViewModel StockViewModel = new StockViewModel();

                                if (Cnt == 0)
                                {
                                    StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                                }
                                else
                                {
                                    if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
                                    {
                                        StockViewModel.StockHeaderId = (int)Header.StockHeaderId;
                                    }
                                    else
                                    {
                                        StockViewModel.StockHeaderId = -1;
                                    }
                                }

                                StockViewModel.StockId = -Cnt;
                                StockViewModel.DocHeaderId = Header.JobReceiveHeaderId;
                                StockViewModel.DocLineId = line.JobReceiveLineId;
                                StockViewModel.DocTypeId = Header.DocTypeId;
                                StockViewModel.StockHeaderDocDate = Header.DocDate;
                                StockViewModel.StockDocDate = Header.DocDate;
                                StockViewModel.DocNo = Header.DocNo;
                                StockViewModel.DivisionId = Header.DivisionId;
                                StockViewModel.SiteId = Header.SiteId;
                                StockViewModel.CurrencyId = null;
                                StockViewModel.PersonId = Header.JobWorkerId;
                                StockViewModel.ProductId = JobOrderLine.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                StockViewModel.HeaderGodownId = Header.GodownId;
                                StockViewModel.HeaderProcessId = Header.ProcessId;
                                StockViewModel.GodownId = (int)Header.GodownId;
                                StockViewModel.Remark = Header.Remark;
                                StockViewModel.Status = Header.Status;
                                StockViewModel.ProcessId = Header.ProcessId;
                                StockViewModel.LotNo = null;

                                if (temp != null)
                                {
                                    StockViewModel.CostCenterId = temp.CostCenterId;
                                    StockViewModel.Rate = temp.Rate;
                                }
                                StockViewModel.Qty_Iss = 0;
                                StockViewModel.Qty_Rec = line.Qty;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = JobOrderLine.Specification;
                                StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                                StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                                StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                                StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                                StockViewModel.ProductUidId = line.ProductUidId;
                                StockViewModel.CreatedBy = User.Identity.Name;
                                StockViewModel.CreatedDate = DateTime.Now;
                                StockViewModel.ModifiedBy = User.Identity.Name;
                                StockViewModel.ModifiedDate = DateTime.Now;

                                string StockPostingError = "";
                                StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                                if (StockPostingError != "")
                                {
                                    string message = StockPostingError;
                                    ModelState.AddModelError("", message);
                                    return PartialView("_Results", vm);
                                }

                                if (Cnt == 0)
                                {
                                    Header.StockHeaderId = StockViewModel.StockHeaderId;
                                }
                                line.StockId = StockViewModel.StockId;
                            }


                            if (Settings.isPostedInStockProcess ?? false)
                            {
                                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                                if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                {
                                    StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
                                }
                                else if (Settings.isPostedInStock ?? false)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                                {
                                    StockProcessViewModel.StockHeaderId = -1;
                                }
                                else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                                {
                                    StockProcessViewModel.StockHeaderId = -1;
                                }
                                else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                {
                                    StockProcessViewModel.StockHeaderId = 0;
                                }

                                StockProcessViewModel.StockProcessId = -Cnt;
                                StockProcessViewModel.DocHeaderId = Header.JobReceiveHeaderId;
                                StockProcessViewModel.DocLineId = line.JobReceiveLineId;
                                StockProcessViewModel.DocTypeId = Header.DocTypeId;
                                StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                                StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                                StockProcessViewModel.DocNo = Header.DocNo;
                                StockProcessViewModel.DivisionId = Header.DivisionId;
                                StockProcessViewModel.SiteId = Header.SiteId;
                                StockProcessViewModel.CurrencyId = null;
                                StockProcessViewModel.PersonId = Header.JobWorkerId;
                                StockProcessViewModel.ProductId = JobOrderLine.ProductId;
                                StockProcessViewModel.HeaderFromGodownId = null;
                                StockProcessViewModel.HeaderGodownId = Header.GodownId;
                                StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                                StockProcessViewModel.GodownId = (int)Header.GodownId;
                                StockProcessViewModel.Remark = Header.Remark;
                                StockProcessViewModel.Status = Header.Status;
                                StockProcessViewModel.ProcessId = Header.ProcessId;
                                StockProcessViewModel.LotNo = null;

                                if (temp != null)
                                {
                                    StockProcessViewModel.CostCenterId = temp.CostCenterId;
                                    StockProcessViewModel.Rate = temp.Rate;
                                }
                                StockProcessViewModel.Qty_Iss = line.Qty + line.LossQty;
                                StockProcessViewModel.Qty_Rec = 0;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = JobOrderLine.Specification;
                                StockProcessViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                                StockProcessViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                                StockProcessViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                                StockProcessViewModel.ProductUidId = line.ProductUidId;
                                StockProcessViewModel.CreatedBy = User.Identity.Name;
                                StockProcessViewModel.CreatedDate = DateTime.Now;
                                StockProcessViewModel.ModifiedBy = User.Identity.Name;
                                StockProcessViewModel.ModifiedDate = DateTime.Now;

                                string StockProcessPostingError = "";
                                StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                                if (StockProcessPostingError != "")
                                {
                                    string message = StockProcessPostingError;
                                    ModelState.AddModelError("", message);
                                    return PartialView("_Results", vm);
                                }

                                if ((Settings.isPostedInStock ?? false) == false)
                                {
                                    if (Cnt == 0)
                                    {
                                        Header.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                    }
                                }

                                line.StockProcessId = StockProcessViewModel.StockProcessId;
                            }


                            line.ObjectState = Model.ObjectState.Added;
                            db.JobReceiveLine.Add(line);

                            new JobReceiveLineStatusService(_unitOfWork).CreateLineStatus(line.JobReceiveLineId, ref db, true);

                            // _JobOrderCancelLineService.Create(line);

                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            if (!string.IsNullOrEmpty(Settings.SqlProcConsumption))
                            {
                                var BomPostList = _JobReceiveLineService.GetBomPostingDataForWeaving(JobOrderLine.ProductId, JobOrderLine.Dimension1Id, JobOrderLine.Dimension2Id, JobOrderLine.Dimension3Id, JobOrderLine.Dimension4Id, Header.ProcessId, line.Qty, Header.DocTypeId, Settings.SqlProcConsumption, line.JobOrderLineId, line.Weight).ToList();

                                foreach (var BomItem in BomPostList)
                                {
                                    JobReceiveBom BomPost = new JobReceiveBom();
                                    BomPost.JobReceiveBomId = -Cnt1;
                                    BomPost.JobReceiveHeaderId = Header.JobReceiveHeaderId;
                                    BomPost.JobReceiveLineId = line.JobReceiveLineId;
                                    BomPost.CreatedBy = User.Identity.Name;
                                    BomPost.CreatedDate = DateTime.Now;
                                    BomPost.ModifiedBy = User.Identity.Name;
                                    BomPost.ModifiedDate = DateTime.Now;
                                    BomPost.ProductId = BomItem.ProductId;
                                    BomPost.Qty = Convert.ToDecimal(BomItem.Qty);


                                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
                                    if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = (int)Header.StockHeaderId;
                                    }
                                    else if (Settings.isPostedInStock ?? false)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = -1;
                                    }
                                    else if (Cnt1 > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                                    {
                                        StockProcessBomViewModel.StockHeaderId = -1;
                                    }
                                    else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = 0;
                                    }

                                    StockProcessBomViewModel.StockProcessId = -Cnt1;
                                    StockProcessBomViewModel.DocHeaderId = Header.JobReceiveHeaderId;
                                    StockProcessBomViewModel.DocLineId = line.JobReceiveLineId;
                                    StockProcessBomViewModel.DocTypeId = Header.DocTypeId;
                                    StockProcessBomViewModel.StockHeaderDocDate = Header.DocDate;
                                    StockProcessBomViewModel.StockProcessDocDate = Header.DocDate;
                                    StockProcessBomViewModel.DocNo = Header.DocNo;
                                    StockProcessBomViewModel.DivisionId = Header.DivisionId;
                                    StockProcessBomViewModel.SiteId = Header.SiteId;
                                    StockProcessBomViewModel.CurrencyId = null;
                                    StockProcessBomViewModel.HeaderProcessId = null;
                                    StockProcessBomViewModel.PersonId = Header.JobWorkerId;
                                    StockProcessBomViewModel.ProductId = BomItem.ProductId;
                                    StockProcessBomViewModel.HeaderFromGodownId = null;
                                    StockProcessBomViewModel.HeaderGodownId = null;
                                    StockProcessBomViewModel.GodownId = Header.GodownId;
                                    StockProcessBomViewModel.ProcessId = Header.ProcessId;
                                    StockProcessBomViewModel.LotNo = null;
                                    StockProcessBomViewModel.CostCenterId = JobOrderHeader.CostCenterId;
                                    StockProcessBomViewModel.Qty_Iss = BomItem.Qty;
                                    StockProcessBomViewModel.Qty_Rec = 0;
                                    StockProcessBomViewModel.Rate = 0;
                                    StockProcessBomViewModel.ExpiryDate = null;
                                    StockProcessBomViewModel.Specification = null;
                                    StockProcessBomViewModel.Dimension1Id = BomItem.Dimension1Id;
                                    StockProcessBomViewModel.Dimension2Id = BomItem.Dimension2Id;
                                    StockProcessBomViewModel.Dimension3Id = BomItem.Dimension3Id;
                                    StockProcessBomViewModel.Dimension4Id = BomItem.Dimension4Id;
                                    StockProcessBomViewModel.Remark = null;
                                    StockProcessBomViewModel.Status = Header.Status;
                                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                                    string StockProcessPostingError = "";
                                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                                    if (StockProcessPostingError != "")
                                    {
                                        ModelState.AddModelError("", StockProcessPostingError);
                                        return PartialView("_Results", vm);
                                    }

                                    BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;
                                    BomPost.ObjectState = Model.ObjectState.Added;
                                    db.JobReceiveBom.Add(BomPost);


                                    if (Settings.isPostedInStock == false && Settings.isPostedInStockProcess == false)
                                    {
                                        if (Header.StockHeaderId == null)
                                        {
                                            Header.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
                                        }
                                    }

                                    Cnt1 = Cnt1 + 1;
                                }
                            }
                            Cnt = Cnt + 1;
                        }
                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ModifiedBy = User.Identity.Name;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobReceiveHeader.Add(Header);
                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobReceiveDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                try
                {
                    if (EventException)
                    { throw new Exception((string)TempData["CSEXC"]); }

                    db.SaveChanges();
                    db.Dispose();
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_BarCodes", vm);
                }

                try
                {
                    JobReceiveDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_BarCodes", vm);

        }


        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _JobReceiveLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;            
            ViewBag.JobReceiveHeaderId = id;
            var p = _JobReceiveLineService.GetLineListForIndex(id).ToList();
            return PartialView(p);
        }

        [HttpGet]
        public JsonResult ConsumptionIndex(int id)
        {
            var p = _JobReceiveLineService.GetConsumptionLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult ByProductIndex(int id)
        {
            var p = _JobReceiveLineService.GetByProductListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(JobReceiveLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            var temp = new JobReceiveHeaderService(_unitOfWork).GetJobReceiveHeader(vm.JobReceiveHeaderId);
            ViewBag.DocNo = temp.DocTypeName + "-" + temp.DocNo;
        }

        [HttpGet]
        public ActionResult CreateLine(int Id, int JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int Id, int JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        public ActionResult _Create(int Id, int JobWorkerId) //Id ==> Header Id
        {
            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(Id);
            JobReceiveLineViewModel s = new JobReceiveLineViewModel();

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.JobReceiveHeaderId = Id;
            s.JobReceiveHeaderDocNo = H.DocNo;
            s.JobWorkerId = JobWorkerId;
            PrepareViewBag(s);
            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }
            ViewBag.LineMode = "Create";
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobReceiveLineViewModel svm)
        {
            JobReceiveLine s = Mapper.Map<JobReceiveLineViewModel, JobReceiveLine>(svm);
            JobReceiveHeader temp = new JobReceiveHeaderService(_unitOfWork).Find(s.JobReceiveHeaderId);
            bool BeforeSave = true;
            try
            {

                if (svm.JobReceiveLineId <= 0)
                    BeforeSave = JobReceiveDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReceiveLineId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobReceiveDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReceiveLineId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (svm.JobReceiveLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.DocQty <= 0)
                ModelState.AddModelError("DocQty", "The Job Qty field is required");

            if (settings.IsVisibleWeight == true && settings.IsMandatoryWeight == true && svm.Weight <= 0)
                ModelState.AddModelError("Weight", "The Weight filed is required.");

            if (svm.ReceiveQty <= 0)
                ModelState.AddModelError("ReceiveQty", "The Rec Qty field is required");

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation Error before save.");

            if (svm.DealQty <= 0 && svm.PassQty > 0)
            {
                ModelState.AddModelError("DealQty", "DealQty field is required");
            }

            if (svm.JobReceiveSettings.isVisibleMachine && svm.JobReceiveSettings.isMandatoryMachine && (svm.MachineId <= 0 || svm.MachineId == null))
            {
                ModelState.AddModelError("MachineId", "The Machine field is required");
            }

            //if (svm.OrderBalanceQty < svm.DocQty)
            //{
            //    ModelState.AddModelError("DocQty", "DocQty exceeding BalanceQty");
            //}

            if (svm.ProductUidName != null && svm.ProductUidName != "")
            {
                var Productids = (from p in db.ProductUid where p.ProductUidName == svm.ProductUidName select p).FirstOrDefault();
                if (Productids != null)
                {
                    var Productuids = new JobReceiveLineService(_unitOfWork).ProductUidsExist(s.JobReceiveHeaderId, Productids.ProductUIDId).FirstOrDefault();
                    if (Productuids != null)
                    {
                        ModelState.AddModelError("ProductUidId", "Already Received");
                    }
                }
            }


            if (settings.LossPer != null)
            {
                if (svm.LossQty > (svm.DocQty * (int)settings.LossPer / 100))
                {
                    ModelState.AddModelError("LossQty", "Loss Qty exceeding allowed loss % [" + settings.LossPer.ToString() + "]");
                }
            }


            if (svm.JobOrderLineId != null)
            {
                if (svm.ProductId != 0 && svm.JobOrderLineId != null && svm.JobOrderLineId != 0)
                {
                    if (svm.OrderBalanceQty < svm.ReceiveQty)
                    {
                        Decimal? ExcessAllowedQty = new JobOrderLineService(_unitOfWork).GetExcessReceiveAllowedAgainstOrderQty((int)svm.JobOrderLineId);
                        if (ExcessAllowedQty != null)
                        {
                            Decimal TotalReceiveQty = 0;
                            var ReceiveLinesList = (from L in db.JobReceiveLine
                                                    where L.JobOrderLineId == svm.JobOrderLineId
                                                    group new { L } by new { L.JobOrderLineId } into Result
                                                    select new
                                                    {
                                                        Qty = Result.Sum(m => m.L.Qty)
                                                    }).FirstOrDefault();
                            if (ReceiveLinesList != null)
                                TotalReceiveQty = ReceiveLinesList.Qty;

                            TotalReceiveQty = TotalReceiveQty + svm.ReceiveQty;
                            var JobOrderLine = new JobOrderLineService(_unitOfWork).Find((int)svm.JobOrderLineId);
                            Decimal ExcessQty = TotalReceiveQty - JobOrderLine.Qty;

                            if (ExcessQty > ExcessAllowedQty)
                            {
                                ModelState.AddModelError("Qty", "Qty exceeding allowed excess receive qty for product.");
                            }
                        }
                    }
                }
            }

            //if (svm.ProductId != 0 && svm.JobOrderLineId != null && svm.JobOrderLineId != 0)
            //{
            //    var ProductSiteDetail = (from Ps in db.ProductSiteDetail where Ps.ProductId == svm.ProductId && Ps.SiteId == temp.SiteId && Ps.DivisionId == temp.DivisionId && Ps.ProcessId == temp.ProcessId select Ps).FirstOrDefault();
            //    var JobOrderLine = new JobOrderLineService(_unitOfWork).Find(svm.JobOrderLineId);
            //    Decimal TotalReceiveQty = 0;
            //    var ReceiveLinesList = (from L in db.JobReceiveLine
            //                       where L.JobOrderLineId == svm.JobOrderLineId
            //                       group new { L } by new { L.JobOrderLineId } into Result
            //                       select new
            //                       {
            //                           Qty = Result.Sum(m => m.L.Qty)
            //                       }).FirstOrDefault();
            //    if (ReceiveLinesList != null)
            //        TotalReceiveQty = ReceiveLinesList.Qty;

            //    TotalReceiveQty = TotalReceiveQty + svm.ReceiveQty;

            //    if (ProductSiteDetail != null && JobOrderLine != null)
            //    {
            //        if (JobOrderLine.Qty < TotalReceiveQty)
            //        {
            //            if (ProductSiteDetail.ExcessReceiveAllowedAgainstOrderPer != null || ProductSiteDetail.ExcessReceiveAllowedAgainstOrderPer != 0 || ProductSiteDetail.ExcessReceiveAllowedAgainstOrderQty != 0 || ProductSiteDetail.ExcessReceiveAllowedAgainstOrderQty != 0)
            //            {
            //                Decimal ExcessQty = TotalReceiveQty - JobOrderLine.Qty;
            //                Decimal ExcessAllowedWithPer = JobOrderLine.Qty * (ProductSiteDetail.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100;
            //                Decimal ExcessAllowedWithQty = ProductSiteDetail.ExcessReceiveAllowedAgainstOrderQty ?? 0;
            //                Decimal ExcessAllowedQty = 0;
            //                if (ExcessAllowedWithPer > ExcessAllowedWithQty)
            //                    ExcessAllowedQty = ExcessAllowedWithPer;
            //                else
            //                    ExcessAllowedQty = ExcessAllowedWithQty;

            //                if (ExcessQty > ExcessAllowedQty)
            //                {
            //                    ModelState.AddModelError("Qty", "Qty exceeding allowed excess receive qty for product.");
            //                }
            //            }
            //        }
            //    }
            //}



            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.JobReceiveLineId <= 0)
                {
                    StockViewModel StockViewModel = new StockViewModel();
                    StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                    //JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(s.JobOrderLineId);
                    //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);

                    s.Qty = svm.ReceiveQty;
                    s.Sr = _JobReceiveLineService.GetMaxSr(s.JobReceiveHeaderId);
                    s.LossQty = svm.LossQty;
                    s.PassQty = svm.PassQty;
                    s.DealUnitId = svm.DealUnitId;
                    s.DealQty = svm.DealQty;
                    s.LotNo  = svm.LotNo;
                    s.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    s.IncentiveAmt = svm.IncentiveAmt;
                    s.IncentiveRate = svm.IncentiveRate;
                    s.PenaltyAmt = svm.PenaltyAmt;
                    s.PenaltyRate = svm.PenaltyRate;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;

                    decimal SettingsStockQty = (decimal)GetStockQtyFromSettings(s, settings.StockQty, "Qty");

                    if (s.JobOrderLineId != null)
                    {
                        new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReceive((int)s.JobOrderLineId, s.JobReceiveLineId, temp.DocDate, s.Qty + s.LossQty, ref db);
                    }

                    //Posting in Stock
                    if (svm.JobReceiveSettings.isPostedInStock)
                    {
                        StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockViewModel.DocHeaderId = temp.JobReceiveHeaderId;
                        StockViewModel.DocLineId = s.JobReceiveLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = temp.ProcessId;
                        StockViewModel.PersonId = temp.JobWorkerId;
                        StockViewModel.ProductId = svm.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = temp.GodownId;
                        StockViewModel.ProcessId = temp.ProcessId;
                        StockViewModel.LotNo = s.LotNo;
                        StockViewModel.CostCenterId = svm.CostCenterId;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = SettingsStockQty;
                        StockViewModel.Rate = svm.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = svm.Specification;
                        StockViewModel.Dimension1Id = svm.Dimension1Id;
                        StockViewModel.Dimension2Id = svm.Dimension2Id;
                        StockViewModel.Dimension3Id = svm.Dimension3Id;
                        StockViewModel.Dimension4Id = svm.Dimension4Id;
                        StockViewModel.Remark = s.Remark;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.ProductUidId = s.ProductUidId;
                        StockViewModel.CreatedBy = temp.CreatedBy;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = temp.ModifiedBy;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }

                        s.StockId = StockViewModel.StockId;

                        if (temp.StockHeaderId == null)
                        {
                            temp.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                    }



                    //Posting in StockProcess
                    if (svm.JobReceiveSettings.isPostedInStockProcess)
                    {
                        if (temp.StockHeaderId != null && temp.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessViewModel.StockHeaderId = (int)temp.StockHeaderId;
                        }
                        else if (svm.JobReceiveSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                        {
                            StockProcessViewModel.StockHeaderId = -1;
                        }
                        else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                        {
                            StockProcessViewModel.StockHeaderId = 0;
                        }
                        StockProcessViewModel.DocHeaderId = temp.JobReceiveHeaderId;
                        StockProcessViewModel.DocLineId = s.JobReceiveLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.JobWorkerId;
                        StockProcessViewModel.ProductId = svm.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = null;
                        StockProcessViewModel.GodownId = temp.GodownId;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = s.LotNo;
                        StockProcessViewModel.CostCenterId = svm.CostCenterId;
                        StockProcessViewModel.Qty_Iss = SettingsStockQty + svm.LossQty;
                        StockProcessViewModel.Qty_Rec = 0;
                        StockProcessViewModel.Rate = svm.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = svm.Specification;
                        StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = svm.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = svm.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.ProductUidId = s.ProductUidId;
                        StockProcessViewModel.CreatedBy = temp.CreatedBy;
                        StockProcessViewModel.CreatedDate = DateTime.Now;
                        StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }

                        s.StockProcessId = StockProcessViewModel.StockProcessId;

                        if (svm.JobReceiveSettings.isPostedInStock == false)
                        {
                            if (temp.StockHeaderId == null)
                            {
                                temp.StockHeaderId = StockProcessViewModel.StockHeaderId;
                            }
                        }
                    }


                    //_JobReceiveLineService.Create(s);





                    //JobOrderLine JOline = new JobOrderLineService(_unitOfWork).Find(s.JobOrderLineId);

                    #region BomPost

                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    if (!string.IsNullOrEmpty(svm.JobReceiveSettings.SqlProcConsumption))
                    {
                        var BomPostList = _JobReceiveLineService.GetBomPostingDataForWeaving(svm.ProductId, svm.Dimension1Id, svm.Dimension2Id, svm.Dimension3Id, svm.Dimension4Id, temp.ProcessId, s.Qty + s.LossQty, temp.DocTypeId, svm.JobReceiveSettings.SqlProcConsumption, s.JobOrderLineId, s.Weight).ToList();

                        int Cnt1 = 0;
                        foreach (var item in BomPostList)
                        {
                            JobReceiveBom BomPost = new JobReceiveBom();
                            BomPost.JobReceiveHeaderId = temp.JobReceiveHeaderId;
                            BomPost.JobReceiveLineId = s.JobReceiveLineId;
                            BomPost.CreatedBy = User.Identity.Name;
                            BomPost.CreatedDate = DateTime.Now;
                            BomPost.ModifiedBy = User.Identity.Name;
                            BomPost.ModifiedDate = DateTime.Now;
                            BomPost.ProductId = item.ProductId;
                            BomPost.Qty = Convert.ToDecimal(item.Qty);


                            StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
                            if (temp.StockHeaderId != null && temp.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessBomViewModel.StockHeaderId = (int)temp.StockHeaderId;
                            }
                            else if (svm.JobReceiveSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                            {
                                StockProcessBomViewModel.StockHeaderId = -1;
                            }
                            else if (Cnt1 > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                            {
                                StockProcessBomViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessBomViewModel.StockHeaderId = 0;
                            }
                            StockProcessBomViewModel.StockProcessId = -Cnt1;
                            StockProcessBomViewModel.DocHeaderId = temp.JobReceiveHeaderId;
                            StockProcessBomViewModel.DocLineId = s.JobReceiveLineId;
                            StockProcessBomViewModel.DocTypeId = temp.DocTypeId;
                            StockProcessBomViewModel.StockHeaderDocDate = temp.DocDate;
                            StockProcessBomViewModel.StockProcessDocDate = temp.DocDate;
                            StockProcessBomViewModel.DocNo = temp.DocNo;
                            StockProcessBomViewModel.DivisionId = temp.DivisionId;
                            StockProcessBomViewModel.SiteId = temp.SiteId;
                            StockProcessBomViewModel.CurrencyId = null;
                            StockProcessBomViewModel.HeaderProcessId = null;
                            StockProcessBomViewModel.PersonId = temp.JobWorkerId;
                            StockProcessBomViewModel.ProductId = item.ProductId;
                            StockProcessBomViewModel.HeaderFromGodownId = null;
                            StockProcessBomViewModel.HeaderGodownId = null;
                            StockProcessBomViewModel.GodownId = temp.GodownId;
                            StockProcessBomViewModel.ProcessId = temp.ProcessId;
                            StockProcessBomViewModel.LotNo = s.LotNo;
                            StockProcessBomViewModel.CostCenterId = svm.CostCenterId;
                            StockProcessBomViewModel.Qty_Iss = item.Qty;
                            StockProcessBomViewModel.Qty_Rec = 0;
                            StockProcessBomViewModel.Rate = 0;
                            StockProcessBomViewModel.ExpiryDate = null;
                            StockProcessBomViewModel.Specification = null;
                            StockProcessBomViewModel.Dimension1Id = null;
                            StockProcessBomViewModel.Dimension2Id = null;
                            StockProcessBomViewModel.Dimension3Id = null;
                            StockProcessBomViewModel.Dimension4Id = null;
                            StockProcessBomViewModel.Remark = null;
                            StockProcessBomViewModel.Status = temp.Status;
                            StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                            StockProcessBomViewModel.CreatedDate = DateTime.Now;
                            StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                            StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                ModelState.AddModelError("", StockProcessPostingError);
                                return PartialView("_Create", svm);
                            }

                            BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;
                            BomPost.ObjectState = Model.ObjectState.Added;
                            //new JobReceiveBomService(_unitOfWork).Create(BomPost);
                            db.JobReceiveBom.Add(BomPost);

                            if (svm.JobReceiveSettings.isPostedInStock == false && svm.JobReceiveSettings.isPostedInStockProcess == false)
                            {
                                if (temp.StockHeaderId == null)
                                {
                                    temp.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
                                }
                            }

                            Cnt1 = Cnt1 + 1;
                        }
                    }

                    #endregion


                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                    }

                    //new JobReceiveHeaderService(_unitOfWork).Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveHeader.Add(temp);


                    if (svm.ProductUidId != null && svm.ProductUidId > 0)
                    {
                        //ProductUid Produid = new ProductUidService(_unitOfWork).Find(svm.ProductUidId ?? 0);
                        ProductUid Produid = (from p in db.ProductUid
                                              where p.ProductUIDId == svm.ProductUidId
                                              select p).FirstOrDefault();



                            s.ProductUidLastTransactionDocId = Produid.LastTransactionDocId;
                            s.ProductUidLastTransactionDocDate = Produid.LastTransactionDocDate;
                            s.ProductUidLastTransactionDocNo = Produid.LastTransactionDocNo;
                            s.ProductUidLastTransactionDocTypeId = Produid.LastTransactionDocTypeId;
                            s.ProductUidLastTransactionPersonId = Produid.LastTransactionPersonId;
                            s.ProductUidStatus = Produid.Status;
                            s.ProductUidCurrentProcessId = Produid.CurrenctProcessId;
                            s.ProductUidCurrentGodownId = Produid.CurrenctGodownId;



                            Produid.LastTransactionDocId = temp.JobReceiveHeaderId;
                            Produid.LastTransactionDocNo = temp.DocNo;
                            Produid.LastTransactionDocTypeId = temp.DocTypeId;
                            Produid.LastTransactionDocDate = temp.DocDate;
                            Produid.LastTransactionPersonId = temp.JobWorkerId;
                            Produid.CurrenctGodownId = temp.GodownId;
                            Produid.CurrenctProcessId = temp.ProcessId;
                            Produid.Status = (!string.IsNullOrEmpty(settings.BarcodeStatusUpdate) ? settings.BarcodeStatusUpdate : ProductUidStatusConstants.Receive);

                            if (Produid.ProcessesDone == null)
                            {
                                Produid.ProcessesDone = "|" + temp.ProcessId.ToString() + "|";
                            }
                            else
                            {
                                Produid.ProcessesDone = Produid.ProcessesDone + ",|" + temp.ProcessId.ToString() + "|";
                            }

                            Produid.ObjectState = Model.ObjectState.Modified;
                            db.ProductUid.Add(Produid);
                        
                    }
                    s.ObjectState = Model.ObjectState.Added;
                    db.JobReceiveLine.Add(s);

                    new JobReceiveLineStatusService(_unitOfWork).CreateLineStatus(s.JobReceiveLineId, ref db, true);

                    try
                    {
                        JobReceiveDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobReceiveHeaderId, s.JobReceiveLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobReceiveDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobReceiveHeaderId, s.JobReceiveLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveHeaderId,
                        DocLineId = s.JobReceiveLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.JobReceiveHeaderId, JobWorkerId = svm.JobWorkerId });
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    StringBuilder logstring = new StringBuilder();
                    JobReceiveLine RecLine = _JobReceiveLineService.Find(svm.JobReceiveLineId);


                    JobReceiveLine ExRec = new JobReceiveLine();
                    ExRec = Mapper.Map<JobReceiveLine>(RecLine);


                    //JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(s.JobOrderLineId);
                    //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);

                    RecLine.ProductId = svm.ProductId;
                    RecLine.Dimension1Id = svm.Dimension1Id;
                    RecLine.Dimension2Id = svm.Dimension2Id;
                    RecLine.Dimension3Id = svm.Dimension3Id;
                    RecLine.Dimension4Id = svm.Dimension4Id;
                    RecLine.PenaltyAmt = svm.PenaltyAmt;
                    RecLine.PenaltyRate = svm.PenaltyRate;
                    RecLine.Remark = svm.Remark;
                    RecLine.LotNo = svm.LotNo;
                    RecLine.Qty = svm.ReceiveQty;
                    RecLine.LossQty = svm.LossQty;
                    RecLine.PassQty = svm.PassQty;
                    RecLine.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    RecLine.DealQty = svm.DealQty;
                    RecLine.DealUnitId = svm.DealUnitId;
                    RecLine.MachineId = svm.MachineId;
                    RecLine.Weight = svm.Weight;
                    RecLine.IncentiveAmt = svm.IncentiveAmt;
                    RecLine.IncentiveRate = svm.IncentiveRate;

                    decimal SettingsStockQty = (decimal)GetStockQtyFromSettings(RecLine, settings.StockQty, "Qty");

                    if (RecLine.StockId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockViewModel.StockId = RecLine.StockId ?? 0;
                        StockViewModel.DocHeaderId = RecLine.JobReceiveHeaderId;
                        StockViewModel.DocLineId = RecLine.JobReceiveLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = temp.ProcessId;
                        StockViewModel.PersonId = temp.JobWorkerId;
                        StockViewModel.ProductId = svm.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = temp.GodownId;
                        StockViewModel.GodownId = temp.GodownId;
                        StockViewModel.ProcessId = temp.ProcessId;
                        //StockViewModel.LotNo = JobOrderLine.LotNo;
                        StockViewModel.LotNo = svm.LotNo;
                        StockViewModel.CostCenterId = svm.CostCenterId;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = SettingsStockQty;
                        StockViewModel.Rate = svm.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = svm.Specification;
                        StockViewModel.Dimension1Id = svm.Dimension1Id;
                        StockViewModel.Dimension2Id = svm.Dimension2Id;
                        StockViewModel.Dimension3Id = svm.Dimension3Id;
                        StockViewModel.Dimension4Id = svm.Dimension4Id;
                        StockViewModel.Remark = svm.Remark;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.ProductUidId = svm.ProductUidId;
                        StockViewModel.CreatedBy = RecLine.CreatedBy;
                        StockViewModel.CreatedDate = RecLine.CreatedDate;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }
                    }




                    if (RecLine.StockProcessId != null)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                        StockProcessViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockProcessViewModel.StockProcessId = RecLine.StockProcessId ?? 0;
                        StockProcessViewModel.DocHeaderId = RecLine.JobReceiveHeaderId;
                        StockProcessViewModel.DocLineId = RecLine.JobReceiveLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.JobWorkerId;
                        StockProcessViewModel.ProductId = svm.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.GodownId = temp.GodownId;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = svm.LotNo;
                        StockProcessViewModel.CostCenterId = svm.CostCenterId;
                        StockProcessViewModel.Qty_Iss = SettingsStockQty + svm.LossQty;
                        StockProcessViewModel.Qty_Rec = 0;
                        StockProcessViewModel.Rate = svm.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = svm.Specification;
                        StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = svm.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = svm.Dimension4Id;
                        StockProcessViewModel.Remark = RecLine.Remark;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.ProductUidId = svm.ProductUidId;
                        StockProcessViewModel.CreatedBy = RecLine.CreatedBy;
                        StockProcessViewModel.CreatedDate = RecLine.CreatedDate;
                        StockProcessViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }
                    }


                    RecLine.ModifiedDate = DateTime.Now;
                    RecLine.ModifiedBy = User.Identity.Name;
                    RecLine.ObjectState = Model.ObjectState.Modified;

                    if (s.JobOrderLineId != null)
                    {
                        new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReceive((int)s.JobOrderLineId, s.JobReceiveLineId, temp.DocDate, RecLine.Qty + RecLine.LossQty, ref db);
                    }

                    //_JobReceiveLineService.Update(temp1);

                    db.JobReceiveLine.Add(RecLine);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = RecLine,
                    });


                    #region BomPost

                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data

                    if (!string.IsNullOrEmpty(svm.JobReceiveSettings.SqlProcConsumption))
                    {
                        //IEnumerable<JobReceiveBom> OldBomList = new JobReceiveBomService(_unitOfWork).GetBomForLine(temp1.JobReceiveLineId);

                        var OldBomList = (from p in db.JobReceiveBom
                                          where p.JobReceiveLineId == RecLine.JobReceiveLineId
                                          select p).ToList();

                        var StockProcessIds = OldBomList.Select(m => m.StockProcessId).ToArray();

                        var StockProcessRecords = (from p in db.StockProcess
                                                   where StockProcessIds.Contains(p.StockProcessId)
                                                   select p).ToList();

                        foreach (var item in OldBomList)
                        {
                            if (item.StockProcessId != null)
                            {
                                var StockProcRec = StockProcessRecords.Where(m => m.StockProcessId == item.StockProcessId).FirstOrDefault();
                                StockProcRec.ObjectState = Model.ObjectState.Deleted;
                                db.StockProcess.Remove(StockProcRec);
                            }
                            item.ObjectState = Model.ObjectState.Deleted;
                            db.JobReceiveBom.Remove(item);
                            //new JobReceiveBomService(_unitOfWork).Delete(item.JobReceiveBomId);
                        }


                        var BomPostList = _JobReceiveLineService.GetBomPostingDataForWeaving(svm.ProductId, svm.Dimension1Id, svm.Dimension2Id, svm.Dimension3Id, svm.Dimension4Id, temp.ProcessId, RecLine.Qty + RecLine.LossQty, temp.DocTypeId, svm.JobReceiveSettings.SqlProcConsumption, RecLine.JobOrderLineId, RecLine.Weight).ToList();

                        foreach (var item in BomPostList)
                        {
                            JobReceiveBom BomPost = new JobReceiveBom();
                            BomPost.JobReceiveHeaderId = temp.JobReceiveHeaderId;
                            BomPost.JobReceiveLineId = s.JobReceiveLineId;
                            BomPost.CreatedBy = User.Identity.Name;
                            BomPost.CreatedDate = DateTime.Now;
                            BomPost.ModifiedBy = User.Identity.Name;
                            BomPost.ModifiedDate = DateTime.Now;
                            BomPost.ProductId = item.ProductId;
                            BomPost.Qty = Convert.ToDecimal(item.Qty);



                            StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
                            if (temp.StockHeaderId != null && temp.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessBomViewModel.StockHeaderId = (int)temp.StockHeaderId;
                            }
                            else if (svm.JobReceiveSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                            {
                                StockProcessBomViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessBomViewModel.StockHeaderId = 0;
                            }
                            StockProcessBomViewModel.DocHeaderId = temp.JobReceiveHeaderId;
                            StockProcessBomViewModel.DocLineId = s.JobReceiveLineId;
                            StockProcessBomViewModel.DocTypeId = temp.DocTypeId;
                            StockProcessBomViewModel.StockHeaderDocDate = temp.DocDate;
                            StockProcessBomViewModel.StockProcessDocDate = temp.DocDate;
                            StockProcessBomViewModel.DocNo = temp.DocNo;
                            StockProcessBomViewModel.DivisionId = temp.DivisionId;
                            StockProcessBomViewModel.SiteId = temp.SiteId;
                            StockProcessBomViewModel.CurrencyId = null;
                            StockProcessBomViewModel.HeaderProcessId = null;
                            StockProcessBomViewModel.PersonId = temp.JobWorkerId;
                            StockProcessBomViewModel.ProductId = item.ProductId;
                            StockProcessBomViewModel.HeaderFromGodownId = null;
                            StockProcessBomViewModel.HeaderGodownId = null;
                            StockProcessBomViewModel.GodownId = temp.GodownId;
                            StockProcessBomViewModel.ProcessId = temp.ProcessId;
                            StockProcessBomViewModel.LotNo = s.LotNo;
                            StockProcessBomViewModel.CostCenterId = svm.CostCenterId;
                            StockProcessBomViewModel.Qty_Iss = item.Qty;
                            StockProcessBomViewModel.Qty_Rec = 0;
                            StockProcessBomViewModel.Rate = 0;
                            StockProcessBomViewModel.ExpiryDate = null;
                            StockProcessBomViewModel.Specification = null;
                            StockProcessBomViewModel.Dimension1Id = null;
                            StockProcessBomViewModel.Dimension2Id = null;
                            StockProcessBomViewModel.Dimension3Id = null;
                            StockProcessBomViewModel.Dimension4Id = null;
                            StockProcessBomViewModel.Remark = null;
                            StockProcessBomViewModel.Status = temp.Status;
                            StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                            StockProcessBomViewModel.CreatedDate = DateTime.Now;
                            StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                            StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                ModelState.AddModelError("", StockProcessPostingError);
                                return PartialView("_Create", svm);
                            }

                            BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;

                            BomPost.ObjectState = Model.ObjectState.Added;
                            //new JobReceiveBomService(_unitOfWork).Create(BomPost);
                            db.JobReceiveBom.Add(BomPost);


                            if (svm.JobReceiveSettings.isPostedInStock == false && svm.JobReceiveSettings.isPostedInStockProcess == false)
                            {
                                if (temp.StockHeaderId == null)
                                {
                                    temp.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
                                }
                            }
                        }
                    }

                    #endregion


                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }
                    //new JobReceiveHeaderService(_unitOfWork).Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveHeader.Add(temp);


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReceiveDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobReceiveHeaderId, RecLine.JobReceiveLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        EventException = true;
                    }


                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobReceiveDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobReceiveHeaderId, RecLine.JobReceiveLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveHeaderId,
                        DocLineId = RecLine.JobReceiveLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        xEModifications = Modifications,
                        DocStatus = temp.Status,
                    }));


                    return Json(new { success = true });

                }
            }
            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }



        [HttpGet]
        public ActionResult _ModifyLine(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterSubmit(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }



        [HttpGet]
        private ActionResult _Modify(int id)
        {
            JobReceiveLineViewModel temp = _JobReceiveLineService.GetJobReceiveLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Edit";

            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(temp.JobReceiveHeaderId);

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            PrepareViewBag(temp);

            return PartialView("_Create", temp);
        }

        [HttpGet]
        public ActionResult _DeleteLine(int id)
        {
            return _Delete(id);
        }
        [HttpGet]
        public ActionResult _DeleteLine_AfterSubmit(int id)
        {
            return _Delete(id);
        }

        [HttpGet]
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            JobReceiveLineViewModel temp = _JobReceiveLineService.GetJobReceiveLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Delete";

            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(temp.JobReceiveHeaderId);

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            PrepareViewBag(temp);

            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobReceiveLineViewModel vm)
        {
            bool BeforeSave = true;
            bool IsProductUidGeneratedFromReceive = false;
            int MainSiteId = (from S in db.Site where S.SiteCode == "MAIN" select S).FirstOrDefault().SiteId;

            try
            {
                BeforeSave = JobReceiveDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobReceiveHeaderId, vm.JobReceiveLineId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave && !EventException)
            {
                int? StockId = 0;
                int? StockProcessId = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                JobReceiveLine JobReceiveLine = (from p in db.JobReceiveLine
                                                 where p.JobReceiveLineId == vm.JobReceiveLineId
                                                 select p).FirstOrDefault();

                JobReceiveHeader header = new JobReceiveHeaderService(_unitOfWork).Find(JobReceiveLine.JobReceiveHeaderId);

                JobReceiveLineStatus LineStatus = (from p in db.JobReceiveLineStatus
                                                   where p.JobReceiveLineId == JobReceiveLine.JobReceiveLineId
                                                   select p).FirstOrDefault();

                StockId = JobReceiveLine.StockId;
                StockProcessId = JobReceiveLine.StockProcessId;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveLine>(JobReceiveLine),
                });

                if (JobReceiveLine.JobOrderLineId != null)
                {
                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReceive((int)JobReceiveLine.JobOrderLineId, JobReceiveLine.JobReceiveLineId, header.DocDate, 0, ref db);
                    LineStatus.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveLineStatus.Remove(LineStatus);
                }



                ProductUid ProductUid = (from p in db.ProductUid
                                         where p.ProductUIDId == vm.ProductUidId
                                         select p).FirstOrDefault();

                if (vm.ProductUidId != null && vm.ProductUidId != 0)
                {
                    if (!(JobReceiveLine.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && JobReceiveLine.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || header.SiteId == MainSiteId)
                    {


                        if ((header.DocNo != ProductUid.LastTransactionDocNo || header.DocTypeId != ProductUid.LastTransactionDocTypeId))
                        {
                            ModelState.AddModelError("", "Bar Code Can't be deleted because this is already transfered to another process.");
                            PrepareViewBag(vm);
                            return PartialView("_Create", vm);
                        }

                        if (JobReceiveLine.ProductUidHeaderId == null || JobReceiveLine.ProductUidHeaderId == 0)
                        {
                            ProductUid.LastTransactionDocDate = JobReceiveLine.ProductUidLastTransactionDocDate;
                            ProductUid.LastTransactionDocId = JobReceiveLine.ProductUidLastTransactionDocId;
                            ProductUid.LastTransactionDocNo = JobReceiveLine.ProductUidLastTransactionDocNo;
                            ProductUid.LastTransactionDocTypeId = JobReceiveLine.ProductUidLastTransactionDocTypeId;
                            ProductUid.LastTransactionPersonId = JobReceiveLine.ProductUidLastTransactionPersonId;
                            ProductUid.CurrenctGodownId = JobReceiveLine.ProductUidCurrentGodownId;
                            ProductUid.CurrenctProcessId = JobReceiveLine.ProductUidCurrentProcessId;
                            ProductUid.Status = JobReceiveLine.ProductUidStatus;

                            ProductUid.ObjectState = Model.ObjectState.Modified;
                            db.ProductUid.Add(ProductUid);

                            new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(vm.JobReceiveHeaderId, header.DocTypeId, header.SiteId, header.DivisionId, ref db);
                        }
                    }
                    else
                    {
                        var MainJobRec = (from p in db.JobReceiveLine
                                          join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                                          join d in db.DocumentType on t.DocTypeId equals d.DocumentTypeId
                                          where p.ProductUidId == vm.ProductUidId && t.SiteId != header.SiteId && d.DocumentTypeName == TransactionDoctypeConstants.WeavingBazar
                                          && p.LockReason != null
                                          select p).ToList().LastOrDefault();

                        if (MainJobRec != null)
                        {
                            MainJobRec.LockReason = null;
                            MainJobRec.ObjectState = Model.ObjectState.Modified;
                            db.JobReceiveLine.Add(MainJobRec);
                        }
                    }
                }


                if (JobReceiveLine.ProductUidHeaderId != null && JobReceiveLine.ProductUidHeaderId != 0)
                {
                    IsProductUidGeneratedFromReceive = true;
                }

                JobReceiveLine.ObjectState = Model.ObjectState.Deleted;
                db.JobReceiveLine.Remove(JobReceiveLine);




                if (StockId != null)
                {
                    new StockService(_unitOfWork).DeleteStockDB((int)StockId, ref db, true);
                }

                if (StockProcessId != null)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB((int)StockProcessId, ref db, true);
                }


                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveHeader.Add(header);
                }

                var Boms = (from p in db.JobReceiveBom
                            where p.JobReceiveLineId == vm.JobReceiveLineId
                            select p).ToList();

                var StockProcessIds = Boms.Select(m => m.StockProcessId).ToArray();

                var StockProcessRecords = (from p in db.StockProcess
                                           where StockProcessIds.Contains(p.StockProcessId)
                                           select p).ToList();

                foreach (var item in Boms)
                {
                    if (item.StockProcessId != null)
                    {
                        var StockProcessRecord = StockProcessRecords.Where(m => m.StockProcessId == item.StockProcessId).FirstOrDefault();
                        StockProcessRecord.ObjectState = Model.ObjectState.Deleted;
                        db.StockProcess.Remove(StockProcessRecord);
                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveBom.Remove(item);
                }

                if (IsProductUidGeneratedFromReceive == true)
                {
                    ProductUid.ObjectState = Model.ObjectState.Deleted;
                    db.ProductUid.Remove(ProductUid);
                }



                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobReceiveDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobReceiveLine.JobReceiveHeaderId, JobReceiveLine.JobReceiveLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }


                try
                {
                    if (EventException)
                        throw new Exception();
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    PrepareViewBag(vm);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobReceiveDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobReceiveLine.JobReceiveHeaderId, JobReceiveLine.JobReceiveLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.JobReceiveHeaderId,
                    DocLineId = JobReceiveLine.JobReceiveLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
        }

        public JsonResult GetPendingOrders(int HeaderId, string term, int Limit)
        {

            var Header = db.JobReceiveHeader.Find(HeaderId);

            var DocType = db.DocumentType.Where(m => m.DocumentTypeName == TransactionDoctypeConstants.TraceMapReceive).FirstOrDefault();

            if (DocType != null)
            {
                if (Header.DocTypeId == DocType.DocumentTypeId)
                {
                    return Json(new JobOrderHeaderService(_unitOfWork).GetPendingJobOrdersWithPatternMatchTraceMapReceive(Header.JobWorkerId, Header.ProcessId, term, Limit).ToList());
                }
                else
                {
                    return Json(new JobOrderHeaderService(_unitOfWork).GetPendingJobOrdersWithPatternMatch(Header.JobWorkerId, term, Limit).ToList());
                }
            }
            else
            {
                return Json(new JobOrderHeaderService(_unitOfWork).GetPendingJobOrdersWithPatternMatch(Header.JobWorkerId, term, Limit).ToList());
            }
            
        }

        public JsonResult GetOrderDetail(int OrderId, int ReceiveId)
        {
            return Json(new JobOrderLineService(_unitOfWork).GetLineDetailForReceive(OrderId, ReceiveId));
        }

        public JsonResult GetProductDetailJson(int ProductId, int? HeaderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            List<Product> ProductJson = new List<Product>();

            decimal BalanceQty = 0;

            if (HeaderId.HasValue)
            {
                BalanceQty = _JobReceiveLineService.GetConsumptionBalanceQty(HeaderId.Value, ProductId);
            }

            return Json(new
            {
                ProductId = product.ProductId,
                StandardCost = product.StandardCost,
                UnitId = product.UnitId,
                BalanceQty = BalanceQty
            });
        }

        public ActionResult GetPendingJobOrderProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            return new JsonpResult { Data = _JobReceiveLineService.GetPendingProductsForJobReceive(searchTerm, pageSize, pageNum, filter), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult GetPendingJobOrderProductGroups(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            return new JsonpResult { Data = _JobReceiveLineService.GetPendingProductGroupsForJobReceive(searchTerm, pageSize, pageNum, filter), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetPendingJobOrders(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            return new JsonpResult { Data = _JobReceiveLineService.GetPendingJobOrders(searchTerm, pageSize, pageNum, filter), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetPendingCostCenterHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var temp = _JobReceiveLineService.GetPendingCostCenters(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _JobReceiveLineService.GetPendingCostCenters(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetBarCodes(int Id)
        {
            return Json(_JobReceiveLineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBarCodesForProductUid(int[] Id)
        {
            return Json(_JobReceiveLineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }


        private object GetStockQtyFromSettings(object Src, string StockQtyType, string DefaultQtyType)
        {
            var Type = Src.GetType();
            if (!string.IsNullOrEmpty(StockQtyType) && Type.GetProperty(StockQtyType) != null)
            {
                var Prop = Type.GetProperty(StockQtyType);
                return Prop.GetValue(Src);
            }
            else
            {
                var Default = Type.GetProperty(DefaultQtyType);
                return Default.GetValue(Src);
            }
        }

        public JsonResult GetProductsForConsumption(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {

            var Query = _JobReceiveLineService.GetConsumptionProducts(searchTerm, filter);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            return new JsonResult
            {
                Data = temp,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        public ActionResult GetProductUidHelpList(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceHeaderId
        {
            List<ComboBoxResult> ProductUidJson = _JobReceiveLineService.FGetProductUidHelpList(filter, searchTerm).ToList();

            var count = ProductUidJson.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = ProductUidJson;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductUid(string Ids)
        {
            ComboBoxResult ProductUidJson = new ComboBoxResult();

            var ProductUid = from L in db.ProductUid
                             where L.ProductUidName == Ids
                             select new
                             {
                                 id = L.ProductUidName,
                                 text = L.ProductUidName
                             };

            ProductUidJson.id = ProductUid.FirstOrDefault().id;
            ProductUidJson.text = ProductUid.FirstOrDefault().text;

            return Json(ProductUidJson);
        }

        public ActionResult GetJobOrderForProduct(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceReturnHeaderId
        {
            var Query = _JobReceiveLineService.GetJobOrderHelpListForProduct(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            TempData["CSEXC"] = null;

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
