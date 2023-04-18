using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Repository
{
	public class AuthManager : IAuthManager
	{
		private readonly IMapper _mapper;
		private readonly UserManager<ApiUser> _userManager;

		public AuthManager(IMapper mapper, UserManager<ApiUser> userManager)
    {
			this._mapper = mapper;
			this._userManager = userManager;
		}
    public async Task<IEnumerable<IdentityError>> Register(ApiUserDTO userDTO)
		{
			ApiUser user = _mapper.Map<ApiUser>(userDTO);
			user.UserName = userDTO.Email;

			IdentityResult result = await _userManager.CreateAsync(user, userDTO.Password);

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, "User");
			}

			return result.Errors;
		}
	}
}
