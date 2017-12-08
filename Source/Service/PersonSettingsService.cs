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
    public interface IPersonSettingsService : IDisposable
    {
        PersonSettings Create(PersonSettings pt);
        void Delete(int id);
        void Delete(PersonSettings s);
        PersonSettings Find(int Id);
        void Update(PersonSettings s);
        IEnumerable<PersonSettings> GetPersonSettingsList();
        PersonSettings GetPersonSettings(int DocTypeId);
    }

    public class PersonSettingsService : IPersonSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonSettings> _PersonSettingsRepository;
        RepositoryQuery<PersonSettings> PersonSettingsRepository;
        public PersonSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonSettingsRepository = new Repository<PersonSettings>(db);
            PersonSettingsRepository = new RepositoryQuery<PersonSettings>(_PersonSettingsRepository);
        }

        public PersonSettings Find(int id)
        {
            return _unitOfWork.Repository<PersonSettings>().Find(id);
        }

        public PersonSettings GetPersonSettings(int DocTypeId)
        {
            return _unitOfWork.Repository<PersonSettings>().Query().Get().Where(m=>m.DocTypeId==DocTypeId).FirstOrDefault();
        }


        public PersonSettings Create(PersonSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonSettings>().Delete(id);
        }

        public void Delete(PersonSettings s)
        {
            _unitOfWork.Repository<PersonSettings>().Delete(s);
        }

        public void Update(PersonSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonSettings>().Update(s);
        }


        public IEnumerable<PersonSettings> GetPersonSettingsList()
        {
            var pt = _unitOfWork.Repository<PersonSettings>().Query().Get();

            return pt;
        }

        //new added
        public PersonSettings GetPersonSettingsForDocument(int DocTypeId)
        {
            return (from p in db.PersonSettings
                    where p.DocTypeId == DocTypeId 
                    select p
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
        }

    }
}
