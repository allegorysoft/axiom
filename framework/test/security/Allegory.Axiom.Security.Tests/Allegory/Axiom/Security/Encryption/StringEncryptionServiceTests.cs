using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Security.Encryption;

public class StringEncryptionServiceTests(
    IntegrationTestFixture fixture) :
    IClassFixture<IntegrationTestFixture>
{
    protected IStringEncryptionService Service => fixture.Service<IStringEncryptionService>();

    [Fact]
    public void ShouldEncryptAndDecrypt()
    {
        const string plainText = "Hello, World!";

        var encrypted = Service.Encrypt(plainText);
        var decrypted = Service.Decrypt(encrypted);

        decrypted.ShouldBe(plainText);
    }

    [Fact]
    public void ShouldNotThrowForEmptyOrNull()
    {
        Service.Encrypt(string.Empty).ShouldBe(string.Empty);
        Service.Decrypt(string.Empty).ShouldBe(string.Empty);

        Service.Encrypt(null!).ShouldBeNull();
        Service.Decrypt(null!).ShouldBeNull();
    }

    [Fact]
    public void ShouldProduceDifferentCiphertextsForSamePlaintext()
    {
        const string plainText = "Same input";

        var encrypted1 = Service.Encrypt(plainText);
        var encrypted2 = Service.Encrypt(plainText);

        encrypted1.ShouldNotBe(encrypted2);
        Service.Decrypt(encrypted1).ShouldBe(plainText);
        Service.Decrypt(encrypted2).ShouldBe(plainText);
    }

    [Fact]
    public void ShouldReturnTrueAndPlaintextForValidCiphertext()
    {
        const string plainText = "Test data";
        var encrypted = Service.Encrypt(plainText);

        var result = Service.TryDecrypt(encrypted, out var decrypted);

        result.ShouldBeTrue();
        decrypted.ShouldBe(plainText);
    }

    [Fact]
    public void ShouldReturnFalseForInvalidCiphertext()
    {
        var result = Service.TryDecrypt("not-base64!", out var decrypted);

        result.ShouldBeFalse();
        decrypted.ShouldBe(string.Empty);
    }

    [Fact]
    public void ShouldThrowCryptographicExceptionForInvalidCiphertext()
    {
        Should.Throw<CryptographicException>(() =>
            Service.Decrypt("invalid-ciphertext"));
    }

    [Fact]
    public async Task ShouldThrowCryptographicExceptionForWrongPassphrase()
    {
        const string plainText = "Secret message";
        var encrypted = Service.Encrypt(plainText);

        var services = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.Configure<StringEncryptionOptions>(o => o.PassPhrase = "DifferentPassphrase");
        });

        var otherService = services.GetRequiredService<IStringEncryptionService>();

        Should.Throw<CryptographicException>(() =>
            otherService.Decrypt(encrypted));
    }

    [Fact]
    public void ShouldHandleUnicodeText()
    {
        const string plainText = "🎉 Unicode: 你好, 世界! Привет мир! 🌍";

        var encrypted = Service.Encrypt(plainText);
        var decrypted = Service.Decrypt(encrypted);

        decrypted.ShouldBe(plainText);
    }

    [Fact]
    public void ShouldHandleLargeText()
    {
        var plainText = new string('A', 100_000);

        var encrypted = Service.Encrypt(plainText);
        var decrypted = Service.Decrypt(encrypted);

        decrypted.ShouldBe(plainText);
    }

    [Fact]
    public void ShouldHandleSpecialCharacters()
    {
        const string plainText = "Special: \t\n\r\"\\'/<>[]{}|`~!@#$%^&*()-_=+";

        var encrypted = Service.Encrypt(plainText);
        var decrypted = Service.Decrypt(encrypted);

        decrypted.ShouldBe(plainText);
    }

    [Fact]
    public async Task ShouldWorkWithCustomIterations()
    {
        const string plainText = "Custom iterations test";

        var services = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.Configure<StringEncryptionOptions>(o =>
            {
                o.PassPhrase = "CustomPassphrase";
                o.Iterations = 50_000;
            });
        });

        var customService = services.GetRequiredService<IStringEncryptionService>();

        var encrypted = customService.Encrypt(plainText);
        var decrypted = customService.Decrypt(encrypted);

        decrypted.ShouldBe(plainText);
    }

    [Fact]
    public void ShouldProduceValidBase64Ciphertext()
    {
        var encrypted = Service.Encrypt("test");

        Should.NotThrow(() => Convert.FromBase64String(encrypted));
    }

    [Fact]
    public async Task ShouldDecryptAcrossServicesWithSamePassphrase()
    {
        const string plainText = "Cross-service test";

        var services1 = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.Configure<StringEncryptionOptions>(o => o.PassPhrase = "SharedSecret");
        });

        var services2 = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.Configure<StringEncryptionOptions>(o => o.PassPhrase = "SharedSecret");
        });

        var service1 = services1.GetRequiredService<IStringEncryptionService>();
        var service2 = services2.GetRequiredService<IStringEncryptionService>();

        var encrypted = service1.Encrypt(plainText);
        var decrypted = service2.Decrypt(encrypted);

        decrypted.ShouldBe(plainText);
    }
}