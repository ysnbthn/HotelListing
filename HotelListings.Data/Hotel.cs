using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.Data
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }

        [ForeignKey(nameof(Country))] // key adı = Country adı
        public int CountryId { get; set; } // countrydeki Id
        public Country Country { get; set; } // relation için table adı
    }
}
