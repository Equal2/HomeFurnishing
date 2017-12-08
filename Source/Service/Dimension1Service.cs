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
using Model.ViewModels;

namespace Service
{
    public interface IDimension1Service : IDisposable
    {
        Dimension1 Create(Dimension1 pt);
        void Delete(int id);
        void Delete(Dimension1 pt);
        Dimension1 Find(string Name);
        Dimension1 Find(int id);
        IEnumerable<Dimension1> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Dimension1 pt);
        Dimension1 Add(Dimension1 pt);
        IQueryable<Dimension1> GetDimension1List(int id);
        IQueryable<Dimension1ViewModel> GetDimension1ViewModelList(int id);

        // IEnumerable<Dimension1> GetDimension1List(int buyerId);
        Task<IEquatable<Dimension1>> GetAsync();
        Task<Dimension1> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
    }

    public class Dimension1Service : IDimension1Service
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Dimension1> _Dimension1Repository;
        RepositoryQuery<Dimension1> Dimension1Repository;
        public Dimension1Service(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _Dimension1Repository = new Repository<Dimension1>(db);
            Dimension1Repository = new RepositoryQuery<Dimension1>(_Dimension1Repository);
        }


        public Dimension1 Find(string Name)
        {            
            return Dimension1Repository.Get().Where(i => i.Dimension1Name == Name).FirstOrDefault();
        }


        public Dimension1 Find(int id)
        {
            return _unitOfWork.Repository<Dimension1>().Find(id);            
        }

        public Dimension1 Create(Dimension1 pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Dimension1>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Dimension1>().Delete(id);
        }

        public void Delete(Dimension1 pt)
        {
            _unitOfWork.Repository<Dimension1>().Delete(pt);
        }

        public void Update(Dimension1 pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Dimension1>().Update(pt);
        }

        public IEnumerable<Dimension1> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Dimension1>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Dimension1Name))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Dimension1> GetDimension1List(int id)
        {
            //var pt = _unitOfWork.Repository<Dimension1>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.Dimension1Name);

            return ( from p in db.Dimension1
                         where p.ProductTypeId==id 
                         orderby p.Dimension1Name
                         select p
                         );
        }


        public IQueryable<Dimension1ViewModel> GetDimension1ViewModelList(int id)
        {
            //var pt = _unitOfWork.Repository<Dimension1>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.Dimension1Name);

            return (from p in db.Dimension1
                    where p.ProductTypeId == id
                    orderby p.Dimension1Name
                    select new Dimension1ViewModel
                    {
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1Name,
                        Description = p.Description,
                        IsActive = p.IsActive
                    });
        }

        public Dimension1 Add(Dimension1 pt)
        {
            _unitOfWork.Repository<Dimension1>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Dimension1
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension1
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).FirstOrDefault();
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

                temp = (from p in db.Dimension1
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Dimension1
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Dimension1>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dimension1> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
