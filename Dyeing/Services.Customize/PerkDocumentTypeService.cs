using Infrastructure.IO;
using Model;
using Models.Customize.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Customize
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
        PerkDocumentType GetPerkDocumentTypeForPerk(int DocTypeId, int PerkId, int SiteId, int DivisionId);
    }

    public class PerkDocumentTypeService : IPerkDocumentTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PerkDocumentType> _PerkDocumentTypeRepository;
        public PerkDocumentTypeService(IUnitOfWork unitOfWork, IRepository<PerkDocumentType> PerkDocTypeRepo)
        {
            _unitOfWork = unitOfWork;
            _PerkDocumentTypeRepository = PerkDocTypeRepo;
        }
        public PerkDocumentTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PerkDocumentTypeRepository = unitOfWork.Repository<PerkDocumentType>();
        }

        public PerkDocumentType Find(int id)
        {
            return _PerkDocumentTypeRepository.Find(id);
        }

        public PerkDocumentType Create(PerkDocumentType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _PerkDocumentTypeRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _PerkDocumentTypeRepository.Delete(id);
        }

        public void Delete(PerkDocumentType pt)
        {
            _PerkDocumentTypeRepository.Delete(pt);
        }

        public void Update(PerkDocumentType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _PerkDocumentTypeRepository.Update(pt);
        }

        public IEnumerable<PerkDocumentType> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _PerkDocumentTypeRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PerkDocumentTypeId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PerkDocumentType> GetPerkDocumentTypeList()
        {
            var pt = _PerkDocumentTypeRepository.Query().Get().OrderBy(m => m.PerkDocumentTypeId);

            return pt;
        }

        public PerkDocumentType Add(PerkDocumentType pt)
        {
            _PerkDocumentTypeRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _PerkDocumentTypeRepository.Instance
                        orderby p.PerkDocumentTypeId
                        select p.PerkDocumentTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _PerkDocumentTypeRepository.Instance
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

                temp = (from p in _PerkDocumentTypeRepository.Instance
                        orderby p.PerkDocumentTypeId
                        select p.PerkDocumentTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _PerkDocumentTypeRepository.Instance
                        orderby p.PerkDocumentTypeId
                        select p.PerkDocumentTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public PerkDocumentType GetPerkDocumentTypeForPerk(int DocTypeId, int PerkId, int SiteId, int DivisionId)
        {
            return (from p2 in _PerkDocumentTypeRepository.Instance
                    where p2.DocTypeId == DocTypeId && p2.PerkId == PerkId && p2.SiteId == SiteId && p2.DivisionId == DivisionId
                    select p2).FirstOrDefault();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
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
