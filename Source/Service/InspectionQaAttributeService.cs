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
    public interface IInspectionQaAttributeService : IDisposable
    {
        InspectionQaAttributes Create(InspectionQaAttributes Qa);
        void Delete(int id);
        void Delete(InspectionQaAttributes qa);
        void Update(InspectionQaAttributes qa);
        InspectionQaAttributes Find(int id);
        InspectionQaAttributes GetInspectionQaAttribute(int id);
        IEnumerable<InspectionQaAttributes> GetInspectionQaAttributeList();
        IEnumerable<InspectionQaAttributes> GetInspectionQaAttributeListForHeader(int HeaderId);

    }

    public class InspectionQaAttributeService:IInspectionQaAttributeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWork _UnitOfWork;
        public InspectionQaAttributeService(IUnitOfWork unitofwork)
        {
            _UnitOfWork = unitofwork;
        }

        public InspectionQaAttributes Create(InspectionQaAttributes qa)
        {
            qa.ObjectState = ObjectState.Added;
            _UnitOfWork.Repository<InspectionQaAttributes>().Insert(qa);
            return (qa);
        }

        public void Delete(int id)
        {
            _UnitOfWork.Repository<InspectionQaAttributes>().Delete(id);
        }
        public void Delete(InspectionQaAttributes qa)
        {
            _UnitOfWork.Repository<InspectionQaAttributes>().Delete(qa);
        }
        public void Update(InspectionQaAttributes qa)
        {
            qa.ObjectState = ObjectState.Modified;
            _UnitOfWork.Repository<InspectionQaAttributes>().Update(qa);
        }
        public InspectionQaAttributes Find(int id)
        {
            return _UnitOfWork.Repository<InspectionQaAttributes>().Find(id);
        }
        public IEnumerable<InspectionQaAttributes> GetInspectionQaAttributeList()
        {
            return _UnitOfWork.Repository<InspectionQaAttributes>().Query().Get();
        }
        public IEnumerable<InspectionQaAttributes> GetInspectionQaAttributeListForHeader(int id)
        {
            return _UnitOfWork.Repository<InspectionQaAttributes>().Query().Include(m=>m.ProductTypeQaRankAttribute).Get().Where(m => m.InspectionHeaderId == id);
        }
        public InspectionQaAttributes GetInspectionQaAttribute(int id)
        {
            return _UnitOfWork.Repository<InspectionQaAttributes>().Query().Get().Where(m => m.InspectionQaAttributesId == id).FirstOrDefault();
        }

        public void Dispose()
        {

        }
    }
}
