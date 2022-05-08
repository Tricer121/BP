using System.Security.Cryptography;
using System.Text;

namespace backend.Common;

public class HashService
{
    public byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
        {
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
    }

    public string GetHashString(string inputString)
    {
        var sb = new StringBuilder();
        foreach (var b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}