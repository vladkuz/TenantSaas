namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Captures attribution inputs used to establish tenant context.
/// </summary>
public sealed record TenantAttributionInputs(IReadOnlyList<TenantAttributionInput> Inputs)
{
    /// <inheritdoc />
    public bool Equals(TenantAttributionInputs? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        if (Inputs.Count != other.Inputs.Count)
        {
            return false;
        }

        for (var index = 0; index < Inputs.Count; index++)
        {
            if (!Equals(Inputs[index], other.Inputs[index]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var input in Inputs)
        {
            hash.Add(input);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Creates an empty attribution input set.
    /// </summary>
    public static TenantAttributionInputs None()
        => new([]);

    /// <summary>
    /// Creates attribution inputs from explicit initialization scope.
    /// </summary>
    public static TenantAttributionInputs FromExplicitScope(TenantScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return scope switch
        {
            TenantScope.Tenant tenantScope => new(
                [
                    new TenantAttributionInput(
                        TenantAttributionSource.ExplicitContext,
                        tenantScope.Id)
                ]),
            _ => new(
                [
                    new TenantAttributionInput(
                        TenantAttributionSource.ExplicitContext,
                        TenantId: null)
                ])
        };
    }

    /// <summary>
    /// Creates attribution inputs from resolved source inputs.
    /// </summary>
    public static TenantAttributionInputs FromSources(
        IReadOnlyDictionary<TenantAttributionSource, TenantId> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        if (sources.Count == 0)
        {
            return None();
        }

        var inputs = sources
            .OrderBy(entry => entry.Key)
            .Select(entry => new TenantAttributionInput(entry.Key, entry.Value))
            .ToArray();

        return new TenantAttributionInputs(inputs);
    }
}
