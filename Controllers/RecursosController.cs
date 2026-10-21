using Aula.Data;
using Aula.Models;
using Aula.Services;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers;

// AJUSTA: AulaDbContext, Recurso, Leccione y los DbSet (Recursos, Lecciones)
// según los nombres de tu scaffold.
[Authorize(Roles = "Administrador,Profesor,Estudiante")]
public class RecursosController : Controller
{
    private static readonly string[] ExtensionesPermitidas =
        { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".zip" };
    private const long TamanoMaximo = 25 * 1024 * 1024; // 25 MB

    private readonly EduLearnDbContext _context;
    private readonly CursoAccesoService _acceso;
    private readonly IWebHostEnvironment _env;

    public RecursosController(EduLearnDbContext context, CursoAccesoService acceso, IWebHostEnvironment env)
    {
        _context = context;
        _acceso = acceso;
        _env = env;
    }

    // Los archivos se guardan FUERA de wwwroot: solo se sirven por Descargar (con permisos)
    private string CarpetaUploads => Path.Combine(_env.ContentRootPath, "Uploads", "recursos");

    // ---------- AGREGAR ----------
    // GET /Recursos/Agregar?leccionId=3
    [HttpGet]
    [Authorize(Roles = "Administrador,Profesor")]
    public async Task<IActionResult> Agregar(int leccionId)
    {
        var leccion = await _context.Lecciones.AsNoTracking().FirstOrDefaultAsync(l => l.Id == leccionId);
        if (leccion == null || !await _acceso.PuedeEditarAsync(User, leccion.CursoId))
            return NotFound();

        return View(new RecursoFormViewModel
        {
            LeccionId = leccionId,
            CursoId = leccion.CursoId,
            LeccionTitulo = leccion.Titulo
        });
    }

    // POST /Recursos/Agregar
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Profesor")]
    [RequestSizeLimit(26_000_000)]
    public async Task<IActionResult> Agregar(RecursoFormViewModel vm)
    {
        var leccion = await _context.Lecciones.AsNoTracking().FirstOrDefaultAsync(l => l.Id == vm.LeccionId);
        if (leccion == null || !await _acceso.PuedeEditarAsync(User, leccion.CursoId))
            return NotFound();

        vm.CursoId = leccion.CursoId;
        vm.LeccionTitulo = leccion.Titulo;

        var extension = "";

        if (vm.Tipo == TipoRecurso.Documento)
        {
            if (vm.Archivo == null || vm.Archivo.Length == 0)
            {
                ModelState.AddModelError(nameof(vm.Archivo), "Selecciona un archivo.");
            }
            else
            {
                extension = Path.GetExtension(vm.Archivo.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(extension))
                    ModelState.AddModelError(nameof(vm.Archivo),
                        $"Tipo no permitido. Usa: {string.Join(", ", ExtensionesPermitidas)}");
                else if (vm.Archivo.Length > TamanoMaximo)
                    ModelState.AddModelError(nameof(vm.Archivo), "El archivo supera los 25 MB.");
            }
        }
        else
        {
            if (!Uri.TryCreate(vm.Url?.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError(nameof(vm.Url), "Ingresa un enlace válido que empiece con http:// o https://");
            }
        }

        if (!ModelState.IsValid) return View(vm);

        string valorUrl;
        string? rutaFisica = null;

        if (vm.Tipo == TipoRecurso.Documento)
        {
            Directory.CreateDirectory(CarpetaUploads);
            valorUrl = $"{Guid.NewGuid():N}{extension}";           // nombre seguro, no el original
            rutaFisica = Path.Combine(CarpetaUploads, valorUrl);

            await using var stream = System.IO.File.Create(rutaFisica);
            await vm.Archivo!.CopyToAsync(stream);
        }
        else
        {
            valorUrl = vm.Url!.Trim();
        }

        try
        {
            _context.Recursos.Add(new Recurso
            {
                LeccionId = vm.LeccionId,
                Nombre = vm.Nombre.Trim(),
                Tipo = (byte)vm.Tipo,
                Url = valorUrl
            });
            await _context.SaveChangesAsync();
        }
        catch
        {
            // si falla la BD, no dejar un archivo huérfano
            if (rutaFisica != null && System.IO.File.Exists(rutaFisica))
                System.IO.File.Delete(rutaFisica);
            throw;
        }

        TempData["Ok"] = "Recurso agregado correctamente.";
        return RedirectToAction("Gestionar", "Curso", new { id = leccion.CursoId });
    }

    // ---------- DESCARGAR ----------
    // GET /Recursos/Descargar/7  (profesor dueño, admin o estudiante inscrito)
    [HttpGet]
    public async Task<IActionResult> Descargar(int id)
    {
        var recurso = await _context.Recursos.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (recurso == null || recurso.Tipo != (byte)TipoRecurso.Documento) return NotFound();

        var leccion = await _context.Lecciones.AsNoTracking().FirstOrDefaultAsync(l => l.Id == recurso.LeccionId);
        if (leccion == null || !await _acceso.PuedeVerAsync(User, leccion.CursoId)) return NotFound();

        var nombreAlmacenado = Path.GetFileName(recurso.Url);       // evita path traversal
        var ruta = Path.Combine(CarpetaUploads, nombreAlmacenado);
        if (!System.IO.File.Exists(ruta)) return NotFound();

        new FileExtensionContentTypeProvider().TryGetContentType(nombreAlmacenado, out var contentType);

        var ext = Path.GetExtension(nombreAlmacenado);
        var nombreDescarga = recurso.Nombre.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
            ? recurso.Nombre
            : recurso.Nombre + ext;

        return PhysicalFile(ruta, contentType ?? "application/octet-stream", nombreDescarga);
    }

    // ---------- ELIMINAR ----------
    // POST /Recursos/Eliminar/7
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Profesor")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var recurso = await _context.Recursos.FirstOrDefaultAsync(r => r.Id == id);
        if (recurso == null) return NotFound();

        var leccion = await _context.Lecciones.AsNoTracking().FirstOrDefaultAsync(l => l.Id == recurso.LeccionId);
        if (leccion == null || !await _acceso.PuedeEditarAsync(User, leccion.CursoId)) return NotFound();

        if (recurso.Tipo == (byte)TipoRecurso.Documento)
        {
            var ruta = Path.Combine(CarpetaUploads, Path.GetFileName(recurso.Url));
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }

        _context.Recursos.Remove(recurso);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Recurso eliminado.";
        return RedirectToAction("Gestionar", "Curso", new { id = leccion.CursoId });
    }
}