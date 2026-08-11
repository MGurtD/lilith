using Application.Contracts;
namespace Application.Contracts;

public interface IMenuItemService
{
    Task<GenericResponse> GetAll(bool hierarchy = false);
    Task<GenericResponse> Get(Guid id);
    Task<GenericResponse> Create(CreateMenuItemRequest request);
    Task<GenericResponse> Update(UpdateMenuItemRequest request);
    Task<GenericResponse> GetTranslationMatrix();
    Task<GenericResponse> UpdateTranslations(UpdateMenuItemTranslationsRequest request);
    Task<GenericResponse> Delete(Guid id);
}
