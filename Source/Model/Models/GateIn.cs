using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class GateIn : EntityBase, IHistoryLog
    {
        public GateIn()
        {
        }

        [Key]
        public int GateInId { get; set; }

        [Display(Name = "Gate In Type"), Required]
        [ForeignKey("DocType")]
        [Index("IX_GateIn_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Gate In Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Gate In No"), Required, MaxLength(20)]
        [Index("IX_GateIn_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_GateIn_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_GateIn_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

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

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
