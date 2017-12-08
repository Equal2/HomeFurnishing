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
    //temprory user password store service
    //ToDo: need to remove after Identity 2.0 implimentation
    public interface ITempUserStoreService : IDisposable
    {
        int Create(TempUserStore pt);


        TempUserStore FindByEmail(string email);
    }

    public class TempUserStoreService : ITempUserStoreService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;       
        public TempUserStoreService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;           
        }


        public TempUserStore FindByEmail(string email)
        {
            var retval = db.TempUserStore.Where(m => m.Email == email).FirstOrDefault();//_unitOfWork.Repository<TempUserStore>().Query().Get().FirstOrDefault(m=>m.Email==email);
            return retval;
        }

        public int Create(TempUserStore pt)
        {
            pt.ObjectState = Model.ObjectState.Added;
            db.TempUserStore.Add(pt);

            int ret = 0;
            try { 
            ret= db.SaveChanges();
            }
            catch(Exception ex)
            {
            }
          
            return ret;
        }
        public void Dispose()
        {
        }

        public void Create(string p)
        {
            throw new NotImplementedException();
        }
    }
}
