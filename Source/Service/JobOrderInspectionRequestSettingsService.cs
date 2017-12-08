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
    public interface IJobOrderInspectionRequestSettingsService : IDisposable
    {
        JobOrderInspectionRequestSettings Create(JobOrderInspectionRequestSettings pt,string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionRequestSettings pt);
        JobOrderInspectionRequestSettings Find(int id);
        void Update(JobOrderInspectionRequestSettings pt,string UserName);
        JobOrderInspectionRequestSettings GetJobOrderInspectionRequestSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        Task<IEquatable<JobOrderInspectionRequestSettings>> GetAsync();
        Task<JobOrderInspectionRequestSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class JobOrderInspectionRequestSettingsService : IJobOrderInspectionRequestSettingsService
    {
        private ApplicationDbContext db;
        public JobOrderInspectionRequestSettingsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JobOrderInspectionRequestSettings Find(int id)
        {
            return db.JobOrderInspectionRequestSettings.Find(id);
        }

        public JobOrderInspectionRequestSettings GetJobOrderInspectionRequestSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.JobOrderInspectionRequestSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public JobOrderInspectionRequestSettings Create(JobOrderInspectionRequestSettings pt,string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionRequestSettings.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var record = db.JobOrderInspectionRequestSettings.Find(id);
            record.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestSettings.Remove(record);
        }

        public void Delete(JobOrderInspectionRequestSettings pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestSettings.Remove(pt);
        }

        public void Update(JobOrderInspectionRequestSettings pt,string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionRequestSettings.Add(pt);
        }

        
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderInspectionRequestSettings
                        orderby p.JobOrderInspectionRequestSettingsId
                        select p.JobOrderInspectionRequestSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestSettings
                        orderby p.JobOrderInspectionRequestSettingsId
                        select p.JobOrderInspectionRequestSettingsId).FirstOrDefault();
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

                temp = (from p in db.JobOrderInspectionRequestSettings
                        orderby p.JobOrderInspectionRequestSettingsId
                        select p.JobOrderInspectionRequestSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestSettings
                        orderby p.JobOrderInspectionRequestSettingsId
                        select p.JobOrderInspectionRequestSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderInspectionRequestSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionRequestSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
