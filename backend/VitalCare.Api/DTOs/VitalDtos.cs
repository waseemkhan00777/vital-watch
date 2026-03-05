namespace VitalCare.Api.DTOs;

/// <summary>Discriminated by type; matches frontend VitalSubmission (Zod).</summary>
public abstract record VitalSubmissionBase
{
    public DateTime? RecordedAt { get; init; }
}

public record BloodPressureSubmission : VitalSubmissionBase
{
    public const string Type = "blood_pressure";
    public decimal Systolic { get; init; }
    public decimal Diastolic { get; init; }
}

public record HeartRateSubmission : VitalSubmissionBase
{
    public const string Type = "heart_rate";
    public decimal Value { get; init; }
}

public record BloodGlucoseSubmission : VitalSubmissionBase
{
    public const string Type = "blood_glucose";
    public decimal Value { get; init; }
}

public record OxygenSaturationSubmission : VitalSubmissionBase
{
    public const string Type = "oxygen_saturation";
    public decimal Value { get; init; }
}

public record WeightSubmission : VitalSubmissionBase
{
    public const string Type = "weight";
    public decimal Value { get; init; }
}

public record VitalReadingDto(
    string Id,
    string PatientId,
    string Type,
    decimal Value,
    decimal? ValueSecondary,
    string Unit,
    string RecordedAt,
    string Source,
    string CreatedAt
);
