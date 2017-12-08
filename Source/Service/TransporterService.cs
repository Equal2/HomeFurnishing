using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;

namespace Service
{
    public interface ITransporterService : IDisposable
    {
        Transporter Create(Transporter Transporter);
        void Delete(int id);
        void Delete(Transporter Transporter);
        Transporter GetTransporter(int TransporterId);
        IEnumerable<Transporter> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Transporter Transporter);
        Transporter Add(Transporter Transporter);
        IEnumerable<Transporter> GetTransporterList();
        Task<IEquatable<Transporter>> GetAsync();
        Task<Transporter> FindAsync(int id);
        Transporter GetTransporterByName(string Name);
        Transporter Find(int id);
        TransporterViewModel GetTransporterViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<TransporterIndexViewModel> GetTransporterListForIndex();
    }

    public class TransporterService : ITransporterService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public TransporterService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Transporter GetTransporterByName(string Transporter)
        {
            return (from b in db.Transporter
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == Transporter
                    select b
                        ).FirstOrDefault();
        }
        public Transporter GetTransporter(int TransporterId)
        {
            return _unitOfWork.Repository<Transporter>().Find(TransporterId);
        }

        public Transporter Find(int id)
        {
            return _unitOfWork.Repository<Transporter>().Find(id);
        }

        public Transporter Create(Transporter Transporter)
        {
            Transporter.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Transporter>().Insert(Transporter);
            return Transporter;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Transporter>().Delete(id);
        }

        public void Delete(Transporter Transporter)
        {
            _unitOfWork.Repository<Transporter>().Delete(Transporter);
        }

        public void Update(Transporter Transporter)
        {
            Transporter.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Transporter>().Update(Transporter);
        }

        public IEnumerable<Transporter> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var Transporter = _unitOfWork.Repository<Transporter>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return Transporter;
        }

        public IEnumerable<Transporter> GetTransporterList()
        {
            var Transporter = _unitOfWork.Repository<Transporter>().Query().Include(m => m.Person).Get().Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return Transporter;
        }

        public Transporter Add(Transporter Transporter)
        {
            _unitOfWork.Repository<Transporter>().Insert(Transporter);
            return Transporter;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Transporter>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Transporter> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in db.Transporter
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.Transporter
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).FirstOrDefault();
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

                temp = (from b in db.Transporter
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.Transporter
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public TransporterViewModel GetTransporterViewModel(int id)
        {
            TransporterViewModel Transporterviewmodel = (from b in db.Transporter
                                                     join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                                     from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                                     join p in db.Persons on BusinessEntityTab.PersonID equals p.PersonID into PersonTable
                                                     from PersonTab in PersonTable.DefaultIfEmpty()
                                   join pa in db.PersonAddress on b.PersonID equals pa.PersonId into PersonAddressTable
                                   from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                   join ac in db.LedgerAccount on b.PersonID equals ac.PersonId into AccountTable
                                   from AccountTab in AccountTable.DefaultIfEmpty()
                                   where b.PersonID == id
                                   select new TransporterViewModel
                                   {
                                       PersonId = b.PersonID,
                                       Name = PersonTab.Name,
                                       Suffix = PersonTab.Suffix,
                                       Code = PersonTab.Code,
                                       Phone = PersonTab.Phone,
                                       Mobile = PersonTab.Mobile,
                                       Email = PersonTab.Email,
                                       Address = PersonAddressTab.Address,
                                       CityId = PersonAddressTab.CityId,
                                       Zipcode = PersonAddressTab.Zipcode,
                                       TdsCategoryId = BusinessEntityTab.TdsCategoryId,
                                       TdsGroupId = BusinessEntityTab.TdsGroupId,
                                       IsActive = PersonTab.IsActive,
                                       LedgerAccountGroupId = AccountTab.LedgerAccountGroupId,
                                       CreatedBy = PersonTab.CreatedBy,
                                       CreatedDate = PersonTab.CreatedDate,
                                       PersonAddressID = PersonAddressTab.PersonAddressID,
                                       AccountId = AccountTab.LedgerAccountId,
                                       ImageFileName = PersonTab.ImageFileName,
                                       ImageFolderName = PersonTab.ImageFolderName
                                   }
                   ).FirstOrDefault();

            var PersonRegistration = (from pp in db.PersonRegistration
                                      where pp.PersonId == id
                                      select new
                                      {
                                          PersonRegistrationId = pp.PersonRegistrationID,
                                          RregistrationType = pp.RegistrationType,
                                          RregistrationNo = pp.RegistrationNo
                                      }).ToList();

            if (PersonRegistration != null)
            {
                foreach (var item in PersonRegistration)
                {
                    if (item.RregistrationType == PersonRegistrationType.PANNo)
                    {
                        Transporterviewmodel.PersonRegistrationPanNoID = item.PersonRegistrationId;
                        Transporterviewmodel.PanNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.ServiceTaxNo)
                    {
                        Transporterviewmodel.PersonRegistrationServiceTaxNoID = item.PersonRegistrationId;
                        Transporterviewmodel.ServiceTaxNo = item.RregistrationNo;
                    }
                }
            }

            return Transporterviewmodel;

          }

        public IQueryable<TransporterIndexViewModel> GetTransporterListForIndex()
        {
            var temp = from p in db.Transporter
                       join p1 in db.Persons on p.PersonID equals p1.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new TransporterIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name
                       };
            return temp;
        }

    }

}
