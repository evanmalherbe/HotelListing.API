using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Repository
{
	public class AuthManager : IAuthManager
	{
		private readonly IMapper _mapper;
		private readonly UserManager<ApiUser> _userManager;
		private readonly IConfiguration _configuration;

		public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration)
		{
			this._mapper = mapper;
			this._userManager = userManager;
			this._configuration = configuration;
		}

		public async Task<AuthResponseDTO> Login(LoginDTO loginDTO)
		{
			ApiUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
			bool isValidUser = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

			if (user == null || isValidUser == false)
			{
				return null;
			}

			string token = await GenerateToken(user);
			return new AuthResponseDTO
			{
				Token = token,
				UserId = user.Id
			};
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

		private async Task<string> GenerateToken(ApiUser user)
		{
			SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

			SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			IList<string> roles = await _userManager.GetRolesAsync(user);
			List<Claim> roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
			IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

			IEnumerable<Claim> claims = new List<Claim>
			{
				new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.Email),
				new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email,user.Email),
				new Claim("uid", user.Id),
			}
			.Union(userClaims).Union(roleClaims);

			var token = new JwtSecurityToken(
				issuer: _configuration["JwtSettings:Issuer"],
				audience: _configuration["JwtSettings:Audience"],
				claims: claims,
				expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
				signingCredentials: credentials
				);

			return new JwtSecurityTokenHandler().WriteToken(token);

		}
	}
}
