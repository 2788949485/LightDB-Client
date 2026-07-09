using DbLiteDesktop.Services;
using Xunit;

namespace DbLiteDesktop.Tests.Services;

public class PasswordEncryptServiceTests
{
    private readonly PasswordEncryptService _service = new();

    [Fact]
    public void Encrypt_ThenDecrypt_ReturnsOriginalText()
    {
        const string input = "secret-password";

        var encrypted = _service.Encrypt(input);
        var decrypted = _service.Decrypt(encrypted);

        Assert.NotEqual(input, encrypted);
        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void Encrypt_EmptyString_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, _service.Encrypt(string.Empty));
    }
}
