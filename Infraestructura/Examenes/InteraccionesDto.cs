namespace Infraestructura.Examenes;

/// <summary>
/// estuctura para hacer parse de las interacciones del usuario en la UI
/// </summary>
public class InteraccionesDto {
	public Dictionary<string, int>? ClickLinks { get; set; }
	public Dictionary<string, decimal>? HoverLinks { get; set; }
	public Dictionary<string, int>? ClickFiles { get; set; }
	public Dictionary<string, decimal>? HoverFiles { get; set; }

	public static InteraccionesDto Parse(string? json) {
		if (string.IsNullOrEmpty(json))
			return new InteraccionesDto();
		var item = JSON.Parse<InteraccionesDto>(json);
		if (item.ClickLinks != null && item.ClickLinks.Count == 0)
			item.ClickLinks = null;
		if (item.HoverLinks != null && item.HoverLinks.Count == 0)
			item.HoverLinks = null;
		if (item.ClickFiles != null && item.ClickFiles.Count == 0)
			item.ClickFiles = null;
		if (item.HoverFiles != null && item.HoverFiles.Count == 0)
			item.HoverFiles = null;
		return JSON.Parse<InteraccionesDto>(json);
	}
}