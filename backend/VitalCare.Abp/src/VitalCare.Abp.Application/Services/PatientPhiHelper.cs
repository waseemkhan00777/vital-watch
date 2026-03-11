namespace VitalCare.Abp.Services;

/// <summary>
/// Helpers for encrypting and decrypting PHI (Protected Health Information) fields on patient records.
///
/// Field-level encryption strategy for the User entity:
///   ENCRYPTED at rest (AES-256-GCM via IEncryptionService):
///     - Name          — free-text PII; encrypted on every write, decrypted on every read
///
///   PLAINTEXT by design (required for indexed lookup / authentication):
///     - Email         — used as the primary login identifier; must be searchable; treat as sensitive
///                       but cannot be encrypted without a search-safe scheme (e.g. deterministic or HMAC index)
///
///   TODO — add and encrypt when these fields are introduced to the User entity:
///     - Phone         — encrypt with IEncryptionService.Encrypt(); store as encrypted column
///     - DateOfBirth   — encrypt with IEncryptionService.Encrypt()
///     - Address       — encrypt with IEncryptionService.Encrypt()
///
/// Pattern for any new PHI field:
///   On write:  entity.Field = _encryption.Encrypt(value);
///   On read:   dto.Field    = _encryption.Decrypt(entity.Field);
/// </summary>
public static class PatientPhiHelper
{
    /// <summary>
    /// Additional PHI field keys used in dynamic patient-data dictionaries
    /// (e.g. supplementary data passed via generic key-value APIs).
    /// </summary>
    public static readonly string[] PhiFields = { "dob", "phone", "address" };

    public static Dictionary<string, string?> EncryptPatientData(IEncryptionService encryption, IReadOnlyDictionary<string, string?> data)
    {
        var result = new Dictionary<string, string?>();
        foreach (var kv in data)
        {
            result[kv.Key] = PhiFields.Contains(kv.Key, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(kv.Value)
                ? encryption.EncryptValue(kv.Value)
                : kv.Value;
        }
        return result;
    }

    public static Dictionary<string, string?> DecryptPatientData(IEncryptionService encryption, IReadOnlyDictionary<string, string?> data)
    {
        var result = new Dictionary<string, string?>();
        foreach (var kv in data)
        {
            result[kv.Key] = PhiFields.Contains(kv.Key, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(kv.Value)
                ? encryption.DecryptValue(kv.Value)
                : kv.Value;
        }
        return result;
    }
}
