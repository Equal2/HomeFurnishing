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
using Model.ViewModel;

namespace Service
{
    public interface IJobReceiveQASettingsService : IDisposable
    {
        JobReceiveQASettings Create(JobReceiveQASettings pt,string UserName);
        void Delete(int id);
        void Delete(JobReceiveQASettings pt);
        JobReceiveQASettings Find(int id);
        void Update(JobReceiveQASettings pt,string UserName);
        JobReceiveQASettings GetJobReceiveQASettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        Task<IEquatable<JobReceiveQASettings>> GetAsync();
        Task<JobReceiveQASettings> FindAsync(int id);        
    }

    public class JobReceiveQASettingsService : IJobReceiveQASettingsService
    {
        ApplicationDbContext db;
        private bool disposed = false;
        public JobReceiveQASettingsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JobReceiveQASettings Find(int id)
        {
            return db.JobReceiveQASettings.Find(id);
        }

        public JobReceiveQASettings GetJobReceiveQASettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.JobReceiveQASettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }


        public JobReceiveQASettings Create(JobReceiveQASettings pt,string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobReceiveQASettings.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            JobReceiveQASettings temp = db.JobReceiveQASettings.Find(id);
            temp.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveQASettings.Remove(temp);
        }

        public void Delete(JobReceiveQASettings pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveQASettings.Remove(pt);
        }

        public void Update(JobReceiveQASettings pt,string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobReceiveQASettings.Add(pt);
        }


        public void Dispose()
        {
           
        }        


        public Task<IEquatable<JobReceiveQASettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveQASettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
