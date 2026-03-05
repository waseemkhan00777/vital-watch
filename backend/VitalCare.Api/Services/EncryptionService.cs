using System.Security.Cryptography;
using System.Text;

namespace VitalCare.Api.Services;

/// <summary>HIPAA: AES-256-CBC encryption for PHI at rest. Key from config (32 bytes for AES-256).</summary>
public class EncryptionService : IEncryptionService
{
    private const string Prefix = "ENC:";
    private readonly byte[] _key;

    public EncryptionService(IConfiguration config)
    {
        var keyB64 = config["Encryption:Key"];
        if (string.IsNullOrEmpty(keyB64))
        {
            // Fallback for dev only; production must set Encryption:Key (base64, 32 bytes)
            var fallback = "VitalCareEncryptionKey32BytesLong!!";
            _key = Encoding.UTF8.GetBytes(fallback)[..32];
        }
        else
        {
            _key = Convert.FromBase64String(keyB64);
            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption:Key must be 32 bytes (base64).");
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        var bytes = Encoding.UTF8.GetBytes(plainText);
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var enc = aes.CreateEncryptor();
        var encrypted = enc.TransformFinalBlock(bytes, 0, bytes.Length);
        var combined = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, combined, aes.IV.Length, encrypted.Length);
        return Prefix + Convert.ToBase64String(combined);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        if (!cipherText.StartsWith(Prefix, StringComparison.Ordinal))
            return cipherText; // Legacy plaintext
        var combined = Convert.FromBase64String(cipherText[Prefix.Length..]);
        if (combined.Length < 16) return cipherText;
        using var aes = Aes.Create();
        aes.Key = _key;
        var iv = new byte[16];
        Buffer.BlockCopy(combined, 0, iv, 0, 16);
        aes.IV = iv;
        using var dec = aes.CreateDecryptor();
        var encrypted = new byte[combined.Length - 16];
        Buffer.BlockCopy(combined, 16, encrypted, 0, encrypted.Length);
        var decrypted = dec.TransformFinalBlock(encrypted, 0, encrypted.Length);
        return Encoding.UTF8.GetString(decrypted);
    }
}
