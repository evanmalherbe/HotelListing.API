using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Contracts;
using HotelListing.API.Models.Country;
using HotelListing.API.Repository;
using AutoMapper;
using HotelListing.API.Models.Hotels;
using System.Diagnostics.Metrics;
using HotelListing.API.Models;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
		    private readonly IMapper _mapper;
		    private readonly IHotelsRepository _hotelsRepository;

		    public HotelsController(IMapper mapper, IHotelsRepository hotelsRepository)
        {
			    this._mapper = mapper;
			    this._hotelsRepository = hotelsRepository;
		    }

        // GET: api/Hotels/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<GetHotelDTO>>> GetHotels()
        {
          List<Hotel> hotels = await _hotelsRepository.GetAllAsync();
          if (hotels == null)
          {
              return NotFound();
          }
          
          List<GetHotelDTO> records = _mapper.Map<List<GetHotelDTO>>(hotels);

          return Ok(records); 
        }

        // GET: api/Hotels?StartIndex=0&pagesize=25&PageNumber=1
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetHotelDTO>>> GetPagedHotels([FromQuery] QueryParameters queryParameters)
        {
          //List<Hotel> hotels = await _hotelsRepository.GetAllAsync();
          //if (hotels == null)
          //{
          //    return NotFound();
          //}
          
          //List<GetHotelDTO> records = _mapper.Map<List<GetHotelDTO>>(hotels);

          //return Ok(records); 

          PagedResult<GetHotelDTO> pagedHotelsResult = await _hotelsRepository.GetAllAsync<GetHotelDTO>(queryParameters);

          return Ok(pagedHotelsResult);
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetHotelDetailsDTO>> GetHotel(int id)
        {
          Hotel hotel =  await _hotelsRepository.GetDetails(id);

          if (hotel == null)
          {
              return NotFound();
          }
          
          GetHotelDetailsDTO hotelDTO = _mapper.Map<GetHotelDetailsDTO>(hotel);

          return Ok(hotelDTO);
        }

        // PUT: api/Hotels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, UpdateHotelDTO updateHotelDTO)
        {
            if (id != updateHotelDTO.Id)
            {
                return BadRequest();
            }

            Hotel hotel = await _hotelsRepository.GetAsync(id);

            if (hotel == null)
            {
              return NotFound();
            }

            _mapper.Map(updateHotelDTO, hotel);

            try
            {
                await _hotelsRepository.UpdateAsync(hotel);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await HotelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Hotels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Hotel>> PostHotel(CreateHotelDTO createHotelDTO)
		{
          Hotel hotel = _mapper.Map<Hotel>(createHotelDTO);

          if (hotel == null)
          {
              return Problem("Entity set 'HotelListingDbContext.Hotels'  is null.");
          }

          await _hotelsRepository.UpdateAsync(hotel);

          return CreatedAtAction("GetHotel", new { id = hotel.Id }, hotel);
        }

        // DELETE: api/Hotels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            Hotel hotel = await _hotelsRepository.GetAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            await _hotelsRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> HotelExists(int id)
        {
            return await _hotelsRepository.Exists(id);
        }
    }
}
