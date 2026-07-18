using Application.Contracts;
using Application.Services;
using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.System
{
    public class UserTableViewService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IUserTableViewService
    {
        private const string DefaultViewName = "Per defecte";

        public async Task<IEnumerable<UserTableView>> GetByUserAndPage(Guid userId, string page)
        {
            var views = unitOfWork.UserTableViews.Find(v => v.UserId == userId && v.Page == page);
            return views;
        }

        public async Task<UserTableView?> GetById(Guid id)
        {
            return await unitOfWork.UserTableViews.Get(id);
        }

        public async Task<GenericResponse> Create(UserTableView userTableView)
        {
            // Check if view with same UserId + Page + Name already exists
            var exists = unitOfWork.UserTableViews.Find(v =>
                v.UserId == userTableView.UserId &&
                v.Page == userTableView.Page &&
                v.Name == userTableView.Name).Any();

            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("TableViewNameExists"));
            }

            // Always create with IsDefault = false initially
            userTableView.IsDefault = false;

            await unitOfWork.UserTableViews.Add(userTableView);
            return new GenericResponse(true, userTableView);
        }

        public async Task<GenericResponse> Update(Guid id, UserTableView userTableView)
        {
            var existing = await unitOfWork.UserTableViews.Get(id);
            if (existing == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            // Check if another view with same UserId + Page + Name exists
            var duplicate = unitOfWork.UserTableViews.Find(v =>
                v.UserId == existing.UserId &&
                v.Page == existing.Page &&
                v.Name == userTableView.Name &&
                v.Id != id).Any();

            if (duplicate)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("TableViewNameExists"));
            }

            // Update allowed fields
            existing.Name = userTableView.Name;
            existing.ViewConfig = userTableView.ViewConfig;

            await unitOfWork.UserTableViews.Update(existing);
            return new GenericResponse(true, existing);
        }

        public async Task<GenericResponse> Delete(Guid id)
        {
            var existing = await unitOfWork.UserTableViews.Get(id);
            if (existing == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.UserTableViews.Remove(existing);
            return new GenericResponse(true, existing);
        }

        public async Task<GenericResponse> SetDefault(Guid id, bool isDefault)
        {
            var existing = await unitOfWork.UserTableViews.Get(id);
            if (existing == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            if (isDefault)
            {
                // Unset all other defaults for the same user + page
                var currentDefaults = unitOfWork.UserTableViews.Find(v =>
                    v.UserId == existing.UserId &&
                    v.Page == existing.Page &&
                    v.IsDefault);

                foreach (var view in currentDefaults)
                {
                    view.IsDefault = false;
                    await unitOfWork.UserTableViews.Update(view);
                }

                // Set this view as default
                existing.IsDefault = true;
            }
            else
            {
                // Unset this view as default
                existing.IsDefault = false;
            }

            await unitOfWork.UserTableViews.Update(existing);
            return new GenericResponse(true, existing);
        }

        public async Task<GenericResponse> EnsureDefault(EnsureDefaultRequest request)
        {
            var existingDefault = unitOfWork.UserTableViews
                .Find(v => v.UserId == request.UserId &&
                           v.Page == request.Page &&
                           v.IsDefault)
                .FirstOrDefault();

            if (existingDefault != null)
                return new GenericResponse(true, existingDefault);

            var anyView = unitOfWork.UserTableViews
                .Find(v => v.UserId == request.UserId &&
                           v.Page == request.Page)
                .Any();

            if (anyView)
            {
                return new GenericResponse(true, (object?)null);
            }

            var newDefault = new UserTableView
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Page = request.Page,
                Name = DefaultViewName,
                IsDefault = true,
                ViewConfig = "{\"columns\":[]}",
            };

            try
            {
                await unitOfWork.UserTableViews.Add(newDefault);
                return new GenericResponse(true, newDefault);
            }
            catch (DbUpdateException)
            {
                var persistedDefault = unitOfWork.UserTableViews
                    .Find(v => v.UserId == request.UserId &&
                               v.Page == request.Page &&
                               v.IsDefault)
                    .FirstOrDefault();

                if (persistedDefault != null)
                    return new GenericResponse(true, persistedDefault);

                var any = unitOfWork.UserTableViews
                    .Find(v => v.UserId == request.UserId &&
                               v.Page == request.Page)
                    .Any();

                if (any)
                    return new GenericResponse(true, (object?)null);

                throw;
            }
        }
    }
}