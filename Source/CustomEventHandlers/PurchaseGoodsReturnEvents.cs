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
using PurchaseGoodsReturnDocumentEvents;

namespace Jobs.Controllers
{


    public class PurchaseGoodsReturnEvents : PurchaseGoodsReturnDocEvents
    {
        //For Subscribing Events
        public PurchaseGoodsReturnEvents()
        {
            Initialized = true;
        }

    }
}
