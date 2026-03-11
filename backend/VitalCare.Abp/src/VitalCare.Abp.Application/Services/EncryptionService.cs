using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VitalCare.Abp.Services;

/// <summary>
/// AES-256-GCM authenticated encryption for PHI fields.
/// Ciphertext format: base64( nonce[12] || tag[16] || ciphertext )
/// Legacy AES-CTR and AES-CBC formats are transparently migrated on first read.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<EncryptionService>? _logger;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService>? logger = null)
    {
        _logger = logger;
        var keyMaterial = configuration["Encryption:Key"] ?? configuration["ENCRYPTION_KEY"] ?? "";

        if (string.IsNullOrWhiteSpace(keyMaterial))
            throw new InvalidOperationException(
                "Encryption key is not configured. Set the Encryption:Key environment variable (e.g. ENCRYPTION__Key) " +
                "to a 32-byte base64-encoded value. Generate one with: openssl rand -base64 32");

        // Prefer a raw 32-byte base64 key (recommended)
        if (keyMaterial.Length == 44 && IsBase64(keyMaterial))
        {
            try
            {
                var raw = Convert.FromBase64String(keyMaterial);
                if (raw.Length == 32)
                {
                    _key = raw;
                    return;
                }
            }
            catch { /* fall through to PBKDF2 */ }
        }

        // Passphrase — derive a 32-byte key with PBKDF2 (fixed salt acceptable here;
        // recommend using a raw base64 key in production instead)
        var salt = Encoding.UTF8.GetBytes("VitalCare.Encryption.Salt.v1");
        using var derive = new Rfc2898DeriveBytes(keyMaterial, salt, 100_000, HashAlgorithmName.SHA256);
        _key = derive.GetBytes(32);
    }

    /// <summary>Encrypts using AES-256-GCM. Returns base64(nonce[12] + tag[16] + ciphertext).</summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);
        var tag = new byte[16];
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[12 + 16 + cipherBytes.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, 12);
        cipherBytes.CopyTo(result, 28);
        return Convert.ToBase64String(result);
    }

    /// <summary>Decrypts AES-256-GCM ciphertext. Falls back to legacy formats for migration.</summary>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        // Try GCM format first: base64 of nonce[12]+tag[16]+ciphertext (min 29 bytes)
        try
        {
            if (IsBase64(cipherText))
            {
                var data = Convert.FromBase64String(cipherText);
                if (data.Length >= 29)
                {
                    var nonce = data[..12];
                    var tag = data[12..28];
                    var cipher = data[28..];
                    var plain = new byte[cipher.Length];
                    using var aes = new AesGcm(_key, 16);
                    aes.Decrypt(nonce, cipher, tag, plain);
                    return Encoding.UTF8.GetString(plain);
                }
            }
        }
        catch (CryptographicException)
        {
            // GCM tag verification failed — try legacy formats
        }

        return TryLegacyDecrypt(cipherText);
    }

    public string? EncryptValue(string? value)
    {
        if (value == null) return null;
        return string.IsNullOrEmpty(value) ? value : Encrypt(value);
    }

    public string? DecryptValue(string? value)
    {
        if (value == null) return null;
        if (string.IsNullOrEmpty(value)) return value;
        try { return Decrypt(value); }
        catch (Exception ex)
        {
            _logger?.LogWarning("Decryption failed for a stored value. The value may be corrupted or use an unknown format. Error: {Error}", ex.GetType().Name);
            return value;
        }
    }

    /// <summary>Attempts to decrypt legacy AES-CTR (hex iv:ct) or legacy AES-CBC (base64) formats.</summary>
    private string TryLegacyDecrypt(string cipherText)
    {
        // Legacy format 1: AES-CTR "hexiv:hexciphertext"
        var colon = cipherText.IndexOf(':');
        if (colon > 0 && colon < cipherText.Length - 1)
        {
            try
            {
                var iv = Convert.FromHexString(cipherText.AsSpan(0, colon));
                var ct = Convert.FromHexString(cipherText.AsSpan(colon + 1));
                if (iv.Length == 16)
                {
                    var plain = AesCtrTransform(ct, _key, iv);
                    return Encoding.UTF8.GetString(plain);
                }
            }
            catch { /* try next format */ }
        }

        // Legacy format 2: AES-CBC base64(iv[16] + ciphertext)
        if (IsBase64(cipherText))
        {
            try
            {
                var full = Convert.FromBase64String(cipherText);
                if (full.Length > 16)
                {
                    using var aes = Aes.Create();
                    aes.Key = _key;
                    aes.IV = full[..16];
                    using var decryptor = aes.CreateDecryptor();
                    var decrypted = decryptor.TransformFinalBlock(full, 16, full.Length - 16);
                    return Encoding.UTF8.GetString(decrypted);
                }
            }
            catch { /* could not decrypt */ }
        }

        _logger?.LogWarning("Could not decrypt a stored PHI field. The value will be returned as-is.");
        return cipherText;
    }

    private static byte[] AesCtrTransform(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        aes.IV = new byte[16];
        const int blockSize = 16;
        var result = new byte[data.Length];
        var counter = (byte[])iv.Clone();
        for (int i = 0; i < data.Length; i += blockSize)
        {
            using var enc = aes.CreateEncryptor();
            var keystream = enc.TransformFinalBlock(counter, 0, blockSize);
            var len = Math.Min(blockSize, data.Length - i);
            for (int j = 0; j < len; j++)
                result[i + j] = (byte)(data[i + j] ^ keystream[j]);
            IncrementCounter(counter);
        }
        return result;
    }

    private static void IncrementCounter(byte[] counter)
    {
        for (int i = 15; i >= 0; i--)
            if (++counter[i] != 0) break;
    }

    private static bool IsBase64(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        try { Convert.FromBase64String(s); return true; }
        catch { return false; }
    }
}
