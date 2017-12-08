using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Models.Login.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }
        [MaxLength(250)]
        public string ApplicationURL { get; set; }
        [MaxLength(250)]
        public string ApplicationDefaultPage { get; set; }

        [MaxLength(250)]
        public string ApplicationDescription { get; set; }
        [MaxLength(100)]
        public string IconName { get; set; }
        [MaxLength(250)]
        public string ConnectionString { get; set; }

        public string TopBarColour { get; set; }

    }
}