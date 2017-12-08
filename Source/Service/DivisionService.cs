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
    public interface IDivisionService : IDisposable
    {
        Division Create(Division pt);
        void Delete(int id);
        void Delete(Division pt);
        Division Find(string Name);
        Division Find(int id);
        IEnumerable<Division> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Division pt);
        Division Add(Division pt);
        IQueryable<Division> GetDivisionList();
        Task<IEquatable<Division>> GetAsync();
        Task<Division> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DivisionService : IDivisionService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Division> _DivisionRepository;
        RepositoryQuery<Division> DivisionRepository;
        public DivisionService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DivisionRepository = new Repository<Division>(db);
            DivisionRepository = new RepositoryQuery<Division>(_DivisionRepository);
        }

        public Division Find(string Name)
        {
            return DivisionRepository.Get().Where(i => i.DivisionName == Name).FirstOrDefault();
        }


        public Division Find(int id)
        {
            return _unitOfWork.Repository<Division>().Find(id);
        }

        public Division Create(Division pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Division>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Division>().Delete(id);
        }

        public void Delete(Division pt)
        {
            _unitOfWork.Repository<Division>().Delete(pt);
        }

        public void Update(Division pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Division>().Update(pt);
        }

        public IEnumerable<Division> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Division>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DivisionName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Division> GetDivisionList()
        {
            var pt = _unitOfWork.Repository<Division>().Query().Get().OrderBy(m=>m.DivisionName);

            return pt;
        }
        public IQueryable<Division> GetDivisionList(string RoleIds)
        {
            //var pt = from p in db.Divisions
            //         join t in db.RolesDivision on p.DivisionId equals t.DivisionId
            //         where RoleIds.Contains(t.RoleId)
            //         orderby p.DivisionName
            //         select p;

            var temp = from p in db.Divisions
                       join t in db.RolesDivision on p.DivisionId equals t.DivisionId
                       where RoleIds.Contains(t.RoleId)
                       group p by p.DivisionId into g
                       select g.FirstOrDefault();
               

            return temp;
        }

        public Division Add(Division pt)
        {
            _unitOfWork.Repository<Division>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Divisions
                        orderby p.DivisionName
                        select p.DivisionId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Divisions
                        orderby p.DivisionName
                        select p.DivisionId).FirstOrDefault();
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

                temp = (from p in db.Divisions
                        orderby p.DivisionName
                        select p.DivisionId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Divisions
                        orderby p.DivisionName
                        select p.DivisionId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Division>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Division> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
