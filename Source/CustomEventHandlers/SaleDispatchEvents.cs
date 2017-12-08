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
using SaleDispatchDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class SaleDispatchEvents : SaleDispatchDocEvents
    {
        //For Subscribing Events
        public SaleDispatchEvents()
        {
            Initialized = true;
            _onHeaderSave += SaleDispatchEvents__onHeaderSave;
            _onLineSave += SaleDispatchEvents__onLineSave;
            _onLineSaveBulk += SaleDispatchEvents__onLineSaveBulk;
            _onLineDelete += SaleDispatchEvents__onLineDelete;
            _onHeaderDelete += SaleDispatchEvents__onHeaderDelete;
            _onHeaderSubmit += SaleDispatchEvents__onHeaderSubmit;
            _beforeLineSave += SaleDispatchEvents__beforeLineSaveDataValidation;
            _beforeLineDelete += SaleDispatchEvents__beforeLineDeleteDataValidation;
            _beforeLineSaveBulk += SaleDispatchEvents__beforeLineSaveBylkDataValidation;
            _afterHeaderSubmit += SaleDispatchEvents_afterHeaderSubmitEvent;

        }

        private void SaleDispatchEvents_afterHeaderSubmitEvent(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }


        void SaleDispatchEvents__onHeaderSubmit(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            int Id = EventArgs.DocId;

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();


                    using (SqlCommand cmd = new SqlCommand("Web.sp_CreatePurchaseOnBranch"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@SaleDispatchHeaderId", Id);
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

        bool SaleDispatchEvents__beforeLineSaveDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }

        bool SaleDispatchEvents__beforeLineDeleteDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        bool SaleDispatchEvents__beforeLineSaveBylkDataValidation(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return true;
        }


        void SaleDispatchEvents__onHeaderDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleDispatchEvents__onLineDelete(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleDispatchEvents__onLineSaveBulk(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }

        void SaleDispatchEvents__onLineSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }


        void SaleDispatchEvents__onHeaderSave(object sender, SaleEventArgs EventArgs, ref ApplicationDbContext db)
        {
        }
    }
}
