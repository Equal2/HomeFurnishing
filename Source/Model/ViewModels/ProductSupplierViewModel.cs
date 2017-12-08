
namespace Model.ViewModel
{
    public class ProductSupplierViewModel
    {
        public int ProductSupplierId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int? LeadTime { get; set; }
        public decimal? MinimumOrderQty { get; set; }
        public decimal? MaximumOrderQty { get; set; }
        public decimal? Cost { get; set; }
        public bool Default { get; set; }

    }
}
