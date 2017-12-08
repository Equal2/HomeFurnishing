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
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface IPersonProductUidService : IDisposable
    {
        PersonProductUid Create(PersonProductUid p);
        //void Delete(int id);
        //void Delete(PersonProductUid p);
        //PersonProductUid Find(string PersonProductUidName);
        IQueryable<PersonProductUidViewModel> GetPersonProductUidList(int GenDocTypeId, int GenDocId, int GenDocLineId);
        PersonProductUid Find(int id);
        PersonProductUid FindPendingToPack(int SaleOrderLineid);

    }



    public class PersonProductUidService : IPersonProductUidService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonProductUid> _productRepository;
        //private readonly Repository<PersonProductUidDimension> _productdimensionRepository;

        RepositoryQuery<PersonProductUid> productRepository;
        //RepositoryQuery<PersonProductUidDimension> productdimensionRepository;

        public PersonProductUidService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productRepository = new Repository<PersonProductUid>(db);
            //_productdimensionRepository = new Repository<PersonProductUidDimension>(db);

            productRepository = new RepositoryQuery<PersonProductUid>(_productRepository);
            // productdimensionRepository = new RepositoryQuery<PersonProductUidDimension>(_productdimensionRepository);
        }



        public PersonProductUid Create(PersonProductUid p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonProductUid>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonProductUid>().Delete(id);
        }

        public void Delete(PersonProductUid p)
        {
            _unitOfWork.Repository<PersonProductUid>().Delete(p);
        }

        public void Update(PersonProductUid p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonProductUid>().Update(p);
        }




        public PersonProductUid Find(int id)
        {

            return _unitOfWork.Repository<PersonProductUid>().Find(id);
        }


        public PersonProductUid FindPendingToPack(int SaleOrderLineid)
        {

            var temp = from p in db.PersonProductUid
                       join PC in db.PackingLine on p.PersonProductUidId equals PC.PersonProductUidId into PCTable
                       from PCTab in PCTable.DefaultIfEmpty()
                       where p.SaleOrderLineId == SaleOrderLineid && PCTab ==null  
                       select p;
            if (temp.Count() > 0)
                return temp.ToList().First();
            else
                return null;

        }

        public IQueryable<PersonProductUidViewModel> GetPersonProductUidList(int GenDocTypeId, int GenDocId,int GenDocLineId)
        {

            var temp = from p in db.PersonProductUid
                       where p.GenDocTypeId == GenDocTypeId && p.GenDocId == GenDocId && p.GenLineId == GenDocLineId
                       select new PersonProductUidViewModel
                       {
                           ProductUidName = p.ProductUidName,
                           ProductUidSpecification = p.ProductUidSpecification,
                           PersonProductUidId = p.PersonProductUidId,
                           GenDocId = p.GenDocId,
                           GenDocTypeId = p.GenDocTypeId,
                           GenDocLineId = p.GenLineId,
                       };

            if(temp.Count() == 0)
            {

                SaleEnquiryLine SEL = new SaleEnquiryLineService(_unitOfWork).Find(GenDocLineId);
                List<PersonProductUidViewModel> employees = new List<PersonProductUidViewModel>();
                for (int i = 0; i < SEL.Qty ; i++)
                {
                    employees.Add(new PersonProductUidViewModel());
                }


                IQueryable<PersonProductUidViewModel> PPU = employees.AsQueryable();

                return PPU;

            }
            else 
            return temp;

        }

        public PersonProductUid Add(PersonProductUid p)
        {
            _unitOfWork.Repository<PersonProductUid>().Add(p);
            return p;
        }





        public void Dispose()
        {
        }


        public Task<IEquatable<PersonProductUid>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonProductUid> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


    }
}
