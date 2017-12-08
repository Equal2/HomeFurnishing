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
using JobInvoiceReceiveDocumentEvents;

namespace Jobs.Controllers
{


    public class JobInvoiceReceiveEvents : JobInvoiceReceiveDocEvents
    {
        //For Subscribing Events
        public JobInvoiceReceiveEvents()
        {
            Initialized = true;           
        }       

    }
}
