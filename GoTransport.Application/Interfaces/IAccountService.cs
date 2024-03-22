using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IAccountService
{
    Task<JsonResponse<UserTokenDto>> LoginAsync(UserLoginDto request);
}