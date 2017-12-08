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
    public interface IAttendanceHeaderService : IDisposable
    {
        AttendanceHeader Create(AttendanceHeader pt);
        void Delete(int id);
        void Delete(AttendanceHeader pt);
        AttendanceHeader Find(int id);
        AttendanceHeader Find(string DocNo);
        IEnumerable<AttendanceHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(AttendanceHeader pt);
        AttendanceHeader Add(AttendanceHeader pt);
        IQueryable<AttendanceHeaderViewModel> GetAttendanceHeaderList(int id, string Uname);

        AttendanceLine FindLine(int id);
        IEnumerable<AttendanceLinesViewModel> GetAttendanceLineView(int id);
        AttendanceHeaderViewModel GetAttendanceHeader(int id);

        // IEnumerable<PurchaseOrderHeader> GetPurchaseOrderHeaderList(int buyerId);              
        AttendanceHeader GetAttendanceHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();

        DateTime GetShiftTime(int Id);
        IEnumerable<int> GetListEmpId(int DepartmentID);
        AttendanceLine LineCreate(AttendanceLine pt);
        IQueryable<AttendanceHeaderViewModel> GetAttendanceheaderPendingToSubmit(int id, string Uname);
        IQueryable<AttendanceHeaderViewModel> GetAttendanceheaderPendingToReview(int id, string Uname);
    }

    public class AttendanceHeaderService : IAttendanceHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<AttendanceHeader> _AttendanceHeader1Repository;
        RepositoryQuery<AttendanceHeader> AttendanceHeader1Repository;
        public AttendanceHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _AttendanceHeader1Repository = new Repository<AttendanceHeader>(db);
            AttendanceHeader1Repository = new RepositoryQuery<AttendanceHeader>(_AttendanceHeader1Repository);
        }
        public AttendanceHeader GetAttendanceHeaderByName(string terms)
        {
            return (from p in db.AttendanceHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }
        public AttendanceHeader Find(string DocNo)
        {
            return (from p in db.AttendanceHeader
                    where p.DocNo == DocNo
                    select p
                        ).FirstOrDefault();
        }

        public AttendanceLine FindLine(int id)
        {
            return _unitOfWork.Repository<AttendanceLine>().Find(id);
        }
        public IEnumerable<AttendanceLinesViewModel> GetAttendanceLineView(int id)
        {
            return (from H in db.AttendanceHeader
                    join L in db.AttendanceLine on H.AttendanceHeaderId equals L.AttendanceHeaderId
                    join p in db.Persons on L.EmployeeId equals p.PersonID
                    where H.AttendanceHeaderId == id
                    select new AttendanceLinesViewModel { AttendanceHeaderId = L.AttendanceHeaderId, AttendanceLineId = L.AttendanceLineId, DocTime = L.DocTime, Name = p.Name, AttendanceCategory = L.AttendanceCategory, Remark = L.Remark }
                    );
        }
        public IEnumerable<int> GetListEmpId(int DepartmentID)
        {
            var Result = (from E in db.Employee
                          where E.DepartmentID == DepartmentID
                          select E.PersonID).Distinct().ToList();
            return Result;
        }

        public AttendanceLine LineCreate(AttendanceLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<AttendanceLine>().Add(pt);
            return pt;
        }
        public AttendanceHeader Find(int id)
        {
            return _unitOfWork.Repository<AttendanceHeader>().Find(id);
        }

        public AttendanceHeader Create(AttendanceHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<AttendanceHeader>().Insert(pt);
            return pt;
        }
        public AttendanceHeaderViewModel GetAttendanceHeader(int id)
        {
            return (from p in db.AttendanceHeader
                    where p.AttendanceHeaderId == id
                    select new AttendanceHeaderViewModel
                    {
                        AttendanceHeaderId = p.AttendanceHeaderId,
                        DocTypeId = p.DocTypeId,
                        DocDate = p.DocDate,
                        SiteId = p.SiteId,
                        DocNo = p.DocNo,
                        PersonId = p.PersonId,
                        DepartmentId = p.DepartmentId,
                        Remark = p.Remark,
                        Status = p.Status,
                        ShiftId = p.ShiftId,
                        ModifiedDate = p.ModifiedDate,
                        ModifiedBy = p.ModifiedBy,
                        CreatedBy = p.CreatedBy,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<Shift> GetShiftList()
        {
            var pt = (from p in db.Shift
                      orderby p.ShiftName
                      select p);
            return pt;
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
            _unitOfWork.Repository<AttendanceHeader>().Delete(id);
        }

        public void Delete(AttendanceHeader pt)
        {
            _unitOfWork.Repository<AttendanceHeader>().Delete(pt);
        }

        public void Update(AttendanceHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<AttendanceHeader>().Update(pt);
        }

        public IEnumerable<AttendanceHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<AttendanceHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<AttendanceHeaderViewModel> GetAttendanceHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            return (from p in db.AttendanceHeader
                    join t in db._Users on p.ModifiedBy equals t.UserName into table
                    from tab in table.DefaultIfEmpty()
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DocTypeId == id
                    select new AttendanceHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeName = p.DocType.DocumentTypeName,
                        AttendanceHeaderId = p.AttendanceHeaderId,
                        Remark = p.Remark,
                        Name = p.Person.Name,
                        Status = p.Status,
                        ModifiedBy = p.ModifiedBy,
                        FirstName = tab.FirstName,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        DepartmentName = p.Department.DepartmentName,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                );
        }

        public AttendanceHeader Add(AttendanceHeader pt)
        {

            _unitOfWork.Repository<AttendanceHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.AttendanceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.AttendanceHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.AttendanceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.AttendanceHeaderId).FirstOrDefault();
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

                temp = (from p in db.AttendanceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.AttendanceHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.AttendanceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.AttendanceHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<AttendanceHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public DateTime GetShiftTime(int Id)
        {
            return (from p in db.Shift
                    select p.StartTime).AsEnumerable().FirstOrDefault();
        }
        public IQueryable<AttendanceHeaderViewModel> GetAttendanceheaderPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var AttandanceHeader = GetAttendanceHeaderList(id, Uname).AsQueryable();
            //return AttandanceHeader;
            var PendingToSubmit = from p in AttandanceHeader
                                  where (p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified) && (p.ModifiedBy == Uname || UserRoles.Contains("admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<AttendanceHeaderViewModel> GetAttendanceheaderPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var AttandanceHeader = GetAttendanceHeaderList(id, Uname).AsQueryable();
            //return AttandanceHeader;
            var PendingToReview = from p in AttandanceHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public void Dispose()
        {
        }





    }
}
