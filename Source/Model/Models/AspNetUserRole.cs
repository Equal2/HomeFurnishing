using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class AspNetUserRole
    {
        [Key]
        [Column(Order = 1)]
        [MaxLength(128)]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("AspNetRole")]
        [MaxLength(128)]
        public string RoleId { get; set; }
        public virtual AspNetRole AspNetRole { get; set; }

    }
}
