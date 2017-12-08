using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using System.Xml.Linq;
using System.Xml;
using System.Data.SqlClient;
using System.Configuration;

namespace Jobs.Areas.Rug.Controllers
{
    public class ExcessInventoryAnalysisController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IExceptionHandlingService _exception;
        public ExcessInventoryAnalysisController()
        {
            _exception = new ExceptionHandlingService();
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult GetProductTransactionDetail(int? ProductId)
        {
            if (ProductId == 0 || ProductId == null)
            {
                ModelState.AddModelError("", "Please select Product.");
                return View("Index");
            }

            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);

            IEnumerable<ProductTransactionDetail> ProductTransactionDetail = db.Database.SqlQuery<ProductTransactionDetail>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ExcessInventoryStatusAnalysisProductWise_Step1 @ProductId", SqlParameterProductId).ToList();

            ViewBag.ProductId = ProductId;
            ViewBag.ProductName = (from P in db.Product where P.ProductId == ProductId select P).FirstOrDefault().ProductName;

            return PartialView("ExcessInventoryAnalysis", ProductTransactionDetail);
        }


        public ActionResult GetProductTransactionDetailDateWise(int? ProductId, string TransactionType)
        {
            if (ProductId == 0 || ProductId == null)
            {
                ModelState.AddModelError("", "Please select Product.");
                return View("Index");
            }

            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterTransactionType = new SqlParameter("@TransactionType", TransactionType);

            IEnumerable<ProductTransactionDetailDateWise> ProductTransactionDetailDateWise = db.Database.SqlQuery<ProductTransactionDetailDateWise>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ExcessInventoryStatusAnalysisProductWise_Step2 @ProductId,@TransactionType", SqlParameterProductId, SqlParameterTransactionType).ToList();

            ViewBag.ProductName = (from P in db.Product where P.ProductId == ProductId select P).FirstOrDefault().ProductName;
            ViewBag.TransactionType = TransactionType;

            return PartialView("ProductTransactionDetailDateWise", ProductTransactionDetailDateWise);
        }

    }

    public class ProductTransactionDetail
    {
        public string TransactionType { get; set; }
        public Decimal Qty { get; set; }
    }

    public class ProductTransactionDetailDateWise
    {
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public Decimal Qty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
