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
    public interface INarrationService : IDisposable
    {
        Narration Create(Narration pt);
        void Delete(int id);
        void Delete(Narration pt);
        Narration Find(string Name);
        Narration Find(int id);
        IEnumerable<Narration> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Narration pt);
        Narration Add(Narration pt);
        IEnumerable<Narration> GetNarrationList();

        // IEnumerable<Narration> GetNarrationList(int buyerId);
        Task<IEquatable<Narration>> GetAsync();
        Task<Narration> FindAsync(int id);
        Narration GetNarrationByName(string terms);
        IEnumerable<string> GetNarrationNames();
        int NextId(int id);
        int PrevId(int id);
    }

    public class NarrationService : INarrationService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Narration> _NarrationRepository;
        RepositoryQuery<Narration> NarrationRepository;
        public NarrationService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _NarrationRepository = new Repository<Narration>(db);
            NarrationRepository = new RepositoryQuery<Narration>(_NarrationRepository);
        }
        public Narration GetNarrationByName(string terms)
        {
            return (from p in db.Narration
                    where p.NarrationName == terms
                    select p).FirstOrDefault();
        }

        public Narration Find(string Name)
        {
            return NarrationRepository.Get().Where(i => i.NarrationName == Name).FirstOrDefault();
        }


        public Narration Find(int id)
        {
            return _unitOfWork.Repository<Narration>().Find(id);
        }

        public Narration Create(Narration pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Narration>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Narration>().Delete(id);
        }

        public void Delete(Narration pt)
        {
            _unitOfWork.Repository<Narration>().Delete(pt);
        }

        public void Update(Narration pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Narration>().Update(pt);
        }

        public IEnumerable<Narration> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Narration>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.NarrationName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Narration> GetNarrationList()
        {
            var pt = _unitOfWork.Repository<Narration>().Query().Get().OrderBy(m=>m.NarrationName);

            return pt;
        }
        public IEnumerable<string> GetNarrationNames()
        {
            return (from p in db.Narration
                    orderby p.NarrationName
                    select p.NarrationName
                        );
        }

        public Narration Add(Narration pt)
        {
            _unitOfWork.Repository<Narration>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Narration
                        orderby p.NarrationName
                        select p.NarrationId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Narration
                        orderby p.NarrationName
                        select p.NarrationId).FirstOrDefault();
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

                temp = (from p in db.Narration
                        orderby p.NarrationName
                        select p.NarrationId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Narration
                        orderby p.NarrationName
                        select p.NarrationId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Narration>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Narration> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
