using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Process : EntityBase, IHistoryLog
    {
        public Process()
        {
        }

        [Key]
        public int ProcessId { get; set; }

        [Display(Name = "Code")]
        [MaxLength(50, ErrorMessage = "Process code cannot exceed 50 characters"), Required]
        [Index("IX_Process_ProcessCode", IsUnique = true)]
        public string ProcessCode { get; set; }        

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Process Name cannot exceed 50 characters"), Required]
        [Index("IX_Process_ProcessName", IsUnique = true)]
        public string ProcessName { get; set; }

        [ForeignKey("ParentProcess"), Display(Name = "Parent Process")]
        public int? ParentProcessId { get; set; }
        public virtual Process ParentProcess { get; set; }

        [ForeignKey("Account"), Display(Name = "Account")]
        public int AccountId { get; set; }
        public virtual LedgerAccount Account { get; set; }

        [Display(Name = "Is Affected Stock ?")]
        public Boolean IsAffectedStock { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        public int? ProcessSr { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
