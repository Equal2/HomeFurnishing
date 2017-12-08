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
using Model.ViewModels;

namespace Service
{
    public interface IChargeGroupSettingsService : IDisposable
    {
        ChargeGroupSettings Create(ChargeGroupSettings pt);
        void Delete(int id);
        void Delete(ChargeGroupSettings pt);
        ChargeGroupSettings Find(int id);
        IEnumerable<ChargeGroupSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ChargeGroupSettings pt);
        ChargeGroupSettings Add(ChargeGroupSettings pt);
        IEnumerable<ChargeGroupSettingsViewModel> GetChargeGroupSettingsList();
        Task<IEquatable<ChargeGroupSettings>> GetAsync();
        Task<ChargeGroupSettings> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ChargeGroupSettingsService : IChargeGroupSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ChargeGroupSettings> _ChargeGroupSettingsRepository;
        RepositoryQuery<ChargeGroupSettings> ChargeGroupSettingsRepository;
        public ChargeGroupSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ChargeGroupSettingsRepository = new Repository<ChargeGroupSettings>(db);
            ChargeGroupSettingsRepository = new RepositoryQuery<ChargeGroupSettings>(_ChargeGroupSettingsRepository);
        }




        public ChargeGroupSettings Find(int id)
        {
            return _unitOfWork.Repository<ChargeGroupSettings>().Find(id);
        }

        public ChargeGroupSettings Create(ChargeGroupSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ChargeGroupSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ChargeGroupSettings>().Delete(id);
        }

        public void Delete(ChargeGroupSettings pt)
        {
            _unitOfWork.Repository<ChargeGroupSettings>().Delete(pt);
        }

        public void Update(ChargeGroupSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ChargeGroupSettings>().Update(pt);
        }

        public IEnumerable<ChargeGroupSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ChargeGroupSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ChargeGroupSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ChargeGroupSettingsViewModel> GetChargeGroupSettingsList()
        {
            var pt = (from H in db.ChargeGroupSettings
                      orderby H.ProcessId, H.ChargeGroupPersonId, H.ChargeGroupProductId, H.ChargeTypeId
                      select new ChargeGroupSettingsViewModel
                      {
                          ChargeGroupSettingsId = H.ChargeGroupSettingsId,
                          ProcessId = H.ProcessId,
                          ProcessName = H.Process.ProcessName,
                          ChargeGroupPersonId = H.ChargeGroupPersonId,
                          ChargeGroupPersonName = H.ChargeGroupPerson.ChargeGroupPersonName,
                          ChargeGroupProductId = H.ChargeGroupProductId,
                          ChargeGroupProductName = H.ChargeGroupProduct.ChargeGroupProductName,
                          ChargeTypeId = H.ChargeTypeId,
                          ChargeTypeName = H.ChargeType.ChargeTypeName,
                          ChargeLedgerAccountId = H.ChargeLedgerAccountId,
                          ChargeLedgerAccountName = H.ChargeLedgerAccount.LedgerAccountName,
                          ChargePer = H.ChargePer,
                          CreatedBy = H.CreatedBy,
                          ModifiedBy = H.ModifiedBy
                      }).ToList();

            return pt;
        }

        public ChargeGroupSettings Add(ChargeGroupSettings pt)
        {
            _unitOfWork.Repository<ChargeGroupSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ChargeGroupSettings
                        orderby p.ChargeGroupSettingsId
                        select p.ChargeGroupSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeGroupSettings
                        orderby p.ChargeGroupSettingsId
                        select p.ChargeGroupSettingsId).FirstOrDefault();
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

                temp = (from p in db.ChargeGroupSettings
                        orderby p.ChargeGroupSettingsId
                        select p.ChargeGroupSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeGroupSettings
                        orderby p.ChargeGroupSettingsId
                        select p.ChargeGroupSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ChargeGroupSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChargeGroupSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
