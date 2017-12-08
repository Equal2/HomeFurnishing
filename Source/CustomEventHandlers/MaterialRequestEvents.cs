using CustomEventArgs;
using Data.Models;
using System;
using MaterialRequestDocumentEvents;
using System.Linq;
using DocumentEvents;

namespace Jobs.Controllers
{


    public class MaterialRequestEvents : MaterialRequestDocEvents
    {
        //For Subscribing Events
        public MaterialRequestEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += MaterialRequestEvents__beforeHeaderSave;
            //_onHeaderSave += MaterialRequestEvents__onHeaderSave;
            //_afterHeaderSave += MaterialRequestEvents__afterHeaderSave;
            //_beforeHeaderDelete += MaterialRequestEvents__beforeHeaderDelete;
            _onLineSave += MaterialRequestEvents__onLineSave;
            //_beforeLineDelete += MaterialRequestEvents__beforeLineDelete;
            //_afterHeaderDelete += MaterialRequestEvents__afterHeaderDelete;
            _onLineSaveBulk += MaterialRequestEvents__onLineSaveBulk;
            _onLineDelete += MaterialRequestEvents__onLineDelete;
            _onHeaderDelete += MaterialRequestEvents__onHeaderDelete;
        }

        void MaterialRequestEvents__onHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();
            var Temp = DbContext.RequisitionLine.Where(m => m.RequisitionHeaderId == EventArgs.DocId).ToList();

            int Count = 0;
            if (Temp != null && Temp.Count > 0)
                Count = Temp.Count;

            var ReqHeader = (from p in DbContext.RequisitionHeader
                             where p.RequisitionHeaderId == EventArgs.DocId
                             select p
                            ).FirstOrDefault();

            DbContext.Dispose();


            if (ReqHeader.CostCenterId.HasValue)
            {

                var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                        where p.CostCenterId == ReqHeader.CostCenterId
                                        select p).FirstOrDefault();

                if (CostCenterStatus != null)
                {
                    CostCenterStatus.RequisitionProductCount = (CostCenterStatus.RequisitionProductCount ?? 0) - Count;
                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                }
            }

        }

        void MaterialRequestEvents__onLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var ReqHeader = (from p in DbContext.RequisitionHeader
                             where p.RequisitionHeaderId == EventArgs.DocId
                             select p
                            ).FirstOrDefault();

            DbContext.Dispose();

            if (ReqHeader.CostCenterId.HasValue)
            {
                var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                        where p.CostCenterId == ReqHeader.CostCenterId
                                        select p).FirstOrDefault();

                if (CostCenterStatus != null)
                {
                    CostCenterStatus.RequisitionProductCount = (CostCenterStatus.RequisitionProductCount ?? 0) - 1;
                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                }
            }

        }

        void MaterialRequestEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.RequisitionLine.Local.Where(m => m.RequisitionHeaderId == EventArgs.DocId).ToList();

            int Count = 0;
            if (Temp != null && Temp.Count > 0)
                Count = Temp.Count;

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var ReqHeader = (from p in DbContext.RequisitionHeader
                             where p.RequisitionHeaderId == EventArgs.DocId
                             select p
                            ).FirstOrDefault();

            DbContext.Dispose();


            if (ReqHeader.CostCenterId.HasValue)
            {

                var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                        where p.CostCenterId == ReqHeader.CostCenterId
                                        select p).FirstOrDefault();

                if (CostCenterStatus != null)
                {
                    CostCenterStatus.RequisitionProductCount = (CostCenterStatus.RequisitionProductCount ?? 0) + Count;
                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                }
            }

        }

        void MaterialRequestEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool MaterialRequestEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialRequestEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JobHeader = (from p in DbContext.RequisitionHeader
                             where p.RequisitionHeaderId == EventArgs.DocId
                             select p
                            ).FirstOrDefault();

            DbContext.Dispose();


            if (JobHeader.CostCenterId.HasValue && EventArgs.Mode == EventModeConstants.Add)
            {

                var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                        where p.CostCenterId == JobHeader.CostCenterId
                                        select p).FirstOrDefault();

                if (CostCenterStatus != null)
                {
                    CostCenterStatus.RequisitionProductCount = (CostCenterStatus.RequisitionProductCount ?? 0) + 1;
                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                }
            }


        }

        bool MaterialRequestEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialRequestEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void MaterialRequestEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var MaterialRequest = db.MaterialRequestHeader.Local.Where(m => m.MaterialRequestHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == MaterialRequest.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool MaterialRequestEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
