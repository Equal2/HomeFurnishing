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
    public interface IDocumentCategoryService : IDisposable
    {
        DocumentCategory Create(DocumentCategory pt);
        void Delete(int id);
        void Delete(DocumentCategory pt);
        IEnumerable<DocumentCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocumentCategory pt);
        DocumentCategory Add(DocumentCategory pt);
        IEnumerable<DocumentCategory> GetDocumentCategoryList();

        Task<IEquatable<DocumentCategory>> GetAsync();
        Task<DocumentCategory> FindAsync(int id);
        DocumentCategory Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DocumentCategoryService : IDocumentCategoryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public DocumentCategoryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public DocumentCategory Create(DocumentCategory pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocumentCategory>().Insert(pt);
            return pt;
        }

        public DocumentCategory Find(int id)
        {
           return _unitOfWork.Repository<DocumentCategory>().Find(id);
        }

        public DocumentCategory FindByName(string DocumentCategoryName)
        {
            return _unitOfWork.Repository<DocumentCategory>().Query().Get().Where(i => i.DocumentCategoryName == DocumentCategoryName).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocumentCategory>().Delete(id);
        }

        public void Delete(DocumentCategory pt)
        {
            _unitOfWork.Repository<DocumentCategory>().Delete(pt);
        }

        public void Update(DocumentCategory pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocumentCategory>().Update(pt);
        }

        public IEnumerable<DocumentCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocumentCategory>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocumentCategoryName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<DocumentCategory> GetDocumentCategoryList()
        {
            var pt = _unitOfWork.Repository<DocumentCategory>().Query().Get().OrderBy(m=>m.DocumentCategoryName);

            return pt;
        }


        public DocumentCategory Add(DocumentCategory pt)
        {
            _unitOfWork.Repository<DocumentCategory>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DocumentCategory
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentCategory
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).FirstOrDefault();
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

                temp = (from p in db.DocumentCategory
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentCategory
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }




        public void Dispose()
        {
        }


        public Task<IEquatable<DocumentCategory>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocumentCategory> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
