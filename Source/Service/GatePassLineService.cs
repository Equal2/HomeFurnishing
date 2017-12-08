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
using System.Data.SqlClient;

namespace Service
{
    public interface IGatePassLineService : IDisposable
    {
        GatePassLine Create(GatePassLine pt);
        void Delete(int id);
        void Delete(GatePassLine pt);
        GatePassLine Find(int id);
        IEnumerable<GatePassLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(GatePassLine pt);
        GatePassLine Add(GatePassLine pt);                
        Task<IEquatable<GatePassLine>> GetAsync();
        Task<GatePassLine> FindAsync(int id);
        IEnumerable<GatePassLine> GetGatePassLineList(int GatePassHeaderId);

        IQueryable<GatePassLineViewModel> GetGatePassLineListForIndex(int JobOrderHeaderId);
        GatePassLineViewModel GetGatePassLine(int id);
    }

    public class GatePassLineService : IGatePassLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<GatePassLine> _GatePassLineRepository;
        RepositoryQuery<GatePassLine> GatePassLineRepository;
        public GatePassLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GatePassLineRepository = new Repository<GatePassLine>(db);
            GatePassLineRepository = new RepositoryQuery<GatePassLine>(_GatePassLineRepository);
        }
     

        public GatePassLine Find(int id)
        {
            return _unitOfWork.Repository<GatePassLine>().Find(id);
        }

        public GatePassLine Create(GatePassLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<GatePassLine>().Insert(pt);
            return pt;
        }     

        public void Delete(int id)
        {
            _unitOfWork.Repository<GatePassLine>().Delete(id);
        }

        public void Delete(GatePassLine pt)
        {
            _unitOfWork.Repository<GatePassLine>().Delete(pt);
        }

        public void Update(GatePassLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<GatePassLine>().Update(pt);
        }

        public IEnumerable<GatePassLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<GatePassLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.GatePassLineId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public GatePassLine Add(GatePassLine pt)
        {
            _unitOfWork.Repository<GatePassLine>().Insert(pt);
            return pt;
        }

        public IEnumerable<GatePassLine> GetGatePassLineList(int GatePassHeaderId)
        {
            return (from p in db.GatePassLine
                    where p.GatePassHeaderId == GatePassHeaderId
                    select p);
        }

        public IQueryable<GatePassLineViewModel> GetGatePassLineListForIndex(int GatePassHeaderId)
        {
            var temp = from p in db.GatePassLine                
                       where p.GatePassHeaderId == GatePassHeaderId
                       orderby p.GatePassLineId
                       select new GatePassLineViewModel
                       {
                           GatePassHeaderId=p.GatePassHeaderId,
                           GatePassLineId=p.GatePassLineId,
                           Product=p.Product,
                           Specification=p.Specification,
                           Qty=p.Qty,
                           UnitId=p.UnitId,
                           DecimalPlaces = (from o in db.Units
                                            where o.UnitId== p.UnitId
                                            select o.DecimalPlaces).Max(),
                           UnitName = (from o in db.Units
                                            where o.UnitId == p.UnitId
                                            select o.UnitName).Max(),
                           CreatedBy =p.CreatedBy,
                           CreatedDate = p.CreatedDate,
                           ModifiedBy = p.ModifiedBy,
                           ModifiedDate = p.ModifiedDate,
                           
                       };
            return temp;
        }


        public GatePassLineViewModel GetGatePassLine(int id)
        {
            var temp = (from p in db.GatePassLine
                        where p.GatePassLineId == id
                        select new GatePassLineViewModel
                        {
                            Product = p.Product,
                            GatePassHeaderId = p.GatePassHeaderId,
                            GatePassLineId=p.GatePassLineId,
                            Specification = p.Specification,
                            Qty = p.Qty,
                            UnitId = p.UnitId,                           
                            
                        }).FirstOrDefault();
            return temp;
        }
        public void GenerateGatePass(int id)//PurchaseGoodsReturnHeaderId
        {
           
            


        }

        public void Dispose()
        {
        }


        public Task<IEquatable<GatePassLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GatePassLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
