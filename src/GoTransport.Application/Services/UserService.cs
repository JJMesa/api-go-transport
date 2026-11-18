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
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return ResponseBuilder<UserDto>.NotFound();
        return ResponseBuilder<UserDto>.Ok(user.ToDto());
    }

    public async Task<JsonResponse<UserDto>> CreateAsync(UserCreationDto userCreationDto)
    {
        if (await IsDuplicateEmailAsync(userCreationDto.Email))
            return ResponseBuilder<UserDto>.BadRequest(ErrorMessages.DuplicateEmail);

        var user = userCreationDto.ToEntity();
        var isCreated = await _userManager.CreateAsync(user, userCreationDto.Password);

        if (isCreated.Succeeded)
        {
            var assingRolesResult = await _roleService.AssignRolesToUser(user, userCreationDto.RoleId);
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

    public async Task<JsonResponse<UserDto>> UpdateAsync(long id, UserUpdateDto userUpdateDto)
    {
        if (id != userUpdateDto.UserId)
            return ResponseBuilder<UserDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return ResponseBuilder<UserDto>.NotFound();

        userUpdateDto.ApplyTo(user);
        await _userRepository.UpdateAsync(user);

        return ResponseBuilder<UserDto>.Ok(user.ToDto());
    }

    private async Task<bool> IsDuplicateEmailAsync(string email) =>
        await _userManager.FindByEmailAsync(email) is not null;
}