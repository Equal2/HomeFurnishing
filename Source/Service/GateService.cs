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

namespace Service
{
    public interface IGateService : IDisposable
    {
        Gate Create(Gate pt);
        void Delete(int id);
        void Delete(Gate pt);
        Gate Find(string Name);
        Gate Find(int id);
        IEnumerable<Gate> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Gate pt);
        Gate Add(Gate pt);
        IEnumerable<Gate> GetGateList(int SiteId);

        // IEnumerable<Gate> GetGateList(int buyerId);
        Task<IEquatable<Gate>> GetAsync();
        Task<Gate> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class GateService : IGateService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Gate> _GateRepository;
        RepositoryQuery<Gate> GateRepository;
        public GateService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GateRepository = new Repository<Gate>(db);
            GateRepository = new RepositoryQuery<Gate>(_GateRepository);
        }

        public Gate Find(string Name)
        {
            return GateRepository.Get().Where(i => i.GateName == Name).FirstOrDefault();
        }


        public Gate Find(int id)
        {
            return _unitOfWork.Repository<Gate>().Find(id);
        }

        public Gate Create(Gate pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Gate>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Gate>().Delete(id);
        }

        public void Delete(Gate pt)
        {
            _unitOfWork.Repository<Gate>().Delete(pt);
        }

        public void Update(Gate pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Gate>().Update(pt);
        }

        public IEnumerable<Gate> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Gate>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.GateName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Gate> GetGateList(int SiteId)
        {
            var pt = _unitOfWork.Repository<Gate>().Query().Get().OrderBy(m => m.GateName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public Gate Add(Gate pt)
        {
            _unitOfWork.Repository<Gate>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Gate
                        orderby p.GateName
                        select p.GateId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Gate
                        orderby p.GateName
                        select p.GateId).FirstOrDefault();
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

                temp = (from p in db.Gate
                        orderby p.GateName
                        select p.GateId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Gate
                        orderby p.GateName
                        select p.GateId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<Gate>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Gate> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
