using Application.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace Application.Services.System
{
    public class FileService(IOptions<AppSettings> settings, IUnitOfWork unitOfWork, ILocalizationService localizationService) : IFileService
    {

        public IEnumerable<object> GetFiles() => Directory.GetFiles(settings.Value.FileManagment.UploadPath);

        public async Task<GenericResponse> UploadFile(IFormFile file, string entity, Guid id)
        {
            try
            {
                if (file.Length > 0 && CreateDirectory(entity))
                {
                    // Crear arxiu al filesystem
                    var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
                    var fileName = $"{id}_{timestamp}{Path.GetExtension(file.FileName)}";
                    var path = Path.Combine(settings.Value.FileManagment.UploadPath, entity, fileName);
                    using var fileStream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(fileStream);

                    // Crear arxiu a la BDD
                    var dbFile = new Domain.Entities.File()
                    {
                        Entity = entity,
                        EntityId = id,
                        Path = path,
                        OriginalName = file.FileName,
                        Size = file.Length,
                        Type = GetTypeFromFormFile(file)
                    };
                    await unitOfWork.Files.Add(dbFile);

                    return new GenericResponse(true, dbFile);
                }
                else
                {
                    return new GenericResponse(false, localizationService.GetLocalizedString("FileUploadDirectoryError", entity));
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse(false, ex.Message);
            }
        }

        public IEnumerable<Domain.Entities.File> GetEntityFiles(string Entity, Guid EntityId)
        {
            var files = unitOfWork.Files.Find(f => f.Entity == Entity && f.EntityId == EntityId).OrderByDescending(f => f.OriginalName);
            return files;
        }

        public IEnumerable<Domain.Entities.File> GetEntityDocuments(string Entity, Guid EntityId)
        {
            var files = unitOfWork.Files.Find(f => f.Entity == Entity && f.EntityId == EntityId && f.Type == (int)FileType.Document);
            return files;
        }

        public IEnumerable<Domain.Entities.File> GetEntityImages(string Entity, Guid EntityId)
        {
            var files = unitOfWork.Files.Find(f => f.Entity == Entity && f.EntityId == EntityId && f.Type == FileType.Image);
            return files;
        }

        public async Task<GenericResponse> RemoveEntityFiles(string Entity, Guid EntityId)
        {
            var files = GetEntityFiles(Entity, EntityId);

            try
            {
                foreach (var file in files)
                {
                    global::System.IO.File.Delete(file.Path);
                }
                await unitOfWork.Files.RemoveRange(files);

                return new GenericResponse(false, files);
            }
            catch (Exception ex)
            {
                return new GenericResponse(false, ex.Message);
            }
        }

        public async Task<GenericResponse> RemoveFile(Guid id)
        {
            try
            {
                var file = await unitOfWork.Files.Get(id);
                if (file is not null)
                {
                    global::System.IO.File.Delete(file.Path);
                    await unitOfWork.Files.Remove(file);
                }

                return new GenericResponse(true, file);
            }
            catch (Exception ex)
            {
                return new GenericResponse(false, ex.Message);
            }
        }

        public static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            
            // Añadir tipos MIME específicos para archivos CAD
            return ext switch
            {
                // Archivos CAD
                ".stp" => "application/step",
                ".step" => "application/step",
                ".dxf" => "application/dxf",
                ".dwg" => "application/acad",
                ".stl" => "application/sla",
                ".iges" => "application/iges",
                ".igs" => "application/iges",
                
                // Para otros archivos, usar el provider estándar
                _ => GetStandardContentType(path)
            };
        }

        private static string GetStandardContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private bool CreateDirectory(string entity)
        {
            try
            {
                string rootPath = Path.Combine(settings.Value.FileManagment.UploadPath, entity);
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private FileType GetTypeFromFormFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    return FileType.Image;
                case ".mp4":
                case ".mov":
                case ".wmv":
                case ".flv":
                    return FileType.Video;
                default:
                    return FileType.Document;
            }
        }
    }
}






