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

namespace Service
{
    public interface IJobReceiveQAPenaltyService : IDisposable
    {
        JobReceiveQAPenalty Create(JobReceiveQAPenalty pt, string UserName);
        void Delete(int id);
        void Delete(JobReceiveQAPenalty pt);
        JobReceiveQAPenalty Find(int id);
        void Update(JobReceiveQAPenalty pt, string UserName);
        Task<IEquatable<JobReceiveQAPenalty>> GetAsync();
        Task<JobReceiveQAPenalty> FindAsync(int id);
        JobReceiveQAPenaltyViewModel GetJobReceiveQAPenaltyForEdit(int id);
        IEnumerable<JobReceiveQAPenaltyViewModel> GetLineListForIndex(int headerId);

    }

    public class JobReceiveQAPenaltyService : IJobReceiveQAPenaltyService
    {
        ApplicationDbContext db;
        private readonly IStockService _stockService;
        public JobReceiveQAPenaltyService(ApplicationDbContext db, IUnitOfWork _unitOfWork)
        {
            this.db = db;
            _stockService = new StockService(_unitOfWork);
        }

        public JobReceiveQAPenalty Find(int id)
        {
            return db.JobReceiveQAPenalty.Find(id);
        }

        public JobReceiveQAPenalty Create(JobReceiveQAPenalty pt, string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;

            pt.ObjectState = ObjectState.Added;
            db.JobReceiveQAPenalty.Add(pt);

            if (pt.JobReceiveQALineId > 0)
            {
                List<JobReceiveQAPenalty> PenaltyLines = (from Pl in db.JobReceiveQAPenalty where Pl.JobReceiveQALineId == pt.JobReceiveQALineId select Pl).ToList();
                Decimal TotalPenalty = 0;
                if (PenaltyLines.Count() != 0)
                {
                    TotalPenalty = PenaltyLines.Sum(i => i.Amount) + pt.Amount;
                }
                else
                {
                    TotalPenalty = pt.Amount;
                }

                JobReceiveQALine Line = db.JobReceiveQALine.Find(pt.JobReceiveQALineId);
                Line.PenaltyAmt = TotalPenalty;
                Line.ObjectState = ObjectState.Modified;
                db.JobReceiveQALine.Add(Line);
            }

            return pt;
        }

        public void Delete(int id)
        {
            JobReceiveQAPenalty Temp = db.JobReceiveQAPenalty.Find(id);
            Temp.ObjectState = Model.ObjectState.Deleted;

            db.JobReceiveQAPenalty.Remove(Temp);
        }

        public void Delete(JobReceiveQAPenalty pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveQAPenalty.Remove(pt);
        }

        public void Update(JobReceiveQAPenalty pt, string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobReceiveQAPenalty.Add(pt);
        }

        public JobReceiveQAPenaltyViewModel GetJobReceiveQAPenaltyForEdit(int id)
        {
            return (from p in db.JobReceiveQAPenalty
                    where p.JobReceiveQAPenaltyId == id
                    select new JobReceiveQAPenaltyViewModel
                    {
                        JobReceiveQAPenaltyId = p.JobReceiveQAPenaltyId,
                        JobReceiveQALineId = p.JobReceiveQALineId,
                        ReasonId = p.ReasonId,
                        Amount = p.Amount,
                        Remark = p.Remark,
                        CreatedBy = p.CreatedBy,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        ModifiedDate = p.ModifiedDate,
                        OMSId = p.OMSId
                    }).FirstOrDefault();
        }

        public IEnumerable<JobReceiveQAPenaltyViewModel> GetLineListForIndex(int JobReceiveQALineId)
        {
            var pt = (from p in db.JobReceiveQAPenalty
                      where p.JobReceiveQALineId == JobReceiveQALineId
                      orderby p.Sr
                      select new JobReceiveQAPenaltyViewModel
                      {
                          JobReceiveQAPenaltyId = p.JobReceiveQAPenaltyId,
                          ReasonName = p.Reason.ReasonName,
                          Amount = p.Amount,
                          Remark = p.Remark
                      });

            return pt;
        }



        public void Dispose()
        {
        }






        public Task<IEquatable<JobReceiveQAPenalty>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveQAPenalty> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
