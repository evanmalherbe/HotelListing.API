using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Core.Contracts
{
	public interface IAuthManager
	{
		Task<IEnumerable<IdentityError>> Register(ApiUserDTO userDTO);
		Task<AuthResponseDTO> Login(LoginDTO loginDTO);

		Task<string> CreateRefreshToken();
		Task<AuthResponseDTO> VerifyRefreshToken(AuthResponseDTO request);
	}
}
