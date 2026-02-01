using FluentAssertions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.Core.Tenancy;

namespace TenantSaas.ContractTests;

public sealed class AttributionRulesTests
{
    [Fact]
    public void Attribution_Sources_Should_Be_Complete_And_Documented()
    {
        var sources = Enum.GetValues<TenantAttributionSource>();

        sources.Should().BeEquivalentTo(
            [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.HeaderValue,
                TenantAttributionSource.HostHeader,
                TenantAttributionSource.TokenClaim,
                TenantAttributionSource.ExplicitContext
            ],
            options => options.WithStrictOrdering());

        sources.Should().OnlyContain(source => !string.IsNullOrWhiteSpace(source.GetDisplayName()));
        sources.Should().OnlyContain(source => !string.IsNullOrWhiteSpace(source.GetIdentifier()));
        sources.Should().OnlyContain(source => !string.IsNullOrWhiteSpace(source.GetDescription()));
    }

    [Fact]
    public void InvariantCode_Should_Be_Stable_And_Mapped()
    {
        InvariantCode.TenantAttributionUnambiguous.Should().Be("tenant-attribution-unambiguous");
        TrustContractV1.InvariantTenantAttributionUnambiguous.Should().Be(InvariantCode.TenantAttributionUnambiguous);
    }

    [Fact]
    public void ProblemDetails_Type_Should_Be_Valid_URN()
    {
        var problemType = TrustContractV1.ProblemTypeTenantAttributionUnambiguous;
        
        problemType.Should().StartWith("urn:");
        problemType.Should().Contain("tenant-attribution-unambiguous");
        problemType.Should().Be("urn:tenant-saas:trust-contract:tenant-attribution-unambiguous");
    }

