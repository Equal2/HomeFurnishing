using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public abstract class DocumentLineAttributes : EntityBase
    {
        [Key]
        public int Id { get; set; }
        public int HeaderTableId { get; set; }
        public int LineTableId { get; set; }
        public int Sr { get; set; }

        [ForeignKey("DocumentTypeAttribute")]
        public int DocumentTypeAttributeId { get; set; }
        public virtual DocumentTypeHeaderAttribute DocumentTypeAttribute { get; set; }
        public string Value { get; set; }
    }
}
