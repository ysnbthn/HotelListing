using Newtonsoft.Json;

namespace HotelListing.Models
{
    public class Error
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        // hatayı cevap olarak göndermek için json objesine çevir
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
