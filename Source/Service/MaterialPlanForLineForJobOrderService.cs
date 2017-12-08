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
    public interface IMaterialPlanForJobOrderService : IDisposable
    {
        MaterialPlanForJobOrder Create(MaterialPlanForJobOrder pt);
        void Delete(int id);
        void Delete(MaterialPlanForJobOrder pt);
        MaterialPlanForJobOrder Find(int id);
        IEnumerable<MaterialPlanForJobOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanForJobOrder pt);
        MaterialPlanForJobOrder Add(MaterialPlanForJobOrder pt);
        IEnumerable<MaterialPlanForJobOrder> GetMaterialPlanForJobOrderList();        
        Task<IEquatable<MaterialPlanForJobOrder>> GetAsync();
        Task<MaterialPlanForJobOrder> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class MaterialPlanForJobOrderService : IMaterialPlanForJobOrderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanForJobOrder> _MaterialPlanForJobOrderRepository;
        RepositoryQuery<MaterialPlanForJobOrder> MaterialPlanForJobOrderRepository;
        public MaterialPlanForJobOrderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanForJobOrderRepository = new Repository<MaterialPlanForJobOrder>(db);
            MaterialPlanForJobOrderRepository = new RepositoryQuery<MaterialPlanForJobOrder>(_MaterialPlanForJobOrderRepository);
        }


        public MaterialPlanForJobOrder Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanForJobOrder>().Find(id);
        }

        public MaterialPlanForJobOrder Create(MaterialPlanForJobOrder pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanForJobOrder>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanForJobOrder>().Delete(id);
        }

        public void Delete(MaterialPlanForJobOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanForJobOrder>().Delete(pt);
        }

        public void Update(MaterialPlanForJobOrder pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanForJobOrder>().Update(pt);
        }

        public IEnumerable<MaterialPlanForJobOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanForJobOrder>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanForJobOrderId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanForJobOrder> GetMaterialPlanForJobOrderList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanForJobOrder>().Query().Get().OrderBy(m => m.MaterialPlanForJobOrderId);

            return pt;
        }

        public MaterialPlanForJobOrder Add(MaterialPlanForJobOrder pt)
        {
            _unitOfWork.Repository<MaterialPlanForJobOrder>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanForJobOrder
                        orderby p.MaterialPlanForJobOrderId
                        select p.MaterialPlanForJobOrderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForJobOrder
                        orderby p.MaterialPlanForJobOrderId
                        select p.MaterialPlanForJobOrderId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanForJobOrder
                        orderby p.MaterialPlanForJobOrderId
                        select p.MaterialPlanForJobOrderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForJobOrder
                        orderby p.MaterialPlanForJobOrderId
                        select p.MaterialPlanForJobOrderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialPlanForJobOrder>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanForJobOrder> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
