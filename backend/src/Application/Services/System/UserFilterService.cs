using Application.Contracts;
using Application.Services;
using Domain.Entities.Auth;

namespace Application.Services.System
{
    public class UserFilterService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IUserFilterService
    {
        public async Task<IEnumerable<UserFilter>> GetUserFiltersByUserId(Guid userId)
        {
            var filters = unitOfWork.UserFilters.Find(f => f.UserId == userId);
            return filters;
        }

        public async Task<UserFilter?> GetUserFilterByUserIdPageKey(Guid userId, string page, string key)
        {
            var filter = unitOfWork.UserFilters.Find(f => f.UserId == userId && f.Page == page && f.Key == key)
                .FirstOrDefault();
            return filter;
        }

        public async Task<GenericResponse> CreateUserFilter(UserFilter userFilter)
        {
            // Check if filter already exists
            var exists = unitOfWork.UserFilters.Find(f => 
                f.UserId == userFilter.UserId && 
                f.Page == userFilter.Page && 
                f.Key == userFilter.Key).Any();
            
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            // Persist
            await unitOfWork.UserFilters.Add(userFilter);
            return new GenericResponse(true, userFilter);
        }

        public async Task<GenericResponse> UpdateUserFilter(UserFilter userFilter)
        {
            // Check if filter exists
            var existing = unitOfWork.UserFilters.Find(f => 
                f.UserId == userFilter.UserId && 
                f.Page == userFilter.Page && 
                f.Key == userFilter.Key).FirstOrDefault();
            
            if (existing == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", userFilter.Id));
            }

            // Update filter value
            existing.Filter = userFilter.Filter;
            await unitOfWork.UserFilters.Update(existing);
            return new GenericResponse(true, existing);
        }

        public async Task<GenericResponse> RemoveUserFilter(Guid id)
        {
            var filter = await unitOfWork.UserFilters.Get(id);
            if (filter == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.UserFilters.Remove(filter);
            return new GenericResponse(true, filter);
        }
    }
}
