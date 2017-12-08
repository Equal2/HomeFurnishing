using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    public class QAGroupLineLineViewModel
    {      
        public int QAGroupLineId { get; set; }

        public int? JobReceiveQAAttributeId { get; set; }

        public int QAGroupId { get; set; }
        public string Name { get; set; }

        public bool IsMandatory { get; set; }

        public string DataType { get; set; }

        public string ListItem { get; set; }

        public string DefaultValue { get; set; }

        public string Value { get; set; }

        public string Remarks { get; set; }

        public bool IsActive { get; set; }


        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string OMSId { get; set; }

    }
}
