using System.ComponentModel.DataAnnotations;
namespace Model.ViewModel
{
    public class MultipleDocumentPrint
    {

        [Display(Name = "Document Type")]
        public string DocumentTypeId { get; set; }

        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        [Display(Name = "Sale Order")]
        public string SaleOrder { get; set; }

        [Display(Name = "Sale Order")]
        public string SaleOrderHeaderId { get; set; }
    }
}
