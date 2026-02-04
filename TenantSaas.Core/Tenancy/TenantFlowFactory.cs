using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Factory for creating scoped tenant execution flows for non-request operations.
/// </summary>
/// <remarks>
/// <para>
/// This factory uses <see cref="ITenantContextInitializer"/> internally to establish
/// context and wraps it in a <see cref="TenantFlowScope"/> for automatic cleanup.
/// </para>
/// <para>
/// Each flow wrapper clears any existing ambient context before initialization,
/// ensuring new flows start with a clean state and preventing cross-flow leakage.
/// </para>
/// </remarks>
/// <param name="initializer">The context initializer used to establish tenant context.</param>
/// <param name="accessor">The ambient context accessor used for cleanup checks.</param>
public sealed class TenantFlowFactory(
    ITenantContextInitializer initializer,
    IMutableTenantContextAccessor accessor) : ITenantFlowFactory
{
    /// <inheritdoc />
    public ITenantFlowScope CreateBackgroundFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        // Clear any existing context to ensure new flow starts clean
        initializer.Clear();

        var context = initializer.InitializeBackground(scope, traceId, attributionInputs);
        return new TenantFlowScope(context, accessor);
    }

    /// <inheritdoc />
    public ITenantFlowScope CreateAdminFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        // Clear any existing context to ensure new flow starts clean
        initializer.Clear();

        var context = initializer.InitializeAdmin(scope, traceId, attributionInputs);
        return new TenantFlowScope(context, accessor);
    }

    /// <inheritdoc />
    public ITenantFlowScope CreateScriptedFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        // Clear any existing context to ensure new flow starts clean
        initializer.Clear();

        var context = initializer.InitializeScripted(scope, traceId, attributionInputs);
        return new TenantFlowScope(context, accessor);
    }
}
