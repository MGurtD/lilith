using Application.Contracts;
using Application.Services;
using Domain.Entities.Auth;

namespace Application.Services.System
{
    public class RoleService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IRoleService
    {
        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            var roles = await unitOfWork.Roles.GetAll();
            return roles.OrderBy(r => r.Name);
        }

        public async Task<Role?> GetRoleById(Guid id)
        {
            return await unitOfWork.Roles.Get(id);
        }

        public async Task<GenericResponse> CreateRole(Role role)
        {
            // Validate name uniqueness
            var exists = unitOfWork.Roles.Find(r => r.Name == role.Name).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            // Persist
            await unitOfWork.Roles.Add(role);
            return new GenericResponse(true, role);
        }

        public async Task<GenericResponse> UpdateRole(Role role)
        {
            // Check if role exists
            var existing = await unitOfWork.Roles.Get(role.Id);
            if (existing == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", role.Id));
            }

            // Update
            await unitOfWork.Roles.Update(role);
            return new GenericResponse(true, role);
        }

        public async Task<GenericResponse> RemoveRole(Guid id)
        {
            var role = await unitOfWork.Roles.Get(id);
            if (role == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.Roles.Remove(role);
            return new GenericResponse(true, role);
        }
    }
}
