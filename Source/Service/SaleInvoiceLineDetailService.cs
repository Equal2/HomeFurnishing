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
    public interface ISaleInvoiceLineDetailService : IDisposable
    {
        SaleInvoiceLineDetail Create(SaleInvoiceLineDetail pt);
        void Delete(int id);
        void Delete(SaleInvoiceLineDetail pt);
        SaleInvoiceLineDetail Find(int id);
        IEnumerable<SaleInvoiceLineDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleInvoiceLineDetail pt);
        SaleInvoiceLineDetail Add(SaleInvoiceLineDetail pt);
        IEnumerable<SaleInvoiceLineDetail> GetSaleInvoiceLineDetailList();
        Task<IEquatable<SaleInvoiceLineDetail>> GetAsync();
        Task<SaleInvoiceLineDetail> FindAsync(int id);
        void Create(int id, ref ApplicationDbContext Context, bool DBbased);
    }

    public class SaleInvoiceLineDetailService : ISaleInvoiceLineDetailService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleInvoiceLineDetail> _SaleInvoiceLineDetailRepository;
        RepositoryQuery<SaleInvoiceLineDetail> SaleInvoiceLineDetailRepository;
        public SaleInvoiceLineDetailService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleInvoiceLineDetailRepository = new Repository<SaleInvoiceLineDetail>(db);
            SaleInvoiceLineDetailRepository = new RepositoryQuery<SaleInvoiceLineDetail>(_SaleInvoiceLineDetailRepository);
        }


        public SaleInvoiceLineDetail Find(int id)
        {
            return _unitOfWork.Repository<SaleInvoiceLineDetail>().Find(id);
        }

        public SaleInvoiceLineDetail Create(SaleInvoiceLineDetail pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleInvoiceLineDetail>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleInvoiceLineDetail>().Delete(id);
        }

        public void Delete(SaleInvoiceLineDetail pt)
        {
            _unitOfWork.Repository<SaleInvoiceLineDetail>().Delete(pt);
        }

        public void Update(SaleInvoiceLineDetail pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleInvoiceLineDetail>().Update(pt);
        }

        public IEnumerable<SaleInvoiceLineDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleInvoiceLineDetail>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleInvoiceLineDetail> GetSaleInvoiceLineDetailList()
        {
            var pt = _unitOfWork.Repository<SaleInvoiceLineDetail>().Query().Get();

            return pt;
        }

        public SaleInvoiceLineDetail Add(SaleInvoiceLineDetail pt)
        {
            _unitOfWork.Repository<SaleInvoiceLineDetail>().Insert(pt);
            return pt;
        }

        public void Create(int id, ref ApplicationDbContext Context, bool DBbased)
        {
            SaleInvoiceLineDetail Stat = new SaleInvoiceLineDetail();
            Stat.SaleInvoiceLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            if (DBbased)
                Context.SaleInvoiceLineDetail.Add(Stat);
            else
                Add(Stat);
        }
        public void DeleteHeaderStatus(int id)
        {
            SaleInvoiceLineDetail Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleInvoiceLineDetail>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleInvoiceLineDetail> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
