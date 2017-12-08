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

namespace Service
{
    public interface IGateInService : IDisposable
    {
        GateIn Create(GateIn pt);
        void Delete(int id);
        void Delete(GateIn pt);
        GateIn Find(string Name);
        GateIn Find(int id);
        IEnumerable<GateIn> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(GateIn pt);
        GateIn Add(GateIn pt);
        IEnumerable<GateIn> GetGateInList(int SiteId);

        // IEnumerable<GateIn> GetGateInList(int buyerId);
        Task<IEquatable<GateIn>> GetAsync();
        Task<GateIn> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<GateInListViewModel> GetPendingGateIns(int id);//SupplierId
    }

    public class GateInService : IGateInService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<GateIn> _GateInRepository;
        RepositoryQuery<GateIn> GateInRepository;
        public GateInService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GateInRepository = new Repository<GateIn>(db);
            GateInRepository = new RepositoryQuery<GateIn>(_GateInRepository);
        }

        public GateIn Find(string Name)
        {
            return GateInRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public GateIn Find(int id)
        {
            return _unitOfWork.Repository<GateIn>().Find(id);
        }

        public GateIn Create(GateIn pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<GateIn>().Insert(pt);
            return pt;
        }
        public IEnumerable<GateInListViewModel> GetPendingGateIns(int id)//SupplierId
        {

            var TES=from p in db.GateIn
                    join t in db.PurchaseGoodsReceiptHeader on p.GateInId equals t.GateInId into table
                    from tab in table.DefaultIfEmpty()                    
                    where p.GateInId==null
                    orderby p.DocNo
                    select new GateInListViewModel
                    {
                        DocNo = p.DocNo,
                        GateInId = p.GateInId
                    };

            return (TES);
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<GateIn>().Delete(id);
        }

        public void Delete(GateIn pt)
        {
            _unitOfWork.Repository<GateIn>().Delete(pt);
        }

        public void Update(GateIn pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<GateIn>().Update(pt);
        }

        public IEnumerable<GateIn> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<GateIn>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<GateIn> GetGateInList(int SiteId)
        {
            var pt = _unitOfWork.Repository<GateIn>().Query().Get().OrderBy(m => m.DocNo).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public GateIn Add(GateIn pt)
        {
            _unitOfWork.Repository<GateIn>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.GateIn
                        orderby p.DocNo
                        select p.GateInId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.GateIn
                        orderby p.DocNo
                        select p.GateInId).FirstOrDefault();
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

                temp = (from p in db.GateIn
                        orderby p.DocNo
                        select p.GateInId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.GateIn
                        orderby p.DocNo
                        select p.GateInId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<GateIn>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GateIn> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
