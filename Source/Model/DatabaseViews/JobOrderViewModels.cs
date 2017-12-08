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
    [Table("ViewJobOrderHeader")]
    public class ViewJobOrderHeader
    {
        [Key]
        public int JobOrderHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public DateTime DueDate { get; set; }
        public int JobWorkerId { get; set; }
        public int BillToPartyId { get; set; }
        public int OrderById { get; set; }
        public int GodownId { get; set; }
        public int JobInstructionId { get; set; }
        public string TermsAndConditions { get; set; }
        public int ProcessId { get; set; }
        public int ConstCenterId { get; set; }
        public int MachineId { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    
    [Table("ViewJobOrderLine")]
    public class ViewJobOrderLine
    {
        [Key]
        public int JobOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public decimal OrderQty { get; set; }
        public decimal Rate { get; set; }
        public decimal OrderAmount { get; set; }
        public int ProductId { get; set; }
        public string DeliveryUnitId { get; set; }
        public string Remark { get; set; }
    }

    [Table("ViewJobOrderBalance")]
    public class ViewJobOrderBalance : EntityBase
    {
        [Key]
        public int JobOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }


        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }
        public int ? ProductUidId { get; set; }

    }

    [Table("ViewJobOrderBalanceForInvoice")]
    public class ViewJobOrderBalanceForInvoice :EntityBase
    {
        [Key]
        public int JobOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("JobWorker"), Display(Name = "JobWorker")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        public DateTime OrderDate { get; set; }


        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }

    }


    [Table("ViewJobReceiveBalance")]
    public class ViewJobReceiveBalance :EntityBase
    {
        [Key]
        public int JobReceiveLineId { get; set; }

        [ForeignKey("JobOrderLine")]
        public int JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }
        public int JobOrderHeaderId { get; set; }
        public decimal BalanceQty { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public string JobReceiveNo { get; set; }

        public string JobOrderNo { get; set; }
                
        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }


        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int ? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }


        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int ? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }


        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }



        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }

        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }


                

    }

    [Table("ViewJobReceiveBalanceForInvoice")]
    public class ViewJobReceiveBalanceForInvoice : EntityBase
    {
        [Key]
        public int JobReceiveLineId { get; set; }

        [ForeignKey("JobOrderLine")]
        public int JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }
        public int JobOrderHeaderId { get; set; }
        public decimal BalanceQty { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public string JobReceiveNo { get; set; }

        public string JobOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }


        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }


        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }



        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }


        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }




        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }

        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }




    }

    [Table("ViewJobInvoiceBalanceForRateAmendment")]
    public class ViewJobInvoiceBalanceForRateAmendment
    {
        [Key]
        public int JobInvoiceLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int JobInvoiceHeaderId { get; set; }
        public string JobInvoiceNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("JobWorker"), Display(Name = "JobWorker")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }
        public DateTime InvoiceDate { get; set; }

        public int ? ProductUidId { get; set; }
        public string ProductUidName { get; set; }


        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

    }


    [Table("ViewJobOrderInspectionRequestBalance")]
    public class ViewJobOrderInspectionRequestBalance
    {
        [Key]
        public int JobOrderInspectionRequestLineId { get; set; }
        public int JobOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }       
        public int JobOrderInspectionRequestHeaderId { get; set; }
        public string JobOrderInspectionRequestNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int JobWorkerId { get; set; }
        public DateTime RequestDate { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }

        public int? ProductUidId { get; set; }

    }


    [Table("ViewJobOrderBalanceForInspectionRequest")]
    public class ViewJobOrderBalanceForInspectionRequest
    {
        [Key]
        public int JobOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }      
        public int JobOrderHeaderId { get; set; }
        public string JobOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }




        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }

        public int? ProductUidId { get; set; }

    }

    [Table("ViewJobOrderBalanceForInspection")]
    public class ViewJobOrderBalanceForInspection
    {
        [Key]
        public int JobOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }

        public int? ProductUidId { get; set; }

    }

    [Table("ViewJobOrderBalanceFromStatus")]
    public class ViewJobOrderBalanceFromStatus :EntityBase
    {
        [Key]
        public int JobOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }


        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }


        public int? ProductUidId { get; set; }

    }





    [Table("ViewJobReceiveBalanceForQA")]
    public class ViewJobReceiveBalanceForQA
    {
        [Key]
        public int JobReceiveLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string JobReceiveNo { get; set; }        
        public int ProductId { get; set; }
        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }
        public int? ProductUidId { get; set; }

    }

    [Table("ViewJobInvoiceBalance")]
    public class ViewJobInvoiceBalance
    {
        [Key]
        public int JobInvoiceLineId { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public int JobReceiveLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int JobInvoiceHeaderId { get; set; }
        public string JobInvoiceNo { get; set; }
        public int ProductId { get; set; }
        public int JobWorkerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int JobInvoiceDocTypeId { get; set; }
        public string JobReceiveNo { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }
}
