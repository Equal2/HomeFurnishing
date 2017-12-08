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
    public interface IDimension3Service : IDisposable
    {
        Dimension3 Create(Dimension3 pt);
        void Delete(int id);
        void Delete(Dimension3 pt);
        Dimension3 Find(string Name);
        Dimension3 Find(int id);
        IEnumerable<Dimension3> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Dimension3 pt);
        Dimension3 Add(Dimension3 pt);
        IQueryable<Dimension3> GetDimension3List(int id);

        // IEnumerable<Dimension3> GetDimension3List(int buyerId);
        Task<IEquatable<Dimension3>> GetAsync();
        Task<Dimension3> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
    }

    public class Dimension3Service : IDimension3Service
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Dimension3> _Dimension3Repository;
        RepositoryQuery<Dimension3> Dimension3Repository;
        public Dimension3Service(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _Dimension3Repository = new Repository<Dimension3>(db);
            Dimension3Repository = new RepositoryQuery<Dimension3>(_Dimension3Repository);
        }


        public Dimension3 Find(string Name)
        {            
            return Dimension3Repository.Get().Where(i => i.Dimension3Name == Name).FirstOrDefault();
        }


        public Dimension3 Find(int id)
        {
            return _unitOfWork.Repository<Dimension3>().Find(id);            
        }

        public Dimension3 Create(Dimension3 pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Dimension3>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Dimension3>().Delete(id);
        }

        public void Delete(Dimension3 pt)
        {
            _unitOfWork.Repository<Dimension3>().Delete(pt);
        }

        public void Update(Dimension3 pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Dimension3>().Update(pt);
        }

        public IEnumerable<Dimension3> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Dimension3>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Dimension3Name))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Dimension3> GetDimension3List(int id)
        {
            //var pt = _unitOfWork.Repository<Dimension3>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.Dimension3Name);

            return ( from p in db.Dimension3
                         where p.ProductTypeId==id 
                         orderby p.Dimension3Name
                         select p
                         );
        }

        public Dimension3 Add(Dimension3 pt)
        {
            _unitOfWork.Repository<Dimension3>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Dimension3
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension3Name
                        select p.Dimension3Id).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension3
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension3Name
                        select p.Dimension3Id).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id,int ptypeid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.Dimension3
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension3Name
                        select p.Dimension3Id).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension3
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension3Name
                        select p.Dimension3Id).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Dimension3>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dimension3> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
