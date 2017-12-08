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
using GatePassDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class GatePassEvents : GatePassDocEvents
    {
        //For Subscribing Events
        public GatePassEvents()
        {
            Initialized = true;
            _onHeaderSave += GatePassEvents__onHeaderSave;
            _onLineSave += GatePassEvents__onLineSave;
            _onLineSaveBulk += GatePassEvents__onLineSaveBulk;
            _onLineDelete += GatePassEvents__onLineDelete;
            _onHeaderDelete += GatePassEvents__onHeaderDelete;
            _onHeaderSubmit += GatePassEvents__onHeaderSubmit;
            _beforeLineSave += GatePassEvents__beforeLineSaveDataValidation;
            _beforeLineDelete += GatePassEvents__beforeLineDeleteDataValidation;
            _beforeLineSaveBulk += GatePassEvents__beforeLineSaveBylkDataValidation;

        }


        void GatePassEvents__onHeaderSubmit(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

            int Id = EventArgs.DocId;

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            try
            {

            }

            catch (Exception ex)
            {
                //Header.Status = (int)StatusConstants.Drafted;
                //new JobReceiveHeaderService(_unitOfWork).Update(Header);
                //_unitOfWork.Save();
                throw ex;
            }

            // return Redirect(ReturnUrl);


        }

        bool GatePassEvents__beforeLineSaveDataValidation(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

            return true;
        }

        bool GatePassEvents__beforeLineDeleteDataValidation(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {


            return true;
        }


        bool GatePassEvents__beforeLineSaveBylkDataValidation(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

            return true;
        }


        void GatePassEvents__onHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            
        }

        void GatePassEvents__onLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

        }

        void GatePassEvents__onLineSaveBulk(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

        }

        void GatePassEvents__onLineSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

        }


        void GatePassEvents__onHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

        }

    }
}
