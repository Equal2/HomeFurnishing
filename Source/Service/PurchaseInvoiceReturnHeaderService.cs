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
    public interface IPurchaseInvoiceReturnHeaderService : IDisposable
    {
        PurchaseInvoiceReturnHeader Create(PurchaseInvoiceReturnHeader pt);
        void Delete(int id);
        void Delete(PurchaseInvoiceReturnHeader pt);
        PurchaseInvoiceReturnHeader Find(string Name);
        PurchaseInvoiceReturnHeader Find(int id);
        
        IEnumerable<PurchaseInvoiceReturnHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseInvoiceReturnHeader pt);
        PurchaseInvoiceReturnHeader Add(PurchaseInvoiceReturnHeader pt);
        PurchaseInvoiceReturnHeaderViewModel GetPurchaseInvoiceReturnHeader(int id);//HeadeRId
        IQueryable<PurchaseInvoiceReturnHeaderViewModel> GetPurchaseInvoiceReturnHeaderList(int id, string Uname);
        IQueryable<PurchaseInvoiceReturnHeaderViewModel> GetPurchaseInvoiceReturnPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseInvoiceReturnHeaderViewModel> GetPurchaseInvoiceReturnPendingToReview(int id, string Uname);
        Task<IEquatable<PurchaseInvoiceReturnHeader>> GetAsync();
        Task<PurchaseInvoiceReturnHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
    }

    public class PurchaseInvoiceReturnHeaderService : IPurchaseInvoiceReturnHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseInvoiceReturnHeader> _PurchaseInvoiceReturnHeaderRepository;
        RepositoryQuery<PurchaseInvoiceReturnHeader> PurchaseInvoiceReturnHeaderRepository;
        public PurchaseInvoiceReturnHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseInvoiceReturnHeaderRepository = new Repository<PurchaseInvoiceReturnHeader>(db);
            PurchaseInvoiceReturnHeaderRepository = new RepositoryQuery<PurchaseInvoiceReturnHeader>(_PurchaseInvoiceReturnHeaderRepository);
        }

        public PurchaseInvoiceReturnHeader Find(string Name)
        {
            return PurchaseInvoiceReturnHeaderRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public PurchaseInvoiceReturnHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseInvoiceReturnHeader>().Find(id);
        }       

        public PurchaseInvoiceReturnHeader Create(PurchaseInvoiceReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseInvoiceReturnHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseInvoiceReturnHeader>().Delete(id);
        }

        public void Delete(PurchaseInvoiceReturnHeader pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceReturnHeader>().Delete(pt);
        }

        public void Update(PurchaseInvoiceReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseInvoiceReturnHeader>().Update(pt);
        }

        public IEnumerable<PurchaseInvoiceReturnHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseInvoiceReturnHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public PurchaseInvoiceReturnHeaderViewModel GetPurchaseInvoiceReturnHeader(int id)
        {
            return (from p in db.PurchaseInvoiceReturnHeader
                    where p.PurchaseInvoiceReturnHeaderId == id
                    select new PurchaseInvoiceReturnHeaderViewModel
                    {
                        PurchaseInvoiceReturnHeaderId = p.PurchaseInvoiceReturnHeaderId,
                        DivisionId = p.DivisionId,
                        DocNo = p.DocNo,
                        DocDate = p.DocDate,
                        DocTypeId = p.DocTypeId,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        PurchaseGoodsReturnHeaderId=p.PurchaseGoodsReturnHeaderId,
                        Status = p.Status,
                        SupplierId = p.SupplierId,                        
                        SalesTaxGroupId=p.SalesTaxGroupId,
                        CurrencyId=p.CurrencyId,
                        ReasonId=p.ReasonId,
                        ModifiedBy=p.ModifiedBy,
                        GatePassHeaderId = p.PurchaseGoodsReturnHeader.GatePassHeaderId,
                        GatePassDocNo = p.PurchaseGoodsReturnHeader.GatePassHeader.DocNo,
                        GatePassStatus = (p.PurchaseGoodsReturnHeader.GatePassHeader == null ? 0 : p.PurchaseGoodsReturnHeader.GatePassHeader.Status),
                        GatePassDocDate = p.PurchaseGoodsReturnHeader.GatePassHeader.DocDate,
                        CreatedDate=p.CreatedDate,
                        LockReason=p.LockReason,
                    }

                        ).FirstOrDefault();
        }
        public IQueryable<PurchaseInvoiceReturnHeaderViewModel> GetPurchaseInvoiceReturnHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            var pt = (from p in db.PurchaseInvoiceReturnHeader
                      join t3 in db._Users on p.ModifiedBy equals t3.UserName into table3
                      from tab3 in table3.DefaultIfEmpty()
                      join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                      from tab in table.DefaultIfEmpty()
                      join t1 in db.Persons on p.SupplierId equals t1.PersonID into table2
                      from tab2 in table2.DefaultIfEmpty()
                      orderby p.DocDate descending, p.DocNo descending
                      where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                      select new PurchaseInvoiceReturnHeaderViewModel
                      {
                          DocDate = p.DocDate,
                          DocNo = p.DocNo,
                          DocTypeName = tab.DocumentTypeName,
                          PurchaseInvoiceReturnHeaderId = p.PurchaseInvoiceReturnHeaderId,
                          Remark = p.Remark,
                          Status = p.Status,
                          SupplierName = tab2.Name,
                          ModifiedBy = p.ModifiedBy,
                          FirstName = tab3.FirstName,
                          ReviewCount = p.ReviewCount,
                          ReviewBy = p.ReviewBy,
                          Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                          GatePassDocNo = p.PurchaseGoodsReturnHeader.GatePassHeader.DocNo,
                          GatePassHeaderId = p.PurchaseGoodsReturnHeader.GatePassHeaderId,
                          GatePassDocDate = p.PurchaseGoodsReturnHeader.GatePassHeader.DocDate,
                          GatePassStatus = (p.PurchaseGoodsReturnHeader.GatePassHeaderId != null ? p.PurchaseGoodsReturnHeader.GatePassHeader.Status : 0),
                      }
                         );
            return pt;
        }

        public PurchaseInvoiceReturnHeader Add(PurchaseInvoiceReturnHeader pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceReturnHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceReturnHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceReturnHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceReturnHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceReturnHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseInvoiceReturnHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<PurchaseInvoiceReturnHeaderViewModel> GetPurchaseInvoiceReturnPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseInvoiceReturnHeader = GetPurchaseInvoiceReturnHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseInvoiceReturnHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseInvoiceReturnHeaderViewModel> GetPurchaseInvoiceReturnPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseInvoiceReturnHeader = GetPurchaseInvoiceReturnHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseInvoiceReturnHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseInvoiceReturnHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseInvoiceReturnHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
