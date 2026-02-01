using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Reference implementation of tenant attribution resolution.
/// </summary>
public sealed class TenantAttributionResolver : ITenantAttributionResolver
{
    /// <inheritdoc />
    public TenantAttributionResult Resolve(
        IReadOnlyDictionary<TenantAttributionSource, TenantId> availableSources,
        TenantAttributionRules rules,
        ExecutionKind executionKind,
        string? endpointKey = null)
    {
        ArgumentNullException.ThrowIfNull(availableSources);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(executionKind);

        if (availableSources.Count == 0)
        {
            return TenantAttributionResult.WasNotFound();
        }

        var ruleSet = rules.ResolveFor(executionKind, endpointKey);
        var allowed = new HashSet<TenantAttributionSource>(ruleSet.AllowedSources);
        var disallowed = availableSources.Keys
            .Where(source => !allowed.Contains(source))
            .OrderBy(source => (int)source)
            .ToArray();

        if (disallowed.Length > 0)
        {
            return TenantAttributionResult.IsNotAllowed(disallowed[0]);
        }

        var orderedSources = ruleSet.PrecedenceOrder;
        var provided = new List<(TenantAttributionSource Source, TenantId TenantId)>();

        foreach (var source in orderedSources)
        {
            if (availableSources.TryGetValue(source, out var tenantId))
            {
                provided.Add((source, tenantId));
            }
        }

        if (provided.Count == 0)
        {
            return TenantAttributionResult.WasNotFound();
        }

        if (ruleSet.Strategy == AttributionStrategy.FirstMatch)
        {
            var winner = provided[0];
            return TenantAttributionResult.Succeeded(winner.TenantId, winner.Source);
        }

        var first = provided[0].TenantId;
        var allMatch = provided.All(entry => entry.TenantId.Equals(first));

        if (allMatch)
        {
            var winner = provided[0];
            return TenantAttributionResult.Succeeded(winner.TenantId, winner.Source);
        }

        // Conflicts are returned in precedence order for consistent debugging
        var conflicts = provided
            .Select(entry => new AttributionConflict(entry.Source, entry.TenantId))
            .ToArray();

        return TenantAttributionResult.IsAmbiguous(conflicts);
    }
}
