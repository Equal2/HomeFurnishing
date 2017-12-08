using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PersonRole : EntityBase, IHistoryLog
    {
        [Key]
        public int PersonRoleId { get; set; }

        [ForeignKey("Person"), Display(Name = "Person Name")]
        [Index("IX_PersonRole_DocId", IsUnique = true, Order = 1)]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("RoleDocType"), Display(Name = "Role")]
        [Index("IX_PersonRole_DocId", IsUnique = true, Order = 2)]
        public int RoleDocTypeId { get; set; }
        public virtual DocumentType RoleDocType { get; set; }

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

    }
}