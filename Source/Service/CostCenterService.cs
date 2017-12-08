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
    public interface ICostCenterService : IDisposable
    {
        CostCenter Create(CostCenter pt);
        void Delete(int id);
        void Delete(CostCenter pt);
        CostCenter Find(string Name);
        CostCenter Find(int id);
        CostCenter Find(string Name, int DivisionId, int SiteId, int DocTypeId);
        IEnumerable<CostCenter> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(CostCenter pt);
        CostCenter Add(CostCenter pt);
        IEnumerable<CostCenter> GetCostCenterList();

        // IEnumerable<CostCenter> GetCostCenterList(int buyerId);
        Task<IEquatable<CostCenter>> GetAsync();
        Task<CostCenter> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class CostCenterService : ICostCenterService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<CostCenter> _CostCenterRepository;
        RepositoryQuery<CostCenter> CostCenterRepository;
        public CostCenterService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CostCenterRepository = new Repository<CostCenter>(db);
            CostCenterRepository = new RepositoryQuery<CostCenter>(_CostCenterRepository);
        }


        public CostCenter Find(string Name)
        {            
            return CostCenterRepository.Get().Where(i => i.CostCenterName == Name).FirstOrDefault();
        }

        public CostCenter Find(string Name, int DivisionId, int SiteId, int DocTypeId)
        {
            return CostCenterRepository.Get().Where(i => i.CostCenterName == Name && i.DivisionId == DivisionId && i.SiteId == SiteId && i.DocTypeId == DocTypeId).FirstOrDefault();
        }


        public CostCenter Find(int id)
        {
            return _unitOfWork.Repository<CostCenter>().Find(id);            
        }

        public CostCenter Create(CostCenter pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<CostCenter>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<CostCenter>().Delete(id);
        }

        public void Delete(CostCenter pt)
        {
            _unitOfWork.Repository<CostCenter>().Delete(pt);
        }

        public void Update(CostCenter pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<CostCenter>().Update(pt);
        }

        public IEnumerable<CostCenter> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<CostCenter>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CostCenterName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CostCenter> GetCostCenterList()
        {
            var pt = _unitOfWork.Repository<CostCenter>().Query().Get().OrderBy(m=>m.CostCenterName);

            return pt;
        }

        public CostCenter Add(CostCenter pt)
        {
            _unitOfWork.Repository<CostCenter>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.CostCenter
                        orderby p.CostCenterName
                        select p.CostCenterId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.CostCenter
                        orderby p.CostCenterName
                        select p.CostCenterId).FirstOrDefault();
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

                temp = (from p in db.CostCenter
                        orderby p.CostCenterName
                        select p.CostCenterId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.CostCenter
                        orderby p.CostCenterName
                        select p.CostCenterId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<CostCenter>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CostCenter> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
