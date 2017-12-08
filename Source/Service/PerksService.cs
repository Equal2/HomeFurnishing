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
        Task<IEquatable<Perk>> GetAsync();
        Task<Perk> FindAsync(int id);
        Perk GetPerkByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class PerkService : IPerkService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Perk> _PerkRepository;
        RepositoryQuery<Perk> PerkRepository;
        public PerkService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PerkRepository = new Repository<Perk>(db);
            PerkRepository = new RepositoryQuery<Perk>(_PerkRepository);
        }
        public Perk GetPerkByName(string terms)
        {
            return (from p in db.Perk
                    where p.PerkName == terms
                    select p).FirstOrDefault();
        }

        public Perk Find(string Name)
        {
            return PerkRepository.Get().Where(i => i.PerkName == Name).FirstOrDefault();
        }


        public Perk Find(int id)
        {
            return _unitOfWork.Repository<Perk>().Find(id);
        }

        public IEnumerable<PerkViewModel> GetPerkListForDocumentType(int DocTypeId)
        {
            return (from p in db.PerkDocumentType
                    join t in db.Perk on p.PerkId equals t.PerkId
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

            var JobOrder=new JobOrderHeaderService(_unitOfWork).Find(JobOrderHeaderId);

            return (from p in db.JobOrderPerk
                    join t in db.Perk on p.PerkId equals t.PerkId
                    join t2 in db.PerkDocumentType.Where(m=>m.RateDocId == null ) on new { t.PerkId, JobOrder.DocTypeId } equals new { t2.PerkId,t2.DocTypeId} into PerkDocType
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
                temp = (from p in db.Perk
                        orderby p.PerkName
                        select p.PerkId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Perk
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

                temp = (from p in db.Perk
                        orderby p.PerkName
                        select p.PerkId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Perk
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
