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
    public interface ILocationService : IDisposable
    {
        Location Create(Location pt);
        void Delete(int id);
        void Delete(Location pt);
        Location GetLocation(int ptId);
        Location Find(string Name);
        Location Find(int id);
        IEnumerable<Location> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Location pt);
        Location Add(Location pt);
        IEnumerable<Location> GetLocationList();

        // IEnumerable<Location> GetLocationList(int buyerId);
        Task<IEquatable<Location>> GetAsync();
        Task<Location> FindAsync(int id);
    }

    public class LocationService : ILocationService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Location> _LocationRepository;
        RepositoryQuery<Location> LocationRepository;
        public LocationService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LocationRepository = new Repository<Location>(db);
            LocationRepository = new RepositoryQuery<Location>(_LocationRepository);
        }

        public Location GetLocation(int pt)
        {
            //return LocationRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            return _unitOfWork.Repository<Location>().Find(pt);
        }

        public Location Find(string Name)
        {
            return LocationRepository.Get().Where(i => i.LocationName == Name).FirstOrDefault();
        }


        public Location Find(int id)
        {
            return _unitOfWork.Repository<Location>().Find(id);
        }

        public Location Create(Location pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Location>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Location>().Delete(id);
        }

        public void Delete(Location pt)
        {
            _unitOfWork.Repository<Location>().Delete(pt);
        }

        public void Update(Location pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Location>().Update(pt);
        }

        public IEnumerable<Location> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Location>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.LocationName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Location> GetLocationList()
        {
            var pt = _unitOfWork.Repository<Location>().Query().Get();

            return pt;
        }

        public Location Add(Location pt)
        {
            _unitOfWork.Repository<Location>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Location>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Location> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
