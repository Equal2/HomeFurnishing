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
using Model.ViewModel;
using Model.ViewModels;
using AutoMapper;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class JobReturnEventsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobReturnEventsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public ActionResult JobReturn_OnSubmit(int Id, string ReturnUrl)
        {
            int Cnt = 0;
            JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(Id);

            
            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            try
            {
                DataSet ds = new DataSet();
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostBomForWeavingReturn"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobReturnHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        //cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        //cmd.Connection.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostWeavingReturnAtBranch"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobReturnHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        //cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        //cmd.Connection.Close();
                    }


                    var Temp = (from L in db.JobReturnLine
                                where L.JobReturnHeaderId == Id
                                select new
                                {
                                    JobReceiveHeaderId = L.JobReceiveLine.JobReceiveHeaderId
                                }).Distinct();


                    foreach (var item in Temp)
                    {
                        using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".Mig_WeavingReceiveLedgerPosting"))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = sqlConnection;
                            cmd.Parameters.AddWithValue("@JobReceiveHeaderId", item.JobReceiveHeaderId);
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }

                        //using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".Mig_PostWeavingConsumption_OldData"))
                        //{
                        //    cmd.CommandType = CommandType.StoredProcedure;
                        //    cmd.Connection = sqlConnection;
                        //    cmd.Parameters.AddWithValue("@JobReceiveHeaderId", item.JobReceiveHeaderId);
                        //    cmd.CommandTimeout = 1000;
                        //    cmd.ExecuteNonQuery();
                        //}
                    }


                    //var TempBranch = (from L in db.JobReturnLine
                    //                  join BJrl in db.JobReceiveLine on L.JobReceiveLineId equals BJrl.JobReceiveLineId into JobReceiveLineTable
                    //                  from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                    //                  where L.JobReturnHeaderId == Id
                    //                  select new
                    //                  {
                    //                      JobReceiveHeaderId = JobReceiveLineTab.JobReceiveHeaderId
                    //                  }).Distinct();

                    //foreach (var item in TempBranch)
                    //{
                    //    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".Mig_PostWeavingConsumption_OldData"))
                    //    {
                    //        cmd.CommandType = CommandType.StoredProcedure;
                    //        cmd.Connection = sqlConnection;
                    //        cmd.Parameters.AddWithValue("@JobReceiveHeaderId", item.JobReceiveHeaderId);
                    //        cmd.CommandTimeout = 1000;
                    //        cmd.ExecuteNonQuery();
                    //    }
                    //}
                }
            }

            catch (Exception ex)
            {
                Header.Status = (int)StatusConstants.Drafted;
                new JobReturnHeaderService(_unitOfWork).Update(Header);
                _unitOfWork.Save();
                throw ex;
            }

            return Redirect(ReturnUrl);
        }

        public ActionResult JobReturn_OnApprove(int Id, string ReturnUrl)
        {
            JobReturnHeader H = new JobReturnHeaderService(_unitOfWork).Find(Id);
            return Redirect(ReturnUrl);
        }
    }
}