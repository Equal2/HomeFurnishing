using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ImportMessage : EntityBase
    {
        [Key]
        public int ImportMessageId { get; set; }
        [ForeignKey("ImportHeader")]
        public int ImportHeaderId { get; set; }
        public virtual ImportHeader ImportHeader { get; set; }
        
        [MaxLength(100)]
        public string SqlProcedure { get; set; }
        
        [MaxLength(100)]
        public string RecordId { get; set; }
        public string Head { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public string Remark { get; set; }
        public Boolean IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
