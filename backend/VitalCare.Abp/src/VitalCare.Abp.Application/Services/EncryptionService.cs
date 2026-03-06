using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace VitalCare.Abp.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"];
        if (!string.IsNullOrEmpty(keyBase64))
        {
            _key = Convert.FromBase64String(keyBase64);
            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption:Key must be 32 bytes (base64).");
        }
        else
        {
            _key = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(_key);
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        try
        {
            var full = Convert.FromBase64String(cipherText);
            if (full.Length < 16) return cipherText; // too short to be IV+ciphertext; treat as plain
            using var aes = Aes.Create();
            aes.Key = _key;
            var iv = new byte[16];
            Buffer.BlockCopy(full, 0, iv, 0, 16);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var encrypted = new byte[full.Length - 16];
            Buffer.BlockCopy(full, 16, encrypted, 0, encrypted.Length);
            var decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return System.Text.Encoding.UTF8.GetString(decrypted);
        }
        catch (FormatException)
        {
            return cipherText; // not valid base64, e.g. plain text from seeder
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return cipherText; // wrong key or not our ciphertext (e.g. plain text)
        }
    }
}
