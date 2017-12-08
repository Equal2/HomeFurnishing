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
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using Model.ViewModel;
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IPurchaseIndentCancelHeaderService : IDisposable
    {
        PurchaseIndentCancelHeader Create(PurchaseIndentCancelHeader p);
        void Delete(int id);
        PurchaseIndentCancelHeader Find(int id);
        void Delete(PurchaseIndentCancelHeader p);
        IEnumerable<PurchaseIndentCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseIndentCancelHeader p);
        PurchaseIndentCancelHeader Add(PurchaseIndentCancelHeader p);
        IQueryable<PurchaseIndentCancelHeaderViewModel> GetPurchaseIndentCancelHeaderList(int id, string Uname);
        IQueryable<PurchaseIndentCancelHeaderViewModel> GetPurchaseIndentCancelPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseIndentCancelHeaderViewModel> GetPurchaseIndentCancelPendingToReview(int id, string Uname);
        PurchaseIndentCancelHeader FindByDocNo(string DocNo);
        string GetMaxDocNo();
        bool CheckForDocNoExists(string DocNo);
        int NextId(int id);
        int PrevId(int id);

        PurchaseIndentCancelHeader GetPurchaseIndentCancelForMaterialPlan(int id);
    }

    public class PurchaseIndentCancelHeaderService : IPurchaseIndentCancelHeaderService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseIndentCancelHeader> _PurchaseIndentCancelHeaderRepository;
        RepositoryQuery<PurchaseIndentCancelHeader> PurchaseIndentCancelHeaderRepository;
        public PurchaseIndentCancelHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseIndentCancelHeaderRepository = new Repository<PurchaseIndentCancelHeader>(db);
            PurchaseIndentCancelHeaderRepository = new RepositoryQuery<PurchaseIndentCancelHeader>(_PurchaseIndentCancelHeaderRepository);
        }

        public PurchaseIndentCancelHeader FindByDocNo(string DocNo)
        {
            return (from p in db.PurchaseIndentCancelHeader
                        where p.DocNo==DocNo
                        select p
                        ).FirstOrDefault();
        }

        public PurchaseIndentCancelHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseIndentCancelHeader>().Find(id);
        }

        public PurchaseIndentCancelHeader Create(PurchaseIndentCancelHeader p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseIndentCancelHeader>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseIndentCancelHeader>().Delete(id);
        }

        public void Delete(PurchaseIndentCancelHeader p)
        {
            _unitOfWork.Repository<PurchaseIndentCancelHeader>().Delete(p);
        }

        public void Update(PurchaseIndentCancelHeader p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseIndentCancelHeader>().Update(p);
        }

        public IEnumerable<PurchaseIndentCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseIndentCancelHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseIndentCancelHeaderId))
                .Filter(q => !string.IsNullOrEmpty(q.DocNo ))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IQueryable<PurchaseIndentCancelHeaderViewModel> GetPurchaseIndentCancelHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            var temp = (from p in db.PurchaseIndentCancelHeader
                        join t in db._Users on p.ModifiedBy equals t.UserName into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                        from tab in table.DefaultIfEmpty()
                        orderby p.DocDate descending, p.DocNo descending
                        where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == id
                        select new PurchaseIndentCancelHeaderViewModel
                        {
                            DocDate = p.DocDate,
                            DocNo = p.DocNo,
                            DocTypeName = tab.DocumentTypeName,
                            PurchaseIndentCancelHeaderId = p.PurchaseIndentCancelHeaderId,
                            Remark = p.Remark,
                            Status = p.Status,
                            ModifiedBy = p.ModifiedBy,
                            FirstName = tab1.FirstName,
                            ReviewCount = p.ReviewCount,
                            ReviewBy = p.ReviewBy,
                            Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        }
                       );

            return temp;
            
            
        }

        public PurchaseIndentCancelHeader Add(PurchaseIndentCancelHeader p)
        {
            _unitOfWork.Repository<PurchaseIndentCancelHeader>().Insert(p);
            return p;
        }

        public IQueryable<PurchaseIndentCancelHeaderViewModel> GetPurchaseIndentCancelPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseIndentCancelHeader = GetPurchaseIndentCancelHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseIndentCancelHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseIndentCancelHeaderViewModel> GetPurchaseIndentCancelPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseIndentCancelHeader = GetPurchaseIndentCancelHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseIndentCancelHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseIndentCancelHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseIndentCancelHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseIndentCancelHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }
        public bool CheckForDocNoExists(string DocNo)
        {
            var temp = (from p in db.PurchaseIndentCancelHeader
                        where p.DocNo == DocNo
                        select p).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.PurchaseIndentCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseIndentCancelHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in db.PurchaseIndentCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseIndentCancelHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseIndentCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseIndentCancelHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseIndentCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseIndentCancelHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public PurchaseIndentCancelHeader GetPurchaseIndentCancelForMaterialPlan(int id)
        {
            return (from p in db.PurchaseIndentCancelHeader
                    where p.MaterialPlanCancelHeaderId == id
                    select p
                        ).FirstOrDefault();
        }
    }
}
