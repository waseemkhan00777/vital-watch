using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface IUserAppService
{
    Task<IReadOnlyList<UserDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
