using System.Text.Json;
using Microsoft.Extensions.Logging;
using Volo.Abp.Users;
using VitalCare.Abp.DTOs;
using VitalCare.Abp.Entities;
using VitalCare.Abp.Repositories;
using VitalCare.Abp.Services;

namespace VitalCare.Abp;

public class VitalReadingAppService : IVitalReadingAppService
{
    private readonly IVitalReadingRepository _vitalRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICaregiverLinkRepository _caregiverLinkRepository;
    private readonly AlertEvaluationService _alertEvaluationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<VitalReadingAppService> _logger;

    public VitalReadingAppService(
        IVitalReadingRepository vitalRepository,
        IUserRepository userRepository,
        ICaregiverLinkRepository caregiverLinkRepository,
        AlertEvaluationService alertEvaluationService,
        IAuditService auditService,
        ICurrentUser currentUser,
        ILogger<VitalReadingAppService> logger)
    {
        _vitalRepository = vitalRepository;
        _userRepository = userRepository;
        _caregiverLinkRepository = caregiverLinkRepository;
        _alertEvaluationService = alertEvaluationService;
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<VitalReadingDto?> SubmitVitalAsync(JsonElement body, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Id == null || _currentUser.Roles?.Contains("patient") != true)
            return null;

        var patientId = _currentUser.Id.Value;
        if (!body.TryGetProperty("type", out var typeEl))
            return null;
        var type = typeEl.GetString() ?? "";
        var recordedAt = body.TryGetProperty("recordedAt", out var ra) && ra.ValueKind == JsonValueKind.String
            ? DateTime.TryParse(ra.GetString(), out var dt) ? dt : DateTime.UtcNow
            : DateTime.UtcNow;

        (decimal value, decimal? valueSecondary, string unit) parsed = type switch
        {
            "blood_pressure" => body.TryGetProperty("systolic", out var sys) && body.TryGetProperty("diastolic", out var dia)
                ? (sys.GetDecimal(), dia.GetDecimal(), "mmHg")
                : (0m, null, ""),
            "heart_rate" => body.TryGetProperty("value", out var v) ? (v.GetDecimal(), null, "bpm") : (0m, null, ""),
            "blood_glucose" => body.TryGetProperty("value", out var v) ? (v.GetDecimal(), null, "mg/dL") : (0m, null, ""),
            "oxygen_saturation" => body.TryGetProperty("value", out var v) ? (v.GetDecimal(), null, "%") : (0m, null, ""),
            "weight" => body.TryGetProperty("value", out var v) ? (v.GetDecimal(), null, "kg") : (0m, null, ""),
            _ => (0m, null, "")
        };

        if (parsed.unit == "") return null;

        var reading = new VitalReading(Guid.NewGuid())
        {
            PatientId = patientId,
            Type = type,
            Value = parsed.value,
            ValueSecondary = parsed.valueSecondary,
            Unit = parsed.unit,
            RecordedAt = recordedAt,
            Source = "manual",
            CreatedAt = DateTime.UtcNow
        };
        await _vitalRepository.InsertAsync(reading, true, cancellationToken);
        await _alertEvaluationService.EvaluateAsync(reading, cancellationToken);
        await _auditService.LogAsync(patientId, _currentUser.Email, _currentUser.Roles?.FirstOrDefault() ?? "", "vital_reading", "create", resourceId: reading.Id.ToString(), cancellationToken: cancellationToken);

        return new VitalReadingDto(reading.Id.ToString(), reading.PatientId.ToString(), reading.Type, reading.Value, reading.ValueSecondary, reading.Unit, reading.RecordedAt, reading.Source, reading.CreatedAt);
    }

    public async Task<IReadOnlyList<VitalReadingDto>> GetListAsync(Guid? patientId, CancellationToken cancellationToken = default)
    {
        var allowedPatientIds = await GetAllowedPatientIdsAsync(cancellationToken);
        if (allowedPatientIds == null) return Array.Empty<VitalReadingDto>();

        var filterId = patientId ?? (allowedPatientIds.Count == 1 ? allowedPatientIds.First() : null);
        if (filterId.HasValue && !allowedPatientIds.Contains(filterId.Value))
            return Array.Empty<VitalReadingDto>();

        var list = await _vitalRepository.GetListByPatientIdsAsync(allowedPatientIds, filterId, 500, cancellationToken);
        return list.Select(v => new VitalReadingDto(v.Id.ToString(), v.PatientId.ToString(), v.Type, v.Value, v.ValueSecondary, v.Unit, v.RecordedAt, v.Source, v.CreatedAt)).ToList();
    }

    public async Task<VitalReadingDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var allowedPatientIds = await GetAllowedPatientIdsAsync(cancellationToken);
        if (allowedPatientIds == null) return null;
        var reading = await _vitalRepository.GetAsync(id, true, cancellationToken);
        if (reading == null || !allowedPatientIds.Contains(reading.PatientId)) return null;
        return new VitalReadingDto(reading.Id.ToString(), reading.PatientId.ToString(), reading.Type, reading.Value, reading.ValueSecondary, reading.Unit, reading.RecordedAt, reading.Source, reading.CreatedAt);
    }

    private async Task<HashSet<Guid>?> GetAllowedPatientIdsAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.Id == null) return null;
        if (_currentUser.Roles?.Contains("admin") == true || _currentUser.Roles?.Contains("clinician") == true)
        {
            var patientIds = await _userRepository.GetPatientIdsAsync(cancellationToken);
            return patientIds.ToHashSet();
        }
        if (_currentUser.Roles?.Contains("patient") == true)
            return new HashSet<Guid> { _currentUser.Id.Value };
        if (_currentUser.Roles?.Contains("caregiver") == true)
        {
            var patientIds = await _caregiverLinkRepository.GetConsentedPatientIdsForCaregiverAsync(_currentUser.Id.Value, cancellationToken);
            return patientIds.ToHashSet();
        }
        return null;
    }
}
