using System.Web.Mvc;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using Model.ViewModel;
using System.Configuration;
using System.Linq;
using Data;


namespace Web
{
    [Authorize]
    public class MdashBoardController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
    
        // GET: /ProductMaster/

       
      public DashBoardTables FProductTest()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
          

            IEnumerable<ProductionTest> FPTest = db.Database.SqlQuery<ProductionTest>("Select DivisionName,ProductCategoryName,Qty,Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProductionTest Where SiteId="+SiteId+" and  DivisionId="+ DivisionId).ToList();
            IEnumerable<MDashBoardPacking> Packing = db.Database.SqlQuery<MDashBoardPacking>("select DivisionName,Name,Qty,Area from " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDashBoardPacking Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId).ToList();
            IEnumerable<MDashBoardDyeing> Dyeing = db.Database.SqlQuery<MDashBoardDyeing>("select DivisionName,Location,Qty from " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDashBoardDyeing Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId).ToList();
            IEnumerable<MDashBoardPurchase> Purchase = db.Database.SqlQuery<MDashBoardPurchase>("select DivisionName,ProductCategoryName,Qty,Area from  " + ConfigurationManager.AppSettings["DataBaseSchema"] + ".MDashBoardPurchase Where SiteId=" + SiteId + " and  DivisionId=" + DivisionId).ToList();
            /*SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            IEnumerable<ProductionTest> FPTest = db.Database.SqlQuery<ProductionTest>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spRep_ProductionTest @SiteId,@DivisionId", SqlParameterSiteId, SqlParameterDivisionId).ToList();*/
           /* SqlParameter SqlParameterSiteId1 = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId1 = new SqlParameter("@DivisionId", DivisionId);
            IEnumerable<MDashBoardPacking> Packing = db.Database.SqlQuery<MDashBoardPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spRep_MDashBoardPacking @SiteId,@DivisionId", SqlParameterSiteId1, SqlParameterDivisionId1).ToList();*/
            /*SqlParameter SqlParameterSiteId2 = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId2 = new SqlParameter("@DivisionId", DivisionId);
            IEnumerable<MDashBoardDyeing> Dyeing = db.Database.SqlQuery<MDashBoardDyeing>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spRep_MDashBoardDyeing @SiteId,@DivisionId", SqlParameterSiteId2, SqlParameterDivisionId2).ToList();*/
           /* SqlParameter SqlParameterSiteId3 = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId3 = new SqlParameter("@DivisionId", DivisionId);
            IEnumerable<MDashBoardPurchase> Purchase = db.Database.SqlQuery<MDashBoardPurchase>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spRep_MDashBoardPurchase @SiteId,@DivisionId", SqlParameterSiteId3, SqlParameterDivisionId3).ToList();*/
            DashBoardTables DB = new DashBoardTables();

             DB.ProductionTest = FPTest.ToList();
            DB.Packing = Packing.ToList();            
            DB.Dyeing = Dyeing.ToList();
            DB.Purchase = Purchase.ToList();
          
            return DB;            
        }

     public ActionResult index()
        {
         //var temp = db.Agent.Find(1);
            var t = FProductTest();
            return View(t);
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
