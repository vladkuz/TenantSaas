using TenantSaas.Abstractions.Contexts;

namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Resolves tenant identity from available attribution sources.
/// </summary>
public interface ITenantAttributionResolver
{
    /// <summary>
    /// Resolves tenant attribution given available sources and rules.
    /// Must be deterministic and side-effect-free.
    /// </summary>
    /// <param name="availableSources">Available tenant identifiers keyed by attribution source.</param>
    /// <param name="rules">Attribution rules to apply.</param>
    /// <param name="executionKind">Execution context kind for rule override resolution.</param>
    /// <param name="endpointKey">Optional endpoint identifier for endpoint-specific rule overrides.</param>
    TenantAttributionResult Resolve(
        IReadOnlyDictionary<TenantAttributionSource, TenantId> availableSources,
        TenantAttributionRules rules,
        ExecutionKind executionKind,
        string? endpointKey = null);
}
