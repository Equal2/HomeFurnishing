using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.DatabaseViews
{
    [Table("_Roles")]
    public class _Roles : EntityBase
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
