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
    public interface ISizeService : IDisposable
    {
        Size Create(Size pt);
        void Delete(int id);
        void Delete(Size pt);
        Size Find(string Name);
        Size Find(int id);
        IEnumerable<Size> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        Size GetSizeByName(string Name);
        void Update(Size pt);
        Size Add(Size pt);
        IQueryable<Size> GetSizeList();

        // IEnumerable<Size> GetSizeList(int buyerId);
        Task<IEquatable<Size>> GetAsync();
        Task<Size> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        bool CheckSizeExists(int id);
    }

    public class SizeService : ISizeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Size> _SizeRepository;
        RepositoryQuery<Size> SizeRepository;
        public SizeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SizeRepository = new Repository<Size>(db);
            SizeRepository = new RepositoryQuery<Size>(_SizeRepository);
        }


        public Size GetSizeByName(string name)
        {
            return (from p in db.Size
                    where p.SizeName == name
                    select p
                        ).FirstOrDefault();
        }

        public Size Find(string Name)
        {
            return SizeRepository.Get().Where(i => i.SizeName == Name).FirstOrDefault();
        }


        public Size Find(int id)
        {
            return _unitOfWork.Repository<Size>().Find(id);
        }

        public Size Create(Size pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Size>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Size>().Delete(id);
        }

        public void Delete(Size pt)
        {
            _unitOfWork.Repository<Size>().Delete(pt);
        }

        public void Update(Size pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Size>().Update(pt);
        }

        public IEnumerable<Size> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Size>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SizeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Size> GetSizeList()
        {
            var pt = _unitOfWork.Repository<Size>().Query().Get().OrderBy(m=>m.SizeName);

            return pt;
        }

        public Size Add(Size pt)
        {
            _unitOfWork.Repository<Size>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Size
                        orderby p.SizeName
                        select p.SizeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Size
                        orderby p.SizeName
                        select p.SizeId).FirstOrDefault();
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

                temp = (from p in db.Size
                        orderby p.SizeName
                        select p.SizeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Size
                        orderby p.SizeName
                        select p.SizeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public bool CheckSizeExists(int id)
        {
            return (from p in db.ProductSize
                    where p.SizeId == id
                    select p).Any();
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Size>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Size> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
