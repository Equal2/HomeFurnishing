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
using Presentation;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class JobReceiveEventsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobReceiveEventsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public ActionResult JobReceive_OnSubmit(int Id, string ReturnUrl)
        {
            int Cnt = 0;
            JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(Id);

            //#region BomPost

            //IEnumerable<JobReceiveBom> JobReceiveBomList = new JobReceiveBomService(_unitOfWork).GetBomForHeader(Header.JobReceiveHeaderId);

            //foreach (JobReceiveBom item in JobReceiveBomList)
            //{
            //    if (item.StockProcessId != 0 && item.JobReceiveBomId != null)
            //    {
            //        new StockProcessService(_unitOfWork).Delete((int)item.StockProcessId);
            //    }
            //    new JobReceiveBomService(_unitOfWork).Delete(item.JobReceiveBomId);
            //}

            //SqlParameter SQLJobReceiveHeaderId = new SqlParameter("@JobReceiveHeaderId", Id);

            //List<ProcGetBomForWeavingViewModel> BomForWeaving = new List<ProcGetBomForWeavingViewModel>();

            //BomForWeaving = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("Web.ProcGetBomForWeavingReceive @JobReceiveHeaderId", SQLJobReceiveHeaderId).ToList();

            //foreach (var item in BomForWeaving)
            //{
            //    Cnt = Cnt + 1;

            //    JobReceiveBom BomPost = new JobReceiveBom();
            //    BomPost.JobReceiveHeaderId = Id;
            //    BomPost.CreatedBy = User.Identity.Name;
            //    BomPost.CreatedDate = DateTime.Now;
            //    BomPost.ModifiedBy = User.Identity.Name;
            //    BomPost.ModifiedDate = DateTime.Now;
            //    BomPost.ProductId = item.ProductId;
            //    BomPost.Dimension1Id = item.Dimension1Id;
            //    BomPost.Dimension2Id = item.Dimension2Id;
            //    BomPost.CostCenterId = item.CostCenterId;
            //    BomPost.Qty = Convert.ToDecimal(item.Qty);

            //    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
            //    if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
            //    {
            //        StockProcessBomViewModel.StockHeaderId = (int)Header.StockHeaderId;
            //    }

            //    StockProcessBomViewModel.StockProcessId = -Cnt;
            //    StockProcessBomViewModel.DocHeaderId = Header.JobReceiveHeaderId;
            //    StockProcessBomViewModel.DocLineId = BomPost.JobReceiveBomId;
            //    StockProcessBomViewModel.DocTypeId = Header.DocTypeId;
            //    StockProcessBomViewModel.StockHeaderDocDate = Header.DocDate;
            //    StockProcessBomViewModel.StockProcessDocDate = DateTime.Now.Date;
            //    StockProcessBomViewModel.DocNo = Header.DocNo;
            //    StockProcessBomViewModel.DivisionId = Header.DivisionId;
            //    StockProcessBomViewModel.SiteId = Header.SiteId;
            //    StockProcessBomViewModel.CurrencyId = null;
            //    StockProcessBomViewModel.HeaderProcessId = null;
            //    StockProcessBomViewModel.PersonId = Header.JobWorkerId;
            //    StockProcessBomViewModel.ProductId = item.ProductId;
            //    StockProcessBomViewModel.HeaderFromGodownId = null;
            //    StockProcessBomViewModel.HeaderGodownId = null;
            //    StockProcessBomViewModel.GodownId = Header.GodownId;
            //    StockProcessBomViewModel.ProcessId = Header.ProcessId;
            //    StockProcessBomViewModel.LotNo = null;
            //    StockProcessBomViewModel.CostCenterId = item.CostCenterId;
            //    StockProcessBomViewModel.Qty_Iss = item.Qty;
            //    StockProcessBomViewModel.Qty_Rec = 0;
            //    StockProcessBomViewModel.Rate = 0;
            //    StockProcessBomViewModel.ExpiryDate = null;
            //    StockProcessBomViewModel.Specification = null;
            //    StockProcessBomViewModel.Dimension1Id = item.Dimension1Id;
            //    StockProcessBomViewModel.Dimension2Id = item.Dimension2Id;
            //    StockProcessBomViewModel.Remark = null;
            //    StockProcessBomViewModel.Status = Header.Status;
            //    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
            //    StockProcessBomViewModel.CreatedDate = DateTime.Now;
            //    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
            //    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

            //    string StockProcessPostingError = "";
            //    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPost(ref StockProcessBomViewModel);

            //    if (StockProcessPostingError != "")
            //    {
            //        ModelState.AddModelError("", StockProcessPostingError);
            //        //return PartialView("_Create", svm);
            //    }

            //    BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;
            //    BomPost.ObjectState = Model.ObjectState.Added;
            //    new JobReceiveBomService(_unitOfWork).Create(BomPost);

            //    if (Header.StockHeaderId == null)
            //    {
            //        Header.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
            //    }
            //}

            //#endregion









            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            JobReceiveSettings  Setting = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Header.DocTypeId , Header.DivisionId, Header.SiteId);
            //string ProcConsumption = Setting.SqlProcConsumption.ToString();

            try
            {
                DataSet ds = new DataSet();
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostBomForWeavingReceive"))

                    
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobReceiveHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        //cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        //cmd.Connection.Close();
                    }

                    //using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".Mig_PostWeavingConsumption_OldData"))
                    //{
                    //    cmd.CommandType = CommandType.StoredProcedure;
                    //    cmd.Connection = sqlConnection;
                    //    cmd.Parameters.AddWithValue("@JobReceiveHeaderId", Id);
                    //    cmd.CommandTimeout = 1000;
                    //    cmd.ExecuteNonQuery();
                    //}

                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PostBazarAtBranch"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobReceiveHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        //cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        //cmd.Connection.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".Mig_WeavingReceiveLedgerPosting"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobReceiveHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        //cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        //cmd.Connection.Close();
                    }

                    //using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_WeavingSmallChunk_LedgerPost"))
                    //{
                    //    cmd.CommandType = CommandType.StoredProcedure;
                    //    cmd.Connection = sqlConnection;
                    //    cmd.Parameters.AddWithValue("@JobReceiveHeaderId", Id);
                    //    cmd.CommandTimeout = 1000;
                    //    //cmd.Connection.Open();
                    //    cmd.ExecuteNonQuery();
                    //    //cmd.Connection.Close();
                    //}
                }
            }

            catch (Exception ex)
            {
                if (Header == null)
                {
                    throw new Exception("Header is null");
                }
                Header.Status = (int)StatusConstants.Drafted;
                new JobReceiveHeaderService(_unitOfWork).Update(Header);
                _unitOfWork.Save();
                return Redirect(ReturnUrl).Warning(ex.Message.ToString());
                //throw ex;
            }

            return Redirect(ReturnUrl);
        }

        public ActionResult JobReceive_OnApprove(int Id, string ReturnUrl)
        {
            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(Id);
            return Redirect(ReturnUrl);
        }

        public void CreateJobReceiveAtBranch(int JobReceiveHeaderId)
        {
            JobReceiveHeader JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(JobReceiveHeaderId);

            int SisteConcernSiteId = (from H in db.JobReceiveHeader
                                      join p in db.Persons on H.JobWorkerId equals p.PersonID into JobWorkerTable
                                      from JobWorkerTab in JobWorkerTable.DefaultIfEmpty()
                                      join s in db.Site on JobWorkerTab.PersonID equals s.PersonId into SiteTable
                                      from SiteTab in SiteTable.DefaultIfEmpty()
                                      where H.JobReceiveHeaderId == JobReceiveHeaderId
                                      select new
                                      {
                                          SiteId = SiteTab.SiteId
                                      }).FirstOrDefault().SiteId;

            if (SisteConcernSiteId != null && SisteConcernSiteId != 0)
            {
                var Temp = (from L in db.JobReceiveLine
                            join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                            from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                            join P in db.ProductUidSiteDetail on L.ProductUidId equals P.ProductUIDId into ProductUidTable
                            from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                            join Joh in db.JobOrderHeader on ProductUidTab.GenDocId equals Joh.JobOrderHeaderId into JobOrderHeaderTable
                            from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                            where L.JobReceiveHeaderId == JobReceiveHeaderId
                            group new { JobReceiveHeaderTab, JobOrderHeaderTab } by new { JobOrderHeaderTab.JobWorkerId } into Results
                            select new
                            {
                                DocDate = Results.Max(i => i.JobReceiveHeaderTab.DocDate),
                                DivisionId = Results.Max(i => i.JobReceiveHeaderTab.DivisionId),
                                ProcessId = Results.Max(i => i.JobReceiveHeaderTab.ProcessId),
                                JobWorkerId = Results.Key.JobWorkerId,
                                Remark = Results.Max(i => i.JobReceiveHeaderTab.Remark)
                            }).ToList();

                foreach (var SisterConcernJobReceiveHeader in Temp)
                {
                    JobReceiveHeader Header = new JobReceiveHeader();
                    Header.DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.WeavingBazar).DocumentTypeId;
                    Header.DocDate = SisterConcernJobReceiveHeader.DocDate;
                    Header.DocNo = "1";
                    Header.DivisionId = SisterConcernJobReceiveHeader.DivisionId;
                    Header.SiteId = SisteConcernSiteId;
                    Header.ProcessId = SisterConcernJobReceiveHeader.ProcessId;
                    Header.JobWorkerId = SisterConcernJobReceiveHeader.JobWorkerId;
                    Header.JobReceiveById = 1;
                    Header.GodownId = 1;
                    Header.Remark = SisterConcernJobReceiveHeader.Remark;
                    Header.CreatedBy = User.Identity.Name;
                    Header.CreatedDate = DateTime.Now;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;

                    new JobReceiveHeaderService(_unitOfWork).Create(Header);
                }





            }
        }
    }
}