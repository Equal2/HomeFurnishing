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
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IPurchaseOrderAmendmentHeaderService : IDisposable
    {
        PurchaseOrderAmendmentHeader Create(PurchaseOrderAmendmentHeader pt, string UserName);
        void Delete(int id);
        void Delete(PurchaseOrderAmendmentHeader pt);
        PurchaseOrderAmendmentHeader Find(int id);
        void Update(PurchaseOrderAmendmentHeader pt, string UserName);
        IQueryable<PurchaseOrderAmendmentHeaderViewModel> GetPurchaseOrderAmendmentHeaderList(int id, string Uname);
        IQueryable<PurchaseOrderAmendmentHeaderViewModel> GetPurchaseOrderAmendmentHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseOrderAmendmentHeaderViewModel> GetPurchaseOrderAmendmentHeaderListPendingToReview(int id, string Uname);
        Task<IEquatable<PurchaseOrderAmendmentHeader>> GetAsync();
        Task<PurchaseOrderAmendmentHeader> FindAsync(int id);
        PurchaseOrderAmendmentHeader GetPurchaseOrderAmendmentHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class PurchaseOrderAmendmentHeaderService : IPurchaseOrderAmendmentHeaderService
    {
        private ApplicationDbContext db;
        private bool disposed = false;
        public PurchaseOrderAmendmentHeaderService(ApplicationDbContext db)
        {
            this.db = db;

        }
        public PurchaseOrderAmendmentHeader GetPurchaseOrderAmendmentHeaderByName(string terms)
        {
            return (from p in db.PurchaseOrderAmendmentHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public PurchaseOrderAmendmentHeader Find(int id)
        {
            return db.PurchaseOrderAmendmentHeader.Find(id);
        }

        public PurchaseOrderAmendmentHeader Create(PurchaseOrderAmendmentHeader pt, string UserName)
        {
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedDate = DateTime.Now;
            pt.CreatedBy = UserName;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Added;
            db.PurchaseOrderAmendmentHeader.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            PurchaseOrderAmendmentHeader Header = db.PurchaseOrderAmendmentHeader.Find(id);
            db.PurchaseOrderAmendmentHeader.Remove(Header);
        }

        public void Delete(PurchaseOrderAmendmentHeader pt)
        {
            db.PurchaseOrderAmendmentHeader.Remove(pt);
        }

        public void Update(PurchaseOrderAmendmentHeader pt,string UserName)
        {
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Modified;
            db.PurchaseOrderAmendmentHeader.Add(pt);
        }

        public IQueryable<PurchaseOrderAmendmentHeaderViewModel> GetPurchaseOrderAmendmentHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.PurchaseOrderAmendmentHeader
                    join t in db.Persons on p.SupplierId equals t.PersonID
                    orderby p.DocNo descending, p.DocDate descending
                    where p.SiteId == SiteId && p.DivisionId == p.DivisionId && p.DocTypeId == id
                    select new PurchaseOrderAmendmentHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        PurchaseOrderAmendmentHeaderId = p.PurchaseOrderAmendmentHeaderId,
                        CreatedBy = p.CreatedBy,
                        CreatedDate = p.CreatedDate,
                        DocTypeId = p.DocTypeId,
                        ModifiedBy = p.ModifiedBy,
                        ModifiedDate = p.ModifiedDate,
                        Remark = p.Remark,
                        SupplierName = t.Name,
                        Status = p.Status,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) == 1),
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                    }
                        );
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseOrderAmendmentHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderAmendmentHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderAmendmentHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderAmendmentHeaderId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.PurchaseOrderAmendmentHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderAmendmentHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderAmendmentHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderAmendmentHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<PurchaseOrderAmendmentHeaderViewModel> GetPurchaseOrderAmendmentHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseOrderHeader = GetPurchaseOrderAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseOrderAmendmentHeaderViewModel> GetPurchaseOrderAmendmentHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseOrderHeader = GetPurchaseOrderAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public Task<IEquatable<PurchaseOrderAmendmentHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderAmendmentHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
