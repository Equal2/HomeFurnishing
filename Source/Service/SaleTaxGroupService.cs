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
    public interface ISalesTaxGroupService : IDisposable
    {
        SalesTaxGroup Create(SalesTaxGroup pt);
        void Delete(int id);
        void Delete(SalesTaxGroup pt);
        SalesTaxGroup Find(int id);
        IEnumerable<SalesTaxGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesTaxGroup pt);
        SalesTaxGroup Add(SalesTaxGroup pt);
        IEnumerable<SalesTaxGroup> GetSalesTaxGroupList();
        Task<IEquatable<SalesTaxGroup>> GetAsync();
        Task<SalesTaxGroup> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);

        int GetSalesTaxGroupId(int PersonId, int ProductId);
    }

    public class SalesTaxGroupService : ISalesTaxGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SalesTaxGroup> _SalesTaxGroupRepository;
        RepositoryQuery<SalesTaxGroup> SalesTaxGroupRepository;
        public SalesTaxGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SalesTaxGroupRepository = new Repository<SalesTaxGroup>(db);
            SalesTaxGroupRepository = new RepositoryQuery<SalesTaxGroup>(_SalesTaxGroupRepository);
        }

        public SalesTaxGroup Find(int id)
        {
            return _unitOfWork.Repository<SalesTaxGroup>().Find(id);            
        }

        public SalesTaxGroup Create(SalesTaxGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesTaxGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesTaxGroup>().Delete(id);
        }

        public void Delete(SalesTaxGroup pt)
        {
            _unitOfWork.Repository<SalesTaxGroup>().Delete(pt);
        }

        public void Update(SalesTaxGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesTaxGroup>().Update(pt);
        }

        public IEnumerable<SalesTaxGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SalesTaxGroup>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesTaxGroup> GetSalesTaxGroupList()
        {
            var pt = _unitOfWork.Repository<SalesTaxGroup>().Query().Get();

            return pt;
        }

        public SalesTaxGroup Add(SalesTaxGroup pt)
        {
            _unitOfWork.Repository<SalesTaxGroup>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SalesTaxGroup
                        select p.SalesTaxGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxGroup
                        select p.SalesTaxGroupId).FirstOrDefault();
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

                temp = (from p in db.SalesTaxGroup
                        select p.SalesTaxGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxGroup
                        select p.SalesTaxGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int GetSalesTaxGroupId(int PersonId, int ProductId)
        {
            var SalesTaxGroupProduct = (from P in db.Product where P.ProductId == ProductId select new { SalesTaxGroupProductId = P.SalesTaxGroupProductId }).FirstOrDefault();
            var SalesTaxGroupParty = (from P in db.BusinessEntity where P.PersonID == PersonId select new { SalesTaxGroupPartyId = P.SalesTaxGroupPartyId }).FirstOrDefault();


            var SalesTaxGroup = (from L in db.SalesTaxGroup
                    where L.SalesTaxGroupProductId == SalesTaxGroupProduct.SalesTaxGroupProductId && L.SalesTaxGroupPartyId == SalesTaxGroupParty.SalesTaxGroupPartyId
                    select new
                    {
                        SalesTaxGroupId = L.SalesTaxGroupId
                    }).FirstOrDefault();

            if (SalesTaxGroup != null)
            {
                return SalesTaxGroup.SalesTaxGroupId;
            }
            else
            {
                return 0;
            }
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SalesTaxGroup>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SalesTaxGroup> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
