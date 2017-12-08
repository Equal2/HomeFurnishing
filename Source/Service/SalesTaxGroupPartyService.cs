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
    public interface ISalesTaxGroupPartyService : IDisposable
    {
        SalesTaxGroupParty Create(SalesTaxGroupParty pt);
        void Delete(int id);
        void Delete(SalesTaxGroupParty pt);
        SalesTaxGroupParty Find(string Name);
        SalesTaxGroupParty Find(int id);
        IEnumerable<SalesTaxGroupParty> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesTaxGroupParty pt);
        SalesTaxGroupParty Add(SalesTaxGroupParty pt);
        IEnumerable<SalesTaxGroupParty> GetSalesTaxGroupPartyList();

        // IEnumerable<SalesTaxGroupParty> GetSalesTaxGroupPartyList(int buyerId);
        Task<IEquatable<SalesTaxGroupParty>> GetAsync();
        Task<SalesTaxGroupParty> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class SalesTaxGroupPartyService : ISalesTaxGroupPartyService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SalesTaxGroupParty> _SalesTaxGroupPartyRepository;
        RepositoryQuery<SalesTaxGroupParty> SalesTaxGroupPartyRepository;
        public SalesTaxGroupPartyService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SalesTaxGroupPartyRepository = new Repository<SalesTaxGroupParty>(db);
            SalesTaxGroupPartyRepository = new RepositoryQuery<SalesTaxGroupParty>(_SalesTaxGroupPartyRepository);
        }


        public SalesTaxGroupParty Find(string Name)
        {            
            return SalesTaxGroupPartyRepository.Get().Where(i => i.SalesTaxGroupPartyName == Name).FirstOrDefault();
        }


        public SalesTaxGroupParty Find(int id)
        {
            return _unitOfWork.Repository<SalesTaxGroupParty>().Find(id);            
        }

        public SalesTaxGroupParty Create(SalesTaxGroupParty pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesTaxGroupParty>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesTaxGroupParty>().Delete(id);
        }

        public void Delete(SalesTaxGroupParty pt)
        {
            _unitOfWork.Repository<SalesTaxGroupParty>().Delete(pt);
        }

        public void Update(SalesTaxGroupParty pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesTaxGroupParty>().Update(pt);
        }

        public IEnumerable<SalesTaxGroupParty> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SalesTaxGroupParty>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SalesTaxGroupPartyName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesTaxGroupParty> GetSalesTaxGroupPartyList()
        {
            var pt = _unitOfWork.Repository<SalesTaxGroupParty>().Query().Get().OrderBy(m=>m.SalesTaxGroupPartyName);

            return pt;
        }

        public SalesTaxGroupParty Add(SalesTaxGroupParty pt)
        {
            _unitOfWork.Repository<SalesTaxGroupParty>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SalesTaxGroupParty
                        orderby p.SalesTaxGroupPartyName
                        select p.SalesTaxGroupPartyId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxGroupParty
                        orderby p.SalesTaxGroupPartyName
                        select p.SalesTaxGroupPartyId).FirstOrDefault();
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

                temp = (from p in db.SalesTaxGroupParty
                        orderby p.SalesTaxGroupPartyName
                        select p.SalesTaxGroupPartyId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxGroupParty
                        orderby p.SalesTaxGroupPartyName
                        select p.SalesTaxGroupPartyId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SalesTaxGroupParty>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SalesTaxGroupParty> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
