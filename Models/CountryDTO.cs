using System.ComponentModel.DataAnnotations;

namespace HotelListing.Models
{
    // aynı yere koyduk çünkü Single Responsibility Pattern
    public class CreateCountryDTO
    {
        [Required]
        [StringLength(maximumLength: 50, ErrorMessage = "Country Name is Too Long")]
        public string Name { get; set; }
        [Required]
        [StringLength(maximumLength: 2, ErrorMessage = "Short Country Name is Too Long")]
        public string ShortName { get; set; }
    }
    public class CountryDTO : CreateCountryDTO
    {
        // mantık = user countryDTO görcek database bunu görmeyecek
        // database de normal country'i görücek ama bunu görmeyecek
        // biz doğrulamayı yapıp birbirine dönüştürerek aktarıcaz
        public int Id { get; set; }
        public IList<HotelDTO> Hotel { get; set; }
    }
}
