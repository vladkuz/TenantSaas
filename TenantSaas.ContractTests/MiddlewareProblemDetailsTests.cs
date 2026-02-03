using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Tenancy;
using TenantSaas.Sample.Middleware;
using System.Text.Json;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests;

/// <summary>
/// End-to-end tests for middleware Problem Details integration.
/// Validates that middleware returns standardized RFC 7807 Problem Details for invariant violations.
/// </summary>
public class MiddlewareProblemDetailsTests
{
    [Fact]
    public async Task TenantContextMiddleware_MissingAttribution_ReturnsStandardizedProblemDetails()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "test-trace-middleware";
        context.Response.Body = new MemoryStream(); // Enable response body capture

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Response is Problem Details
        context.Response.StatusCode.Should().Be(422, "Ambiguous attribution returns HTTP 422");

        // Read response body
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseBody.Should().NotBeEmpty();

        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        problemDetails.Should().NotBeNull();

        // Validate Problem Details structure
        problemDetails!.Type.Should().StartWith("urn:tenantsaas:error:");
        problemDetails.Title.Should().NotBeNullOrWhiteSpace();
        problemDetails.Status.Should().Be(422);
        problemDetails.Detail.Should().NotBeNullOrWhiteSpace();

        // Validate required extensions
        problemDetails.Extensions.Should().ContainKey(InvariantCodeKey);
        problemDetails.Extensions.Should().ContainKey(TraceId);
        problemDetails.Extensions[TraceId]?.ToString().Should().Be("test-trace-middleware");
        problemDetails.Extensions.Should().ContainKey(RequestId);
        problemDetails.Extensions.Should().ContainKey(GuidanceLink);
    }

    [Fact]
    public async Task TenantContextMiddleware_HealthCheck_SkipsTenantRequirement()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var nextCalled = false;

        var middleware = new TenantContextMiddleware(
            next: _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            accessor: accessor,
            attributionResolver: attributionResolver);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        context.TraceIdentifier = "test-trace-health";

        // Act
        await middleware.InvokeAsync(context);

        // Assert - No Problem Details, next middleware called
        nextCalled.Should().BeTrue("Health check should bypass tenant requirement");
        context.Response.StatusCode.Should().Be(200, "Health check should succeed");
    }

    [Fact]
    public async Task TenantContextMiddleware_HealthCheck_BypassesAttribution()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var nextCalled = false;

        var middleware = new TenantContextMiddleware(
            next: _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            accessor: accessor,
            attributionResolver: attributionResolver);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        context.TraceIdentifier = "test-trace-health";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Health check bypasses attribution, next middleware called
        nextCalled.Should().BeTrue("Health check should bypass tenant requirement and call next");
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ProblemDetailsExceptionMiddleware_UnhandledException_ReturnsGeneric500()
    {
        // Arrange
        var logger = new TestLogger<ProblemDetailsExceptionMiddleware>();
        var middleware = new ProblemDetailsExceptionMiddleware(
            next: _ => throw new InvalidOperationException("Test exception"),
            logger: logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/error";
        context.TraceIdentifier = "test-trace-exception";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Generic 500 Problem Details
        context.Response.StatusCode.Should().Be(500);

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Type.Should().Be("urn:tenantsaas:error:internal-server-error");
        problemDetails.Title.Should().Be("Internal server error");
        problemDetails.Status.Should().Be(500);
        problemDetails.Detail.Should().Contain("trace ID", "Generic error should instruct user to provide trace ID");

        // Validate extensions
        problemDetails.Extensions.Should().ContainKey(InvariantCodeKey);
        problemDetails.Extensions[InvariantCodeKey]?.ToString().Should().Be("InternalServerError");
        problemDetails.Extensions.Should().ContainKey(TraceId);
        problemDetails.Extensions[TraceId]?.ToString().Should().Be("test-trace-exception");
        problemDetails.Extensions.Should().ContainKey(RequestId);
        problemDetails.Extensions.Should().ContainKey(GuidanceLink);
        problemDetails.Extensions[GuidanceLink]?.ToString().Should().Be("https://docs.tenantsaas.dev/errors/internal-server-error");

        // Verify exception was logged
        logger.LoggedMessages.Should().Contain(msg => msg.Contains("Unhandled exception"));
    }

    [Fact]
    public async Task ProblemDetailsExceptionMiddleware_NeverLeaksExceptionDetails()
    {
        // Arrange
        var logger = new TestLogger<ProblemDetailsExceptionMiddleware>();
        var sensitiveMessage = "SECRET_DATABASE_PASSWORD_12345";

        var middleware = new ProblemDetailsExceptionMiddleware(
            next: _ => throw new InvalidOperationException(sensitiveMessage),
            logger: logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/error";
        context.TraceIdentifier = "test-trace-leak";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Exception message NOT in response
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        responseBody.Should().NotContain(sensitiveMessage,
            "Exception details must never leak to client");
        responseBody.Should().NotContain("InvalidOperationException",
            "Exception type must not leak to client");
    }

    [Fact]
    public async Task ProblemDetailsExceptionMiddleware_ResponseAlreadyStarted_RethrowsException()
    {
        // Arrange
        var logger = new TestLogger<ProblemDetailsExceptionMiddleware>();
        var testException = new InvalidOperationException("Test exception for HasStarted");

        var middleware = new ProblemDetailsExceptionMiddleware(
            next: _ => throw testException,
            logger: logger);

        // Create HttpContext with HasStarted = true using mocking
        var context = CreateHttpContextWithResponseStarted();

        // Act & Assert - Exception should be rethrown when response has started
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
        
        thrownException.Should().BeSameAs(testException);
        
        // Exception was still logged
        logger.LoggedMessages.Should().Contain(msg => msg.Contains("Unhandled exception"));
    }

    [Fact]
    public async Task TenantContextMiddleware_ResponseAlreadyStarted_DoesNotWriteProblemDetails()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver);

        // Create HttpContext with HasStarted = true using mocking
        var (context, responseBodyMock) = CreateHttpContextWithResponseStartedAndBody();

        // Act - Should not throw, just return without writing
        await middleware.InvokeAsync(context);

        // Assert - No content written to response body
        responseBodyMock.Length.Should().Be(0,
            "No content should be written when response has already started");
    }

    private static HttpContext CreateHttpContextWithResponseStarted()
    {
        var responseMock = new Mock<HttpResponse>();
        responseMock.Setup(r => r.HasStarted).Returns(true);
        responseMock.Setup(r => r.Body).Returns(new MemoryStream());
        responseMock.Setup(r => r.Headers).Returns(new HeaderDictionary());

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Path).Returns("/api/test");
        requestMock.Setup(r => r.Headers).Returns(new HeaderDictionary());

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(c => c.Response).Returns(responseMock.Object);
        contextMock.Setup(c => c.Request).Returns(requestMock.Object);
        contextMock.Setup(c => c.TraceIdentifier).Returns("test-trace-started");

        return contextMock.Object;
    }

    private static (HttpContext Context, MemoryStream ResponseBody) CreateHttpContextWithResponseStartedAndBody()
    {
        var responseBody = new MemoryStream();

        var responseMock = new Mock<HttpResponse>();
        responseMock.Setup(r => r.HasStarted).Returns(true);
        responseMock.Setup(r => r.Body).Returns(responseBody);
        responseMock.Setup(r => r.Headers).Returns(new HeaderDictionary());

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Path).Returns("/api/tenants");
        requestMock.Setup(r => r.Headers).Returns(new HeaderDictionary());
        requestMock.Setup(r => r.RouteValues).Returns(new RouteValueDictionary());

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(c => c.Response).Returns(responseMock.Object);
        contextMock.Setup(c => c.Request).Returns(requestMock.Object);
        contextMock.Setup(c => c.TraceIdentifier).Returns("test-trace-started");

        return (contextMock.Object, responseBody);
    }

    /// <summary>
    /// Simple test logger that captures log messages for validation.
    /// </summary>
    private class TestLogger<T> : ILogger<T>
    {
        public List<string> LoggedMessages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            LoggedMessages.Add(formatter(state, exception));
        }
    }
}
