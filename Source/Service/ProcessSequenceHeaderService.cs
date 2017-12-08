using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using Model.ViewModel;

namespace Service
{
    public interface IProcessSequenceHeaderService : IDisposable
    {
        ProcessSequenceHeader Create(ProcessSequenceHeader s);
        void Delete(int id);
        void Delete(ProcessSequenceHeader s);
        ProcessSequenceHeader GetProcessSequenceHeader(int id);
        ProcessSequenceHeader GetProcessSequenceHeaderForProductCollection(int id);

        ProcessSequenceHeaderIndexViewModel GetProcessSequenceHeaderVM(int id);
        ProcessSequenceHeader Find(int id);
        ProcessSequenceHeader Find(string ProcessSequenceHeaderName);        
        IQueryable<ProcessSequenceHeaderIndexViewModel> GetProcessSequenceHeaderList();
        void Update(ProcessSequenceHeader s);
        int NextId(int id);
        int PrevId(int id);

        IEnumerable<ProductProcessViewModel> FGetProductProcessFromProcessSequence(int ProcessSequenceHeaderId);

    }
    public class ProcessSequenceHeaderService : IProcessSequenceHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public ProcessSequenceHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public ProcessSequenceHeader Create(ProcessSequenceHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProcessSequenceHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProcessSequenceHeader>().Delete(id);
        }
        public void Delete(ProcessSequenceHeader s)
        {
            _unitOfWork.Repository<ProcessSequenceHeader>().Delete(s);
        }
        public void Update(ProcessSequenceHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProcessSequenceHeader>().Update(s);
        }

        public ProcessSequenceHeader GetProcessSequenceHeader(int id)
        {
            return _unitOfWork.Repository<ProcessSequenceHeader>().Query().Get().Where(m => m.ProcessSequenceHeaderId == id).FirstOrDefault();
        }

        public ProcessSequenceHeader GetProcessSequenceHeaderForProductCollection(int id)
        {
            int RefDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductCollection).DocumentTypeId;

            var temp = (from p in db.ProcessSequenceHeader
                        where p.ReferenceDocId == id && p.ReferenceDocTypeId == RefDocTypeId
                        select p).FirstOrDefault();

            if (temp != null)
                return temp;
            else
            {
                ProcessSequenceHeader Header = new ProcessSequenceHeader();
                Header.ReferenceDocTypeId = RefDocTypeId;
                return Header;
            }
        }


        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProcessSequenceHeader
                        orderby p.ProcessSequenceHeaderName
                        select p.ProcessSequenceHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProcessSequenceHeader
                        orderby p.ProcessSequenceHeaderName
                        select p.ProcessSequenceHeaderId).FirstOrDefault();
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

                temp = (from p in db.ProcessSequenceHeader
                        orderby p.ProcessSequenceHeaderName
                        select p.ProcessSequenceHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProcessSequenceHeader
                        orderby p.ProcessSequenceHeaderName
                        select p.ProcessSequenceHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ProcessSequenceHeaderIndexViewModel GetProcessSequenceHeaderVM(int id)
        {

            ProcessSequenceHeaderIndexViewModel temp = (from p in db.ProcessSequenceHeader
                                                        where p.ProcessSequenceHeaderId == id
                                                        select new ProcessSequenceHeaderIndexViewModel
                                                        {
                                                            ProcessSequenceHeaderId = p.ProcessSequenceHeaderId,
                                                            ProcessSequenceHeaderName = p.ProcessSequenceHeaderName,
                                                            CreatedBy = p.CreatedBy,
                                                            CreatedDate = p.CreatedDate,
                                                        }

                ).FirstOrDefault();
            return temp;
        }

        public ProcessSequenceHeader Find(int id)
        {
            return _unitOfWork.Repository<ProcessSequenceHeader>().Find(id);
        }

        public ProcessSequenceHeader Find(string ProcessSequenceHeaderName)
        {
            return _unitOfWork.Repository<ProcessSequenceHeader>().Query().Get().Where(m => m.ProcessSequenceHeaderName == ProcessSequenceHeaderName).FirstOrDefault();
        }

        public IQueryable<ProcessSequenceHeaderIndexViewModel> GetProcessSequenceHeaderList()
        {
            var temp = from p in db.ProcessSequenceHeader
                       orderby p.ProcessSequenceHeaderName
                       select new ProcessSequenceHeaderIndexViewModel
                       {
                           ProcessSequenceHeaderId = p.ProcessSequenceHeaderId,
                           ProcessSequenceHeaderName = p.ProcessSequenceHeaderName
                       };
            return temp;
        }

        public IEnumerable<ProductProcessViewModel> FGetProductProcessFromProcessSequence(int ProcessSequenceHeaderId)
        {
            SqlParameter SqlParameterProcessSequenceHeaderId = new SqlParameter("@ProcessSequenceHeaderId", ProcessSequenceHeaderId);

            IEnumerable<ProductProcessViewModel> ProductProcessList = db.Database.SqlQuery<ProductProcessViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetProductProcessForProcessSequence @ProcessSequenceHeaderId", SqlParameterProcessSequenceHeaderId).ToList();

            return ProductProcessList;
        }

        public void Dispose()
        {
        }
    }

    
}
