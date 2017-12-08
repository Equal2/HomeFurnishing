using CustomEventArgs;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateConversionDocumentEvents
{
    public delegate bool DocumentEventHandlerWithReturn(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db);
    public delegate void DocumentEventHandler(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db);
    public class RateConversionDocEvents
    {       

              //HeaderSave Events
        protected static event DocumentEventHandlerWithReturn _beforeHeaderSave;
        protected static event DocumentEventHandler _onHeaderSave;
        protected static event DocumentEventHandler _afterHeaderSave;

        //Header Delete
        protected static event DocumentEventHandlerWithReturn _beforeHeaderDelete;
        protected static event DocumentEventHandler _onHeaderDelete;
        protected static event DocumentEventHandler _afterHeaderDelete;

        //Header Submit
        protected static event DocumentEventHandlerWithReturn _beforeHeaderSubmit;
        protected static event DocumentEventHandler _onHeaderSubmit;
        protected static event DocumentEventHandler _afterHeaderSubmit;

        //Header Review
        protected static event DocumentEventHandlerWithReturn _beforeHeaderReview;
        protected static event DocumentEventHandler _onHeaderReview;
        protected static event DocumentEventHandler _afterHeaderReview;

        //Header Print
        protected static event DocumentEventHandlerWithReturn _beforeHeaderPrint;
        protected static event DocumentEventHandler _afterHeaderPrint;

        //Line Save
        protected static event DocumentEventHandlerWithReturn _beforeLineSave;
        protected static event DocumentEventHandler _onLineSave;
        protected static event DocumentEventHandler _afterLineSave;

        //Line Save Bulk
        protected static event DocumentEventHandlerWithReturn _beforeLineSaveBulk;
        protected static event DocumentEventHandler _onLineSaveBulk;
        protected static event DocumentEventHandler _afterLineSaveBulk;

        //Line Delete
        protected static event DocumentEventHandlerWithReturn _beforeLineDelete;
        protected static event DocumentEventHandler _onLineDelete;
        protected static event DocumentEventHandler _afterLineDelete;
        public static bool Initialized { get; set; }

        #region Methods to Raise Events from Controller
        //Public methods to call where ever we intend to raise out custom events
        //Calling this method with the arguments will raise the appropriate event and all the subscibers of this event will be noified to execute event data

        public static bool beforeHeaderSaveEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeHeaderSave != null)
                return _beforeHeaderSave(sender, Args, ref db);
            else return true;
        }

        public static void onHeaderSaveEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_onHeaderSave != null)
                _onHeaderSave(sender, Args, ref db);
        }

        public static void afterHeaderSaveEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterHeaderSave != null)
                _afterHeaderSave(sender, Args, ref db);
        }

        public static bool beforeHeaderDeleteEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeHeaderDelete != null)
                return _beforeHeaderDelete(sender, Args, ref db);
            else return true;
        }

        public static void onHeaderDeleteEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_onHeaderDelete != null)
                _onHeaderDelete(sender, Args, ref db);
        }

        public static void afterHeaderDeleteEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterHeaderDelete != null)
                _afterHeaderDelete(sender, Args, ref db);
        }

        public static bool beforeHeaderSubmitEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeHeaderSubmit != null)
                return _beforeHeaderSubmit(sender, Args, ref db);
            else return true;
        }

        public static void onHeaderSubmitEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_onHeaderSubmit != null)
                _onHeaderSubmit(sender, Args, ref db);
        }

        public static void afterHeaderSubmitEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterHeaderSubmit != null)
                _afterHeaderSubmit(sender, Args, ref db);
        }

        public static bool beforeHeaderReviewEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeHeaderReview != null)
                return _beforeHeaderReview(sender, Args, ref db);
            else return true;
        }

        public static void onHeaderReviewEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_onHeaderReview != null)
                _onHeaderReview(sender, Args, ref db);
        }

        public static void afterHeaderReviewEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterHeaderReview != null)
                _afterHeaderReview(sender, Args, ref db);
        }


        public static bool beforeHeaderPrintEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeHeaderPrint != null)
                return _beforeHeaderPrint(sender, Args, ref db);
            else return true;
        }

        public static void afterHeaderPrintEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterHeaderPrint != null)
                _afterHeaderPrint(sender, Args, ref db);
        }

        public static bool beforeLineSaveEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeLineSave != null)
                return _beforeLineSave(sender, Args, ref db);
            else return true;
        }

        public static void onLineSaveEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_onLineSave != null)
                _onLineSave(sender, Args, ref db);
        }

        public static void afterLineSaveEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterLineSave != null)
                _afterLineSave(sender, Args, ref db);
        }

        public static bool beforeLineSaveBulkEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeLineSaveBulk != null)
                return _beforeLineSaveBulk(sender, Args, ref db);
            else return true;
        }

        public static void onLineSaveBulkEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_onLineSaveBulk != null)
                _onLineSaveBulk(sender, Args, ref db);
        }

        public static void afterLineSaveBulkEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterLineSaveBulk != null)
                _afterLineSaveBulk(sender, Args, ref db);
        }


        public static bool beforeLineDeleteEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_beforeLineDelete != null)
                return _beforeLineDelete(sender, Args, ref db);
            else return true;
        }

        public static void onLineDeleteEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_onLineDelete != null)
                _onLineDelete(sender, Args, ref db);
        }

        public static void afterLineDeleteEvent(object sender, StockEventArgs Args, ref ApplicationDbContext db)
        {
            if (_afterLineDelete != null)
                _afterLineDelete(sender, Args, ref db);
        }

        #endregion
    }    

}
