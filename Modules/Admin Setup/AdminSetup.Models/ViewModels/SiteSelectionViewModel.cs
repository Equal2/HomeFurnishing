using System.ComponentModel.DataAnnotations;

namespace AdminSetup.Models.ViewModels
{
    public class SiteSelectionViewModel
    {
        [Required,Range(1,int.MaxValue,ErrorMessage="The Site field is required.")]
        public int SiteId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "The Division field is required.")]
        public int DivisionId { get; set; }
    }
}
