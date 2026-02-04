namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Represents a single attribution input used to establish tenant context.
/// </summary>
/// <param name="Source">Attribution source type.</param>
/// <param name="TenantId">Tenant identifier supplied by the source (if any).</param>
public sealed record TenantAttributionInput(
    TenantAttributionSource Source,
    TenantId? TenantId);
