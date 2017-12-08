using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PersonProductUidViewModel
    {
        public int PersonProductUidId { get; set; }

        [Display(Name = "Product Uid Name")]
        [MaxLength(50), Required]
        public string ProductUidName { get; set; }

        [Display(Name = "Product Uid Specification")]
        public string ProductUidSpecification { get; set; }

        public int? GenDocId { get; set; }
        public string GenDocNo { get; set; }

        public int? GenDocTypeId { get; set; }
        public virtual DocumentType GenDocType { get; set; }

        public int? GenDocLineId { get; set; }

    }

    public class PersonProductUidSummaryViewModel
    {
        public List<PersonProductUidViewModel> JobInvoiceSummaryViewModel { get; set; }
        public int? GenDocLineId { get; set; }

    }

}
