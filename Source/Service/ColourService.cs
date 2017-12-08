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
    public interface IColourService : IDisposable
    {
        Colour Create(Colour pt);
        void Delete(int id);
        void Delete(Colour pt);
        Colour Find(string Name);
        Colour Find(int id);
        IEnumerable<Colour> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Colour pt);
        Colour Add(Colour pt);
        IEnumerable<Colour> GetColourList();

        // IEnumerable<Colour> GetColourList(int buyerId);
        Task<IEquatable<Colour>> GetAsync();
        Task<Colour> FindAsync(int id);
        Colour GetColourByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ColourService : IColourService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Colour> _ColourRepository;
        RepositoryQuery<Colour> ColourRepository;
        public ColourService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ColourRepository = new Repository<Colour>(db);
            ColourRepository = new RepositoryQuery<Colour>(_ColourRepository);
        }
        public Colour GetColourByName(string terms)
        {
            return (from p in db.Colour
                    where p.ColourName == terms
                    select p).FirstOrDefault();
        }

        public Colour Find(string Name)
        {
            return ColourRepository.Get().Where(i => i.ColourName == Name).FirstOrDefault();
        }


        public Colour Find(int id)
        {
            return _unitOfWork.Repository<Colour>().Find(id);
        }

        public Colour Create(Colour pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Colour>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Colour>().Delete(id);
        }

        public void Delete(Colour pt)
        {
            _unitOfWork.Repository<Colour>().Delete(pt);
        }

        public void Update(Colour pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Colour>().Update(pt);
        }

        public IEnumerable<Colour> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Colour>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ColourName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Colour> GetColourList()
        {
            var pt = _unitOfWork.Repository<Colour>().Query().Get().OrderBy(m=>m.ColourName);

            return pt;
        }

        public Colour Add(Colour pt)
        {
            _unitOfWork.Repository<Colour>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Colour
                        orderby p.ColourName
                        select p.ColourId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Colour
                        orderby p.ColourName
                        select p.ColourId).FirstOrDefault();
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

                temp = (from p in db.Colour
                        orderby p.ColourName
                        select p.ColourId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Colour
                        orderby p.ColourName
                        select p.ColourId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Colour>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Colour> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
