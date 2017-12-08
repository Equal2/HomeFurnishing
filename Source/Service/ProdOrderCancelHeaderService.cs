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
    public interface IProdOrderCancelHeaderService : IDisposable
    {
        ProdOrderCancelHeader Create(ProdOrderCancelHeader p);
        void Delete(int id);
        ProdOrderCancelHeader Find(int id);
        void Delete(ProdOrderCancelHeader p);
        IEnumerable<ProdOrderCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProdOrderCancelHeader p);
        ProdOrderCancelHeader Add(ProdOrderCancelHeader p);
        IQueryable<ProdOrderCancelHeaderViewModel> GetProdOrderCancelHeaderList(int id, string Uname);
        IQueryable<ProdOrderCancelHeaderViewModel> GetProdOrderCancelHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<ProdOrderCancelHeaderViewModel> GetProdOrderCancelHeaderListPendingToReview(int id, string Uname);
        ProdOrderCancelHeader FindByDocNo(string DocNo);
        string GetMaxDocNo();
        bool CheckForDocNoExists(string DocNo);
        int NextId(int id);
        int PrevId(int id);

        ProdOrderCancelHeader GetProdOrderCancelForMaterialPlan(int id);
    }

    public class ProdOrderCancelHeaderService : IProdOrderCancelHeaderService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProdOrderCancelHeader> _ProdOrderCancelHeaderRepository;
        RepositoryQuery<ProdOrderCancelHeader> ProdOrderCancelHeaderRepository;
        public ProdOrderCancelHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProdOrderCancelHeaderRepository = new Repository<ProdOrderCancelHeader>(db);
            ProdOrderCancelHeaderRepository = new RepositoryQuery<ProdOrderCancelHeader>(_ProdOrderCancelHeaderRepository);
        }

        public ProdOrderCancelHeader FindByDocNo(string DocNo)
        {
            return (from p in db.ProdOrderCancelHeader
                        where p.DocNo==DocNo
                        select p
                        ).FirstOrDefault();
        }

        public ProdOrderCancelHeader Find(int id)
        {
            return _unitOfWork.Repository<ProdOrderCancelHeader>().Find(id);
        }

        public ProdOrderCancelHeader Create(ProdOrderCancelHeader p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProdOrderCancelHeader>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProdOrderCancelHeader>().Delete(id);
        }

        public void Delete(ProdOrderCancelHeader p)
        {
            _unitOfWork.Repository<ProdOrderCancelHeader>().Delete(p);
        }

        public void Update(ProdOrderCancelHeader p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProdOrderCancelHeader>().Update(p);
        }

        public IEnumerable<ProdOrderCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProdOrderCancelHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProdOrderCancelHeaderId))
                .Filter(q => !string.IsNullOrEmpty(q.DocNo ))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IQueryable<ProdOrderCancelHeaderViewModel> GetProdOrderCancelHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var temp = (from p in db.ProdOrderCancelHeader
                        join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                        from tab in table.DefaultIfEmpty()                        
                        where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == id
                        orderby p.DocDate descending, p.DocNo descending
                        select new ProdOrderCancelHeaderViewModel
                        {
                            DocDate = p.DocDate,
                            DocNo = p.DocNo,
                            DocTypeName = tab.DocumentTypeName,
                            ProdOrderCancelHeaderId = p.ProdOrderCancelHeaderId,
                            Remark = p.Remark,
                            Status = p.Status,
                            ModifiedBy = p.ModifiedBy,
                            ReviewCount = p.ReviewCount,
                            ReviewBy = p.ReviewBy,
                            Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        }
                       );

            return temp;
        }

        public IQueryable<ProdOrderCancelHeaderViewModel> GetProdOrderCancelHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetProdOrderCancelHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<ProdOrderCancelHeaderViewModel> GetProdOrderCancelHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetProdOrderCancelHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public ProdOrderCancelHeader Add(ProdOrderCancelHeader p)
        {
            _unitOfWork.Repository<ProdOrderCancelHeader>().Insert(p);
            return p;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProdOrderCancelHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProdOrderCancelHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<ProdOrderCancelHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }
        public bool CheckForDocNoExists(string DocNo)
        {
            var temp = (from p in db.ProdOrderCancelHeader
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

                temp = (from p in db.ProdOrderCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.ProdOrderCancelHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in db.ProdOrderCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.ProdOrderCancelHeaderId).FirstOrDefault();
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

                temp = (from p in db.ProdOrderCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.ProdOrderCancelHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProdOrderCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.ProdOrderCancelHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ProdOrderCancelHeader GetProdOrderCancelForMaterialPlan(int id)
        {
            return (from p in db.ProdOrderCancelHeader
                    where p.MaterialPlanCancelHeaderId == id
                    select p
                        ).FirstOrDefault();
        }


    }
}
