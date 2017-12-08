using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.SqlServer;
using Data.Models;
using Data.Infrastructure;
using Model.ViewModels;
using Model;
using Model.Models;
using Core.Common;

namespace Service
{
    public interface IOverTimeApplicationHeaderService : IDisposable
    {
        OverTimeApplicationHeader Create(OverTimeApplicationHeader pt);
        void Delete(int id);
        void Delete(OverTimeApplicationHeader pt);
        OverTimeApplicationHeader Find(int id);
        OverTimeApplicationHeader Find(string DocNo);
        IEnumerable<OverTimeApplicationHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(OverTimeApplicationHeader pt);
        OverTimeApplicationHeader Add(OverTimeApplicationHeader pt);
        IQueryable<OverTimeApplicationHeaderViewModel> GetOverTimeApplicationHeaderList(int id, string Uname);

        AttendanceLine FindLine(int id);
       // IEnumerable<AttendanceLinesViewModel> GetAttendanceLineView(int id);
        OverTimeApplicationHeaderViewModel GetOverTimeApplicationHeader(int id);

        // IEnumerable<PurchaseOrderHeader> GetPurchaseOrderHeaderList(int buyerId);              
        OverTimeApplicationHeader GetOverTimeApplicationHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();

       
        
        AttendanceLine LineCreate(AttendanceLine pt);
        IQueryable<OverTimeApplicationHeaderViewModel> GetOverTimeApplicationHeaderPendingToSubmit(int id, string Uname);
        IQueryable<OverTimeApplicationHeaderViewModel> GetOverTimeApplicationHeaderPendingToReview(int id, string Uname);

        IEnumerable<PersonViewModel> GetListEmpName(int DepartmentID);
    }

