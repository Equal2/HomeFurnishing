using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Buyer : EntityBase
    {
        public Buyer()
        {
        }

        [Key]
        [ForeignKey("Person"), Display(Name = "Person")]
        public int PersonID { get; set; }
        public virtual Person Person { get; set; }

        //[ForeignKey("SaleOrderHeader")]
        public int? ExtraSaleOrderHeaderId { get; set; }
        //public virtual SaleOrderHeader SaleOrderHeader { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
