using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Data.Infrastructure;
using AutoMapper;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using System.Data.SqlClient;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class DyeingOrderWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IJobOrderHeaderService _JobOrderHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public DyeingOrderWizardController(IJobOrderHeaderService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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

        public ActionResult CreateDyeingOrder()
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.DyeingOrder).DocumentTypeId;

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

            return View("CreateDyeingOrder");
        }

        public JsonResult PendingProdOrders()
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterDocCateId = new SqlParameter("@DocumentCategoryId", new DocumentCategoryService(_unitOfWork).FindByName(TransactionDocCategoryConstants.DyeingPlanning).DocumentCategoryId);

            IEnumerable<DyeingOrderWizardViewModel> FgetProdOrders = db.Database.SqlQuery<DyeingOrderWizardViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_DyeingOrderWizard_Step1 @SiteId, @DivisionId, @DocumentCategoryId", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDocCateId).ToList();


            return Json(new { data = FgetProdOrders }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SelectedProdOrderList(string ProdOrderLineId)
        {
            SqlParameter SqlParameterProdOrderLineId = new SqlParameter("@ProdOrderLineIdList", ProdOrderLineId);

            IEnumerable<DyeingOrderWizardViewModel> FgetProdOrders = db.Database.SqlQuery<DyeingOrderWizardViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_DyeingOrderWizard_Step2 @ProdOrderLineIdList", SqlParameterProdOrderLineId).ToList();
            return Json(new { Success = true, Data = FgetProdOrders }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SummarizeProdOrderList(string ProdOrderLineId)
        {
            TempData["TempProdOrders"] = ProdOrderLineId;
            return Json(new { Success = "URL", Data = "/Rug/DyeingOrderWizard/SummarizeProdOrders" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SummarizeProdOrders()
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.DyeingOrder).DocumentTypeId;

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            ViewBag.AllowedPerc = settings.ExcessQtyAllowedPer;

            return View("SummarizeProdOrders");
        }

        public JsonResult GetProdOrdersForSummary()
        {
            string ProdOrderLineIds = (string)TempData["TempProdOrders"];

            List<int> ProdOrderIds = new List<int>();

            if (!string.IsNullOrEmpty(ProdOrderLineIds))
            {
                foreach (var item in ProdOrderLineIds.Split(','))
                    if (!string.IsNullOrWhiteSpace(item))
                        ProdOrderIds.Add(Convert.ToInt32(item));

                var BAlRec = (from p in db.ViewProdOrderBalance
                              join t in db.ProdOrderLine on p.ProdOrderLineId equals t.ProdOrderLineId
                              join t2 in db.ProdOrderHeader on p.ProdOrderHeaderId equals t2.ProdOrderHeaderId
                              where ProdOrderIds.Contains(p.ProdOrderLineId)
                              orderby t2.DocDate, t2.DocNo, t.Sr
                              select new DyeingOrderWizardViewModel
                              {
                                  Dimension1List = p.Dimension1.Dimension1Name,
                                  Dimension2Name = p.Dimension2.Dimension2Name,
                                  DocDate = p.IndentDate,
                                  ProdOrderHeaderId = p.ProdOrderHeaderId,
                                  ProdOrderLineId = p.ProdOrderLineId,
                                  ProdOrderNo = p.ProdOrderNo,
                                  BuyerCode = t2.Buyer.Code,
                                  ProductList = p.Product.ProductName,
                                  Qty = p.BalanceQty,
                                  BalanceQty = p.BalanceQty,
                              }).ToList();

                var tBAlRec = (from p in BAlRec
                               select new DyeingOrderWizardViewModel
                               {
                                   Dimension1List = p.Dimension1List,
                                   Dimension2Name = p.Dimension2Name,
                                   ProdOrderDate = p.DocDate.ToString("dd/MMM/yyyy"),
                                   ProdOrderHeaderId = p.ProdOrderHeaderId,
                                   ProdOrderLineId = p.ProdOrderLineId,
                                   ProdOrderNo = p.ProdOrderNo,
                                   BuyerCode = p.BuyerCode,
                                   ProductList = p.ProductList,
                                   Qty = p.Qty,
                                   BalanceQty = p.BalanceQty,
                               }).ToList();

                return Json(new { data = tBAlRec }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ConfirmProdOrderList(List<ProdOrderHeaderListViewModel> ProdOrderLineId, bool CancelBalProdOrdrs)
        {
            System.Web.HttpContext.Current.Session["ConfirmProdOrderIds"] = ProdOrderLineId;
            System.Web.HttpContext.Current.Session["CancelBalProdOrdrs"] = CancelBalProdOrdrs;

            return Json(new { Success = "URL", Data = "/Rug/DyeingOrderWizard/Create" }, JsonRequestBehavior.AllowGet);
        }


        private void PrepareViewBag()
        {
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
        }

        // GET: /JobOrderHeader/Create

        public ActionResult Create()//DocumentTypeId
        {
            JobOrderHeaderViewModel p = new JobOrderHeaderViewModel();

            p.DocDate = DateTime.Now;
            p.DueDate = DateTime.Now;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.CreatedDate = DateTime.Now;

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.DyeingOrder).DocumentTypeId;

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


            List<PerkViewModel> Perks = new List<PerkViewModel>();
            //Perks
            if (p.JobOrderSettings.Perks != null)
                foreach (var item in p.JobOrderSettings.Perks.Split(',').ToList())
                {
                    PerkViewModel temp = Mapper.Map<Perk, PerkViewModel>(new PerkService(_unitOfWork).Find(Convert.ToInt32(item)));
                    Perks.Add(temp);
                }

            p.PerkViewModel = Perks;

            p.ProcessId = settings.ProcessId;
            PrepareViewBag();
            p.DocTypeId = DocTypeId;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobOrderHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobOrderHeaderViewModel svm)
        {
            JobOrderHeader s = Mapper.Map<JobOrderHeaderViewModel, JobOrderHeader>(svm);
            ProdOrderCancelHeader cHeader = new ProdOrderCancelHeader();

            if (svm.JobOrderSettings != null)
            {
                if (svm.JobOrderSettings.isMandatoryCostCenter == true && (svm.CostCenterId <= 0 || svm.CostCenterId == null))
                {
                    ModelState.AddModelError("CostCenterId", "The CostCenter field is required");
                }
                if (svm.JobOrderSettings.isMandatoryMachine == true && (svm.MachineId <= 0 || svm.MachineId == null))
                {
                    ModelState.AddModelError("MachineId", "The Machine field is required");
                }
                if (svm.JobOrderSettings.isPostedInStock == true && !svm.GodownId.HasValue)
                {
                    ModelState.AddModelError("GodownId", "The Godown field is required");
                }
            }

            #region DocTypeTimeLineValidation

            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
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

                List<ProdOrderHeaderListViewModel> ProdOrderIds = (List<ProdOrderHeaderListViewModel>)System.Web.HttpContext.Current.Session["ConfirmProdOrderIds"];
                bool CancelBalProdOrders = (bool)System.Web.HttpContext.Current.Session["CancelBalProdOrdrs"];
                bool CreateDyeingOrder = ProdOrderIds.Any(m => m.Qty > 0);

                var ProdOrderLineIds = ProdOrderIds.Select(m => m.ProdOrderLineId).ToArray();

                var BalProdOrders = (from p in db.ViewProdOrderBalance.AsNoTracking()
                                     where ProdOrderLineIds.Contains(p.ProdOrderLineId)
                                     select p).ToList();

                var ProdOrderLines = (from p in db.ProdOrderLine.AsNoTracking()
                                      where ProdOrderLineIds.Contains(p.ProdOrderLineId)
                                      select p).ToList();

                var ProductIds = BalProdOrders.Select(m => m.ProductId).ToList();

                var Products = (from p in db.Product.AsNoTracking()
                                where ProductIds.Contains(p.ProductId)
                                select p).ToList();

                bool CancelQty = (from p in ProdOrderIds
                                  join t in BalProdOrders
                                  on p.ProdOrderLineId equals t.ProdOrderLineId
                                  where (t.BalanceQty - p.Qty) > 0
                                  select p).Any();
                CancelBalProdOrders = (CancelBalProdOrders && CancelQty);

                if (CreateDyeingOrder)
                {
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
                }

                if (CancelBalProdOrders)
                {
                    cHeader.DocNo = s.DocNo;
                    cHeader.DocDate = s.DocDate;
                    cHeader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDoctypeConstants.DyeingPlanCancel).DocumentTypeId;
                    cHeader.DivisionId = s.DivisionId;
                    cHeader.SiteId = s.SiteId;
                    cHeader.CreatedBy = User.Identity.Name;
                    cHeader.CreatedDate = DateTime.Now;
                    cHeader.ModifiedBy = User.Identity.Name;
                    cHeader.ModifiedDate = DateTime.Now;
                    cHeader.ReferenceDocId = s.JobOrderHeaderId;
                    cHeader.ReferenceDocTypeId = s.DocTypeId;
                    cHeader.Remark = s.Remark;
                    cHeader.ObjectState = Model.ObjectState.Added;
                    new ProdOrderCancelHeaderService(_unitOfWork).Create(cHeader);
                }

                int Cnt = 0;

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

                foreach (var ProdORderLineId in ProdOrderIds)
                {

                    var BalProdOrderLine = BalProdOrders.Where(m => m.ProdOrderLineId == ProdORderLineId.ProdOrderLineId).FirstOrDefault();
                    var Product = Products.Where(m => m.ProductId == BalProdOrderLine.ProductId).FirstOrDefault();


                    if (ProdORderLineId.Qty <= BalProdOrderLine.BalanceQty && ProdORderLineId.Qty > 0)
                        if (((Settings.isVisibleRate == false || Settings.isVisibleRate == true)))
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
                                StockViewModel.ProductId = BalProdOrderLine.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                StockViewModel.HeaderGodownId = s.GodownId;
                                StockViewModel.HeaderProcessId = s.ProcessId;
                                StockViewModel.GodownId = (int)s.GodownId;
                                StockViewModel.Remark = s.Remark;
                                StockViewModel.Status = s.Status;
                                StockViewModel.ProcessId = s.ProcessId;
                                StockViewModel.LotNo = null;
                                StockViewModel.CostCenterId = s.CostCenterId;
                                StockViewModel.Qty_Iss = ProdORderLineId.Qty;
                                StockViewModel.Qty_Rec = 0;
                                StockViewModel.Rate = 0;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = ProdOrderLines.Where(m => m.ProdOrderLineId == ProdORderLineId.ProdOrderLineId).FirstOrDefault().Specification;
                                StockViewModel.Dimension1Id = BalProdOrderLine.Dimension1Id;
                                StockViewModel.Dimension2Id = BalProdOrderLine.Dimension2Id;
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
                                StockProcessViewModel.ProductId = BalProdOrderLine.ProductId;
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
                                StockProcessViewModel.Qty_Rec = ProdORderLineId.Qty;
                                StockProcessViewModel.Rate = 0;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = ProdOrderLines.Where(m => m.ProdOrderLineId == ProdORderLineId.ProdOrderLineId).FirstOrDefault().Specification;
                                StockProcessViewModel.Dimension1Id = BalProdOrderLine.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = BalProdOrderLine.Dimension2Id;
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



                            line.JobOrderHeaderId = s.JobOrderHeaderId;
                            line.ProdOrderLineId = BalProdOrderLine.ProdOrderLineId;
                            line.ProductId = BalProdOrderLine.ProductId;
                            line.Dimension1Id = BalProdOrderLine.Dimension1Id;
                            line.Dimension2Id = BalProdOrderLine.Dimension2Id;
                            line.Specification = ProdOrderLines.Where(m => m.ProdOrderLineId == ProdORderLineId.ProdOrderLineId).FirstOrDefault().Specification;
                            line.Qty = ProdORderLineId.Qty;
                            line.Rate = ProdORderLineId.Rate;
                            line.DealQty = ProdORderLineId.Qty;
                            line.UnitId = Product.UnitId;
                            line.DealUnitId = Product.UnitId;
                            line.Amount = (line.DealQty * line.Rate);
                            line.UnitConversionMultiplier = 1;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobOrderLineId = pk;
                            line.Sr = pk;
                            line.ObjectState = Model.ObjectState.Added;
                            new JobOrderLineService(_unitOfWork).Create(line);

                            new JobOrderLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderLineId, ref db, false);

                            LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobOrderLineId, HeaderTableId = s.JobOrderHeaderId, PersonID = s.JobWorkerId, DealQty = line.DealQty });

                            pk++;
                            Cnt++;
                        }


                    if (CancelBalProdOrders && (BalProdOrderLine.BalanceQty - ProdORderLineId.Qty > 0))
                    {
                        ProdOrderCancelLine cLine = new ProdOrderCancelLine();
                        cLine.CreatedBy = User.Identity.Name;
                        cLine.CreatedDate = DateTime.Now;
                        cLine.ModifiedBy = User.Identity.Name;
                        cLine.ModifiedDate = DateTime.Now;
                        cLine.ProdOrderCancelHeaderId = cHeader.ProdOrderCancelHeaderId;
                        cLine.ProdOrderLineId = ProdORderLineId.ProdOrderLineId;
                        cLine.Qty = (BalProdOrderLine.BalanceQty - ProdORderLineId.Qty);
                        cLine.ReferenceDocTypeId = cHeader.ReferenceDocTypeId;
                        cLine.ObjectState = Model.ObjectState.Added;
                        new ProdOrderCancelLineService(_unitOfWork).Create(cLine);
                    }

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



                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    PrepareViewBag();
                    ViewBag.Mode = "Add";
                    return View("Create", svm);

                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.JobOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Added,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));

                System.Web.HttpContext.Current.Session.Remove("ConfirmProdOrderIds");
                System.Web.HttpContext.Current.Session.Remove("CancelBalProdOrdrs");

                if (CreateDyeingOrder)
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobOrderHeader/Submit/" + s.JobOrderHeaderId);
                else
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobOrderHeader/Index/" + s.DocTypeId);

            }

            PrepareViewBag();
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }

        public JsonResult GetShadeWiseBal(string IdList)
        {

            List<int> ProdOrderLineIds = new List<int>();


            if (!string.IsNullOrEmpty(IdList))
                foreach (var item in IdList.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse))
                    ProdOrderLineIds.Add(item);

            var Arr = ProdOrderLineIds.ToArray();

            var CancelDetail = (from p in db.ViewProdOrderBalance
                                where Arr.Contains(p.ProdOrderLineId)
                                join t3 in db.Dimension1 on p.Dimension1Id equals t3.Dimension1Id into table3
                                from tab3 in table3.DefaultIfEmpty()
                                join t4 in db.Dimension2 on p.Dimension2Id equals t4.Dimension2Id
                                select new
                                {
                                    p.BalanceQty,
                                    tab3.Dimension1Name,
                                    t4.Dimension2Name,
                                }).ToList();

            return Json(new { Data = CancelDetail }, JsonRequestBehavior.AllowGet);

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