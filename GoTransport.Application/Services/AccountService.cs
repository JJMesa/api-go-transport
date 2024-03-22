using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Settings;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GoTransport.Application.Services;

[Transient]
internal class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSettings _jwtSettings;

    public AccountService(UserManager<User> userManager
        , IOptionsSnapshot<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<JsonResponse<UserTokenDto>> LoginAsync(UserLoginDto userLogin)
    {
        var user = await _userManager.FindByEmailAsync(userLogin.Email);
        if (user is null) return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);

        var checkUserAndPass = await _userManager.CheckPasswordAsync(user, userLogin.Password);
        if (!checkUserAndPass) return ResponseBuilder<UserTokenDto>.BadRequest(ErrorMessages.InvalidCredentials);

        var token = await GenerateToken(user);

        return ResponseBuilder<UserTokenDto>.Ok(token);
    }

    private async Task<UserTokenDto> GenerateToken(User user)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        var dateBase = DateTime.UtcNow;
        var expires = dateBase.Add(_jwtSettings.ExpiryTime);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new ClaimsIdentity(new[]
            {
                new Claim(UserCustomClaims.Id, user.Id.ToString(), ClaimValueTypes.Integer64),
                new Claim(UserCustomClaims.FirstName, user.FirstName),
                new Claim(UserCustomClaims.LastName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, dateBase.ToLongDateString())
            })),
            Expires = expires,
            NotBefore = dateBase,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Audience = _jwtSettings.TokenAudience,
            Issuer = _jwtSettings.TokenIssuer
        };

        var roles = await GetRolesToClaimsAsync(user);
        tokenDescriptor.Subject.AddClaim(roles);

        var token = jwtHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtHandler.WriteToken(token);

        return new UserTokenDto()
        {
            Token = jwtToken,
            Expires = expires.ToPacificStandardTime()
        };
    }

    private async Task<Claim> GetRolesToClaimsAsync(User user)
    {
        Claim roleClaimList = new(ClaimTypes.Role, "");
        var roleList = await _userManager.GetRolesAsync(user);

        if (!roleList.Any()) return roleClaimList;

        roleClaimList = new Claim(ClaimTypes.Role, roleList.First());
        return roleClaimList;
    }
}