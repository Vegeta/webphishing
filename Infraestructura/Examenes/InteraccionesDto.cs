namespace Infraestructura.Examenes;

/// <summary>
/// estuctura para hacer parse de las interacciones del usuario en la UI
/// </summary>
public class InteraccionesDto : List<FilaInter> {
	public Dictionary<string, int>? ClickLinks { get; set; }
	public Dictionary<string, decimal>? HoverLinks { get; set; }
	public Dictionary<string, int>? ClickFiles { get; set; }
	public Dictionary<string, decimal>? HoverFiles { get; set; }

	public static InteraccionesDto Parse(string? json) {
		if (string.IsNullOrEmpty(json))
			return new InteraccionesDto();
		return JSON.Parse<InteraccionesDto>(json);
	}
}