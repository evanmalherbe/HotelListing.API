using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.API.Core.Models.Hotels
{
	public abstract class BaseHotelDTO
	{
			[Required]
      public string? Name { get; set; }
			[Required]
      public string? Address { get; set; }
      public double? Rating { get; set; }
			[Required]
			[Range(1, int.MaxValue)]
			public int CountryId { get; set; }
	}
}
