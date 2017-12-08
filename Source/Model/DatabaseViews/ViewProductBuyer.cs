using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{

    [Table("ViewProductBuyer")]
    public class ViewProductBuyer
    {
        [Key]
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string BuyerSku { get; set; }
        public string BuyerSpecification { get; set; }
        public string BuyerSpecification1 { get; set; }
        public string BuyerSpecification2 { get; set; }
        public string BuyerSpecification3 { get; set; }
        public int BuyerId { get; set; }
        
    }
}
