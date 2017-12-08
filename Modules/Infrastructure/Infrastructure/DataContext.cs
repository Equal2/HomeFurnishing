#region

using Microsoft.AspNet.Identity.EntityFramework;
using Model;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


#endregion

namespace Infrastructure.IO
{
    public class DataContext : IdentityDbContext<IdentityUser>, IDataContext
    {
        private readonly Guid _instanceId;
        public DataContext(string nameOrConnectionString,bool Schema) : base(nameOrConnectionString,Schema)
        {
            _instanceId = Guid.NewGuid();
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }         

        public Guid InstanceId
        {
            get { return _instanceId; }
        }

        public new DbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public override int SaveChanges()
        {
            SyncObjectsStatePreCommit();
            var changes = base.SaveChanges();
            SyncObjectsStatePostCommit();
            return changes;
        }

        public override Task<int> SaveChangesAsync()
        {
            SyncObjectsStatePreCommit();
            var changesAsync = base.SaveChangesAsync();
            SyncObjectsStatePostCommit();
            return changesAsync;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            Task<int> changesAsync = null;
            try
            {
                SyncObjectsStatePreCommit();

                changesAsync = base.SaveChangesAsync(cancellationToken);
                SyncObjectsStatePostCommit();

            }
            catch { }
            //catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            //{
            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
            //        }
            //    }
            //}
            return changesAsync;
        }

        public void SyncObjectState(object entity)
        {
            Entry(entity).State = StateHelper.ConvertState(((IObjectState)entity).ObjectState);
        }
        private void SyncObjectsStatePreCommit()
        { 
            foreach (var dbEntityEntry in ChangeTracker.Entries())
            {
                             
                if (
                        dbEntityEntry.Entity.GetType() == typeof(Microsoft.AspNet.Identity.EntityFramework.IdentityRole)
                        || dbEntityEntry.Entity.GetType() == typeof(Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole)
                        || dbEntityEntry.Entity.GetType() == typeof(Microsoft.AspNet.Identity.EntityFramework.IdentityUserClaim)                    
                    )

                    continue;

                dbEntityEntry.State = StateHelper.ConvertState(((IObjectState)dbEntityEntry.Entity).ObjectState);
            }
        }

        private void SyncObjectsStatePostCommit()
        {
            foreach (var dbEntityEntry in ChangeTracker.Entries())
            {
                if (
                        dbEntityEntry.Entity.GetType() == typeof(Microsoft.AspNet.Identity.EntityFramework.IdentityRole)
                        || dbEntityEntry.Entity.GetType() == typeof(Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole)
                        || dbEntityEntry.Entity.GetType() == typeof(Microsoft.AspNet.Identity.EntityFramework.IdentityUserClaim)
                    )
                    continue;

                ((IObjectState)dbEntityEntry.Entity).ObjectState = StateHelper.ConvertState(dbEntityEntry.State);
            }
        }
    }
}