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
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Service
{
    public interface IMaterialPlanCancelForProdOrderLineService : IDisposable
    {
        MaterialPlanCancelForProdOrderLine Create(MaterialPlanCancelForProdOrderLine pt);
        void Delete(int id);
        void Delete(MaterialPlanCancelForProdOrderLine pt);
        MaterialPlanCancelForProdOrderLine Find(int id);
        IEnumerable<MaterialPlanCancelForProdOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanCancelForProdOrderLine pt);
        MaterialPlanCancelForProdOrderLine Add(MaterialPlanCancelForProdOrderLine pt);
        IEnumerable<MaterialPlanCancelForProdOrderLine> GetMaterialPlanCancelForLineList();
        Task<IEquatable<MaterialPlanCancelForProdOrderLine>> GetAsync();
        Task<MaterialPlanCancelForProdOrderLine> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        int GetMaxSr(int id);
    }

    public class MaterialPlanCancelForProdOrderLineService : IMaterialPlanCancelForProdOrderLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanCancelForProdOrderLine> _MaterialPlanCancelForLineRepository;
        RepositoryQuery<MaterialPlanCancelForProdOrderLine> MaterialPlanCancelForLineRepository;
        public MaterialPlanCancelForProdOrderLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanCancelForLineRepository = new Repository<MaterialPlanCancelForProdOrderLine>(db);
            MaterialPlanCancelForLineRepository = new RepositoryQuery<MaterialPlanCancelForProdOrderLine>(_MaterialPlanCancelForLineRepository);
        }


        public MaterialPlanCancelForProdOrderLine Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>().Find(id);
        }

        public MaterialPlanCancelForProdOrderLine Create(MaterialPlanCancelForProdOrderLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>().Delete(id);
        }

        public void Delete(MaterialPlanCancelForProdOrderLine pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>().Delete(pt);
        }

        public void Update(MaterialPlanCancelForProdOrderLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>().Update(pt);
        }

        public IEnumerable<MaterialPlanCancelForProdOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanCancelForProdOrderLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanCancelForProdOrderLine> GetMaterialPlanCancelForLineList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>().Query().Get().OrderBy(m => m.MaterialPlanCancelForProdOrderLineId);

            return pt;
        }

        public MaterialPlanCancelForProdOrderLine Add(MaterialPlanCancelForProdOrderLine pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelForProdOrderLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanCancelForProdOrderLine
                        orderby p.MaterialPlanCancelForProdOrderLineId
                        select p.MaterialPlanCancelForProdOrderLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelForProdOrderLine
                        orderby p.MaterialPlanCancelForProdOrderLineId
                        select p.MaterialPlanCancelForProdOrderLineId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanCancelForProdOrderLine
                        orderby p.MaterialPlanCancelForProdOrderLineId
                        select p.MaterialPlanCancelForProdOrderLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelForProdOrderLine
                        orderby p.MaterialPlanCancelForProdOrderLineId
                        select p.MaterialPlanCancelForProdOrderLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanCancelForProdOrderLine> GetMaterialPlanCancelForProdORderForMaterialPlanCancel(int MaterialPlanCancelLineId)
        {
            return (from p in db.MaterialPlanCancelForProdOrderLine
                    where p.MaterialPlanCancelLineId == MaterialPlanCancelLineId
                    select p);
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanCancelLine
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


        public Task<IEquatable<MaterialPlanCancelForProdOrderLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanCancelForProdOrderLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
