using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services.Production;

public class BrandingService(
    IUnitOfWork unitOfWork,
    IOptions<AppSettings> settings,
    ILocalizationService localizationService,
    ILogger<BrandingService> logger) : IBrandingService
{
    private const long MaxLogoSize = 2 * 1024 * 1024;
    private const string LegacyBrandingEntity = "EnterpriseBranding";
    private const string MainBrandingEntity = "EnterpriseBranding:main";
    private const string SidebarBrandingEntity = "EnterpriseBranding:sidebar";

    public async Task<BrandingResponse> GetCurrent()
    {
        var enterprise = await GetCurrentEnterprise();
        if (enterprise is null)
            return BrandingResponse.Default;

        var mainLogo = await GetValidLogoFile(enterprise, enterprise.LogoMainFileId, BrandingLogoSlot.Main);
        var sidebarLogo = await GetValidLogoFile(enterprise, enterprise.LogoSidebarFileId, BrandingLogoSlot.Sidebar);

        return new BrandingResponse(
            string.IsNullOrWhiteSpace(enterprise.BrandName) ? "Temges" : enterprise.BrandName.Trim(),
            BrandingPalette.NormalizeOrDefault(enterprise.PrimaryColor),
            mainLogo is not null,
            sidebarLogo is not null,
            GetVersion(enterprise),
            mainLogo?.Id.ToString("N"),
            sidebarLogo?.Id.ToString("N"));
    }

    public async Task<BrandingLogoContent?> GetCurrentLogo(BrandingLogoSlot slot)
    {
        var enterprise = await GetCurrentEnterprise();
        if (enterprise is null)
            return null;

        var file = await GetValidLogoFile(enterprise, GetLogoFileId(enterprise, slot), slot);
        if (file is null)
            return null;

        return new BrandingLogoContent(
            new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read),
            GetContentType(file.Path),
            file.UpdatedOn == default ? file.CreatedOn : file.UpdatedOn);
    }

    public async Task<GenericResponse> UpdateCurrent(BrandingUpdateRequest request)
    {
        var enterprise = await GetCurrentEnterprise();
        if (enterprise is null)
            return BrandingEnterpriseUnavailable();

        var brandName = NormalizeBrandName(request.BrandName);
        if (!BrandingPalette.TryNormalize(request.PrimaryColor, out var primaryColor))
            return new GenericResponse(false, localizationService.GetLocalizedString("BrandingPaletteInvalid"));

        var validation = ValidateBranding(brandName);
        if (validation is not null)
            return validation;

        enterprise.BrandName = brandName;
        enterprise.PrimaryColor = primaryColor;
        await unitOfWork.Enterprises.Update(enterprise);

        return new GenericResponse(true, await GetCurrent());
    }

    public async Task<GenericResponse> UploadCurrentLogo(BrandingLogoSlot slot, IFormFile? file)
    {
        var enterprise = await GetCurrentEnterprise();
        return enterprise is null
            ? BrandingEnterpriseUnavailable()
            : await UploadLogo(enterprise.Id, slot, file);
    }

    public async Task<GenericResponse> RemoveCurrentLogo(BrandingLogoSlot slot)
    {
        var enterprise = await GetCurrentEnterprise();
        return enterprise is null
            ? BrandingEnterpriseUnavailable()
            : await RemoveLogo(enterprise.Id, slot);
    }

    public async Task<GenericResponse> UploadLogo(Guid enterpriseId, BrandingLogoSlot slot, IFormFile? file)
    {
        var enterprise = await unitOfWork.Enterprises.Get(enterpriseId);
        if (enterprise is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", enterpriseId));

        if (file is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("BrandingLogoRequired"));

        var validation = await ValidateLogo(file);
        if (!validation.IsValid)
            return new GenericResponse(false, localizationService.GetLocalizedString(validation.ErrorKey!));

        var directory = Path.Combine(settings.Value.FileManagment.UploadPath, LegacyBrandingEntity);
        Directory.CreateDirectory(directory);

        var dbFile = new Domain.Entities.File
        {
            Entity = GetBrandingEntity(slot),
            EntityId = enterpriseId,
            Type = Domain.Entities.FileType.Image,
            Size = file.Length,
            OriginalName = Path.GetFileName(file.FileName),
            Path = Path.Combine(directory, $"{Guid.NewGuid():N}{validation.Extension}")
        };

        var databaseCommitted = false;
        try
        {
            await using (var output = new FileStream(dbFile.Path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(output);
            }

            var previousFileId = GetLogoFileId(enterprise, slot);
            await unitOfWork.Files.AddWithoutSave(dbFile);
            SetLogoFileId(enterprise, slot, dbFile.Id);
            if (!unitOfWork.Enterprises.UpdateWithoutSave(enterprise))
                throw new InvalidOperationException("Unable to stage the Enterprise branding update");

            await unitOfWork.CompleteAsync();
            databaseCommitted = true;

            if (previousFileId.HasValue &&
                previousFileId.Value != dbFile.Id &&
                await IsBrandingFile(previousFileId.Value, enterpriseId, slot))
                await RemoveStoredFileBestEffort(previousFileId.Value);

            try
            {
                return new GenericResponse(true, await GetCurrent());
            }
            catch (Exception responseException)
            {
                logger.LogWarning(responseException,
                    "Branding logo committed for Enterprise {EnterpriseId}, but the response could not be built",
                    enterpriseId);
                return new GenericResponse(true);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to store {Slot} branding logo for Enterprise {EnterpriseId}", slot, enterpriseId);
            if (!databaseCommitted && global::System.IO.File.Exists(dbFile.Path))
            {
                try
                {
                    global::System.IO.File.Delete(dbFile.Path);
                }
                catch (Exception cleanupException)
                {
                    logger.LogWarning(cleanupException, "Unable to remove failed branding file {FilePath}", dbFile.Path);
                }
            }

            return new GenericResponse(false, localizationService.GetLocalizedString("BrandingLogoUploadFailed"));
        }
    }

    public async Task<GenericResponse> RemoveLogo(Guid enterpriseId, BrandingLogoSlot slot)
    {
        var enterprise = await unitOfWork.Enterprises.Get(enterpriseId);
        if (enterprise is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", enterpriseId));

        var fileId = GetLogoFileId(enterprise, slot);
        if (!fileId.HasValue)
            return new GenericResponse(true, await GetCurrent());

        var file = await unitOfWork.Files.Get(fileId.Value);
        SetLogoFileId(enterprise, slot, null);
        await unitOfWork.Enterprises.Update(enterprise);
        if (file is not null && IsBrandingFile(file, enterprise.Id, slot))
            await RemoveStoredFileBestEffort(fileId.Value);

        return new GenericResponse(true, await GetCurrent());
    }

    public async Task<GenericResponse> RemoveEnterpriseFiles(Guid enterpriseId)
    {
        var files = await unitOfWork.Files.FindAsync(file =>
            file.EntityId == enterpriseId &&
            (file.Entity == LegacyBrandingEntity ||
             file.Entity == MainBrandingEntity ||
             file.Entity == SidebarBrandingEntity));

        foreach (var file in files)
            await RemoveStoredFileBestEffort(file.Id);

        return new GenericResponse(true);
    }

    private async Task<Enterprise?> GetCurrentEnterprise()
    {
        var enabled = await unitOfWork.Enterprises.FindAsync(e => !e.Disabled);
        if (enabled.Count == 0)
            return null;

        if (enabled.Count > 1)
        {
            logger.LogError("Branding cannot be resolved because {Count} Enterprises are enabled", enabled.Count);
            return null;
        }

        return enabled[0];
    }

    private async Task<Domain.Entities.File?> GetValidLogoFile(
        Enterprise enterprise,
        Guid? fileId,
        BrandingLogoSlot slot)
    {
        if (!fileId.HasValue)
            return null;

        var file = await unitOfWork.Files.Get(fileId.Value);
        return file is not null &&
               IsBrandingFile(file, enterprise.Id, slot) &&
               file.Type == Domain.Entities.FileType.Image &&
               IsInsideUploadRoot(file.Path) &&
               global::System.IO.File.Exists(file.Path)
            ? file
            : null;
    }

    private async Task<(bool IsValid, string? Extension, string? ErrorKey)> ValidateLogo(IFormFile file)
    {
        if (file.Length <= 0 || file.Length > MaxLogoSize)
            return (false, null, "BrandingLogoInvalidSize");

        var header = new byte[12];
        await using var input = file.OpenReadStream();
        var bytesRead = await input.ReadAsync(header.AsMemory(0, header.Length));

        if (bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return IsAcceptedMime(file.ContentType, "image/png") && IsAcceptedExtension(file.FileName, ".png")
                ? (true, ".png", null)
                : (false, null, "BrandingLogoInvalidFormat");

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return IsAcceptedMime(file.ContentType, "image/jpeg") && IsAcceptedExtension(file.FileName, ".jpg")
                ? (true, ".jpg", null)
                : (false, null, "BrandingLogoInvalidFormat");

        var isWebP = bytesRead >= 12 &&
                     header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                     header.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        if (isWebP)
            return IsAcceptedMime(file.ContentType, "image/webp") && IsAcceptedExtension(file.FileName, ".webp")
                ? (true, ".webp", null)
                : (false, null, "BrandingLogoInvalidFormat");

        return (false, null, "BrandingLogoInvalidFormat");
    }

    private GenericResponse? ValidateBranding(string? brandName)
    {
        if (brandName?.Length > 60)
            return new GenericResponse(false, localizationService.GetLocalizedString("BrandingNameInvalid"));

        return null;
    }

    private GenericResponse BrandingEnterpriseUnavailable() =>
        new(false, localizationService.GetLocalizedString("BrandingEnterpriseUnavailable"));

    private static string? NormalizeBrandName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsAcceptedExtension(string fileName, string expectedExtension)
    {
        var extension = Path.GetExtension(fileName);
        return expectedExtension == ".jpg"
            ? string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
              string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
            : string.Equals(extension, expectedExtension, StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsAcceptedMime(string? actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

    private bool IsInsideUploadRoot(string path)
    {
        var root = Path.GetFullPath(settings.Value.FileManagment.UploadPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(path);
        return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    private async Task RemoveStoredFileBestEffort(Guid fileId)
    {
        try
        {
            var file = await unitOfWork.Files.Get(fileId);
            if (file is null)
                return;

            if (IsInsideUploadRoot(file.Path) && global::System.IO.File.Exists(file.Path))
                global::System.IO.File.Delete(file.Path);

            await unitOfWork.Files.Remove(file);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to remove replaced branding file {FileId}", fileId);
        }
    }

    private static Guid? GetLogoFileId(Enterprise enterprise, BrandingLogoSlot slot) =>
        slot == BrandingLogoSlot.Main ? enterprise.LogoMainFileId : enterprise.LogoSidebarFileId;

    private static string GetBrandingEntity(BrandingLogoSlot slot) =>
        slot == BrandingLogoSlot.Main ? MainBrandingEntity : SidebarBrandingEntity;

    private static bool IsBrandingFile(
        Domain.Entities.File file,
        Guid enterpriseId,
        BrandingLogoSlot slot) =>
        file.EntityId == enterpriseId &&
        (file.Entity == GetBrandingEntity(slot) ||
         slot == BrandingLogoSlot.Main && file.Entity == LegacyBrandingEntity);

    private async Task<bool> IsBrandingFile(Guid fileId, Guid enterpriseId, BrandingLogoSlot slot)
    {
        var file = await unitOfWork.Files.Get(fileId);
        return file is not null && IsBrandingFile(file, enterpriseId, slot);
    }

    private static void SetLogoFileId(Enterprise enterprise, BrandingLogoSlot slot, Guid? fileId)
    {
        if (slot == BrandingLogoSlot.Main)
            enterprise.LogoMainFileId = fileId;
        else
            enterprise.LogoSidebarFileId = fileId;
    }

    private static string GetVersion(Enterprise enterprise)
    {
        var timestamp = enterprise.UpdatedOn == default ? enterprise.CreatedOn : enterprise.UpdatedOn;
        return timestamp == default ? enterprise.Id.ToString("N") : timestamp.ToUniversalTime().ToString("O");
    }

    private static string GetContentType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
}
