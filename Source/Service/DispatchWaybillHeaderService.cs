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

namespace Service
{
    public interface IDispatchWaybillHeaderService : IDisposable
    {
        DispatchWaybillHeader Create(DispatchWaybillHeader s);
        void Delete(int id);
        void Delete(DispatchWaybillHeader s);
        DispatchWaybillHeader GetDispatchWaybillHeader(int id);

        DispatchWaybillHeader Find(int id);
        IQueryable<DispatchWaybillHeaderViewModel> GetDispatchWaybillHeaderList(int DocTypeId, string Uname);
        IQueryable<DispatchWaybillHeaderViewModel> GetDispatchWaybillHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<DispatchWaybillHeaderViewModel> GetDispatchWaybillHeaderListPendingToReview(int id, string Uname);
        DispatchWaybillHeaderViewModel GetDispatchWaybillHeaderViewModel(int id);
        
        void Update(DispatchWaybillHeader s);
        string GetMaxDocNo();
        DispatchWaybillHeader FindByDocNo(string Docno, int DivisionId, int SiteId);
        int NextId(int id);
        int PrevId(int id);

       
    }
    public class DispatchWaybillHeaderService : IDispatchWaybillHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public DispatchWaybillHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public DispatchWaybillHeader Create(DispatchWaybillHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DispatchWaybillHeader>().Insert(s);
            return s;
        }

       public void Delete(int id)
     {
         _unitOfWork.Repository<DispatchWaybillHeader>().Delete(id);
     }
       public void Delete(DispatchWaybillHeader s)
        {
            _unitOfWork.Repository<DispatchWaybillHeader>().Delete(s);
        }
       public void Update(DispatchWaybillHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DispatchWaybillHeader>().Update(s);            
        }

       public DispatchWaybillHeader GetDispatchWaybillHeader(int id)
        {
            return _unitOfWork.Repository<DispatchWaybillHeader>().Query().Get().Where(m => m.DispatchWaybillHeaderId == id).FirstOrDefault();
        }

       public DispatchWaybillHeader Find(int id)
       {
           return _unitOfWork.Repository<DispatchWaybillHeader>().Find(id);
       }

       public DispatchWaybillHeader FindByDocNo(string Docno, int DivisionId, int SiteId)
       {
           return _unitOfWork.Repository<DispatchWaybillHeader>().Query().Get().Where(m => m.DocNo == Docno && m.DivisionId == DivisionId && m.SiteId == SiteId ).FirstOrDefault();

       }

       public int NextId(int id)
       {
           int temp = 0;
           if (id != 0)
           {

               temp = (from p in db.DispatchWaybillHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.DispatchWaybillHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


           }
           else
           {
               temp = (from p in db.DispatchWaybillHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.DispatchWaybillHeaderId).FirstOrDefault();
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

               temp = (from p in db.DispatchWaybillHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.DispatchWaybillHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
           }
           else
           {
               temp = (from p in db.DispatchWaybillHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.DispatchWaybillHeaderId).AsEnumerable().LastOrDefault();
           }
           if (temp != 0)
               return temp;
           else
               return id;
       }

       public string GetMaxDocNo()
       {
           int x;
           var maxVal = _unitOfWork.Repository<DispatchWaybillHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
           return (maxVal + 1).ToString();
       }

       public void Dispose()
       {
       }

       public IQueryable<DispatchWaybillHeaderViewModel> GetDispatchWaybillHeaderList(int DocTypeId, string Uname)
       {
           IQueryable<DispatchWaybillHeaderViewModel> DispatchWaybillHeader = from H in db.DispatchWaybillHeader
                                                                  join p in db.Persons on H.ConsigneeId equals p.PersonID into PersonTable
                                                                  from PersonTab in PersonTable.DefaultIfEmpty()
                                                                  join s in db.SaleInvoiceHeader on H.SaleInvoiceHeaderId equals s.SaleInvoiceHeaderId into SaleInvoiceHeaderTable
                                                                  from SaleInvoiceHeaderTab in SaleInvoiceHeaderTable.DefaultIfEmpty()
                                                                  where H.DocTypeId == DocTypeId
                                                                  orderby H.DispatchWaybillHeaderId
                                                                  select new DispatchWaybillHeaderViewModel
                                                                  {
                                                                      DispatchWaybillHeaderId = H.DispatchWaybillHeaderId,
                                                                      DocDate = H.DocDate,
                                                                      DocNo = H.DocNo,
                                                                      ConsigneeName = PersonTab.Name,
                                                                      SaleInvoiceHeaderDocNo = SaleInvoiceHeaderTab.DocNo,
                                                                      WaybillNo = H.WaybillNo,
                                                                      WaybillDate = H.WaybillDate,
                                                                      Remark = H.Remark,
                                                                      Status = H.Status,
                                                                      ModifiedBy = H.ModifiedBy,
                                                                      ReviewCount = H.ReviewCount,
                                                                      ReviewBy = H.ReviewBy,
                                                                      Reviewed = (SqlFunctions.CharIndex(Uname, H.ReviewBy) > 0),
                                                                  };

           return DispatchWaybillHeader;
       }

       public IQueryable<DispatchWaybillHeaderViewModel> GetDispatchWaybillHeaderListPendingToSubmit(int id, string Uname)
       {

           List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
           var LedgerHeader = GetDispatchWaybillHeaderList(id, Uname).AsQueryable();

           var PendingToSubmit = from p in LedgerHeader
                                 where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                 select p;
           return PendingToSubmit;

       }

       public IQueryable<DispatchWaybillHeaderViewModel> GetDispatchWaybillHeaderListPendingToReview(int id, string Uname)
       {

           List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
           var LedgerHeader = GetDispatchWaybillHeaderList(id, Uname).AsQueryable();

           var PendingToReview = from p in LedgerHeader
                                 where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                 select p;
           return PendingToReview;

       }



        public DispatchWaybillHeaderViewModel GetDispatchWaybillHeaderViewModel(int id)
        {
            DispatchWaybillHeaderViewModel packingheaderlistpendingtoapprove = (from H in db.DispatchWaybillHeader
                                                                          join B in db.Persons on H.ConsigneeId equals B.PersonID into ConsigneeTable
                                                                          from ConsigneeTab in ConsigneeTable.DefaultIfEmpty()
                                                                          orderby H.DocDate descending, H.DocNo descending
                                                                          where H.DispatchWaybillHeaderId == id
                                                                          select new DispatchWaybillHeaderViewModel
                                                                          {
                                                                              DispatchWaybillHeaderId = H.DispatchWaybillHeaderId,
                                                                              DocDate = H.DocDate,
                                                                              DocNo = H.DocNo,
                                                                              ConsigneeName = ConsigneeTab.Name,
                                                                              RouteId = H.RouteId,
                                                                              Remark = H.Remark,
                                                                              Status = H.Status,
                                                                              ModifiedBy = H.ModifiedBy,
                                                                          }).FirstOrDefault();

            return packingheaderlistpendingtoapprove;
        }
    }
}
