using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Models.Customize.ViewModels;
using Infrastructure.IO;

namespace Services.Customize
{
    public interface IPerkService : IDisposable
    {
        Perk Create(Perk pt);
        void Delete(int id);
        void Delete(Perk pt);
        Perk Find(string Name);
        Perk Find(int id);
        IEnumerable<Perk> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Perk pt);
        Perk Add(Perk pt);
        IEnumerable<Perk> GetPerkList();
        IEnumerable<PerkViewModel> GetPerkListForDocumentType(int DocTypeId);
        IEnumerable<PerkViewModel> GetPerkListForDocumentTypeForEdit(int JobOrderHeaderId);
        Task<IEquatable<Perk>> GetAsync();
        Task<Perk> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class PerkService : IPerkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Perk> _PerkRepository;
        public PerkService(IUnitOfWork unitOfWork, IRepository<Perk> PerkRepo)
        {
            _unitOfWork = unitOfWork;
            _PerkRepository = PerkRepo;
        }
        public PerkService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PerkRepository = unitOfWork.Repository<Perk>();
        }
        public Perk Find(string Name)
        {
            return _PerkRepository.Query().Get().Where(i => i.PerkName == Name).FirstOrDefault();
        }


        public Perk Find(int id)
        {
            return _PerkRepository.Find(id);
        }

        public IEnumerable<PerkViewModel> GetPerkListForDocumentType(int DocTypeId)
        {
            return (from p in _unitOfWork.Repository<PerkDocumentType>().Instance
                    join t in _PerkRepository.Instance on p.PerkId equals t.PerkId
                    where p.DocTypeId == DocTypeId
                    select new PerkViewModel
                    {
                        Base = t.Base,
                        BaseDescription = t.BaseDescription,
                        CostConversionMultiplier = t.CostConversionMultiplier,
                        IsActive = t.IsActive,
                        PerkId = p.PerkId,
                        PerkName = t.PerkName,
                        Worth = t.Worth,
                        WorthDescription = t.WorthDescription,
                    }
                        );
        }

        public IEnumerable<PerkViewModel> GetPerkListForDocumentTypeForEdit(int JobOrderHeaderId)
        {

            var JobOrder= _unitOfWork.Repository<JobOrderHeader>().Find(JobOrderHeaderId);

            return (from p in _unitOfWork.Repository<JobOrderPerk>().Instance
                    join t in _PerkRepository.Instance on p.PerkId equals t.PerkId
                    join t2 in _unitOfWork.Repository<PerkDocumentType>().Query().Get().Where(m=>m.RateDocId == null ) on new { t.PerkId, JobOrder.DocTypeId } equals new { t2.PerkId,t2.DocTypeId} into PerkDocType
                    from t2 in PerkDocType.DefaultIfEmpty()
                    where p.JobOrderHeaderId == JobOrderHeaderId
                    select new PerkViewModel
                    {
                        JobOrderHeaderId = p.JobOrderHeaderId,
                        JobOrderPerkId = p.JobOrderPerkId,
                        Base = p.Base,
                        BaseDescription = t.BaseDescription,
                        CostConversionMultiplier = t.CostConversionMultiplier,
                        IsActive = t.IsActive,
                        PerkId = p.PerkId,
                        PerkName = t.PerkName,
                        Worth = p.Worth,
                        WorthDescription = t.WorthDescription,
                        IsEditableRate=(t2==null? true : t2.IsEditableRate),
                    }
                        );
        }

        public Perk Create(Perk pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Perk>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Perk>().Delete(id);
        }

        public void Delete(Perk pt)
        {
            _unitOfWork.Repository<Perk>().Delete(pt);
        }

        public void Update(Perk pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Perk>().Update(pt);
        }

        public IEnumerable<Perk> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Perk>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PerkName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Perk> GetPerkList()
        {
            var pt = _unitOfWork.Repository<Perk>().Query().Get().OrderBy(m=>m.PerkName);

            return pt;
        }

        public Perk Add(Perk pt)
        {
            _unitOfWork.Repository<Perk>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _PerkRepository.Instance
                        orderby p.PerkName
                        select p.PerkId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _PerkRepository.Instance
                        orderby p.PerkName
                        select p.PerkId).FirstOrDefault();
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

                temp = (from p in _PerkRepository.Instance
                        orderby p.PerkName
                        select p.PerkId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _PerkRepository.Instance
                        orderby p.PerkName
                        select p.PerkId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        public Task<IEquatable<Perk>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Perk> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
