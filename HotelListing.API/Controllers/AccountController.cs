using HotelListing.API.Core.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;

namespace HotelListing.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IAuthManager _authManager;
		private readonly ILogger<AccountController> _logger;

		public AccountController(IAuthManager authManager, ILogger<AccountController> logger)
		{
			this._authManager = authManager;
			this._logger = logger;
		}

		// POST: api/Account/register
		[HttpPost]
		[Route("register")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult> Register([FromBody] ApiUserDTO apiUserDTO)
		{
			IEnumerable<IdentityError> errors = await _authManager.Register(apiUserDTO);

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

		// POST: api/Account/refreshtoken
		[HttpPost]
		[Route("refreshtoken")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult> RefreshToken([FromBody] AuthResponseDTO request)
		{
			AuthResponseDTO authResponse = await _authManager.VerifyRefreshToken(request);

			if (authResponse == null)
			{
				return Unauthorized();
			}
			return Ok(authResponse);
		}
	}
}
