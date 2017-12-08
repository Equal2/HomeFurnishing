using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class PersonContactViewModel
    {
        [Key]
        public int PersonContactId { get; set; }

        //This is the id of that person for whom this contact has been generated
        public int PersonID { get; set; }

        [ForeignKey("PersonContactType"), Display(Name = "Contact Type")]
        public int PersonContactTypeId { get; set; }
        public virtual PersonContactType PersonContactType { get; set; }
        public string PersonContactTypeName { get; set; }

        //this is the new generated id for the added contact person
        public int ContactId { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Name { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Suffix { get; set; }

        [Index("IX_Person_Code", IsUnique = true)]
        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Code { get; set; }

        [MaxLength(11, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Phone { get; set; }

        [MaxLength(10, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Mobile { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Email { get; set; }
    }
}
