namespace Webapp.Web;

public class ImagenesEjercicios : IImagenesEjercicios {

	string basePath;

	public string BasePath => basePath;

	public ImagenesEjercicios(string basePath) {
		this.basePath = basePath;
	}

	public bool FileExists(string nombre) {
		var filePath = Path.Combine(basePath, nombre);
		return File.Exists(filePath);
	}
	public void Delete(string nombre) {
		var filePath = Path.Combine(basePath, nombre);
		File.Delete(filePath);
	}

	public void SaveUpload(IFormFile formFile) {
		var filePath = Path.Combine(basePath, formFile.FileName);

		using (var stream = new FileStream(filePath, FileMode.Create)) {
			formFile.CopyTo(stream);
		}
	}

	public async void SaveFile(FileStream f) {
		var nombre = Path.GetFileName(f.Name);
		var filePath = Path.Combine(basePath, nombre);

		using (var stream = new FileStream(filePath, FileMode.Create)) {
			await f.CopyToAsync(stream);
		}
	}

	public IList<string> GetFiles() {
		var list = Directory.GetFiles(basePath).ToList();
		return list.Select(x => Path.GetFileName(x)).ToList();

	}
}

