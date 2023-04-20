using HotelListing.API.Core.Models.Country;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.API.Core.Models.Hotels
{
	public class GetHotelDetailsDTO
	{
		  public int Id { get; set; }
		  public string? Name { get; set; }
      public string? Address { get; set; }
      public double? Rating { get; set; }
			public GetCountryDTO? Country {  get; set; }
			
	}
}
