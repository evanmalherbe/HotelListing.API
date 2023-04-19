using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IAuthManager _authManager;

		public AccountController(IAuthManager authManager)
		{
			this._authManager = authManager;
		}

		// POST: api/Account/register
		[HttpPost]
		[Route("register")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult> Register([FromBody] ApiUserDTO apiUserDTO)
		{
			var errors = await _authManager.Register(apiUserDTO);

			if (errors.Any())
			{
				foreach (var error in errors)
				{
					ModelState.AddModelError(error.Code, error.Description);
				}
				return BadRequest(ModelState);
			}
			return Ok();
		}

		// POST: api/Account/login
		[HttpPost]
		[Route("login")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO)
		{
			AuthResponseDTO authResponse = await _authManager.Login(loginDTO);

			if (authResponse == null)
			{
				return Unauthorized();
			}
			return Ok(authResponse);
		}
	}
}
