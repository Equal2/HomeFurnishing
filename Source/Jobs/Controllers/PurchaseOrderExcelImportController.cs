using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data;
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
    public class PurchaseOrderExcelImportController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        IProductService _ProductService;
        ISupplierService _SupplierService;
        IPurchaseOrderHeaderService _PurchaseOrderHeaderService;
        IPurchaseOrderLineService _PurchaseOrderLineService;
        IUnitOfWork _unitOfWork;

        public PurchaseOrderExcelImportController(IProductService productService, 
                                       ISupplierService suppserv, 
                                       IPurchaseOrderHeaderService purchaseOrderHeaderService,
                                       IPurchaseOrderLineService purchaseOrderLineService, 
                                       IUnitOfWork unitOfWork)
        {
            _ProductService = productService;
            _SupplierService = suppserv;
            _PurchaseOrderHeaderService = purchaseOrderHeaderService;
            _PurchaseOrderLineService = purchaseOrderLineService;
            _unitOfWork = unitOfWork;
        }




        //  [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PurchaseOrderExcelImport()
        {
            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);
            file.SaveAs(filePath);   
            var excel = new ExcelQueryFactory();
            //excel.FileName =ConfigurationManager.AppSettings["ExcelFilePath"]; //  @"C:\Users\guru\Downloads\PO.xls"; //TODO: Rempve hardcode
            excel.FileName = filePath;
            var purchaseOrderRecordList = from c in excel.Worksheet<PurchaseOrderExcel>() orderby c.OrderNumber  select c;
            int divisionid = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int siteid = (int)System.Web.HttpContext.Current.Session["SiteId"];
            StringBuilder strSku = new StringBuilder();
            StringBuilder strSupplier = new StringBuilder();
            StringBuilder strOrderNo = new StringBuilder();
            foreach (var purchaseOrderRecord in purchaseOrderRecordList)
            {

                if (_PurchaseOrderHeaderService.Find(purchaseOrderRecord.OrderNumber) != null)
                {
                    if (!strOrderNo.ToString().Contains("" + purchaseOrderRecord.OrderNumber))
                    {
                        strOrderNo.Append(" " + purchaseOrderRecord.OrderNumber+",");
                    }
                }


                if (_ProductService.Find(purchaseOrderRecord.Product) == null)
                {
                    if (!strSku.ToString().Contains("" + purchaseOrderRecord.Product))
                    {
                        strSku.Append(" " + purchaseOrderRecord.Product+",");
                    }                   
                }

                if (_SupplierService.GetSupplierByName(purchaseOrderRecord.Supplier) == null)
                {
                    if (!strSupplier.ToString().Contains("" + purchaseOrderRecord.Supplier))
                    {
                        strSupplier.Append(" " + purchaseOrderRecord.Supplier+",");
                    }
                }
            }
            if (!string.IsNullOrEmpty(strSku.ToString()) || !string.IsNullOrEmpty(strOrderNo.ToString()) || !string.IsNullOrEmpty(strSupplier.ToString()))
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
                if(string.IsNullOrEmpty(strSupplier.ToString()))
                {
                    ViewBag.supplier = null;
                }
                else
                {
                    ViewBag.supplier = strSupplier;
                }
                
                
                return View();
            }
            else
            {
                string previousRecord = null;
                int count = 0;
                PurchaseOrderHeader p = new PurchaseOrderHeader();
                foreach (var purchaseOrderRecord in purchaseOrderRecordList)
                {

                   

                    if (count == 0 || purchaseOrderRecord.OrderNumber != previousRecord)
                    {
                   
                        //Object for PurchaseorderHeader Model to Save Order Num,Order Date, Ship Date and Supplier

                        PurchaseOrderHeader pd = new PurchaseOrderHeader();
                        pd.CreatedDate = DateTime.Now;
                        pd.ModifiedDate = DateTime.Now;
                        //pd.DueDate = purchaseOrderRecord.ShipDate;
                        pd.DocDate = purchaseOrderRecord.OrderDate;
                        pd.DocNo = purchaseOrderRecord.OrderNumber;
                        pd.SupplierId = _SupplierService.GetSupplierByName(purchaseOrderRecord.Supplier).PersonId;
                        pd.CreatedBy = User.Identity.Name;
                        pd.ActualDueDate = pd.DueDate;
                        pd.ModifiedBy = User.Identity.Name;
                        pd.DocTypeId = new DocumentTypeService(_unitOfWork ).FindByName(TransactionDocCategoryConstants.PurchaseOrder).DocumentTypeId ;
                        pd.DivisionId = divisionid;
                        pd.SiteId = siteid;
                        pd.ShipMethodId = 1;
                        pd.DeliveryTermsId = 1;
                        //pd.ShipAddress = new PersonAddressService(_unitOfWork).GetShipAddressByPersonId(pd.SupplierId).PersonAddressID;


                        _PurchaseOrderHeaderService.Create(pd);
                        p = pd;
                    }




                    //Object for PurchaseOrderLineDetail Model to Save Rate, Quantity, Product

                    Product product = _ProductService.Find(purchaseOrderRecord.Product);
                    PurchaseOrderLine pl = new PurchaseOrderLine();
                    pl.PurchaseOrderHeaderId = p.PurchaseOrderHeaderId;
                    pl.Rate = (decimal)purchaseOrderRecord.Rate;
                    pl.Qty = purchaseOrderRecord.Quantity;
                    pl.ProductId = product.ProductId;
                    pl.DealUnitId = product.UnitId;                                                                                
                    pl.CreatedDate = DateTime.Now;
                    pl.ModifiedDate = DateTime.Now;
                    pl.DealQty = purchaseOrderRecord.Quantity;
                    pl.CreatedBy = User.Identity.Name;
                    pl.ModifiedBy = User.Identity.Name;
                    pl.Amount = pl.Rate * pl.Qty;


                    _PurchaseOrderLineService.Create(pl);
                  
                    

                    count++;
                    previousRecord = purchaseOrderRecord.OrderNumber;


                    _unitOfWork.Save();
                }


                


                return View("Sucess");
            }

            
        }
    }
}