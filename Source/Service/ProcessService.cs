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

namespace Service
{
    public interface IProcessService : IDisposable
    {
        Process Create(Process p);
        void Delete(int id);
        void Delete(Process p);
        
        IEnumerable<Process> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Process p);
        Process Add(Process p);
        IEnumerable<Process> GetProcessList();
        IEnumerable<Process> GetProcessList(int prodyctTypeId);
        Task<IEquatable<Process>> GetAsync();
        Task<Process> FindAsync(int id);
        Process Find(string ProcessName);
        Process Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }



    public class ProcessService : IProcessService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Process> _productRepository;
        //private readonly Repository<ProcessDimension> _productdimensionRepository;

        RepositoryQuery<Process> productRepository;
        //RepositoryQuery<ProcessDimension> productdimensionRepository;

        public ProcessService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productRepository = new Repository<Process>(db);
            //_productdimensionRepository = new Repository<ProcessDimension>(db);

            productRepository = new RepositoryQuery<Process>(_productRepository);
            // productdimensionRepository = new RepositoryQuery<ProcessDimension>(_productdimensionRepository);
        }

        public Process Create(Process p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Process>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Process>().Delete(id);
        }

        public void Delete(Process p)
        {
            _unitOfWork.Repository<Process>().Delete(p);
        }

        public void Update(Process p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Process>().Update(p);
        }

        public IEnumerable<Process> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Process>()
                .Query()
                .Filter(q => !string.IsNullOrEmpty(q.ProcessName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IEnumerable<Process> GetProcessList()
        {
            var p = _unitOfWork.Repository<Process>().Query().Get().OrderBy(m => m.ProcessName) ;

            return p;

        }

        public IEnumerable<Process> GetAccessoryList()
        {
            var p = _unitOfWork.Repository<Process>().Query().Get();
            return p;
        }



        public Process Find(string ProcessName)
        {

            Process p = _unitOfWork.Repository<Process>().Query().Get().Where(i => i.ProcessName == ProcessName).FirstOrDefault();

            return p;
        }

        public Process Find(int id)
        {
            return _unitOfWork.Repository<Process>().Find(id);
        }


        public IEnumerable<Process> GetProcessList(int productTypeId)
        {
            return _unitOfWork.Repository<Process>().Query().Get().OrderBy(m => m.ProcessName);
        }

        public Process Add(Process p)
        {
            _unitOfWork.Repository<Process>().Add(p);
            return p;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Process
                        orderby p.ProcessName
                        select p.ProcessId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Process
                        orderby p.ProcessName
                        select p.ProcessId).FirstOrDefault();
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

                temp = (from p in db.Process
                        orderby p.ProcessName
                        select p.ProcessId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Process
                        orderby p.ProcessName
                        select p.ProcessId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }




        public void Dispose()
        {
        }


        public Task<IEquatable<Process>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Process> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
