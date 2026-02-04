namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Single required initialization primitive for establishing tenant context per execution flow.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for the required initialization primitive that establishes
/// tenant context for request, background, admin, and scripted execution flows.
/// </para>
/// <para>
/// Initialization is idempotent within a flow; repeated calls with identical inputs return the
/// existing context. Calls with conflicting inputs will throw to prevent context corruption.
/// </para>
/// <para>
/// After initialization, context is available via <see cref="ITenantContextAccessor"/>.
/// </para>
/// </remarks>
public interface ITenantContextInitializer
{
    /// <summary>
    /// Initializes tenant context for an HTTP or API request flow.
    /// </summary>
    /// <param name="scope">The tenant scope for the request.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="requestId">Request identifier.</param>
    /// <param name="attributionInputs">Attribution inputs captured during initialization.</param>
    /// <returns>The initialized tenant context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scope, traceId, or requestId is null/whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when already initialized with conflicting inputs.</exception>
    TenantContext InitializeRequest(
        TenantScope scope,
        string traceId,
        string requestId,
        TenantAttributionInputs? attributionInputs = null);

    /// <summary>
    /// Initializes tenant context for a background job or worker flow.
    /// </summary>
    /// <param name="scope">The tenant scope for the background job.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="attributionInputs">Attribution inputs captured during initialization.</param>
    /// <returns>The initialized tenant context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scope or traceId is null/whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when already initialized with conflicting inputs.</exception>
    TenantContext InitializeBackground(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);

    /// <summary>
    /// Initializes tenant context for an administrative operation flow.
    /// </summary>
    /// <param name="scope">The tenant scope for the admin operation.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="attributionInputs">Attribution inputs captured during initialization.</param>
    /// <returns>The initialized tenant context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scope or traceId is null/whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when already initialized with conflicting inputs.</exception>
    TenantContext InitializeAdmin(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);

    /// <summary>
    /// Initializes tenant context for a CLI or script execution flow.
    /// </summary>
    /// <param name="scope">The tenant scope for the scripted execution.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="attributionInputs">Attribution inputs captured during initialization.</param>
    /// <returns>The initialized tenant context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scope or traceId is null/whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when already initialized with conflicting inputs.</exception>
    TenantContext InitializeScripted(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);

    /// <summary>
    /// Clears the current tenant context.
    /// </summary>
    /// <remarks>
    /// Should be called in a finally block after flow completes to prevent context leakage.
    /// </remarks>
    void Clear();
}
