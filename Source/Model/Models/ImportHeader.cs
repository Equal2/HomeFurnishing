using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ImportHeader : EntityBase, IHistoryLog
    {
        public ImportHeader()
        {
            ImportLines = new List<ImportLine>();
        }
        [Key]
        public int ImportHeaderId { get; set; }
        public string ImportName { get; set; }
        public string SqlProc { get; set; }
        public string Notes { get; set; }
        public string FileType { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public ICollection<ImportLine> ImportLines { get; set; }

    }
}
