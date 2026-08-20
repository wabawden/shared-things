using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace SharedThings.Api.Security;

public static class InvitationTokens
{
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}