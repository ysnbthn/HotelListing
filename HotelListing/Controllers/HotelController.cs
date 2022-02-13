using AutoMapper;
using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CountryController> _logger;
        private readonly IMapper _mapper;

        public HotelController(IUnitOfWork unitOfWork, ILogger<CountryController> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetHotels([FromQuery] RequestParams requestParams)
        {

            var hotels = await _unitOfWork.Hotels.GetPagedList(requestParams);
            var results = _mapper.Map<IList<HotelDTO>>(hotels);
            return Ok(results);

        }

        [HttpGet("{id:int}", Name = "GetHotel")] // diğer endpointler onu çağırabilsin diye isim veriyoruz
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotel(int id)
        {
            // artık include string listesi yerine linQ var
            var hotel = await _unitOfWork.Hotels.Get(q => q.Id == id, include: q=> q.Include(x=> x.Country));
            if (hotel == null) { throw new Exception(); }
            var result = _mapper.Map<HotelDTO>(hotel);
            return Ok(result);

        }

        [Authorize(Roles = "Administrator")] // sadece admin otel yapabilir
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDTO hotelDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid POST attempt in {nameof(CreateHotel)}");
                return BadRequest(ModelState);
            }

            var hotel = _mapper.Map<Hotel>(hotelDTO);
            await _unitOfWork.Hotels.Insert(hotel);
            await _unitOfWork.Save();
            // işlem bittikten sonra gethotel ile objeyi çağır
            return CreatedAtRoute("GetHotel", new { id = hotel.Id }, hotel);

        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHotel([FromBody] CreateHotelDTO hotelDTO, int id)
        {
            if (!ModelState.IsValid || id < 1)
            {
                _logger.LogError($"Invalid UPDATE attempt in {nameof(UpdateHotel)}");
                return BadRequest(ModelState);
            }
            var hotel = await _unitOfWork.Hotels.Get(a => a.Id == id);
            if (hotel == null)
            {
                _logger.LogError($"Invalid UPDATE Hotel in the {nameof(UpdateHotel)}");
                return StatusCode(500, "Submitted data is invalid.");
            }
            // hotelDTO'yu hotel yap
            _mapper.Map(hotelDTO, hotel);
            _unitOfWork.Hotels.Update(hotel);
            await _unitOfWork.Save();

            return NoContent();

        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            if (id < 1)
            {
                _logger.LogError($"Invalid DELETE attempt in {nameof(DeleteHotel)}");
                return BadRequest();
            }


            var hotel = await _unitOfWork.Hotels.Get(x => x.Id == id);
            if (hotel == null)
            {
                _logger.LogError($"Invalid DELETE attempt in {nameof(DeleteHotel)}");
                return BadRequest("Submitted data is invalid");
            }

            await _unitOfWork.Hotels.Delete(id);
            await _unitOfWork.Save();

            return NoContent();
        }

    }
}
