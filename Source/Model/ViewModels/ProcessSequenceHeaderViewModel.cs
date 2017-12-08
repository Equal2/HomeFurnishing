using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class ProcessSequenceHeaderIndexViewModel 
    {
        [Key]
        public int ProcessSequenceHeaderId { get; set; }

        [Display(Name = "Sequence Name"), MaxLength(50),Required(ErrorMessage="The Squence Name Field is Required")]
        public string ProcessSequenceHeaderName { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
    }
    public class ProcessSequenceHeaderIndexViewModelForEdit : ProcessSequenceHeaderIndexViewModel
    {
        [Required,MinLength(20)]
        public string LogReason { get; set; }
    }

    public class ProcessSequenceMasterDetailModel
    {
        public int ProcessSequenceHeaderId { get; set; }
        public ProcessSequenceHeaderIndexViewModel ProcessSequenceHeaderIndexViewModel { get; set; }
        public ProcessSequenceHeaderIndexViewModelForEdit ProcessSequenceHeaderIndexViewModelForEdit { get; set; } 
        public List<ProcessSequenceLineIndexViewModel> ProcessSequenceLineIndexViewModel { get; set; }
    }
}
