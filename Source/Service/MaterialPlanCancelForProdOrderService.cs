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
    public interface IMaterialPlanCancelForProdOrderService : IDisposable
    {
        MaterialPlanCancelForProdOrder Create(MaterialPlanCancelForProdOrder pt);
        void Delete(int id);
        void Delete(MaterialPlanCancelForProdOrder pt);
        MaterialPlanCancelForProdOrder Find(int id);
        IEnumerable<MaterialPlanCancelForProdOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanCancelForProdOrder pt);
        MaterialPlanCancelForProdOrder Add(MaterialPlanCancelForProdOrder pt);
        IEnumerable<MaterialPlanCancelForProdOrder> GetMaterialPlanCancelForProdOrderList();        
        Task<IEquatable<MaterialPlanCancelForProdOrder>> GetAsync();
        Task<MaterialPlanCancelForProdOrder> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<MaterialPlanCancelForProdOrder> GetMaterialPlanCancelForProdORderForMaterialPlanCancel(int MaterialPlanCancelHeaderId);
        int GetMaxSr(int id);
    }

    public class MaterialPlanCancelForProdOrderService : IMaterialPlanCancelForProdOrderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanCancelForProdOrder> _MaterialPlanCancelForProdOrderRepository;
        RepositoryQuery<MaterialPlanCancelForProdOrder> MaterialPlanCancelForProdOrderRepository;
        public MaterialPlanCancelForProdOrderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanCancelForProdOrderRepository = new Repository<MaterialPlanCancelForProdOrder>(db);
            MaterialPlanCancelForProdOrderRepository = new RepositoryQuery<MaterialPlanCancelForProdOrder>(_MaterialPlanCancelForProdOrderRepository);
        }


        public MaterialPlanCancelForProdOrder Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanCancelForProdOrder>().Find(id);
        }

        public MaterialPlanCancelForProdOrder Create(MaterialPlanCancelForProdOrder pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanCancelForProdOrder>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanCancelForProdOrder>().Delete(id);
        }

        public void Delete(MaterialPlanCancelForProdOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelForProdOrder>().Delete(pt);
        }

        public void Update(MaterialPlanCancelForProdOrder pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanCancelForProdOrder>().Update(pt);
        }

        public IEnumerable<MaterialPlanCancelForProdOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanCancelForProdOrder>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanCancelForProdOrderId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanCancelForProdOrder> GetMaterialPlanCancelForProdOrderList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanCancelForProdOrder>().Query().Get().OrderBy(m => m.MaterialPlanCancelForProdOrderId);

            return pt;
        }

        public MaterialPlanCancelForProdOrder Add(MaterialPlanCancelForProdOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelForProdOrder>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanCancelForProdOrder
                        orderby p.MaterialPlanCancelForProdOrderId
                        select p.MaterialPlanCancelForProdOrderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelForProdOrder
                        orderby p.MaterialPlanCancelForProdOrderId
                        select p.MaterialPlanCancelForProdOrderId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanCancelForProdOrder
                        orderby p.MaterialPlanCancelForProdOrderId
                        select p.MaterialPlanCancelForProdOrderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelForProdOrder
                        orderby p.MaterialPlanCancelForProdOrderId
                        select p.MaterialPlanCancelForProdOrderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanCancelForProdOrder> GetMaterialPlanCancelForProdORderForMaterialPlanCancel(int MaterialPlanCancelHeaderId)
        {
            return (from p in db.MaterialPlanCancelForProdOrder
                    where p.MaterialPlanCancelHeaderId == MaterialPlanCancelHeaderId
                    select p);
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanCancelForProdOrder
                       where p.MaterialPlanCancelHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialPlanCancelForProdOrder>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanCancelForProdOrder> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
