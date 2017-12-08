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
//using EmailContents;
using Model.ViewModel;
using Model.ViewModels;
using AutoMapper;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class RateConversionEventsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public RateConversionEventsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public ActionResult RateConversion_OnSubmit(int Id, string ReturnUrl)
        {
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(Id);

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            try
            {
                DataSet ds = new DataSet();
                //using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                //{
                //    sqlConnection.Open();

                //    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".Mig_RateConversionLedgerPosting"))
                //    {
                //        cmd.CommandType = CommandType.StoredProcedure;
                //        cmd.Connection = sqlConnection;
                //        cmd.Parameters.AddWithValue("@StockHeaderId", Id);
                //        cmd.CommandTimeout = 1000;
                //        cmd.ExecuteNonQuery();
                //    }
                //}
            }

            catch (Exception ex)
            {
                Header.Status = (int)StatusConstants.Drafted;
                new StockHeaderService(_unitOfWork).Update(Header);
                _unitOfWork.Save();
                throw ex;
            }

            return Redirect(ReturnUrl);
        }


    }
}