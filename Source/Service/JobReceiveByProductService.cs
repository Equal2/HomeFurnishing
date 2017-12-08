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
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;


namespace Service
{
    public interface IJobReceiveByProductService : IDisposable
    {
        JobReceiveByProduct Create(JobReceiveByProduct s);
        void Delete(int id);
        void Delete(JobReceiveByProduct s);
        JobReceiveByProduct Find(int id);
        void Update(JobReceiveByProduct s);
        JobReceiveByProductViewModel GetJobReceiveByProduct(int id);
    }

    public class JobReceiveByProductService : IJobReceiveByProductService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobReceiveByProductService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public JobReceiveByProduct Create(JobReceiveByProduct S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveByProduct>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveByProduct>().Delete(id);
        }

        public void Delete(JobReceiveByProduct s)
        {
            _unitOfWork.Repository<JobReceiveByProduct>().Delete(s);
        }

        public void Update(JobReceiveByProduct s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveByProduct>().Update(s);
        }



        public JobReceiveByProduct Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveByProduct>().Find(id);
        }

        public JobReceiveByProductViewModel GetJobReceiveByProduct(int id)
        {

            return (from p in db.JobReceiveByProduct
                    where p.JobReceiveByProductId == id
                    select new JobReceiveByProductViewModel
                    {
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        Dimension3Id = p.Dimension3Id,
                        Dimension4Id = p.Dimension4Id,
                        JobReceiveByProductId = p.JobReceiveByProductId,
                        JobReceiveHeaderId = p.JobReceiveHeaderId,
                        LotNo = p.LotNo,
                        ProductId = p.ProductId,
                        Qty = p.Qty,
                        UnitId = p.Product.UnitId,
                    }
                       ).FirstOrDefault();

        }
        public void Dispose()
        {
        }
    }
}
