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
using System.Data;
using System.Data.SqlClient;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductBuyersExcelImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IProductService _ProductService;
        IProductBuyerService _ProductBuyerService;
        IUnitOfWork _unitOfWork;
        IBuyerService _BuyerService;


        public ProductBuyersExcelImportController(IProductService productService,                                       
                                       IUnitOfWork unitOfWork,
                                       IProductBuyerService productBuyerService, 
                                       IBuyerService buyer)
        {

            _BuyerService = buyer;
            _ProductService = productService;
            _ProductBuyerService = productBuyerService;
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]
        public ActionResult Index()
        {
          
            return View();
        }

        //public ActionResult ProductBuyersExcelImport(int id)
        //{
            
        //    var file = Request.Files[0];
        //    string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
        //    file.SaveAs(filePath);   
        //    var excel = new ExcelQueryFactory();            
        //    excel.FileName = filePath;
        //    var ProductBuyerRecordList = from c in excel.Worksheet<ProductBuyerExcel>()  select c;
            
        //    StringBuilder strProduct = new StringBuilder();
        //    StringBuilder strBuyer = new StringBuilder();


        //    foreach (var ProductBuyerRecord in ProductBuyerRecordList)
        //    {

        //        if (_ProductService.FindProduct(ProductBuyerRecord.ProductName ) == null)
        //        {
        //            if (!strProduct.ToString().Contains("" + ProductBuyerRecord.ProductName))
        //            {
        //                strProduct.Append(" " + ProductBuyerRecord.ProductName + ",");
        //            }                   
        //        }

        //        if (ProductBuyerRecord.BuyerName !=null)
        //        {
        //            if (_BuyerService.GetBuyerByName(ProductBuyerRecord.BuyerName) == null)
        //        {
        //            if (!strBuyer.ToString().Contains("" + ProductBuyerRecord.BuyerName))
        //            {
        //                strBuyer.Append(" " + ProductBuyerRecord.BuyerName + ",");
        //            }
        //        }
        //        }
                

        //    }
        //     if (!string.IsNullOrEmpty(strProduct.ToString())                 
        //        || !string.IsNullOrEmpty(strBuyer.ToString()) )
        //    {
        //        if(string.IsNullOrEmpty(strProduct.ToString()))
        //        {
        //            ViewBag.Product = null;
        //        }
        //        else
        //        {
        //            ViewBag.Product = strProduct;
        //        }


        //        if (string.IsNullOrEmpty(strBuyer.ToString()))
        //        {
        //            ViewBag.Buyer = null;
        //        }
        //        else
        //        {
        //            ViewBag.Buyer = strBuyer;
        //        }

              
        //        return View();
        //    }
        //    else
        //    {

        //        foreach (var ProductBuyerRecord in ProductBuyerRecordList)
        //        {

        //            //Object for ProductBuyer Model to Save 

        //            Product product = _ProductService.Find(ProductBuyerRecord.ProductName );
        //            Buyer buyer = _BuyerService.GetBuyerByName(ProductBuyerRecord.BuyerName );

        //            ProductBuyer PB = _ProductBuyerService.Find(buyer.PersonID, product.ProductId);

        //            if (PB == null)
        //            {
        //                ProductBuyer sl = new ProductBuyer();


        //                sl.ProductId = product.ProductId;
        //                sl.BuyerId = buyer.PersonID;

        //                sl.BuyerSku = ProductBuyerRecord.BuyerSKU;
        //                sl.BuyerUpcCode = ProductBuyerRecord.BuyerUPC;
        //                sl.CreatedDate = DateTime.Now;
        //                sl.ModifiedDate = DateTime.Now;
        //                sl.CreatedBy = User.Identity.Name;
        //                sl.ModifiedBy = User.Identity.Name;
        //                sl.ObjectState = Model.ObjectState.Added;

        //                _ProductBuyerService.Create(sl);
        //               // new ProductBuyerService(_unitOfWork).Create(sl);
        //            }

        //            else
        //            {

        //                ProductBuyer P = _ProductBuyerService.Find(PB.ProductBuyerId);

        //                P.ProductId = product.ProductId;
        //                P.BuyerId = buyer.PersonID;
        //                P.BuyerSku = ProductBuyerRecord.BuyerSKU;
        //                P.BuyerUpcCode = ProductBuyerRecord.BuyerUPC;                        
        //                P.ModifiedDate = DateTime.Now;                        
        //                P.ModifiedBy = User.Identity.Name;
        //                P.ObjectState = Model.ObjectState.Modified;

        //                _ProductBuyerService.Update(P);    
        //                //new ProductBuyerService(_unitOfWork).Update(P);  
        //            }

               
        //        }



        //        try
        //        {
        //            _unitOfWork.Save();
        //        }

        //        catch (Exception ex)
        //        {

        //            ModelState.AddModelError("", "");
        //            ViewBag.Mode = "Edit";
        //            return View("Create");
        //        }

        //        ViewBag.id = id;
        //        return View("Sucess");
        //    }                        
        //}

        public ActionResult ProductBuyersExcelImport()
        {
            string ErrorText = "";

            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);
            file.SaveAs(filePath);
            var excel = new ExcelQueryFactory();
            excel.FileName = filePath;
            var ProductBuyerRecordList = from c in excel.Worksheet<ProductBuyerExcel>() select c;

            StringBuilder strProduct = new StringBuilder();
            StringBuilder strBuyer = new StringBuilder();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ProductName");
            dataTable.Columns.Add("BuyerName");            
            dataTable.Columns.Add("BuyerSKU");
            dataTable.Columns.Add("BuyerUPC");
            dataTable.Columns.Add("CreatedBy");


            foreach (var ProductBuyerRecord in ProductBuyerRecordList)
            {

                    var dr = dataTable.NewRow();
                    dr["ProductName"] = ProductBuyerRecord.ProductName;
                    dr["BuyerName"] = ProductBuyerRecord.BuyerName ;
                    dr["BuyerSKU"] =  ProductBuyerRecord.BuyerSKU ;
                    dr["BuyerUPC"] =  ProductBuyerRecord.BuyerUPC ;
                    dr["CreatedBy"] = User.Identity.Name;

                    dataTable.Rows.Add(dr);

            }




            //string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportProductBuyerFromExcelFile"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@FileData", dataTable);
                    cmd.CommandTimeout = 1000;
                    //cmd.Connection.Open();
                    //cmd.ExecuteNonQuery();
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                    //cmd.Connection.Close();
                }
            }

            if (ds.Tables[0].Rows.Count == 0)
            {
                return View("Sucess");
            }
            else
            {
                for (int j = 0; j <= ds.Tables[0].Rows.Count - 1; j++)
                {
                    ErrorText = ErrorText + ds.Tables[0].Rows[j]["ErrorText"].ToString() + "." + Environment.NewLine;
                }
                ViewBag.Error = ErrorText;
                return View("Error");
            }
        }

    }
}