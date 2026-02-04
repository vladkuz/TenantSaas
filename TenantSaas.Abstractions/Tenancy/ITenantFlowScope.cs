namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Represents a scoped execution flow for non-request tenant operations.
/// </summary>
/// <remarks>
/// <para>
/// Implements both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> 
/// to automatically clear tenant context when the flow completes.
/// </para>
/// <para>
/// Usage pattern:
/// <code>
/// using var flow = flowFactory.CreateBackgroundFlow(scope, traceId);
/// // flow.Context is available for the duration of the using block
/// await DoWork();
/// // Context is automatically cleared on disposal
/// </code>
/// </para>
/// </remarks>
public interface ITenantFlowScope : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the tenant context established for this flow.
    /// </summary>
    TenantContext Context { get; }
}
