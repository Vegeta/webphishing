using System.Security.Cryptography;

namespace Infraestructura;

public class PasswordHash {

	public string Hash(string password) {
		return BCrypt.Net.BCrypt.HashPassword(password, 10, BCrypt.Net.SaltRevision.Revision2B);
	}

	public bool Verify(string hash, string password) {
		return BCrypt.Net.BCrypt.Verify(password, hash);
	}

	/// <summary>
	/// Creates a cryptographically secure random key string.
	/// </summary>
	/// <param name="count">The number of bytes of random values to create the string from</param>
	/// <returns>A secure random string</returns>
	public static string CreateSecureRandomString(int count = 32) =>
		Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));

}