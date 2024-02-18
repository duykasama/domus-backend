using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Domus.Common.Constants;
using Domus.Common.Exceptions;
using Domus.Common.Extensions;
using Domus.Common.Helpers;
using Domus.Common.Settings;
using Domus.DAL.Interfaces;
using Domus.Domain.Entities;
using Domus.Service.Constants;
using Domus.Service.Interfaces;
using Domus.Service.Models;
using Domus.Service.Models.Email;
using Domus.Service.Models.Requests.Authentication;
using Domus.Service.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Domus.Service.Implementations;

public class GoogleOAuthService : IGoogleOAuthService
{
	private readonly IConfiguration _configuration;
	private readonly UserManager<DomusUser> _userManager;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IJwtService _jwtService;
	private readonly IEmailService _emailService;

	public GoogleOAuthService(IConfiguration configuration, UserManager<DomusUser> userManager, IUnitOfWork unitOfWork, IJwtService jwtService, IEmailService emailService)
	{
		_configuration = configuration;
		_userManager = userManager;
		_unitOfWork = unitOfWork;
		_jwtService = jwtService;
		_emailService = emailService;
	}

	public async Task<ServiceActionResult> LoginAsync(OAuthRequest request)
	{
		var googleSettings = _configuration.GetSection(nameof(GoogleSettings)).Get<GoogleSettings>() ?? throw new MissingGoogleSettingsException();
		var googleLoginRequest = (GoogleLoginRequest)request;

		var client = new HttpClient();
		var code = WebUtility.UrlDecode(googleLoginRequest.Code);
		var requestParams = new Dictionary<string, string>
		{
			{ GoogleAuthConstants.CODE, code },
			{ GoogleAuthConstants.CLIENT_ID, googleSettings.ClientId },
			{ GoogleAuthConstants.CLIENT_SECRET, googleSettings.ClientSecret },
			{ GoogleAuthConstants.REDIRECT_URI, googleSettings.RedirectUri },
			{ GoogleAuthConstants.GRANT_TYPE, GoogleAuthConstants.AUTHORIZATION_CODE }
		};

		var content = new FormUrlEncodedContent(requestParams);
		var response = await client.PostAsync(GoogleAuthConstants.GOOGLE_TOKEN_URL, content);
		if (!response.IsSuccessStatusCode)
			throw new Exception();

		var authObject = JsonConvert.DeserializeObject<GoogleAuthResponse>(await response.Content.ReadAsStringAsync());
		if  (authObject?.IdToken == null)
			throw new Exception();

		var handler = new JwtSecurityTokenHandler();
		var securityToken = handler.ReadJwtToken(authObject.IdToken);
		securityToken.Claims.TryGetValue(GoogleTokenClaimConstants.EMAIL, out var email);
		securityToken.Claims.TryGetValue(GoogleTokenClaimConstants.EMAIL_VERIFIED, out var emailVerified);
		securityToken.Claims.TryGetValue(GoogleTokenClaimConstants.GIVEN_NAME, out var name);
		securityToken.Claims.TryGetValue(GoogleTokenClaimConstants.PICTURE, out var picture);

		var user = await _userManager.FindByEmailAsync(email) ?? await CreateNewUserAsync(email, emailVerified, picture);

		var tokenResponse = new TokenResponse
		{
			AccessToken = _jwtService.GenerateAccessToken(user, _userManager.GetRolesAsync(user).Result.ToList()),
			RefreshToken = await _jwtService.GenerateRefreshToken(user.Id),
			ExpiresAt = DateTimeOffset.Now.AddHours(1)
		};

        return new ServiceActionResult(true) { Data = tokenResponse };
    }

	private async Task<DomusUser> CreateNewUserAsync(string email, string emailVerified, string picture)
	{
		var user = new DomusUser
		{
			Email = email,
			EmailConfirmed = emailVerified.ToBool(),
			UserName = email,
			ProfileImage = picture,
		};
		var autoGeneratedPassword = RandomPasswordHelper.GenerateRandomPassword(10);

		var result = await _userManager.CreateAsync(user, autoGeneratedPassword);
		if (!result.Succeeded)
			throw new Exception();
		await _userManager.AddToRoleAsync(user, UserRoleConstants.CLIENT);
		await _unitOfWork.CommitAsync();

		var emailRequest = new PasswordEmail()
		{
			To = user.Email,
			UserName = user.UserName,
			Subject = "PASSWORD CONFIRM EMAIL",
			Password = autoGeneratedPassword
		};
		_emailService.SendEmail(emailRequest);

		var userReturned = await _userManager.FindByEmailAsync(email);
		if (userReturned == null)
			throw new Exception();

		return userReturned;
	}
}
