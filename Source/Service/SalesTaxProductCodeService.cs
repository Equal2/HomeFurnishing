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
    public interface ISalesTaxProductCodeService : IDisposable
    {
        SalesTaxProductCode Create(SalesTaxProductCode pt);
        void Delete(int id);
        void Delete(SalesTaxProductCode pt);
        SalesTaxProductCode Find(string Name);
        SalesTaxProductCode Find(int id);
        IEnumerable<SalesTaxProductCode> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesTaxProductCode pt);
        SalesTaxProductCode Add(SalesTaxProductCode pt);
        IEnumerable<SalesTaxProductCode> GetSalesTaxProductCodeList();

        // IEnumerable<SalesTaxProductCode> GetSalesTaxProductCodeList(int buyerId);
        Task<IEquatable<SalesTaxProductCode>> GetAsync();
        Task<SalesTaxProductCode> FindAsync(int id);
        SalesTaxProductCode GetSalesTaxProductCodeByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class SalesTaxProductCodeService : ISalesTaxProductCodeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SalesTaxProductCode> _SalesTaxProductCodeRepository;
        RepositoryQuery<SalesTaxProductCode> SalesTaxProductCodeRepository;
        public SalesTaxProductCodeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SalesTaxProductCodeRepository = new Repository<SalesTaxProductCode>(db);
            SalesTaxProductCodeRepository = new RepositoryQuery<SalesTaxProductCode>(_SalesTaxProductCodeRepository);
        }
        public SalesTaxProductCode GetSalesTaxProductCodeByName(string terms)
        {
            return (from p in db.SalesTaxProductCode
                    where p.Code == terms
                    select p).FirstOrDefault();
        }

        public SalesTaxProductCode Find(string Name)
        {
            return SalesTaxProductCodeRepository.Get().Where(i => i.Code == Name).FirstOrDefault();
        }


        public SalesTaxProductCode Find(int id)
        {
            return _unitOfWork.Repository<SalesTaxProductCode>().Find(id);
        }

        public SalesTaxProductCode Create(SalesTaxProductCode pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesTaxProductCode>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesTaxProductCode>().Delete(id);
        }

        public void Delete(SalesTaxProductCode pt)
        {
            _unitOfWork.Repository<SalesTaxProductCode>().Delete(pt);
        }

        public void Update(SalesTaxProductCode pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesTaxProductCode>().Update(pt);
        }

        public IEnumerable<SalesTaxProductCode> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SalesTaxProductCode>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Code))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesTaxProductCode> GetSalesTaxProductCodeList()
        {
            var pt = _unitOfWork.Repository<SalesTaxProductCode>().Query().Get().OrderBy(m=>m.Code);

            return pt;
        }

        public SalesTaxProductCode Add(SalesTaxProductCode pt)
        {
            _unitOfWork.Repository<SalesTaxProductCode>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SalesTaxProductCode
                        orderby p.Code
                        select p.SalesTaxProductCodeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxProductCode
                        orderby p.Code
                        select p.SalesTaxProductCodeId).FirstOrDefault();
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

                temp = (from p in db.SalesTaxProductCode
                        orderby p.Code
                        select p.SalesTaxProductCodeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxProductCode
                        orderby p.Code
                        select p.SalesTaxProductCodeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SalesTaxProductCode>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SalesTaxProductCode> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
