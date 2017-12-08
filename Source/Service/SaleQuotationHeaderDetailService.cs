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
    public interface ISaleQuotationHeaderDetailService : IDisposable
    {
        SaleQuotationHeaderDetail Create(SaleQuotationHeaderDetail pt);
        void Delete(int id);
        void Delete(SaleQuotationHeaderDetail pt);
        SaleQuotationHeaderDetail Find(int id);
        IEnumerable<SaleQuotationHeaderDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleQuotationHeaderDetail pt);
        SaleQuotationHeaderDetail Add(SaleQuotationHeaderDetail pt);
        IEnumerable<SaleQuotationHeaderDetail> GetSaleQuotationHeaderDetailList();
        Task<IEquatable<SaleQuotationHeaderDetail>> GetAsync();
        Task<SaleQuotationHeaderDetail> FindAsync(int id);
        void Create(int id, ref ApplicationDbContext Context, bool DBbased);
    }

    public class SaleQuotationHeaderDetailService : ISaleQuotationHeaderDetailService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleQuotationHeaderDetail> _SaleQuotationHeaderDetailRepository;
        RepositoryQuery<SaleQuotationHeaderDetail> SaleQuotationHeaderDetailRepository;
        public SaleQuotationHeaderDetailService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleQuotationHeaderDetailRepository = new Repository<SaleQuotationHeaderDetail>(db);
            SaleQuotationHeaderDetailRepository = new RepositoryQuery<SaleQuotationHeaderDetail>(_SaleQuotationHeaderDetailRepository);
        }


        public SaleQuotationHeaderDetail Find(int id)
        {
            return _unitOfWork.Repository<SaleQuotationHeaderDetail>().Find(id);
        }

        public SaleQuotationHeaderDetail Create(SaleQuotationHeaderDetail pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleQuotationHeaderDetail>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleQuotationHeaderDetail>().Delete(id);
        }

        public void Delete(SaleQuotationHeaderDetail pt)
        {
            _unitOfWork.Repository<SaleQuotationHeaderDetail>().Delete(pt);
        }

        public void Update(SaleQuotationHeaderDetail pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleQuotationHeaderDetail>().Update(pt);
        }

        public IEnumerable<SaleQuotationHeaderDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleQuotationHeaderDetail>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleQuotationHeaderDetail> GetSaleQuotationHeaderDetailList()
        {
            var pt = _unitOfWork.Repository<SaleQuotationHeaderDetail>().Query().Get();

            return pt;
        }

        public SaleQuotationHeaderDetail Add(SaleQuotationHeaderDetail pt)
        {
            _unitOfWork.Repository<SaleQuotationHeaderDetail>().Insert(pt);
            return pt;
        }

        public void Create(int id, ref ApplicationDbContext Context, bool DBbased)
        {
            SaleQuotationHeaderDetail Stat = new SaleQuotationHeaderDetail();
            Stat.SaleQuotationHeaderId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            if (DBbased)
                Context.SaleQuotationHeaderDetail.Add(Stat);
            else
                Add(Stat);
        }
        public void DeleteHeaderStatus(int id)
        {
            SaleQuotationHeaderDetail Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleQuotationHeaderDetail>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleQuotationHeaderDetail> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
