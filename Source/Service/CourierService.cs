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
    public interface ICourierService : IDisposable
    {
        Courier Create(Courier Courier);
        void Delete(int id);
        void Delete(Courier Courier);
        Courier GetCourier(int CourierId);
        IEnumerable<Courier> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Courier Courier);
        Courier Add(Courier Courier);
        IEnumerable<Courier> GetCourierList();
        Task<IEquatable<Courier>> GetAsync();
        Task<Courier> FindAsync(int id);
        Courier GetCourierByName(string Name);
        Courier Find(int id);
        CourierViewModel GetCourierViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<CourierIndexViewModel> GetCourierListForIndex();
    }

    public class CourierService : ICourierService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public CourierService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Courier GetCourierByName(string Courier)
        {
            return (from b in db.Courier
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == Courier
                    select b
                        ).FirstOrDefault();
        }
        public Courier GetCourier(int CourierId)
        {
            return _unitOfWork.Repository<Courier>().Find(CourierId);
        }

        public Courier Find(int id)
        {
            return _unitOfWork.Repository<Courier>().Find(id);
        }

        public Courier Create(Courier Courier)
        {
            Courier.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Courier>().Insert(Courier);
            return Courier;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Courier>().Delete(id);
        }

        public void Delete(Courier Courier)
        {
            _unitOfWork.Repository<Courier>().Delete(Courier);
        }

        public void Update(Courier Courier)
        {
            Courier.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Courier>().Update(Courier);
        }

        public IEnumerable<Courier> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var Courier = _unitOfWork.Repository<Courier>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return Courier;
        }

        public IEnumerable<Courier> GetCourierList()
        {
            var Courier = _unitOfWork.Repository<Courier>().Query().Include(m => m.Person).Get().Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return Courier;
        }

        public Courier Add(Courier Courier)
        {
            _unitOfWork.Repository<Courier>().Insert(Courier);
            return Courier;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Courier>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Courier> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in db.Courier
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.Courier
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

                temp = (from b in db.Courier
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.Courier
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

        public CourierViewModel GetCourierViewModel(int id)
        {
            CourierViewModel Courierviewmodel = (from b in db.Courier
                                                     join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                                     from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                                     join p in db.Persons on BusinessEntityTab.PersonID equals p.PersonID into PersonTable
                                                     from PersonTab in PersonTable.DefaultIfEmpty()
                                   join pa in db.PersonAddress on b.PersonID equals pa.PersonId into PersonAddressTable
                                   from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                   join ac in db.LedgerAccount on b.PersonID equals ac.PersonId into AccountTable
                                   from AccountTab in AccountTable.DefaultIfEmpty()
                                   where b.PersonID == id
                                   select new CourierViewModel
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
                        Courierviewmodel.PersonRegistrationPanNoID = item.PersonRegistrationId;
                        Courierviewmodel.PanNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.ServiceTaxNo)
                    {
                        Courierviewmodel.PersonRegistrationServiceTaxNoID = item.PersonRegistrationId;
                        Courierviewmodel.ServiceTaxNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.KYCNo)
                    {
                        Courierviewmodel.PersonRegistrationKYCNoID = item.PersonRegistrationId;
                        Courierviewmodel.KYCNo = item.RregistrationNo;
                    }
                }
            }

            return Courierviewmodel;

          }

        public IQueryable<CourierIndexViewModel> GetCourierListForIndex()
        {
            var temp = from p in db.Courier
                       join p1 in db.Persons on p.PersonID equals p1.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new CourierIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name
                       };
            return temp;
        }

    }

}
