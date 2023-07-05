namespace Webapp.Web {
	public interface IImagenesEjercicios {
		string BasePath { get; }

		void Delete(string nombre);
		bool FileExists(string nombre);
		void SaveFile(FileStream f);
		void SaveUpload(IFormFile formFile);
		IList<string> GetFiles();
	}
}