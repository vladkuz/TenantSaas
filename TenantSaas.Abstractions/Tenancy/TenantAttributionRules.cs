using TenantSaas.Abstractions.Contexts;

namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Defines how tenant attribution should resolve across enabled sources.
/// </summary>
public enum AttributionStrategy
{
    /// <summary>
    /// The first enabled source that provides a tenant identifier wins.
    /// </summary>
    FirstMatch = 1,

    /// <summary>
    /// All enabled sources that provide a tenant identifier must agree.
    /// </summary>
    AllMustAgree = 2
}

/// <summary>
/// Captures allowed attribution sources and precedence for a specific rule set.
/// </summary>
public sealed class TenantAttributionRuleSet
{
    /// <summary>
    /// Creates a new rule set for tenant attribution.
    /// </summary>
    public TenantAttributionRuleSet(
        IReadOnlyList<TenantAttributionSource> allowedSources,
        AttributionStrategy strategy,
        IReadOnlyList<TenantAttributionSource>? precedenceOrder = null)
    {
        ArgumentNullException.ThrowIfNull(allowedSources);

        var allowed = NormalizeSources(allowedSources, nameof(allowedSources));
        var precedence = precedenceOrder is null || precedenceOrder.Count == 0
            ? [.. allowed]
            : NormalizeSources(precedenceOrder, nameof(precedenceOrder));

        if (precedence.Any(source => !allowed.Contains(source)))
        {
            throw new ArgumentException("Precedence order must be a subset of allowed sources.", nameof(precedenceOrder));
        }

        // Note: If precedence order is a subset of allowed sources, sources not in precedence are ignored.

        AllowedSources = allowed;
        PrecedenceOrder = precedence;
        Strategy = strategy;
    }

    /// <summary>
    /// Gets the enabled attribution sources for this rule set.
    /// </summary>
    public IReadOnlyList<TenantAttributionSource> AllowedSources { get; }

    /// <summary>
    /// Gets the precedence order applied to enabled sources.
    /// </summary>
    public IReadOnlyList<TenantAttributionSource> PrecedenceOrder { get; }

    /// <summary>
    /// Gets the strategy used when multiple sources are available.
    /// </summary>
    public AttributionStrategy Strategy { get; }

    private static TenantAttributionSource[] NormalizeSources(
        IReadOnlyList<TenantAttributionSource> sources,
        string parameterName)
    {
        var normalized = sources.ToArray();
        var unique = normalized.ToHashSet();

        if (normalized.Any(source => !Enum.IsDefined(source)))
        {
            throw new ArgumentException("Unknown attribution source provided.", parameterName);
        }

        if (unique.Count != normalized.Length)
        {
            throw new ArgumentException("Duplicate attribution sources are not allowed.", parameterName);
        }

        return normalized;
    }
}

/// <summary>
/// Defines attribution rules with optional execution-kind or endpoint overrides.
/// </summary>
public sealed class TenantAttributionRules
{
    /// <summary>
    /// Creates a new set of attribution rules.
    /// </summary>
    public TenantAttributionRules(
        TenantAttributionRuleSet defaultRuleSet,
        IReadOnlyDictionary<ExecutionKind, TenantAttributionRuleSet>? executionKindOverrides = null,
        IReadOnlyDictionary<string, TenantAttributionRuleSet>? endpointOverrides = null)
    {
        ArgumentNullException.ThrowIfNull(defaultRuleSet);

        DefaultRuleSet = defaultRuleSet;
        ExecutionKindOverrides = executionKindOverrides ?? new Dictionary<ExecutionKind, TenantAttributionRuleSet>();
        EndpointOverrides = endpointOverrides is null
            ? new Dictionary<string, TenantAttributionRuleSet>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, TenantAttributionRuleSet>(endpointOverrides, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the default rule set used when no overrides apply.
    /// </summary>
    public TenantAttributionRuleSet DefaultRuleSet { get; }

    /// <summary>
    /// Gets per-execution-kind overrides.
    /// </summary>
    public IReadOnlyDictionary<ExecutionKind, TenantAttributionRuleSet> ExecutionKindOverrides { get; }

    /// <summary>
    /// Gets per-endpoint overrides.
    /// </summary>
    public IReadOnlyDictionary<string, TenantAttributionRuleSet> EndpointOverrides { get; }

    /// <summary>
    /// Resolves the rule set for a given execution kind and optional endpoint key.
    /// </summary>
    public TenantAttributionRuleSet ResolveFor(ExecutionKind executionKind, string? endpointKey = null)
    {
        ArgumentNullException.ThrowIfNull(executionKind);

        if (!string.IsNullOrWhiteSpace(endpointKey)
            && EndpointOverrides.TryGetValue(endpointKey, out var endpointRuleSet))
        {
            return endpointRuleSet;
        }

        return ExecutionKindOverrides.TryGetValue(executionKind, out var executionRuleSet)
            ? executionRuleSet
            : DefaultRuleSet;
    }

    /// <summary>
    /// Creates a new rules instance resolved for the supplied execution kind and optional endpoint.
    /// </summary>
    public TenantAttributionRules ForExecutionKind(ExecutionKind executionKind, string? endpointKey = null)
        => new(ResolveFor(executionKind, endpointKey));

    /// <summary>
    /// Creates the default safe attribution rules.
    /// Use this for background jobs, admin operations, or when tenant context is explicitly initialized.
    /// Only allows ExplicitContext source with FirstMatch strategy.
    /// </summary>
    public static TenantAttributionRules Default()
        => new(new TenantAttributionRuleSet(
            allowedSources: [TenantAttributionSource.ExplicitContext],
            strategy: AttributionStrategy.FirstMatch));

    /// <summary>
    /// Creates a recommended rule set for typical web API flows.
    /// Use this for HTTP request handlers where tenant comes from route and/or auth token.
    /// Requires RouteParameter and TokenClaim to agree (AllMustAgree strategy).
    /// </summary>
    public static TenantAttributionRules ForWebApi()
        => new(new TenantAttributionRuleSet(
            allowedSources: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.TokenClaim
            ],
            strategy: AttributionStrategy.AllMustAgree));
}
