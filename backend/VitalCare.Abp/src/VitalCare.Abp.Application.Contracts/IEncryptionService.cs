namespace VitalCare.Abp;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
