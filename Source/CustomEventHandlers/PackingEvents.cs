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
using PackingDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class PackingEvents : PackingDocEvents
    {
        //For Subscribing Events
        public PackingEvents()
        {
            Initialized = true;
            _onHeaderSave += PackingEvents__onHeaderSave;
            _onLineSave += PackingEvents__onLineSave;
            _onLineSaveBulk += PackingEvents__onLineSaveBulk;
            _onLineDelete += PackingEvents__onLineDelete;
            _onHeaderDelete += PackingEvents__onHeaderDelete;
            _onHeaderSubmit += PackingEvents__onHeaderSubmit;
            _beforeLineSave += PackingEvents__beforeLineSaveDataValidation;
            _beforeLineDelete += PackingEvents__beforeLineDeleteDataValidation;
            _beforeLineSaveBulk += PackingEvents__beforeLineSaveBylkDataValidation;
            _afterHeaderSubmit += PackingEvents_afterHeaderSubmitEvent;

        }

        private void PackingEvents_afterHeaderSubmitEvent(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }


        void PackingEvents__onHeaderSubmit(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        bool PackingEvents__beforeLineSaveDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }

        bool PackingEvents__beforeLineDeleteDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        bool PackingEvents__beforeLineSaveBylkDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        void PackingEvents__onHeaderDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void PackingEvents__onLineDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void PackingEvents__onLineSaveBulk(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void PackingEvents__onLineSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }


        void PackingEvents__onHeaderSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }
    }
}
