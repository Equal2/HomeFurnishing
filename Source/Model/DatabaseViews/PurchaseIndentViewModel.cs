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
    [Table("ViewPurchaseIndentHeader")]
    public class ViewPurchaseIndentHeader
    {
        [Key]
        public int PurchaseIndentHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal TotalQty { get; set; }
    }
    
    
    [Table("ViewPurchaseIndentLine")]
    public class ViewPurchaseIndentLine
    {
        [Key]
        public int PurchaseIndentLineId { get; set; }
        public int PurchaseIndentHeaderId { get; set; }
        public decimal Qty { get; set; }
        public int ProductId { get; set; }
        public string Remark { get; set; }
    }

    [Table("ViewPurchaseIndentBalance")]
    public class ViewPurchaseIndentBalance
    {
        [Key]
        public int PurchaseIndentLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int PurchaseIndentHeaderId { get; set; }
        public string PurchaseIndentNo { get; set; }
        public int ProductId { get; set; }
        public DateTime IndentDate { get; set; }
        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
        public string DocTypeName { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }
}
