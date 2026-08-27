using Domain.Entities.Production;

namespace Application.Tests.TestData;

/// <summary>Builder for <see cref="Enterprise"/> test data.</summary>
public static class EnterpriseBuilder
{
    /// <summary>Returns a minimal enabled enterprise suitable for Branding tests.</summary>
    public static Enterprise Default() => new()
    {
        Name = "Test Enterprise",
        Disabled = false,
    };
}
