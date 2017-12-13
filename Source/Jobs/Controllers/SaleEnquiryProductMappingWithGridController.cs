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
    public class SaleEnquiryProductMappingWithGridController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleEnquiryLineService _SaleEnquiryLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleEnquiryProductMappingWithGridController(ISaleEnquiryLineService SaleEnquiryLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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
        // GET: /SaleEnquiryProductMappingWithGridMaster/

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult PendingMappingFill()
        {
            IEnumerable<SaleEnquiryLineIndexViewModel> SaleEnquiryProductMappingWithGrid = _SaleEnquiryLineService.GetSaleEnquiryLineListForIndex().ToList();

            if (SaleEnquiryProductMappingWithGrid != null)
            {
                List<SaleEnquiryLineIndexViewModel_Temp> SaleEnquiryProductMappingWithGrid_TempList = new List<SaleEnquiryLineIndexViewModel_Temp>();

                foreach(var temp in SaleEnquiryProductMappingWithGrid)
                {
                    SaleEnquiryLineIndexViewModel_Temp obj = new SaleEnquiryLineIndexViewModel_Temp(); 
                    obj.SaleEnquiryLineId = temp.SaleEnquiryLineId;
                    obj.SaleEnquiryHeaderDocNo = temp.SaleEnquiryHeaderDocNo;
                    obj.SaleEnquiryHeaderDocDate = temp.SaleEnquiryHeaderDocDate.ToString("dd/MMM/yyyy");
                    obj.SaleToBuyerId = temp.SaleToBuyerId;
                    obj.SaleToBuyerName = temp.SaleToBuyerName;
                    obj.BuyerSpecification = temp.BuyerSpecification;
                    obj.BuyerSpecification1 = temp.BuyerSpecification1;
                    obj.BuyerSpecification2 = temp.BuyerSpecification2;
                    obj.BuyerSpecification3 = temp.BuyerSpecification3;

                    SaleEnquiryProductMappingWithGrid_TempList.Add(obj);
                }

                JsonResult json = Json(new { Success = true, Data = SaleEnquiryProductMappingWithGrid_TempList }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductBuyerSettings()
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            
            ProductBuyerSettings ProductBuyerSettings = new ProductBuyerSettingsService(_unitOfWork).GetProductBuyerSettings(DivisionId, SiteId);

            return Json(new
            {
                BuyerSpecificationCaption = ProductBuyerSettings.BuyerSpecificationDisplayName,
                BuyerSpecification1Caption = ProductBuyerSettings.BuyerSpecification1DisplayName,
                BuyerSpecification2Caption = ProductBuyerSettings.BuyerSpecification2DisplayName,
                BuyerSpecification3Caption = ProductBuyerSettings.BuyerSpecification3DisplayName,
            });
        }

        public JsonResult GetCustomProductHelpList()
        {
            int DocTypeId = (from H in db.SaleEnquiryHeader select H).FirstOrDefault().DocTypeId;
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(DocTypeId, DivisionId, SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            var ProductList =  (from Pt in db.FinishedProduct
                    join Vrs in db.ViewRugSize on Pt.ProductId equals Vrs.ProductId into ViewRugSizeTable
                    from ViewRugSizeTab in ViewRugSizeTable.DefaultIfEmpty()
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(Pt.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(Pt.ProductId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(Pt.ProductGroupId.ToString()))
                    orderby Pt.ProductName
                    select new ComboBoxResult
                    {
                        id = Pt.ProductName.ToString(),
                        text = Pt.ProductName,
                        AProp1 = "Design : " + Pt.ProductGroup.ProductGroupName,
                        AProp2 = "Size : " + ViewRugSizeTab.StandardSizeName,
                        TextProp1 = "Colour : " + Pt.Colour.ColourName,
                        TextProp2 = "Quality : " + Pt.ProductQuality.ProductQualityName
                    }).ToList();

            JsonResult json = Json(new { Success = true, Data = ProductList }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        public void Post(int SaleEnquiryLineId, string ProductName)
        {
            SaleEnquiryLine Line = new SaleEnquiryLineService(_unitOfWork).Find(SaleEnquiryLineId);
            SaleEnquiryLineExtended LineExtended = new SaleEnquiryLineExtendedService(_unitOfWork).Find(SaleEnquiryLineId);

            Product Product = new ProductService(_unitOfWork).Find(ProductName);

            if (Product != null)
            {
                Line.ProductId = Product.ProductId;
                Line.ModifiedDate = DateTime.Now;
                Line.ModifiedBy = User.Identity.Name;
                Line.ObjectState = Model.ObjectState.Modified;
                new SaleEnquiryLineService(_unitOfWork).Update(Line);

                SaleEnquiryHeader Header = new SaleEnquiryHeaderService(_unitOfWork).Find(Line.SaleEnquiryHeaderId);

                ProductBuyer PB = new ProductBuyerService(_unitOfWork).Find((int)Header.SaleToBuyerId, Product.ProductId);
                if (PB == null)
                {
                    string BuyerSku = LineExtended.BuyerSpecification.Replace("-", "") + "-" + LineExtended.BuyerSpecification1 + "-" + LineExtended.BuyerSpecification2;

                    ProductBuyer ProdBuyer = new ProductBuyer();
                    ProdBuyer.BuyerId = (int)Header.SaleToBuyerId;
                    ProdBuyer.ProductId = Product.ProductId;
                    ProdBuyer.BuyerSku = LineExtended.BuyerSku == null ? BuyerSku : LineExtended.BuyerSku;
                    ProdBuyer.BuyerUpcCode = LineExtended.BuyerUpcCode;
                    ProdBuyer.BuyerSpecification = LineExtended.BuyerSpecification;
                    ProdBuyer.BuyerSpecification1 = LineExtended.BuyerSpecification1;
                    ProdBuyer.BuyerSpecification2 = LineExtended.BuyerSpecification2;
                    ProdBuyer.BuyerSpecification3 = LineExtended.BuyerSpecification3;
                    ProdBuyer.CreatedDate = DateTime.Now;
                    ProdBuyer.CreatedBy = User.Identity.Name;
                    ProdBuyer.ModifiedDate = DateTime.Now;
                    ProdBuyer.ModifiedBy = User.Identity.Name;
                    ProdBuyer.ObjectState = Model.ObjectState.Added;
                    new ProductBuyerService(_unitOfWork).Create(ProdBuyer);
                }

                SaleOrderHeader SaleOrderHeader = new SaleOrderHeaderService(_unitOfWork).Find_ByReferenceDocId(Header.DocTypeId, Header.SaleEnquiryHeaderId);
                if (SaleOrderHeader == null)
                    CreateSaleOrder(Header.SaleEnquiryHeaderId);
                else
                    CreateSaleOrderLine(Line.SaleEnquiryLineId, SaleOrderHeader.SaleOrderHeaderId);

            
                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                }
            }
        }


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


            IEnumerable<SaleEnquiryLine> LineList = new SaleEnquiryLineService(_unitOfWork).GetSaleEnquiryLineListForHeader(SaleEnquiryHeaderId).Where(m => m.ProductId != null);
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


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class SaleEnquiryLineIndexViewModel_Temp : SaleEnquiryLineViewModel
    {
        public string SaleEnquiryHeaderDocNo { get; set; }
        public string SaleEnquiryHeaderDocDate { get; set; }
        public int ProgressPerc { get; set; }
        public int unitDecimalPlaces { get; set; }


    }
}