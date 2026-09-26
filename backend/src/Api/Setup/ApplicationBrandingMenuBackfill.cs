using Application.Contracts;
using Domain.Entities.Auth;

namespace Api.Setup;

public sealed class ApplicationBrandingMenuBackfill(
    IServiceProvider serviceProvider,
    ILogger<ApplicationBrandingMenuBackfill> logger) : IHostedService
{
    private const string DefaultProfileName = "default";
    private const string SuperuserProfileName = "superuser";
    private const string RootKey = "system";
    private const string ItemKey = "application_branding";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var languageCatalog = scope.ServiceProvider.GetRequiredService<ILanguageCatalog>();

        try
        {
            var menuItems = (await unitOfWork.MenuItems.GetAll()).ToDictionary(item => item.Key);
            var root = await EnsureMenuItem(unitOfWork, menuItems, RootKey, "Sistema", "pi pi-cog", null, null);
            var branding = await EnsureMenuItem(
                unitOfWork,
                menuItems,
                ItemKey,
                "Branding",
                "pi pi-palette",
                "/system/application-branding",
                root.Id);
            var languages = await languageCatalog.GetAllAsync();
            await EnsureTranslations(unitOfWork, root.Id, languages, "Sistema", "Sistema", "System");
            await EnsureTranslations(unitOfWork, branding.Id, languages, "Branding", "Marca", "Branding");

            var profiles = (await unitOfWork.Profiles.GetAll()).ToDictionary(profile => profile.Name);
            var defaultProfile = await EnsureProfile(unitOfWork, profiles, DefaultProfileName, "System default profile");
            var superuserProfile = await EnsureProfile(unitOfWork, profiles, SuperuserProfileName, "Superuser profile with full access");

            await EnsureAssignment(unitOfWork, defaultProfile, root);
            await EnsureAssignment(unitOfWork, defaultProfile, branding);
            await EnsureAssignment(unitOfWork, superuserProfile, root);
            await EnsureAssignment(unitOfWork, superuserProfile, branding);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to backfill the application branding menu");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task<MenuItem> EnsureMenuItem(
        IUnitOfWork unitOfWork,
        IDictionary<string, MenuItem> menuItems,
        string key,
        string title,
        string icon,
        string? route,
        Guid? parentId)
    {
        if (menuItems.TryGetValue(key, out var existing))
            return existing;

        var item = new MenuItem
        {
            Key = key,
            Icon = icon,
            Route = route,
            ParentId = parentId,
            SortOrder = menuItems.Count,
        };

        await unitOfWork.MenuItems.Add(item);
        menuItems[key] = item;
        return item;
    }

    private static async Task EnsureTranslations(
        IUnitOfWork unitOfWork,
        Guid menuItemId,
        IEnumerable<LanguageDto> languages,
        string catalan,
        string spanish,
        string english)
    {
        var existingCodes = (await unitOfWork.MenuItemTranslations.FindAsync(t => t.MenuItemId == menuItemId))
            .Select(t => t.LanguageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var language in languages.Where(l => !existingCodes.Contains(l.Code)))
        {
            var title = language.Code.ToLowerInvariant() switch
            {
                "es" => spanish,
                "en" => english,
                _ => catalan
            };
            await unitOfWork.MenuItemTranslations.Add(new MenuItemTranslation
            {
                MenuItemId = menuItemId,
                LanguageCode = language.Code.ToLowerInvariant(),
                Title = title
            });
        }
    }

    private static async Task<Profile> EnsureProfile(
        IUnitOfWork unitOfWork,
        IDictionary<string, Profile> profiles,
        string name,
        string description)
    {
        if (profiles.TryGetValue(name, out var existing))
            return existing;

        var profile = new Profile { Name = name, Description = description, IsSystem = true };
        await unitOfWork.Profiles.Add(profile);
        profiles[name] = profile;
        return profile;
    }

    private static async Task EnsureAssignment(IUnitOfWork unitOfWork, Profile profile, MenuItem menuItem)
    {
        var exists = unitOfWork.ProfileMenuItems
            .Find(item => item.ProfileId == profile.Id && item.MenuItemId == menuItem.Id)
            .Any();

        if (!exists)
        {
            await unitOfWork.ProfileMenuItems.Add(new ProfileMenuItem
            {
                ProfileId = profile.Id,
                MenuItemId = menuItem.Id,
                IsDefault = false,
            });
        }
    }
}
