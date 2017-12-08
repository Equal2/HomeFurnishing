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
    public interface IPersonDocumentService : IDisposable
    {
        PersonDocument Create(PersonDocument pt);
        void Delete(int id);
        void Delete(PersonDocument pt);
        PersonDocument GetPersonDocument(int ptId);
        IEnumerable<PersonDocument> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonDocument pt);
        PersonDocument Add(PersonDocument pt);
        IEnumerable<PersonDocument> GetPersonDocumentList();
        IEnumerable<PersonDocument> GetPersonDocumentList(int id);
        PersonDocument Find(int id);
        // IEnumerable<PersonDocument> GetPersonDocumentList(int buyerId);
        Task<IEquatable<PersonDocument>> GetAsync();
        Task<PersonDocument> FindAsync(int id);
        IQueryable<PersonDocumentViewModel> GetPersonDocumentListForIndex(int PersonId);
    }

    public class PersonDocumentService : IPersonDocumentService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonDocument> _PersonDocumentRepository;
        RepositoryQuery<PersonDocument> PersonDocumentRepository;
        public PersonDocumentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonDocumentRepository = new Repository<PersonDocument>(db);
            PersonDocumentRepository = new RepositoryQuery<PersonDocument>(_PersonDocumentRepository);
        }

        public PersonDocument GetPersonDocument(int pt)
        {
            //return PersonDocumentRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            //return _unitOfWork.Repository<PersonDocument>().Find(pt);
            //return PersonDocumentRepository.Get().Where(i => i.PersonDocumentId == pt).FirstOrDefault();

            return PersonDocumentRepository.Include(r => r.Person).Get().Where(i => i.PersonDocumentID == pt).FirstOrDefault();
        }

        public PersonDocument Create(PersonDocument pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonDocument>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonDocument>().Delete(id);
        }

        public void Delete(PersonDocument pt)
        {
            _unitOfWork.Repository<PersonDocument>().Delete(pt);
        }

        public void Update(PersonDocument pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonDocument>().Update(pt);
        }

        public IEnumerable<PersonDocument> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonDocument>()
                .Query()
                //.OrderBy(q => q.OrderBy(c => c.Supplier ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonDocument> GetPersonDocumentList()
        {
            var pt = _unitOfWork.Repository<PersonDocument>().Query().Include(p => p.Person).Get();
            return pt;
        }

        public PersonDocument Add(PersonDocument pt)
        {
            _unitOfWork.Repository<PersonDocument>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PersonDocument>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonDocument> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<PersonDocument> GetPersonDocumentList(int id)
        {
            var pt = _unitOfWork.Repository<PersonDocument>().Query().Include(m=>m.Person).Get().Where(m => m.PersonId == id).ToList();
            return pt;
        }
        public PersonDocument Find(int id)
        {
            return _unitOfWork.Repository<PersonDocument>().Find(id);
        }

        public IEnumerable<PersonDocument> GetPersonDocumentIdListByPersonId(int PersonId)
        {
            var pt = _unitOfWork.Repository<PersonDocument>().Query().Get().Where(m => m.PersonId == PersonId).ToList();
            return pt;
        }

        public IQueryable<PersonDocumentViewModel> GetPersonDocumentListForIndex(int PersonId)
        {
            var temp = from pc in db.PersonDocument
                       orderby pc.PersonDocumentID
                       where pc.PersonId == PersonId
                       select new PersonDocumentViewModel
                       {
                           PersonDocumentID = pc.PersonDocumentID,
                           PersonId = pc.PersonId,
                           Name = pc.Name,
                           Description = pc.Description
                       };
            return temp;
        }

    }
}
