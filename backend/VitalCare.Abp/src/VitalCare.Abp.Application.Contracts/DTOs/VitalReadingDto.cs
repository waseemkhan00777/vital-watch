namespace VitalCare.Abp.DTOs;

public record VitalReadingDto(
    string Id,
    string PatientId,
    string Type,
    decimal Value,
    decimal? ValueSecondary,
    string Unit,
    DateTime RecordedAt,
    string Source,
    DateTime CreatedAt);
