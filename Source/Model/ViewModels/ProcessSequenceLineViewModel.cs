using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class ProcessSequenceLineViewModel
    {
        [Key]
        public int ProcessSequenceLineId { get; set; }

        [ForeignKey("ProcessSequenceHeader")]
        public int ProcessSequenceHeaderId { get; set; }
        public virtual ProcessSequenceHeader ProcessSequenceHeader { get; set; }

        [ForeignKey("Process"), Display(Name = "Process"),Required(ErrorMessage="Please select a Process")]
        public int  ProcessId { get; set; }
        public virtual Process Process { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Sequence"), Required]
        public int Sequence { get; set; }

        [Display(Name = "Days"), Required]
        public int Days { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
        public int? RefDocId { get; set; }
        public int? RefDocTypeId { get; set; }
        public int? ProductRateGroupId { get; set; }
    }

    public class ProcessSequenceLineIndexViewModel : ProcessSequenceLineViewModel
    {
        public string ProcessSequenceHeaderName { get; set; }

    }
}
