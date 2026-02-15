using Application.Contracts;
using Application.Services;
using Domain.Entities.Auth;

namespace Application.Services.System
{

    public class ProfileService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IProfileService
    {
        public async Task<GenericResponse> Create(Profile profile)
        {
            var exists = unitOfWork.Profiles.Find(p => p.Name == profile.Name).Any();
            if (exists)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileNameExists", profile.Name));
            await unitOfWork.Profiles.Add(profile);
            return new GenericResponse(true, profile);
        }

        public async Task<GenericResponse> Update(Profile profile)
        {
            var current = await unitOfWork.Profiles.Get(profile.Id);
            if (current is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileNotFound", profile.Id));
            current.Name = profile.Name;
            current.Description = profile.Description;
            await unitOfWork.Profiles.Update(current);
            return new GenericResponse(true, current);
        }

        public async Task<GenericResponse> Delete(Guid id)
        {
            var profile = await unitOfWork.Profiles.Get(id);
            if (profile is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileNotFound", id));
            if (profile.IsSystem)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileDeleteForbidden"));
            await unitOfWork.Profiles.Remove(profile);
            return new GenericResponse(true, true);
        }

        public async Task<GenericResponse> GetAll()
        {
            var profiles = await unitOfWork.Profiles.GetAll();
            return new GenericResponse(true, profiles.OrderBy(p => p.Name));
        }

        public async Task<GenericResponse> Get(Guid id)
        {
            var profile = await unitOfWork.Profiles.Get(id);
            if (profile is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileNotFound", id));
            return new GenericResponse(true, profile);
        }

        public async Task<GenericResponse> AssignMenu(Guid profileId, IEnumerable<Guid> menuItemIds, Guid? defaultMenuItemId = null)
        {
            var profile = await unitOfWork.Profiles.Get(profileId);
            if (profile is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileNotFound", profileId));

            // Load existing assignments
            var existing = unitOfWork.ProfileMenuItems.Find(p => p.ProfileId == profileId).ToList();
            // Remove those not in incoming set
            foreach (var item in existing.Where(e => !menuItemIds.Contains(e.MenuItemId)))
            {
                await unitOfWork.ProfileMenuItems.Remove(item);
            }
            // Add new ones
            foreach (var id in menuItemIds.Where(id => !existing.Any(e => e.MenuItemId == id)))
            {
                var menuItem = await unitOfWork.MenuItems.Get(id);
                if (menuItem is null) return new GenericResponse(false, localizationService.GetLocalizedString("MenuItemNotFound", id));
                await unitOfWork.ProfileMenuItems.Add(new ProfileMenuItem { ProfileId = profileId, MenuItemId = id, IsDefault = false });
            }

            // Set default
            if (defaultMenuItemId.HasValue)
            {
                if (!menuItemIds.Contains(defaultMenuItemId.Value))
                    return new GenericResponse(false, localizationService.GetLocalizedString("DefaultMenuItemNotInProfile"));
                var assigned = unitOfWork.ProfileMenuItems.Find(p => p.ProfileId == profileId).ToList();
                foreach (var item in assigned)
                {
                    item.IsDefault = item.MenuItemId == defaultMenuItemId.Value;
                    await unitOfWork.ProfileMenuItems.Update(item);
                }
            }

            return new GenericResponse(true, localizationService.GetLocalizedString("ProfileMenuUpdated"));
        }

        public async Task<GenericResponse> GetMenuForProfile(Guid profileId)
        {
            var profile = await unitOfWork.Profiles.Get(profileId);
            if (profile is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileNotFound", profileId));

            var assignments = unitOfWork.ProfileMenuItems.Find(p => p.ProfileId == profileId).ToList();
            var ids = assignments.Select(a => a.MenuItemId).ToList();
            var defaultId = assignments.FirstOrDefault(a => a.IsDefault)?.MenuItemId;
            return new GenericResponse(true, new { menuItemIds = ids, defaultMenuItemId = defaultId });
        }

        public async Task<GenericResponse> GetMenuForUser(Guid userId)
        {
            var user = await unitOfWork.Users.Get(userId);
            if (user is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("UserNotFound"));
            if (user.ProfileId is null)
                return new GenericResponse(true, Enumerable.Empty<object>());

            var assignments = unitOfWork.ProfileMenuItems.Find(p => p.ProfileId == user.ProfileId).ToList();
            var menuIds = assignments.Select(a => a.MenuItemId).ToHashSet();
            var menuItems = unitOfWork.MenuItems.Find(m => menuIds.Contains(m.Id)).OrderBy(m => m.SortOrder).ToList();

            // Build hierarchy
            var dict = menuItems.ToDictionary(m => m.Id, m => new MenuDto(m));
            List<MenuDto> roots = new();
            foreach (var dto in dict.Values)
            {
                if (dto.ParentId.HasValue && dict.ContainsKey(dto.ParentId.Value))
                {
                    dict[dto.ParentId.Value].Children.Add(dto);
                }
                else
                {
                    roots.Add(dto);
                }
            }

            var defaultItem = assignments.FirstOrDefault(a => a.IsDefault)?.MenuItemId;
            return new GenericResponse(true, new { items = roots, defaultScreen = defaultItem });
        }

        public async Task<GenericResponse> SetUserProfile(Guid userId, Guid profileId)
        {
            var user = await unitOfWork.Users.Get(userId);
            if (user is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("UserNotFound"));
            var profile = await unitOfWork.Profiles.Get(profileId);
            if (profile is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("ProfileNotFound", profileId));

            user.ProfileId = profileId;
            await unitOfWork.Users.Update(user);
            return new GenericResponse(true, localizationService.GetLocalizedString("UserProfileAssigned"));
        }

        private record MenuDto
        {
            public Guid Id { get; init; }
            public string Key { get; init; }
            public string Title { get; init; }
            public string? Icon { get; init; }
            public string? Route { get; init; }
            public int SortOrder { get; init; }
            public Guid? ParentId { get; init; }
            public List<MenuDto> Children { get; init; } = new();
            public MenuDto(MenuItem item)
            {
                Id = item.Id; Key = item.Key; Title = item.Title; Icon = item.Icon; Route = item.Route; SortOrder = item.SortOrder; ParentId = item.ParentId;
            }
        }
    }
}





