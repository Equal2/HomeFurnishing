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
    public interface IDimension2Service : IDisposable
    {
        Dimension2 Create(Dimension2 pt);
        void Delete(int id);
        void Delete(Dimension2 pt);
        Dimension2 Find(string Name);
        Dimension2 Find(int id);
        IEnumerable<Dimension2> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Dimension2 pt);
        Dimension2 Add(Dimension2 pt);
        IQueryable<Dimension2> GetDimension2List(int id);

        // IEnumerable<Dimension2> GetDimension2List(int buyerId);
        Task<IEquatable<Dimension2>> GetAsync();
        Task<Dimension2> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
    }

    public class Dimension2Service : IDimension2Service
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Dimension2> _Dimension2Repository;
        RepositoryQuery<Dimension2> Dimension2Repository;
        public Dimension2Service(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _Dimension2Repository = new Repository<Dimension2>(db);
            Dimension2Repository = new RepositoryQuery<Dimension2>(_Dimension2Repository);
        }


        public Dimension2 Find(string Name)
        {            
            return Dimension2Repository.Get().Where(i => i.Dimension2Name == Name).FirstOrDefault();
        }


        public Dimension2 Find(int id)
        {
            return _unitOfWork.Repository<Dimension2>().Find(id);            
        }

        public Dimension2 Create(Dimension2 pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Dimension2>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Dimension2>().Delete(id);
        }

        public void Delete(Dimension2 pt)
        {
            _unitOfWork.Repository<Dimension2>().Delete(pt);
        }

        public void Update(Dimension2 pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Dimension2>().Update(pt);
        }

        public IEnumerable<Dimension2> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Dimension2>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Dimension2Name))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Dimension2> GetDimension2List(int id)
        {
            //var pt = _unitOfWork.Repository<Dimension2>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.Dimension2Name);

            return ( from p in db.Dimension2
                         where p.ProductTypeId==id 
                         orderby p.Dimension2Name
                         select p
                         );
        }

        public Dimension2 Add(Dimension2 pt)
        {
            _unitOfWork.Repository<Dimension2>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Dimension2
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension2
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).FirstOrDefault();
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

                temp = (from p in db.Dimension2
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension2
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Dimension2>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dimension2> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
