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
    public interface IMaterialPlanCancelForSaleOrderService : IDisposable
    {
        MaterialPlanCancelForSaleOrder Create(MaterialPlanCancelForSaleOrder pt);
        void Delete(int id);
        void Delete(MaterialPlanCancelForSaleOrder pt);
        MaterialPlanCancelForSaleOrder Find(int id);
        IEnumerable<MaterialPlanCancelForSaleOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanCancelForSaleOrder pt);
        MaterialPlanCancelForSaleOrder Add(MaterialPlanCancelForSaleOrder pt);
        IEnumerable<MaterialPlanCancelForSaleOrder> GetMaterialPlanCancelForSaleOrderList();        
        Task<IEquatable<MaterialPlanCancelForSaleOrder>> GetAsync();
        Task<MaterialPlanCancelForSaleOrder> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<MaterialPlanCancelForSaleOrder> GetPlanSaleOrderForMaterialPlanCancelLine(int MaterialPlanCancelLineId);
        IEnumerable<MaterialPlanCancelForSaleOrder> GetMaterialPlanCancelForSaleOrderForMaterialPlanCancelline(int MaterialPlanCancelLineId);
        int GetMaxSr(int id);
    }

    public class MaterialPlanCancelForSaleOrderService : IMaterialPlanCancelForSaleOrderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanCancelForSaleOrder> _MaterialPlanCancelForSaleOrderRepository;
        RepositoryQuery<MaterialPlanCancelForSaleOrder> MaterialPlanCancelForSaleOrderRepository;
        public MaterialPlanCancelForSaleOrderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanCancelForSaleOrderRepository = new Repository<MaterialPlanCancelForSaleOrder>(db);
            MaterialPlanCancelForSaleOrderRepository = new RepositoryQuery<MaterialPlanCancelForSaleOrder>(_MaterialPlanCancelForSaleOrderRepository);
        }


        public MaterialPlanCancelForSaleOrder Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>().Find(id);
        }

        public MaterialPlanCancelForSaleOrder Create(MaterialPlanCancelForSaleOrder pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>().Delete(id);
        }

        public void Delete(MaterialPlanCancelForSaleOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>().Delete(pt);
        }
        public IEnumerable<MaterialPlanCancelForSaleOrder> GetPlanSaleOrderForMaterialPlanCancelLine(int MaterialPlanCancelLineId)
        {
            return (from p in db.MaterialPlanCancelForSaleOrder
                    where p.MaterialPlanCancelLineId == MaterialPlanCancelLineId
                    select p
                        ).ToList();
        }
        public void Update(MaterialPlanCancelForSaleOrder pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>().Update(pt);
        }

        public IEnumerable<MaterialPlanCancelForSaleOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanCancelForSaleOrderId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanCancelForSaleOrder> GetMaterialPlanCancelForSaleOrderList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>().Query().Get().OrderBy(m => m.MaterialPlanCancelForSaleOrderId);

            return pt;
        }

        public MaterialPlanCancelForSaleOrder Add(MaterialPlanCancelForSaleOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelForSaleOrder>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanCancelForSaleOrder
                        orderby p.MaterialPlanCancelForSaleOrderId
                        select p.MaterialPlanCancelForSaleOrderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelForSaleOrder
                        orderby p.MaterialPlanCancelForSaleOrderId
                        select p.MaterialPlanCancelForSaleOrderId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanCancelForSaleOrder
                        orderby p.MaterialPlanCancelForSaleOrderId
                        select p.MaterialPlanCancelForSaleOrderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelForSaleOrder
                        orderby p.MaterialPlanCancelForSaleOrderId
                        select p.MaterialPlanCancelForSaleOrderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanCancelForSaleOrder> GetMaterialPlanCancelForSaleOrderForMaterialPlanCancelline(int MaterialPlanCancelLineId)
        {
            return (from p in db.MaterialPlanCancelForSaleOrder
                    where p.MaterialPlanCancelLineId == MaterialPlanCancelLineId
                    select p
                       );
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanCancelForSaleOrder
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


        public Task<IEquatable<MaterialPlanCancelForSaleOrder>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanCancelForSaleOrder> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
