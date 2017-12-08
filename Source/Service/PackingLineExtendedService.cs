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
    public interface IPackingLineExtendedService : IDisposable
    {
        PackingLineExtended Create(PackingLineExtended pt);
        void Delete(int id);
        void Delete(PackingLineExtended pt);
        PackingLineExtended Find(int id);
        IEnumerable<PackingLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PackingLineExtended pt);
        PackingLineExtended Add(PackingLineExtended pt);
        IEnumerable<PackingLineExtended> GetPackingLineExtendedList();

        // IEnumerable<PackingLineExtended> GetPackingLineExtendedList(int buyerId);
        Task<IEquatable<PackingLineExtended>> GetAsync();
        Task<PackingLineExtended> FindAsync(int id);
        void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased);        
    }

    public class PackingLineExtendedService : IPackingLineExtendedService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PackingLineExtended> _PackingLineExtendedRepository;
        RepositoryQuery<PackingLineExtended> PackingLineExtendedRepository;
        public PackingLineExtendedService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PackingLineExtendedRepository = new Repository<PackingLineExtended>(db);
            PackingLineExtendedRepository = new RepositoryQuery<PackingLineExtended>(_PackingLineExtendedRepository);
        }


        public PackingLineExtended Find(int id)
        {
            return _unitOfWork.Repository<PackingLineExtended>().Find(id);
        }

        public PackingLineExtended Create(PackingLineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PackingLineExtended>().Insert(pt);
            return pt;
        }

        public PackingLineExtended DBCreate(PackingLineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.PackingLineExtended.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PackingLineExtended>().Delete(id);
        }

        public void Delete(PackingLineExtended pt)
        {
            _unitOfWork.Repository<PackingLineExtended>().Delete(pt);
        }

        public void Update(PackingLineExtended pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PackingLineExtended>().Update(pt);
        }

        public IEnumerable<PackingLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PackingLineExtended>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PackingLineExtended> GetPackingLineExtendedList()
        {
            var pt = _unitOfWork.Repository<PackingLineExtended>().Query().Get();

            return pt;
        }

        public PackingLineExtended Add(PackingLineExtended pt)
        {
            _unitOfWork.Repository<PackingLineExtended>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased)
        {
            PackingLineExtended Stat = new PackingLineExtended();
            Stat.PackingLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            if (IsDBbased)
                context.PackingLineExtended.Add(Stat);
            else
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            PackingLineExtended Stat = Find(id);
            Delete(Stat);
        }     

        public void Dispose()
        {
        }


        public Task<IEquatable<PackingLineExtended>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PackingLineExtended> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
