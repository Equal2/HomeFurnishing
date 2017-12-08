using System;
using System.ComponentModel.DataAnnotations;

namespace Models.BasicSetup.ViewModels
{
    public class ProductUidViewModel
    {
        public int ProductUIDId { get; set; }

        [Display(Name = "Product Uid Name")]
        [MaxLength(50), Required]
        public string ProductUidName { get; set; }

        [Display(Name = "Product"), Required]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

    }

    public class UIDValidationViewModel
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }

        public int ProductUIDId { get; set; }

        [Display(Name = "Product Uid Name")]
        [MaxLength(50), Required]
        public string ProductUidName { get; set; }

        [Display(Name = "Product"), Required]
        public int ProductId { get; set; }
        public string ProductName { get; set; }


        //Update Fields
        public int? GenDocId { get; set; }
        public int? GenLineId { get; set; }
        public string GenDocNo { get; set; }

        [Display(Name = "Document Type")]
        public int? GenDocTypeId { get; set; }
        public string GenDocTypeName { get; set; }

        public DateTime? GenDocDate { get; set; }

        [ Display(Name = "Gen Person")]
        public int? GenPersonId { get; set; }
        public string GenPersonName { get; set; }

        public int? LastTransactionDocId { get; set; }

        public string LastTransactionDocNo { get; set; }

        public int? LastTransactionDocLineId { get; set; }

        [Display(Name = "Document Type")]
        public int? LastTransactionDocTypeId { get; set; }
        public string LastTransactionDocTypeName { get; set; }

        public DateTime? LastTransactionDocDate { get; set; }

        [Display(Name = "Last Transaction Person")]
        public int? LastTransactionPersonId { get; set; }
        public string LastTransactionPersonName { get; set; }

        [Display(Name = "Current Godown")]
        public int? CurrenctGodownId { get; set; }
        public string CurrentGodownName { get; set; }

        [Display(Name = "Current Processy")]
        public int? CurrenctProcessId { get; set; }
        public string CurrentProcessName { get; set; }

        [MaxLength(10)]
        public string Status { get; set; }


        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Dimension1")]        
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]        
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public string LotNo { get; set; }
        public bool? Branch { get; set; }
        public int? ProductUidHeaderId { get; set; }
        public bool Validation { get; set; }
    }

    public class ProductUidTransactionDetail
    {
        public int DocLineId { get; set; }
        public string DocNo { get; set; }
    }


    public class ProductUidHeaderIndexViewModel 
    {

        [Key]
        public int ProductUidHeaderId { get; set; }

        [Display(Name = "Product"), Required]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        public int? DivisionId { get; set; }

        public int? SiteId { get; set; }


        [Display(Name = "Lot No"), MaxLength(50)]
        public string LotNo { get; set; }

        //Update Fields
        public int? GenDocId { get; set; }

        public string GenDocNo { get; set; }

        public int GenDocTypeId { get; set; }
        public string GenDocTypeName { get; set; }

        public DateTime GenDocDate { get; set; }

        public int? GenPersonId { get; set; }
        public string GenPersonName { get; set; }


        

        [Display(Name = "Godown")]
        public int? GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Process")]
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        public int Qty { get; set; }

        [Display(Name = "Remark")]
        public string GenRemark { get; set; }

        [Display(Name = "Product Uids")]
        public string ProductUids { get; set; }


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

        public string PrintProductUidHeaderIds { get; set; }
    }

}