    public class OverTimeApplicationHeaderService : IOverTimeApplicationHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<OverTimeApplicationHeader> _OverTimeApplicationHeaderRepository;
        RepositoryQuery<OverTimeApplicationHeader> OverTimeApplicationHeaderRepository;
        public OverTimeApplicationHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _OverTimeApplicationHeaderRepository = new Repository<OverTimeApplicationHeader>(db);
            OverTimeApplicationHeaderRepository = new RepositoryQuery<OverTimeApplicationHeader>(_OverTimeApplicationHeaderRepository);
        }
        public OverTimeApplicationHeader GetOverTimeApplicationHeaderByName(string terms)
        {
            return (from p in db.OverTimeApplicationHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }
        public OverTimeApplicationHeader Find(string DocNo)
        {
            return (from p in db.OverTimeApplicationHeader
                    where p.DocNo == DocNo
                    select p
                        ).FirstOrDefault();
        }

        public AttendanceLine FindLine(int id)
        {
            return _unitOfWork.Repository<AttendanceLine>().Find(id);
        }
        /* public IEnumerable<AttendanceLinesViewModel> GetAttendanceLineView(int id)
         {
             return (from H in db.OverTimeApplicationHeader
                     join L in db.AttendanceLine on H.OverTimeApplicationHeaderId equals L.OverTimeApplicationHeaderId
                     join p in db.Persons on L.EmployeeId equals p.PersonID
                     where H.OverTimeApplicationHeaderId == id
                     select new AttendanceLinesViewModel { OverTimeApplicationHeaderId = L.OverTimeApplicationHeaderId, AttendanceLineId = L.AttendanceLineId, DocTime = L.DocTime, Name = p.Name, AttendanceCategory = L.AttendanceCategory, Remark = L.Remark }
                     );
         }*/
    


     

      

        public IEnumerable<PersonViewModel> GetListEmpName(int DepartmentID)
        {
            return (from E in db.Employee
                          join P in db.Persons on E.PersonID equals P.PersonID
                          where E.DepartmentID == DepartmentID
                          select new PersonViewModel { Name = P.Name, PersonID = P.PersonID }
                          );
        }
        public AttendanceLine LineCreate(AttendanceLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<AttendanceLine>().Add(pt);
            return pt;
        }
        public OverTimeApplicationHeader Find(int id)
        {
            return _unitOfWork.Repository<OverTimeApplicationHeader>().Find(id);
        }

        public OverTimeApplicationHeader Create(OverTimeApplicationHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<OverTimeApplicationHeader>().Insert(pt);
            return pt;
        }
        public OverTimeApplicationHeaderViewModel GetOverTimeApplicationHeader(int id)
        {
            string empid = string.Join(",", (from l in db.OverTimeApplicationLine where l.OverTimeApplicationHeaderId == id select l.EmployeeId).ToArray());

            return (from p in db.OverTimeApplicationHeader
                    where p.OverTimeApplicationId == id
                    select new OverTimeApplicationHeaderViewModel
                    {
                        OverTimeApplicationId =p.OverTimeApplicationId,
                        DocTypeId = p.DocTypeId,
                        DocDate = p.DocDate,
                        SiteId = p.SiteId,
                        DocNo = p.DocNo,
                        PersonId = p.PersonId,
                        DepartmentId = p.DepartmentId,
                        Remark = p.Remark,
                        PersonId1= empid.ToString(),
                        Status = p.Status,
                        GodownId=p.GodownId,
                        ModifiedDate = p.ModifiedDate,
                        ModifiedBy = p.ModifiedBy,
                        CreatedBy = p.CreatedBy,
        }
                    ).FirstOrDefault();
        }

      
        /* public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrders(int ProductId, int PurchaseGoodsReceiptHeaderId)//Product Id
         {

             var PurchaseReceipt = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(PurchaseGoodsReceiptHeaderId);

             var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(PurchaseReceipt.DocTypeId, PurchaseReceipt.DivisionId, PurchaseReceipt.SiteId);

             string[] contraDocTypes = null;
             if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
             else { contraDocTypes = new string[] { "NA" }; }

             string[] contraSites = null;
             if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
             else { contraSites = new string[] { "NA" }; }

             string[] contraDivisions = null;
             if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
             else { contraDivisions = new string[] { "NA" }; }

             int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
             int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

             var tem = from p in db.ViewPurchaseOrderBalance
                       join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                       from tab in table.DefaultIfEmpty()
                       where p.ProductId == ProductId && p.BalanceQty > 0 && p.SupplierId == PurchaseReceipt.SupplierId
                       && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                            && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                       && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                       orderby p.PurchaseOrderNo
                       select new PurchaseOrderLineListViewModel
                       {
                           DocNo = p.PurchaseOrderNo,
                           PurchaseOrderLineId = p.PurchaseOrderLineId,
                           Dimension1Name = tab.Dimension1.Dimension1Name,
                           Dimension2Name = tab.Dimension2.Dimension2Name,
                       };

             return (tem);
         }
         */

        /* public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrdersForOrderCancel(int ProductId, int PurchaseOrderCancelHeaderId)//Product Id
         {

             var PurchaseOrderCancel = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(PurchaseOrderCancelHeaderId);

             var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrderCancel.DocTypeId, PurchaseOrderCancel.DivisionId, PurchaseOrderCancel.SiteId);

             string[] contraDocTypes = null;
             if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
             else { contraDocTypes = new string[] { "NA" }; }

             string[] contraSites = null;
             if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
             else { contraSites = new string[] { "NA" }; }

             string[] contraDivisions = null;
             if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
             else { contraDivisions = new string[] { "NA" }; }

             int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
             int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

             var tem = from p in db.ViewPurchaseOrderBalance
                       join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                       from tab in table.DefaultIfEmpty()
                       where p.ProductId == ProductId && p.BalanceQty > 0 && p.SupplierId == PurchaseOrderCancel.SupplierId
                       && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                       && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                       && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                       orderby p.PurchaseOrderNo
                       select new PurchaseOrderLineListViewModel
                       {
                           DocNo = p.PurchaseOrderNo,
                           PurchaseOrderLineId = p.PurchaseOrderLineId,
                           Dimension1Name = tab.Dimension1.Dimension1Name,
                           Dimension2Name = tab.Dimension2.Dimension2Name,
                       };

             return (tem);
         }
         */
        public void Delete(int id)
        {
            _unitOfWork.Repository<OverTimeApplicationHeader>().Delete(id);
        }

        public void Delete(OverTimeApplicationHeader pt)
        {
            _unitOfWork.Repository<OverTimeApplicationHeader>().Delete(pt);
        }

        public void Update(OverTimeApplicationHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<OverTimeApplicationHeader>().Update(pt);
        }

        public IEnumerable<OverTimeApplicationHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<OverTimeApplicationHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<OverTimeApplicationHeaderViewModel> GetOverTimeApplicationHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            return (from p in db.OverTimeApplicationHeader
                    join t in db._Users on p.ModifiedBy equals t.UserName into table
                    from tab in table.DefaultIfEmpty()
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DocTypeId == id
                    select new OverTimeApplicationHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeName = p.DocType.DocumentTypeName,
                        OverTimeApplicationId = p.OverTimeApplicationId,
                        Remark = p.Remark,
                        Name = p.Person.Name,
                        Status = p.Status,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        DepartmentName = p.Department.DepartmentName,
                        GodownId=p.GodownId,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                );
        }

        public OverTimeApplicationHeader Add(OverTimeApplicationHeader pt)
        {

            _unitOfWork.Repository<OverTimeApplicationHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.OverTimeApplicationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.OverTimeApplicationId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.OverTimeApplicationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.OverTimeApplicationId).FirstOrDefault();
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

                temp = (from p in db.OverTimeApplicationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.OverTimeApplicationId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.OverTimeApplicationHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.OverTimeApplicationId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<OverTimeApplicationHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

      
        public IQueryable<OverTimeApplicationHeaderViewModel> GetOverTimeApplicationHeaderPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var AttandanceHeader = GetOverTimeApplicationHeaderList(id, Uname).AsQueryable();
            //return AttandanceHeader;
            var PendingToSubmit = from p in AttandanceHeader
                                  where (p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified) && (p.ModifiedBy == Uname || UserRoles.Contains("admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<OverTimeApplicationHeaderViewModel> GetOverTimeApplicationHeaderPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var AttandanceHeader = GetOverTimeApplicationHeaderList(id, Uname).AsQueryable();
            //return AttandanceHeader;
            var PendingToReview = from p in AttandanceHeader
                                  where p.Status == (int)StatusConstants.Submitted //&& (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public void Dispose()
        {
        }





    }
}
