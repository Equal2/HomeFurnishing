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
    public interface IJobOrderInspectionSettingsService : IDisposable
    {
        JobOrderInspectionSettings Create(JobOrderInspectionSettings pt,string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionSettings pt);
        JobOrderInspectionSettings Find(int id);
        void Update(JobOrderInspectionSettings pt,string UserName);
        JobOrderInspectionSettings GetJobOrderInspectionSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        Task<IEquatable<JobOrderInspectionSettings>> GetAsync();
        Task<JobOrderInspectionSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class JobOrderInspectionSettingsService : IJobOrderInspectionSettingsService
    {
        private ApplicationDbContext db;
        public JobOrderInspectionSettingsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JobOrderInspectionSettings Find(int id)
        {
            return db.JobOrderInspectionSettings.Find(id);
        }

        public JobOrderInspectionSettings GetJobOrderInspectionSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.JobOrderInspectionSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();


        }
        public JobOrderInspectionSettings Create(JobOrderInspectionSettings pt,string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionSettings.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var record = db.JobOrderInspectionSettings.Find(id);
            record.ObjectState = Model.ObjectState.Deleted;

            db.JobOrderInspectionSettings.Remove(record);
        }

        public void Delete(JobOrderInspectionSettings pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionSettings.Remove(pt);
        }

        public void Update(JobOrderInspectionSettings pt,string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionSettings.Add(pt);
        }
      
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderInspectionSettings
                        orderby p.JobOrderInspectionSettingsId
                        select p.JobOrderInspectionSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionSettings
                        orderby p.JobOrderInspectionSettingsId
                        select p.JobOrderInspectionSettingsId).FirstOrDefault();
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

                temp = (from p in db.JobOrderInspectionSettings
                        orderby p.JobOrderInspectionSettingsId
                        select p.JobOrderInspectionSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionSettings
                        orderby p.JobOrderInspectionSettingsId
                        select p.JobOrderInspectionSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderInspectionSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
