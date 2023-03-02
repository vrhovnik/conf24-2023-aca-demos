using System.Security.Cryptography;
using System.Text;

namespace ITS.Core;

public class HashGenerator
{
    public static string GetHash(string toHash, int lengthOfResult)
    {
        using var md5Hasher = MD5.Create();
        var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(toHash));
        return BitConverter.ToString(data).Replace("-", "")[..lengthOfResult];
    }

    public static string GetHash(string toHash, int lengthOfResult, string prefix) => $"{prefix}{GetHash(toHash, lengthOfResult)}";
}