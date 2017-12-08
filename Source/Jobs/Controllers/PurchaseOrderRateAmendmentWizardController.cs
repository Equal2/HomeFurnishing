using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using AutoMapper;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using PurchaseOrderAmendmentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;



namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseOrderRateAmendmentWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseOrderAmendmentHeaderService _PurchaseOrderAmendmentHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PurchaseOrderRateAmendmentWizardController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseOrderAmendmentHeaderService = new PurchaseOrderAmendmentHeaderService(db);
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!PurchaseOrderAmendmentEvents.Initialized)
            {
                PurchaseOrderAmendmentEvents Obj = new PurchaseOrderAmendmentEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag(int id)
        {
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            ViewBag.Name = DocType.DocumentTypeName;
            ViewBag.id = id;
            ViewBag.ReasonList = new ReasonService(_unitOfWork).GetReasonList(DocType.DocumentTypeName).ToList();

        }
        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("RateAmendtmentWizard", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult RateAmendtmentWizard(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            PurchaseOrderAmendmentHeaderViewModel vm = new PurchaseOrderAmendmentHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseOrderAmendment", "PurchaseOrderSettings", new { id = id }).Warning("Please create Purchase amendment settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            return View();
        }

        public JsonResult AjaxGetJsonData(int DocType, DateTime? FromDate, DateTime? ToDate, string PurchaseOrderHeaderId, string SupplierId
            , string ProductId, string Dimension1Id, string Dimension2Id, string ProductGroupId, string ProductCategoryId, decimal? Rate, decimal NewRate
            , decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            string SortColName = "";


            // note: we only sort one column at a time
            if (Request.Form["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
                SortColName = Request.Form["columns[" + sortColumn + "][data]"];
            }
            if (Request.Form["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            bool Success = true;

            var data = FilterData(DocType, FromDate, ToDate, PurchaseOrderHeaderId, SupplierId,
                                            ProductId, Dimension1Id, Dimension2Id, ProductGroupId, ProductCategoryId, Rate, NewRate, MultiplierGT, MultiplierLT, Sample);

            var RecCount = data.Count();

            if (RecCount > 1000 || RecCount == 0)
            {
                Success = false;
                return Json(new { Success = Success, Message = (RecCount > 1000 ? "No of records exceeding 1000." : "No Records found.") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CList = data.ToList().Select(m => new PurchaseOrderAmendmentWizardViewModel
                {
                    SOrderDate = m.OrderDate.ToString("dd/MMM/yyyy"),
                    PurchaseOrderLineId = m.PurchaseOrderLineId,
                    OrderNo = m.OrderNo,
                    SupplierName = m.SupplierName,
                    ProductName = m.ProductName,
                    SupplierId = m.SupplierId,
                    Dimension1Name = m.Dimension1Name,
                    Dimension2Name = m.Dimension2Name,
                    OldRate = m.OldRate,
                    Rate = m.Rate,
                    ProductGroupName = m.ProductGroupName
                }).ToList();

                return Json(new { Data = CList, Success = Success }, JsonRequestBehavior.AllowGet);
            }
        }


        private static int TOTAL_ROWS = 0;
        //private static readonly List<DataItem> _data = CreateData();    
        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<PurchaseOrderAmendmentWizardViewModel> data { get; set; }
        }

        // here we simulate data from a database table.
        // !!!!DO NOT DO THIS IN REAL APPLICATION !!!!
        //private static List<DataItem> CreateData()
        //{
        //    Random rnd = new Random();
        //    List<DataItem> list = new List<DataItem>();
        //    for (int i = 1; i <= TOTAL_ROWS; i++)
        //    {
        //        DataItem item = new DataItem();
        //        item.Name = "Name_" + i.ToString().PadLeft(5, '0');
        //        DateTime dob = new DateTime(1900 + rnd.Next(1, 100), rnd.Next(1, 13), rnd.Next(1, 28));
        //        item.Age = ((DateTime.Now - dob).Days / 365).ToString();
        //        item.DoB = dob.ToShortDateString();
        //        list.Add(item);
        //    }
        //    return list;
        //}

        private int SortString(string s1, string s2, string sortDirection)
        {
            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }

        private int SortInteger(string s1, string s2, string sortDirection)
        {
            int i1 = int.Parse(s1);
            int i2 = int.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }

        private int SortDateTime(string s1, string s2, string sortDirection)
        {
            DateTime d1 = DateTime.Parse(s1);
            DateTime d2 = DateTime.Parse(s2);
            return sortDirection == "asc" ? d1.CompareTo(d2) : d2.CompareTo(d1);
        }

        // here we simulate SQL search, sorting and paging operations
        private IQueryable<PurchaseOrderAmendmentWizardViewModel> FilterData(int DocType, DateTime? FromDate, DateTime? ToDate,
                                                                    string PurchaseOrderHeaderId, string SupplierId, string ProductId, string Dimension1Id,
            string Dimension2Id, string ProductGroupId, string ProductCategoryId, decimal? Rate, decimal NewRate, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {

            List<int> PurchaseOrderHeaderIds = new List<int>();
            if (!string.IsNullOrEmpty(PurchaseOrderHeaderId))
                foreach (var item in PurchaseOrderHeaderId.Split(','))
                    PurchaseOrderHeaderIds.Add(Convert.ToInt32(item));


            List<int> SupplierIds = new List<int>();
            if (!string.IsNullOrEmpty(SupplierId))
                foreach (var item in SupplierId.Split(','))
                    SupplierIds.Add(Convert.ToInt32(item));

            //List<int> ProductIds = new List<int>();
            //if (!string.IsNullOrEmpty(ProductId))
            //    foreach (var item in ProductId.Split(','))
            //        ProductIds.Add(Convert.ToInt32(item));

            List<int> Dimension1Ids = new List<int>();
            if (!string.IsNullOrEmpty(Dimension1Id))
                foreach (var item in Dimension1Id.Split(','))
                    Dimension1Ids.Add(Convert.ToInt32(item));

            List<int> Dimension2Ids = new List<int>();
            if (!string.IsNullOrEmpty(Dimension2Id))
                foreach (var item in Dimension2Id.Split(','))
                    Dimension2Ids.Add(Convert.ToInt32(item));

            //List<int> ProductGroupIds = new List<int>();
            //if (!string.IsNullOrEmpty(ProductGroupId))
            //    foreach (var item in ProductGroupId.Split(','))
            //        ProductGroupIds.Add(Convert.ToInt32(item));

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocType, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            IQueryable<PurchaseOrderAmendmentWizardViewModel> _data = from p in db.ViewPurchaseOrderBalanceForInvoice
                                                                      join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId
                                                                      join jw in db.Persons on p.SupplierId equals jw.PersonID into jwtable
                                                                      from jwtab in jwtable.DefaultIfEmpty()
                                                                      join prod in db.FinishedProduct on p.ProductId equals prod.ProductId into prodtable
                                                                      from prodtab in prodtable.DefaultIfEmpty()
                                                                      join dim1 in db.Dimension1 on p.Dimension1Id equals dim1.Dimension1Id into dimtable
                                                                      from dimtab in dimtable.DefaultIfEmpty()
                                                                      join dim2 in db.Dimension2 on p.Dimension2Id equals dim2.Dimension2Id into dim2table
                                                                      from dim2tab in dim2table.DefaultIfEmpty()
                                                                      join pg in db.ProductGroups on prodtab.ProductGroupId equals pg.ProductGroupId into pgtable
                                                                      from pgtab in pgtable.DefaultIfEmpty()
                                                                      join pc in db.ProductCategory on prodtab.ProductCategoryId equals pc.ProductCategoryId into pctable
                                                                      from pctab in pctable.DefaultIfEmpty()
                                                                      where p.BalanceQty > 0
                                                                      && (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.PurchaseOrderHeader.DocTypeId.ToString()))
                                                                      select new PurchaseOrderAmendmentWizardViewModel
                                                                      {
                                                                          OrderDate = p.OrderDate,
                                                                          OrderNo = p.PurchaseOrderNo,
                                                                          PurchaseOrderLineId = p.PurchaseOrderLineId,
                                                                          OldRate = p.Rate,
                                                                          Rate = NewRate,
                                                                          SupplierName = jwtab.Name,
                                                                          ProductName = prodtab.ProductName,
                                                                          Dimension1Name = dimtab.Dimension1Name,
                                                                          Dimension2Name = dim2tab.Dimension2Name,
                                                                          PurchaseOrderHeaderId = p.PurchaseOrderHeaderId,
                                                                          SupplierId = p.SupplierId,
                                                                          ProductGroupId = pgtab.ProductGroupId,
                                                                          ProductGroupName = pgtab.ProductGroupName,
                                                                          ProductCategoryId = pctab.ProductCategoryId,
                                                                          ProductCategoryName = pctab.ProductCategoryName,
                                                                          ProdId = p.ProductId,
                                                                          Dimension1Id = p.Dimension1Id,
                                                                          Dimension2Id = p.Dimension2Id,
                                                                          UnitConversionMultiplier = t.UnitConversionMultiplier,
                                                                          Sample = prodtab.IsSample,
                                                                      };



            //if (FromDate.HasValue)
            //    _data = from p in _data
            //            where p.OrderDate >= FromDate
            //            select p;

            if (FromDate.HasValue)
                _data = _data.Where(m => m.OrderDate >= FromDate);

            if (ToDate.HasValue)
                _data = _data.Where(m => m.OrderDate <= ToDate);

            if (Rate.HasValue && Rate.Value > 0)
                _data = _data.Where(m => m.OldRate == Rate.Value);

            if (MultiplierGT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier >= MultiplierGT.Value);

            if (MultiplierLT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier <= MultiplierLT.Value);


            if (!string.IsNullOrEmpty(PurchaseOrderHeaderId))
                _data = _data.Where(m => PurchaseOrderHeaderIds.Contains(m.PurchaseOrderHeaderId));

            if (!string.IsNullOrEmpty(SupplierId))
                _data = _data.Where(m => SupplierIds.Contains(m.SupplierId));

            if (!string.IsNullOrEmpty(ProductId))
                _data = _data.Where(m => m.ProductName.Contains(ProductId));

            if (!string.IsNullOrEmpty(Dimension1Id))
                _data = _data.Where(m => Dimension1Ids.Contains(m.Dimension1Id ?? 0));

            if (!string.IsNullOrEmpty(Dimension2Id))
                _data = _data.Where(m => Dimension2Ids.Contains(m.Dimension2Id ?? 0));

            if (!string.IsNullOrEmpty(ProductGroupId))
                _data = _data.Where(m => m.ProductGroupName.Contains(ProductGroupId));

            if (!string.IsNullOrEmpty(ProductCategoryId))
                _data = _data.Where(m => m.ProductCategoryName.Contains(ProductCategoryId));

            if (!string.IsNullOrEmpty(Sample) && Sample != "Include")
            {
                if (Sample == "Exclude")
                    _data = _data.Where(m => m.Sample == false);
                else if (Sample == "Only")
                    _data = _data.Where(m => m.Sample == true);
            }

            _data = _data.OrderBy(m => m.OrderDate).ThenBy(m => m.OrderNo);

            // get just one page of data
            return _data.Select(m => new PurchaseOrderAmendmentWizardViewModel
            {
                OrderDate = m.OrderDate,
                OrderNo = m.OrderNo,
                PurchaseOrderLineId = m.PurchaseOrderLineId,
                OldRate = m.OldRate,
                Rate = m.Rate,
                SupplierName = m.SupplierName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                PurchaseOrderHeaderId = m.PurchaseOrderHeaderId,
                SupplierId = m.SupplierId,
                ProductGroupId = m.ProductGroupId,
                ProductGroupName = m.ProductGroupName,
                ProductCategoryId = m.ProductCategoryId,
                ProductCategoryName = m.ProductCategoryName,
                ProdId = m.ProdId,
                Dimension1Id = m.Dimension1Id,
                Dimension2Id = m.Dimension2Id,
                UnitConversionMultiplier = m.UnitConversionMultiplier,
                Sample = m.Sample,
            });

        }

        public ActionResult ConfirmedPurchaseOrders(List<PurchaseOrderAmendmentWizardViewModel> ConfirmedList, int DocTypeId, string UserRemark)
        {
            //System.Web.HttpContext.Current.Session["RateAmendmentWizardOrders"] = ConfirmedList;
            //return Json(new { Success = "URL", Data = "/PurchaseOrderRateAmendmentWizard/Create/" + DocTypeId }, JsonRequestBehavior.AllowGet);

            if (ConfirmedList.Count() > 0 && ConfirmedList.GroupBy(m => m.SupplierId).Count() > 1)
                return Json(new { Success = false, Data = " Multiple Headers are selected. " }, JsonRequestBehavior.AllowGet);
            else if (ConfirmedList.Count() == 0 )
                return Json(new { Success = false, Data = " No Records are selected. " }, JsonRequestBehavior.AllowGet);
            else
            {

                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

                bool BeforeSave = true;
                int Serial = 1;
                Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

                try
                {
                    BeforeSave = PurchaseOrderAmendmentDocEvents.beforeWizardSaveEvent(this, new PurchaseEventArgs(0), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { Success = false, Data = message }, JsonRequestBehavior.AllowGet);
                }


                if (!BeforeSave)
                    TempData["CSEXC"] += "Failed validation before save";


                int Cnt = 0;
                int Sr = 0;

                List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
                List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
                int pk = 0;
                bool HeaderChargeEdit = false;



                PurchaseOrderSetting Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

                int? MaxLineId = 0;
                int PersonCount = 0;
                if (!Settings.CalculationId.HasValue)
                {
                    throw new Exception("Calculation not configured in Purchase order settings");
                }

                int CalculationId = Settings.CalculationId ?? 0;

                List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();


                if (ModelState.IsValid && BeforeSave && !EventException)
                {

                    PurchaseOrderAmendmentHeader pt = new PurchaseOrderAmendmentHeader();

                    //Getting Settings
                    pt.SiteId = SiteId;
                    pt.SupplierId = ConfirmedList.FirstOrDefault().SupplierId;
                    pt.DivisionId = DivisionId;
                    pt.DocTypeId = DocTypeId;
                    pt.DocDate = DateTime.Now;
                    pt.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseOrderAmendmentHeaders", pt.DocTypeId, pt.DocDate, pt.DivisionId, pt.SiteId);


                    pt.Status = (int)StatusConstants.Drafted;

                    _PurchaseOrderAmendmentHeaderService.Create(pt, User.Identity.Name);

                    var SelectedPurchaseOrders = ConfirmedList;

                    var PurchaseOrderLineIds = SelectedPurchaseOrders.Select(m => m.PurchaseOrderLineId).ToArray();

                    var PurchaseOrderBalanceRecords = (from p in db.ViewPurchaseOrderBalanceForInvoice
                                                       where PurchaseOrderLineIds.Contains(p.PurchaseOrderLineId)
                                                       select p).AsNoTracking().ToList();

                    var PurchaseOrderRecords = (from p in db.PurchaseOrderLine
                                                where PurchaseOrderLineIds.Contains(p.PurchaseOrderLineId)
                                                select p).AsNoTracking().ToList();

                    foreach (var item in SelectedPurchaseOrders)
                    {
                        PurchaseOrderLine orderline = PurchaseOrderRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault();
                        var balorderline = PurchaseOrderBalanceRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault();

                        if (item.Rate - PurchaseOrderBalanceRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault().Rate != 0)
                        {
                            PurchaseOrderRateAmendmentLine line = new PurchaseOrderRateAmendmentLine();

                            line.PurchaseOrderAmendmentHeaderId = pt.PurchaseOrderAmendmentHeaderId;
                            line.PurchaseOrderLineId = item.PurchaseOrderLineId;
                            line.Qty = PurchaseOrderBalanceRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault().BalanceQty;
                            line.AmendedRate = item.Rate;
                            line.Rate = line.AmendedRate - balorderline.Rate;
                            line.Amount = balorderline.BalanceQty * orderline.UnitConversionMultiplier * line.Rate;
                            line.PurchaseOrderRate = balorderline.Rate;
                            line.Sr = Serial++;
                            line.PurchaseOrderRateAmendmentLineId = pk;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            LineStatus.Add(line.PurchaseOrderLineId, line.Rate);

                            line.ObjectState = Model.ObjectState.Added;
                            db.PurchaseOrderRateAmendmentLine.Add(line);

                            if (Settings.CalculationId.HasValue)
                            {
                                LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseOrderRateAmendmentLineId, HeaderTableId = pt.PurchaseOrderAmendmentHeaderId, PersonID = pt.SupplierId, DealQty = orderline.DealQty });
                            }
                            pk++;
                            Cnt = Cnt + 1;

                        }
                    }

                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseRateOnAmendmentMultiple(LineStatus, pt.DocDate, ref db);


                    new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, pt.PurchaseOrderAmendmentHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseOrderAmendmentHeaderCharges", "Web.PurchaseOrderRateAmendmentLineCharges", out PersonCount, pt.DocTypeId, pt.SiteId, pt.DivisionId);

                    //Saving Charges
                    foreach (var item in LineCharges)
                    {

                        PurchaseOrderRateAmendmentLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, PurchaseOrderRateAmendmentLineCharge>(item);
                        PoLineCharge.ObjectState = Model.ObjectState.Added;
                        db.PurchaseOrderRateAmendmentLineCharge.Add(PoLineCharge);

                    }


                    //Saving Header charges
                    for (int i = 0; i < HeaderCharges.Count(); i++)
                    {

                        if (!HeaderChargeEdit)
                        {
                            PurchaseOrderAmendmentHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, PurchaseOrderAmendmentHeaderCharge>(HeaderCharges[i]);
                            POHeaderCharge.HeaderTableId = pt.PurchaseOrderAmendmentHeaderId;
                            POHeaderCharge.PersonID = pt.SupplierId;
                            POHeaderCharge.ObjectState = Model.ObjectState.Added;
                            db.PurchaseOrderAmendmentHeaderCharges.Add(POHeaderCharge);
                        }
                        else
                        {
                            var footercharge = new PurchaseOrderAmendmentHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                            footercharge.Rate = HeaderCharges[i].Rate;
                            footercharge.Amount = HeaderCharges[i].Amount;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseOrderAmendmentHeaderCharges.Add(footercharge);
                        }

                    }


                    try
                    {
                        PurchaseOrderAmendmentDocEvents.onWizardSaveEvent(this, new PurchaseEventArgs(pt.PurchaseOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
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
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        return Json(new { Success = false, Data = message }, JsonRequestBehavior.AllowGet);
                    }

                    try
                    {
                        PurchaseOrderAmendmentDocEvents.afterWizardSaveEvent(this, new PurchaseEventArgs(pt.PurchaseOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseOrderAmendmentHeaderId,
                        ActivityType = (int)ActivityTypeContants.WizardCreate,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    return Json(new { Success = "URL", Data = "/PurchaseOrderAmendmentHeader/Submit/" + pt.PurchaseOrderAmendmentHeaderId }, JsonRequestBehavior.AllowGet);

                }

                else
                    return Json(new { Success = false, Data = "ModelState is Invalid" }, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult SelectedRecords(List<PurchaseOrderAmendmentWizardViewModel> SelectedRecords)
        {
            var OrderIds = SelectedRecords.Select(m => m.PurchaseOrderLineId).ToArray();
            var RecordDetails = (from p in db.ViewPurchaseOrderBalanceForInvoice
                                 join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId
                                 join h in db.PurchaseOrderHeader on t.PurchaseOrderHeaderId equals h.PurchaseOrderHeaderId
                                 where OrderIds.Contains(p.PurchaseOrderLineId)
                                 select new PurchaseOrderAmendmentWizardViewModel
                                 {
                                     OrderDate = p.OrderDate,
                                     OrderNo = p.PurchaseOrderNo,
                                     PurchaseOrderLineId = p.PurchaseOrderLineId,
                                     OldRate = p.Rate,
                                     SupplierName = h.Supplier.Name,
                                     ProductName = t.Product.ProductName,
                                     Dimension1Name = t.Dimension1.Dimension1Name,
                                     Dimension2Name = t.Dimension2.Dimension2Name,
                                     ProductGroupName = t.Product.ProductGroup.ProductGroupName

                                 }).ToList();

            var RecordDetailList = RecordDetails.Select(m => new PurchaseOrderAmendmentWizardViewModel
            {
                SOrderDate = m.OrderDate.ToString("dd/MMM/yyyy"),
                PurchaseOrderLineId = m.PurchaseOrderLineId,
                OrderNo = m.OrderNo,
                SupplierName = m.SupplierName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                OldRate = m.OldRate,
                Rate = SelectedRecords.Where(t => t.PurchaseOrderLineId == m.PurchaseOrderLineId).FirstOrDefault().Rate,
                ProductGroupName = m.ProductGroupName
            }).ToList();

            return PartialView("_SelectedRecords", RecordDetailList);

        }




        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            PurchaseOrderAmendmentHeaderViewModel vm = new PurchaseOrderAmendmentHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(id, vm.DivisionId, vm.SiteId);
            vm.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseOrderAmendmentHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseOrderAmendmentHeaderViewModel vm)
        {
            bool BeforeSave = true;
            int Serial = 1;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            PurchaseOrderAmendmentHeader pt = AutoMapper.Mapper.Map<PurchaseOrderAmendmentHeaderViewModel, PurchaseOrderAmendmentHeader>(vm);

            try
            {
                BeforeSave = PurchaseOrderAmendmentDocEvents.beforeWizardSaveEvent(this, new PurchaseEventArgs(vm.PurchaseOrderAmendmentHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }


            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before save";


            int Cnt = 0;
            int Sr = 0;

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            bool HeaderChargeEdit = false;



            PurchaseOrderSetting Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(vm.DocTypeId, vm.DivisionId, vm.SiteId);

            int? MaxLineId = 0;
            int PersonCount = 0;
            if (!Settings.CalculationId.HasValue)
            {
                throw new Exception("Calculation not configured in Purchase order settings");
            }

            int CalculationId = Settings.CalculationId ?? 0;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                pt.Status = (int)StatusConstants.Drafted;

                _PurchaseOrderAmendmentHeaderService.Create(pt, User.Identity.Name);

                var SelectedPurchaseOrders = (List<PurchaseOrderAmendmentWizardViewModel>)System.Web.HttpContext.Current.Session["RateAmendmentWizardOrders"];

                var PurchaseOrderLineIds = SelectedPurchaseOrders.Select(m => m.PurchaseOrderLineId).ToArray();

                var PurchaseOrderBalanceRecords = (from p in db.ViewPurchaseOrderBalanceForInvoice
                                                   where PurchaseOrderLineIds.Contains(p.PurchaseOrderLineId)
                                                   select p).AsNoTracking().ToList();

                var PurchaseOrderRecords = (from p in db.PurchaseOrderLine
                                            where PurchaseOrderLineIds.Contains(p.PurchaseOrderLineId)
                                            select p).AsNoTracking().ToList();

                foreach (var item in SelectedPurchaseOrders)
                {
                    PurchaseOrderLine orderline = PurchaseOrderRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault();
                    var balorderline = PurchaseOrderBalanceRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault();

                    if (item.Rate - PurchaseOrderBalanceRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault().Rate != 0)
                    {
                        PurchaseOrderRateAmendmentLine line = new PurchaseOrderRateAmendmentLine();

                        line.PurchaseOrderAmendmentHeaderId = pt.PurchaseOrderAmendmentHeaderId;
                        line.PurchaseOrderLineId = item.PurchaseOrderLineId;
                        line.Qty = PurchaseOrderBalanceRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault().BalanceQty;
                        line.AmendedRate = item.Rate;
                        line.Rate = line.AmendedRate - balorderline.Rate;
                        line.Amount = balorderline.BalanceQty * orderline.UnitConversionMultiplier * line.Rate;
                        line.PurchaseOrderRate = balorderline.Rate;
                        line.Sr = Serial++;
                        line.PurchaseOrderRateAmendmentLineId = pk;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        LineStatus.Add(line.PurchaseOrderLineId, line.Rate);

                        line.ObjectState = Model.ObjectState.Added;
                        db.PurchaseOrderRateAmendmentLine.Add(line);

                        if (Settings.CalculationId.HasValue)
                        {
                            LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseOrderRateAmendmentLineId, HeaderTableId = pt.PurchaseOrderAmendmentHeaderId, PersonID = pt.SupplierId, DealQty = orderline.DealQty });
                        }
                        pk++;
                        Cnt = Cnt + 1;

                    }
                }

                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseRateOnAmendmentMultiple(LineStatus, pt.DocDate, ref db);


                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, pt.PurchaseOrderAmendmentHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseOrderAmendmentHeaderCharges", "Web.PurchaseOrderRateAmendmentLineCharges", out PersonCount, pt.DocTypeId, pt.SiteId, pt.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    PurchaseOrderRateAmendmentLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, PurchaseOrderRateAmendmentLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.PurchaseOrderRateAmendmentLineCharge.Add(PoLineCharge);

                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        PurchaseOrderAmendmentHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, PurchaseOrderAmendmentHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = pt.PurchaseOrderAmendmentHeaderId;
                        POHeaderCharge.PersonID = pt.SupplierId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.PurchaseOrderAmendmentHeaderCharges.Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new PurchaseOrderAmendmentHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderAmendmentHeaderCharges.Add(footercharge);
                    }

                }


                try
                {
                    PurchaseOrderAmendmentDocEvents.onWizardSaveEvent(this, new PurchaseEventArgs(pt.PurchaseOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
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
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    PrepareViewBag(vm.DocTypeId);
                    ViewBag.Mode = "Add";
                    return View("Create", vm);
                }

                try
                {
                    PurchaseOrderAmendmentDocEvents.afterWizardSaveEvent(this, new PurchaseEventArgs(pt.PurchaseOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseOrderAmendmentHeaderId,
                    ActivityType = (int)ActivityTypeContants.WizardCreate,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

                Session.Remove("RateAmendmentWizardOrders");

                return RedirectToAction("Index", "PurchaseOrderAmendmentHeader", new { id = pt.DocTypeId }).Success("Data saved Successfully");

            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        public ActionResult Filters(int DocTypeId, DateTime? FromDate, DateTime? ToDate,
            string PurchaseOrderHeaderId, string SupplierId, string ProductId, string Dimension1Id, string Dimension2Id, string ProductGroupId,
            string ProductCategoryId, decimal? Rate, decimal NewRate, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {
            PurchaseOrderRateAmendmentWizardFilterViewModel vm = new PurchaseOrderRateAmendmentWizardFilterViewModel();

            List<SelectListItem> tempSOD = new List<SelectListItem>();
            tempSOD.Add(new SelectListItem { Text = "Include Sample", Value = "Include" });
            tempSOD.Add(new SelectListItem { Text = "Exculde Sample", Value = "Exculde" });
            tempSOD.Add(new SelectListItem { Text = "Only Sample", Value = "Only" });

            ViewBag.SOD = new SelectList(tempSOD, "Value", "Text", Sample);


            vm.DocTypeId = DocTypeId;
            vm.FromDate = FromDate;
            vm.ToDate = ToDate;
            vm.PurchaseOrderHeaderId = PurchaseOrderHeaderId;
            vm.SupplierId = SupplierId;
            vm.ProductId = ProductId;
            vm.Dimension1Id = Dimension1Id;
            vm.Dimension2Id = Dimension2Id;
            vm.ProductGroupId = ProductGroupId;
            vm.ProductCategoryId = ProductCategoryId;
            vm.Rate = Rate;
            vm.NewRate = NewRate;
            vm.MultiplierGT = MultiplierGT;
            vm.MultiplierLT = MultiplierLT;
            vm.Sample = Sample;
            return PartialView("_Filters", vm);
        }


        public JsonResult GetPendingPurchaseOrdersHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var Records = new PurchaseOrderRateAmendmentLineService(_unitOfWork).GetPendingPurchaseOrderHelpList(filter, searchTerm);

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingSupplierHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var Records = new PurchaseOrderRateAmendmentLineService(_unitOfWork).GetPendingSupplierHelpList(filter, searchTerm);

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingProductHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var Records = new PurchaseOrderRateAmendmentLineService(_unitOfWork).GetPendingProductHelpList(filter, searchTerm);

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

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
