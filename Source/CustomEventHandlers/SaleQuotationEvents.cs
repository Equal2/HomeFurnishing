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
using SaleQuotationDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class SaleQuotationEvents : SaleQuotationDocEvents
    {
        //For Subscribing Events
        public SaleQuotationEvents()
        {
            Initialized = true;
            _onHeaderSave += SaleQuotationEvents__onHeaderSave;
            _onLineSave += SaleQuotationEvents__onLineSave;
            _onLineSaveBulk += SaleQuotationEvents__onLineSaveBulk;
            _onLineDelete += SaleQuotationEvents__onLineDelete;
            _onHeaderDelete += SaleQuotationEvents__onHeaderDelete;
            _onHeaderSubmit += SaleQuotationEvents__onHeaderSubmit;
            _beforeLineSave += SaleQuotationEvents__beforeLineSaveDataValidation;
            _beforeLineDelete += SaleQuotationEvents__beforeLineDeleteDataValidation;
            _beforeLineSaveBulk += SaleQuotationEvents__beforeLineSaveBylkDataValidation;

        }


        void SaleQuotationEvents__onHeaderSubmit(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        bool SaleQuotationEvents__beforeLineSaveDataValidation(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {


            return true;
        }

        bool SaleQuotationEvents__beforeLineDeleteDataValidation(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {


            return true;
        }


        bool SaleQuotationEvents__beforeLineSaveBylkDataValidation(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {



            return true;
        }


        void SaleQuotationEvents__onHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

        }

        void SaleQuotationEvents__onLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

        }

        void SaleQuotationEvents__onLineSaveBulk(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        { 
        }

        void SaleQuotationEvents__onLineSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }


        void SaleQuotationEvents__onHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

        }

    }
}
