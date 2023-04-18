using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Models.Users
{
	public class ApiUserDTO
	{
    [Required]
      public string FirstName { get; set; }

    [Required]
      public string LastName  { get; set; }

    [Required]
    [EmailAddress]
      public string Email { get; set; }

    [Required]
    [StringLength(15, ErrorMessage = "Your password must be more than 8 but less than 15 characters.", MinimumLength = 6)]
      public string Password { get; set; }
    }
}
