using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;
using System.Data.SqlClient;

namespace Service
{
    public interface IExcessJobReviewService : IDisposable
    {
        IEnumerable<ExcessJobOrderReviewViewModel> GetExcessJobOrders(int Id);
        bool ApproveExcessStock(int id, string UserName);
        bool DisApproveExcessStock(int id, string UserName);
    }

    public class ExcessJobReviewService : IExcessJobReviewService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public ExcessJobReviewService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ExcessJobOrderReviewViewModel> GetExcessJobOrders(int Id)
        {            

            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", (int)System.Web.HttpContext.Current.Session["SiteId"]);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", (int)System.Web.HttpContext.Current.Session["DivisionId"]);
            SqlParameter SqlParameterDocumentType = new SqlParameter("@DocumentType", Id);

            IEnumerable<ExcessJobOrderReviewViewModel> PendingJOTReview = db.Database.SqlQuery<ExcessJobOrderReviewViewModel>("[Web].[SpCheck_UpdateProdOrderStatus_Wizard] @SiteId, @DivisionId, @DocumentType", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDocumentType).ToList();

            return PendingJOTReview;

        }

        public bool ApproveExcessStock(int id,string UserName)
        {
            var Rec = (from p in db.ProdOrderLineStatus
                      where p.ProdOrderLineId == id
                      select p).FirstOrDefault();           

            if (Rec == null)
                return false;
            else
            { 

                var HeaderRec=(from p in db.ProdOrderLine
                              where p.ProdOrderLineId==id
                              join t in db.ProdOrderHeader on p.ProdOrderHeaderId equals t.ProdOrderHeaderId
                              select t).FirstOrDefault();

                using (ApplicationDbContext con=new ApplicationDbContext())
                {
                    Rec.ExcessJobOrderReviewBy = UserName;
                    Rec.ObjectState = Model.ObjectState.Modified;
                    con.ProdOrderLineStatus.Add(Rec);

                    ActivityLog log = new ActivityLog();
                    log.ActivityType = (int)ActivityTypeContants.Approved;
                    log.CreatedBy = UserName;
                    log.CreatedDate = DateTime.Now;                    
                    log.DocId = HeaderRec.ProdOrderHeaderId;
                    log.DocTypeId = HeaderRec.DocTypeId;
                    log.DocLineId = Rec.ProdOrderLineId;
                    log.Narration = "ExcessStock Approved";
                    log.UserRemark = "";
                    log.ObjectState = Model.ObjectState.Added;

                    con.ActivityLog.Add(log);

                    con.SaveChanges();
                }
                return true;
            }
        }

        public bool DisApproveExcessStock(int id,string UserName)
        {
            var Rec = (from p in db.ProdOrderLineStatus
                       where p.ProdOrderLineId == id
                       select p).FirstOrDefault();

            if (Rec == null)
                return false;
            else
            {

                var HeaderRec = (from p in db.ProdOrderLine
                                 where p.ProdOrderLineId == id
                                 join t in db.ProdOrderHeader on p.ProdOrderHeaderId equals t.ProdOrderHeaderId
                                 select t).FirstOrDefault();

                using (ApplicationDbContext con = new ApplicationDbContext())
                {
                    Rec.ExcessJobOrderReviewBy = null;
                    Rec.ObjectState = Model.ObjectState.Modified;
                    con.ProdOrderLineStatus.Add(Rec);

                    ActivityLog log = new ActivityLog();
                    log.ActivityType = (int)ActivityTypeContants.Approved;
                    log.CreatedBy = UserName;
                    log.DocId = HeaderRec.ProdOrderHeaderId;
                    log.CreatedDate = DateTime.Now;
                    log.DocTypeId = HeaderRec.DocTypeId;
                    log.DocLineId = Rec.ProdOrderLineId;
                    log.Narration = "ExcessStock DisApproved";
                    log.UserRemark = "";
                    log.ObjectState = Model.ObjectState.Added;

                    con.ActivityLog.Add(log);

                    con.SaveChanges();
                }
                return true;
            }
        }


        public void Dispose()
        {
        }
    }
}
