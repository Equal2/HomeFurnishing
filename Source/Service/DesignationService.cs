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
    public interface IDesignationService : IDisposable
    {
        Designation Create(Designation pt);
        void Delete(int id);
        void Delete(Designation pt);
        IEnumerable<Designation> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Designation pt);
        Designation Add(Designation pt);
        IEnumerable<Designation> GetDesignationList();

        // IEnumerable<Designation> GetDesignationList(int buyerId);
        Task<IEquatable<Designation>> GetAsync();
        Task<Designation> FindAsync(int id);
        Designation Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DesignationService : IDesignationService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Designation> _DesignationRepository;
        RepositoryQuery<Designation> DesignationRepository;
        public DesignationService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DesignationRepository = new Repository<Designation>(db);
            DesignationRepository = new RepositoryQuery<Designation>(_DesignationRepository);
        }

        public Designation Find(int id)
        {
            return _unitOfWork.Repository<Designation>().Find(id);
        }

        public Designation Create(Designation pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Designation>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Designation>().Delete(id);
        }

        public void Delete(Designation pt)
        {
            _unitOfWork.Repository<Designation>().Delete(pt);
        }

        public void Update(Designation pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Designation>().Update(pt);
        }

        public IEnumerable<Designation> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Designation>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DesignationName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Designation> GetDesignationList()
        {
            var pt = _unitOfWork.Repository<Designation>().Query().Get().OrderBy(M=>M.DesignationName).ToList();

            return pt;
        }

        public Designation Add(Designation pt)
        {
            _unitOfWork.Repository<Designation>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Designation
                        orderby p.DesignationName
                        select p.DesignationId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Designation
                        orderby p.DesignationName
                        select p.DesignationId).FirstOrDefault();
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

                temp = (from p in db.Designation
                        orderby p.DesignationName
                        select p.DesignationId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Designation
                        orderby p.DesignationName
                        select p.DesignationId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Designation>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Designation> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
