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
    public class JobOrderCancelEventsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobOrderCancelEventsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public ActionResult JobOrderCancel_OnSubmit(int Id, string ReturnUrl)
        {
            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            JobOrderCancelHeader JobOrderCancelHeader = new JobOrderCancelHeaderService(_unitOfWork).Find(Id);


            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostBomForWeavingOrderCancel"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobOrderCancelHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostRequisitionCancelForWeavingOrderCancel"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobOrderCancelHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            catch (Exception ex)
            {
                JobOrderCancelHeader.Status = (int)StatusConstants.Drafted;
                new JobOrderCancelHeaderService(_unitOfWork).Update(JobOrderCancelHeader);
                _unitOfWork.Save();
                throw ex;
            }


            return Redirect(ReturnUrl);
        }

        public ActionResult JobOrderCancel_OnApprove(int Id, string ReturnUrl)
        {
            JobOrderCancelHeader H = new JobOrderCancelHeaderService(_unitOfWork).Find(Id);
            return Redirect(ReturnUrl);
        }
    }

    public class RequisitionCancelLineFroJobOrderBom
    {
        public int RequisitionLineId { get; set; }
        public int ProductId { get; set; }
        public Decimal Qty { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }


}