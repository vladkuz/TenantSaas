namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Factory for creating scoped tenant execution flows for non-request operations.
/// </summary>
/// <remarks>
/// <para>
/// This factory creates <see cref="ITenantFlowScope"/> instances that automatically
/// manage tenant context lifecycle for background, admin, and scripted execution flows.
/// </para>
/// <para>
/// Each flow type sets the appropriate <see cref="Contexts.ExecutionKind"/> and requires
/// explicit initialization inputs (scope, traceId, optional attribution).
/// </para>
/// <para>
/// Example usage:
/// <code>
/// // Background job
/// using var flow = flowFactory.CreateBackgroundFlow(scope, traceId);
/// await ProcessJobAsync();
/// 
/// // Admin operation
/// using var flow = flowFactory.CreateAdminFlow(scope, traceId);
/// await PerformMaintenanceAsync();
/// 
/// // CLI script
/// using var flow = flowFactory.CreateScriptedFlow(scope, traceId);
/// await RunMigrationAsync();
/// </code>
/// </para>
/// </remarks>
public interface ITenantFlowFactory
{
    /// <summary>
    /// Creates a scoped flow for background job or worker execution.
    /// </summary>
    /// <param name="scope">The tenant scope for the background job.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="attributionInputs">Optional attribution inputs captured during initialization.</param>
    /// <returns>A flow scope that will clear context on disposal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scope is null.</exception>
    /// <exception cref="ArgumentException">Thrown when traceId is null or whitespace.</exception>
    ITenantFlowScope CreateBackgroundFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);

    /// <summary>
    /// Creates a scoped flow for administrative operation execution.
    /// </summary>
    /// <param name="scope">The tenant scope for the admin operation.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="attributionInputs">Optional attribution inputs captured during initialization.</param>
    /// <returns>A flow scope that will clear context on disposal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scope is null.</exception>
    /// <exception cref="ArgumentException">Thrown when traceId is null or whitespace.</exception>
    ITenantFlowScope CreateAdminFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);

    /// <summary>
    /// Creates a scoped flow for CLI or script execution.
    /// </summary>
    /// <param name="scope">The tenant scope for the scripted execution.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="attributionInputs">Optional attribution inputs captured during initialization.</param>
    /// <returns>A flow scope that will clear context on disposal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scope is null.</exception>
    /// <exception cref="ArgumentException">Thrown when traceId is null or whitespace.</exception>
    ITenantFlowScope CreateScriptedFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);
}
