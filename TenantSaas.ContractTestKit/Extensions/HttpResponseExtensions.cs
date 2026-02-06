using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace TenantSaas.ContractTestKit.Extensions;

/// <summary>
/// Extension methods for working with HTTP responses in contract tests.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Reads Problem Details from an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Problem Details, or null if the response is not Problem Details.</returns>
    public static async Task<ProblemDetails?> ReadProblemDetailsAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.Content.Headers.ContentType?.MediaType?.Contains("json") ?? true)
        {
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if an HTTP response is a Problem Details error response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>True if the response appears to be Problem Details.</returns>
    public static bool IsProblemDetails(this HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        return contentType == "application/problem+json" ||
               (contentType == "application/json" && !response.IsSuccessStatusCode);
    }

    /// <summary>
    /// Gets the invariant code from a Problem Details response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invariant code, or null if not present.</returns>
    public static async Task<string?> GetInvariantCodeAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var problemDetails = await response.ReadProblemDetailsAsync(cancellationToken);
        if (problemDetails?.Extensions.TryGetValue("invariantCode", out var value) ?? false)
        {
            return value?.ToString();
        }

        return null;
    }
}
