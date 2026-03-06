using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public class UserAppService : IUserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryption;
    private readonly ICurrentUser _currentUser;

    public UserAppService(IUserRepository userRepository, IEncryptionService encryption, ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _encryption = encryption;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<UserDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true && _currentUser.Roles?.Contains("clinician") != true)
            return Array.Empty<UserDto>();

        var list = await _userRepository.GetListOrderedByEmailAsync(cancellationToken);
        return list.Select(u => new UserDto(u.Id.ToString(), u.Email, u.Role, _encryption.Decrypt(u.Name))).ToList();
    }

    public async Task<UserDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true && _currentUser.Roles?.Contains("clinician") != true)
            return null;
        var user = await _userRepository.GetAsync(id, true, cancellationToken);
        return user == null ? null : new UserDto(user.Id.ToString(), user.Email, user.Role, _encryption.Decrypt(user.Name));
    }
}
