using System.ComponentModel.DataAnnotations;

namespace AdminSetup.Models.ViewModels
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
