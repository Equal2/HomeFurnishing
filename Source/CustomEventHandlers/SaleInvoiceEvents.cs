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
using SaleInvoiceDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class SaleInvoiceEvents : SaleInvoiceDocEvents
    {
        //For Subscribing Events
        public SaleInvoiceEvents()
        {
            Initialized = true;
            _onHeaderSave += SaleInvoiceEvents__onHeaderSave;
            _onLineSave += SaleInvoiceEvents__onLineSave;
            _onLineSaveBulk += SaleInvoiceEvents__onLineSaveBulk;
            _onLineDelete += SaleInvoiceEvents__onLineDelete;
            _onHeaderDelete += SaleInvoiceEvents__onHeaderDelete;
            _onHeaderSubmit += SaleInvoiceEvents__onHeaderSubmit;
            _beforeLineSave += SaleInvoiceEvents__beforeLineSaveDataValidation;
            _beforeLineDelete += SaleInvoiceEvents__beforeLineDeleteDataValidation;
            _beforeLineSaveBulk += SaleInvoiceEvents__beforeLineSaveBylkDataValidation;
            _afterHeaderSubmit += SaleInvoiceEvents_afterHeaderSubmitEvent;

        }

        private void SaleInvoiceEvents_afterHeaderSubmitEvent(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }


        void SaleInvoiceEvents__onHeaderSubmit(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            int Id = EventArgs.DocId;

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            SaleInvoiceHeader Header = db.SaleInvoiceHeader.Find(Id);
            string DocumentTypeName = db.DocumentType.Find(Header.DocTypeId).DocumentTypeName;

            if (DocumentTypeName == "Weaving Cloth Sale Invoice")
            {
                try
                {
                    using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                    {
                        sqlConnection.Open();


                        using (SqlCommand cmd = new SqlCommand("Web.sp_CreatePurchaseOnBranch"))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = sqlConnection;
                            cmd.Parameters.AddWithValue("@SaleInvoiceHeaderId", Id);
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }

                    }
                }

                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        bool SaleInvoiceEvents__beforeLineSaveDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }

        bool SaleInvoiceEvents__beforeLineDeleteDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        bool SaleInvoiceEvents__beforeLineSaveBylkDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        void SaleInvoiceEvents__onHeaderDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleInvoiceEvents__onLineDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleInvoiceEvents__onLineSaveBulk(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleInvoiceEvents__onLineSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }


        void SaleInvoiceEvents__onHeaderSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }
    }
}
