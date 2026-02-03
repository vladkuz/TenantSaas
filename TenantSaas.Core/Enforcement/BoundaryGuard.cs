using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Enforces tenant context invariants at sanctioned boundaries.
/// </summary>
public static class BoundaryGuard
{
    private static volatile ILogger logger = NullLogger.Instance;
    private static volatile ILogEnricher? enricher;
    private static volatile bool isConfigured;
    private static readonly object configLock = new();

    /// <summary>
    /// Configures logging for enforcement boundary operations.
    /// Must be called once during application startup to enable structured logging.
    /// Thread-safe and idempotent - subsequent calls after first configuration are ignored.
    /// </summary>
    /// <param name="loggerInstance">Logger instance for enforcement events.</param>
    /// <param name="enricherInstance">Log enricher for structured field extraction.</param>
    /// <exception cref="ArgumentNullException">Thrown when loggerInstance or enricherInstance is null.</exception>
    public static void Configure(ILogger loggerInstance, ILogEnricher enricherInstance)
    {
        ArgumentNullException.ThrowIfNull(loggerInstance);
        ArgumentNullException.ThrowIfNull(enricherInstance);

        if (isConfigured)
        {
            return; // Already configured, ignore subsequent calls
        }

        lock (configLock)
        {
            if (isConfigured)
            {
                return; // Double-check after acquiring lock
            }

            logger = loggerInstance;
            enricher = enricherInstance;
            isConfigured = true;
        }
    }

    /// <summary>
    /// Resets configuration for testing purposes only.
    /// </summary>
    internal static void ResetForTesting()
    {
        lock (configLock)
        {
            logger = NullLogger.Instance;
            enricher = null;
            isConfigured = false;
        }
    }

    /// <summary>
    /// Requires that tenant context has been initialized.
    /// </summary>
    /// <param name="accessor">Context accessor to check.</param>
    /// <param name="overrideTraceId">Optional trace ID for correlation when context is missing.</param>
    /// <returns>Success with context, or failure with invariant violation.</returns>
    public static EnforcementResult RequireContext(
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

        // Log success - capture local reference to avoid null check race
        var currentEnricher = enricher;
        if (currentEnricher is not null && accessor.Current is not null)
        {
            var logEvent = currentEnricher.Enrich(accessor.Current, "ContextInitialized");
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

    /// <summary>
    /// Requires that tenant attribution is unambiguous.
    /// </summary>
    /// <param name="result">Attribution resolution result.</param>
    /// <param name="traceId">Trace identifier for correlation.</param>
    /// <returns>Success with resolved tenant ID and source, or failure with invariant violation.</returns>
    public static AttributionEnforcementResult RequireUnambiguousAttribution(
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
    private static void LogAttributionResult(
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
