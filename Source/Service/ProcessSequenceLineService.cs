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
    public interface IProcessSequenceLineService : IDisposable
    {
        ProcessSequenceLine Create(ProcessSequenceLine s);
        void Delete(int id);
        void Delete(ProcessSequenceLine s);
        ProcessSequenceLine GetProcessSequenceLine(int id);
        ProcessSequenceLineViewModel GetProcessSequenceLineModel(int id);
        ProcessSequenceLine Find(int id);
        void Update(ProcessSequenceLine s);
        IEnumerable<ProcessSequenceLineIndexViewModel> GetProcessSequenceLineList(int ProcessSequenceHeaderId);
        IQueryable<ProcessSequenceLineIndexViewModel> GetProcessSequenceLineListForIndex(int ProcessSequenceHeaderId);
        IQueryable<ProcessSequenceLineIndexViewModel> GetProcessSequenceLineListForProductCollectionIndex(int ProductCollectionId);
    }

    public class ProcessSequenceLineService : IProcessSequenceLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public ProcessSequenceLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProcessSequenceLine Create(ProcessSequenceLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProcessSequenceLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProcessSequenceLine>().Delete(id);
        }

        public void Delete(ProcessSequenceLine s)
        {
            _unitOfWork.Repository<ProcessSequenceLine>().Delete(s);
        }

        public void Update(ProcessSequenceLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProcessSequenceLine>().Update(s);
        }

        public ProcessSequenceLine GetProcessSequenceLine(int id)
        {
            return _unitOfWork.Repository<ProcessSequenceLine>().Query().Get().Where(m => m.ProcessSequenceLineId == id).FirstOrDefault();
        }

        public ProcessSequenceLineViewModel GetProcessSequenceLineModel(int id)
        {
            return (from L in db.ProcessSequenceLine
                    join P in db.Process on L.ProcessId equals P.ProcessId into ProcessTable from ProcessTab in ProcessTable.DefaultIfEmpty()
                    where L.ProcessSequenceLineId == id
                    select new ProcessSequenceLineViewModel
                    {
                        ProcessSequenceHeaderId = L.ProcessSequenceHeaderId,
                        ProcessSequenceLineId = L.ProcessSequenceLineId,
                        ProcessId = L.ProcessId,
                        ProcessName = ProcessTab.ProcessName,
                        Sequence = L.Sequence,
                        CreatedBy = L.CreatedBy,
                        CreatedDate = L.CreatedDate,
                        ModifiedBy = L.ModifiedBy,
                        ModifiedDate = L.ModifiedDate,
                    }).FirstOrDefault();
        }


        
        public ProcessSequenceLine Find(int id)
        {
            return _unitOfWork.Repository<ProcessSequenceLine>().Find(id);
        }

        public IEnumerable<ProcessSequenceLineIndexViewModel> GetProcessSequenceLineList(int ProcessSequenceHeaderId)
        {
            return (from L in db.ProcessSequenceLine
                    join P in db.Process on L.ProcessId equals P.ProcessId into ProcessTable
                    from ProcessTab in ProcessTable.DefaultIfEmpty()
                    where L.ProcessSequenceHeaderId == ProcessSequenceHeaderId
                    orderby L.Sequence
                    select new ProcessSequenceLineIndexViewModel
                    {
                        ProcessSequenceHeaderId = L.ProcessSequenceHeaderId,
                        ProcessSequenceLineId = L.ProcessSequenceLineId,
                        ProcessId = L.ProcessId,
                        ProcessName = ProcessTab.ProcessName,
                        Sequence = L.Sequence,
                        Days = L.Days,
                        CreatedBy = L.CreatedBy,
                        CreatedDate = L.CreatedDate,
                        ModifiedBy = L.ModifiedBy,
                        ModifiedDate = L.ModifiedDate,
                    }
                );
        }

        public IQueryable<ProcessSequenceLineIndexViewModel> GetProcessSequenceLineListForIndex(int ProcessSequenceHeaderId)
        {
            var temp = from L in db.ProcessSequenceLine
                       join P in db.Process on L.ProcessId equals P.ProcessId into ProcessTable
                       from ProcessTab in ProcessTable.DefaultIfEmpty()
                       where L.ProcessSequenceHeaderId == ProcessSequenceHeaderId
                       orderby L.Sequence
                       select new ProcessSequenceLineIndexViewModel
                       {
                           ProcessSequenceHeaderId = L.ProcessSequenceHeaderId,
                           ProcessSequenceLineId = L.ProcessSequenceLineId,
                           ProcessId = L.ProcessId,
                           ProcessName = ProcessTab.ProcessName,
                           Sequence = L.Sequence,
                           Days = L.Days,
                           CreatedBy = L.CreatedBy,
                           CreatedDate = L.CreatedDate,
                           ModifiedBy = L.ModifiedBy,
                           ModifiedDate = L.ModifiedDate,
                       };
            return temp;
        }

        public IQueryable<ProcessSequenceLineIndexViewModel> GetProcessSequenceLineListForProductCollectionIndex(int ProductCollectionId)
        {

            int RefDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductCollection).DocumentTypeId;

            var temp = from L in db.ProcessSequenceLine
                       join P in db.Process on L.ProcessId equals P.ProcessId into ProcessTable
                       from ProcessTab in ProcessTable.DefaultIfEmpty()
                       join t in db.ProcessSequenceHeader on L.ProcessSequenceHeaderId equals t.ProcessSequenceHeaderId
                       where t.ReferenceDocId == ProductCollectionId && t.ReferenceDocTypeId==RefDocTypeId
                       orderby L.Sequence
                       select new ProcessSequenceLineIndexViewModel
                       {
                           ProcessSequenceHeaderId = L.ProcessSequenceHeaderId,
                           ProcessSequenceLineId = L.ProcessSequenceLineId,
                           ProcessId = L.ProcessId,
                           ProcessName = ProcessTab.ProcessName,
                           Sequence = L.Sequence,
                           Days = L.Days,
                           CreatedBy = L.CreatedBy,
                           CreatedDate = L.CreatedDate,
                           ModifiedBy = L.ModifiedBy,
                           ModifiedDate = L.ModifiedDate,
                       };
            return temp;
        }

        public void Dispose()
        {
        }
    }
}
