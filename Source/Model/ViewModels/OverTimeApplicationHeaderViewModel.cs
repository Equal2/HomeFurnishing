using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class OverTimeApplicationHeaderViewModel
    {

        public int OverTimeApplicationId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? SiteId { get; set; }
        public int DivisionId { get; set; }
        public int PersonId { get; set; }
        public string PersonId1 { get; set; }
        public string Name { get; set; }

        //public int? GodownId { get; set; }
        public int GodownId { get; set; }
        
        public string GodownName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ReferenceDocTypeId { get; set; }
        public int? ReferenceDocId { get; set; }
        public string ReferenceDocNo { get; set; }
        public string OMSId { get; set; }

        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }


    }
}
