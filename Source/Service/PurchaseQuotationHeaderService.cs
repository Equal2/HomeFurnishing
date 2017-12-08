using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using System.Data.Entity.SqlServer;
using Model.ViewModel;
using AutoMapper;

namespace Service
{
    public interface IPurchaseQuotationHeaderService : IDisposable
    {
        PurchaseQuotationHeader Create(PurchaseQuotationHeader s);
        void Delete(int id);
        void Delete(PurchaseQuotationHeader s);
        PurchaseQuotationHeaderViewModel GetPurchaseQuotationHeader(int id);
        PurchaseQuotationHeader Find(int id);
        IQueryable<PurchaseQuotationHeaderViewModel> GetPurchaseQuotationHeaderList(int id, string name);
        IQueryable<PurchaseQuotationHeaderViewModel> GetPurchaseQuotationPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseQuotationHeaderViewModel> GetPurchaseQuotationPendingToReview(int id, string Uname);
        void Update(PurchaseQuotationHeader s);
        int NextId(int id);
        int PrevId(int id);
    }
    public class PurchaseQuotationHeaderService : IPurchaseQuotationHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();

        public PurchaseQuotationHeaderService()
        {
        }

        public PurchaseQuotationHeader Create(PurchaseQuotationHeader s)
        {
            s.ObjectState = ObjectState.Added;
            db.PurchaseQuotationHeader.Add(s);
            return s;
        }

        public void Delete(int id)
        {
            var obj = db.PurchaseQuotationHeader.Find(id);
            obj.ObjectState = Model.ObjectState.Deleted;
            db.PurchaseQuotationHeader.Remove(obj);
        }
        public void Delete(PurchaseQuotationHeader s)
        {
            s.ObjectState = Model.ObjectState.Deleted;
            db.PurchaseQuotationHeader.Remove(s);
        }
        public void Update(PurchaseQuotationHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            db.PurchaseQuotationHeader.Add(s);
        }


        public PurchaseQuotationHeader Find(int id)
        {
            return db.PurchaseQuotationHeader.Find(id);
        }

        public IQueryable<PurchaseQuotationHeaderViewModel> GetPurchaseQuotationHeaderList(int id, string name)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.PurchaseQuotationHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id && p.DocTypeId == id
                    select new PurchaseQuotationHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        Status = p.Status,
                        PurchaseQuotationHeaderId = p.PurchaseQuotationHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        VendorQuotationDate = p.VendorQuotationDate,
                        VendorQuotationNo = p.VendorQuotationNo,
                        Reviewed = (SqlFunctions.CharIndex(name, p.ReviewBy) > 0),
                        SupplierName=p.Supplier.Person.Name,
                    });
        }

        public PurchaseQuotationHeaderViewModel GetPurchaseQuotationHeader(int id)
        {
            var Rec = (from p in db.PurchaseQuotationHeader
                       where p.PurchaseQuotationHeaderId == id
                       select p).FirstOrDefault();

            return Mapper.Map<PurchaseQuotationHeaderViewModel>(Rec);
        }

        public IQueryable<PurchaseQuotationHeaderViewModel> GetPurchaseQuotationPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseQuotationHeader = GetPurchaseQuotationHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseQuotationHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseQuotationHeaderViewModel> GetPurchaseQuotationPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseQuotationHeader = GetPurchaseQuotationHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseQuotationHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseQuotationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseQuotationHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseQuotationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseQuotationHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseQuotationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseQuotationHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseQuotationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseQuotationHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
