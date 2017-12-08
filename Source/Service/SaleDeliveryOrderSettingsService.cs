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

namespace Service
{
    public interface ISaleDeliveryOrderSettingsService : IDisposable
    {
        SaleDeliveryOrderSettings Create(SaleDeliveryOrderSettings pt);
        void Delete(int id);
        void Delete(SaleDeliveryOrderSettings pt);
        SaleDeliveryOrderSettings Find(int id);
        IEnumerable<SaleDeliveryOrderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleDeliveryOrderSettings pt);
        SaleDeliveryOrderSettings Add(SaleDeliveryOrderSettings pt);
        SaleDeliveryOrderSettings GetSaleDeliveryOrderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        Task<IEquatable<SaleDeliveryOrderSettings>> GetAsync();
        Task<SaleDeliveryOrderSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class SaleDeliveryOrderSettingsService : ISaleDeliveryOrderSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleDeliveryOrderSettings> _SaleDeliveryOrderSettingsRepository;
        RepositoryQuery<SaleDeliveryOrderSettings> SaleDeliveryOrderSettingsRepository;
        public SaleDeliveryOrderSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleDeliveryOrderSettingsRepository = new Repository<SaleDeliveryOrderSettings>(db);
            SaleDeliveryOrderSettingsRepository = new RepositoryQuery<SaleDeliveryOrderSettings>(_SaleDeliveryOrderSettingsRepository);
        }

        public SaleDeliveryOrderSettings Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryOrderSettings>().Find(id);
        }

        public SaleDeliveryOrderSettings GetSaleDeliveryOrderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.SaleDeliveryOrderSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();


        }
        public SaleDeliveryOrderSettings Create(SaleDeliveryOrderSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliveryOrderSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDeliveryOrderSettings>().Delete(id);
        }

        public void Delete(SaleDeliveryOrderSettings pt)
        {
            _unitOfWork.Repository<SaleDeliveryOrderSettings>().Delete(pt);
        }

        public void Update(SaleDeliveryOrderSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliveryOrderSettings>().Update(pt);
        }

        public IEnumerable<SaleDeliveryOrderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleDeliveryOrderSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleDeliveryOrderSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public SaleDeliveryOrderSettings Add(SaleDeliveryOrderSettings pt)
        {
            _unitOfWork.Repository<SaleDeliveryOrderSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleDeliveryOrderSettings
                        orderby p.SaleDeliveryOrderSettingsId
                        select p.SaleDeliveryOrderSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliveryOrderSettings
                        orderby p.SaleDeliveryOrderSettingsId
                        select p.SaleDeliveryOrderSettingsId).FirstOrDefault();
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

                temp = (from p in db.SaleDeliveryOrderSettings
                        orderby p.SaleDeliveryOrderSettingsId
                        select p.SaleDeliveryOrderSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliveryOrderSettings
                        orderby p.SaleDeliveryOrderSettingsId
                        select p.SaleDeliveryOrderSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<SaleDeliveryOrderSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleDeliveryOrderSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
