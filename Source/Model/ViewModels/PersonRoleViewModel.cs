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
    public class PersonRoleViewModel
    {
        [Key]
        public int PersonRoleId { get; set; }

        //This is the id of that person for whom this Role has been generated
        public int PersonId { get; set; }

        [ForeignKey("RoleDocType"), Display(Name = "Role")]
        public int RoleDocTypeId { get; set; }
        public virtual DocumentType RoleDocType { get; set; }
        public string RoleDocTypeName { get; set; }

    }
}
