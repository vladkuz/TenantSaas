using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Core.Logging;
using Microsoft.Extensions.Logging;

namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Enforces tenant context invariants at sanctioned boundaries.
/// </summary>
/// <remarks>
/// This class implements the invariant enforcement contract. Register as a singleton
/// in DI and inject where boundary enforcement is needed.
/// </remarks>
public sealed class BoundaryGuard : IBoundaryGuard
{
    private readonly ILogger logger;
    private readonly ILogEnricher enricher;

    /// <summary>
    /// Creates a new BoundaryGuard with the specified dependencies.
    /// </summary>
    /// <param name="logger">Logger instance for enforcement events.</param>
    /// <param name="enricher">Log enricher for structured field extraction.</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    public BoundaryGuard(ILogger<BoundaryGuard> logger, ILogEnricher enricher)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(enricher);

        this.logger = logger;
        this.enricher = enricher;
    }

    /// <inheritdoc />
    public EnforcementResult RequireContext(
        ITenantContextAccessor accessor,
        string? overrideTraceId = null)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        if (!accessor.IsInitialized)
        {
            var traceId = overrideTraceId ?? Guid.NewGuid().ToString("N");
            
            // Log failure before returning result
            EnforcementEventSource.ContextNotInitialized(
                logger,
                traceId,
                requestId: null,
                InvariantCode.ContextInitialized);

            return EnforcementResult.Failure(
                InvariantCode.ContextInitialized,
                traceId,
                "Tenant context must be initialized before operations can proceed.");
        }

        // Log success
        if (accessor.Current is not null)
        {
            var logEvent = enricher.Enrich(accessor.Current, "ContextInitialized");
            EnforcementEventSource.ContextInitialized(
                logger,
                logEvent.TenantRef,
                logEvent.TraceId,
                logEvent.RequestId,
                logEvent.ExecutionKind ?? "unknown",
                logEvent.ScopeType ?? "unknown");
        }

        return EnforcementResult.Success(accessor.Current!);
    }

    /// <inheritdoc />
    public AttributionEnforcementResult RequireUnambiguousAttribution(
        TenantAttributionResult result,
        string traceId)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        var enforcementResult = result switch
        {
            TenantAttributionResult.Success successResult => AttributionEnforcementResult.Success(
                successResult.TenantId,
                successResult.Source,
                traceId),
            
            TenantAttributionResult.Ambiguous ambiguous => AttributionEnforcementResult.Ambiguous(
                [.. ambiguous.Conflicts.Select(c => c.Source.GetDisplayName())],
                traceId),
            
            TenantAttributionResult.NotFound => AttributionEnforcementResult.NotFound(traceId),
            
            TenantAttributionResult.NotAllowed notAllowed => AttributionEnforcementResult.NotAllowed(
                notAllowed.Source,
                traceId),
            
            _ => throw new InvalidOperationException("Unknown attribution result type.")
        };

        // Log based on result type
        LogAttributionResult(enforcementResult, result, traceId);

        return enforcementResult;
    }

    /// <summary>
    /// Logs attribution result based on outcome type.
    /// </summary>
    private void LogAttributionResult(
        AttributionEnforcementResult enforcementResult,
        TenantAttributionResult originalResult,
        string traceId)
    {
        switch (originalResult)
        {
            case TenantAttributionResult.Success successForLog:
                EnforcementEventSource.AttributionResolved(
                    logger,
                    successForLog.TenantId.Value,
                    traceId,
                    requestId: null,
                    successForLog.Source.GetDisplayName());
                break;

            case TenantAttributionResult.Ambiguous:
                if (enforcementResult.ConflictingSources is { Count: > 0 })
                {
                    var conflictingSources = string.Join(", ", enforcementResult.ConflictingSources);
                    EnforcementEventSource.AttributionAmbiguous(
                        logger,
                        traceId,
                        requestId: null,
                        InvariantCode.TenantAttributionUnambiguous,
                        conflictingSources);
                }
                break;

            case TenantAttributionResult.NotFound:
                EnforcementEventSource.AttributionNotFound(
                    logger,
                    traceId,
                    requestId: null,
                    InvariantCode.TenantAttributionUnambiguous);
                break;

            case TenantAttributionResult.NotAllowed notAllowed:
                EnforcementEventSource.AttributionNotAllowed(
                    logger,
                    traceId,
                    requestId: null,
                    InvariantCode.TenantAttributionUnambiguous,
                    notAllowed.Source.GetDisplayName());
                break;
        }
    }
}
