using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Model.ViewModels;
using Jobs.Helpers;
using System.Data.SqlClient;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleEnquiryProductMappingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleEnquiryLineService _SaleEnquiryLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleEnquiryProductMappingController(ISaleEnquiryLineService SaleEnquiryLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleEnquiryLineService = SaleEnquiryLineService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /SaleEnquiryProductMappingMaster/

        public ActionResult Index()
        {
            IEnumerable<SaleEnquiryLineIndexViewModel> SaleEnquiryProductMapping = _SaleEnquiryLineService.GetSaleEnquiryLineListForIndex().ToList();

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            if (SaleEnquiryProductMapping.FirstOrDefault() != null)
            {
                ProductBuyerSettings ProductBuyerSettings = new ProductBuyerSettingsService(_unitOfWork).GetProductBuyerSettings(DivisionId, SiteId);
                SaleEnquiryProductMapping.FirstOrDefault().ProductBuyerSettings = Mapper.Map<ProductBuyerSettings, ProductBuyerSettingsViewModel>(ProductBuyerSettings);
            }

            return View(SaleEnquiryProductMapping);
            //return RedirectToAction("Create");
        }

        // GET: /SaleEnquiryProductMappingMaster/Create

        

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleEnquiryLineViewModel vm)
        {
            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                SaleEnquiryLine temp = new SaleEnquiryLineService(_unitOfWork).Find(vm.SaleEnquiryLineId);

                SaleEnquiryLine ExRec = Mapper.Map<SaleEnquiryLine>(temp);

                temp.ProductId = vm.ProductId;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = User.Identity.Name;
                temp.ObjectState = Model.ObjectState.Modified;
                new SaleEnquiryLineService(_unitOfWork).Update(temp);

                ProductBuyer PB = new ProductBuyerService(_unitOfWork).Find((int)vm.SaleToBuyerId, (int)vm.ProductId);
                if (PB == null)
                {


                    string BuyerSku = vm.BuyerSpecification.Replace("-", "") + "-" + vm.BuyerSpecification1 + "-" + vm.BuyerSpecification2;

                    ProductBuyer ProdBuyer = new ProductBuyer();
                    ProdBuyer.BuyerId = (int)vm.SaleToBuyerId;
                    ProdBuyer.ProductId = (int)vm.ProductId;
                    ProdBuyer.BuyerSku = vm.BuyerSku ==null ? BuyerSku : vm.BuyerSku;
                    ProdBuyer.BuyerUpcCode  = vm.BuyerUpcCode;
                    ProdBuyer.BuyerSpecification = vm.BuyerSpecification;
                    ProdBuyer.BuyerSpecification1 = vm.BuyerSpecification1;
                    ProdBuyer.BuyerSpecification2 = vm.BuyerSpecification2;
                    ProdBuyer.BuyerSpecification3 = vm.BuyerSpecification3;
                    ProdBuyer.CreatedDate = DateTime.Now;
                    ProdBuyer.CreatedBy = User.Identity.Name;
                    ProdBuyer.ModifiedDate = DateTime.Now;
                    ProdBuyer.ModifiedBy = User.Identity.Name;
                    ProdBuyer.ObjectState = Model.ObjectState.Added;
                    new ProductBuyerService(_unitOfWork).Create(ProdBuyer);
                }

                SaleEnquiryHeader SaleEnquiryHeader = new SaleEnquiryHeaderService(_unitOfWork).Find(temp.SaleEnquiryHeaderId);
                SaleOrderHeader SaleOrderHeader = new SaleOrderHeaderService(_unitOfWork).Find_ByReferenceDocId(SaleEnquiryHeader.DocTypeId, SaleEnquiryHeader.SaleEnquiryHeaderId);
                if (SaleOrderHeader == null)
                    CreateSaleOrder(temp.SaleEnquiryHeaderId);                
                else
                    CreateSaleOrderLine(temp.SaleEnquiryLineId, SaleOrderHeader.SaleOrderHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = temp,
                });
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return View("Create", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDoctypeConstants.SaleEnquiryProductMapping).DocumentTypeId,
                    DocId = temp.SaleEnquiryLineId,
                    ActivityType = (int)ActivityTypeContants.Modified,
                    xEModifications = Modifications,
                }));

                //return RedirectToAction("Index").Success("Data saved successfully");

                //var SaleEnquiryProductMapping = _SaleEnquiryLineService.GetSaleEnquiryLineListForIndex().OrderBy(m => m.SaleEnquiryLineId).OrderBy(m => m.SaleEnquiryLineId).FirstOrDefault();

                //if (SaleEnquiryProductMapping != null)
                //{
                //    return RedirectToAction("Edit", new { id = SaleEnquiryProductMapping.SaleEnquiryLineId }).Success("Data saved successfully");
                //}
                //else
                //{
                //    return RedirectToAction("Index").Success("Data saved successfully");
                //}

                int SaleEnquiryId = _SaleEnquiryLineService.NextId(vm.SaleEnquiryLineId);

                if (SaleEnquiryId > 0)
                {
                    return RedirectToAction("Edit", new { id = SaleEnquiryId }).Success("Data saved successfully");
                }
                else
                {
                    return RedirectToAction("Index").Success("Data saved successfully");
                }
                


            }
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            SaleEnquiryLine temp = _SaleEnquiryLineService.GetSaleEnquiryLine(id);
            SaleEnquiryLineExtended Extended = new SaleEnquiryLineExtendedService(_unitOfWork).Find(id);


            if (temp == null)
            {
                return HttpNotFound();
            }


            SaleEnquiryHeader H = new SaleEnquiryHeaderService(_unitOfWork).GetSaleEnquiryHeader(temp.SaleEnquiryHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleEnquiryLineViewModel s = Mapper.Map<SaleEnquiryLine, SaleEnquiryLineViewModel>(temp);

            s.BuyerSpecification = Extended.BuyerSpecification;
            s.BuyerSpecification1 = Extended.BuyerSpecification1;
            s.BuyerSpecification2 = Extended.BuyerSpecification2;
            s.BuyerSpecification3 = Extended.BuyerSpecification3;
            s.BuyerSku = Extended.BuyerSku;
            s.BuyerUpcCode = Extended.BuyerUpcCode;
            s.SaleEnquiryDocNo = H.DocNo;
            s.SaleToBuyerId = H.SaleToBuyerId;
            s.SaleEnquiryHeaderId = H.SaleEnquiryHeaderId;



            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleEnquirySettings = Mapper.Map<SaleEnquirySettings, SaleEnquirySettingsViewModel>(settings);

            ProductBuyerSettings ProductBuyerSettings = new ProductBuyerSettingsService(_unitOfWork).GetProductBuyerSettings(H.DivisionId, H.SiteId);
            s.ProductBuyerSettings = Mapper.Map<ProductBuyerSettings, ProductBuyerSettingsViewModel>(ProductBuyerSettings);

            if (temp.DealUnitId=="MT2")
            {

                string SizeinFeet = "";
               
               using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                {
                    sqlConnection.Open();
                    SqlCommand Totalf = new SqlCommand("SELECT Web.FGetSizeinFeet('" + Extended.BuyerSpecification1 + "')", sqlConnection);

                    SizeinFeet = Convert.ToString(Totalf.ExecuteScalar() == DBNull.Value ? "" : Totalf.ExecuteScalar());
                }

                ViewBag.SizeinFeet = "Feet :"+ SizeinFeet;
            }

            return View("Create", s);
        }

        // GET: /ProductMaster/Delete/5


        public void CreateSaleOrder(int SaleEnquiryHeaderId)
        {
            SaleEnquiryHeader EnquiryHeader = new SaleEnquiryHeaderService(_unitOfWork).Find(SaleEnquiryHeaderId);
            SaleEnquirySettings Settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(EnquiryHeader.DocTypeId, EnquiryHeader.DivisionId, EnquiryHeader.SiteId);

            SaleOrderHeader OrderHeader = new SaleOrderHeader();

            OrderHeader.DocTypeId = (int)Settings.SaleOrderDocTypeId;
            OrderHeader.DocDate = EnquiryHeader.DocDate;
            OrderHeader.DocNo = EnquiryHeader.DocNo;
            OrderHeader.DivisionId = EnquiryHeader.DivisionId;
            OrderHeader.SiteId = EnquiryHeader.SiteId;
            OrderHeader.BuyerOrderNo = EnquiryHeader.BuyerEnquiryNo;
            OrderHeader.SaleToBuyerId = EnquiryHeader.SaleToBuyerId;
            OrderHeader.BillToBuyerId = EnquiryHeader.BillToBuyerId;
            OrderHeader.CurrencyId = EnquiryHeader.CurrencyId;
            OrderHeader.Priority = EnquiryHeader.Priority;
            OrderHeader.UnitConversionForId = EnquiryHeader.UnitConversionForId;
            OrderHeader.ShipMethodId = EnquiryHeader.ShipMethodId;
            OrderHeader.ShipAddress = EnquiryHeader.ShipAddress;
            OrderHeader.DeliveryTermsId = EnquiryHeader.DeliveryTermsId;
            OrderHeader.Remark = EnquiryHeader.Remark;
            OrderHeader.DueDate = EnquiryHeader.DueDate;
            OrderHeader.ActualDueDate = EnquiryHeader.ActualDueDate;
            OrderHeader.Advance = EnquiryHeader.Advance;
            OrderHeader.ReferenceDocId = EnquiryHeader.SaleEnquiryHeaderId;
            OrderHeader.ReferenceDocTypeId = EnquiryHeader.DocTypeId;
            OrderHeader.CreatedDate = DateTime.Now;
            OrderHeader.ModifiedDate = DateTime.Now;
            OrderHeader.ModifiedDate = DateTime.Now;
            OrderHeader.ModifiedBy = User.Identity.Name;
            OrderHeader.Status = (int)StatusConstants.Submitted;
            OrderHeader.ReviewBy = User.Identity.Name;
            OrderHeader.ReviewCount = 1;
            //OrderHeader.LockReason = "Sale order is created for enquiry.Now you can't modify enquiry, changes can be done in sale order.";
            new SaleOrderHeaderService(_unitOfWork).Create(OrderHeader);


            IEnumerable<SaleEnquiryLine> LineList = new SaleEnquiryLineService(_unitOfWork).GetSaleEnquiryLineListForHeader(SaleEnquiryHeaderId).Where(m=>m.ProductId !=null );
            int i = 0;
            foreach (SaleEnquiryLine Line in LineList)
            {
                SaleOrderLine OrderLine = new SaleOrderLine();
                OrderLine.SaleOrderLineId = i;
                i = i - 1;
                OrderLine.DueDate = Line.DueDate;
                OrderLine.ProductId = Line.ProductId ?? 0;
                OrderLine.Specification = Line.Specification;
                OrderLine.Dimension1Id = Line.Dimension1Id;
                OrderLine.Dimension2Id = Line.Dimension2Id;
                OrderLine.Qty = Line.Qty;
                OrderLine.DealQty = Line.DealQty;
                OrderLine.DealUnitId = Line.DealUnitId;
                OrderLine.UnitConversionMultiplier = Line.UnitConversionMultiplier;
                OrderLine.Rate = Line.Rate;
                OrderLine.Amount = Line.Amount;
                OrderLine.Remark = Line.Remark;
                OrderLine.ReferenceDocTypeId = EnquiryHeader.DocTypeId;
                OrderLine.ReferenceDocLineId = Line.SaleEnquiryLineId;
                OrderLine.CreatedDate = DateTime.Now;
                OrderLine.ModifiedDate = DateTime.Now;
                OrderLine.CreatedBy = User.Identity.Name;
                OrderLine.ModifiedBy = User.Identity.Name;
                new SaleOrderLineService(_unitOfWork).Create(OrderLine);

                new SaleOrderLineStatusService(_unitOfWork).CreateLineStatus(OrderLine.SaleOrderLineId);

                Line.LockReason = "Sale order is created for enquiry.Now you can't modify enquiry, changes can be done in sale order.";
                new SaleEnquiryLineService(_unitOfWork).Update(Line);
            }
        }

        public void CreateSaleOrderLine(int SaleEnquiryLineId, int SaleOrderHeaderId)
        {
            SaleEnquiryLine Line = new SaleEnquiryLineService(_unitOfWork).Find(SaleEnquiryLineId);
            SaleEnquiryHeader EnquiryHeader = new SaleEnquiryHeaderService(_unitOfWork).Find(Line.SaleEnquiryHeaderId);
        
            SaleOrderLine OrderLine = new SaleOrderLine();
                OrderLine.SaleOrderHeaderId = SaleOrderHeaderId;
                OrderLine.DueDate = Line.DueDate;
                OrderLine.ProductId = Line.ProductId ?? 0;
                OrderLine.Specification = Line.Specification;
                OrderLine.Dimension1Id = Line.Dimension1Id;
                OrderLine.Dimension2Id = Line.Dimension2Id;
                OrderLine.Qty = Line.Qty;
                OrderLine.DealQty = Line.DealQty;
                OrderLine.DealUnitId = Line.DealUnitId;
                OrderLine.UnitConversionMultiplier = Line.UnitConversionMultiplier;
                OrderLine.Rate = Line.Rate;
                OrderLine.Amount = Line.Amount;
                OrderLine.Remark = Line.Remark;
                OrderLine.ReferenceDocTypeId = EnquiryHeader.DocTypeId;
                OrderLine.ReferenceDocLineId = Line.SaleEnquiryLineId;
                OrderLine.CreatedDate = DateTime.Now;
                OrderLine.ModifiedDate = DateTime.Now;
                OrderLine.CreatedBy = User.Identity.Name;
                OrderLine.ModifiedBy = User.Identity.Name;
                new SaleOrderLineService(_unitOfWork).Create(OrderLine);

                new SaleOrderLineStatusService(_unitOfWork).CreateLineStatus(OrderLine.SaleOrderLineId);

                Line.LockReason = "Sale order is created for enquiry.Now you can't modify enquiry, changes can be done in sale order.";
                new SaleEnquiryLineService(_unitOfWork).Update(Line);

            var PersonProductUid = (from p in db.PersonProductUid
                                    where p.GenLineId == SaleEnquiryLineId && p.GenDocTypeId == EnquiryHeader.DocTypeId && p.GenDocId == EnquiryHeader.SaleEnquiryHeaderId
                                    select p).ToList();

            if (PersonProductUid.Count() != 0)
            {
                foreach (var item2 in PersonProductUid)
                {
                    PersonProductUid PPU = new PersonProductUidService(_unitOfWork).Find(item2.PersonProductUidId);
                    PPU.SaleOrderLineId = OrderLine.SaleOrderLineId;
                    PPU.ObjectState = Model.ObjectState.Modified;
                    new PersonProductUidService(_unitOfWork).Create(PPU);
                }
            }

        }

        [HttpGet]
        public ActionResult NextPage(int id)
        {
            var nextId = _SaleEnquiryLineService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)
        {
            var nextId = _SaleEnquiryLineService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }


        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = GetCustomProductsHelpList(filter, searchTerm);
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

        public IQueryable<ComboBoxResult> GetCustomProductsHelpList(int Id, string term)
        {

            var SaleEnquiry = new SaleEnquiryHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(SaleEnquiry.DocTypeId, SaleEnquiry.DivisionId, SaleEnquiry.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            return (from Pt in db.FinishedProduct
                    join Vrs in db.ViewRugSize on Pt.ProductId equals Vrs.ProductId into ViewRugSizeTable from ViewRugSizeTab in ViewRugSizeTable.DefaultIfEmpty()
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(Pt.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(Pt.ProductId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(Pt.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : Pt.ProductName.ToLower().Contains(term.ToLower()))
                    orderby Pt.ProductName
                    select new ComboBoxResult
                    {
                        id = Pt.ProductId.ToString(),
                        text = Pt.ProductName,
                        AProp1 = "Design : " + Pt.ProductGroup.ProductGroupName,
                        AProp2 = "Size : " + ViewRugSizeTab.StandardSizeName,
                        TextProp1 = "Colour : " + Pt.Colour.ColourName,
                        TextProp2 = "Quality : " + Pt.ProductQuality.ProductQualityName
                    });
        }


        


        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocId
        {
            var SaleEnquiry = new SaleEnquiryHeaderService(_unitOfWork).Find(filter);

            var Query = new SaleEnquiryHeaderService(_unitOfWork).GetCustomPerson(SaleEnquiry.DocTypeId, searchTerm);
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
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}