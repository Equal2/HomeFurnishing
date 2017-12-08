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
    public interface IUpdatePurchaseExpiryService : IDisposable
    {
        bool UpdatePurchaseOrderExpiry(int PurchaseOrderId, string Reason, string User, DateTime ExpiryDate);
    }

    public class UpdatePurchaseExpiryService : IUpdatePurchaseExpiryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public UpdatePurchaseExpiryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool UpdatePurchaseOrderExpiry(int PurchaseOrderId, string Reason, string User, DateTime ExpiryDate)
        {
            var Rec = (from p in db.PurchaseOrderHeader
                           where p.PurchaseOrderHeaderId==PurchaseOrderId
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
                    con.PurchaseOrderHeader.Add(Rec);

                    ActivityLog log = new ActivityLog();                    
                    log.CreatedBy = User;
                    log.CreatedDate = DateTime.Now;                    
                    log.DocId = Rec.PurchaseOrderHeaderId;
                    log.DocTypeId = Rec.DocTypeId;                    
                    log.Narration = "PurchaseOrder Expiry changed";
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
