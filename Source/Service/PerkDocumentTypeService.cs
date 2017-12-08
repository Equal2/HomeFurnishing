using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public interface IPerkDocumentTypeService : IDisposable
    {
        PerkDocumentType Create(PerkDocumentType pt);
        void Delete(int id);
        void Delete(PerkDocumentType pt);
        PerkDocumentType Find(int id);
        IEnumerable<PerkDocumentType> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PerkDocumentType pt);
        PerkDocumentType Add(PerkDocumentType pt);
        IEnumerable<PerkDocumentType> GetPerkDocumentTypeList();
        Task<IEquatable<PerkDocumentType>> GetAsync();
        Task<PerkDocumentType> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class PerkDocumentTypeService : IPerkDocumentTypeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PerkDocumentType> _PerkDocumentTypeRepository;
        RepositoryQuery<PerkDocumentType> PerkDocumentTypeRepository;
        public PerkDocumentTypeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PerkDocumentTypeRepository = new Repository<PerkDocumentType>(db);
            PerkDocumentTypeRepository = new RepositoryQuery<PerkDocumentType>(_PerkDocumentTypeRepository);
        }


        public PerkDocumentType Find(int id)
        {
            return _unitOfWork.Repository<PerkDocumentType>().Find(id);
        }

        public PerkDocumentType Create(PerkDocumentType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PerkDocumentType>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PerkDocumentType>().Delete(id);
        }

        public void Delete(PerkDocumentType pt)
        {
            _unitOfWork.Repository<PerkDocumentType>().Delete(pt);
        }

        public void Update(PerkDocumentType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PerkDocumentType>().Update(pt);
        }

        public IEnumerable<PerkDocumentType> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PerkDocumentType>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PerkDocumentTypeId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PerkDocumentType> GetPerkDocumentTypeList()
        {
            var pt = _unitOfWork.Repository<PerkDocumentType>().Query().Get().OrderBy(m=>m.PerkDocumentTypeId);

            return pt;
        }

        public PerkDocumentType Add(PerkDocumentType pt)
        {
            _unitOfWork.Repository<PerkDocumentType>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PerkDocumentType
                        orderby p.PerkDocumentTypeId
                        select p.PerkDocumentTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PerkDocumentType
                        orderby p.PerkDocumentTypeId
                        select p.PerkDocumentTypeId).FirstOrDefault();
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

                temp = (from p in db.PerkDocumentType
                        orderby p.PerkDocumentTypeId
                        select p.PerkDocumentTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PerkDocumentType
                        orderby p.PerkDocumentTypeId
                        select p.PerkDocumentTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PerkDocumentType>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PerkDocumentType> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
