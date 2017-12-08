using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class CityViewModel
    {
        public int CityId { get; set; }
        [Required]
        public string CityName { get; set; }
        [Required]
        public int StateId { get; set; }
        public string StateName { get; set; }
        [Required]
        public int CountryId { get; set; }
        public bool IsActive { get; set; }

    }
}
