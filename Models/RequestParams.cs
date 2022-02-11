namespace HotelListing.Models
{
    public class RequestParams
    {
        // kullanıcı tek seferde max 50 tane çekebilir
        const int maxPageSize = 50;
        // default page no 1
        public int PageNumber { get; set; } = 1;
        // default page size 10 records
        private int _pageSize = 10;

        public int PageSize
        {
            get { return _pageSize; }
            // eğer 50 den fazla istediyse 50 ye eşitle
            set { _pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }
    }
}
