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
using JobOrderAmendmentDocumentEvents;

namespace Jobs.Controllers
{


    public class JobOrderAmendmentEvents : JobOrderAmendmentDocEvents
    {
        //For Subscribing Events
        public JobOrderAmendmentEvents()
        {
            Initialized = true;           
        } 
    }
}
