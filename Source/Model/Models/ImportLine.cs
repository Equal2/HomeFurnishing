using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ImportLine : EntityBase, IHistoryLog
    {
        [Key]
        public int ImportLineId { get; set; }
        [ForeignKey("ImportHeader")]
        public int ImportHeaderId { get; set; }
        public virtual ImportHeader ImportHeader { get; set; }

        [Display(Name="Display Name"), Required]
        public string DisplayName { get; set; }

        [Display(Name = "Field Name"), Required]
        public string FieldName { get; set; }

        [Display(Name = "Data Type"), Required]
        public string DataType { get; set; }

        [Display(Name = "Type"), Required]
        public string Type { get; set; }

        [Display(Name = "List Item")]
        public string ListItem { get; set; }
        public string DefaultValue { get; set; }

        public int? MaxLength { get; set; }

        public Boolean IsVisible { get; set; }


        [MaxLength(100)]
        public string SqlProcGetSet { get; set; }

        
        public int Serial { get; set; }
        public int? NoOfCharToEnter { get; set; }
        public string SqlParameter { get; set; }
        public bool IsCollapse { get; set; }
        public bool IsMandatory { get; set; }
        public string PlaceHolder { get; set; }
        public string ToolTip { get; set; }
        public int FileNo { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

    }
}
