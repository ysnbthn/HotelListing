using Microsoft.AspNetCore.Identity;

namespace HotelListing.Entities
{
    public class ApiUser : IdentityUser
    {
        // identity user fonksiyonları + ihtiyacımı olan yerler
        public string FirstName { get; set; }
        public string LasttName { get; set; }
    }
}
