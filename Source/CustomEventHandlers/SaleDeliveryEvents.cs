using Core.Common;
using CustomEventArgs;
using Data.Models;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentEvents;
using SaleDeliveryDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class SaleDeliveryEvents : SaleDeliveryDocEvents
    {
        //For Subscribing Events
        public SaleDeliveryEvents()
        {
            Initialized = true;
            _onHeaderSave += SaleDeliveryEvents__onHeaderSave;
            _onLineSave += SaleDeliveryEvents__onLineSave;
            _onLineSaveBulk += SaleDeliveryEvents__onLineSaveBulk;
            _onLineDelete += SaleDeliveryEvents__onLineDelete;
            _onHeaderDelete += SaleDeliveryEvents__onHeaderDelete;
            _onHeaderSubmit += SaleDeliveryEvents__onHeaderSubmit;
            _beforeLineSave += SaleDeliveryEvents__beforeLineSaveDataValidation;
            _beforeLineDelete += SaleDeliveryEvents__beforeLineDeleteDataValidation;
            _beforeLineSaveBulk += SaleDeliveryEvents__beforeLineSaveBylkDataValidation;
            _afterHeaderSubmit += SaleDeliveryEvents_afterHeaderSubmitEvent;

        }

        private void SaleDeliveryEvents_afterHeaderSubmitEvent(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }


        void SaleDeliveryEvents__onHeaderSubmit(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        bool SaleDeliveryEvents__beforeLineSaveDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }

        bool SaleDeliveryEvents__beforeLineDeleteDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        bool SaleDeliveryEvents__beforeLineSaveBylkDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        void SaleDeliveryEvents__onHeaderDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleDeliveryEvents__onLineDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleDeliveryEvents__onLineSaveBulk(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleDeliveryEvents__onLineSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }


        void SaleDeliveryEvents__onHeaderSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }
    }
}
