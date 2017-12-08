using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class GateInViewModel
    {
        public int GateInId { get; set; }

        [Display(Name = "Gate In Type"), Required]
        [ForeignKey("DocType")]        
        public int DocTypeId { get; set; }
        public string DocTypeName{get;set;}

        [DataType(DataType.Date)]        
        [Display(Name = "Gate In Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Gate In No"), Required, MaxLength(20)]        
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]        
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; }

        [Display(Name = "Vehicla No")]
        [MaxLength(20)]
        public string VehicleNo { get; set; }

        [Display(Name = "Transporter")]
        [MaxLength(250)]
        public string Transporter { get; set; }

        [Display(Name = "Vehicle Gross Weight")]
        public Decimal VehicleGrossWeight { get; set; }

        [Display(Name = "Vehicle Tare Weight")]
        public Decimal VehicleTareWeight { get; set; }

        [Display(Name = "Driver Name")]
        [MaxLength(250)]
        public string DiverName { get; set; }

        [Display(Name = "No Of Packages")]
        public Decimal NoOfPackages { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

    }

    public class GateInListViewModel
    {
        public int GateInId { get; set; }
        public string DocNo { get; set; }
    }
}
