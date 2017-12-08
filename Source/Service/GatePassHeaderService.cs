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
using System.Data.Entity.SqlServer;
using Model.ViewModel;

namespace Service
{
    public interface IGatePassHeaderService : IDisposable
    {
        GatePassHeader Create(GatePassHeader pt);
        void Delete(int id);
        void Delete(GatePassHeader pt);
        GatePassHeader Find(string Name);
        GatePassHeader Find(int id);
        IEnumerable<GatePassHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(GatePassHeader pt);
        GatePassHeader Add(GatePassHeader pt);
        Task<IEquatable<GatePassHeader>> GetAsync();
        Task<GatePassHeader> FindAsync(int id);

        GatePassHeaderViewModel GetGatePassHeader(int id);
        IQueryable<GatePassHeaderViewModel> GetPendingGatePassList(int GodownId);

        IQueryable<GatePassHeaderViewModel> GetGatePassHeaderList(int DocumentTypeId, string Uname);

        IQueryable<GatePassHeaderViewModel> GetGatePassHeaderListPendingToSubmit(int DocumentTypeId, string Uname);
        IQueryable<GatePassHeaderViewModel> GetGatePassHeaderListPendingToReview(int DocumentTypeId, string Uname);
        bool CheckForDocNoExists(int GodownId, string docno, int doctypeId);
        IEnumerable<GatePassHeaderViewModel> GetActiveGatePassesForSupplier(int SupplierId);
        IQueryable<ComboBoxResult> GetGatePassActiveJobWorkers(string term);
    }

