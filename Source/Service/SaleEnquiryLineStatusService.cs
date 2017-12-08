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
    public interface ISaleEnquiryLineStatusService : IDisposable
    {
        SaleEnquiryLineStatus Create(SaleEnquiryLineStatus pt);
        void Delete(int id);
        void Delete(SaleEnquiryLineStatus pt);
        SaleEnquiryLineStatus Find(int id);
        IEnumerable<SaleEnquiryLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleEnquiryLineStatus pt);
        SaleEnquiryLineStatus Add(SaleEnquiryLineStatus pt);
        IEnumerable<SaleEnquiryLineStatus> GetSaleEnquiryLineStatusList();

        // IEnumerable<SaleEnquiryLineStatus> GetSaleEnquiryLineStatusList(int buyerId);
        Task<IEquatable<SaleEnquiryLineStatus>> GetAsync();
        Task<SaleEnquiryLineStatus> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class SaleEnquiryLineStatusService : ISaleEnquiryLineStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleEnquiryLineStatus> _SaleEnquiryLineStatusRepository;
        RepositoryQuery<SaleEnquiryLineStatus> SaleEnquiryLineStatusRepository;
        public SaleEnquiryLineStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleEnquiryLineStatusRepository = new Repository<SaleEnquiryLineStatus>(db);
            SaleEnquiryLineStatusRepository = new RepositoryQuery<SaleEnquiryLineStatus>(_SaleEnquiryLineStatusRepository);
        }


        public SaleEnquiryLineStatus Find(int id)
        {
            return _unitOfWork.Repository<SaleEnquiryLineStatus>().Find(id);
        }

        public SaleEnquiryLineStatus Create(SaleEnquiryLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleEnquiryLineStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleEnquiryLineStatus>().Delete(id);
        }

        public void Delete(SaleEnquiryLineStatus pt)
        {
            _unitOfWork.Repository<SaleEnquiryLineStatus>().Delete(pt);
        }

        public void Update(SaleEnquiryLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleEnquiryLineStatus>().Update(pt);
        }

        public IEnumerable<SaleEnquiryLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleEnquiryLineStatus>()
                .Query()                             
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleEnquiryLineStatus> GetSaleEnquiryLineStatusList()
        {
            var pt = _unitOfWork.Repository<SaleEnquiryLineStatus>().Query().Get();

            return pt;
        }    

        public SaleEnquiryLineStatus Add(SaleEnquiryLineStatus pt)
        {
            _unitOfWork.Repository<SaleEnquiryLineStatus>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            SaleEnquiryLineStatus Stat = new SaleEnquiryLineStatus();
            Stat.SaleEnquiryLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            SaleEnquiryLineStatus Stat = Find(id);
            Delete(Stat);
        }


        //CancelQtyUpdate Functions



        //InvoiceQtyUpdate Functions







        //AmendmentQtyUpdate Functions





     
        public void Dispose()
        {
        }


        public Task<IEquatable<SaleEnquiryLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleEnquiryLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
