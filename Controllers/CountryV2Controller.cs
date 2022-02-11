using AutoMapper;
using HotelListing.Data;
using HotelListing.Entities;
using HotelListing.IRepository;
using HotelListing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Controllers
{
    [ApiVersion("2.0", Deprecated = true)]
    [Route("api/Country")]
    [ApiController]
    public class CountryV2Controller : ControllerBase
    {
        // test için
        private DatabaseContext _context;

        public CountryV2Controller(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Country>> GetCountries()
        {

            return await _context.Countries.ToListAsync();

        }
    }
}
