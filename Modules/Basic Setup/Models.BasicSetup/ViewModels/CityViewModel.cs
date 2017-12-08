using System.ComponentModel.DataAnnotations;

namespace Models.BasicSetup.ViewModels
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
