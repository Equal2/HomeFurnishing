using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.DatabaseViews
{
    [Table("_Employee")]
    public class _Employee : EntityBase
    {
        [Key]
        public int PersonID { get; set; }
        public int DesignationID { get; set; }
        public int DepartmentID { get; set; }
    }
}
