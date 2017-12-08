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
    public interface IImportLineService : IDisposable
    {
        ImportLine Create(ImportLine pt);
        void Delete(int id);
        void Delete(ImportLine pt);
        ImportLine Find(int id);
        void Update(ImportLine pt);
        ImportLine Add(ImportLine pt);
        IEnumerable<ImportLine> GetImportLineList(int id);
        ImportLine GetImportLine(int id);
        ImportLine GetImportLineByName(string Name,int HeaderID);
    }

    public class ImportLineService : IImportLineService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public ImportLineService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
        }
        public ImportLine Find(int id)
        {
            return _unitOfWork.Repository<ImportLine>().Find(id);            
        }

        public ImportLine Create(ImportLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ImportLine>().Insert(pt);
            return pt;
        }
        public ImportLine GetImportLine(int id)
        {
            return ((from p in db.ImportLine
                        where p.ImportLineId==id
                        select p).FirstOrDefault()
                        );
        }
        public ImportLine GetImportLineByName(string Name,int HeaderID)
        {
            return (from p in db.ImportLine
                    where p.ImportHeaderId == HeaderID && p.FieldName == Name
                    select p
                        ).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ImportLine>().Delete(id);
        }

        public void Delete(ImportLine pt)
        {
            _unitOfWork.Repository<ImportLine>().Delete(pt);
        }

        public void Update(ImportLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ImportLine>().Update(pt);
        }

        public IEnumerable<ImportLine> GetImportLineList(int id)
        {
            var pt = _unitOfWork.Repository<ImportLine>().Query().Get().Where(m=>m.ImportHeaderId==id).OrderBy(m=>m.Serial);

            return pt;
        }

        public ImportLine Add(ImportLine pt)
        {
            _unitOfWork.Repository<ImportLine>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }

    }
}
