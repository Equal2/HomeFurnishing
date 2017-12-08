using CustomEventArgs;
using Data.Models;
using System;
using MaterialRequestCancelDocumentEvents;

namespace Jobs.Controllers
{


    public class MaterialRequestCancelEvents : MaterialRequestCancelDocEvents
    {        
        //For Subscribing Events
        public MaterialRequestCancelEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += MaterialRequestCancelEvents__beforeHeaderSave;
            //_onHeaderSave += MaterialRequestCancelEvents__onHeaderSave;
            //_afterHeaderSave += MaterialRequestCancelEvents__afterHeaderSave;
            //_beforeHeaderDelete += MaterialRequestCancelEvents__beforeHeaderDelete;
            //_onLineSave += MaterialRequestCancelEvents__onLineSave;
            //_beforeLineDelete += MaterialRequestCancelEvents__beforeLineDelete;
            //_afterHeaderDelete += MaterialRequestCancelEvents__afterHeaderDelete;
            //_onLineSaveBulk += MaterialRequestCancelEvents__onLineSaveBulk;
        }

        void MaterialRequestCancelEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialRequestCancelEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool MaterialRequestCancelEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialRequestCancelEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool MaterialRequestCancelEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialRequestCancelEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialRequestCancelEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var MaterialRequestCancel = db.MaterialRequestCancelHeader.Local.Where(m => m.MaterialRequestCancelHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == MaterialRequestCancel.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool MaterialRequestCancelEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
