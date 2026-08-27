using Application.Contracts;
using Application.Services.Production;
using Application.Services.System;
using Application.Tests.TestData;
using Application.Tests.TestSupport;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using SystemFile = System.IO.File;

namespace Application.Tests.Services.System;

/// <summary>
/// Unit tests for <see cref="BrandingService"/> and its interaction with
/// <see cref="EnterpriseService"/>.
/// </summary>
public class BrandingServiceTests
{
    private static readonly byte[] PngHeader = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public async Task GetCurrent_returns_branding_from_the_single_enabled_enterprise()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            enterprise.BrandName = "  Acme  ";
            enterprise.PrimaryColor = "  InDiGo  ";
            var sut = BuildSut(new BrandingTestContext(enterprise), root);

            var response = await sut.GetCurrent();

            Assert.Equal("Acme", response.BrandName);
            Assert.Equal(BrandingPalette.Indigo, response.PrimaryColor);
            Assert.Null(response.MainLogoVersion);
            Assert.Null(response.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UpdateCurrent_normalizes_allowed_palette_keys_and_validates_brand_name()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var sut = BuildSut(new BrandingTestContext(enterprise), root);

            var invalid = await sut.UpdateCurrent(new BrandingUpdateRequest("A".PadRight(61, 'x'), "blue"));
            var valid   = await sut.UpdateCurrent(new BrandingUpdateRequest("  Acme  ", "  TeAL  "));

            Assert.False(invalid.Result);
            Assert.True(valid.Result);
            Assert.Equal("Acme", enterprise.BrandName);
            Assert.Equal(BrandingPalette.Teal, enterprise.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [InlineData("BLACK",   BrandingPalette.Black)]
    [InlineData("BLUE",    BrandingPalette.Blue)]
    [InlineData("indigo",  BrandingPalette.Indigo)]
    [InlineData("EMERALD", BrandingPalette.Emerald)]
    [InlineData("teal",    BrandingPalette.Teal)]
    [InlineData("VIOLET",  BrandingPalette.Violet)]
    [InlineData("orange",  BrandingPalette.Orange)]
    [InlineData("ROSE",    BrandingPalette.Rose)]
    public async Task UpdateCurrent_accepts_and_normalizes_each_allowed_palette_key(
        string primaryColor,
        string expectedPalette)
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var sut = BuildSut(new BrandingTestContext(enterprise), root);

            var response = await sut.UpdateCurrent(new BrandingUpdateRequest("Acme", primaryColor));

            Assert.True(response.Result);
            Assert.Equal(expectedPalette, enterprise.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [InlineData("#12ABEF")]
    [InlineData("magenta")]
    public async Task UpdateCurrent_rejects_non_palette_values(string primaryColor)
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var sut = BuildSut(new BrandingTestContext(enterprise), root);

            var response = await sut.UpdateCurrent(new BrandingUpdateRequest("Acme", primaryColor));

            Assert.False(response.Result);
            Assert.Contains("BrandingPaletteInvalid", response.Errors);
            Assert.Null(enterprise.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("#12ABEF")]
    [InlineData("unknown")]
    public async Task GetCurrent_falls_back_to_default_for_legacy_or_unknown_palette_values(string? primaryColor)
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            enterprise.PrimaryColor = primaryColor;
            var sut = BuildSut(new BrandingTestContext(enterprise), root);

            var response = await sut.GetCurrent();

            Assert.Equal(BrandingPalette.Default, response.PrimaryColor);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task Current_logo_operations_resolve_the_enabled_enterprise()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var uow = new BrandingTestContext(enterprise);
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream(PngHeader);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var upload = await sut.UploadCurrentLogo(BrandingLogoSlot.Main, file);
            var remove = await sut.RemoveCurrentLogo(BrandingLogoSlot.Main);

            Assert.True(upload.Result);
            Assert.True(remove.Result);
            Assert.Null(enterprise.LogoMainFileId);
            Assert.Empty(uow.FilesStore.Store);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_uses_distinct_entities_for_both_slots_and_replacements()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var uow = new BrandingTestContext(enterprise);
            var sut = BuildSut(uow, root);

            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main,    NewFormFile(new MemoryStream(PngHeader), "main.png",              "image/png"));
            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Sidebar, NewFormFile(new MemoryStream(PngHeader), "sidebar.png",           "image/png"));
            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main,    NewFormFile(new MemoryStream(PngHeader), "main-replacement.png",  "image/png"));
            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Sidebar, NewFormFile(new MemoryStream(PngHeader), "sidebar-replacement.png","image/png"));

            Assert.Equal(2, uow.FilesStore.Store.Count);
            Assert.Contains(uow.FilesStore.Store, f => f.Entity == "EnterpriseBranding:main");
            Assert.Contains(uow.FilesStore.Store, f => f.Entity == "EnterpriseBranding:sidebar");
            Assert.Equal(enterprise.LogoMainFileId,    uow.FilesStore.Store.Single(f => f.Entity == "EnterpriseBranding:main").Id);
            Assert.Equal(enterprise.LogoSidebarFileId, uow.FilesStore.Store.Single(f => f.Entity == "EnterpriseBranding:sidebar").Id);

            var response = await sut.GetCurrent();
            Assert.Equal(enterprise.LogoMainFileId?.ToString("N"),    response.MainLogoVersion);
            Assert.Equal(enterprise.LogoSidebarFileId?.ToString("N"), response.SidebarLogoVersion);
            Assert.NotEqual(response.MainLogoVersion, response.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task Legacy_branding_file_is_valid_only_for_the_main_slot()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var path = Path.Combine(root, "EnterpriseBranding", "legacy.png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await SystemFile.WriteAllBytesAsync(path, PngHeader);
            var legacyFile = NewBrandingFile(enterprise.Id, path, "EnterpriseBranding");
            enterprise.LogoMainFileId    = legacyFile.Id;
            enterprise.LogoSidebarFileId = legacyFile.Id;
            var uow = new BrandingTestContext(enterprise);
            uow.FilesStore.Store.Add(legacyFile);

            var response = await BuildSut(uow, root).GetCurrent();

            Assert.True(response.HasMainLogo);
            Assert.False(response.HasSidebarLogo);
            Assert.Equal(legacyFile.Id.ToString("N"), response.MainLogoVersion);
            Assert.Null(response.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task GetCurrent_logo_tokens_change_when_a_slot_file_is_replaced()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var uow = new BrandingTestContext(enterprise);
            var sut = BuildSut(uow, root);

            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main,
                NewFormFile(new MemoryStream(PngHeader), "main.png",             "image/png"));
            var first = await sut.GetCurrent();

            await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main,
                NewFormFile(new MemoryStream(PngHeader), "main-replacement.png", "image/png"));
            var second = await sut.GetCurrent();

            Assert.NotNull(first.MainLogoVersion);
            Assert.NotNull(second.MainLogoVersion);
            Assert.NotEqual(first.MainLogoVersion, second.MainLogoVersion);
            Assert.Null(first.SidebarLogoVersion);
            Assert.Null(second.SidebarLogoVersion);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task GetCurrent_returns_no_logo_token_for_invalid_slot_files()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var outsidePath = Path.Combine(Path.GetTempPath(), $"lilith-invalid-branding-{Guid.NewGuid():N}.png");
            await SystemFile.WriteAllBytesAsync(outsidePath, PngHeader);
            try
            {
                var mainFile    = NewBrandingFile(enterprise.Id, outsidePath, "EnterpriseBranding:main");
                var sidebarFile = NewBrandingFile(enterprise.Id, Path.Combine(root, "missing.png"), "EnterpriseBranding:sidebar");
                enterprise.LogoMainFileId    = mainFile.Id;
                enterprise.LogoSidebarFileId = sidebarFile.Id;
                var uow = new BrandingTestContext(enterprise);
                uow.FilesStore.Store.AddRange([mainFile, sidebarFile]);

                var response = await BuildSut(uow, root).GetCurrent();

                Assert.False(response.HasMainLogo);
                Assert.False(response.HasSidebarLogo);
                Assert.Null(response.MainLogoVersion);
                Assert.Null(response.SidebarLogoVersion);
            }
            finally
            {
                if (SystemFile.Exists(outsidePath))
                    SystemFile.Delete(outsidePath);
            }
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task Enterprise_update_preserves_persisted_branding_fields()
    {
        var enterprise = EnterpriseBuilder.Default();
        enterprise.BrandName          = "Current brand";
        enterprise.PrimaryColor       = "#123456";
        enterprise.LogoMainFileId     = Guid.NewGuid();
        enterprise.LogoSidebarFileId  = Guid.NewGuid();

        var request = EnterpriseBuilder.Default();
        request.Id               = enterprise.Id;
        request.Name             = "Updated enterprise";
        request.BrandName        = "Stale brand";
        request.PrimaryColor     = "#FFFFFF";
        request.LogoMainFileId   = Guid.NewGuid();
        request.LogoSidebarFileId = Guid.NewGuid();

        var service = new EnterpriseService(
            new BrandingTestContext(enterprise).UnitOfWork,
            NullLocalizationService.Instance,
            Substitute.For<IBrandingService>());

        var response = await service.Update(request);

        Assert.True(response.Result);
        Assert.Equal("Current brand", request.BrandName);
        Assert.Equal("#123456",        request.PrimaryColor);
        Assert.Equal(enterprise.LogoMainFileId,    request.LogoMainFileId);
        Assert.Equal(enterprise.LogoSidebarFileId, request.LogoSidebarFileId);
    }

    [Fact]
    public async Task UploadLogo_rejects_invalid_file_without_creating_storage()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var uow = new BrandingTestContext(enterprise);
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream([1, 2, 3]);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var response = await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, file);

            Assert.False(response.Result);
            Assert.Empty(uow.FilesStore.Store);
            Assert.Equal(0, uow.CompleteCallCount);
            Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_rejects_missing_file_with_localized_validation()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var uow = new BrandingTestContext(enterprise);
            var response = await BuildSut(uow, root).UploadLogo(enterprise.Id, BrandingLogoSlot.Main, null);

            Assert.False(response.Result);
            Assert.Contains("BrandingLogoRequired", response.Errors);
            Assert.Empty(uow.FilesStore.Store);
            Assert.Equal(0, uow.CompleteCallCount);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_removes_physical_file_when_database_commit_fails()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var uow = new BrandingTestContext(enterprise)
            {
                CommitException = new InvalidOperationException("database unavailable")
            };
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream(PngHeader);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var response = await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, file);

            Assert.False(response.Result);
            Assert.Equal(1, uow.CompleteCallCount);
            Assert.Empty(uow.FilesStore.Store);
            Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task UploadLogo_preserves_committed_branding_when_response_read_fails()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var uow = new BrandingTestContext(enterprise);
            // After commit succeeds, make FindAsync throw to simulate a post-commit DB read failure.
            uow.AfterCommit = () => uow.EnterprisesStore.ThrowOnFindAsync = true;
            var sut = BuildSut(uow, root);
            using var stream = new MemoryStream(PngHeader);
            var file = NewFormFile(stream, "logo.png", "image/png");

            var response = await sut.UploadLogo(enterprise.Id, BrandingLogoSlot.Main, file);

            Assert.True(response.Result);
            Assert.Null(response.Content);
            var committedFile = Assert.Single(uow.FilesStore.Store);
            Assert.Equal(committedFile.Id, enterprise.LogoMainFileId);
            Assert.True(SystemFile.Exists(committedFile.Path));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task RemoveEnterpriseFiles_removes_orphaned_branding_rows_and_files()
    {
        var root = CreateTempDirectory();
        try
        {
            var enterprise = EnterpriseBuilder.Default();
            var referencedPath = Path.Combine(root, "EnterpriseBranding", "referenced.png");
            var orphanedPath   = Path.Combine(root, "EnterpriseBranding", "orphaned.png");
            var sidebarPath    = Path.Combine(root, "EnterpriseBranding", "sidebar.png");
            Directory.CreateDirectory(Path.GetDirectoryName(referencedPath)!);
            await SystemFile.WriteAllBytesAsync(referencedPath, PngHeader);
            await SystemFile.WriteAllBytesAsync(orphanedPath,   PngHeader);
            await SystemFile.WriteAllBytesAsync(sidebarPath,    PngHeader);

            var uow = new BrandingTestContext(enterprise);
            uow.FilesStore.Store.AddRange(
            [
                NewBrandingFile(enterprise.Id, referencedPath),
                NewBrandingFile(enterprise.Id, orphanedPath),
                NewBrandingFile(enterprise.Id, sidebarPath, "EnterpriseBranding:sidebar"),
            ]);
            var sut = BuildSut(uow, root);

            var response = await sut.RemoveEnterpriseFiles(enterprise.Id);

            Assert.True(response.Result);
            Assert.Empty(uow.FilesStore.Store);
            Assert.False(SystemFile.Exists(referencedPath));
            Assert.False(SystemFile.Exists(orphanedPath));
            Assert.False(SystemFile.Exists(sidebarPath));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    // -------- helpers --------

    private static BrandingService BuildSut(BrandingTestContext context, string root) =>
        new(
            context.UnitOfWork,
            Options.Create(new AppSettings
            {
                FileManagment = new FileManagmentSettings { UploadPath = root }
            }),
            NullLocalizationService.Instance,
            NullLogger<BrandingService>.Instance);

    private static Domain.Entities.File NewBrandingFile(
        Guid enterpriseId,
        string path,
        string entity = "EnterpriseBranding") => new()
    {
        Entity       = entity,
        EntityId     = enterpriseId,
        Type         = FileType.Image,
        Path         = path,
        OriginalName = Path.GetFileName(path),
        Size         = PngHeader.Length,
    };

    private static FormFile NewFormFile(Stream stream, string fileName, string contentType) =>
        new(stream, 0, stream.Length, "file", fileName)
        {
            Headers     = new HeaderDictionary(),
            ContentType = contentType,
        };

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "lilith-branding-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

}
