using System.Security.Cryptography;
using System.Text;

namespace FeatureFlags.Application.Common.Utils;

public static class HashingUtils
{
    /// <summary>
    /// Computes a deterministic rollout percentage (0-100) based on an identifier and a feature flag key.
    /// This ensures consistent hashing: the same user always gets the same flag value for the same flag.
    /// </summary>
    public static int GetRolloutPercentage(string identifier, string featureFlagKey)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return 0;

        // Combine identifier and flag key to ensure different users get different hashes per flag
        var input = $"{identifier}:{featureFlagKey}";
        
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        
        // Convert the first 4 bytes of MD5 hash to an unsigned integer
        var hashValue = BitConverter.ToUInt32(hashBytes, 0);
        
        // Modulo 100 to get a value between 0 and 99, then +1 to get 1 to 100.
        return (int)(hashValue % 100) + 1;
    }
}
