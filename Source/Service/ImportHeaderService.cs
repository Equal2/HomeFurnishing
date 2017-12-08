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
    public interface IImportHeaderService : IDisposable
    {
        ImportHeader Create(ImportHeader pt);
        void Delete(int id);
        void Delete(ImportHeader pt);
        ImportHeader Find(int id);
        void Update(ImportHeader pt);
        ImportHeader Add(ImportHeader pt);
        IEnumerable<ImportHeader> GetImportHeaderList();
        ImportHeader GetImportHeader(int id);
        ImportHeader GetImportHeaderByName(string name);
        IEnumerable<ImportHeader> GetImportHeaderListForCopy(int id);

    }

    public class ImportHeaderService : IImportHeaderService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public ImportHeaderService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
        }
        //public ImportHeaderService(ApplicationDbContext db)
        //{
        //    this.db = db;            
        //}

        public ImportHeader Find(int id)
        {
            return _unitOfWork.Repository<ImportHeader>().Find(id);            
        }

        public ImportHeader Create(ImportHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ImportHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ImportHeader>().Delete(id);
        }

        public void Delete(ImportHeader pt)
        {
            _unitOfWork.Repository<ImportHeader>().Delete(pt);
        }
        public ImportHeader GetImportHeader(int id)
        {
            return ((from p in db.ImportHeader
                     where p.ImportHeaderId == id
                     select p
                         ).FirstOrDefault());
        }
        public ImportHeader GetImportHeaderByName(string name)
        {
            return ((from p in db.ImportHeader
                     where p.ImportName == name
                     select p
                         ).FirstOrDefault());
        }

        public void Update(ImportHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ImportHeader>().Update(pt);
        }

        public IEnumerable<ImportHeader> GetImportHeaderList()
        {
            var pt = _unitOfWork.Repository<ImportHeader>().Query().Get();

            return pt;
        }

        public IEnumerable<ImportHeader> GetImportHeaderListForCopy(int id)
        {
            var pt = _unitOfWork.Repository<ImportHeader>().Query().Get().Where(m=>m.ImportHeaderId!=id);

            return pt;
        }

        public ImportHeader Add(ImportHeader pt)
        {
            _unitOfWork.Repository<ImportHeader>().Insert(pt);
            return pt;
        }



        public void Dispose()
        {
        }

    }
}
