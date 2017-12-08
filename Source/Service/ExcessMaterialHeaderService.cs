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
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using Model.ViewModel;
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IExcessMaterialHeaderService : IDisposable
    {
        ExcessMaterialHeader Create(ExcessMaterialHeader s);
        void Delete(int id);
        void Delete(ExcessMaterialHeader s);
        ExcessMaterialHeader Find(int id);
        void Update(ExcessMaterialHeader s);
        string GetMaxDocNo();
        ExcessMaterialHeader FindByDocNo(string Docno);
        IQueryable<ExcessMaterialHeaderViewModel> GetExcessMaterialHeaderList(int DocTypeId, string UName);
        IQueryable<ExcessMaterialHeaderViewModel> GetExcessMaterialHeaderListPendingToSubmit(int DocTypeId, string UName);
        IQueryable<ExcessMaterialHeaderViewModel> GetExcessMaterialHeaderListPendingToReview(int DocTypeId, string UName);
        ExcessMaterialHeaderViewModel GetExcessMaterialHeader(int id);

        void UpdateExcessMaterialHeader(ExcessMaterialHeaderViewModel S);
        string GetPersonName(int id);
    }
    public class ExcessMaterialHeaderService : IExcessMaterialHeaderService
    {

        private readonly ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public ExcessMaterialHeaderService(IUnitOfWorkForService unit,ApplicationDbContext Context)
        {
            _unitOfWork = unit;
            db = Context;
        }

        public ExcessMaterialHeader Create(ExcessMaterialHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ExcessMaterialHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ExcessMaterialHeader>().Delete(id);
        }


        public void Delete(int id, ApplicationDbContext Context)
        {
            //_unitOfWork.Repository<ExcessMaterialHeader>().Delete(id);
            ExcessMaterialHeader ExcessMaterialheader = (from H in db.ExcessMaterialHeader where H.ExcessMaterialHeaderId == id select H).FirstOrDefault();
            ExcessMaterialheader.ObjectState = Model.ObjectState.Deleted;
            db.ExcessMaterialHeader.Attach(ExcessMaterialheader);
            db.ExcessMaterialHeader.Remove(ExcessMaterialheader);
        }

        public void Delete(ExcessMaterialHeader s)
        {
            _unitOfWork.Repository<ExcessMaterialHeader>().Delete(s);
        }
        public void Update(ExcessMaterialHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ExcessMaterialHeader>().Update(s);
        }
        public ExcessMaterialHeader Find(int id)
        {
            return _unitOfWork.Repository<ExcessMaterialHeader>().Find(id);
        }


        public ExcessMaterialHeader FindByDocNo(string Docno)
        {
            return _unitOfWork.Repository<ExcessMaterialHeader>().Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

        }     

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<ExcessMaterialHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<ExcessMaterialHeaderViewModel> GetExcessMaterialHeaderList(int DocTypeId, string UName)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.ExcessMaterialHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == DocTypeId
                    select new ExcessMaterialHeaderViewModel
                    {
                        CurrencyName = p.Currency.Name,
                        DivisionName = p.Division.DivisionName,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        GodownName = p.Godown.GodownName,
                        PersonName = p.Person.Name,
                        ProcessName = p.Process.ProcessName,
                        Remark = p.Remark,
                        Status = p.Status,
                        ExcessMaterialHeaderId = p.ExcessMaterialHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(UName, p.ReviewBy) > 0),
                    }
                        );

        }

        public IQueryable<ExcessMaterialHeaderViewModel> GetExcessMaterialHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var ExcessMaterialHeader = GetExcessMaterialHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in ExcessMaterialHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<ExcessMaterialHeaderViewModel> GetExcessMaterialHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var ExcessMaterialHeader = GetExcessMaterialHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in ExcessMaterialHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public ExcessMaterialHeaderViewModel GetExcessMaterialHeader(int id)
        {

            return (from p in db.ExcessMaterialHeader
                    where p.ExcessMaterialHeaderId == id
                    select new ExcessMaterialHeaderViewModel
                    {
                        CurrencyId = p.CurrencyId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        SiteId = p.SiteId,
                        DivisionId = p.DivisionId,
                        GodownId = p.GodownId,
                        PersonId = p.PersonId,
                        ProcessId = p.ProcessId,
                        Remark = p.Remark,
                        Status = p.Status,
                        ExcessMaterialHeaderId = p.ExcessMaterialHeaderId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                    }
                        ).FirstOrDefault();

        }

        public void UpdateExcessMaterialHeader(ExcessMaterialHeaderViewModel S)
        {
            ExcessMaterialHeader ExcessMaterialHeader = Find(S.ExcessMaterialHeaderId);

            ExcessMaterialHeader.DocTypeId = S.DocTypeId;
            ExcessMaterialHeader.DocDate = S.DocDate;
            ExcessMaterialHeader.DocNo = S.DocNo;
            ExcessMaterialHeader.DivisionId = S.DivisionId;
            ExcessMaterialHeader.SiteId = S.SiteId;
            ExcessMaterialHeader.CurrencyId = S.CurrencyId;
            ExcessMaterialHeader.PersonId = S.PersonId;
            ExcessMaterialHeader.ProcessId = S.ProcessId;
            ExcessMaterialHeader.GodownId = S.GodownId;
            ExcessMaterialHeader.Remark = S.Remark;
            ExcessMaterialHeader.Status = S.Status;
            ExcessMaterialHeader.ModifiedBy = S.ModifiedBy;
            ExcessMaterialHeader.ModifiedDate = S.ModifiedDate;


            Update(ExcessMaterialHeader);

        }

        public string GetPersonName(int id)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = (from p in db.MaterialIssueSettings
                            where p.DocTypeId == id && p.SiteId == SiteId && p.DivisionId == DivisionId
                            select p).FirstOrDefault();

            return Settings != null ? (string.IsNullOrEmpty(Settings.PersonFieldHeading) ? "Person" : Settings.PersonFieldHeading) : "Person";


        }

        public void Dispose()
        {
        }
    }
}
