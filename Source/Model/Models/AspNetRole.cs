using System.ComponentModel.DataAnnotations;

namespace Model.Models
{
    public class AspNetRole
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
               
        public string Name { get; set; }


    }
}