    public class GatePassHeaderService : IGatePassHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<GatePassHeader> _GatePassHeaderRepository;
        RepositoryQuery<GatePassHeader> GatePassHeaderRepository;
        public GatePassHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GatePassHeaderRepository = new Repository<GatePassHeader>(db);
            GatePassHeaderRepository = new RepositoryQuery<GatePassHeader>(_GatePassHeaderRepository);
        }

        public GatePassHeader Find(string Name)
        {
            return GatePassHeaderRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public GatePassHeader Find(int id)
        {
            return _unitOfWork.Repository<GatePassHeader>().Find(id);
        }

        public GatePassHeader Create(GatePassHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<GatePassHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<GatePassHeader>().Delete(id);
        }

        public void Delete(GatePassHeader pt)
        {
            _unitOfWork.Repository<GatePassHeader>().Delete(pt);
        }

        public void Update(GatePassHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<GatePassHeader>().Update(pt);
        }

        public IEnumerable<GatePassHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<GatePassHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public GatePassHeader Add(GatePassHeader pt)
        {
            _unitOfWork.Repository<GatePassHeader>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<GatePassHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GatePassHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }



        public IQueryable<GatePassHeaderViewModel> GetPendingGatePassList(int GodownId)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //var Query = (from a in db.GatePassLine.ToList()
            //        group a by a.GatePassHeaderId into g
            //        select new 
            //        {
            //            GatePassHeaderId = g.Key,
            //            Remark = string.Join(",", g.Select(x => x.Product + "(" + x.Qty + x.UnitId + ")"))
            //        });




            //var Query1 = (from G in db.GatePassHeader
            //              join p in db.Persons on G.PersonId equals p.PersonID
            //              join L in db.GatePassLine on G.GatePassHeaderId equals L.GatePassHeaderId                         
            //              into table 
            //              from ta in table.DefaultIfEmpty()
            //              group new { G,ta} by G.GatePassHeaderId into GL
            //              select new GatePassHeader
            //              {    
            //                 GatePassHeaderId=GL.Key,
            //                 DocNo=GL.Max(x=>x.G.DocNo),
            //                  DocDate = GL.Max(x => x.G.DocDate),
            //                  Status = GL.Max(x => x.G.Status),
            //                  Remark = string.Join(",", GL.Select(x => x.ta.Product + "(" + x.ta.Qty + x.ta.UnitId + ")"))

            //              }
            //             ).ToList();

            //var Query2 = (
            //             from G in db.GatePassHeader
            //             join L in db.GatePassLine on G.GatePassHeaderId equals L.GatePassHeaderId
            //             join P in db.Persons on G.PersonId equals P.PersonID
            //             select new
            //             {
            //                 GatePassHeaderId=G.GatePassHeaderId,
            //                 DocNo=G.DocNo,
            //                 DocDate=G.DocDate,
            //                 Status=G.Status,
            //                 Name=P.Name,
            //                 Product=L.Product,
            //                 Qty=L.Qty,
            //                 UnitId=L.UnitId

            //             });

            return (
                from G in db.GatePassHeader
                join P in db.Persons on G.PersonId equals P.PersonID
                orderby G.DocNo descending, G.GatePassHeaderId descending
                where G.GodownId == GodownId && G.Status == 0
                select new GatePassHeaderViewModel
                {
                    GatePassHeaderId = G.GatePassHeaderId,
                    DocNo = G.DocNo,
                    DocDate = G.DocDate,
                    Status = G.Status,
                    Name = P.Name
                }
                   );


            //return (from a in Query2
            //              group a by a.GatePassHeaderId into g
            //              select new 
            //              {
            //                  GatePassHeaderId = g.Key,
            //                  DocNo=g.Max(x=>x.DocNo),
            //                  DocDate = g.Max(x => x.DocDate),
            //                  Status = g.Max(x => x.Status),
            //Remark = string.Join(",", g.Select(x => x.Product + " ( " + x.Qty + x.UnitId + " ) ")),
            //    Product=g.Select(x=>x.Product),
            //    Qty=g.Select(x=>x.Qty),
            //    UnitId=g.Select(x=>x.UnitId),
            //});






            //var Test = (from G in db.GatePassHeader
            //            join p in db.Persons on G.PersonId equals p.PersonID
            //            join L in Query on G.GatePassHeaderId equals L.GatePassHeaderId
            //            orderby G.GatePassHeaderId ascending
            //            where G.GodownId == GodownId
            //            select new GatePassHeader
            //            {
            //                GatePassHeaderId = G.GatePassHeaderId,
            //                DocNo = G.DocNo,
            //                DocDate = G.DocDate,
            //                Remark = Query.Where(p => p.GatePassHeaderId == G.GatePassHeaderId).Select(p => p.Remark).FirstOrDefault(),
            //                Status = G.Status
            //            }).ToList();



            //return (
            //from G in db.GatePassHeader
            //join p in db.Persons on G.PersonId equals p.PersonID
            //join L in Query on G.GatePassHeaderId equals L.GatePassHeaderId
            //orderby G.GatePassHeaderId ascending
            //where G.GodownId == GodownId
            //select new GatePassHeader
            //{
            //    GatePassHeaderId = G.GatePassHeaderId,
            //    DocNo = G.DocNo,
            //    DocDate = G.DocDate,
            //    Remark = L.Remark,
            //    Status = G.Status
            //});

            //from G in db.GatePassHeader
            //join p in db.Persons on G.PersonId equals p.PersonID
            //join L in db.GatePassLine on G.GatePassHeaderId equals L.GatePassHeaderId
            //orderby G.GatePassHeaderId ascending
            //where G.GodownId == GodownId
            //group new { G, L } by new { L.GatePassHeaderId } into GL
            //select new GatePassHeader
            //{
            //    GatePassHeaderId = GL.Key.GatePassHeaderId,
            //    DocNo = GL.Max(x => x.G.DocNo),
            //    DocDate = GL.Max(x => x.G.DocDate),
            //    Status = GL.Max(x => x.G.Status),
            //    Remark = string.Join(",", GL.Select(x => x.L.Product + "(" + x.L.Qty + x.L.UnitId + ")"))
            //}



        }


        public IQueryable<GatePassHeaderViewModel> GetGatePassHeaderList(int DocumentTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.GatePassHeader
                    join t in db.Persons on p.PersonId equals t.PersonID
                    join dt in db.DocumentType on p.DocTypeId equals dt.DocumentTypeId
                    join G in db.Godown on p.GodownId equals G.GodownId
                    orderby p.DocDate descending, p.DocNo descending
                    where p.DocTypeId == DocumentTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select new GatePassHeaderViewModel
                    {
                        DocTypeName = dt.DocumentTypeName,
                        Name = t.Name + "," + t.Suffix,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        Status = p.Status,
                        GatePassHeaderId = p.GatePassHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        GodownName = G.GodownName,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        OrderById = p.OrderById,
                        TotalQty = p.GatePassLines.Sum(m => m.Qty),
                        DecimalPlaces = (from o in p.GatePassLines
                                         join u in db.Units on o.UnitId equals u.UnitId
                                         select u.DecimalPlaces).Max(),
                    });
        }

        public IQueryable<GatePassHeaderViewModel> GetGatePassHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var GatePassHeader = GetGatePassHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in GatePassHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<GatePassHeaderViewModel> GetGatePassHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var GatePassHeader = GetGatePassHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in GatePassHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public GatePassHeaderViewModel GetGatePassHeader(int id)
        {
            return (from p in db.GatePassHeader
                    where p.GatePassHeaderId == id
                    select new GatePassHeaderViewModel
                    {
                        DocTypeName = p.DocType.DocumentTypeName,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        GatePassHeaderId = p.GatePassHeaderId,
                        Status = p.Status,
                        DocTypeId = p.DocTypeId,
                        PersonId = p.PersonId,
                        GodownId = p.GodownId,
                        DivisionId = p.DivisionId,
                        SiteId = p.SiteId,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        OrderById = p.OrderById,
                        ReferenceDocId = p.ReferenceDocId
                    }
                        ).FirstOrDefault();
        }

        public bool CheckForDocNoExists(int GodownId, string docno, int doctypeId)
        {
            var GateId = (from G in db.Godown where G.GodownId == GodownId select G.GateId).FirstOrDefault();
            var temp = (from p in db.GatePassHeader
                        join G in db.Godown on p.GodownId equals G.GodownId
                        where p.DocNo == docno && p.DocTypeId == doctypeId && G.GateId == GateId
                        select p).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        public bool CheckForDocNoExists(int GodownId, string docno, int doctypeId, int headerid)
        {
            var temp = (from p in db.GatePassHeader
                        where p.DocNo == docno && p.GatePassHeaderId != headerid
                        select p).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        public IEnumerable<GatePassHeaderViewModel> GetActiveGatePassesForSupplier(int SupplierId)
        {
            return (from p in db.GatePassHeader
                    where p.PersonId == SupplierId && p.Status == (int)StatusConstants.Drafted
                    select new GatePassHeaderViewModel
                    {
                        Name = p.Person.Name,
                        DocNo = p.DocNo,
                        Product = p.GatePassLines.Max(m => m.Product),
                        Qty = p.GatePassLines.Sum(m => m.Qty),
                        UnitName = p.GatePassLines.Max(m => m.Unit.UnitName),
                        GatePassHeaderId = p.GatePassHeaderId,
                    }).Take(10).ToList();
        }

        public IQueryable<ComboBoxResult> GetGatePassActiveJobWorkers(string term)
        {

            var list = (from jw in db.JobWorker
                        join p in db.Persons on jw.PersonID equals p.PersonID
                        join gp in db.GatePassHeader on p.PersonID equals gp.PersonId                        
                        where gp.Status == (int)StatusConstants.Drafted                        
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.Name.ToLower().Contains(term.ToLower())))
                        group p by jw.PersonID into pg 
                        orderby pg.FirstOrDefault().Name
                        select new ComboBoxResult
                        {
                            id = pg.FirstOrDefault().PersonID.ToString(),
                            text = pg.FirstOrDefault().Name
                        }
              );

            return list;
        }

    }
}
