using TenantSaas.Abstractions.Invariants;

namespace TenantSaas.Abstractions.BreakGlass;

/// <summary>
/// Validates break-glass declarations for privileged or cross-tenant operations.
/// </summary>
/// <remarks>
/// Defense-in-depth: field validation duplicates constructor checks to catch
/// declarations created via unsafe deserialization or reflection.
/// </remarks>
public static class BreakGlassValidator
{
    /// <summary>
    /// Validates a break-glass declaration for required fields.
    /// </summary>
    public static BreakGlassValidationResult Validate(BreakGlassDeclaration? declaration)
    {
        if (declaration is null)
        {
            return BreakGlassValidationResult.Invalid(
                InvariantCode.BreakGlassExplicitAndAudited,
                "Break-glass declaration is required.");
        }

        if (string.IsNullOrWhiteSpace(declaration.ActorId))
        {
            return BreakGlassValidationResult.Invalid(
                InvariantCode.BreakGlassExplicitAndAudited,
                "Break-glass actor identity is required.");
        }

        if (string.IsNullOrWhiteSpace(declaration.Reason))
        {
            return BreakGlassValidationResult.Invalid(
                InvariantCode.BreakGlassExplicitAndAudited,
                "Break-glass reason is required.");
        }

        if (string.IsNullOrWhiteSpace(declaration.DeclaredScope))
        {
            return BreakGlassValidationResult.Invalid(
                InvariantCode.BreakGlassExplicitAndAudited,
                "Break-glass declared scope is required.");
        }

        return BreakGlassValidationResult.Valid();
    }
}
