using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public abstract class DocumentHeaderAttributes : EntityBase
    {
        [Key]
        public int Id { get; set; }
        public int HeaderTableId { get; set; }

        [ForeignKey("DocumentTypeHeaderAttribute")]
        public int DocumentTypeHeaderAttributeId { get; set; }
        public virtual DocumentTypeHeaderAttribute DocumentTypeHeaderAttribute { get; set; }
        public string Value { get; set; }
    }
}
