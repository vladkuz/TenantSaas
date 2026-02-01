namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Represents the result of tenant attribution resolution.
/// </summary>
public abstract record TenantAttributionResult
{
    private TenantAttributionResult()
    {
    }

    /// <summary>
    /// Tenant attribution resolved successfully from a single source.
    /// </summary>
    public sealed record Success : TenantAttributionResult
    {
        /// <summary>
        /// Creates a successful attribution result.
        /// </summary>
        public Success(TenantId tenantId, TenantAttributionSource source)
        {
            if (string.IsNullOrWhiteSpace(tenantId.Value))
            {
                throw new ArgumentException("Tenant identifier is required.", nameof(tenantId));
            }

            TenantId = tenantId;
            Source = source;
        }

        /// <summary>
        /// Gets the resolved tenant identifier.
        /// </summary>
        public TenantId TenantId { get; }

        /// <summary>
        /// Gets the attribution source that resolved the tenant.
        /// </summary>
        public TenantAttributionSource Source { get; }
    }

    /// <summary>
    /// Tenant attribution is ambiguous due to conflicting sources.
    /// </summary>
    public sealed record Ambiguous : TenantAttributionResult
    {
        /// <summary>
        /// Creates an ambiguous attribution result.
        /// </summary>
        public Ambiguous(IReadOnlyList<AttributionConflict> conflicts)
        {
            ArgumentNullException.ThrowIfNull(conflicts);

            if (conflicts.Count == 0)
            {
                throw new ArgumentException("Ambiguous attribution must include conflicts.", nameof(conflicts));
            }

            Conflicts = conflicts;
        }

        /// <summary>
        /// Gets the conflicting attribution sources.
        /// </summary>
        public IReadOnlyList<AttributionConflict> Conflicts { get; }
    }

    /// <summary>
    /// No tenant attribution sources provided a tenant identifier.
    /// </summary>
    public sealed record NotFound : TenantAttributionResult
    {
        internal NotFound()
        {
        }
    }

    /// <summary>
    /// A tenant attribution source was provided but is not allowed.
    /// </summary>
    public sealed record NotAllowed : TenantAttributionResult
    {
        /// <summary>
        /// Creates a not-allowed attribution result.
        /// </summary>
        public NotAllowed(TenantAttributionSource source)
        {
            Source = source;
        }

        /// <summary>
        /// Gets the disallowed source.
        /// </summary>
        public TenantAttributionSource Source { get; }
    }

    /// <summary>
    /// Creates a successful attribution result.
    /// </summary>
    public static TenantAttributionResult Succeeded(TenantId id, TenantAttributionSource source)
        => new Success(id, source);

    /// <summary>
    /// Creates an ambiguous attribution result.
    /// </summary>
    public static TenantAttributionResult IsAmbiguous(IReadOnlyList<AttributionConflict> conflicts)
        => new Ambiguous(conflicts);

    /// <summary>
    /// Creates a not-found attribution result.
    /// </summary>
    public static TenantAttributionResult WasNotFound()
        => new NotFound();

    /// <summary>
    /// Creates a not-allowed attribution result.
    /// </summary>
    public static TenantAttributionResult IsNotAllowed(TenantAttributionSource source)
        => new NotAllowed(source);
}

/// <summary>
/// Describes a conflict between two attribution sources.
/// </summary>
public readonly record struct AttributionConflict(
    TenantAttributionSource Source,
    TenantId ProvidedTenantId);
