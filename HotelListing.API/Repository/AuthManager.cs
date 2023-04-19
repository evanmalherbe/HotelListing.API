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
		private readonly ILogger<AuthManager> _logger;
		private ApiUser _user;
		private const string _loginProvider = "HotelListingApi";
		private const string _refreshToken = "RefreshToken";

		public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration, ILogger<AuthManager> logger)
		{
			this._mapper = mapper;
			this._userManager = userManager;
			this._configuration = configuration;
			this._logger = logger;
		}

		public async Task<string> CreateRefreshToken()
		{
			await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshToken);
			string newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _loginProvider, _refreshToken);
			IdentityResult result = await _userManager.SetAuthenticationTokenAsync(_user, _loginProvider, _refreshToken, newRefreshToken);

			return newRefreshToken;
		}

		public async Task<AuthResponseDTO> Login(LoginDTO loginDTO)
		{
			_logger.LogInformation($"Looking for user with email {loginDTO.Email}");
			_user = await _userManager.FindByEmailAsync(loginDTO.Email);
			bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDTO.Password);

			if (_user == null || isValidUser == false)
			{
				_logger.LogWarning($"User with email {loginDTO.Email} was not found.");
				return null;
			}

			string token = await GenerateToken();
			_logger.LogInformation($"Token for user with email {loginDTO.Email} was generated successfully. Token: {token}");

			return new AuthResponseDTO
			{
				Token = token,
				UserId = _user.Id,
				RefreshToken = await CreateRefreshToken()
			};
		}

		public async Task<IEnumerable<IdentityError>> Register(ApiUserDTO userDTO)
		{
			_user = _mapper.Map<ApiUser>(userDTO);
			_user.UserName = userDTO.Email;

			IdentityResult result = await _userManager.CreateAsync(_user, userDTO.Password);

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(_user, "User");
			}

			return result.Errors;
		}

		public async Task<AuthResponseDTO> VerifyRefreshToken(AuthResponseDTO request)
		{
			JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			JwtSecurityToken tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
			string username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email)?.Value;

			_user = await _userManager.FindByNameAsync(username);

			if (_user == null || _user.Id != request.UserId)
			{
				return null;
			}

			var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshToken, request.RefreshToken);

			if (isValidRefreshToken)
			{
				var token = await GenerateToken();
				return new AuthResponseDTO
				{
					Token = token,
					UserId = _user.Id,
					RefreshToken = await CreateRefreshToken()
				};
			}

			await _userManager.UpdateSecurityStampAsync(_user);
			return null;
		}

		private async Task<string> GenerateToken()
		{
			SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

			SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			IList<string> roles = await _userManager.GetRolesAsync(_user);
			List<Claim> roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
			IList<Claim> userClaims = await _userManager.GetClaimsAsync(_user);

			IEnumerable<Claim> claims = new List<Claim>
			{
				new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, _user.Email),
				new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email,_user.Email),
				new Claim("uid", _user.Id),
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
