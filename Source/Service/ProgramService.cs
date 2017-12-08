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
    public interface IProgramService : IDisposable
    {
        Sch_Program Create(Sch_Program pt);
        void Delete(int id);
        void Delete(Sch_Program pt);
        Sch_Program Find(string Name);
        Sch_Program Find(int id);
        IEnumerable<Sch_Program> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Sch_Program pt);
        Sch_Program Add(Sch_Program pt);
        IEnumerable<Sch_Program> GetProgramList();

        // IEnumerable<Sch_Program> GetProgramList(int buyerId);
        Task<IEquatable<Sch_Program>> GetAsync();
        Task<Sch_Program> FindAsync(int id);
        Sch_Program GetProgramByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProgramService : IProgramService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Sch_Program> _ProgramRepository;
        RepositoryQuery<Sch_Program> ProgramRepository;
        public ProgramService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProgramRepository = new Repository<Sch_Program>(db);
            ProgramRepository = new RepositoryQuery<Sch_Program>(_ProgramRepository);
        }
        public Sch_Program GetProgramByName(string terms)
        {
            return (from p in db.Sch_Program
                    where p.ProgramName == terms
                    select p).FirstOrDefault();
        }

        public Sch_Program Find(string Name)
        {
            return ProgramRepository.Get().Where(i => i.ProgramName == Name).FirstOrDefault();
        }


        public Sch_Program Find(int id)
        {
            return _unitOfWork.Repository<Sch_Program>().Find(id);
        }

        public Sch_Program Create(Sch_Program pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Sch_Program>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Sch_Program>().Delete(id);
        }

        public void Delete(Sch_Program pt)
        {
            _unitOfWork.Repository<Sch_Program>().Delete(pt);
        }

        public void Update(Sch_Program pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Sch_Program>().Update(pt);
        }

        public IEnumerable<Sch_Program> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Sch_Program>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProgramName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Sch_Program> GetProgramList()
        {
            var pt = _unitOfWork.Repository<Sch_Program>().Query().Get().OrderBy(m=>m.ProgramName);

            return pt;
        }

        public Sch_Program Add(Sch_Program pt)
        {
            _unitOfWork.Repository<Sch_Program>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Sch_Program
                        orderby p.ProgramName
                        select p.ProgramId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Sch_Program
                        orderby p.ProgramName
                        select p.ProgramId).FirstOrDefault();
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

                temp = (from p in db.Sch_Program
                        orderby p.ProgramName
                        select p.ProgramId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Sch_Program
                        orderby p.ProgramName
                        select p.ProgramId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Sch_Program>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Sch_Program> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
