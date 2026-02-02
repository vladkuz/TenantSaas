using FluentAssertions;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTests;

public sealed class InvariantRegistryTests
{
    [Theory]
    [InlineData(InvariantCode.ContextInitialized)]
    [InlineData(InvariantCode.TenantAttributionUnambiguous)]
    [InlineData(InvariantCode.TenantScopeRequired)]
    [InlineData(InvariantCode.BreakGlassExplicitAndAudited)]
    [InlineData(InvariantCode.DisclosureSafe)]
    public void InvariantRegistry_Contains_All_Invariants(string code)
    {
        var definition = TrustContractV1.GetInvariant(code);

        definition.Should().NotBeNull();
        definition.InvariantCode.Should().Be(code);
        definition.Name.Should().NotBeNullOrWhiteSpace();
        definition.Description.Should().NotBeNullOrWhiteSpace();
        definition.Category.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(InvariantCode.ContextInitialized, "Initialization")]
    [InlineData(InvariantCode.TenantAttributionUnambiguous, "Attribution")]
    [InlineData(InvariantCode.TenantScopeRequired, "Scope")]
    [InlineData(InvariantCode.BreakGlassExplicitAndAudited, "Authorization")]
    [InlineData(InvariantCode.DisclosureSafe, "Disclosure")]
    public void InvariantRegistry_Categories_Are_Consistent(string code, string expectedCategory)
    {
        var definition = TrustContractV1.GetInvariant(code);

        definition.Category.Should().Be(expectedCategory);
    }

    [Fact]
    public void InvariantCode_Uses_Stable_Strings()
    {
        var allCodes = new[]
        {
            InvariantCode.ContextInitialized,
            InvariantCode.TenantAttributionUnambiguous,
            InvariantCode.TenantScopeRequired,
            InvariantCode.BreakGlassExplicitAndAudited,
            InvariantCode.DisclosureSafe
        };

        foreach (var code in allCodes)
        {
            code.Should().NotBeNullOrWhiteSpace();
            code.Should().NotContain("{");
            code.Should().MatchRegex("^[A-Z][a-zA-Z]*$");
        }
    }

    [Fact]
    public void InvariantRegistry_Throws_For_Unknown_Code()
    {
        Action act = () => TrustContractV1.GetInvariant("UnknownCode");

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*UnknownCode*");
    }

    [Fact]
    public void InvariantRegistry_TryGet_Returns_False_For_Unknown_Code()
    {
        var found = TrustContractV1.TryGetInvariant("UnknownCode", out var definition);

        found.Should().BeFalse();
        definition.Should().BeNull();
    }

