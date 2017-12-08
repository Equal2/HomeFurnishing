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
    public interface ISaleEnquiryLineExtendedService : IDisposable
    {
        SaleEnquiryLineExtended Create(SaleEnquiryLineExtended pt);
        void Delete(int id);
        void Delete(SaleEnquiryLineExtended pt);
        SaleEnquiryLineExtended Find(int id);
        IEnumerable<SaleEnquiryLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleEnquiryLineExtended pt);
        SaleEnquiryLineExtended Add(SaleEnquiryLineExtended pt);
        IEnumerable<SaleEnquiryLineExtended> GetSaleEnquiryLineExtendedList();

        // IEnumerable<SaleEnquiryLineExtended> GetSaleEnquiryLineExtendedList(int buyerId);
        Task<IEquatable<SaleEnquiryLineExtended>> GetAsync();
        Task<SaleEnquiryLineExtended> FindAsync(int id);
        void CreateLineStatus(int id);

    }

    public class SaleEnquiryLineExtendedService : ISaleEnquiryLineExtendedService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleEnquiryLineExtended> _SaleEnquiryLineExtendedRepository;
        RepositoryQuery<SaleEnquiryLineExtended> SaleEnquiryLineExtendedRepository;
        public SaleEnquiryLineExtendedService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleEnquiryLineExtendedRepository = new Repository<SaleEnquiryLineExtended>(db);
            SaleEnquiryLineExtendedRepository = new RepositoryQuery<SaleEnquiryLineExtended>(_SaleEnquiryLineExtendedRepository);
        }


        public SaleEnquiryLineExtended Find(int id)
        {
            return _unitOfWork.Repository<SaleEnquiryLineExtended>().Find(id);
        }

        public SaleEnquiryLineExtended Create(SaleEnquiryLineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleEnquiryLineExtended>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleEnquiryLineExtended>().Delete(id);
        }

        public void Delete(SaleEnquiryLineExtended pt)
        {
            _unitOfWork.Repository<SaleEnquiryLineExtended>().Delete(pt);
        }

        public void Update(SaleEnquiryLineExtended pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleEnquiryLineExtended>().Update(pt);
        }

        public IEnumerable<SaleEnquiryLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleEnquiryLineExtended>()
                .Query()                             
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleEnquiryLineExtended> GetSaleEnquiryLineExtendedList()
        {
            var pt = _unitOfWork.Repository<SaleEnquiryLineExtended>().Query().Get();

            return pt;
        }    

        public SaleEnquiryLineExtended Add(SaleEnquiryLineExtended pt)
        {
            _unitOfWork.Repository<SaleEnquiryLineExtended>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            SaleEnquiryLineExtended Stat = new SaleEnquiryLineExtended();
            Stat.SaleEnquiryLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            SaleEnquiryLineExtended Stat = Find(id);
            Delete(Stat);
        }



            
        public void Dispose()
        {
        }


        public Task<IEquatable<SaleEnquiryLineExtended>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleEnquiryLineExtended> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
