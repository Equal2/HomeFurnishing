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
    public interface IUpdateJobExpiryService : IDisposable
    {
        bool UpdateJobOrderExpiry(int JobOrderId, string Reason, string User, DateTime ExpiryDate);
    }

    public class UpdateJobExpiryService : IUpdateJobExpiryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public UpdateJobExpiryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool UpdateJobOrderExpiry(int JobOrderId, string Reason, string User, DateTime ExpiryDate)
        {
            var Rec = (from p in db.JobOrderHeader
                           where p.JobOrderHeaderId==JobOrderId
                           select p).FirstOrDefault();           

            if (Rec == null)
                return false;
            else
            { 
                using (ApplicationDbContext con=new ApplicationDbContext())
                {                    
                    Rec.DueDate = ExpiryDate;
                    Rec.ModifiedBy = User;
                    Rec.ModifiedDate = DateTime.Now;
                    Rec.ObjectState = Model.ObjectState.Modified;
                    con.JobOrderHeader.Add(Rec);

                    ActivityLog log = new ActivityLog();                    
                    log.CreatedBy = User;
                    log.CreatedDate = DateTime.Now;                    
                    log.DocId = Rec.JobOrderHeaderId;
                    log.DocTypeId = Rec.DocTypeId;                    
                    log.Narration = "JobOrder Expiry changed";
                    log.UserRemark = Reason;
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
