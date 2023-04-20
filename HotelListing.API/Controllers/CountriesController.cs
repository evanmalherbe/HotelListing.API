using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Core.Models.Country;
using AutoMapper;
using HotelListing.API.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Core.Exceptions;
using HotelListing.API.Core.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HotelListing.API.Controllers
{
    [Route("api/v{version:apiVersion}/countries")]
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    public class CountriesController : ControllerBase
    {
      private readonly IMapper _mapper;
		  private readonly ICountriesRepository _countriesRepository;

		  public CountriesController(IMapper mapper, ICountriesRepository countriesRepository)
		  {
        this._mapper = mapper;
			  this._countriesRepository = countriesRepository;
		  }

      // GET: api/Countries/GetAll
      [HttpGet("GetAll")]
      public async Task<ActionResult<IEnumerable<GetCountryDTO>>> GetCountries()
		  {
        var countries = await _countriesRepository.GetAllAsync();
			  return Ok(countries);
      }

      // GET: api/v1/Countries?StartIndex=0&pagesize=25&PageNumber=1
      [HttpGet]
      public async Task<ActionResult<PagedResult<GetCountryDTO>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
		  {
        PagedResult<GetCountryDTO> pagedCountriesResult = await _countriesRepository.GetAllPagedAsync<GetCountryDTO>(queryParameters);

        return Ok(pagedCountriesResult);
      }

      // GET: api/Countries/5
      [HttpGet("{id}")]
      public async Task<ActionResult<GetCountryDetailsDTO>> GetCountry(int id)
      {
          var country = await _countriesRepository.GetDetails(id);

          //if (country == null)
          //{
          //  throw new NotFoundException(nameof(GetCountry), id);
          //}

          //GetCountryDetailsDTO countryDTO = _mapper.Map<GetCountryDetailsDTO>(country);

          return Ok(country);
      }

      // PUT: api/Countries/5
      // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
      [HttpPut("{id}")]
      [Authorize]
      public async Task<IActionResult> PutCountry(int id, UpdateCountryDTO updateCountryDTO)
      {
          if (id != updateCountryDTO.Id)
          {
              return BadRequest("Invalid record Id");
          }

          //Country country = await _countriesRepository.GetAsync(id);

          //if (country == null)
          //{
          //  throw new NotFoundException(nameof(GetCountries), id);
          //}

          //_mapper.Map(updateCountryDTO, country);

          try
          {
              await _countriesRepository.UpdateAsync(id, updateCountryDTO);
          }
          catch (DbUpdateConcurrencyException)
          {
              if (!await CountryExists(id))
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

		private async Task<bool> CountryExists(int id)
		{
      return await _countriesRepository.Exists(id);
		}

		// POST: api/Countries
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPost]
    [Authorize]
    public async Task<ActionResult<GetCountryDTO>> PostCountry(CreateCountryDTO createCountryDTO)
    {
       // Country country = _mapper.Map<Country>(createCountryDTO);

      var country = await _countriesRepository.AddAsync<CreateCountryDTO, GetCountryDTO>(createCountryDTO);

      return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        //Country country = await _countriesRepository.GetAsync(id);

        //if (country == null)
        //{
        //  throw new NotFoundException(nameof(DeleteCountry), id);
        //}

        await _countriesRepository.DeleteAsync(id);
  
        return NoContent();
    }
    }
}
