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
    public interface IManufacturerService : IDisposable
    {
        Manufacturer Create(Manufacturer Manufacturer);
        void Delete(int id);
        void Delete(Manufacturer Manufacturer);
        Manufacturer GetManufacturer(int ManufacturerId);
        IEnumerable<Manufacturer> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Manufacturer Manufacturer);
        Manufacturer Add(Manufacturer Manufacturer);
        IEnumerable<Manufacturer> GetManufacturerList();
        Task<IEquatable<Manufacturer>> GetAsync();
        Task<Manufacturer> FindAsync(int id);
        Manufacturer GetManufacturerByName(string Name);
        Manufacturer Find(int id);
        ManufacturerViewModel GetManufacturerViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ManufacturerIndexViewModel> GetManufacturerListForIndex();
    }

    public class ManufacturerService : IManufacturerService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public ManufacturerService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Manufacturer GetManufacturerByName(string Manufacturer)
        {
            return (from b in db.Manufacturer
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == Manufacturer
                    select new Manufacturer
                    {
                        PersonID = b.PersonID,
                    }).FirstOrDefault();
        }
        public Manufacturer GetManufacturer(int ManufacturerId)
        {
            return _unitOfWork.Repository<Manufacturer>().Find(ManufacturerId);
        }

        public Manufacturer Find(int id)
        {
            return _unitOfWork.Repository<Manufacturer>().Find(id);
        }

        public Manufacturer Create(Manufacturer Manufacturer)
        {
            Manufacturer.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Manufacturer>().Insert(Manufacturer);
            return Manufacturer;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Manufacturer>().Delete(id);
        }

        public void Delete(Manufacturer Manufacturer)
        {
            _unitOfWork.Repository<Manufacturer>().Delete(Manufacturer);
        }

        public void Update(Manufacturer Manufacturer)
        {
            Manufacturer.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Manufacturer>().Update(Manufacturer);
        }

        public IEnumerable<Manufacturer> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var Manufacturer = _unitOfWork.Repository<Manufacturer>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return Manufacturer;
        }

        public IEnumerable<Manufacturer> GetManufacturerList()
        {
            var Manufacturer = _unitOfWork.Repository<Manufacturer>().Query().Include(m => m.Person)
                .Get()
                .Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return Manufacturer;
        }

        public Manufacturer Add(Manufacturer Manufacturer)
        {
            _unitOfWork.Repository<Manufacturer>().Insert(Manufacturer);
            return Manufacturer;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Manufacturer>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Manufacturer> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in db.Manufacturer
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.Manufacturer
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

                temp = (from b in db.Manufacturer
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.Manufacturer
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

        public ManufacturerViewModel GetManufacturerViewModel(int id)
        {
            ManufacturerViewModel Manufacturerviewmodel = (from b in db.Manufacturer
                                             join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                             from PersonTab in PersonTable.DefaultIfEmpty()
                                             where PersonTab.PersonID == id
                                             select new ManufacturerViewModel
                                             {
                                                 PersonId = PersonTab.PersonID,
                                                 Name = PersonTab.Name,
                                                 Suffix = PersonTab.Suffix,
                                                 Code = PersonTab.Code,
                                                 Description = PersonTab.Description,
                                                 IsActive = PersonTab.IsActive,
                                                 CreatedBy = PersonTab.CreatedBy,
                                                 CreatedDate = PersonTab.CreatedDate,
                                                 ImageFileName = PersonTab.ImageFileName,
                                                 ImageFolderName = PersonTab.ImageFolderName
                                             }).FirstOrDefault();

            return Manufacturerviewmodel;
        }


        public IQueryable<ManufacturerIndexViewModel> GetManufacturerListForIndex()
        {
            var temp = from b in db.Manufacturer
                       join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new ManufacturerIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name
                       };
            return temp;
        }

    }

}
