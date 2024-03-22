using GoTransport.Application.Dtos.User;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IUserService
{
    Task<JsonPagedResponse<IEnumerable<UserDto>>> GetAsync(UserParameters parameters, CancellationToken cancellationToken);

    Task<JsonResponse<UserDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken);

    Task<JsonResponse<UserDto>> CreateAsync(UserCreationDto userCreationDto);

    Task<JsonResponse<UserDto>> UpdateAsync(dynamic id, UserUpdateDto userUpdateDto);
}