using FluentAssertions;
using Microsoft.AspNetCore.Http;
using TenantSaas.Sample.Middleware;

namespace TenantSaas.ContractTests;

/// <summary>
/// Tests for HttpCorrelationExtensions to verify proper trace_id and request_id extraction
/// from distributed tracing headers and fallback behavior.
/// </summary>
public class HttpCorrelationExtensionsTests
{
    [Fact]
    public void GetTraceId_WithTraceparentHeader_ExtractsTraceId()
    {
        // Arrange - W3C Trace Context format: version-trace_id-parent_id-flags
        var context = new DefaultHttpContext();
        context.Request.Headers["traceparent"] = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01";

        // Act
        var traceId = context.GetTraceId();

        // Assert
        traceId.Should().Be("0af7651916cd43dd8448eb211c80319c");
    }

    [Fact]
    public void GetTraceId_WithXTraceIdHeader_UsesXTraceId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Trace-Id"] = "custom-trace-id-12345";

        // Act
        var traceId = context.GetTraceId();

        // Assert
        traceId.Should().Be("custom-trace-id-12345");
    }

    [Fact]
    public void GetTraceId_WithNoHeaders_FallsBackToTraceIdentifier()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "aspnet-trace-identifier";

        // Act
        var traceId = context.GetTraceId();

        // Assert
        traceId.Should().Be("aspnet-trace-identifier");
    }

    [Fact]
    public void GetTraceId_TraceparentTakesPrecedenceOverXTraceId()
    {
        // Arrange - W3C traceparent has valid 32 hex character trace-id
        var context = new DefaultHttpContext();
        context.Request.Headers["traceparent"] = "00-a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6-b7ad6b7169203331-01";
        context.Request.Headers["X-Trace-Id"] = "custom-trace-id";

        // Act
        var traceId = context.GetTraceId();

        // Assert - traceparent should win
        traceId.Should().Be("a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6");
    }

    [Fact]
    public void GetRequestId_WithXRequestIdHeader_UsesHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Request-ID"] = "client-request-12345";
        context.TraceIdentifier = "server-generated-id";

        // Act
        var requestId = context.GetRequestId();

        // Assert
        requestId.Should().Be("client-request-12345");
    }

    [Fact]
    public void GetRequestId_WithNoHeader_FallsBackToTraceIdentifier()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "server-generated-id";

        // Act
        var requestId = context.GetRequestId();

        // Assert
        requestId.Should().Be("server-generated-id");
    }

    [Fact]
    public void GetCorrelationIds_ReturnsBothIds()
    {
        // Arrange - W3C traceparent format: 00-{32 hex trace-id}-{16 hex parent-id}-{2 hex flags}
        var context = new DefaultHttpContext();
        context.Request.Headers["traceparent"] = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01";
        context.Request.Headers["X-Request-ID"] = "request-specific-id";

        // Act
        var (traceId, requestId) = context.GetCorrelationIds();

        // Assert
        traceId.Should().Be("0af7651916cd43dd8448eb211c80319c");
        requestId.Should().Be("request-specific-id");
    }

    [Fact]
    public void GetCorrelationIds_DifferentiatesTraceIdAndRequestId()
    {
        // Arrange - No distributed tracing headers, different fallback behavior
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "aspnet-identifier";
        // No headers - both should fall back but trace_id and request_id have different meanings

        // Act
        var (traceId, requestId) = context.GetCorrelationIds();

        // Assert - Both fall back to TraceIdentifier when no headers present
        traceId.Should().Be("aspnet-identifier");
        requestId.Should().Be("aspnet-identifier");
    }

    [Fact]
    public void GetTraceId_WithEmptyTraceparent_FallsBackToNextSource()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["traceparent"] = "";
        context.Request.Headers["X-Trace-Id"] = "fallback-trace-id";

        // Act
        var traceId = context.GetTraceId();

        // Assert
        traceId.Should().Be("fallback-trace-id");
    }

    [Fact]
    public void GetTraceId_WithMalformedTraceparent_FallsBackToNextSource()
    {
        // Arrange - traceparent with insufficient parts
        var context = new DefaultHttpContext();
        context.Request.Headers["traceparent"] = "00";
        context.Request.Headers["X-Trace-Id"] = "fallback-trace-id";

        // Act
        var traceId = context.GetTraceId();

        // Assert
        traceId.Should().Be("fallback-trace-id");
    }

    [Fact]
    public void GetRequestId_WithEmptyHeader_FallsBackToTraceIdentifier()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Request-ID"] = "";
        context.TraceIdentifier = "server-generated";

        // Act
        var requestId = context.GetRequestId();

        // Assert
        requestId.Should().Be("server-generated");
    }
}
