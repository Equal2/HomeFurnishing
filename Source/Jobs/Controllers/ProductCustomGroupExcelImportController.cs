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
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductCustomGroupExcelImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IProductCustomGroupHeaderService _ProductCustomGroupHeaderService;
        IProductCustomGroupLineService _ProductCustomGroupLineService;
        IProductService _ProductService;
        IUnitOfWork _unitOfWork;

        public ProductCustomGroupExcelImportController(IProductCustomGroupHeaderService ProductCustomGroupHeaderService, IProductCustomGroupLineService ProductCustomGroupLineService, ProductService ProductService, IUnitOfWork unitOfWork)
        {
            _ProductCustomGroupHeaderService = ProductCustomGroupHeaderService;
            _ProductCustomGroupLineService = ProductCustomGroupLineService;
            _ProductService = ProductService;
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductCustomGroupExcelImport()
        {
            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
            file.SaveAs(filePath);   
            var excel = new ExcelQueryFactory();
            //excel.FileName =ConfigurationManager.AppSettings["ExcelFilePath"]; //  @"C:\Users\guru\Downloads\PO.xls"; //TODO: Rempve hardcode
            excel.FileName = filePath;
            var ProductCustomGroupRecordList = from c in excel.Worksheet<ProductCustomGroupExcelViewModel>() select c;
            
            StringBuilder strProduct = new StringBuilder();

            foreach (var ProductCustomGroupRecord in ProductCustomGroupRecordList)
            {
                if (_ProductService.Find(ProductCustomGroupRecord.ProductName) == null)
                {
                    if (!strProduct.ToString().Contains("" + ProductCustomGroupRecord.ProductName))
                    {
                        strProduct.Append(" " + ProductCustomGroupRecord.ProductName + ",");
                    }                   
                }
            }
            if (!string.IsNullOrEmpty(strProduct.ToString()))
            {
                if (string.IsNullOrEmpty(strProduct.ToString()))
                {
                    ViewBag.product=null;
                }
                else
                {
                    ViewBag.product = strProduct;
                }
                return View();
            }
            else
            {
                string previousRecord = null;
                int count = 0;
                ProductCustomGroupHeader stemp = new ProductCustomGroupHeader();
                foreach (var ProductCustomGroupRecord in ProductCustomGroupRecordList)
                {                  
                    if (count == 0 || ProductCustomGroupRecord.ProductCustomGroupName != previousRecord)
                    {                   
                        ProductCustomGroupHeader s = new ProductCustomGroupHeader();
                        s.ProductCustomGroupName = ProductCustomGroupRecord.ProductCustomGroupName;
                        s.IsActive = true;
                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now; 
                        s.CreatedBy = User.Identity.Name;                     
                        s.ModifiedBy = User.Identity.Name;
                                   
                        _ProductCustomGroupHeaderService.Create(s);
                        stemp = s;
                    }

                    //Object for ProductCustomGroupLineDetail Model to Save Rate, Quantity, Product

                    Product product = _ProductService.Find(ProductCustomGroupRecord.ProductName);
                    ProductCustomGroupLine sl = new ProductCustomGroupLine();
                    sl.ProductCustomGroupHeaderId = stemp.ProductCustomGroupId;
                    sl.ProductId = product.ProductId;
                    sl.Qty = ProductCustomGroupRecord.Qty;
                    sl.CreatedDate = DateTime.Now;
                    sl.ModifiedDate = DateTime.Now;
                    sl.CreatedBy = User.Identity.Name;
                    sl.ModifiedBy = User.Identity.Name;



                    _ProductCustomGroupLineService.Create(sl);                
                    
                    count++;
                    previousRecord = ProductCustomGroupRecord.ProductCustomGroupName;

                    _unitOfWork.Save();
                }               
                
                return View("Sucess");
            }                        
        }
    }
}