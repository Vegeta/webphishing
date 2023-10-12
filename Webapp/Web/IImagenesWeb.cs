namespace Webapp.Web;

public interface IImagenesWeb {
	string BasePath { get; }

	void Delete(string nombre);
	bool FileExists(string nombre);
	void SaveFile(FileStream f);
	void SaveUpload(IFormFile formFile);
	public IList<string> GetFiles();

	public IList<LineaArchivo> FileDetails();

	public string RealPath(string nombre);
}

public class LineaArchivo {
	public string Nombre { get; set; } = "";
	public string Size { get; set; } = "";
}