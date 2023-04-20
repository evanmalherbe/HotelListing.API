﻿
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Data;

namespace HotelListing.API.Core.Contracts
{
	public interface ICountriesRepository : IGenericRepository<Country>
	{
		Task<GetCountryDTO> GetDetails(int id);
	}
}
