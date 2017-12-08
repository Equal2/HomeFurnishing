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
    public class JobOrderEventsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobOrderEventsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public ActionResult JobOrder_OnSubmit(int Id, string ReturnUrl)
        {
            JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(Id);

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            

            int MainSitid = new SiteService(_unitOfWork).FindBySiteCode("Main").SiteId;
            try
            {
                DataSet ds = new DataSet();
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    if (JobOrderHeader.SiteId != MainSitid)
                    {
                        using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostBomForWeavingOrder"))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = sqlConnection;
                            cmd.Parameters.AddWithValue("@JobOrderHeaderId", Id);
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_UpdateProductUidValuesForJobOrder"))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = sqlConnection;
                            cmd.Parameters.AddWithValue("@JobOrderHeaderId", Id);
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostRequisitionForWeavingOrder"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobOrderHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        cmd.ExecuteNonQuery();
                    }

                    if (JobOrderHeader.SiteId == MainSitid)
                    {
                        using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostProdOrderAtBranch"))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = sqlConnection;
                            cmd.Parameters.AddWithValue("@JobOrderHeaderId", Id);
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }



                        //using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".Mig_CreateItemUid"))
                        //{
                        //    cmd.CommandType = CommandType.StoredProcedure;
                        //    cmd.Connection = sqlConnection;
                        //    cmd.Parameters.AddWithValue("@HeaderId", Id);
                        //    cmd.Parameters.AddWithValue("@DocTypeId", JobOrderHeader.DocTypeId);
                        //    cmd.CommandTimeout = 1000;
                        //    cmd.ExecuteNonQuery();
                        //}



                    }
                }

            }

            catch (Exception ex)
            {
                JobOrderHeader.Status = (int)StatusConstants.Drafted;
                new JobOrderHeaderService(_unitOfWork).Update(JobOrderHeader);
                _unitOfWork.Save();
                throw ex;
            }

            //if (JobOrderHeader.SiteId == MainSitid)
            //{
            //    var temp = from H in db.JobOrderHeader
            //               join P in db.Persons on H.JobWorkerId equals P.PersonID into PersonTable from PersonTab in PersonTable.DefaultIfEmpty()
            //               join S in db.Site on PersonTab.PersonID equals S.PersonId into SiteTable from SiteTab in SiteTable.DefaultIfEmpty()
            //                where H.JobOrderHeaderId == Id
            //                select new {
            //                    GodownId = 
            //                }
            //}




            //int RequisitionDocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.Requisition).DocumentTypeId;

            //RequisitionHeader Header = new RequisitionHeader();
            //Header.DocTypeId = RequisitionDocTypeId;
            //Header.DocDate = JobOrderHeader.DocDate;
            //Header.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".RequisitionHeaders", RequisitionDocTypeId, JobOrderHeader.DocDate, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);
            //Header.DivisionId = JobOrderHeader.DivisionId;
            //Header.SiteId = JobOrderHeader.SiteId;
            //Header.Remark = JobOrderHeader.Remark;
            //Header.Status = JobOrderHeader.Status;
            //Header.CreatedBy = JobOrderHeader.CreatedBy;
            //Header.CreatedDate = JobOrderHeader.CreatedDate;
            //Header.ModifiedBy = JobOrderHeader.ModifiedBy;
            //Header.ModifiedDate = JobOrderHeader.ModifiedDate;
            //Header.PersonId = JobOrderHeader.JobWorkerId;
            //Header.CostCenterId = JobOrderHeader.CostCenterId;
            //Header.ReferenceDocTypeId = JobOrderHeader.DocTypeId;
            //Header.ReferenceDocId = JobOrderHeader.JobOrderHeaderId;
            //Header.ReasonId = 13;

            //new RequisitionHeaderService(_unitOfWork).Create(Header);

            //var JobOrderBomSummary = (from L in db.JobOrderBom
            //                         join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
            //                         from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
            //                         where L.JobOrderHeaderId == Id
            //                         group new { L, JobOrderHeaderTab } by new { L.ProductId, L.Dimension1Id, L.Dimension2Id } into Result
            //                         select new
            //                         {
            //                             ProductId = Result.Key.ProductId,
            //                             Qty = Result.Sum(i => i.L.Qty),
            //                             CreatedBy = Result.Max(i => i.L.CreatedBy),
            //                             CreatedDate = Result.Max(i => i.L.CreatedDate),
            //                             ModifiedBy = Result.Max(i => i.L.ModifiedBy),
            //                             ModifiedDate = Result.Max(i => i.L.ModifiedDate),
            //                             Dimension1Id = Result.Key.Dimension1Id,
            //                             Dimension2Id = Result.Key.Dimension2Id,
            //                             ProcessId = Result.Max(i => i.JobOrderHeaderTab.ProcessId)
            //                         }).ToList();

            //foreach (var item in JobOrderBomSummary)
            //{
            //    RequisitionLine Line = new RequisitionLine();
            //    Line.RequisitionHeaderId = Header.RequisitionHeaderId;
            //    Line.ProductId = item.ProductId;
            //    Line.Qty = item.Qty;
            //    Line.CreatedBy = item.CreatedBy;
            //    Line.CreatedDate = item.CreatedDate;
            //    Line.ModifiedBy = item.ModifiedBy;
            //    Line.ModifiedDate = item.ModifiedDate;
            //    Line.Dimension1Id = item.Dimension1Id;
            //    Line.Dimension2Id = item.Dimension2Id;

            //    new RequisitionLineService(_unitOfWork).Create(Line);
            //}

            //try
            //{
            //    _unitOfWork.Save();
            //}

            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    ModelState.AddModelError("", message);
            //}

            return Redirect(ReturnUrl);
        }

        public ActionResult JobOrder_OnApprove(int Id, string ReturnUrl)
        {
            JobOrderHeader H = new JobOrderHeaderService(_unitOfWork).Find(Id);
            return Redirect(ReturnUrl);
        }
    }
}