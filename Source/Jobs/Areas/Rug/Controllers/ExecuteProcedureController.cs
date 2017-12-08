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
using System.IO;
using System.Data.SqlClient;
using System.Data;
using EmailContents;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ExecuteProcedureController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ExecuteProcedureController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public ActionResult WeavingTimeIncentive_LedgerPost(int Id, string ReturnUrl)
        {
            //int SiteId;
            //int DivisionId;
            //string UserName;

            //SiteId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId];
            //DivisionId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId];
            //UserName = User.Identity.Name;

            //string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            //DataSet ds = new DataSet();
            //using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            //{
            //    sqlConnection.Open();

            //    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_WeavingTimeIncentive_LedgerPost"))
            //        {
            //            cmd.CommandType = CommandType.StoredProcedure;
            //            cmd.Connection = sqlConnection;
            //            cmd.Parameters.AddWithValue("@Site", SiteId);
            //            cmd.Parameters.AddWithValue("@Division", DivisionId);
            //            cmd.Parameters.AddWithValue("@UserName", UserName);
            //            cmd.CommandTimeout = 1000;
            //           // cmd.ExecuteNonQuery();

            //            using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
            //            {
            //                adp.Fill(ds);
            //            }
            //        }
            //    }


            //if (ds.Tables[0].Rows[0][0].ToString() == "Sucess")
            //{
            //    return View("Sucess");
            //}
            //else
            //{
            //    ViewBag.Error = ds.Tables[0].Rows[0][0].ToString();
                return View("Error");
            //}

        }


        public ActionResult WeavingTimePenalty_LedgerPost(int Id, string ReturnUrl)
        {
            //int SiteId;
            //int DivisionId;
            //string UserName;

            //SiteId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId];
            //DivisionId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId];
            //UserName = User.Identity.Name;

            //string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            //DataSet ds = new DataSet();
            //using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            //{
            //    sqlConnection.Open();

            //    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_WeavingTimePenalty_LedgerPost"))
            //    {
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Connection = sqlConnection;
            //        cmd.Parameters.AddWithValue("@Site", SiteId);
            //        cmd.Parameters.AddWithValue("@Division", DivisionId );
            //        cmd.Parameters.AddWithValue("@UserName", UserName);
            //        cmd.CommandTimeout = 1000;
            //        //cmd.ExecuteNonQuery();

            //        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
            //        {
            //            adp.Fill(ds);
            //        }

            //    }
            //}


            //if (ds.Tables[0].Rows[0][0].ToString() == "Sucess")
            //{
            //    return View("Sucess");
            //}
            //else
            //{
            //    ViewBag.Error = ds.Tables[0].Rows[0][0].ToString();
               return View("Error");
            //}



        }


        public ActionResult WeavingSmallChunkPenalty_LedgerPost(int Id, string ReturnUrl)
        {
            //int SiteId;
            //int DivisionId;
            //string UserName;

            //SiteId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId];
            //DivisionId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId];
            //UserName = User.Identity.Name;

            //string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            //DataSet ds = new DataSet();
            //using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            //{
            //    sqlConnection.Open();

            //    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_WeavingSmallChunk_LedgerPost"))
            //    {
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Connection = sqlConnection;
            //        cmd.Parameters.AddWithValue("@Site", SiteId);
            //        cmd.Parameters.AddWithValue("@Division", DivisionId);
            //        cmd.Parameters.AddWithValue("@UserName", UserName);
            //        cmd.CommandTimeout = 1000;
            //        //cmd.ExecuteNonQuery();

            //        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
            //        {
            //            adp.Fill(ds);
            //        }

            //    }
            //}


            //if (ds.Tables[0].Rows[0][0].ToString() == "Sucess")
            //{
            //    return View("Sucess");
            //}
            //else
            //{
            //    ViewBag.Error = ds.Tables[0].Rows[0][0].ToString();
                return View("Error");
            //}



        }

    }
}