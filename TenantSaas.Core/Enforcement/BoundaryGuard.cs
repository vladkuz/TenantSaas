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

        var current = accessor.Current;

        if (!accessor.IsInitialized || current is null)
        {
            var traceId = overrideTraceId ?? Guid.NewGuid().ToString("N");
            
            // Log failure before returning result
            EnforcementEventSource.ContextNotInitialized(
                logger,
                nameof(EnforcementEventSource.ContextNotInitialized),
                LogLevel.Warning.ToString(),
                TrustContractV1.DisclosureSafeStateUnknown,
                traceId,
                requestId: null,
                LoggingDefaults.UnknownExecutionKind,
                LoggingDefaults.UnknownScopeType,
                InvariantCode.ContextInitialized);

            return EnforcementResult.Failure(
                InvariantCode.ContextInitialized,
                traceId,
                "Tenant context must be initialized before operations can proceed.");
        }

        LogContextInitialized(current);

        return EnforcementResult.Success(current);
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
                nameof(EnforcementEventSource.ContextNotInitialized),
                LogLevel.Warning.ToString(),
                TrustContractV1.DisclosureSafeStateUnknown,
                traceId,
                requestId: null,
                LoggingDefaults.UnknownExecutionKind,
                LoggingDefaults.UnknownScopeType,
                InvariantCode.ContextInitialized);

            return EnforcementResult.Failure(
                InvariantCode.ContextInitialized,
                traceId,
                "Tenant context must be initialized before operations can proceed.");
        }

        LogContextInitialized(context);

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

        // Fallback to ambient context.
        // Safety: RequireContext(accessor) reads accessor.Current exactly once into a
        // local before the null check, preventing TOCTOU races from mutable accessors.
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
                var tenantRef = DefaultLogEnricher.ToOpaqueTenantRef(successForLog.TenantId.Value);
                EnforcementEventSource.AttributionResolved(
                    logger,
                    nameof(EnforcementEventSource.AttributionResolved),
                    LogLevel.Information.ToString(),
                    tenantRef,
                    traceId,
                    requestId: null,
                    LoggingDefaults.UnknownExecutionKind,
                    LoggingDefaults.UnknownScopeType,
                    successForLog.Source.GetDisplayName());
                break;

            case TenantAttributionResult.Ambiguous:
                if (enforcementResult.ConflictingSources is { Count: > 0 })
                {
                    var conflictingSources = string.Join(", ", enforcementResult.ConflictingSources);
                    EnforcementEventSource.AttributionAmbiguous(
                        logger,
                        nameof(EnforcementEventSource.AttributionAmbiguous),
                        LogLevel.Warning.ToString(),
                        TrustContractV1.DisclosureSafeStateUnknown,
                        traceId,
                        requestId: null,
                        LoggingDefaults.UnknownExecutionKind,
                        LoggingDefaults.UnknownScopeType,
                        InvariantCode.TenantAttributionUnambiguous,
                        conflictingSources);
                }
                break;

            case TenantAttributionResult.NotFound:
                EnforcementEventSource.AttributionNotFound(
                    logger,
                    nameof(EnforcementEventSource.AttributionNotFound),
                    LogLevel.Warning.ToString(),
                    TrustContractV1.DisclosureSafeStateUnknown,
                    traceId,
                    requestId: null,
                    LoggingDefaults.UnknownExecutionKind,
                    LoggingDefaults.UnknownScopeType,
                    InvariantCode.TenantAttributionUnambiguous);
                break;

            case TenantAttributionResult.NotAllowed notAllowed:
                EnforcementEventSource.AttributionNotAllowed(
                    logger,
                    nameof(EnforcementEventSource.AttributionNotAllowed),
                    LogLevel.Warning.ToString(),
                    TrustContractV1.DisclosureSafeStateUnknown,
                    traceId,
                    requestId: null,
                    LoggingDefaults.UnknownExecutionKind,
                    LoggingDefaults.UnknownScopeType,
                    InvariantCode.TenantAttributionUnambiguous,
                    notAllowed.Source.GetDisplayName());
                break;
        }
    }

    private void LogContextInitialized(TenantContext context)
    {
        var logEvent = enricher.Enrich(context, nameof(EnforcementEventSource.ContextInitialized));
        EnforcementEventSource.ContextInitialized(
            logger,
            logEvent.EventName,
            logEvent.Severity,
            logEvent.TenantRef,
            logEvent.TraceId,
            logEvent.RequestId,
            logEvent.ExecutionKind ?? TrustContractV1.DisclosureSafeStateUnknown,
            logEvent.ScopeType ?? TrustContractV1.DisclosureSafeStateUnknown);
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
                nameof(EnforcementEventSource.BreakGlassAttemptDenied),
                LogLevel.Error.ToString(),
                TrustContractV1.DisclosureSafeStateUnknown,
                traceId,
                requestId,
                LoggingDefaults.UnknownExecutionKind,
                LoggingDefaults.UnknownScopeType,
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
                nameof(EnforcementEventSource.BreakGlassInvoked),
                LogLevel.Warning.ToString(),
                declaration.ActorId,
                declaration.Reason,
                declaration.DeclaredScope,
                tenantRef,
                traceId,
                LoggingDefaults.UnknownExecutionKind,
                LoggingDefaults.UnknownScopeType,
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
