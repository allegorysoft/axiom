using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Security.Encryption;

public class StringEncryptionService(
    IOptions<StringEncryptionOptions> options) :
    IStringEncryptionService
{
    protected const int SaltSize = 32;
    protected const int NonceSize = 12;
    protected const int TagSize = 16;
    protected const int KeySize = 32;

    protected StringEncryptionOptions Options { get; } = options.Value;

    public virtual string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var salt = GenerateSalt();
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var key = DeriveKey(Options.PassPhrase, salt);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            var result = new byte[SaltSize + NonceSize + TagSize + cipherBytes.Length];

            salt.CopyTo(result, 0);
            nonce.CopyTo(result, SaltSize);
            tag.CopyTo(result, SaltSize + NonceSize);
            cipherBytes.CopyTo(result, SaltSize + NonceSize + TagSize);

            return Convert.ToBase64String(result);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(plainBytes);
        }
    }

    public virtual string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        return TryDecrypt(cipherText, out var plainText)
            ? plainText
            : throw new CryptographicException("Invalid ciphertext or passphrase.");
    }

    public virtual bool TryDecrypt(string cipherText, out string plainText)
    {
        plainText = string.Empty;

        if (string.IsNullOrEmpty(cipherText))
            return true;

        byte[]? key = null;
        byte[]? plainBytes = null;

        try
        {
            var data = Convert.FromBase64String(cipherText);
            const int headerSize = SaltSize + NonceSize + TagSize;

            if (data.Length < headerSize)
                return false;

            var salt = data.AsSpan(0, SaltSize);
            var nonce = data.AsSpan(SaltSize, NonceSize);
            var tag = data.AsSpan(SaltSize + NonceSize, TagSize);
            var encrypted = data.AsSpan(headerSize);

            key = DeriveKey(Options.PassPhrase, salt);
            plainBytes = new byte[encrypted.Length];

            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, encrypted, tag, plainBytes);

            plainText = new UTF8Encoding(false, true).GetString(plainBytes);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
        finally
        {
            if (key is not null)
                CryptographicOperations.ZeroMemory(key);

            if (plainBytes is not null)
                CryptographicOperations.ZeroMemory(plainBytes);
        }
    }

    protected virtual byte[] DeriveKey(string passPhrase, ReadOnlySpan<byte> salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            passPhrase,
            salt,
            Options.Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }

    protected virtual byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(SaltSize);
    }
}