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
    public interface IMaterialPlanForProdOrderService : IDisposable
    {
        MaterialPlanForProdOrder Create(MaterialPlanForProdOrder pt);
        void Delete(int id);
        void Delete(MaterialPlanForProdOrder pt);
        MaterialPlanForProdOrder Find(int id);
        IEnumerable<MaterialPlanForProdOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanForProdOrder pt);
        MaterialPlanForProdOrder Add(MaterialPlanForProdOrder pt);
        IEnumerable<MaterialPlanForProdOrder> GetMaterialPlanForProdOrderList();        
        Task<IEquatable<MaterialPlanForProdOrder>> GetAsync();
        Task<MaterialPlanForProdOrder> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<MaterialPlanForProdOrder> GetMAterialPlanForProdORderForMaterialPlan(int MaterialPlanHeaderId);
        IEnumerable<MaterialPlanForProdOrderLine> GetPlanProdOrderForMaterialPlanLine(int MaterialPlanLineId);
        int GetMaxSr(int id);
    }

    public class MaterialPlanForProdOrderService : IMaterialPlanForProdOrderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanForProdOrder> _MaterialPlanForProdOrderRepository;
        RepositoryQuery<MaterialPlanForProdOrder> MaterialPlanForProdOrderRepository;
        public MaterialPlanForProdOrderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanForProdOrderRepository = new Repository<MaterialPlanForProdOrder>(db);
            MaterialPlanForProdOrderRepository = new RepositoryQuery<MaterialPlanForProdOrder>(_MaterialPlanForProdOrderRepository);
        }


        public MaterialPlanForProdOrder Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanForProdOrder>().Find(id);
        }

        public MaterialPlanForProdOrder Create(MaterialPlanForProdOrder pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanForProdOrder>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanForProdOrder>().Delete(id);
        }

        public void Delete(MaterialPlanForProdOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanForProdOrder>().Delete(pt);
        }

        public void Update(MaterialPlanForProdOrder pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanForProdOrder>().Update(pt);
        }

        public IEnumerable<MaterialPlanForProdOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanForProdOrder>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanForProdOrderId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanForProdOrder> GetMaterialPlanForProdOrderList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanForProdOrder>().Query().Get().OrderBy(m => m.MaterialPlanForProdOrderId);

            return pt;
        }

        public MaterialPlanForProdOrder Add(MaterialPlanForProdOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanForProdOrder>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanForProdOrder
                        orderby p.MaterialPlanForProdOrderId
                        select p.MaterialPlanForProdOrderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForProdOrder
                        orderby p.MaterialPlanForProdOrderId
                        select p.MaterialPlanForProdOrderId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanForProdOrder
                        orderby p.MaterialPlanForProdOrderId
                        select p.MaterialPlanForProdOrderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForProdOrder
                        orderby p.MaterialPlanForProdOrderId
                        select p.MaterialPlanForProdOrderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanForProdOrder> GetMAterialPlanForProdORderForMaterialPlan(int MaterialPlanHeaderId)
        {
            return (from p in db.MaterialPlanForProdOrder
                    where p.MaterialPlanHeaderId == MaterialPlanHeaderId
                    select p);
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanForProdOrder
                       where p.MaterialPlanHeaderId == id
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

        public IEnumerable<MaterialPlanForProdOrderLine> GetPlanProdOrderForMaterialPlanLine(int MaterialPlanLineId)
        {
            return (from p in db.MaterialPlanForProdOrderLine
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    select p
                        ).ToList();
        }


        public Task<IEquatable<MaterialPlanForProdOrder>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanForProdOrder> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
