using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;

namespace VitalCare.Abp;

public class AlertRuleAppService : IAlertRuleAppService
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly ICurrentUser _currentUser;

    public AlertRuleAppService(IAlertRuleRepository alertRuleRepository, ICurrentUser currentUser)
    {
        _alertRuleRepository = alertRuleRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AlertRuleDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true) return Array.Empty<AlertRuleDto>();
        var list = await _alertRuleRepository.GetListOrderedByVitalTypeAsync(cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<AlertRuleDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true) return null;
        var rule = await _alertRuleRepository.GetAsync(id, true, cancellationToken);
        return rule == null ? null : MapToDto(rule);
    }

    public async Task<AlertRuleDto> CreateAsync(CreateAlertRuleInput input, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true) throw new UnauthorizedAccessException("Admin only");
        var rule = new AlertRule(Guid.NewGuid())
        {
            VitalType = input.VitalType,
            Severity = input.Severity,
            Operator = input.Operator,
            ThresholdMin = input.ThresholdMin,
            ThresholdMax = input.ThresholdMax,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        await _alertRuleRepository.InsertAsync(rule, true, cancellationToken);
        return MapToDto(rule);
    }

    public async Task<AlertRuleDto?> UpdateAsync(Guid id, UpdateAlertRuleInput input, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true) return null;
        var rule = await _alertRuleRepository.GetAsync(id, true, cancellationToken);
        if (rule == null) return null;
        if (input.VitalType != null) rule.VitalType = input.VitalType;
        if (input.Severity != null) rule.Severity = input.Severity;
        if (input.Operator != null) rule.Operator = input.Operator;
        if (input.ThresholdMin.HasValue) rule.ThresholdMin = input.ThresholdMin;
        if (input.ThresholdMax.HasValue) rule.ThresholdMax = input.ThresholdMax;
        if (input.IsActive.HasValue) rule.IsActive = input.IsActive.Value;
        rule.UpdatedAt = DateTime.UtcNow;
        await _alertRuleRepository.UpdateAsync(rule, true, cancellationToken);
        return MapToDto(rule);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Roles?.Contains("admin") != true) return;
        await _alertRuleRepository.DeleteAsync(id, true, cancellationToken);
    }

    private static AlertRuleDto MapToDto(AlertRule r) => new(r.Id.ToString(), r.VitalType, r.Severity, r.Operator, r.ThresholdMin, r.ThresholdMax, r.IsActive, r.CreatedAt, r.UpdatedAt);
}
