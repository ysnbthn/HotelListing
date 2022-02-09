﻿namespace HotelListing.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        // reverse navigation
        public virtual IList<Hotel> Hotels { get; set; }
    }
}
