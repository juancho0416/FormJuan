var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
Directory.CreateDirectory(uploads);
var safeFileName = Path.GetFileName(Archivo.FileName);
var filePath = Path.Combine(uploads, safeFileName);

using (var stream = System.IO.File.Create(filePath))
{
    await Archivo.CopyToAsync(stream);
}

TempData["UploadMessage"] = "Archivo subido correctamente.";
TempData["ShowUploadModal"] = true;
return Page();