using TenantSaas.Abstractions.Invariants;

namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Validates tenant disclosure against safe disclosure rules.
/// </summary>
public static class DisclosureValidator
{
    /// <summary>
    /// Validates disclosure of tenant reference based on context.
    /// </summary>
    public static DisclosureValidationResult Validate(DisclosureContext context, string? disclosedTenantRef)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(disclosedTenantRef))
        {
            return DisclosureValidationResult.Valid();
        }

        if (context.IsEnumerationRisk && !TenantRefSafeState.IsSafeState(disclosedTenantRef))
        {
            return DisclosureValidationResult.Invalid(
                InvariantCode.DisclosureSafe,
                "Tenant disclosure is unsafe when enumeration risk is present.");
        }

        var disclosureUnsafe = !context.IsAuthenticated || !context.IsAuthorizedForTenant;
        if (disclosureUnsafe && !TenantRefSafeState.IsSafeState(disclosedTenantRef))
        {
            return DisclosureValidationResult.Invalid(
                InvariantCode.DisclosureSafe,
                "Tenant disclosure is unsafe for unauthenticated or unauthorized callers.");
        }

        return DisclosureValidationResult.Valid();
    }
}
