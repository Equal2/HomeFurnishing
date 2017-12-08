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
using JobInvoiceAmendmentDocumentEvents;

namespace Jobs.Controllers
{


    public class JobInvoiceAmendmentEvents : JobInvoiceAmendmentDocEvents
    {
        //For Subscribing Events
        public JobInvoiceAmendmentEvents()
        {
            Initialized = true;           
        } 
    }
}
