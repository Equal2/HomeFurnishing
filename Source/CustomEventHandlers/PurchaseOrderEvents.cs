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
using PurchaseOrderDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class PurchaseOrderEvents : PurchaseOrderDocEvents
    {
        //For Subscribing Events
        public PurchaseOrderEvents()
        {
            Initialized = true;
        }

        void PurchaseOrderEvents__afterHeaderSubmit(object sender, PurchaseEventArgs EventArgs, ref ApplicationDbContext db)
        {           

        }
    }
}
