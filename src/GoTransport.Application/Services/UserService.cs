using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.User;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Users;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Application.Services;

[Transient]
internal class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<User> _userRepository;
    private readonly IRoleService _roleService;

    public UserService(UserManager<User> userManager
        , IRepository<User> userRepository
        , IRoleService roleService)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _roleService = roleService;
    }

    public async Task<JsonPagedResponse<IEnumerable<UserDto>>> GetAsync([FromQuery] UserParameters parameters, CancellationToken cancellationToken)
    {
        var users = await _userRepository.ListAsync(new PagedUserSpecification(parameters), cancellationToken);
        var totalRecords = _userRepository.CountAsync(new UserSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, await totalRecords);
        return ResponseBuilder<IEnumerable<UserDto>>.OkPaged(users, metadata);
    }

    public async Task<JsonResponse<UserDto>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null) return ResponseBuilder<UserDto>.NotFound();
        return ResponseBuilder<UserDto>.Ok(user.ToDto());
    }

    public async Task<JsonResponse<UserDto>> CreateAsync(UserCreationDto userCreationDto, CancellationToken cancellationToken)
    {
        if (await IsDuplicateEmailAsync(userCreationDto.Email))
            return ResponseBuilder<UserDto>.BadRequest(ErrorMessages.DuplicateEmail);

        if (!await _roleService.RoleExistsAsync(userCreationDto.RoleId, cancellationToken))
            return ResponseBuilder<UserDto>.BadRequest(ErrorMessages.UnknownRole);

        var user = userCreationDto.ToEntity();
        var isCreated = await _userManager.CreateAsync(user, userCreationDto.Password);

        if (isCreated.Succeeded)
        {
            var assingRolesResult = await _roleService.AssignRolesToUser(user, userCreationDto.RoleId, cancellationToken);
            if (!assingRolesResult.Succeeded)
                return ResponseBuilder<UserDto>.BadRequest(assingRolesResult.Errors.ToErrorList());

            return ResponseBuilder<UserDto>.Created(user.ToDto());
        }
        else
        {
            var errors = isCreated.Errors;
            return ResponseBuilder<UserDto>.BadRequest(errors.ToErrorList());
        }
    }

    public async Task<JsonResponse<UserDto>> UpdateAsync(long id, UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        if (id != userUpdateDto.UserId)
            return ResponseBuilder<UserDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return ResponseBuilder<UserDto>.NotFound();

        if (!await _roleService.RoleExistsAsync(userUpdateDto.RoleId, cancellationToken))
            return ResponseBuilder<UserDto>.BadRequest(ErrorMessages.UnknownRole);

        userUpdateDto.ApplyTo(user);

        // Saving through UserManager with a new security stamp retires the tokens issued before this change,
        // so a deactivated user or a removed role stops working at once instead of when the token expires.
        var updateResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!updateResult.Succeeded)
            return ResponseBuilder<UserDto>.BadRequest(updateResult.Errors.ToErrorList());

        var assignRolesResult = await _roleService.AssignRolesToUser(user, userUpdateDto.RoleId, cancellationToken);
        if (!assignRolesResult.Succeeded)
            return ResponseBuilder<UserDto>.BadRequest(assignRolesResult.Errors.ToErrorList());

        return ResponseBuilder<UserDto>.Ok(user.ToDto());
    }

    private async Task<bool> IsDuplicateEmailAsync(string email) =>
        await _userManager.FindByEmailAsync(email) is not null;
}