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
    public interface IBuyerService : IDisposable
    {
        Buyer Create(Buyer buyer);
        void Delete(int id);
        void Delete(Buyer buyer);
        Buyer GetBuyer(int buyerId);
        IEnumerable<Buyer> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Buyer buyer);
        Buyer Add(Buyer buyer);
        IEnumerable<Buyer> GetBuyerList();
        Task<IEquatable<Buyer>> GetAsync();
        Task<Buyer> FindAsync(int id);
        Buyer GetBuyerByName(string Name);
        Buyer Find(int id);
        BuyerViewModel GetBuyerViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<BuyerIndexViewModel> GetBuyerListForIndex();
    }

    public class BuyerService : IBuyerService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public BuyerService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Buyer GetBuyerByName(string buyer)
        {
            return (from b in db.Buyer
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == buyer
                    select b).FirstOrDefault();
        }
        public Buyer GetBuyer(int buyerId)
        {
            return _unitOfWork.Repository<Buyer>().Find(buyerId);
        }

        public Buyer Find(int id)
        {
            return _unitOfWork.Repository<Buyer>().Find(id);
        }

        public Buyer Create(Buyer buyer)
        {
            buyer.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Buyer>().Insert(buyer);
            return buyer;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Buyer>().Delete(id);
        }

        public void Delete(Buyer buyer)
        {
            _unitOfWork.Repository<Buyer>().Delete(buyer);
        }

        public void Update(Buyer buyer)
        {
            buyer.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Buyer>().Update(buyer);
        }

        public IEnumerable<Buyer> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var buyer = _unitOfWork.Repository<Buyer>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return buyer;
        }

        public IEnumerable<Buyer> GetBuyerList()
        {
            var buyer = _unitOfWork.Repository<Buyer>().Query().Include(m => m.Person)
                .Get()
                .Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return buyer;
        }

        public Buyer Add(Buyer buyer)
        {
            _unitOfWork.Repository<Buyer>().Insert(buyer);
            return buyer;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Buyer>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Buyer> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in db.Buyer
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.Buyer
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

                temp = (from b in db.Buyer
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.Buyer
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

        public BuyerViewModel GetBuyerViewModel(int id)
        {
            BuyerViewModel buyerviewmodel = (from b in db.Buyer
                                             join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                             from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                             join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                             from PersonTab in PersonTable.DefaultIfEmpty()
                                             join pa in db.PersonAddress on PersonTab.PersonID equals pa.PersonId into PersonAddressTable
                                             from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                             join ac in db.LedgerAccount on PersonTab.PersonID equals ac.PersonId into AccountTable
                                             from AccountTab in AccountTable.DefaultIfEmpty()
                                             where PersonTab.PersonID == id
                                             select new BuyerViewModel
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
                                                 PersonRateGroupId = BusinessEntityTab.PersonRateGroupId,
                                                 CreaditDays = BusinessEntityTab.CreaditDays,
                                                 CreaditLimit = BusinessEntityTab.CreaditLimit,
                                                 IsActive = PersonTab.IsActive,
                                                 SalesTaxGroupPartyId = BusinessEntityTab.SalesTaxGroupPartyId,
                                                 CreatedBy = PersonTab.CreatedBy,
                                                 CreatedDate = PersonTab.CreatedDate,
                                                 SiteIds = BusinessEntityTab.SiteIds,
                                                 Tags = PersonTab.Tags,
                                                 ImageFileName = PersonTab.ImageFileName,
                                                 ImageFolderName = PersonTab.ImageFolderName,
                                                 ParentId = BusinessEntityTab.ParentId,
                                                 GuarantorId = BusinessEntityTab.GuarantorId,
                                                 IsSisterConcern = (bool?)BusinessEntityTab.IsSisterConcern ?? false,
                                                 AccountId = (int?)AccountTab.LedgerAccountId ?? 0,
                                                 PersonAddressID = (PersonAddressTab == null ? 0 : PersonAddressTab.PersonAddressID),
                                                 LedgerAccountGroupId = (int?)AccountTab.LedgerAccountGroupId ?? 0,

                                             }).FirstOrDefault();

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
                        buyerviewmodel.PersonRegistrationCstNoID = item.PersonRegistrationId;
                        buyerviewmodel.CstNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.TinNo)
                    {
                        buyerviewmodel.PersonRegistrationTinNoID = item.PersonRegistrationId;
                        buyerviewmodel.TinNo = item.RregistrationNo;
                    }
                }
            }

            string Sites = buyerviewmodel.SiteIds;
            if (Sites != null)
            {
                Sites = Sites.Replace('|', ' ');
                buyerviewmodel.SiteIds = Sites;
            }

            return buyerviewmodel;
        }


        public IQueryable<BuyerIndexViewModel> GetBuyerListForIndex()
        {
            var temp = from b in db.Buyer
                       join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       join add in db.PersonAddress on PersonTab.PersonID equals add.PersonId
                       into table2
                       from addres in table2.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new BuyerIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name,
                           Code = PersonTab.Code,
                           Suffix = PersonTab.Suffix,
                           Mobile = PersonTab.Mobile,
                           Address = addres.Address,
                           City = addres.City.CityName,
                       };
            return temp;
        }

    }

}
