using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Core.Repository;
using AutoMapper;
using HotelListing.API.Core.Models.Hotels;
using System.Diagnostics.Metrics;
using HotelListing.API.Core.Models;

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
          var hotels = await _hotelsRepository.GetAllAsync<GetHotelDTO>();
          return Ok(hotels); 
        }

        // GET: api/Hotels?StartIndex=0&pagesize=25&PageNumber=1
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetHotelDTO>>> GetPagedHotels([FromQuery] QueryParameters queryParameters)
        {
          PagedResult<GetHotelDTO> pagedHotelsResult = await _hotelsRepository.GetAllPagedAsync<GetHotelDTO>(queryParameters);

          return Ok(pagedHotelsResult);
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetHotelDetailsDTO>> GetHotel(int id)
        {
          GetHotelDetailsDTO hotel =  await _hotelsRepository.GetAsync<GetHotelDetailsDTO>(id);
          return Ok(hotel);
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
          GetHotelDTO hotel = await _hotelsRepository.AddAsync<CreateHotelDTO, GetHotelDTO>(createHotelDTO);

          return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
        }

        // DELETE: api/Hotels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            await _hotelsRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> HotelExists(int id)
        {
            return await _hotelsRepository.Exists(id);
        }
    }
}
