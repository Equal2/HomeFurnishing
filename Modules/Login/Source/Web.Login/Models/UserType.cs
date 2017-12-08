using Model;
using System.ComponentModel.DataAnnotations;

namespace Models.Login.Models
{
    public class UserType : EntityBase
    {

        [Key,Required,MaxLength(50)]
        public string Name { get; set; }               
    }
}
