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
    public interface IDimension4Service : IDisposable
    {
        Dimension4 Create(Dimension4 pt);
        void Delete(int id);
        void Delete(Dimension4 pt);
        Dimension4 Find(string Name);
        Dimension4 Find(int id);
        IEnumerable<Dimension4> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Dimension4 pt);
        Dimension4 Add(Dimension4 pt);
        IQueryable<Dimension4> GetDimension4List(int id);

        // IEnumerable<Dimension4> GetDimension4List(int buyerId);
        Task<IEquatable<Dimension4>> GetAsync();
        Task<Dimension4> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
    }

    public class Dimension4Service : IDimension4Service
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Dimension4> _Dimension4Repository;
        RepositoryQuery<Dimension4> Dimension4Repository;
        public Dimension4Service(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _Dimension4Repository = new Repository<Dimension4>(db);
            Dimension4Repository = new RepositoryQuery<Dimension4>(_Dimension4Repository);
        }


        public Dimension4 Find(string Name)
        {            
            return Dimension4Repository.Get().Where(i => i.Dimension4Name == Name).FirstOrDefault();
        }


        public Dimension4 Find(int id)
        {
            return _unitOfWork.Repository<Dimension4>().Find(id);            
        }

        public Dimension4 Create(Dimension4 pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Dimension4>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Dimension4>().Delete(id);
        }

        public void Delete(Dimension4 pt)
        {
            _unitOfWork.Repository<Dimension4>().Delete(pt);
        }

        public void Update(Dimension4 pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Dimension4>().Update(pt);
        }

        public IEnumerable<Dimension4> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Dimension4>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Dimension4Name))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Dimension4> GetDimension4List(int id)
        {
            //var pt = _unitOfWork.Repository<Dimension4>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.Dimension4Name);

            return ( from p in db.Dimension4
                         where p.ProductTypeId==id 
                         orderby p.Dimension4Name
                         select p
                         );
        }

        public Dimension4 Add(Dimension4 pt)
        {
            _unitOfWork.Repository<Dimension4>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Dimension4
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension4Name
                        select p.Dimension4Id).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension4
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension4Name
                        select p.Dimension4Id).FirstOrDefault();
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

                temp = (from p in db.Dimension4
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension4Name
                        select p.Dimension4Id).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension4
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension4Name
                        select p.Dimension4Id).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Dimension4>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dimension4> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
