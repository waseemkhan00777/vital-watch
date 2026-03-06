using VitalCare.Abp.DTOs;

namespace VitalCare.Abp;

public interface IAlertRuleAppService
{
    Task<IReadOnlyList<AlertRuleDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<AlertRuleDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AlertRuleDto> CreateAsync(CreateAlertRuleInput input, CancellationToken cancellationToken = default);
    Task<AlertRuleDto?> UpdateAsync(Guid id, UpdateAlertRuleInput input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
