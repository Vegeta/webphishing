namespace Infraestructura;

public class PasswordHash {

	public string Hash(string password) {
		return BCrypt.Net.BCrypt.HashPassword(password, 10, BCrypt.Net.SaltRevision.Revision2B);
	}

	public bool Verify(string hash, string password) {
		return BCrypt.Net.BCrypt.Verify(password, hash);
	}

}