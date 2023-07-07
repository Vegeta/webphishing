namespace Webapp.Web;

public static class HttpRequestExtensions {
	private const string RequestedWithHeader = "X-Requested-With";
	private const string XmlHttpRequest = "XMLHttpRequest";

	public static bool IsAjaxRequest(this HttpRequest request) {
		if (request == null)
			throw new ArgumentNullException(nameof(request));
		return request.Headers.Any() && request.Headers[RequestedWithHeader] == XmlHttpRequest;
	}
}