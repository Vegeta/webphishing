using Microsoft.AspNetCore.Authentication;

namespace Webapp.Web;

public class AuthConstants : AuthenticationSchemeOptions {
	public const string SessionName = ".Phishing";
}