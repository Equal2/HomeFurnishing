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
    public interface IMaterialPlanForSaleOrderService : IDisposable
    {
        MaterialPlanForSaleOrder Create(MaterialPlanForSaleOrder pt);
        void Delete(int id);
        void Delete(MaterialPlanForSaleOrder pt);
        MaterialPlanForSaleOrder Find(int id);
        IEnumerable<MaterialPlanForSaleOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanForSaleOrder pt);
        MaterialPlanForSaleOrder Add(MaterialPlanForSaleOrder pt);
        IEnumerable<MaterialPlanForSaleOrder> GetMaterialPlanForSaleOrderList();        
        Task<IEquatable<MaterialPlanForSaleOrder>> GetAsync();
        Task<MaterialPlanForSaleOrder> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<MaterialPlanForSaleOrder> GetPlanSaleOrderForMaterialPlanLine(int MaterialPlanLineId);
        
        IEnumerable<MaterialPlanForSaleOrder> GetMaterialPlanForSaleOrderForMaterialPlanline(int MaterialPlanLineId);
        int GetMaxSr(int id);
    }

    public class MaterialPlanForSaleOrderService : IMaterialPlanForSaleOrderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanForSaleOrder> _MaterialPlanForSaleOrderRepository;
        RepositoryQuery<MaterialPlanForSaleOrder> MaterialPlanForSaleOrderRepository;
        public MaterialPlanForSaleOrderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanForSaleOrderRepository = new Repository<MaterialPlanForSaleOrder>(db);
            MaterialPlanForSaleOrderRepository = new RepositoryQuery<MaterialPlanForSaleOrder>(_MaterialPlanForSaleOrderRepository);
        }


        public MaterialPlanForSaleOrder Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanForSaleOrder>().Find(id);
        }

        public MaterialPlanForSaleOrder Create(MaterialPlanForSaleOrder pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanForSaleOrder>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanForSaleOrder>().Delete(id);
        }

        public void Delete(MaterialPlanForSaleOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanForSaleOrder>().Delete(pt);
        }
        public IEnumerable<MaterialPlanForSaleOrder> GetPlanSaleOrderForMaterialPlanLine(int MaterialPlanLineId)
        {
            return (from p in db.MaterialPlanForSaleOrder
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    select p
                        ).ToList();
        }

        

        public void Update(MaterialPlanForSaleOrder pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanForSaleOrder>().Update(pt);
        }

        public IEnumerable<MaterialPlanForSaleOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanForSaleOrder>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanForSaleOrderId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanForSaleOrder> GetMaterialPlanForSaleOrderList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanForSaleOrder>().Query().Get().OrderBy(m => m.MaterialPlanForSaleOrderId);

            return pt;
        }

        public MaterialPlanForSaleOrder Add(MaterialPlanForSaleOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanForSaleOrder>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanForSaleOrder
                        orderby p.MaterialPlanForSaleOrderId
                        select p.MaterialPlanForSaleOrderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForSaleOrder
                        orderby p.MaterialPlanForSaleOrderId
                        select p.MaterialPlanForSaleOrderId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanForSaleOrder
                        orderby p.MaterialPlanForSaleOrderId
                        select p.MaterialPlanForSaleOrderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForSaleOrder
                        orderby p.MaterialPlanForSaleOrderId
                        select p.MaterialPlanForSaleOrderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanForSaleOrder> GetMaterialPlanForSaleOrderForMaterialPlanline(int MaterialPlanLineId)
        {
            return (from p in db.MaterialPlanForSaleOrder
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    select p
                       );
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanForSaleOrder
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


        public Task<IEquatable<MaterialPlanForSaleOrder>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanForSaleOrder> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
