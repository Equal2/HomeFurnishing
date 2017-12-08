using System;
using System.Linq;
using System.Web.Mvc;
using Data.Models;
using Service;
using Core;
using Model.Models;
using System.Configuration;
using System.Text;
using Data.Infrastructure;
using Core.Common;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleOrderExcelImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IProductService _ProductService;
        IPersonService  _PersonService;
        ISaleOrderHeaderService _SaleOrderHeaderService;
        ISaleOrderLineService _SaleOrderLineService;
        IUnitOfWork _unitOfWork;
        IBuyerService _BuyerService;
        ICurrencyService _currencyService;
        IShipMethodService _ShipMethodService;
        IDeliveryTermsService _DeliveryTermsService;

        public SaleOrderExcelImportController(IProductService productService, 
                                       IPersonService personService, 
                                       ISaleOrderHeaderService SaleOrderHeaderService,
                                       ISaleOrderLineService SaleOrderLineService, 
                                       IUnitOfWork unitOfWork,
                                        IBuyerService buyer,
                                        ICurrencyService curr,
                                        IShipMethodService shipmethod,
                                        IDeliveryTermsService delterms)
        {
            _DeliveryTermsService = delterms;
            _ShipMethodService = shipmethod;
            _currencyService = curr;
            _BuyerService = buyer;
            _ProductService = productService;
            _PersonService = personService;
            _SaleOrderHeaderService = SaleOrderHeaderService;
            _SaleOrderLineService = SaleOrderLineService;
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]
        public ActionResult Index(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            return View();
        }

        public ActionResult SaleOrderExcelImport(int id)
        {
            int BuyerId = 0;
            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
            file.SaveAs(filePath);   
            var excel = new ExcelQueryFactory();
            //excel.FileName =ConfigurationManager.AppSettings["ExcelFilePath"]; //  @"C:\Users\guru\Downloads\PO.xls"; //TODO: Rempve hardcode
            excel.FileName = filePath;
            var SaleOrderRecordList = from c in excel.Worksheet<SaleOrderExcel>()  select c;
            
            StringBuilder strSku = new StringBuilder();
            StringBuilder strSaleToBuyer = new StringBuilder();
            StringBuilder strBillToBuyer = new StringBuilder();
            StringBuilder strCurrency = new StringBuilder();
            StringBuilder strShipMethod = new StringBuilder();
            StringBuilder strDeliveryTerms = new StringBuilder();
            StringBuilder strPriority = new StringBuilder();
            StringBuilder strOrderNo = new StringBuilder();

            foreach (var SaleOrderRecord in SaleOrderRecordList)
            {

                if (_SaleOrderHeaderService.FindByDocNo(SaleOrderRecord.OrderNumber) != null)
                {
                    if (!strOrderNo.ToString().Contains("" + SaleOrderRecord.OrderNumber))
                    {
                        strOrderNo.Append(" " + SaleOrderRecord.OrderNumber+",");
                    }
                }


                if (_ProductService.Find(SaleOrderRecord.Product) == null)
                {
                    if (!strSku.ToString().Contains("" + SaleOrderRecord.Product))
                    {
                        strSku.Append(" " + SaleOrderRecord.Product+",");
                    }                   
                }
                if (SaleOrderRecord.SaleToBuyer!=null)
                { 
                if (_BuyerService.GetBuyerByName(SaleOrderRecord.SaleToBuyer) == null)
                {
                    if (!strSaleToBuyer.ToString().Contains("" + SaleOrderRecord.SaleToBuyer))
                    {
                        strSaleToBuyer.Append(" " + SaleOrderRecord.SaleToBuyer + ",");
                    }
                }
                }
                if (SaleOrderRecord.BillToBuyer!=null)
                { 
                if (_BuyerService.GetBuyerByName(SaleOrderRecord.BillToBuyer) == null)
                {
                    if (!strBillToBuyer.ToString().Contains("" + SaleOrderRecord.BillToBuyer))
                    {
                        strBillToBuyer.Append(" " + SaleOrderRecord.BillToBuyer + ",");
                    }
                }
                }
                if (SaleOrderRecord.Currency!=null)
                { 
                if (_currencyService.GetCurrencyByName(SaleOrderRecord.Currency) == null)
                {
                    if (!strCurrency.ToString().Contains("" + SaleOrderRecord.Currency))
                    {
                        strCurrency.Append(" " + SaleOrderRecord.Currency + ",");
                    }
                }
                }
                if (SaleOrderRecord.ShipMethod!=null)
                { 
                if (_ShipMethodService.GetShipMethodByName(SaleOrderRecord.ShipMethod) == null)
                {
                    if (!strShipMethod.ToString().Contains("" + SaleOrderRecord.ShipMethod))
                    {
                        strShipMethod.Append(" " + SaleOrderRecord.ShipMethod + ",");
                    }
                }
                }
                if (SaleOrderRecord.DeliveryTerms!=null)
                { 
                if (_DeliveryTermsService.GetDeliveryTermsByName(SaleOrderRecord.DeliveryTerms) == null)
                {
                    if (!strDeliveryTerms.ToString().Contains("" + SaleOrderRecord.DeliveryTerms))
                    {
                        strDeliveryTerms.Append(" " + SaleOrderRecord.DeliveryTerms + ",");
                    }
                }
                }
                if (SaleOrderRecord.Priority!=null)
                { 
                if(Enum.IsDefined(typeof(SaleOrderPriority),SaleOrderRecord.Priority)==false)
                {
                    if (!strPriority.ToString().Contains("" + SaleOrderRecord.Priority))
                    {
                        strPriority.Append(" " + SaleOrderRecord.Priority + ",");
                    }
                }
                }
            }
            if (!string.IsNullOrEmpty(strSku.ToString()) 
                || !string.IsNullOrEmpty(strOrderNo.ToString()) 
                || !string.IsNullOrEmpty(strSaleToBuyer.ToString())
                || !string.IsNullOrEmpty(strBillToBuyer.ToString())
                || !string.IsNullOrEmpty(strCurrency.ToString())
                || !string.IsNullOrEmpty(strShipMethod.ToString())
                || !string.IsNullOrEmpty(strDeliveryTerms.ToString())
                || !string.IsNullOrEmpty(strPriority.ToString()))
            {
                if(string.IsNullOrEmpty(strOrderNo.ToString()))
                {
                    ViewBag.orderno = null;
                }
                else
                {
                    ViewBag.orderno = strOrderNo;
                }
                if(string.IsNullOrEmpty(strSku.ToString()))
                {
                    ViewBag.product=null;
                }
                else
                {
                    ViewBag.product = strSku;
                }
                if (string.IsNullOrEmpty(strSaleToBuyer.ToString()))
                {
                    ViewBag.SaleToBuyer = null;
                }
                else
                {
                    ViewBag.SaleToBuyer = strSaleToBuyer;
                }
                if (string.IsNullOrEmpty(strBillToBuyer.ToString()))
                {
                    ViewBag.BillToBuyer = null;
                }
                else
                {
                    ViewBag.BillToBuyer = strBillToBuyer;
                }
                if (string.IsNullOrEmpty(strCurrency.ToString()))
                {
                    ViewBag.Currency = null;
                }
                else
                {
                    ViewBag.Currency = strCurrency;
                }
                if (string.IsNullOrEmpty(strShipMethod.ToString()))
                {
                    ViewBag.ShipMethod = null;
                }
                else
                {
                    ViewBag.ShipMethod = strShipMethod;
                }
                if (string.IsNullOrEmpty(strDeliveryTerms.ToString()))
                {
                    ViewBag.DeliveryTerms = null;
                }
                else
                {
                    ViewBag.DeliveryTerms = strDeliveryTerms;
                }
                if (string.IsNullOrEmpty(strPriority.ToString()))
                {
                    ViewBag.Priority = null;
                }
                else
                {
                    ViewBag.Priority = strPriority;
                }
                return View();
            }
            else
            {
                string previousRecord = null;
                int count = 0;
                SaleOrderHeader stemp = new SaleOrderHeader();
                int LinePk = 0;
                foreach (var SaleOrderRecord in SaleOrderRecordList)
                {
                    if (count == 0 || SaleOrderRecord.OrderNumber != previousRecord)
                    {                   
                        //Object for SaleorderHeader Model to Save Order Num,Order Date, Ship Date and Supplier
                        SaleOrderHeader s = new SaleOrderHeader();

                        if (SaleOrderRecord.ShipAddress != null)
                            s.ShipAddress = SaleOrderRecord.ShipAddress;

                        s.DocDate = SaleOrderRecord.OrderDate;
                        s.DocNo = SaleOrderRecord.OrderNumber;
                        s.DueDate = SaleOrderRecord.DueDate;
                        s.ActualDueDate  = SaleOrderRecord.DueDate;
                        s.BuyerOrderNo = SaleOrderRecord.BuyerOrderNo;
                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now; 
                        s.CreatedBy = User.Identity.Name;                     
                            

                        s.ModifiedBy = User.Identity.Name;
                        s.SaleToBuyerId = _BuyerService.GetBuyerByName(SaleOrderRecord.SaleToBuyer).PersonID;
                        if (SaleOrderRecord.BillToBuyer != null)
                            s.BillToBuyerId = _BuyerService.GetBuyerByName(SaleOrderRecord.BillToBuyer).PersonID;
                        else
                        s.BillToBuyerId = s.SaleToBuyerId;
                        BuyerId = s.SaleToBuyerId;

                            s.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                        s.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                        SaleOrderSettings temp = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(id, s.DivisionId, s.SiteId);

                        if (temp == null)
                            throw new Exception("Sale order settings is not configured");

                        if (SaleOrderRecord.Currency != null)
                            s.CurrencyId = _currencyService.GetCurrencyByName(SaleOrderRecord.Currency).ID;
                        else
                            s.CurrencyId = temp.CurrencyId;

                        if (SaleOrderRecord.ShipMethod == null)
                            s.ShipMethodId = temp.ShipMethodId;
                        else
                            s.ShipMethodId = _ShipMethodService.GetShipMethodByName(SaleOrderRecord.ShipMethod).ShipMethodId;

                        if (SaleOrderRecord.DeliveryTerms == null)
                            s.DeliveryTermsId = temp.DeliveryTermsId;
                        else
                            s.DeliveryTermsId = _DeliveryTermsService.GetDeliveryTermsByName(SaleOrderRecord.DeliveryTerms).DeliveryTermsId;

                        if (SaleOrderRecord.Priority == null)
                            s.Priority = temp.Priority;
                        else if (SaleOrderRecord.Priority == "Low")
                            s.Priority = (int)(SaleOrderPriority.Low);                        
                        else if (SaleOrderRecord.Priority == "High")
                            s.Priority = (int)(SaleOrderPriority.High);
                        else 
                            s.Priority = (int)(SaleOrderPriority.Normal);

                        if(temp!=null)
                        {
                            s.DocTypeId = temp.DocTypeId;
                            s.UnitConversionForId = temp.UnitConversionForId;
                        }
                                   
                        _SaleOrderHeaderService.Create(s);
                        stemp = s;
                    }

                    //Object for SaleOrderLineDetail Model to Save Rate, Quantity, Product

                    Product product = _ProductService.Find(SaleOrderRecord.Product);
                    SaleOrderLine sl = new SaleOrderLine();
                    sl.SaleOrderHeaderId = stemp.SaleOrderHeaderId;
                    sl.Rate = (decimal)SaleOrderRecord.Rate;
                    sl.Qty = SaleOrderRecord.Quantity;
                    sl.ProductId = product.ProductId;
                    sl.Specification = product.ProductSpecification;
                    sl.DueDate = SaleOrderRecord.DueDate;
                    sl.DealUnitId = product.UnitId;                            
                    sl.CreatedDate = DateTime.Now;
                    sl.ModifiedDate = DateTime.Now;
                    sl.DealQty = SaleOrderRecord.Quantity;
                    sl.CreatedBy = User.Identity.Name;
                    sl.ModifiedBy = User.Identity.Name;
                    sl.Amount = sl.Rate * sl.Qty;
                    sl.SaleOrderLineId = LinePk++;

                    if (SaleOrderRecord.BuyerUpcCode != "" && SaleOrderRecord.BuyerUpcCode != null)
                    {
                        ProductBuyer productbuyer = new ProductBuyerService(_unitOfWork).Find(BuyerId, sl.ProductId);
                        if (productbuyer != null)
                        {
                            productbuyer.BuyerUpcCode = SaleOrderRecord.BuyerUpcCode;
                            new ProductBuyerService(_unitOfWork).Update(productbuyer);
                        }
                        else
                        {
                            ProductBuyer Temp = new ProductBuyer();
                            Temp.BuyerId = BuyerId;
                            Temp.ProductId = sl.ProductId;
                            Temp.BuyerUpcCode = SaleOrderRecord.BuyerUpcCode;
                            Temp.CreatedDate = DateTime.Now;
                            Temp.ModifiedDate = DateTime.Now;
                            Temp.CreatedBy = User.Identity.Name;
                            Temp.ModifiedBy = User.Identity.Name;
                            Temp.ObjectState = Model.ObjectState.Added;
                            new ProductBuyerService(_unitOfWork).Create(Temp);
                        }
                    }


                    _SaleOrderLineService.Create(sl);
                    new SaleOrderLineStatusService(_unitOfWork).CreateLineStatus(sl.SaleOrderLineId);
                    
                    count++;
                    previousRecord = SaleOrderRecord.OrderNumber;

                    _unitOfWork.Save();
                }

                ViewBag.id = id;
                return View("Sucess");
            }                        
        }
    }
}