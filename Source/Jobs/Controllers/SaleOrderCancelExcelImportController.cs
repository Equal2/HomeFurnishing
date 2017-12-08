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
    public class SaleOrderCancelExcelImportController : System.Web.Mvc.Controller
    {
        IExceptionHandlingService _exception;

        private ApplicationDbContext db = new ApplicationDbContext();
        
        ISaleOrderCancelHeaderService _SaleOrderCancelHeaderService;
        ISaleOrderCancelLineService _SaleOrderCancelLineService;

        ISaleOrderHeaderService _SaleOrderHeaderService;
        ISaleOrderLineService _SaleOrderLineService;

        IUnitOfWork _unitOfWork;

        public SaleOrderCancelExcelImportController( ISaleOrderCancelHeaderService SaleOrderCancelHeaderService,
                                       ISaleOrderCancelLineService SaleOrderCancelLineService,
                                       ISaleOrderHeaderService SaleOrderHeaderService,
                                       ISaleOrderLineService SaleOrderLineService, 
                                       IUnitOfWork unitOfWork)
        {
            _SaleOrderCancelHeaderService = SaleOrderCancelHeaderService;
            _SaleOrderCancelLineService = SaleOrderCancelLineService;
            _SaleOrderHeaderService = SaleOrderHeaderService;
            _SaleOrderLineService = SaleOrderLineService;
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SaleOrderCancelExcelImport()
        {
            int i = 0;
            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
            file.SaveAs(filePath);   
            var excel = new ExcelQueryFactory();
            //excel.FileName =ConfigurationManager.AppSettings["ExcelFilePath"]; //  @"C:\Users\guru\Downloads\PO.xls"; //TODO: Rempve hardcode
            excel.FileName = filePath;
            var SaleOrderCancelRecordList = from c in excel.Worksheet<SaleOrderCancelExcel>() select c;
            
            StringBuilder strSaleOrder = new StringBuilder();
            StringBuilder strSaleOrderCancelNo = new StringBuilder();

            StringBuilder strOtherValidation = new StringBuilder();

            foreach (var SaleOrderCancelRecord in SaleOrderCancelRecordList)
            {

                if (_SaleOrderCancelHeaderService.FindByDocNo(SaleOrderCancelRecord.SaleOrderCancelNo) != null)
                {
                    if (!strSaleOrderCancelNo.ToString().Contains("" + SaleOrderCancelRecord.SaleOrderCancelNo))
                    {
                        strSaleOrderCancelNo.Append(" " + SaleOrderCancelRecord.SaleOrderCancelNo + ",");
                    }
                }

                if (_SaleOrderHeaderService.FindByDocNo(SaleOrderCancelRecord.SaleOrderNo) == null)
                {
                    if (!strSaleOrder.ToString().Contains("" + SaleOrderCancelRecord.SaleOrderNo))
                    {
                        strSaleOrder.Append(" " + SaleOrderCancelRecord.SaleOrderNo + ",");
                    }                   
                }

                
                //SaleOrderLine pol = _SaleOrderLineService.GetSaleOrderLineForSaleOrderAndProduct(SaleOrderCancelRecord.SaleOrderNo, SaleOrderCancelRecord.Product);
                decimal balqty = _SaleOrderCancelLineService.GetSaleOrderBalanceQty(SaleOrderCancelRecord.SaleOrderNo, SaleOrderCancelRecord.Product);
                if (balqty != null )
                {
                    i = i+1;
                    if (balqty < SaleOrderCancelRecord.Quantity)
                    {
                        strOtherValidation.Append("SaleOrder Cancel qty is greater than Order qty for Sale Order " + SaleOrderCancelRecord.SaleOrderNo + " and Product " + SaleOrderCancelRecord.Product + "." + Environment.NewLine);
                    }
                }
            }


            if (!string.IsNullOrEmpty(strSaleOrderCancelNo.ToString()) || !string.IsNullOrEmpty(strSaleOrder.ToString()) || !string.IsNullOrEmpty(strOtherValidation.ToString()))
            {
                if(string.IsNullOrEmpty(strSaleOrderCancelNo.ToString()))
                {
                    ViewBag.SaleOrderCancelNo = null;
                }
                else
                {
                    ViewBag.SaleOrderCancelNo = strSaleOrderCancelNo;
                }

                if (string.IsNullOrEmpty(strSaleOrder.ToString()))
                {
                    ViewBag.SaleOrder = null;
                }
                else
                {
                    ViewBag.SaleOrder = strSaleOrder;
                }

                if (string.IsNullOrEmpty(strOtherValidation.ToString()))
                {
                    ViewBag.OtherValidation = null;
                }
                else
                {
                    ViewBag.OtherValidation = strOtherValidation;
                }
                
                return View();
            }
            else
            {
                string previousRecord = null;
                int count = 0;

                SaleOrderCancelHeader Temp_SaleOrderCancelHeader = new SaleOrderCancelHeader();

                foreach (var SaleOrderCancelRecord in SaleOrderCancelRecordList)
                {
                    //Object for SaleOrderCancelLine Model
                    SaleOrderCancelLine sl = new SaleOrderCancelLine();
                    sl.CreatedBy = User.Identity.Name;
                    sl.CreatedDate = DateTime.Now;
                    sl.ModifiedBy = User.Identity.Name;
                    sl.ModifiedDate = DateTime.Now;
                    sl.SaleOrderCancelHeaderId = Temp_SaleOrderCancelHeader.SaleOrderCancelHeaderId;
                    sl.SaleOrderLineId = _SaleOrderCancelLineService.GetSaleOrderLineIdForProductandSaleOrderDocNo(SaleOrderCancelRecord.SaleOrderNo, SaleOrderCancelRecord.Product);
                    sl.Qty = SaleOrderCancelRecord.Quantity;


                    if (count == 0 || SaleOrderCancelRecord.SaleOrderCancelNo != previousRecord)
                    {                   
                        //Object for SaleOrderCancelHeader Model
                        
                        SaleOrderCancelHeader sh = new SaleOrderCancelHeader();
                        sh.CreatedBy = User.Identity.Name;
                        sh.CreatedDate = DateTime.Now;
                        sh.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                        sh.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                        sh.DocDate = SaleOrderCancelRecord.CancelDate;
                        sh.DocNo = SaleOrderCancelRecord.SaleOrderCancelNo;
                        sh.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrderCancel).DocumentTypeId;
                        sh.ModifiedBy = User.Identity.Name;
                        sh.ModifiedDate = DateTime.Now;
                        sh.Remark = SaleOrderCancelRecord.Remark;
                        sh.ReasonId = new ReasonService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrderCancel).ReasonId;

                        int BuyerId = (from L in db.SaleOrderLine
                                       join H in db.SaleOrderHeader on L.SaleOrderHeaderId equals H.SaleOrderHeaderId into SaleOrderHeaderTable
                                       from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
                                       where L.SaleOrderLineId == sl.SaleOrderLineId
                                       select new { BuyerId = SaleOrderHeaderTab.SaleToBuyerId }).FirstOrDefault().BuyerId;
                        sh.BuyerId = BuyerId;


                        _SaleOrderCancelHeaderService.Create(sh);
                        Temp_SaleOrderCancelHeader = sh;
                    }


                    _SaleOrderCancelLineService.Create(sl);

                    count++;
                    previousRecord = SaleOrderCancelRecord.SaleOrderCancelNo;

                    //Saving DB::
                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        throw ex;
                    }
                    
                }
                
                return View("Sucess");
            }
        }
    }
}