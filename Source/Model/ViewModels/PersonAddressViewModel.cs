
// New namespace imports:

namespace Model.ViewModels
{
    public class PersonAddressViewModel
    {
        public int PersonAddressId { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string AddressType { get; set; }
        public string Address { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string Zipcode { get; set; }


    }
}
