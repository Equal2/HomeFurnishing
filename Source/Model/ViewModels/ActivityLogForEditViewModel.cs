using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Linq;

namespace Model.ViewModel
{
    public class ActivityLogForEditViewModel
    {
        public int DocId { get; set; }
        public int DocTypeId { get; set; }
        public string DocNo { get; set; }
        [Required, MinLength(20, ErrorMessage = "UserRemark must be a minimum of 20 characters")]
        public string UserRemark { get; set; }

    }

    public class ActiivtyLogViewModel
    {
        public int ActivityType { get; set; }
        public string ActivityTypeName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ? CreatedDate { get; set; }
        public string Modifications { get; set; }
        public string UserRemark { get; set; }
        public int ActivityLogId { get; set; }
        public int DocId { get; set; }
        public string DocNo { get; set; }
        public DateTime ? DocDate { get; set; }
        public XmlDocument XmlModifications { get; set; }
        public XElement xEModifications { get; set; }
        public string Narration { get; set; }
        public int ? DocLineId { get; set; }
        public int DocTypeId { get; set; }
        public string User { get; set; }
        public int DocStatus { get; set; }
        public int SessionId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }

        //Explicit Cast Operations
        public static explicit operator ActiivtyLogViewModel(JobOrderHeader Jh)
        {
            ActiivtyLogViewModel vm=new ActiivtyLogViewModel();

            vm.DocDate=Jh.DocDate;
            vm.DocId=Jh.JobOrderHeaderId;
            vm.DocNo=Jh.DocNo;
            
            return vm;
        }

        public ActiivtyLogViewModel Map(ActiivtyLogViewModel nvm)
        {
            this.ActivityType = nvm.ActivityType;
            this.DocTypeId = nvm.DocTypeId;
            this.DocId = nvm.DocId;
            this.DocLineId = nvm.DocLineId;
            this.DocNo = nvm.DocNo;
            this.DocDate = nvm.DocDate;
            this.DocStatus = nvm.DocStatus;
            this.xEModifications = nvm.xEModifications;
            this.UserRemark = nvm.UserRemark;

            return this;
        }
    }

}
