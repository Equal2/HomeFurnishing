using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    public class  GatePassHeaderViewModel
    {
        public int GatePassHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        public int PersonId { get; set; }

        public int ? OrderById { get; set; }
        public string Name { get; set; }
        public int? GodownId { get; set; }
        public string GodownName { get; set; }
        public int Status { get; set; }
        public string Product { get; set;}

         public decimal Qty { get; set; }
        public decimal? TotalQty { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string Remark { get; set; }

        public string ModifiedBy { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? DecimalPlaces { get; set; }
        public int? ReferenceDocId { get; set; }
    }

    public class GatePassHeaderViewModel1
    {
        public int GatePassHeaderId { get; set; }
        public string Name { get; set; }
        public string DocNo { get; set; }
        public string Remark { get; set; }
        public DateTime DocDate { get; set; }
        public int Status { get; set; }
        public string Product { get; set; }
        public decimal Qty { get; set; }
        public string UnitId { get; set; }

    }
    public class GatePassLineViewModel
    {

        public int GatePassLineId { get; set; }         
	   public int GatePassHeaderId { get; set; }
       public string Product { get; set; }
	   public string Specification { get; set; }
	 
         

        public decimal Qty { get; set; }
        public string UnitId { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
         
	public string OMSId { get; set; }
	public int  OverTimeApplicationHeader_OverTimeApplicationId {get;set;}
       // public decimal ? DecimalPlaces { get; set; }
        public int DecimalPlaces { get; set; }
        public string UnitName { get; set; }
    }

    }
