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
    public interface ISupplierService : IDisposable
    {
        Supplier Create(Supplier Supplier);
        void Delete(int id);
        void Delete(Supplier Supplier);
        Supplier GetSupplier(int SupplierId);
        IEnumerable<Supplier> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Supplier Supplier);
        Supplier Add(Supplier Supplier);
        IEnumerable<Supplier> GetSupplierList();
        Task<IEquatable<Supplier>> GetAsync();
        Task<Supplier> FindAsync(int id);
        SupplierViewModel GetSupplierByName(string Name);
        Supplier Find(int id);
        SupplierViewModel GetSupplierViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<SupplierIndexViewModel> GetSupplierListForIndex();
    }

    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public SupplierService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public SupplierViewModel GetSupplierByName(string Supplier)
        {
            return (from b in db.Supplier
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == Supplier
                    select new SupplierViewModel
                    {
                        PersonId = b.PersonID,
                        SalesTaxGroupPartyId = b.SalesTaxGroupPartyId
                    }).FirstOrDefault();
        }
        public Supplier GetSupplier(int SupplierId)
        {
            return _unitOfWork.Repository<Supplier>().Find(SupplierId);
        }

        public Supplier Find(int id)
        {
            return _unitOfWork.Repository<Supplier>().Find(id);
        }

        public Supplier Create(Supplier Supplier)
        {
            Supplier.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Supplier>().Insert(Supplier);
            return Supplier;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Supplier>().Delete(id);
        }

        public void Delete(Supplier Supplier)
        {
            _unitOfWork.Repository<Supplier>().Delete(Supplier);
        }

        public void Update(Supplier Supplier)
        {
            Supplier.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Supplier>().Update(Supplier);
        }

        public IEnumerable<Supplier> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var Supplier = _unitOfWork.Repository<Supplier>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return Supplier;
        }

        public IEnumerable<Supplier> GetSupplierList()
        {
            var Supplier = _unitOfWork.Repository<Supplier>().Query().Include(m => m.Person)
                .Get()
                .Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return Supplier;
        }

        public Supplier Add(Supplier Supplier)
        {
            _unitOfWork.Repository<Supplier>().Insert(Supplier);
            return Supplier;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Supplier>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Supplier> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in db.Supplier
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.Supplier
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

                temp = (from b in db.Supplier
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.Supplier
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

        public SupplierViewModel GetSupplierViewModel(int id)
        {

            SupplierViewModel Supplierviewmodel = (from b in db.Supplier
                                             join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                             from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                             join p in db.Persons on BusinessEntityTab.PersonID equals p.PersonID into PersonTable
                                             from PersonTab in PersonTable.DefaultIfEmpty()
                                             join pa in db.PersonAddress on PersonTab.PersonID equals pa.PersonId into PersonAddressTable
                                             from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                             join ac in db.LedgerAccount on PersonTab.PersonID equals ac.PersonId into AccountTable
                                             from AccountTab in AccountTable.DefaultIfEmpty()
                                             where PersonTab.PersonID == id
                                             select new SupplierViewModel
                                             {
                                                 PersonId = PersonTab.PersonID,
                                                 Name = PersonTab.Name,
                                                 Suffix = PersonTab.Suffix,
                                                 Code = PersonTab.Code,
                                                 Phone = PersonTab.Phone,
                                                 Mobile = PersonTab.Mobile,
                                                 Email = PersonTab.Email,
                                                 Address = PersonAddressTab.Address,
                                                 CityId = PersonAddressTab.CityId,
                                                 Zipcode = PersonAddressTab.Zipcode,
                                                 IsSisterConcern = BusinessEntityTab.IsSisterConcern,
                                                 PersonRateGroupId = BusinessEntityTab.PersonRateGroupId,
                                                 CreaditDays = BusinessEntityTab.CreaditDays,
                                                 CreaditLimit = BusinessEntityTab.CreaditLimit,
                                                 IsActive = PersonTab.IsActive,
                                                 SalesTaxGroupPartyId = b.SalesTaxGroupPartyId,
                                                 LedgerAccountGroupId = AccountTab.LedgerAccountGroupId,
                                                 CreatedBy = PersonTab.CreatedBy,
                                                 CreatedDate = PersonTab.CreatedDate,
                                                 PersonAddressID = PersonAddressTab.PersonAddressID,
                                                 AccountId = AccountTab.LedgerAccountId,
                                                 SiteIds = BusinessEntityTab.SiteIds,
                                                 DivisionIds  = BusinessEntityTab.DivisionIds,
                                                 Tags = PersonTab.Tags,
                                                 ImageFileName = PersonTab.ImageFileName,
                                                 ImageFolderName = PersonTab.ImageFolderName
                                             }).FirstOrDefault();

            var PersonProcess = (from pp in db.PersonProcess
                                 where pp.PersonId == id
                                 select new
                                 {
                                     ProcessId = pp.ProcessId
                                 }).ToList();

            foreach (var item in PersonProcess)
            {
                if (Supplierviewmodel.ProcessIds == "" || Supplierviewmodel.ProcessIds == null)
                {
                    Supplierviewmodel.ProcessIds = item.ProcessId.ToString();
                }
                else
                {
                    Supplierviewmodel.ProcessIds = Supplierviewmodel.ProcessIds + "," + item.ProcessId.ToString();
                }
            }


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
                    if (item.RregistrationType == PersonRegistrationType.CstNo)
                    {
                        Supplierviewmodel.PersonRegistrationCstNoID = item.PersonRegistrationId;
                        Supplierviewmodel.CstNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.TinNo)
                    {
                        Supplierviewmodel.PersonRegistrationTinNoID = item.PersonRegistrationId;
                        Supplierviewmodel.TinNo = item.RregistrationNo;
                    }
                }
            }

            string Divisions = Supplierviewmodel.DivisionIds;
            if (Divisions != null)
            {
                Divisions = Divisions.Replace('|', ' ');
                Supplierviewmodel.DivisionIds = Divisions;
            }

            string Sites = Supplierviewmodel.SiteIds;
            if (Sites != null)
            {
                Sites = Sites.Replace('|', ' ');
                Supplierviewmodel.SiteIds = Sites;
            }

            return Supplierviewmodel;
        }


        public IQueryable<SupplierIndexViewModel> GetSupplierListForIndex()
        {
            var temp = from b in db.Supplier
                       join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new SupplierIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name
                       };
            return temp;
        }

    }

}
