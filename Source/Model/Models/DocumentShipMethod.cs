using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class DocumentShipMethod : EntityBase, IHistoryLog
    {
        //public DocumentShipMethod()
        //{
        //    //Products = new List<Product>();
        //}

        [Key]
        public int DocumentShipMethodId { get; set; }
        [Display(Name = "Document Ship Method")]
        [MaxLength(50, ErrorMessage = "DocumentShipMethod Name cannot exceed 50 characters"), Required]
        [Index("IX_DocumentShipMethod_DocumentShipMethodName", IsUnique = true)]
        public string DocumentShipMethodName { get; set; }        

        //public ICollection<Product> Products { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
     
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
