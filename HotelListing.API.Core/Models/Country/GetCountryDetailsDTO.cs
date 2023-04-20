using HotelListing.API.Core.Models.Hotels;

namespace HotelListing.API.Core.Models.Country
{
	public class GetCountryDetailsDTO
	{
      public int Id { get; set; }
      public string? Name { get; set; }
      public string? ShortName { get; set; }
			public List<GetHotelDTO>? Hotels {  get; set; }
	}
}
