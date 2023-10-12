namespace Webapp.Web;

public class ImagenesWeb : IImagenesWeb {
	public string BasePath { get; }

	public ImagenesWeb(string basePath) {
		BasePath = basePath;
	}

	public bool FileExists(string nombre) {
		var filePath = Path.Combine(BasePath, nombre);
		return File.Exists(filePath);
	}

	public void Delete(string nombre) {
		var filePath = Path.Combine(BasePath, nombre);
		if (File.Exists(filePath))
			File.Delete(filePath);
	}

	public void SaveUpload(IFormFile formFile) {
		var filePath = Path.Combine(BasePath, formFile.FileName);
		using var stream = new FileStream(filePath, FileMode.Create);
		formFile.CopyTo(stream);
	}

	public async void SaveFile(FileStream f) {
		var nombre = Path.GetFileName(f.Name);
		var filePath = Path.Combine(BasePath, nombre);
		await using var stream = new FileStream(filePath, FileMode.Create);
		await f.CopyToAsync(stream);
	}

	public IList<string> GetFiles() {
		var list = Directory.GetFiles(BasePath).ToList();
		return list.Select(x => Path.GetFileName(x)).ToList();
	}

	public string RealPath(string nombre) {
		return Path.Combine(BasePath, nombre);
	}

	public IList<LineaArchivo> FileDetails() {
		var list = Directory.GetFiles(BasePath).ToList();
		return list
			.Where(x => !x.EndsWith(".gitignore"))
			.Select(x => {
				var info = new FileInfo(x);
				var f = new LineaArchivo {
					Nombre = Path.GetFileName(x),
					Size = FormatFileSize(info.Length)
				};
				return f;
			}).ToList();
	}

	public static string FormatFileSize(long fileSize) {
		string[] suffix = { "bytes", "KB", "MB", "GB" };
		long j = 0;

		while (fileSize > 1024 && j < 4) {
			fileSize = fileSize / 1024;
			j++;
		}
		return (fileSize + " " + suffix[j]);
	}
}