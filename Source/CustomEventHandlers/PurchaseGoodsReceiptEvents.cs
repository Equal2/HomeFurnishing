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
using PurchaseGoodsReceiptDocumentEvents;

namespace Jobs.Controllers
{


    public class PurchaseGoodsReceiptEvents : PurchaseGoodsReceiptDocEvents
    {
        //For Subscribing Events
        public PurchaseGoodsReceiptEvents()
        {
            Initialized = true;
        }

    }
}