    [Theory]
    [InlineData(InvariantCode.ContextInitialized)]
    [InlineData(InvariantCode.TenantAttributionUnambiguous)]
    [InlineData(InvariantCode.TenantScopeRequired)]
    [InlineData(InvariantCode.BreakGlassExplicitAndAudited)]
    [InlineData(InvariantCode.DisclosureSafe)]
    public void RefusalMappingRegistry_Contains_All_Mappings(string invariantCode)
    {
        var mapping = TrustContractV1.GetRefusalMapping(invariantCode);

        mapping.Should().NotBeNull();
        mapping.InvariantCode.Should().Be(invariantCode);
        mapping.HttpStatusCode.Should().BeInRange(400, 599);
        mapping.ProblemType.Should().NotBeNullOrWhiteSpace();
        mapping.Title.Should().NotBeNullOrWhiteSpace();
        mapping.GuidanceUri.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RefusalMapping_ProblemType_Is_Stable_Urn()
    {
        foreach (var mapping in TrustContractV1.RefusalMappings.Values)
        {
            mapping.ProblemType.Should().StartWith("urn:tenantsaas:error:");
            mapping.ProblemType.Should().MatchRegex("^urn:tenantsaas:error:[a-z-]+$");
        }
    }

    [Fact]
    public void RefusalMapping_HttpStatusCodes_Are_Appropriate()
    {
        TrustContractV1.GetRefusalMapping(InvariantCode.ContextInitialized).HttpStatusCode.Should().Be(400);
        TrustContractV1.GetRefusalMapping(InvariantCode.TenantAttributionUnambiguous).HttpStatusCode.Should().Be(422);
        TrustContractV1.GetRefusalMapping(InvariantCode.TenantScopeRequired).HttpStatusCode.Should().Be(403);
        TrustContractV1.GetRefusalMapping(InvariantCode.BreakGlassExplicitAndAudited).HttpStatusCode.Should().Be(403);
        TrustContractV1.GetRefusalMapping(InvariantCode.DisclosureSafe).HttpStatusCode.Should().Be(500);
    }

    [Fact]
    public void RefusalMapping_GuidanceUri_Is_WellFormed()
    {
        foreach (var mapping in TrustContractV1.RefusalMappings.Values)
        {
            Uri.IsWellFormedUriString(mapping.GuidanceUri, UriKind.Absolute).Should().BeTrue();
        }
    }

    [Fact]
    public void InvariantDefinition_Is_Immutable()
    {
        var definition = new InvariantDefinition(
            "TestCode",
            "Test Name",
            "Test description",
            "Test");

        definition.InvariantCode.Should().Be("TestCode");
        definition.Name.Should().Be("Test Name");
        definition.Should().BeAssignableTo<IEquatable<InvariantDefinition>>();
    }

    [Fact]
    public void RefusalMapping_Is_Immutable()
    {
        var mapping = new RefusalMapping(
            "TestCode",
            400,
            "urn:test:error",
            "Test Title",
            "https://docs.test.com/errors/test");

        mapping.InvariantCode.Should().Be("TestCode");
        mapping.HttpStatusCode.Should().Be(400);
        mapping.Should().BeAssignableTo<IEquatable<RefusalMapping>>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InvariantDefinition_Throws_On_Invalid_InvariantCode(string? invalidCode)
    {
        Action act = () => new InvariantDefinition(invalidCode!, "Name", "Description", "Category");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("invariantCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InvariantDefinition_Throws_On_Invalid_Name(string? invalidName)
    {
        Action act = () => new InvariantDefinition("Code", invalidName!, "Description", "Category");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InvariantDefinition_Throws_On_Invalid_Description(string? invalidDescription)
    {
        Action act = () => new InvariantDefinition("Code", "Name", invalidDescription!, "Category");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InvariantDefinition_Throws_On_Invalid_Category(string? invalidCategory)
    {
        Action act = () => new InvariantDefinition("Code", "Name", "Description", invalidCategory!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("category");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RefusalMapping_Throws_On_Invalid_InvariantCode(string? invalidCode)
    {
        Action act = () => new RefusalMapping(invalidCode!, 400, "urn:test", "Title", "https://example.com");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("invariantCode");
    }

    [Theory]
    [InlineData(199)]
    [InlineData(200)]
    [InlineData(399)]
    [InlineData(600)]
    [InlineData(700)]
    public void RefusalMapping_Throws_On_Invalid_HttpStatusCode(int invalidStatus)
    {
        Action act = () => new RefusalMapping("Code", invalidStatus, "urn:test", "Title", "https://example.com");

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("httpStatusCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RefusalMapping_Throws_On_Invalid_ProblemType(string? invalidType)
    {
        Action act = () => new RefusalMapping("Code", 400, invalidType!, "Title", "https://example.com");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("problemType");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RefusalMapping_Throws_On_Invalid_Title(string? invalidTitle)
    {
        Action act = () => new RefusalMapping("Code", 400, "urn:test", invalidTitle!, "https://example.com");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RefusalMapping_Throws_On_Invalid_GuidanceUri(string? invalidUri)
    {
        Action act = () => new RefusalMapping("Code", 400, "urn:test", "Title", invalidUri!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("guidanceUri");
    }

    [Fact]
    public void InvariantCode_All_Contains_All_Defined_Codes()
    {
        InvariantCode.All.Should().HaveCount(5);
        InvariantCode.All.Should().Contain(InvariantCode.ContextInitialized);
        InvariantCode.All.Should().Contain(InvariantCode.TenantAttributionUnambiguous);
        InvariantCode.All.Should().Contain(InvariantCode.TenantScopeRequired);
        InvariantCode.All.Should().Contain(InvariantCode.BreakGlassExplicitAndAudited);
        InvariantCode.All.Should().Contain(InvariantCode.DisclosureSafe);
    }

    [Fact]
    public void InvariantRegistry_Has_Entry_For_Each_InvariantCode_All()
    {
        foreach (var code in InvariantCode.All)
        {
            TrustContractV1.Invariants.Should().ContainKey(code);
        }
    }

    [Fact]
    public void RefusalMappingRegistry_Has_Entry_For_Each_InvariantCode_All()
    {
        foreach (var code in InvariantCode.All)
        {
            TrustContractV1.RefusalMappings.Should().ContainKey(code);
        }
    }

    [Theory]
    [InlineData("not-a-uri")]
    [InlineData("relative/path")]
    [InlineData("://missing-scheme")]
    public void RefusalMapping_Throws_On_Malformed_GuidanceUri(string malformedUri)
    {
        Action act = () => new RefusalMapping("Code", 400, "urn:test", "Title", malformedUri);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("guidanceUri")
            .WithMessage("*valid absolute URI*");
    }

    [Fact]
    public void ForBadRequest_Sets_Status_400_And_Builds_ProblemType()
    {
        var mapping = RefusalMapping.ForBadRequest("TestCode", "Title", "https://example.com");

        mapping.HttpStatusCode.Should().Be(400);
        mapping.ProblemType.Should().Be("urn:tenantsaas:error:test-code");
        mapping.InvariantCode.Should().Be("TestCode");
        mapping.Title.Should().Be("Title");
        mapping.GuidanceUri.Should().Be("https://example.com");
    }

    [Fact]
    public void ForForbidden_Sets_Status_403_And_Builds_ProblemType()
    {
        var mapping = RefusalMapping.ForForbidden("AnotherCode", "Forbidden Title", "https://docs.example.com");

        mapping.HttpStatusCode.Should().Be(403);
        mapping.ProblemType.Should().Be("urn:tenantsaas:error:another-code");
        mapping.InvariantCode.Should().Be("AnotherCode");
        mapping.Title.Should().Be("Forbidden Title");
    }

    [Fact]
    public void ForUnprocessableEntity_Sets_Status_422_And_Builds_ProblemType()
    {
        var mapping = RefusalMapping.ForUnprocessableEntity("ValidationCode", "Validation Failed", "https://help.example.com");

        mapping.HttpStatusCode.Should().Be(422);
        mapping.ProblemType.Should().Be("urn:tenantsaas:error:validation-code");
        mapping.InvariantCode.Should().Be("ValidationCode");
    }

    [Fact]
    public void ForInternalServerError_Sets_Status_500_And_Builds_ProblemType()
    {
        var mapping = RefusalMapping.ForInternalServerError("ServerError", "Internal Error", "https://status.example.com");

        mapping.HttpStatusCode.Should().Be(500);
        mapping.ProblemType.Should().Be("urn:tenantsaas:error:server-error");
        mapping.InvariantCode.Should().Be("ServerError");
    }

    [Theory]
    [InlineData("SimpleCode", "simple-code")]
    [InlineData("TwoWords", "two-words")]
    [InlineData("ContextInitialized", "context-initialized")]
    [InlineData("DisclosureSafe", "disclosure-safe")]
    public void ToKebabCase_Converts_PascalCase_Correctly(string input, string expected)
    {
        var mapping = RefusalMapping.ForBadRequest(input, "Title", "https://example.com");

        mapping.ProblemType.Should().Be($"urn:tenantsaas:error:{expected}");
    }
}
