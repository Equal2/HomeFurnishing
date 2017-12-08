using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleEnquiryLineExtended : EntityBase
    {
        [Key]
        [ForeignKey("SaleEnquiryLine")]
        public int SaleEnquiryLineId { get; set; }
        public SaleEnquiryLine SaleEnquiryLine { get; set; }
        public string BuyerSpecification { get; set; }
        public string BuyerSpecification1 { get; set; }
        public string BuyerSpecification2 { get; set; }
        public string BuyerSpecification3 { get; set; }
        public string BuyerSku { get; set; }
        public string BuyerUpcCode { get; set; }
    }
}
