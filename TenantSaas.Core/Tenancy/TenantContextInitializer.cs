using Microsoft.Extensions.Logging;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Single required initialization primitive for establishing tenant context per execution flow.
/// </summary>
/// <remarks>
/// <para>
/// This class provides idempotent initialization for all execution flows (request, background, admin, scripted).
/// Repeated initialization with identical inputs returns the existing context; conflicting inputs throw.
/// </para>
/// <para>
/// Register as scoped in DI for request flows, or use appropriate lifetime for other flow types.
/// </para>
/// </remarks>
/// <param name="accessor">Mutable tenant context accessor for setting/clearing context.</param>
/// <param name="logger">Logger for initialization events.</param>
public sealed class TenantContextInitializer(
    IMutableTenantContextAccessor accessor,
    ILogger<TenantContextInitializer> logger) : ITenantContextInitializer
{
    /// <inheritdoc />
    public TenantContext InitializeRequest(
        TenantScope scope,
        string traceId,
        string requestId,
        TenantAttributionInputs? attributionInputs = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        return InitializeCore(
            () => TenantContext.ForRequest(scope, traceId, requestId, NormalizeInputs(scope, attributionInputs)),
            scope,
            ExecutionKind.Request,
            traceId,
            requestId,
            attributionInputs);
    }

    /// <inheritdoc />
    public TenantContext InitializeBackground(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        return InitializeCore(
            () => TenantContext.ForBackground(scope, traceId, NormalizeInputs(scope, attributionInputs)),
            scope,
            ExecutionKind.Background,
            traceId,
            requestId: null,
            attributionInputs);
    }

    /// <inheritdoc />
    public TenantContext InitializeAdmin(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        return InitializeCore(
            () => TenantContext.ForAdmin(scope, traceId, NormalizeInputs(scope, attributionInputs)),
            scope,
            ExecutionKind.Admin,
            traceId,
            requestId: null,
            attributionInputs);
    }

    /// <inheritdoc />
    public TenantContext InitializeScripted(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        return InitializeCore(
            () => TenantContext.ForScripted(scope, traceId, NormalizeInputs(scope, attributionInputs)),
            scope,
            ExecutionKind.Scripted,
            traceId,
            requestId: null,
            attributionInputs);
    }

    /// <inheritdoc />
    public void Clear()
    {
        accessor.Clear();
        logger.LogDebug("Tenant context cleared");
    }

    private TenantContext InitializeCore(
        Func<TenantContext> contextFactory,
        TenantScope scope,
        ExecutionKind executionKind,
        string traceId,
        string? requestId,
        TenantAttributionInputs? attributionInputs)
    {
        var normalizedInputs = NormalizeInputs(scope, attributionInputs);

        // Idempotent: if already initialized, validate inputs match
        if (accessor.IsInitialized && accessor.Current is not null)
        {
            var existing = accessor.Current;

            // Check if inputs match existing context (idempotent case)
            bool isIdempotent =
                existing.Scope.Equals(scope) &&
                existing.ExecutionKind.Equals(executionKind) &&
                existing.TraceId == traceId &&
                existing.RequestId == requestId &&
                existing.AttributionInputs.Equals(normalizedInputs);

            if (isIdempotent)
            {
                logger.LogDebug(
                    "Tenant context already initialized with identical inputs - returning existing context. TraceId: {TraceId}",
                    traceId);
                return existing;
            }

            // Conflicting inputs - refuse initialization
            throw new TenantContextConflictException(
                "Tenant context already initialized with different inputs.",
                traceId,
                requestId);
        }

        // First initialization - create and set context
        var context = contextFactory();
        accessor.Set(context);

        logger.LogInformation(
            "Tenant context initialized. ExecutionKind: {ExecutionKind}, TraceId: {TraceId}, RequestId: {RequestId}",
            executionKind.Value,
            traceId,
            requestId ?? "null");

        return context;
    }

    private static TenantAttributionInputs NormalizeInputs(
        TenantScope scope,
        TenantAttributionInputs? attributionInputs)
        => attributionInputs ?? TenantAttributionInputs.FromExplicitScope(scope);
}
