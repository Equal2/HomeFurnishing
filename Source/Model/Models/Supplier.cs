using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Supplier : EntityBase
    {       

        public Supplier()
        {            
        }

        [Key]
        [ForeignKey("Person"), Display(Name = "Person")]
        public int PersonID { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("SalesTaxGroupParty"), Display(Name = "Sales Tax Group Party")]
        public int? SalesTaxGroupPartyId { get; set; }
        public virtual SalesTaxGroupParty SalesTaxGroupParty { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
