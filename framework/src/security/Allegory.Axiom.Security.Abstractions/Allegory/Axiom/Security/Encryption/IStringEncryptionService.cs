namespace Allegory.Axiom.Security.Encryption;

public interface IStringEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    bool TryDecrypt(string cipherText, out string plainText);
}