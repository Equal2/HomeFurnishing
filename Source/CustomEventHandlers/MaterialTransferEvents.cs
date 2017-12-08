using CustomEventArgs;
using Data.Models;
using System;
using MaterialTransferDocumentEvents;

namespace Jobs.Controllers
{
    public class MaterialTransferEvents : MaterialTransferDocEvents
    {        
        //For Subscribing Events
        public MaterialTransferEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += MaterialTransferEvents__beforeHeaderSave;
            //_onHeaderSave += MaterialTransferEvents__onHeaderSave;
            //_afterHeaderSave += MaterialTransferEvents__afterHeaderSave;
            //_beforeHeaderDelete += MaterialTransferEvents__beforeHeaderDelete;
            //_onLineSave += MaterialTransferEvents__onLineSave;
            //_beforeLineDelete += MaterialTransferEvents__beforeLineDelete;
            //_afterHeaderDelete += MaterialTransferEvents__afterHeaderDelete;
            //_onLineSaveBulk += MaterialTransferEvents__onLineSaveBulk;
        }

        void MaterialTransferEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialTransferEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool MaterialTransferEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialTransferEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool MaterialTransferEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialTransferEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialTransferEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var MaterialTransfer = db.MaterialTransferHeader.Local.Where(m => m.MaterialTransferHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == MaterialTransfer.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool MaterialTransferEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
