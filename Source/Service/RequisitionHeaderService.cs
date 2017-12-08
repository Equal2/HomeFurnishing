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
using Model.ViewModel;
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IRequisitionHeaderService : IDisposable
    {
        RequisitionHeader Create(RequisitionHeader s);
        void Delete(int id);
        void Delete(RequisitionHeader s);
        RequisitionHeaderViewModel GetRequisitionHeader(int id);
        RequisitionHeader Find(int id);
        IQueryable<RequisitionHeaderViewModel> GetRequisitionHeaderList(int DocTypeId, string UName);
        IQueryable<RequisitionHeaderViewModel> GetRequisitionHeaderListPendingToSubmit(int DocTypeId, string UName);
        IQueryable<RequisitionHeaderViewModel> GetRequisitionHeaderListPendingToReview(int DocTypeId, string UName);
        void Update(RequisitionHeader s);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetRequisitionsForExchange(int id, string term);
        IQueryable<ComboBoxResult> GetCostCentersForExchange(int id, string term);
        IQueryable<ComboBoxResult> GetProductsForExchange(int id, string term);
        IQueryable<ComboBoxResult> GetDimension2ForExchange(int id, string term);
        IQueryable<ComboBoxResult> GetDimension1ForExchange(int id, string term);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);

        /// <summary>
        ///Get the Purchase Indent based on the materialplan headerid
        /// </summary>
        /// <param name="id">MaterialPlanHeaderId</param>          

    }
    public class RequisitionHeaderService : IRequisitionHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public RequisitionHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public RequisitionHeader Create(RequisitionHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RequisitionHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RequisitionHeader>().Delete(id);
        }
        public void Delete(RequisitionHeader s)
        {
            _unitOfWork.Repository<RequisitionHeader>().Delete(s);
        }
        public void Update(RequisitionHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RequisitionHeader>().Update(s);
        }


        public RequisitionHeader Find(int id)
        {
            return _unitOfWork.Repository<RequisitionHeader>().Find(id);
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<RequisitionHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<RequisitionHeaderViewModel> GetRequisitionHeaderList(int DocTypeId, string UName)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.RequisitionHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId==SiteId && p.DivisionId==DivisionId && p.DocTypeId==DocTypeId
                    select new RequisitionHeaderViewModel
                    {

                        DocDate=p.DocDate,
                        DocNo=p.DocNo,                                            
                        Remark=p.Remark,
                        Status=p.Status,
                        RequisitionHeaderId=p.RequisitionHeaderId,
                        ModifiedBy=p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(UName, p.ReviewBy) > 0),
                    });
        }

        public IQueryable<RequisitionHeaderViewModel> GetRequisitionHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var StockHeader = GetRequisitionHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in StockHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<RequisitionHeaderViewModel> GetRequisitionHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var StockHeader = GetRequisitionHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in StockHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public RequisitionHeaderViewModel GetRequisitionHeader(int id)
        {
            return (from p in db.RequisitionHeader
                    where p.RequisitionHeaderId ==id
                    select new RequisitionHeaderViewModel
                    {                        
                        DocTypeName=p.DocType.DocumentTypeName,
                        DocDate=p.DocDate,
                        DocNo=p.DocNo,                        
                        Remark=p.Remark,
                        RequisitionHeaderId=p.RequisitionHeaderId,
                        Status=p.Status,
                        PersonId=p.PersonId,
                        DocTypeId=p.DocTypeId,
                        SiteId=p.SiteId,
                        DivisionId=p.DivisionId,
                        CostCenterId=p.CostCenterId,
                        ReasonId=p.ReasonId,
                        ModifiedBy=p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                        CreatedBy=p.CreatedBy,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public IQueryable<ComboBoxResult> GetRequisitionsForExchange(int id, string term)
        {
            return (from p in db.RequisitionHeader
                    where p.PersonId == id && (string.IsNullOrEmpty(term)?1==1 : p.DocNo.ToLower().Contains(term.ToLower()))
                    orderby p.DocNo
                    select new ComboBoxResult
                    {
                        text = p.DocNo,
                        id = p.RequisitionHeaderId.ToString(),
                    });
        }

        public IQueryable<ComboBoxResult> GetCostCentersForExchange(int id, string term)
        {

            return (from p in db.RequisitionHeader
                    where p.PersonId == id && (string.IsNullOrEmpty(term) ? 1 == 1 : p.CostCenter.CostCenterName.ToLower().Contains(term.ToLower())) && p.CostCenter.IsActive == true && p.CostCenter.Status != (int)StatusConstants.Closed
                    group p by p.CostCenterId into g
                    orderby g.Max(m=>m.CostCenter.CostCenterName)
                    select new ComboBoxResult
                    {
                        text = g.Max(m=>m.CostCenter.CostCenterName),
                        id = g.Key.Value.ToString(),
                    });

        }

        public IQueryable<ComboBoxResult> GetProductsForExchange(int id, string term)
        {

            return (from p in db.RequisitionLine
                    join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                    where t.PersonId == id && (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower()))                    
                    group p by p.ProductId into g
                    orderby g.Max(m => m.Product.ProductName)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.Product.ProductName),
                        id = g.Key.ToString(),
                    });

        }

        public IQueryable<ComboBoxResult> GetDimension1ForExchange(int id, string term)
        {

            return (from p in db.RequisitionLine
                    join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                    where t.PersonId == id && (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()))                    
                    group p by p.Dimension1Id into g
                    orderby g.Max(m => m.Dimension1.Dimension1Name)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.Dimension1.Dimension1Name),
                        id = g.Key.ToString(),
                    });

        }

        public IQueryable<ComboBoxResult> GetDimension2ForExchange(int id, string term)
        {

            return (from p in db.RequisitionLine
                    join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                    where t.PersonId == id && (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower()))                    
                    group p by p.Dimension2Id into g
                    orderby g.Max(m => m.Dimension2.Dimension2Name)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.Dimension2.Dimension2Name),
                        id = g.Key.ToString(),
                    });

        }


        public IQueryable<ComboBoxResult> GetDimension3ForExchange(int id, string term)
        {

            return (from p in db.RequisitionLine
                    join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                    where t.PersonId == id && (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower()))
                    group p by p.Dimension3Id into g
                    orderby g.Max(m => m.Dimension3.Dimension3Name)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.Dimension3.Dimension3Name),
                        id = g.Key.ToString(),
                    });

        }


        public IQueryable<ComboBoxResult> GetDimension4ForExchange(int id, string term)
        {

            return (from p in db.RequisitionLine
                    join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                    where t.PersonId == id && (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower()))
                    group p by p.Dimension4Id into g
                    orderby g.Max(m => m.Dimension4.Dimension4Name)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.Dimension4.Dimension4Name),
                        id = g.Key.ToString(),
                    });

        }


        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new MaterialRequestSettingsService(_unitOfWork).GetMaterialRequestSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] PersonRoles = null;
            if (!string.IsNullOrEmpty(settings.filterPersonRoles)) { PersonRoles = settings.filterPersonRoles.Split(",".ToCharArray()); }
            else { PersonRoles = new string[] { "NA" }; }

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            var list = (from p in db.Persons
                        join bus in db.BusinessEntity on p.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where (settings.ProcessId == null ? 1 == 1 : PersonProcessTab.ProcessId == settings.ProcessId)
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.Name.ToLower().Contains(term.ToLower()) || p.Code.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterPersonRoles) ? 1 == 1 : PersonRoles.Contains(PersonRoleTab.RoleDocTypeId.ToString()))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivIdStr) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteIdStr) != -1
                        && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                        group new { p } by new { p.PersonID } into Result
                        orderby Result.Max(m => m.p.Name)
                        select new ComboBoxResult
                        {
                            id = Result.Key.PersonID.ToString(),
                            text = Result.Max(m => m.p.Name + ", " + m.p.Suffix + " [" + m.p.Code + "]"),
                        }
              );

            return list;
        }

        public void Dispose()
        {
        }
    }
}