    [Fact]
    public void Single_Source_Resolution_Should_Succeed()
    {
        var resolver = new TenantAttributionResolver();
        var rules = new TenantAttributionRules(new TenantAttributionRuleSet(
            allowedSources: [TenantAttributionSource.RouteParameter],
            strategy: AttributionStrategy.FirstMatch));

        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.RouteParameter] = new TenantId("tenant-1")
        };

        var result = resolver.Resolve(sources, rules, ExecutionKind.Request);

        var success = result.Should().BeOfType<TenantAttributionResult.Success>().Subject;
        success.TenantId.Should().Be(new TenantId("tenant-1"));
        success.Source.Should().Be(TenantAttributionSource.RouteParameter);
    }

    [Fact]
    public void Conflicting_Sources_Should_Produce_Ambiguous_Result()
    {
        var resolver = new TenantAttributionResolver();
        var rules = new TenantAttributionRules(new TenantAttributionRuleSet(
            allowedSources: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.HeaderValue
            ],
            strategy: AttributionStrategy.AllMustAgree));

        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.RouteParameter] = new TenantId("tenant-1"),
            [TenantAttributionSource.HeaderValue] = new TenantId("tenant-2")
        };

        var result = resolver.Resolve(sources, rules, ExecutionKind.Request);

        var ambiguous = result.Should().BeOfType<TenantAttributionResult.Ambiguous>().Subject;
        ambiguous.Conflicts.Should().BeEquivalentTo(
            [
                new AttributionConflict(TenantAttributionSource.RouteParameter, new TenantId("tenant-1")),
                new AttributionConflict(TenantAttributionSource.HeaderValue, new TenantId("tenant-2"))
            ],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void Disallowed_Source_Should_Produce_NotAllowed_Result()
    {
        var resolver = new TenantAttributionResolver();
        var rules = new TenantAttributionRules(new TenantAttributionRuleSet(
            allowedSources: [TenantAttributionSource.RouteParameter],
            strategy: AttributionStrategy.FirstMatch));

        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.HeaderValue] = new TenantId("tenant-1")
        };

        var result = resolver.Resolve(sources, rules, ExecutionKind.Request);

        var notAllowed = result.Should().BeOfType<TenantAttributionResult.NotAllowed>().Subject;
        notAllowed.Source.Should().Be(TenantAttributionSource.HeaderValue);
    }

    [Fact]
    public void FirstMatch_Strategy_Should_Use_Precedence_Order()
    {
        var resolver = new TenantAttributionResolver();
        var rules = new TenantAttributionRules(new TenantAttributionRuleSet(
            allowedSources: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.HeaderValue
            ],
            strategy: AttributionStrategy.FirstMatch,
            precedenceOrder: [
                TenantAttributionSource.HeaderValue,
                TenantAttributionSource.RouteParameter
            ]));

        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.RouteParameter] = new TenantId("tenant-1"),
            [TenantAttributionSource.HeaderValue] = new TenantId("tenant-2")
        };

        var result = resolver.Resolve(sources, rules, ExecutionKind.Request);

        var success = result.Should().BeOfType<TenantAttributionResult.Success>().Subject;
        success.Source.Should().Be(TenantAttributionSource.HeaderValue);
        success.TenantId.Should().Be(new TenantId("tenant-2"));
    }

    [Fact]
    public void AllMustAgree_Strategy_Should_Require_Agreement()
    {
        var resolver = new TenantAttributionResolver();
        var rules = new TenantAttributionRules(new TenantAttributionRuleSet(
            allowedSources: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.TokenClaim
            ],
            strategy: AttributionStrategy.AllMustAgree));

        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.RouteParameter] = new TenantId("tenant-1"),
            [TenantAttributionSource.TokenClaim] = new TenantId("tenant-1")
        };

        var result = resolver.Resolve(sources, rules, ExecutionKind.Request);

        var success = result.Should().BeOfType<TenantAttributionResult.Success>().Subject;
        success.TenantId.Should().Be(new TenantId("tenant-1"));
    }

    [Fact]
    public void No_Sources_Should_Produce_NotFound_Result()
    {
        var resolver = new TenantAttributionResolver();
        var rules = TenantAttributionRules.Default();

        var result = resolver.Resolve(
            new Dictionary<TenantAttributionSource, TenantId>(),
            rules,
            ExecutionKind.Background);

        result.Should().BeOfType<TenantAttributionResult.NotFound>();
    }

    [Fact]
    public void ExecutionKind_Customization_Should_Resolve_Rules()
    {
        var defaultRuleSet = new TenantAttributionRuleSet(
            allowedSources: [TenantAttributionSource.ExplicitContext],
            strategy: AttributionStrategy.FirstMatch);

        var requestRuleSet = new TenantAttributionRuleSet(
            allowedSources: [TenantAttributionSource.RouteParameter],
            strategy: AttributionStrategy.FirstMatch);

        var rules = new TenantAttributionRules(
            defaultRuleSet,
            executionKindOverrides: new Dictionary<ExecutionKind, TenantAttributionRuleSet>
            {
                [ExecutionKind.Request] = requestRuleSet
            });

        var resolver = new TenantAttributionResolver();
        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.RouteParameter] = new TenantId("tenant-1")
        };

        // Request context should use the override allowing RouteParameter
        var requestResult = resolver.Resolve(sources, rules, ExecutionKind.Request);
        requestResult.Should().BeOfType<TenantAttributionResult.Success>();

        // Background context should use default rules (ExplicitContext only)
        var backgroundResult = resolver.Resolve(sources, rules, ExecutionKind.Background);
        backgroundResult.Should().BeOfType<TenantAttributionResult.NotAllowed>();
    }

    [Fact]
    public void Ambiguous_Result_Should_Include_Conflict_Details()
    {
        var resolver = new TenantAttributionResolver();
        var rules = new TenantAttributionRules(new TenantAttributionRuleSet(
            allowedSources: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.HostHeader
            ],
            strategy: AttributionStrategy.AllMustAgree));

        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.RouteParameter] = new TenantId("tenant-1"),
            [TenantAttributionSource.HostHeader] = new TenantId("tenant-2")
        };

        var result = resolver.Resolve(sources, rules, ExecutionKind.Request);

        var ambiguous = result.Should().BeOfType<TenantAttributionResult.Ambiguous>().Subject;
        ambiguous.Conflicts.Should().Contain(conflict =>
            conflict.Source == TenantAttributionSource.RouteParameter
            && conflict.ProvidedTenantId == new TenantId("tenant-1"));
        ambiguous.Conflicts.Should().Contain(conflict =>
            conflict.Source == TenantAttributionSource.HostHeader
            && conflict.ProvidedTenantId == new TenantId("tenant-2"));
    }

    [Fact]
    public void Duplicate_Precedence_Should_Be_Rejected()
    {
        var act = () => new TenantAttributionRuleSet(
            allowedSources: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.HeaderValue
            ],
            strategy: AttributionStrategy.FirstMatch,
            precedenceOrder: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.RouteParameter
            ]);

        act.Should().Throw<ArgumentException>();
    }
}
