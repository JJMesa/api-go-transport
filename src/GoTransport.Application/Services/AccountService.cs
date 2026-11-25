using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Settings;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GoTransport.Application.Services;

internal class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AccountService> _logger;
    private readonly TimeProvider _clock;

    public AccountService(UserManager<User> userManager
        , SignInManager<User> signInManager
        , IOptions<JwtSettings> jwtSettings
        , ILogger<AccountService> logger
        , TimeProvider clock)
    {
        _clock = clock;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<JsonResponse<UserTokenDto>> LoginAsync(UserLoginDto userLogin)
    {
        var user = await _userManager.FindByEmailAsync(userLogin.Email);
        // A deactivated account gets the same answer as an unknown one, so the response does not reveal it exists.
        if (user is null || !user.IsActive) return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, lockoutOnFailure: true);

        // A locked-out account gets the same answer as an unknown one, so lockout cannot be used to find out
        // which emails are registered. Operators see it in the log instead.
        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("Sign-in refused: user {UserId} is locked out.", user.Id);
            return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);
        }

        if (!signInResult.Succeeded) return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);

        var token = await GenerateToken(user);

        return ResponseBuilder<UserTokenDto>.Ok(token);
    }

    public async Task<bool> IsTokenUserValidAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(UserCustomClaims.Id);
        var tokenStamp = principal.FindFirstValue(UserCustomClaims.SecurityStamp);
        if (userId is null || tokenStamp is null) return false;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive) return false;

        // Deactivating a user or changing their role rotates the stamp, which retires every token issued before.
        return tokenStamp == (user.SecurityStamp ?? string.Empty);
    }

    private async Task<UserTokenDto> GenerateToken(User user)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        // Login looks the user up by email, so it is always set here.
        var email = user.Email ?? string.Empty;
        var dateBase = _clock.GetUtcNow().UtcDateTime;
        var expires = dateBase.Add(_jwtSettings.ExpiryTime);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new ClaimsIdentity(new[]
            {
                new Claim(UserCustomClaims.Id, user.Id.ToString(), ClaimValueTypes.Integer64),
                new Claim(UserCustomClaims.FirstName, user.FirstName),
                new Claim(UserCustomClaims.LastName, user.LastName),
                new Claim(UserCustomClaims.SecurityStamp, user.SecurityStamp ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            })),
            // The handler writes iat, nbf and exp as numeric dates (seconds since the Unix epoch).
            IssuedAt = dateBase,
            Expires = expires,
            NotBefore = dateBase,
            SigningCredentials = new SigningCredentials(_jwtSettings.CreateSigningKey(), SecurityAlgorithms.HmacSha256),
            Audience = _jwtSettings.TokenAudience,
            Issuer = _jwtSettings.TokenIssuer
        };

        // One claim per role, so a User with several Roles is authorized for each of them.
        var roles = await _userManager.GetRolesAsync(user);
        tokenDescriptor.Subject.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = jwtHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtHandler.WriteToken(token);

        return new UserTokenDto()
        {
            Token = jwtToken,
            // UTC, serialized with a "Z" so clients convert it to their own zone.
            Expires = expires
        };
    }
}