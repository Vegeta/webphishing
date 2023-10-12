using Infraestructura;
using Infraestructura.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.StaticFiles;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Web;

namespace Webapp.Areas.Manage.Controllers;

public class ImagenesController : BaseAdminController {
	private readonly IImagenesWeb _imagenes;
	private readonly IAuditor<PreguntasController> _logger;

	public ImagenesController(IImagenesWeb imagenes, IAuditor<PreguntasController> logger) {
		_imagenes = imagenes;
		_logger = logger;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Imágenes", "/Manage/Imagenes");
		CurrentMenu("Contenido/Imágenes");
		Titulo("Imágenes cargadas");
	}

	public IActionResult Index() {
		var lista = ListaBase();
		ViewBag.lista = JSON.Stringify(lista);
		return View();
	}

	private IEnumerable<LineaArchivo> ListaBase() {
		return _imagenes.FileDetails().OrderBy(x => x.Nombre);
	}

	public IActionResult Lista() {
		var lista = ListaBase();
		return Ok(lista);
	}

	public IActionResult UploadImage(UploadModel model) {
		if (model.File != null)
			_imagenes.SaveUpload(model.File);
		_logger.Info($"Imagen {model.File!.FileName} cargada");
		var lista = ListaBase();
		return Ok(lista);
	}

	public IActionResult Eliminar(string archivo) {
		_imagenes.Delete(archivo);
		_logger.Info($"Imagen {archivo} eliminada");
		var lista = ListaBase();
		return Ok(lista);
	}

	public IActionResult Download(string nombre) {
		var path = _imagenes.RealPath(nombre);
		if (!System.IO.File.Exists(path)) {
			ErrorWeb("Archivo no existe");
			return RedirectToAction("Index");
		}
		var mime = GetMimeTypeForFileExtension(path);
		var name = Path.GetFileName(path);
		return PhysicalFile(path, mime, name, true);
	}

	public string GetMimeTypeForFileExtension(string filePath) {
		const string defaultContentType = "application/octet-stream";

		var provider = new FileExtensionContentTypeProvider();

		if (!provider.TryGetContentType(filePath, out var contentType)) {
			contentType = defaultContentType;
		}

		return contentType;
	}
}