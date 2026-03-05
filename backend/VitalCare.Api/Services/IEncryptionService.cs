namespace VitalCare.Api.Services;

/// <summary>HIPAA: encrypt/decrypt PHI at rest (e.g. patient name).</summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
