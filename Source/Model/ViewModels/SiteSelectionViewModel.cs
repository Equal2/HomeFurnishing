using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SiteSelectionViewModel
    {
        [Required,Range(1,int.MaxValue,ErrorMessage="The Site field is required.")]
        public int SiteId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "The Division field is required.")]
        public int DivisionId { get; set; }
    }
}
