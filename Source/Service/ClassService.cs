using System.Collections.Generic;
using System.Linq;
using Surya.India.Data;
using Surya.India.Data.Infrastructure;
using Surya.India.Model.Models;

using Surya.India.Core.Common;
using System;
using Surya.India.Model;
using System.Threading.Tasks;
using Surya.India.Data.Models;

namespace Surya.India.Service
{
    public interface IClassService : IDisposable
    {
        Sch_Class Create(Sch_Class pt);
        void Delete(int id);
        void Delete(Sch_Class pt);
        Sch_Class Find(string Name);
        Sch_Class Find(int id);
        IEnumerable<Sch_Class> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Sch_Class pt);
        Sch_Class Add(Sch_Class pt);
        IEnumerable<Sch_Class> GetClassList();

        IEnumerable<Sch_Class> GetClassList(int ProgramId);

        // IEnumerable<Sch_Class> GetClassList(int buyerId);
        Task<IEquatable<Sch_Class>> GetAsync();
        Task<Sch_Class> FindAsync(int id);
        Sch_Class GetClassByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ClassService : IClassService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Sch_Class> _ClassRepository;
        RepositoryQuery<Sch_Class> ClassRepository;
        public ClassService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ClassRepository = new Repository<Sch_Class>(db);
            ClassRepository = new RepositoryQuery<Sch_Class>(_ClassRepository);
        }
        public Sch_Class GetClassByName(string terms)
        {
            return (from p in db.Sch_Class
                    where p.ClassName == terms
                    select p).FirstOrDefault();
        }

        public Sch_Class Find(string Name)
        {
            return ClassRepository.Get().Where(i => i.ClassName == Name).FirstOrDefault();
        }


        public Sch_Class Find(int id)
        {
            return _unitOfWork.Repository<Sch_Class>().Find(id);
        }

        public Sch_Class Create(Sch_Class pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Sch_Class>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Sch_Class>().Delete(id);
        }

        public void Delete(Sch_Class pt)
        {
            _unitOfWork.Repository<Sch_Class>().Delete(pt);
        }

        public void Update(Sch_Class pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Sch_Class>().Update(pt);
        }

        public IEnumerable<Sch_Class> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Sch_Class>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ClassName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Sch_Class> GetClassList()
        {
            var pt = _unitOfWork.Repository<Sch_Class>().Query().Get().OrderBy(m=>m.ClassName);

            return pt;
        }

        public IEnumerable<Sch_Class> GetClassList(int ProgramId)
        {
            var pt = _unitOfWork.Repository<Sch_Class>().Query().Get().Where(i => i.ProgramId == ProgramId).OrderBy(m => m.ClassName);

            return pt;
        }

        public Sch_Class Add(Sch_Class pt)
        {
            _unitOfWork.Repository<Sch_Class>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Sch_Class
                        orderby p.ClassName
                        select p.ClassId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Sch_Class
                        orderby p.ClassName
                        select p.ClassId).FirstOrDefault();
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

                temp = (from p in db.Sch_Class
                        orderby p.ClassName
                        select p.ClassId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Sch_Class
                        orderby p.ClassName
                        select p.ClassId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Sch_Class>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Sch_Class> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
