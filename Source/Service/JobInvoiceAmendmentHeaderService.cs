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
    public interface IJobInvoiceAmendmentHeaderService : IDisposable
    {
        JobInvoiceAmendmentHeader Create(JobInvoiceAmendmentHeader pt);
        void Delete(int id);
        void Delete(JobInvoiceAmendmentHeader pt);
        JobInvoiceAmendmentHeader Find(int id);
        IEnumerable<JobInvoiceAmendmentHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobInvoiceAmendmentHeader pt);
        JobInvoiceAmendmentHeader Add(JobInvoiceAmendmentHeader pt);
        IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> GetJobInvoiceAmendmentHeaderList(int id, string Uname);
        IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> GetJobInvoiceAmendmentHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> GetJobInvoiceAmendmentHeaderListPendingToReview(int id, string Uname); 
        Task<IEquatable<JobInvoiceAmendmentHeader>> GetAsync();
        Task<JobInvoiceAmendmentHeader> FindAsync(int id);
        JobInvoiceAmendmentHeader GetJobInvoiceAmendmentHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
    }

    public class JobInvoiceAmendmentHeaderService : IJobInvoiceAmendmentHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobInvoiceAmendmentHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public JobInvoiceAmendmentHeader GetJobInvoiceAmendmentHeaderByName(string terms)
        {
            return (from p in db.JobInvoiceAmendmentHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public JobInvoiceAmendmentHeader Find(int id)
        {
            return _unitOfWork.Repository<JobInvoiceAmendmentHeader>().Find(id);
        }

        public JobInvoiceAmendmentHeader Create(JobInvoiceAmendmentHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobInvoiceAmendmentHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobInvoiceAmendmentHeader>().Delete(id);
        }

        public void Delete(JobInvoiceAmendmentHeader pt)
        {
            _unitOfWork.Repository<JobInvoiceAmendmentHeader>().Delete(pt);
        }

        public void Update(JobInvoiceAmendmentHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobInvoiceAmendmentHeader>().Update(pt);
        }

        public IEnumerable<JobInvoiceAmendmentHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobInvoiceAmendmentHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> GetJobInvoiceAmendmentHeaderList(int id,string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            return (from p in db.JobInvoiceAmendmentHeader
                    orderby p.DocNo descending, p.DocDate descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                    select new JobInvoiceAmendmentHeaderIndexViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        JobInvoiceAmendmentHeaderId = p.JobInvoiceAmendmentHeaderId,
                        ProcessName=p.Process.ProcessName,
                        OrderByName=p.OrderBy.Person.Name,
                        Remark = p.Remark,
                        Status=p.Status,
                        JobWorkerName = p.JobWorker.Person.Name,
                        ModifiedBy=p.ModifiedBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) == 1),
                        ReviewCount=p.ReviewCount,
                        ReviewBy=p.ReviewBy,
                    }
                        );
        }

        public JobInvoiceAmendmentHeader Add(JobInvoiceAmendmentHeader pt)
        {
            _unitOfWork.Repository<JobInvoiceAmendmentHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobInvoiceAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobInvoiceAmendmentHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobInvoiceAmendmentHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobInvoiceAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobInvoiceAmendmentHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobInvoiceAmendmentHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<JobInvoiceAmendmentHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }


        public IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> GetJobInvoiceAmendmentHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobInvoiceHeader = GetJobInvoiceAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobInvoiceHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> GetJobInvoiceAmendmentHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobInvoiceHeader = GetJobInvoiceAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobInvoiceHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobInvoiceAmendmentHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceAmendmentHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
