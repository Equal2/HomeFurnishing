using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class ProductUid : EntityBase, IHistoryLog
    {
        [Key]
        public int ProductUIDId { get; set; }

        [Display(Name="Product Uid Name")]
        [MaxLength(50), Required]
        [Index("IX_ProductUid_ProductUidName", IsUnique = true)]
        public string ProductUidName { get; set; }

        [Display(Name = "Product UID Header")]
        [ForeignKey("ProductUidHeader")]
        public int? ProductUidHeaderId { get; set; }
        public virtual ProductUidHeader ProductUidHeader { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [Display(Name = "Lot No"), MaxLength(50)]
        public string LotNo { get; set; }

        //Update Fields
        public int? GenDocId { get; set; }
        public int? GenLineId { get; set; }

        public string GenDocNo { get; set; }

        [ForeignKey("GenDocType"), Display(Name = "Document Type")]
        public int? GenDocTypeId { get; set; }
        public virtual DocumentType GenDocType { get; set; }

        public DateTime? GenDocDate { get; set; }

        [ForeignKey("GenPerson"), Display(Name = "Gen Person")]
        public int? GenPersonId { get; set; }
        public virtual Person GenPerson { get; set; }

        public int? LastTransactionDocId { get; set; }

        public int? LastTransactionLineId { get; set; }

        public string LastTransactionDocNo { get; set; }

        [ForeignKey("LastTransactionDocType"), Display(Name = "Document Type")]
        public int? LastTransactionDocTypeId { get; set; }
        public virtual DocumentType LastTransactionDocType { get; set; }

        public DateTime? LastTransactionDocDate { get; set; }

        [ForeignKey("LastTransactionPerson"), Display(Name = "Last Transaction Person")]
        public int? LastTransactionPersonId { get; set; }
        public virtual Person LastTransactionPerson { get; set; }

        [ForeignKey("CurrenctGodown"), Display(Name = "Current Godowny")]
        public int? CurrenctGodownId { get; set; }
        public virtual Godown CurrenctGodown { get; set; }
        
        [ForeignKey("CurrenctProcess"), Display(Name = "Current Processy")]
        public int? CurrenctProcessId { get; set; }
        public virtual Process CurrenctProcess { get; set; }

        [MaxLength(10)]
        public string Status { get; set; }
        //End Update Fields

        public string ProcessesDone { get; set; }
        

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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

    public interface IProductUidLastStatus
    {
        int? ProductUidLastTransactionDocId { get; set; }

        string ProductUidLastTransactionDocNo { get; set; }

        int? ProductUidLastTransactionDocTypeId { get; set; }

        DateTime? ProductUidLastTransactionDocDate { get; set; }

        int? ProductUidLastTransactionPersonId { get; set; }

        int? ProductUidCurrentGodownId { get; set; }

        int? ProductUidCurrentProcessId { get; set; }

        [MaxLength(10)]
        string ProductUidStatus { get; set; }

    }
}


