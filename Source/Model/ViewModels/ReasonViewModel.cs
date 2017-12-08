using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels
{
    public class ReasonViewModel
    {
        [Required,MinLength(20,ErrorMessage="Minimum Length of 20 characters should be typed")]
        public string Reason { get; set; }
        public int id { get; set; }
        public string sid { get; set; }
        public int DocTypeId { get; set; }

    }
}
