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
    public interface IPackingHeaderService : IDisposable
    {
        PackingHeader Create(PackingHeader s);
        void Delete(int id);
        void Delete(PackingHeader s);
        PackingHeader GetPackingHeader(int id);

        PackingHeaderViewModel GetPackingHeaderViewModel(int id);
        PackingHeader Find(int id);
        IQueryable<PackingHeaderIndexViewModel> GetPackingHeaderList(int id, string Uname);
        IQueryable<PackingHeaderIndexViewModel> GetPackingHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<PackingHeaderIndexViewModel> GetPackingHeaderListPendingToReview(int id, string Uname);



        IQueryable<PackingHeaderViewModel> GetPackingHeaderList(string Uname);
        IQueryable<PackingHeaderViewModel> GetPackingHeaderListPendingToSubmit(string Uname);
        IQueryable<PackingHeaderViewModel> GetPackingHeaderListPendingToReview(string Uname);





        void Update(PackingHeader s);
        string GetMaxDocNo();
        PackingHeader FindByDocNo(string Docno, int DivisionId, int SiteId);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);

       
    }
    public class PackingHeaderService : IPackingHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public PackingHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public PackingHeader Create(PackingHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PackingHeader>().Insert(s);
            return s;
        }

       public void Delete(int id)
     {
         _unitOfWork.Repository<PackingHeader>().Delete(id);
     }
       public void Delete(PackingHeader s)
        {
            _unitOfWork.Repository<PackingHeader>().Delete(s);
        }
       public void Update(PackingHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PackingHeader>().Update(s);            
        }

       public PackingHeader GetPackingHeader(int id)
        {
            return _unitOfWork.Repository<PackingHeader>().Query().Get().Where(m => m.PackingHeaderId == id).FirstOrDefault();
        }

       public PackingHeader Find(int id)
       {
           return _unitOfWork.Repository<PackingHeader>().Find(id);
       }

       public PackingHeader FindByDocNo(string Docno, int DivisionId, int SiteId)
       {
           return _unitOfWork.Repository<PackingHeader>().Query().Get().Where(m => m.DocNo == Docno && m.DivisionId == DivisionId && m.SiteId == SiteId ).FirstOrDefault();

       }

       public int NextId(int id)
       {
           int temp = 0;
           if (id != 0)
           {

               temp = (from p in db.PackingHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.PackingHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


           }
           else
           {
               temp = (from p in db.PackingHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.PackingHeaderId).FirstOrDefault();
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

               temp = (from p in db.PackingHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.PackingHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
           }
           else
           {
               temp = (from p in db.PackingHeader
                       orderby p.DocDate descending, p.DocNo descending
                       select p.PackingHeaderId).AsEnumerable().LastOrDefault();
           }
           if (temp != 0)
               return temp;
           else
               return id;
       }

       public string GetMaxDocNo()
       {
           int x;
           var maxVal = _unitOfWork.Repository<PackingHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
           return (maxVal + 1).ToString();
       }

       public void Dispose()
       {
       }

        public PackingHeaderViewModel GetPackingHeaderViewModel(int id)
       {

           PackingHeaderViewModel packingheader = (from H in db.PackingHeader
                        join B in db.Persons on H.BuyerId equals B.PersonID into BuyerTable  from BuyerTab in BuyerTable.DefaultIfEmpty()
                        join B in db.Persons on H.JobWorkerId equals B.PersonID into JobWorkerTable from JobWorkerTab in JobWorkerTable.DefaultIfEmpty()
                        join D in db.DocumentType on H.DocTypeId equals D.DocumentTypeId into DocumentTypeTable from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                        join Div in db.Divisions on H.DivisionId equals Div.DivisionId into DivisionTable from DivisionTab in DivisionTable.DefaultIfEmpty()
                        join S in db.Site on H.SiteId equals S.SiteId into SiteTable from SiteTab in SiteTable.DefaultIfEmpty()
                        join G in db.Godown on H.GodownId equals G.GodownId into GodownTable from GodownTab in GodownTable.DefaultIfEmpty()
                                                   join Du in db.Units on H.DealUnitId equals Du.UnitId into DeliveryUnitTable
                                                   from DeliveryUnitTab in DeliveryUnitTable.DefaultIfEmpty()
                        where H.PackingHeaderId == id
                        select new PackingHeaderViewModel
                        {
                            PackingHeaderId = H.PackingHeaderId,
                            DocTypeName = DocumentTypeTab.DocumentTypeName,
                            DocDate = H.DocDate,
                            DocNo = H.DocNo,
                            BuyerId = H.BuyerId,
                            DocTypeId=H.DocTypeId,
                            BuyerName = BuyerTab.Name,
                            JobWorkerId = H.JobWorkerId.Value,
                            JobWorkerName = JobWorkerTab.Name,
                            DivisionId = H.DivisionId,
                            DivisionName = DivisionTab.DivisionName,
                            SiteId = H.SiteId,
                            SiteName = SiteTab.SiteName,
                            GodownId = H.GodownId,
                            GodownName = GodownTab.GodownName,
                            DealUnitId = H.DealUnitId,
                            DealUnitName = DeliveryUnitTab.UnitName,
                            BaleNoPattern = H.BaleNoPattern,
                            ShipMethodId = H.ShipMethodId,
                            Remark = H.Remark,
                            Status = H.Status,
                            CreatedBy = H.CreatedBy,
                            CreatedDate = H.CreatedDate,
                            ModifiedBy = H.ModifiedBy,
                            ModifiedDate = H.ModifiedDate,
                            LockReason=H.LockReason,
                            TotalQty = H.PackingLines.Sum(m => m.Qty),
                            DecimalPlaces = (from o in H.PackingLines
                                             join prod in db.Product on o.ProductId equals prod.ProductId
                                             join u in db.Units on prod.UnitId equals u.UnitId
                                             select u.DecimalPlaces).Max(),

                        }).FirstOrDefault();

           return packingheader;
       }


        public IQueryable<PackingHeaderViewModel> GetPackingHeaderList(string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            IQueryable<PackingHeaderViewModel> packingheaderlist = from H in db.PackingHeader
                                                                        join B in db.Persons on H.BuyerId equals B.PersonID into BuyerTable
                                                                        from BuyerTab in BuyerTable.DefaultIfEmpty()
                                                                        orderby H.DocDate descending, H.DocNo descending
                                                                        where H.SiteId == SiteId && H.DivisionId == DivisionId
                                                                        select new PackingHeaderViewModel
                                                                        {
                                                                            PackingHeaderId = H.PackingHeaderId,
                                                                            DocTypeId = H.DocTypeId,
                                                                            DocDate = H.DocDate,
                                                                            DocNo = H.DocNo,
                                                                            BuyerName = BuyerTab.Name,
                                                                            Remark = H.Remark,
                                                                            Status = H.Status,
                                                                            ModifiedBy = H.ModifiedBy,
                                                                            ReviewCount = H.ReviewCount,
                                                                            ReviewBy = H.ReviewBy,
                                                                            Reviewed = (SqlFunctions.CharIndex(Uname, H.ReviewBy) > 0),
                                                                            TotalQty = H.PackingLines.Sum(m => m.Qty),
                                                                            DecimalPlaces = (from o in H.PackingLines
                                                                                             join prod in db.Product on o.ProductId equals prod.ProductId
                                                                                             join u in db.Units on prod.UnitId equals u.UnitId
                                                                                             select u.DecimalPlaces).Max(),
                                                                        };


            return packingheaderlist;
        }

        public IQueryable<PackingHeaderViewModel> GetPackingHeaderListPendingToSubmit(string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetPackingHeaderList(Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;
        }

        public IQueryable<PackingHeaderViewModel> GetPackingHeaderListPendingToReview(string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetPackingHeaderList(Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;
        }






        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(DocTypeId, DivisionId, SiteId);

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
                        where PersonProcessTab.ProcessId == settings.ProcessId
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

        //New Functions

        public IQueryable<PackingHeaderIndexViewModel> GetPackingHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            IQueryable<PackingHeaderIndexViewModel> packingheaderlist = from H in db.PackingHeader
                                                                        join B in db.Persons on H.BuyerId equals B.PersonID into BuyerTable
                                                                        from BuyerTab in BuyerTable.DefaultIfEmpty()
                                                                        orderby H.DocDate descending, H.DocNo descending
                                                                        where H.SiteId == SiteId && H.DivisionId == DivisionId && H.DocTypeId == id
                                                                        select new PackingHeaderIndexViewModel
                                                                        {
                                                                            PackingHeaderId = H.PackingHeaderId,
                                                                            DocTypeId = H.DocTypeId,
                                                                            DocDate = H.DocDate,
                                                                            DocNo = H.DocNo,
                                                                            BuyerName = BuyerTab.Name,
                                                                            Remark = H.Remark,
                                                                            Status = H.Status,
                                                                            ModifiedBy = H.ModifiedBy,
                                                                            ReviewCount = H.ReviewCount,
                                                                            ReviewBy = H.ReviewBy,
                                                                            Reviewed = (SqlFunctions.CharIndex(Uname, H.ReviewBy) > 0),
                                                                        };


            return packingheaderlist;
        }

        public IQueryable<PackingHeaderIndexViewModel> GetPackingHeaderListPendingToSubmit(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetPackingHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;
        }

        public IQueryable<PackingHeaderIndexViewModel> GetPackingHeaderListPendingToReview(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetPackingHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;
        }
    }
}
