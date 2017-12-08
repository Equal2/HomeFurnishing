using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using System.Web.UI.WebControls;
using AutoMapper;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using Reports.Controllers;



namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class WeavingOrderWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobOrderHeaderService _JobOrderHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public WeavingOrderWizardController(IJobOrderHeaderService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderHeaderService = PurchaseOrderHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult CreateWeavingOrder(int id)//DocTypeId
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            ViewBag.id = id;

            int DocTypeId = id;

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobOrderSettings", new { id = DocTypeId }).Warning("Please create settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            return View("CreateWeavingOrder");
        }

        public JsonResult PendingProdOrders(int id)//DocTypeId
        {

            var temp = _JobOrderHeaderService.GetProdOrdersForWeavingWizard(id).ToList();

            return Json(new { data = temp }, JsonRequestBehavior.AllowGet);
        }


        //public ActionResult SelectedProdOrderList(Dictionary<int,decimal> Selected)
        //{
        //    var temp = 1;

        //    SqlParameter SqlParameterProdOrderLineId = new SqlParameter("@ProdOrderLineIdList", ProdOrderLineId);

        //    IEnumerable<DyeingOrderWizardViewModel> FgetProdOrders = db.Database.SqlQuery<DyeingOrderWizardViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_DyeingOrderWizard_Step2 @ProdOrderLineIdList", SqlParameterProdOrderLineId).ToList();
        //    return Json(new { Success = true, Data = FgetProdOrders }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult ConfirmProdOrderList(List<WeavingOrderWizardViewModel> Selected, int id)
        {
            bool BarCodesBased = Selected.Any(m => m.RefDocLineId.HasValue);

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(id, DivisionId, SiteId);

            if (settings != null)
            {
                BarCodesBased = settings.isVisibleProductUID ?? false;
            }

            System.Web.HttpContext.Current.Session["BarCodesWeavingWizardProdOrder"] = Selected;
            if (!BarCodesBased)
                return Json(new { Success = "URL", Data = "/Rug/WeavingOrderWizard/Create/" + id.ToString() }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { Success = "URL", Data = "/Rug/WeavingOrderWizard/FirstBarCode/" + id.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [Serializable]
        public class SelectedProdOrders
        {
            public int id { get; set; }
            public decimal Qty { get; set; }
            public int? RefDocTypeId { get; set; }
            public int? RefDocLineId { get; set; }
        }

        private void PrepareViewBag()
        {
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }


        public ActionResult FirstBarCode(int id)//DocumentTypeId
        {
            List<WeavingOrderWizardViewModel> ProdOrders = (List<WeavingOrderWizardViewModel>)System.Web.HttpContext.Current.Session["BarCodesWeavingWizardProdOrder"];

            WeavingOrderWizardMasterDetailModel vm = new WeavingOrderWizardMasterDetailModel();

            foreach (var item in ProdOrders)
            {
                if (item.RefDocLineId.HasValue)
                {
                    var BarCodes = new JobOrderLineService(_unitOfWork).GetBarCodesForWeavingWizard(item.RefDocLineId.Value, null).FirstOrDefault();
                    if (BarCodes != null)
                        item.FirstBarCode = Convert.ToInt32(BarCodes.id);
                    item.DocTypeId = id;
                }
            }

            vm.WeavingOrderWizardViewModel = ProdOrders;

            return View("CreateFirstBarCode", vm);

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult FirstBarCodePost(WeavingOrderWizardMasterDetailModel vm)
        {
            WeavingOrderWizardMasterDetailModel MasterDetailModel = new WeavingOrderWizardMasterDetailModel();
            List<WeavingOrderWizardViewModel> List = new List<WeavingOrderWizardViewModel>();

            foreach (var item in vm.WeavingOrderWizardViewModel)
            {

                WeavingOrderWizardViewModel Wizard = new WeavingOrderWizardViewModel();
                Wizard.Size = item.Size;
                Wizard.DesignName = item.DesignName;
                //Wizard.Qty = item.Qty;
                Wizard.BalanceQty = Wizard.BalanceQty;
                Wizard.Incentive = item.Incentive;
                Wizard.RefDocLineId = item.RefDocLineId;
                Wizard.ProdOrderLineId = item.ProdOrderLineId;

                if (item.RefDocLineId.HasValue)
                {

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = new JobOrderLineService(_unitOfWork).GetBarCodesForWeavingWizard(item.RefDocLineId.Value, null);

                        var temp = BarCodes.SkipWhile(m => m.text != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.id));
                        Wizard.Qty = string.IsNullOrEmpty(Ids) ? 0 : Ids.Split(',').Count();
                        Wizard.SelectedBarCodes = Ids;
                    }
                    else
                    {
                        var BarCodes = new JobOrderLineService(_unitOfWork).GetBarCodesForWeavingWizard(item.RefDocLineId.Value, null).ToList();
                        var temp = BarCodes.Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.id));
                        Wizard.Qty = string.IsNullOrEmpty(Ids) ? 0 : Ids.Split(',').Count();
                        Wizard.SelectedBarCodes = Ids;
                    }


                }

                List.Add(Wizard);

            }

            MasterDetailModel.WeavingOrderWizardViewModel = List;

            return View("BarCodeSequence", MasterDetailModel);

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BarCodeSequencePost(WeavingOrderWizardMasterDetailModel vm)
        {

            System.Web.HttpContext.Current.Session["BarCodesWeavingWizardProdOrder"] = vm.WeavingOrderWizardViewModel;

            return RedirectToAction("Create", new { id = vm.WeavingOrderWizardViewModel.FirstOrDefault().DocTypeId });

        }


        // GET: /JobOrderHeader/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            JobOrderHeaderViewModel p = new JobOrderHeaderViewModel();

            p.DocDate = DateTime.Now;
            p.CreatedDate = DateTime.Now;

            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            int DocTypeId = id;

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobOrderSettings", new { id = DocTypeId }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            if (System.Web.HttpContext.Current.Session["BarCodesWeavingWizardProdOrder"] == null)
                return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobOrderHeader/Index/" + DocTypeId);

            var ProdOrderLin = ((List<WeavingOrderWizardViewModel>)System.Web.HttpContext.Current.Session["BarCodesWeavingWizardProdOrder"]).FirstOrDefault();

            int ProdOrderLineId = ProdOrderLin.ProdOrderLineId;

            p.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            var DesignPatternId = (from pol in db.ProdOrderLine
                                   where pol.ProdOrderLineId == ProdOrderLineId
                                   join t in db.FinishedProduct on pol.ProductId equals t.ProductId
                                   select t.ProductDesignPatternId).FirstOrDefault();

            List<PerkViewModel> Perks = new List<PerkViewModel>();
            //Perks
            if (p.JobOrderSettings.Perks != null)
                foreach (var item in p.JobOrderSettings.Perks.Split(',').ToList())
                {
                    PerkViewModel temp = Mapper.Map<Perk, PerkViewModel>(new PerkService(_unitOfWork).Find(Convert.ToInt32(item)));
                    var DocTypePerk = (from p2 in db.PerkDocumentType
                                       where p2.DocTypeId == DocTypeId && p2.PerkId == temp.PerkId && p2.SiteId == p.SiteId && p2.DivisionId == p.DivisionId
                                       select p2).FirstOrDefault();
                    if (DocTypePerk != null)
                    {
                        temp.Base = DocTypePerk.Base;
                        temp.Worth = DocTypePerk.Worth;
                        temp.CostConversionMultiplier = DocTypePerk.CostConversionMultiplier;
                        temp.IsEditableRate = DocTypePerk.IsEditableRate;
                    }
                    else
                    {
                        temp.Base = 0;
                        temp.Worth = 0;
                        temp.CostConversionMultiplier = 0;
                        temp.IsEditableRate = true;
                    }

                    Perks.Add(temp);
                }

            p.PerkViewModel = Perks;
            if (settings.DueDays.HasValue)
            {
                p.DueDate = _JobOrderHeaderService.AddDueDate(DateTime.Now, settings.DueDays.Value);
            }
            else
                p.DueDate = DateTime.Now;

            p.OrderById = new EmployeeService(_unitOfWork).GetEmloyeeForUser(User.Identity.GetUserId());
            p.UnitConversionForId = settings.UnitConversionForId;
            p.ProcessId = settings.ProcessId;
            p.DealUnitId = settings.DealUnitId;
            PrepareViewBag();
            p.DocTypeId = DocTypeId;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobOrderHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);

            if (p.JobOrderSettings.isVisibleCostCenter)
            {
                p.CostCenterName = new JobOrderHeaderService(_unitOfWork).FGetJobOrderCostCenter(p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            }



            //RatesFetching from RateList Master
            var Lines = (List<WeavingOrderWizardViewModel>)System.Web.HttpContext.Current.Session["BarCodesWeavingWizardProdOrder"];

            var DesignName = Lines.FirstOrDefault().DesignName;
            var ProductGroupId = new ProductGroupService(_unitOfWork).Find(DesignName).ProductGroupId;

            var RateListLine = new RateListLineService(_unitOfWork).GetRateListForDesign(ProductGroupId, settings.ProcessId);

            if (RateListLine != null)
            {
                p.Rate = RateListLine.Rate;
                p.Loss = RateListLine.Loss;
                p.UnCountedQty = RateListLine.UnCountedQty;
            }

            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobOrderHeaderViewModel svm)
        {
            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            bool Continue = true;

            JobOrderHeader s = Mapper.Map<JobOrderHeaderViewModel, JobOrderHeader>(svm);
            List<WeavingOrderWizardViewModel> ProdOrdersAndQtys = (List<WeavingOrderWizardViewModel>)System.Web.HttpContext.Current.Session["BarCodesWeavingWizardProdOrder"];
            if (svm.JobOrderSettings != null)
            {
                if (svm.JobOrderSettings.isMandatoryCostCenter == true && (string.IsNullOrEmpty(svm.CostCenterName)))
                {
                    ModelState.AddModelError("CostCenterName", "The CostCenter field is required");
                }
                if (svm.JobOrderSettings.isMandatoryMachine == true && (svm.MachineId <= 0 || svm.MachineId == null))
                {
                    ModelState.AddModelError("MachineId", "The Machine field is required");
                }
                if (svm.JobOrderSettings.isVisibleGodown && svm.JobOrderSettings.isMandatoryGodown && !svm.GodownId.HasValue)
                {
                    ModelState.AddModelError("GodownId", "The Godown field is required");
                }
                if (svm.JobOrderSettings.MaxDays.HasValue && (svm.DueDate - svm.DocDate).Days > svm.JobOrderSettings.MaxDays.Value)
                {
                    ModelState.AddModelError("DueDate", "DueDate is exceeding MaxDueDays.");
                }
            }

            if (svm.DueDate < svm.DocDate )
            {
                ModelState.AddModelError("DueDate", "DueDate should not be less than " + svm.DocDate.ToString());
            }

            if (svm.Rate <= 0 && svm.JobOrderSettings.isMandatoryRate)
                ModelState.AddModelError("Rate", "Rate field is required");

            if (ProdOrdersAndQtys.Count() <= 0)
                ModelState.AddModelError("", "No Records Selected");

            List<JobOrderLine> BarCodesToUpdate = new List<JobOrderLine>();

            bool CostCenterGenerated = false;

            #region DocTypeTimeLineValidation

            try
            {

                if (svm.JobOrderHeaderId <= 0)
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXC"] += ExceptionMsg;

            #endregion

            if (ModelState.IsValid && (TimePlanValidation || Continue))
            {

                if (svm.JobOrderHeaderId <= 0)
                {


                    if (ProdOrdersAndQtys.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(svm.CostCenterName))
                        {

                            var CostCenter = new CostCenterService(_unitOfWork).Find(svm.CostCenterName, svm.DivisionId, svm.SiteId, svm.DocTypeId);
                            if (CostCenter != null)
                            {
                                s.CostCenterId = CostCenter.CostCenterId;
                                if (s.CostCenterId.HasValue)
                                {
                                    var costcen = new CostCenterService(_unitOfWork).Find(s.CostCenterId.Value);
                                    costcen.ProcessId = svm.ProcessId;
                                    new CostCenterService(_unitOfWork).Update(costcen);
                                }
                            }
                            else
                            {
                                CostCenter Cs = new CostCenter();
                                Cs.CostCenterName = svm.CostCenterName;
                                Cs.DivisionId = svm.DivisionId;
                                Cs.SiteId = svm.SiteId;
                                Cs.DocTypeId = svm.DocTypeId;
                                Cs.ProcessId = svm.ProcessId;
                                Cs.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(svm.JobWorkerId).LedgerAccountId;
                                Cs.CreatedBy = User.Identity.Name;
                                Cs.ModifiedBy = User.Identity.Name;
                                Cs.CreatedDate = DateTime.Now;
                                Cs.ModifiedDate = DateTime.Now;
                                Cs.IsActive = true;
                                Cs.ReferenceDocNo = svm.DocNo;
                                Cs.ReferenceDocTypeId = svm.DocTypeId;
                                Cs.StartDate = svm.DocDate;
                                Cs.ParentCostCenterId = new ProcessService(_unitOfWork).Find(svm.ProcessId).CostCenterId;
                                Cs.ObjectState = Model.ObjectState.Added;
                                new CostCenterService(_unitOfWork).Create(Cs);
                                s.CostCenterId = Cs.CostCenterId;

                                new CostCenterStatusService(_unitOfWork).CreateLineStatus(Cs.CostCenterId, ref db, false);

                                CostCenterGenerated = true;
                            }

                        }


                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now;
                        s.ActualDueDate = s.DueDate;
                        s.ActualDocDate = s.DocDate;
                        s.CreatedBy = User.Identity.Name;
                        s.ModifiedBy = User.Identity.Name;
                        s.Status = (int)StatusConstants.Drafted;
                        _JobOrderHeaderService.Create(s);

                        new JobOrderHeaderStatusService(_unitOfWork).CreateHeaderStatus(s.JobOrderHeaderId, ref db, false);

                        if (svm.PerkViewModel != null)
                        {
                            foreach (PerkViewModel item in svm.PerkViewModel)
                            {
                                JobOrderPerk perk = Mapper.Map<PerkViewModel, JobOrderPerk>(item);
                                perk.CreatedBy = User.Identity.Name;
                                perk.CreatedDate = DateTime.Now;
                                perk.ModifiedBy = User.Identity.Name;
                                perk.ModifiedDate = DateTime.Now;
                                perk.JobOrderHeaderId = s.JobOrderHeaderId;
                                new JobOrderPerkService(_unitOfWork).Create(perk);
                            }
                        }

                        //string DealUnit = UnitConstants.SqYard;
                        string DealUnit = svm.DealUnitId;


                        int Cnt = 0;
                        int Sr = 0;

                        List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
                        List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
                        int pk = 0;
                        bool HeaderChargeEdit = false;



                        JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

                        int? MaxLineId = new JobOrderLineChargeService(_unitOfWork).GetMaxProductCharge(s.JobOrderHeaderId, "Web.JobOrderLines", "JobOrderHeaderId", "JobOrderLineId");

                        int PersonCount = 0;
                        //if (!Settings.CalculationId.HasValue)
                        //{
                        //    throw new Exception("Calculation not configured in Job order settings");
                        //}

                        int CalculationId = Settings.CalculationId ?? 0;

                        List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

                        var ProdOrderLineIds = ProdOrdersAndQtys.Select(m => m.ProdOrderLineId).ToArray();

                        var BalQtyandUnits = (from p in db.ViewProdOrderBalance
                                              join t in db.Product on p.ProductId equals t.ProductId
                                              where ProdOrderLineIds.Contains(p.ProdOrderLineId)
                                              select new
                                              {
                                                  BalQty = p.BalanceQty,
                                                  ProdorderLineId = p.ProdOrderLineId,
                                                  UnitId = t.UnitId,
                                              }).ToList();

                        if (ModelState.IsValid)
                        {

                            decimal OrderQty = 0;
                            decimal OrderDealQty = 0;
                            decimal BomQty = 0;

                            foreach (var SelectedProdOrd in ProdOrdersAndQtys)
                            {
                                //if (SelectedProdOrd.ProdOrderLineId > 0 && !SelectedProdOrd.RefDocLineId.HasValue)
                                if (SelectedProdOrd.ProdOrderLineId > 0 && string.IsNullOrEmpty(SelectedProdOrd.SelectedBarCodes))
                                {
                                    var ProdOrderLine = new ProdOrderLineService(_unitOfWork).Find((SelectedProdOrd.ProdOrderLineId));
                                    var Product = new ProductService(_unitOfWork).Find(ProdOrderLine.ProductId);

                                    //decimal balQty = (from p in db.ViewProdOrderBalance
                                    //                  where p.ProdOrderLineId == SelectedProdOrd.ProdOrderLineId
                                    //                  select p.BalanceQty).FirstOrDefault();

                                    var bal = BalQtyandUnits.Where(m => m.ProdorderLineId == SelectedProdOrd.ProdOrderLineId).FirstOrDefault();

                                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                                    if (SelectedProdOrd.Qty <= bal.BalQty)
                                    {
                                        JobOrderLine line = new JobOrderLine();

                                        if (Settings.isPostedInStock ?? false)
                                        {
                                            StockViewModel StockViewModel = new StockViewModel();

                                            if (Cnt == 0)
                                            {
                                                StockViewModel.StockHeaderId = s.StockHeaderId ?? 0;
                                            }
                                            else
                                            {
                                                if (s.StockHeaderId != null && s.StockHeaderId != 0)
                                                {
                                                    StockViewModel.StockHeaderId = (int)s.StockHeaderId;
                                                }
                                                else
                                                {
                                                    StockViewModel.StockHeaderId = -1;
                                                }
                                            }

                                            StockViewModel.StockId = -Cnt;
                                            StockViewModel.DocHeaderId = s.JobOrderHeaderId;
                                            StockViewModel.DocLineId = line.JobOrderLineId;
                                            StockViewModel.DocTypeId = s.DocTypeId;
                                            StockViewModel.StockHeaderDocDate = s.DocDate;
                                            StockViewModel.StockDocDate = s.DocDate;
                                            StockViewModel.DocNo = s.DocNo;
                                            StockViewModel.DivisionId = s.DivisionId;
                                            StockViewModel.SiteId = s.SiteId;
                                            StockViewModel.CurrencyId = null;
                                            StockViewModel.PersonId = s.JobWorkerId;
                                            StockViewModel.ProductId = ProdOrderLine.ProductId;
                                            StockViewModel.HeaderFromGodownId = null;
                                            StockViewModel.HeaderGodownId = s.GodownId;
                                            StockViewModel.HeaderProcessId = s.ProcessId;
                                            StockViewModel.GodownId = (int)s.GodownId;
                                            StockViewModel.Remark = s.Remark;
                                            StockViewModel.Status = s.Status;
                                            StockViewModel.ProcessId = s.ProcessId;
                                            StockViewModel.LotNo = null;
                                            StockViewModel.CostCenterId = s.CostCenterId;
                                            StockViewModel.Qty_Iss = ProdOrderLine.Qty;
                                            StockViewModel.Qty_Rec = 0;
                                            StockViewModel.Rate = SelectedProdOrd.Rate > 0 ? SelectedProdOrd.Rate : svm.Rate;
                                            StockViewModel.ExpiryDate = null;
                                            StockViewModel.Specification = ProdOrderLine.Specification;
                                            StockViewModel.Dimension1Id = ProdOrderLine.Dimension1Id;
                                            StockViewModel.Dimension2Id = ProdOrderLine.Dimension2Id;
                                            StockViewModel.CreatedBy = User.Identity.Name;
                                            StockViewModel.CreatedDate = DateTime.Now;
                                            StockViewModel.ModifiedBy = User.Identity.Name;
                                            StockViewModel.ModifiedDate = DateTime.Now;

                                            string StockPostingError = "";
                                            StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                                            if (StockPostingError != "")
                                            {
                                                string message = StockPostingError;
                                                ModelState.AddModelError("", message);
                                                return View("Create", svm);
                                            }

                                            if (Cnt == 0)
                                            {
                                                s.StockHeaderId = StockViewModel.StockHeaderId;
                                            }
                                            line.StockId = StockViewModel.StockId;
                                        }



                                        if (Settings.isPostedInStockProcess ?? false)
                                        {
                                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                                            if (s.StockHeaderId != null && s.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                            {
                                                StockProcessViewModel.StockHeaderId = (int)s.StockHeaderId;
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
                                            StockProcessViewModel.DocHeaderId = s.JobOrderHeaderId;
                                            StockProcessViewModel.DocLineId = line.JobOrderLineId;
                                            StockProcessViewModel.DocTypeId = s.DocTypeId;
                                            StockProcessViewModel.StockHeaderDocDate = s.DocDate;
                                            StockProcessViewModel.StockProcessDocDate = s.DocDate;
                                            StockProcessViewModel.DocNo = s.DocNo;
                                            StockProcessViewModel.DivisionId = s.DivisionId;
                                            StockProcessViewModel.SiteId = s.SiteId;
                                            StockProcessViewModel.CurrencyId = null;
                                            StockProcessViewModel.PersonId = s.JobWorkerId;
                                            StockProcessViewModel.ProductId = ProdOrderLine.ProductId;
                                            StockProcessViewModel.HeaderFromGodownId = null;
                                            StockProcessViewModel.HeaderGodownId = s.GodownId;
                                            StockProcessViewModel.HeaderProcessId = s.ProcessId;
                                            StockProcessViewModel.GodownId = s.GodownId;
                                            StockProcessViewModel.Remark = s.Remark;
                                            StockProcessViewModel.Status = s.Status;
                                            StockProcessViewModel.ProcessId = s.ProcessId;
                                            StockProcessViewModel.LotNo = null;
                                            StockProcessViewModel.CostCenterId = s.CostCenterId;
                                            StockProcessViewModel.Qty_Iss = 0;
                                            StockProcessViewModel.Qty_Rec = ProdOrderLine.Qty;
                                            StockProcessViewModel.Rate = SelectedProdOrd.Rate > 0 ? SelectedProdOrd.Rate : svm.Rate;
                                            StockProcessViewModel.ExpiryDate = null;
                                            StockProcessViewModel.Specification = ProdOrderLine.Specification;
                                            StockProcessViewModel.Dimension1Id = ProdOrderLine.Dimension1Id;
                                            StockProcessViewModel.Dimension2Id = ProdOrderLine.Dimension2Id;
                                            StockProcessViewModel.CreatedBy = User.Identity.Name;
                                            StockProcessViewModel.CreatedDate = DateTime.Now;
                                            StockProcessViewModel.ModifiedBy = User.Identity.Name;
                                            StockProcessViewModel.ModifiedDate = DateTime.Now;

                                            string StockProcessPostingError = "";
                                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPost(ref StockProcessViewModel);

                                            if (StockProcessPostingError != "")
                                            {
                                                string message = StockProcessPostingError;
                                                ModelState.AddModelError("", message);
                                                return PartialView("_Results", svm);
                                            }


                                            if ((Settings.isPostedInStock ?? false) == false)
                                            {
                                                if (Cnt == 0)
                                                {
                                                    s.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                                }
                                            }

                                            line.StockProcessId = StockProcessViewModel.StockProcessId;
                                        }

                                        UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(ProdOrderLine.ProductId, Product.UnitId, s.UnitConversionForId.Value, DealUnit);

                                        if (uc == null)
                                        {
                                            var product = new ProductService(_unitOfWork).Find(ProdOrderLine.ProductId);
                                            var dealunit = new UnitService(_unitOfWork).Find(DealUnit);
                                            string Msg = "Unit Conversion is not defined for " + product.ProductName + " with " + dealunit.UnitName;
                                            ModelState.AddModelError("", Msg);
                                            PrepareViewBag();
                                            ViewBag.Mode = "Add";
                                            return View("Create", svm);
                                        }

                                        var Unit = new UnitService(_unitOfWork).Find(DealUnit);

                                        line.JobOrderHeaderId = s.JobOrderHeaderId;
                                        line.ProdOrderLineId = ProdOrderLine.ProdOrderLineId;
                                        line.ProductId = ProdOrderLine.ProductId;
                                        line.Dimension1Id = ProdOrderLine.Dimension1Id;
                                        line.Dimension2Id = ProdOrderLine.Dimension2Id;
                                        line.Specification = ProdOrderLine.Specification;
                                        line.Qty = SelectedProdOrd.Qty;
                                        line.Rate = SelectedProdOrd.Rate > 0 ? SelectedProdOrd.Rate : svm.Rate;
                                        //line.LossQty = svm.Loss;
                                        line.LossQty = SelectedProdOrd.Loss > 0 ? SelectedProdOrd.Loss : svm.Loss;
                                        line.NonCountedQty = svm.UnCountedQty;
                                        line.UnitConversionMultiplier = Math.Round( uc.Multiplier, Unit.DecimalPlaces);
                                        line.DealQty = SelectedProdOrd.Qty * line.UnitConversionMultiplier;
                                        line.UnitId = bal.UnitId;
                                        line.DealUnitId = DealUnit;
                                        line.Amount = (line.DealQty * line.Rate);
                                        line.Sr = Sr++;
                                        line.CreatedDate = DateTime.Now;
                                        line.ModifiedDate = DateTime.Now;
                                        line.CreatedBy = User.Identity.Name;
                                        line.ModifiedBy = User.Identity.Name;
                                        line.JobOrderLineId = pk;
                                        line.ObjectState = Model.ObjectState.Added;
                                        new JobOrderLineService(_unitOfWork).Create(line);

                                        new JobOrderLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderLineId, ref db, false);



                                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobOrderLineId, HeaderTableId = s.JobOrderHeaderId, PersonID = s.JobWorkerId, DealQty = line.DealQty, Incentive = SelectedProdOrd.Incentive });

                                        pk++;
                                        Cnt = Cnt + 1;

                                        #region BOMPost

                                        //Saving BOMPOST Data
                                        //Saving BOMPOST Data
                                        //Saving BOMPOST Data
                                        //Saving BOMPOST Data
                                        if (!string.IsNullOrEmpty(Settings.SqlProcConsumption))
                                        {
                                            var BomPostList = new JobOrderLineService(_unitOfWork).GetBomPostingDataForWeaving(line.ProductId, line.Dimension1Id, line.Dimension2Id, null, null, s.ProcessId, line.Qty, s.DocTypeId, Settings.SqlProcConsumption).ToList();

                                            foreach (var Bomitem in BomPostList)
                                            {
                                                JobOrderBom BomPost = new JobOrderBom();
                                                BomPost.CreatedBy = User.Identity.Name;
                                                BomPost.CreatedDate = DateTime.Now;
                                                BomPost.Dimension1Id = Bomitem.Dimension1Id;
                                                BomPost.Dimension2Id = Bomitem.Dimension2Id;
                                                BomPost.JobOrderHeaderId = line.JobOrderHeaderId;
                                                BomPost.JobOrderLineId = line.JobOrderLineId;
                                                BomPost.ModifiedBy = User.Identity.Name;
                                                BomPost.ModifiedDate = DateTime.Now;
                                                BomPost.ProductId = Bomitem.ProductId;
                                                BomPost.Qty = Convert.ToDecimal(Bomitem.Qty);
                                                BomPost.ObjectState = Model.ObjectState.Added;
                                                new JobOrderBomService(_unitOfWork).Create(BomPost);

                                                BomQty += BomPost.Qty;
                                            }
                                        }


                                        OrderQty += line.Qty;
                                        OrderDealQty += line.DealQty;
                                        #endregion

                                    }

                                }
                                //else if (SelectedProdOrd.ProdOrderLineId > 0 && SelectedProdOrd.RefDocLineId.HasValue)
                                else if (SelectedProdOrd.ProdOrderLineId > 0 && SelectedProdOrd.SelectedBarCodes.Split(',').Count() > 0)
                                {
                                    var ProdOrderLine = new ProdOrderLineService(_unitOfWork).Find((SelectedProdOrd.ProdOrderLineId));
                                    var Product = new ProductService(_unitOfWork).Find(ProdOrderLine.ProductId);

                                    //decimal balQty = (from p in db.ViewProdOrderBalance
                                    //                  where p.ProdOrderLineId == SelectedProdOrd.ProdOrderLineId
                                    //                  select p.BalanceQty).FirstOrDefault();

                                    var bal = BalQtyandUnits.Where(m => m.ProdorderLineId == SelectedProdOrd.ProdOrderLineId).FirstOrDefault();

                                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                                    if (!string.IsNullOrEmpty(SelectedProdOrd.SelectedBarCodes) && SelectedProdOrd.SelectedBarCodes.Split(',').Count() <= bal.BalQty)
                                    {

                                        foreach (var BarCode in SelectedProdOrd.SelectedBarCodes.Split(',').Select(Int32.Parse).ToList())
                                        {

                                            JobOrderLine line = new JobOrderLine();

                                            if (Settings.isPostedInStock ?? false)
                                            {
                                                StockViewModel StockViewModel = new StockViewModel();

                                                if (Cnt == 0)
                                                {
                                                    StockViewModel.StockHeaderId = s.StockHeaderId ?? 0;
                                                }
                                                else
                                                {
                                                    if (s.StockHeaderId != null && s.StockHeaderId != 0)
                                                    {
                                                        StockViewModel.StockHeaderId = (int)s.StockHeaderId;
                                                    }
                                                    else
                                                    {
                                                        StockViewModel.StockHeaderId = -1;
                                                    }
                                                }

                                                StockViewModel.StockId = -Cnt;
                                                StockViewModel.DocHeaderId = s.JobOrderHeaderId;
                                                StockViewModel.DocLineId = line.JobOrderLineId;
                                                StockViewModel.DocTypeId = s.DocTypeId;
                                                StockViewModel.StockHeaderDocDate = s.DocDate;
                                                StockViewModel.StockDocDate = s.DocDate;
                                                StockViewModel.DocNo = s.DocNo;
                                                StockViewModel.DivisionId = s.DivisionId;
                                                StockViewModel.SiteId = s.SiteId;
                                                StockViewModel.CurrencyId = null;
                                                StockViewModel.PersonId = s.JobWorkerId;
                                                StockViewModel.ProductId = ProdOrderLine.ProductId;
                                                StockViewModel.HeaderFromGodownId = null;
                                                StockViewModel.HeaderGodownId = s.GodownId;
                                                StockViewModel.HeaderProcessId = s.ProcessId;
                                                StockViewModel.GodownId = (int)s.GodownId;
                                                StockViewModel.Remark = s.Remark;
                                                StockViewModel.Status = s.Status;
                                                StockViewModel.ProcessId = s.ProcessId;
                                                StockViewModel.LotNo = null;
                                                StockViewModel.CostCenterId = s.CostCenterId;
                                                StockViewModel.Qty_Iss = 1;
                                                StockViewModel.Qty_Rec = 0;
                                                StockViewModel.Rate = SelectedProdOrd.Rate > 0 ? SelectedProdOrd.Rate : svm.Rate;
                                                StockViewModel.ExpiryDate = null;
                                                StockViewModel.Specification = ProdOrderLine.Specification;
                                                StockViewModel.Dimension1Id = ProdOrderLine.Dimension1Id;
                                                StockViewModel.Dimension2Id = ProdOrderLine.Dimension2Id;
                                                StockViewModel.CreatedBy = User.Identity.Name;
                                                StockViewModel.CreatedDate = DateTime.Now;
                                                StockViewModel.ModifiedBy = User.Identity.Name;
                                                StockViewModel.ModifiedDate = DateTime.Now;

                                                string StockPostingError = "";
                                                StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                                                if (StockPostingError != "")
                                                {
                                                    string message = StockPostingError;
                                                    ModelState.AddModelError("", message);
                                                    return View("Create", svm);
                                                }

                                                if (Cnt == 0)
                                                {
                                                    s.StockHeaderId = StockViewModel.StockHeaderId;
                                                }
                                                line.StockId = StockViewModel.StockId;
                                            }



                                            if (Settings.isPostedInStockProcess ?? false)
                                            {
                                                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                                                if (s.StockHeaderId != null && s.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                                {
                                                    StockProcessViewModel.StockHeaderId = (int)s.StockHeaderId;
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
                                                StockProcessViewModel.DocHeaderId = s.JobOrderHeaderId;
                                                StockProcessViewModel.DocLineId = line.JobOrderLineId;
                                                StockProcessViewModel.DocTypeId = s.DocTypeId;
                                                StockProcessViewModel.StockHeaderDocDate = s.DocDate;
                                                StockProcessViewModel.StockProcessDocDate = s.DocDate;
                                                StockProcessViewModel.DocNo = s.DocNo;
                                                StockProcessViewModel.DivisionId = s.DivisionId;
                                                StockProcessViewModel.SiteId = s.SiteId;
                                                StockProcessViewModel.CurrencyId = null;
                                                StockProcessViewModel.PersonId = s.JobWorkerId;
                                                StockProcessViewModel.ProductId = ProdOrderLine.ProductId;
                                                StockProcessViewModel.HeaderFromGodownId = null;
                                                StockProcessViewModel.HeaderGodownId = s.GodownId;
                                                StockProcessViewModel.HeaderProcessId = s.ProcessId;
                                                StockProcessViewModel.GodownId = s.GodownId;
                                                StockProcessViewModel.Remark = s.Remark;
                                                StockProcessViewModel.Status = s.Status;
                                                StockProcessViewModel.ProcessId = s.ProcessId;
                                                StockProcessViewModel.LotNo = null;
                                                StockProcessViewModel.CostCenterId = s.CostCenterId;
                                                StockProcessViewModel.Qty_Iss = 0;
                                                StockProcessViewModel.Qty_Rec = 1;
                                                StockProcessViewModel.Rate = SelectedProdOrd.Rate > 0 ? SelectedProdOrd.Rate : svm.Rate;
                                                StockProcessViewModel.ExpiryDate = null;
                                                StockProcessViewModel.Specification = ProdOrderLine.Specification;
                                                StockProcessViewModel.Dimension1Id = ProdOrderLine.Dimension1Id;
                                                StockProcessViewModel.Dimension2Id = ProdOrderLine.Dimension2Id;
                                                StockProcessViewModel.CreatedBy = User.Identity.Name;
                                                StockProcessViewModel.CreatedDate = DateTime.Now;
                                                StockProcessViewModel.ModifiedBy = User.Identity.Name;
                                                StockProcessViewModel.ModifiedDate = DateTime.Now;

                                                string StockProcessPostingError = "";
                                                StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPost(ref StockProcessViewModel);

                                                if (StockProcessPostingError != "")
                                                {
                                                    string message = StockProcessPostingError;
                                                    ModelState.AddModelError("", message);
                                                    return PartialView("_Results", svm);
                                                }


                                                if ((Settings.isPostedInStock ?? false) == false)
                                                {
                                                    if (Cnt == 0)
                                                    {
                                                        s.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                                    }
                                                }

                                                line.StockProcessId = StockProcessViewModel.StockProcessId;
                                            }

                                            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(ProdOrderLine.ProductId, Product.UnitId, s.UnitConversionForId.Value, DealUnit);


                                            if (uc == null)
                                            {
                                                var product = new ProductService(_unitOfWork).Find(ProdOrderLine.ProductId);
                                                var dealunit = new UnitService(_unitOfWork).Find(DealUnit);
                                                string Msg = "Unit Conversion is not defined for " + product.ProductName + " with " + dealunit.UnitName;
                                                ModelState.AddModelError("", Msg);
                                                PrepareViewBag();
                                                ViewBag.Mode = "Add";
                                                return View("Create", svm);
                                            }

                                            var Unit = new UnitService(_unitOfWork).Find(DealUnit);

                                            line.ProductUidId = BarCode;
                                            line.JobOrderHeaderId = s.JobOrderHeaderId;
                                            line.ProdOrderLineId = ProdOrderLine.ProdOrderLineId;
                                            line.ProductId = ProdOrderLine.ProductId;
                                            line.Dimension1Id = ProdOrderLine.Dimension1Id;
                                            line.Dimension2Id = ProdOrderLine.Dimension2Id;
                                            line.Specification = ProdOrderLine.Specification;
                                            line.Qty = 1;
                                            line.Rate = SelectedProdOrd.Rate > 0 ? SelectedProdOrd.Rate : svm.Rate;
                                            line.NonCountedQty = svm.UnCountedQty;
                                            line.LossQty = svm.Loss;
                                            line.UnitConversionMultiplier = Math.Round(uc.Multiplier, Unit.DecimalPlaces);
                                            line.DealQty = 1 * line.UnitConversionMultiplier;
                                            line.UnitId = bal.UnitId;
                                            line.DealUnitId = DealUnit;
                                            line.Amount = (line.DealQty * line.Rate);
                                            line.Sr = Sr++;
                                            line.CreatedDate = DateTime.Now;
                                            line.ModifiedDate = DateTime.Now;
                                            line.CreatedBy = User.Identity.Name;
                                            line.ModifiedBy = User.Identity.Name;
                                            line.JobOrderLineId = pk;
                                            line.ObjectState = Model.ObjectState.Added;
                                            new JobOrderLineService(_unitOfWork).Create(line);

                                            new JobOrderLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderLineId, ref db, false);

                                            if (Settings.CalculationId.HasValue)
                                            {
                                                LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobOrderLineId, HeaderTableId = s.JobOrderHeaderId, PersonID = s.JobWorkerId, DealQty = line.DealQty, Incentive = SelectedProdOrd.Incentive });
                                            }
                                            pk++;
                                            Cnt = Cnt + 1;


                                            #region BOMPost

                                            //Saving BOMPOST Data
                                            //Saving BOMPOST Data
                                            //Saving BOMPOST Data
                                            //Saving BOMPOST Data
                                            if (!string.IsNullOrEmpty(Settings.SqlProcConsumption))
                                            {
                                                var BomPostList = new JobOrderLineService(_unitOfWork).GetBomPostingDataForWeaving(line.ProductId, line.Dimension1Id, line.Dimension2Id, null, null, s.ProcessId, line.Qty, s.DocTypeId, Settings.SqlProcConsumption).ToList();

                                                foreach (var Bomitem in BomPostList)
                                                {
                                                    JobOrderBom BomPost = new JobOrderBom();
                                                    BomPost.CreatedBy = User.Identity.Name;
                                                    BomPost.CreatedDate = DateTime.Now;
                                                    BomPost.Dimension1Id = Bomitem.Dimension1Id;
                                                    BomPost.Dimension2Id = Bomitem.Dimension2Id;
                                                    BomPost.JobOrderHeaderId = line.JobOrderHeaderId;
                                                    BomPost.JobOrderLineId = line.JobOrderLineId;
                                                    BomPost.ModifiedBy = User.Identity.Name;
                                                    BomPost.ModifiedDate = DateTime.Now;
                                                    BomPost.ProductId = Bomitem.ProductId;
                                                    BomPost.Qty = Convert.ToDecimal(Bomitem.Qty);
                                                    BomPost.ObjectState = Model.ObjectState.Added;
                                                    new JobOrderBomService(_unitOfWork).Create(BomPost);

                                                    BomQty += BomPost.Qty;

                                                }
                                            }

                                            OrderQty += line.Qty;
                                            OrderDealQty += line.DealQty;

                                            #endregion


                                            BarCodesToUpdate.Add(line);


                                        }


                                    }

                                }


                            }


                            if (CostCenterGenerated)
                            {
                                ProdOrderLine POL = new ProdOrderLineService(_unitOfWork).Find(ProdOrdersAndQtys.FirstOrDefault().ProdOrderLineId);

                                CostCenterStatusExtended Rec = new CostCenterStatusExtended();
                                Rec.CostCenterId = s.CostCenterId.Value;
                                Rec.ProductId = (int)POL.ProductId;
                                Rec.Rate = ProdOrdersAndQtys.Min(m => m.Rate) > 0 ? ProdOrdersAndQtys.Min(m => m.Rate) : svm.Rate;
                                Rec.OrderQty = Rec.OrderQty ?? 0 + OrderQty;
                                Rec.OrderDealQty = Rec.OrderDealQty ?? 0 + OrderDealQty;
                                Rec.BOMQty = Rec.BOMQty ?? 0 + BomQty;

                                new CostCenterStatusService(_unitOfWork).CreateLineStatusExtended(Rec);

                            }
                            else
                            {
                                decimal StatExtRate = ProdOrdersAndQtys.Min(m => m.Rate) > 0 ? ProdOrdersAndQtys.Min(m => m.Rate) : svm.Rate;

                                var CostCenterStatusExtended = db.CostCenterStatusExtended.Find(s.CostCenterId);
                                CostCenterStatusExtended.Rate = (!CostCenterStatusExtended.Rate.HasValue || CostCenterStatusExtended.Rate == 0)
                                    ? StatExtRate
                                    : (CostCenterStatusExtended.Rate > StatExtRate ? StatExtRate : CostCenterStatusExtended.Rate);
                                CostCenterStatusExtended.OrderQty = CostCenterStatusExtended.OrderQty ?? 0 + OrderQty;
                                CostCenterStatusExtended.OrderDealQty = CostCenterStatusExtended.OrderDealQty ?? 0 + OrderDealQty;
                                CostCenterStatusExtended.BOMQty = CostCenterStatusExtended.BOMQty ?? 0 + BomQty;

                                CostCenterStatusExtended.ObjectState = Model.ObjectState.Modified;
                                _unitOfWork.Repository<CostCenterStatusExtended>().Update(CostCenterStatusExtended);
                            }

                            //new JobOrderHeaderService(_unitOfWork).Update(s);

                            new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, s.JobOrderHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobOrderHeaderCharges", "Web.JobOrderLineCharges", out PersonCount, s.DocTypeId, s.SiteId, s.DivisionId);

                            //Saving Charges
                            foreach (var item in LineCharges)
                            {

                                JobOrderLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobOrderLineCharge>(item);
                                PoLineCharge.ObjectState = Model.ObjectState.Added;
                                new JobOrderLineChargeService(_unitOfWork).Create(PoLineCharge);

                            }


                            //Saving Header charges
                            for (int i = 0; i < HeaderCharges.Count(); i++)
                            {

                                if (!HeaderChargeEdit)
                                {
                                    JobOrderHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobOrderHeaderCharge>(HeaderCharges[i]);
                                    POHeaderCharge.HeaderTableId = s.JobOrderHeaderId;
                                    POHeaderCharge.PersonID = s.JobWorkerId;
                                    POHeaderCharge.ObjectState = Model.ObjectState.Added;
                                    new JobOrderHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                                }
                                else
                                {
                                    var footercharge = new JobOrderHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                                    footercharge.Rate = HeaderCharges[i].Rate;
                                    footercharge.Amount = HeaderCharges[i].Amount;
                                    new JobOrderHeaderChargeService(_unitOfWork).Update(footercharge);
                                }

                            }

                        }
                        string Errormessage = "";
                        try
                        {
                            _unitOfWork.Save();
                        }

                        catch (Exception ex)
                        {
                            Errormessage = _exception.HandleException(ex);
                            ModelState.AddModelError("", Errormessage);
                            PrepareViewBag();
                            ViewBag.Mode = "Add";
                            return View("Create", svm);

                        }


                        if (s.CostCenterId.HasValue && CostCenterGenerated)
                        {
                            var CC = new CostCenterService(_unitOfWork).Find(s.CostCenterId.Value);
                            CC.ReferenceDocId = s.JobOrderHeaderId;
                            new CostCenterService(_unitOfWork).Update(CC);
                        }

                        if (string.IsNullOrEmpty(Errormessage))
                        {

                            foreach (var JorOrderLine in BarCodesToUpdate)
                            {
                                ProductUid Uid = new ProductUidService(_unitOfWork).Find(JorOrderLine.ProductUidId.Value);
                                JobOrderHeader Header = new JobOrderHeaderService(_unitOfWork).Find(JorOrderLine.JobOrderHeaderId);
                                Uid.LastTransactionDocId = JorOrderLine.JobOrderHeaderId;
                                Uid.LastTransactionDocDate = Header.DocDate;
                                Uid.LastTransactionDocNo = Header.DocNo;
                                Uid.LastTransactionDocTypeId = Header.DocTypeId;
                                Uid.LastTransactionLineId = JorOrderLine.JobOrderLineId;
                                Uid.LastTransactionPersonId = Header.JobWorkerId;
                                Uid.Status = ProductUidStatusConstants.Issue;
                                Uid.CurrenctProcessId = Header.ProcessId;
                                Uid.CurrenctGodownId = null;
                                Uid.ModifiedBy = User.Identity.Name;
                                Uid.ModifiedDate = DateTime.Now;
                                new ProductUidService(_unitOfWork).Update(Uid);
                            }


                            try
                            {
                                _unitOfWork.Save();
                            }

                            catch (Exception ex)
                            {
                                Errormessage = _exception.HandleException(ex);
                                ModelState.AddModelError("", Errormessage);
                                PrepareViewBag();
                                ViewBag.Mode = "Add";
                                return View("Create", svm);

                            }




                        }

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = s.DocTypeId,
                            DocId = s.JobOrderHeaderId,
                            ActivityType = (int)ActivityTypeContants.WizardCreate,
                            DocNo = s.DocNo,                            
                            DocDate = s.DocDate,
                            DocStatus = s.Status,
                        }));

                        System.Web.HttpContext.Current.Session.Remove("BarCodesWeavingWizardProdOrder");

                        return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobOrderHeader/Submit/" + s.JobOrderHeaderId);
                    }
                    else
                    {
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobOrderHeader/Index/" + s.DocTypeId);
                    }

                }
                else
                {

                }

            }
            PrepareViewBag();
            ViewBag.Mode = "Add";
            //return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobOrderHeader/Submit/"+s.JobOrderHeaderId);
            return View("Create", svm);
        }

        public JsonResult GetBarCodes(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = new JobOrderLineService(_unitOfWork).GetBarCodesForWeavingWizard(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new JobOrderLineService(_unitOfWork).GetBarCodesForWeavingWizard(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _JobOrderHeaderService.GetCustomPerson(filter, searchTerm);
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
            {
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
