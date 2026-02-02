using System.Text;

namespace TenantSaas.Abstractions.Invariants;

/// <summary>
/// Represents the refusal mapping for an invariant violation.
/// </summary>
public sealed record RefusalMapping
{
    /// <summary>
    /// Gets the invariant code.
    /// </summary>
    public string InvariantCode { get; }

    /// <summary>
    /// Gets the HTTP status code for refusal.
    /// </summary>
    public int HttpStatusCode { get; }

    /// <summary>
    /// Gets the RFC 7807 Problem Details type identifier.
    /// </summary>
    public string ProblemType { get; }

    /// <summary>
    /// Gets the Problem Details title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the guidance URI for remediation.
    /// </summary>
    public string GuidanceUri { get; }

    /// <summary>
    /// Creates a new refusal mapping.
    /// </summary>
    /// <param name="invariantCode">Stable invariant code identifier.</param>
    /// <param name="httpStatusCode">HTTP status code (400-599).</param>
    /// <param name="problemType">RFC 7807 Problem Details type identifier.</param>
    /// <param name="title">Problem Details title.</param>
    /// <param name="guidanceUri">Guidance URI for remediation.</param>
    /// <exception cref="ArgumentException">Thrown when any argument is invalid.</exception>
    public RefusalMapping(
        string invariantCode,
        int httpStatusCode,
        string problemType,
        string title,
        string guidanceUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);
        ArgumentOutOfRangeException.ThrowIfLessThan(httpStatusCode, 400);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(httpStatusCode, 600);
        ArgumentException.ThrowIfNullOrWhiteSpace(problemType);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(guidanceUri);
        if (!Uri.IsWellFormedUriString(guidanceUri, UriKind.Absolute))
        {
            throw new ArgumentException("GuidanceUri must be a valid absolute URI.", nameof(guidanceUri));
        }

        InvariantCode = invariantCode;
        HttpStatusCode = httpStatusCode;
        ProblemType = problemType;
        Title = title;
        GuidanceUri = guidanceUri;
    }

    /// <summary>
    /// Creates a refusal mapping for bad request scenarios.
    /// </summary>
    public static RefusalMapping ForBadRequest(
        string invariantCode,
        string title,
        string guidanceUri)
        => new(
            invariantCode,
            httpStatusCode: 400,
            problemType: BuildProblemType(invariantCode),
            title,
            guidanceUri);

    /// <summary>
    /// Creates a refusal mapping for forbidden scenarios.
    /// </summary>
    public static RefusalMapping ForForbidden(
        string invariantCode,
        string title,
        string guidanceUri)
        => new(
            invariantCode,
            httpStatusCode: 403,
            problemType: BuildProblemType(invariantCode),
            title,
            guidanceUri);

    /// <summary>
    /// Creates a refusal mapping for unprocessable entity scenarios.
    /// </summary>
    public static RefusalMapping ForUnprocessableEntity(
        string invariantCode,
        string title,
        string guidanceUri)
        => new(
            invariantCode,
            httpStatusCode: 422,
            problemType: BuildProblemType(invariantCode),
            title,
            guidanceUri);

    /// <summary>
    /// Creates a refusal mapping for internal server error scenarios.
    /// </summary>
    public static RefusalMapping ForInternalServerError(
        string invariantCode,
        string title,
        string guidanceUri)
        => new(
            invariantCode,
            httpStatusCode: 500,
            problemType: BuildProblemType(invariantCode),
            title,
            guidanceUri);

    private static string BuildProblemType(string invariantCode)
        => $"urn:tenantsaas:error:{ToKebabCase(invariantCode)}";

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 8);

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];

            if (char.IsUpper(character))
            {
                if (index > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(character));
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
