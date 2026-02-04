using TenantSaas.Abstractions.BreakGlass;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;
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
/// <param name="logger">Logger instance for enforcement events.</param>
/// <param name="enricher">Log enricher for structured field extraction.</param>
/// <param name="auditSink">Optional audit sink for break-glass events (Epic 7 implementation).</param>
public sealed class BoundaryGuard(
    ILogger<BoundaryGuard> logger,
    ILogEnricher enricher,
    IBreakGlassAuditSink? auditSink = null) : IBoundaryGuard
{

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
    public EnforcementResult RequireContext(
        TenantContext? context,
        string? overrideTraceId = null)
    {
        if (context is null)
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
        var logEvent = enricher.Enrich(context, "ContextInitialized");
        EnforcementEventSource.ContextInitialized(
            logger,
            logEvent.TenantRef,
            logEvent.TraceId,
            logEvent.RequestId,
            logEvent.ExecutionKind ?? "unknown",
            logEvent.ScopeType ?? "unknown");

        return EnforcementResult.Success(context);
    }

    /// <inheritdoc />
    public EnforcementResult RequireContext(
        ITenantContextAccessor accessor,
        TenantContext? explicitContext)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        // Explicit context takes precedence over ambient
        if (explicitContext is not null)
        {
            return RequireContext(explicitContext);
        }

        // Fallback to ambient context
        return RequireContext(accessor);
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

    /// <inheritdoc />
    public async Task<EnforcementResult> RequireBreakGlassAsync(
        BreakGlassDeclaration? declaration,
        string traceId,
        string? requestId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        // Validate declaration
        var validationResult = BreakGlassValidator.Validate(declaration);
        if (!validationResult.IsValid)
        {
            // Reason is guaranteed non-null when IsValid is false
            var reason = validationResult.Reason!;

            // Log denial
            EnforcementEventSource.BreakGlassAttemptDenied(
                logger,
                traceId,
                requestId,
                InvariantCode.BreakGlassExplicitAndAudited,
                reason);

            return EnforcementResult.Failure(
                InvariantCode.BreakGlassExplicitAndAudited,
                traceId,
                reason);
        }

        // Create audit event (will be used for logging and sink)
        var tenantRef = declaration!.TargetTenantRef
            ?? TrustContractV1.BreakGlassMarkerCrossTenant;

        // Log successful invocation
        EnforcementEventSource.BreakGlassInvoked(
            logger,
            declaration.ActorId,
            declaration.Reason,
            declaration.DeclaredScope,
            tenantRef,
            traceId,
            AuditCode.BreakGlassInvoked);

        // Emit to audit sink if available (fail gracefully)
        if (auditSink != null)
        {
            try
            {
                var auditEvent = BreakGlassAuditHelper.CreateAuditEvent(
                    declaration,
                    traceId,
                    invariantCode: null,
                    operationName: null);

                await auditSink.EmitAsync(auditEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log but don't block - audit sink is optional
                logger.LogError(ex, "Failed to emit break-glass audit event to sink");
            }
        }

        return EnforcementResult.Success();
    }
}
