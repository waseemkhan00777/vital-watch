using VitalCare.Abp.Entities;
using Volo.Abp.Domain.Repositories;

namespace VitalCare.Abp.Repositories;

public interface IAlertRuleRepository : IRepository<AlertRule, Guid>
{
    Task<List<AlertRule>> GetActiveByVitalTypeAsync(string vitalType, CancellationToken cancellationToken = default);
    Task<List<AlertRule>> GetListOrderedByVitalTypeAsync(CancellationToken cancellationToken = default);
}
