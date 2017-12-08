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
using Model.ViewModel;

namespace Service
{
    public interface IRugStencilService : IDisposable
    {
        RugStencil Create(RugStencil pt);
        void Delete(int id);
        void Delete(RugStencil pt);
        RugStencil Find(int id);
        IEnumerable<RugStencil> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(RugStencil pt);
        RugStencil Add(RugStencil pt);
        IEnumerable<ProductDesign> GetRugStencilList();
        IEnumerable<RugStencilViewModel> GetRugSizes(int id);//Design Id

        // IEnumerable<RugStencil> GetRugStencilList(int buyerId);
        Task<IEquatable<RugStencil>> GetAsync();
        Task<RugStencil> FindAsync(int id);
        RugStencil GetRugStencilByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class RugStencilService : IRugStencilService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<RugStencil> _RugStencilRepository;
        RepositoryQuery<RugStencil> RugStencilRepository;
        public RugStencilService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _RugStencilRepository = new Repository<RugStencil>(db);
            RugStencilRepository = new RepositoryQuery<RugStencil>(_RugStencilRepository);
        }
        public RugStencil GetRugStencilByName(string terms)
        {
            return (from p in db.RugStencil                    
                    select p).FirstOrDefault();
        }
        public IEnumerable<RugStencilViewModel> GetRugSizes(int id)//DesignId
        {
            


            return (from p in db.ProductDesigns
                    join t in db.FinishedProduct on p.ProductDesignId equals t.ProductDesignId 
                    join t1 in db.ProductSize on t.ProductId equals t1.ProductId into table1
                    from PS in table1.DefaultIfEmpty()
                    where p.ProductDesignId==id && PS.ProductSizeType.ProductSizeTypeName==SizeTypeConstants.Standard
                    select new RugStencilViewModel
                    {
                        ProductSizeId = PS.SizeId,
                        ProductSizeName = PS.Size.SizeName,
                    }

                        ).Distinct();

        }

        public RugStencil Find(int id)
        {
            return _unitOfWork.Repository<RugStencil>().Find(id);
        }

        public RugStencil Create(RugStencil pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RugStencil>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RugStencil>().Delete(id);
        }

        public void Delete(RugStencil pt)
        {
            _unitOfWork.Repository<RugStencil>().Delete(pt);
        }

        public void Update(RugStencil pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RugStencil>().Update(pt);
        }

        public IEnumerable<RugStencil> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<RugStencil>()
                .Query()                               
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductDesign> GetRugStencilList()
        {
            return (from p in db.RugStencil
                    join t in db.ProductDesigns on p.ProductDesignId equals t.ProductDesignId
                    orderby t.ProductDesignName
                    select t
                        );
        }

        public RugStencil Add(RugStencil pt)
        {
            _unitOfWork.Repository<RugStencil>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RugStencil                        
                        select p.StencilId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RugStencil                        
                        select p.StencilId).FirstOrDefault();
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

                temp = (from p in db.RugStencil                        
                        select p.StencilId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RugStencil                        
                        select p.StencilId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<RugStencil>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RugStencil> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
