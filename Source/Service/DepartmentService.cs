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
    public interface IDepartmentService : IDisposable
    {
        Department Create(Department pt);
        void Delete(int id);
        void Delete(Department pt);
        Department Find(string Name);
        Department Find(int id);      
        void Update(Department pt);
        Department Add(Department pt);
        IEnumerable<Department> GetDepartmentList();
        Department GetDepartmentByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DepartmentService : IDepartmentService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Department> _DepartmentRepository;
        RepositoryQuery<Department> DepartmentRepository;
        public DepartmentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DepartmentRepository = new Repository<Department>(db);
            DepartmentRepository = new RepositoryQuery<Department>(_DepartmentRepository);
        }
        public Department GetDepartmentByName(string terms)
        {
            return (from p in db.Department
                    where p.DepartmentName == terms
                    select p).FirstOrDefault();
        }

        public Department Find(string Name)
        {
            return DepartmentRepository.Get().Where(i => i.DepartmentName == Name).FirstOrDefault();
        }


        public Department Find(int id)
        {
            return _unitOfWork.Repository<Department>().Find(id);
        }

        public Department Create(Department pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Department>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Department>().Delete(id);
        }

        public void Delete(Department pt)
        {
            _unitOfWork.Repository<Department>().Delete(pt);
        }

        public void Update(Department pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Department>().Update(pt);
        }

        public IEnumerable<Department> GetDepartmentList()
        {
            var pt = (from p in db.Department
                      orderby p.DepartmentName
                      select p
                          );

            return pt;
        }

        public Department Add(Department pt)
        {
            _unitOfWork.Repository<Department>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Department
                        orderby p.DepartmentName
                        select p.DepartmentId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Department
                        orderby p.DepartmentName
                        select p.DepartmentId).FirstOrDefault();
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

                temp = (from p in db.Department
                        orderby p.DepartmentName
                        select p.DepartmentId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Department
                        orderby p.DepartmentName
                        select p.DepartmentId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }

    }
}
