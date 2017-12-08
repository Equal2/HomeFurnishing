using Model;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class UserType : EntityBase
    {

        [Key,Required,MaxLength(50)]
        public string Name { get; set; }               
    }
}
