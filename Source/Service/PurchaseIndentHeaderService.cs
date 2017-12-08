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
    public interface IPurchaseIndentHeaderService : IDisposable
    {
        PurchaseIndentHeader Create(PurchaseIndentHeader s);
        void Delete(int id);
        void Delete(PurchaseIndentHeader s);
        PurchaseIndentHeaderViewModel GetPurchaseIndentHeader(int id);
        PurchaseIndentHeader Find(int id);
        IQueryable<PurchaseIndentHeaderViewModel> GetPurchaseIndentHeaderList(int id,string name);
        IQueryable<PurchaseIndentHeaderViewModel> GetPurchaseIndentPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseIndentHeaderViewModel> GetPurchaseIndentPendingToReview(int id, string Uname);
        void Update(PurchaseIndentHeader s);
        string GetMaxDocNo();
        IEnumerable<PurchaseIndentHeaderListViewModel> GetPendingPurchaseIndents(int id, int PurchaseOrderHeaderId);//ProductId

        /// <summary>
        ///Get the Purchase Indent based on the materialplan headerid
        /// </summary>
        /// <param name="id">MaterialPlanHeaderId</param>  
        PurchaseIndentHeader GetPurchaseIndentForMaterialPlan(int id);

    }
    public class PurchaseIndentHeaderService : IPurchaseIndentHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public PurchaseIndentHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public PurchaseIndentHeader Create(PurchaseIndentHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseIndentHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseIndentHeader>().Delete(id);
        }
        public void Delete(PurchaseIndentHeader s)
        {
            _unitOfWork.Repository<PurchaseIndentHeader>().Delete(s);
        }
        public void Update(PurchaseIndentHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseIndentHeader>().Update(s);
        }


        public PurchaseIndentHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseIndentHeader>().Find(id);
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseIndentHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<PurchaseIndentHeaderViewModel> GetPurchaseIndentHeaderList(int id,string name)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.PurchaseIndentHeader
                    join t in db._Users on p.ModifiedBy equals t.UserName into table
                    from tab in table.DefaultIfEmpty()
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id && p.DocTypeId == id
                    select new PurchaseIndentHeaderViewModel
                    {

                        DocDate=p.DocDate,
                        DocNo=p.DocNo,
                        DepartmentName=p.Department.DepartmentName,                        
                        Remark=p.Remark,
                        Status=p.Status,
                        PurchaseIndentHeaderId=p.PurchaseIndentHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        FirstName = tab.FirstName,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(name, p.ReviewBy) > 0),

                    });
        }

        public PurchaseIndentHeaderViewModel GetPurchaseIndentHeader(int id)
        {
            return (from p in db.PurchaseIndentHeader
                    where p.PurchaseIndentHeaderId ==id
                    select new PurchaseIndentHeaderViewModel
                    {
                        DepartmentId=p.DepartmentId,
                        DocTypeId = p.DocTypeId,
                        DocumentTypeName=p.DocType.DocumentTypeName,                        
                        DocDate=p.DocDate,
                        DocNo=p.DocNo,
                        DepartmentName=p.Department.DepartmentName,                        
                        Remark=p.Remark,
                        ReasonId=p.ReasonId,
                        PurchaseIndentHeaderId=p.PurchaseIndentHeaderId,
                        Status=p.Status,
                        SiteId=p.SiteId,
                        DivisionId=p.DivisionId,
                        ModifiedBy=p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                        CreatedBy=p.CreatedBy,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<PurchaseIndentHeaderListViewModel> GetPendingPurchaseIndents(int id, int PurchaseOrderHeaderId)//Product Id
        {


            var PurchaseORder = new PurchaseOrderHeaderService(_unitOfWork).Find(PurchaseOrderHeaderId);

            var Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseORder.DocTypeId, PurchaseORder.DivisionId, PurchaseORder.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(Settings.filterContraSites)) { contraSites = Settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions)) { contraDivisions = Settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            return (from p in db.ViewPurchaseIndentBalance
                    join t in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t.PurchaseIndentLineId into table from tab in table.DefaultIfEmpty()
                    where p.ProductId == id && p.BalanceQty>0
                    && (string.IsNullOrEmpty(Settings.filterContraSites) ? p.SiteId==CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(Settings.filterContraDivisions) ? p.DivisionId==CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    orderby p.PurchaseIndentNo
                    select new PurchaseIndentHeaderListViewModel
                    {
                        DocNo = p.PurchaseIndentNo,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,                        
                        Dimension1Name=tab.Dimension1.Dimension1Name,
                        Dimension2Name=tab.Dimension2.Dimension2Name,
                    }
                        );
        }

        public PurchaseIndentHeader GetPurchaseIndentForMaterialPlan(int id)
        {

            return (from p in db.MaterialPlanLine
                    join t in db.PurchaseIndentLine on p.MaterialPlanLineId equals t.MaterialPlanLineId
                    join t1 in db.PurchaseIndentHeader on t.PurchaseIndentHeaderId equals t1.PurchaseIndentHeaderId
                    where p.MaterialPlanHeaderId == id
                    select t1
                ).FirstOrDefault();

        }
        public IEnumerable<PurchaseIndentHeader> GetPurchaseIndentListForMAterialPlan(int id)
        {
            return (from p in db.PurchaseIndentHeader
                    where p.MaterialPlanHeaderId == id
                    select p
                       );
        }

        public IQueryable<PurchaseIndentHeaderViewModel> GetPurchaseIndentPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseIndentHeader = GetPurchaseIndentHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseIndentHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseIndentHeaderViewModel> GetPurchaseIndentPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseIndentHeader = GetPurchaseIndentHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseIndentHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public void Dispose()
        {
        }
    }
}
