using Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace Application.Contracts
{
    public interface IFileService
    {
        IEnumerable<Domain.Entities.File> GetEntityFiles(string Entity, Guid EntityId);
        IEnumerable<Domain.Entities.File> GetEntityDocuments(string Entity, Guid EntityId);
        IEnumerable<Domain.Entities.File> GetEntityImages(string Entity, Guid EntityId);
        Task<GenericResponse> UploadFile(IFormFile file, string entity, Guid id);
        Task<GenericResponse> RemoveEntityFiles(string Entity, Guid EntityId);
        Task<GenericResponse> RemoveFile(Guid id);
    }
}
