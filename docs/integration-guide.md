# TenantSaas Integration Guide

This guide helps you integrate TenantSaas into your multi-tenant application.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Context Initialization](#context-initialization)
3. [Ambient vs Explicit Context Propagation](#ambient-vs-explicit-context-propagation)
4. [Flow Wrappers for Non-Request Execution](#flow-wrappers-for-non-request-execution)
5. [Middleware Setup](#middleware-setup)
6. [Handling Invariant Violations](#handling-invariant-violations)
7. [Error Handling Best Practices](#error-handling-best-practices)
8. [Testing Integration](#testing-integration)

---

## Getting Started

TenantSaas provides a trust contract-based framework for building multi-tenant SaaS applications with strong tenant isolation guarantees.

### Core Concepts

- **Tenant Context**: Container for tenant scope, execution kind, and correlation IDs
- **Context Initialization**: Single required primitive per flow (request, background, admin, scripted)
- **Invariants**: Named, testable rules enforced at boundaries (e.g., context initialized, attribution unambiguous)
- **Problem Details**: RFC 7807 standardized error responses with stable machine-readable identifiers

---

## Context Initialization

### Initialization Primitive

TenantSaas provides `ITenantContextInitializer` as the single required entry point for establishing tenant context in each execution flow.

**Key characteristics:**
- **Idempotent**: Repeated calls with identical inputs return the same context
- **Validated**: Conflicting inputs throw `InvalidOperationException`
- **Flow-specific**: Separate methods for Request, Background, Admin, and Scripted flows

### Initialization Methods

```csharp
public interface ITenantContextInitializer
{
    // HTTP/API request flows
    TenantContext InitializeRequest(
        TenantScope scope,
        string traceId,
        string requestId,
        TenantAttributionInputs? attributionInputs = null);
    
    // Background jobs/workers
    TenantContext InitializeBackground(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);
    
    // Administrative operations
    TenantContext InitializeAdmin(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);
    
    // CLI/script execution
    TenantContext InitializeScripted(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);
    
    // Cleanup (call in finally block)
    void Clear();
}
```

### Example: Request Flow

```csharp
// In middleware or request handler
var scope = TenantScope.ForTenant(new TenantId("tenant-123"));
var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);
var context = initializer.InitializeRequest(scope, traceId, requestId, attributionInputs);

try
{
    // Process request - context is available via ITenantContextAccessor
    await ProcessRequest();
}
finally
{
    // Always clear to prevent context leakage in pooled environments
    initializer.Clear();
}
```

### Example: Background Job

```csharp
// In background job worker
public async Task ExecuteAsync(CancellationToken cancellationToken)
{
    var scope = TenantScope.ForTenant(new TenantId("tenant-xyz"));
    var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
    
    var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);
    var context = initializer.InitializeBackground(scope, traceId, attributionInputs);
    
    try
    {
        await ProcessJob();
    }
    finally
    {
        initializer.Clear();
    }
}
```

### Explicit Context Passing (Without Ambient)

For framework-free scenarios or when you want to avoid ambient context dependencies, you can pass `TenantContext` explicitly to enforcement boundaries.

**When to use explicit context:**
- Framework-free console applications or workers
- Unit testing without DI container setup
- Scenarios where ambient propagation is undesirable
- When you want full control over context lifetime

**Example: Direct enforcement with explicit context**

```csharp
public class ExplicitContextService
{
    private readonly IBoundaryGuard guard;

    public ExplicitContextService(IBoundaryGuard guard)
    {
        this.guard = guard;
    }

    public Result ProcessWithExplicitContext(string tenantId)
    {
        // Create context directly (no initializer or accessor required)
        var scope = TenantScope.ForTenant(new TenantId(tenantId));
        var context = TenantContext.ForRequest(
            scope,
            traceId: Guid.NewGuid().ToString("N"),
            requestId: Guid.NewGuid().ToString("N"),
            TenantAttributionInputs.FromExplicitScope(scope));

        // Pass context explicitly to enforcement boundary
        var enforcementResult = guard.RequireContext(context);
        
        if (!enforcementResult.IsSuccess)
        {
            // Handle refusal
            return Result.Failure(enforcementResult.Detail);
        }

        // Continue with business logic
        return Result.Success();
    }
}
```

**Example: Hybrid mode (explicit takes precedence)**

```csharp
// When both ambient and explicit contexts are available,
// explicit context takes precedence over ambient
var enforcementResult = guard.RequireContext(accessor, explicitContext);

// The guard will use explicitContext if non-null,
// otherwise it falls back to accessor.Current
```

**Precedence rules:**
1. Explicit context (when provided and non-null) always takes precedence
2. Ambient context (via `ITenantContextAccessor`) is used as fallback
3. Null explicit context is treated as missing and results in refusal

---

## Ambient vs Explicit Context Propagation

TenantSaas supports two context propagation strategies: **ambient** (automatic) and **explicit** (manual). Both are equally valid and provide the same enforcement guarantees.

### Ambient Context Propagation (Default)

**How it works:**
- Uses `AsyncLocal<T>` for automatic context flow
- Context propagates deterministically across async boundaries within the same flow
- No need to pass context explicitly through method parameters

**Deterministic async behavior:**
- ✅ Context flows across `await` boundaries
- ✅ Context flows into `Task.Run` and background continuations
- ✅ Context flows through nested async calls
- ✅ Context survives `ConfigureAwait(false)` (AsyncLocal is independent of SynchronizationContext)
- ✅ Context is visible in parallel tasks (`Task.WhenAll`, `Task.WhenAny`)

**Zero leakage guarantee:**
- New execution flows start with **empty context**
- Previous flow context does **not** leak if properly cleared
- **Middleware must call `Clear()` in a `finally` block** to prevent leakage in pooled environments

**Example: Ambient propagation in action**

```csharp
// Middleware initializes context once
public async Task InvokeAsync(HttpContext httpContext)
{
    var context = initializer.InitializeRequest(scope, traceId, requestId);
    
    try
    {
        // Context automatically available in all async calls
        await _next(httpContext);
    }
    finally
    {
        initializer.Clear(); // CRITICAL: prevent leakage
    }
}

// Deep in the call stack - no explicit passing needed
public async Task ProcessOrderAsync()
{
    // Context available via accessor
    var enforcementResult = guard.RequireContext(accessor);
    
    // Nested async calls see same context automatically
    await ValidateInventoryAsync();
    await ChargePaymentAsync();
}

private async Task ValidateInventoryAsync()
{
    // Context still available here
    var enforcementResult = guard.RequireContext(accessor);
    
    // Even Task.Run sees the same context
    await Task.Run(() => CheckStock());
}
```

**When to use ambient:**
- Standard web applications with request-scoped flows
- Background jobs with clear start/end boundaries
- When reducing plumbing is valuable
- When enforcement outcomes must be consistent across the entire flow

### Explicit Context Passing (Alternative)

**How it works:**
- Pass `TenantContext` directly through method parameters
- No ambient storage or accessor required
- Full control over context lifetime and scope

**Example: Explicit propagation**

```csharp
public async Task ProcessOrderAsync(TenantContext context)
{
    // Pass context explicitly to enforcement boundary
    var enforcementResult = guard.RequireContext(context);
    
    // Must pass context explicitly to nested calls
    await ValidateInventoryAsync(context);
    await ChargePaymentAsync(context);
}

private async Task ValidateInventoryAsync(TenantContext context)
{
    // Context passed explicitly
    var enforcementResult = guard.RequireContext(context);
    
    // Task.Run requires explicit capture
    await Task.Run(() => CheckStock(context));
}
```

**When to use explicit:**
- Framework-free console applications or CLI tools
- Unit tests without DI container setup
- When ambient propagation is undesirable for architectural reasons
- When you want explicit visibility of context flow in method signatures

### Choosing at DI Registration Time

The choice between ambient and explicit mode is made at DI registration:

```csharp
// Option 1: Ambient mode (default) - uses AsyncLocal propagation
var accessor = new AmbientTenantContextAccessor();
builder.Services.AddSingleton<ITenantContextAccessor>(accessor);
builder.Services.AddSingleton<IMutableTenantContextAccessor>(accessor);

// Option 2: Explicit mode - no automatic propagation
builder.Services.AddScoped<ITenantContextAccessor, ExplicitTenantContextAccessor>();
builder.Services.AddScoped<IMutableTenantContextAccessor, ExplicitTenantContextAccessor>();
```

**Note:** There is no runtime configuration switch. The propagation strategy is determined by which accessor implementation you register.

### Hybrid Mode (Best of Both Worlds)

You can mix ambient and explicit strategies. **Explicit context always takes precedence** when both are available.

```csharp
// Ambient context exists from middleware
var ambientContext = accessor.Current;

// But you can override with explicit context for specific operations
var adminScope = TenantScope.ForSharedSystem();
var adminContext = TenantContext.ForAdmin(adminScope, traceId);

// Explicit context wins
var result = guard.RequireContext(accessor, adminContext);
```

**Precedence rules (deterministic):**
1. **Explicit context** (if provided and non-null) → always wins
2. **Ambient context** (via accessor) → fallback if explicit is null
3. **No context** → refusal with `ContextInitialized` invariant

### Comparison Table

| Feature | Ambient | Explicit |
|---------|---------|----------|
| **Plumbing** | Minimal (no method parameters) | Explicit (context in signatures) |
| **Async propagation** | Automatic (AsyncLocal) | Manual (must pass through) |
| **Leakage risk** | Requires `Clear()` discipline | Lower (scoped lifetime) |
| **DI dependency** | Requires `ITenantContextAccessor` | Optional (can pass directly) |
| **Enforcement guarantees** | Identical | Identical |
| **Testability** | Requires accessor setup | Direct context injection |
| **Visibility** | Implicit (hidden in stack) | Explicit (visible in signatures) |

**Key Insight:** Both strategies enforce the same invariants and produce identical refusal behavior. Choose based on your architectural preferences and team conventions.

---

## Flow Wrappers for Non-Request Execution

For background jobs, admin tasks, and CLI scripts, TenantSaas provides **flow wrappers** that automatically manage context lifecycle. This eliminates manual `try`/`finally` blocks and ensures consistent cleanup.

### The `ITenantFlowFactory` Interface

```csharp
public interface ITenantFlowFactory
{
    // Background jobs/workers
    ITenantFlowScope CreateBackgroundFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);
    
    // Administrative operations
    ITenantFlowScope CreateAdminFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);
    
    // CLI/script execution
    ITenantFlowScope CreateScriptedFlow(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs? attributionInputs = null);
}

// The scope returned by the factory
public interface ITenantFlowScope : IDisposable, IAsyncDisposable
{
    TenantContext Context { get; }
}
```

### Background Job Pattern

```csharp
public class TenantJobProcessor(
    ITenantFlowFactory flowFactory,
    ITenantService service)
{
    public async Task ProcessJobAsync(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var scope = TenantScope.ForTenant(new TenantId(tenantId));
        var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

        // Flow wrapper handles initialization and cleanup automatically
        await using var flow = flowFactory.CreateBackgroundFlow(scope, traceId);
        
        // Context is available via flow.Context or accessor
        // ExecutionKind is automatically set to Background
        await service.DoWorkAsync(cancellationToken);
        
        // Context automatically cleared when flow scope is disposed
    }
}
```

### Admin Task Pattern

```csharp
public class MaintenanceService(ITenantFlowFactory flowFactory)
{
    public async Task RunTenantMaintenanceAsync(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var scope = TenantScope.ForTenant(new TenantId(tenantId));
        var traceId = Guid.NewGuid().ToString("N");

        await using var flow = flowFactory.CreateAdminFlow(scope, traceId);
        
        // ExecutionKind is Admin
        await CleanupOldDataAsync(cancellationToken);
        await OptimizeIndexesAsync(cancellationToken);
        
        // Cleanup is automatic
    }

    public async Task RunSystemMaintenanceAsync(
        CancellationToken cancellationToken)
    {
        // SharedSystem scope for cross-tenant operations
        var scope = TenantScope.ForSharedSystem();
        var traceId = Guid.NewGuid().ToString("N");

        await using var flow = flowFactory.CreateAdminFlow(scope, traceId);
        
        await GlobalCleanupAsync(cancellationToken);
    }
}
```

### CLI Script Pattern

```csharp
// Program.cs for a CLI tool
public static async Task<int> Main(string[] args)
{
    using var host = CreateHostBuilder(args).Build();
    var flowFactory = host.Services.GetRequiredService<ITenantFlowFactory>();

    var tenantId = args[0];
    var scope = TenantScope.ForTenant(new TenantId(tenantId));
    var traceId = Guid.NewGuid().ToString("N");
    var cancellationToken = CancellationToken.None;

    await using var flow = flowFactory.CreateScriptedFlow(scope, traceId);
    
    // ExecutionKind is Scripted
    var migrator = host.Services.GetRequiredService<IMigrationService>();
    await migrator.RunAsync(cancellationToken);
    
    return 0;
}
```

### Key Guarantees

| Guarantee | Description |
|-----------|-------------|
| **Explicit inputs required** | Must provide `TenantScope` and `traceId` - no implicit defaults |
| **Automatic cleanup** | Context cleared on disposal (both sync and async) |
| **Idempotent disposal** | Safe to call `Dispose()` multiple times |
| **Clean flow start** | New flow wrapper clears any prior ambient context |
| **Parallel isolation** | Concurrent flows remain isolated via AsyncLocal |
| **Enforcement equivalence** | Same invariant checks as manual initialization |

### Bypassed Wrapper Behavior

If you bypass the flow wrapper and don't initialize context, enforcement boundaries will **refuse**:

```csharp
public async Task BypassedExample()
{
    // No flow wrapper or initializer called
    // Context is NOT initialized
    
    var result = guard.RequireContext(accessor);
    // result.IsSuccess == false
    // result.InvariantCode == "ContextInitialized"
}
```

### DI Registration

```csharp
// Register all required services
builder.Services.AddSingleton<IMutableTenantContextAccessor, AmbientTenantContextAccessor>();
builder.Services.AddSingleton<ITenantContextAccessor>(sp =>
    sp.GetRequiredService<IMutableTenantContextAccessor>());
builder.Services.AddScoped<ITenantContextInitializer, TenantContextInitializer>();
builder.Services.AddScoped<ITenantFlowFactory, TenantFlowFactory>();

// For background/admin/scripted flows, resolve ITenantFlowFactory from a scope
// created per job/command (e.g., using IServiceScopeFactory).
```

### Execution Kind and Scope for Downstream Use

Every `TenantContext` carries first-class fields for **execution kind** and **tenant scope** that must be available to downstream enforcement, logging, and audit mapping.

**Execution Kind Taxonomy:**

| Value | Description | Use Case |
|-------|-------------|----------|
| `request` | HTTP or API request flow | Endpoints, controllers |
| `background` | Background job or worker flow | Hangfire jobs, IHostedService |
| `admin` | Administrative operation flow | Maintenance tasks, system operations |
| `scripted` | CLI or script execution flow | Console apps, migrations |

**Tenant Scope Types:**

| Type | Description | Example |
|------|-------------|---------|
| `Tenant` | Scoped to a specific tenant | Normal tenant operations |
| `SharedSystem` | Cross-tenant system operations | Global maintenance, reporting |
| `NoTenant` | Explicit no-tenant context | Health checks, public endpoints |

**Downstream Guarantees:**

1. **Enforcement**: `BoundaryGuard.RequireContext()` validates that context is initialized and contains valid execution kind and scope. Missing or invalid values result in a `ContextInitialized` refusal with Problem Details.

2. **Logging**: `DefaultLogEnricher.Enrich()` always emits `ExecutionKind` and `ScopeType` in `StructuredLogEvent`. These fields are required for audit trail correlation.

3. **Audit Mapping**: Execution kind and scope inform audit event categorization and routing decisions.

**Example: Accessing execution metadata in business logic**

```csharp
public class TenantService(ITenantContextAccessor accessor)
{
    public string GetOperationCategory()
    {
        var context = accessor.Current;
        if (context is null) throw new InvalidOperationException("Context not initialized");

        // ExecutionKind and Scope are always present on a valid context
        // Use static members for type-safe comparison
        return context.ExecutionKind switch
        {
            _ when context.ExecutionKind == ExecutionKind.Request => "user-initiated",
            _ when context.ExecutionKind == ExecutionKind.Background => "automated",
            _ when context.ExecutionKind == ExecutionKind.Admin => "administrative",
            _ when context.ExecutionKind == ExecutionKind.Scripted => "scripted",
            _ => "unknown"
        };
    }
}
```

**Invariant Enforcement:**

If context is missing or not properly initialized, enforcement boundaries refuse with:

```json
{
  "type": "https://tenantsaas.example.com/errors/context-initialized",
  "title": "Tenant context not initialized",
  "status": 500,
  "detail": "Tenant context must be initialized before operations can proceed.",
  "instance": "/api/example",
  "invariant_code": "ContextInitialized",
  "trace_id": "abc123..."
}
```

---

## Middleware Setup

### 1. Configure Services

```csharp
// Register context accessor
builder.Services.AddSingleton<IMutableTenantContextAccessor, AmbientTenantContextAccessor>();

// Register context initializer (scoped for request flows)
builder.Services.AddScoped<ITenantContextInitializer, TenantContextInitializer>();

// Register attribution resolver
builder.Services.AddSingleton<ITenantAttributionResolver, TenantAttributionResolver>();
```

### 2. Add Middleware

```csharp
// Add exception handling first (outermost middleware)
app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

// Add tenant context middleware
app.UseMiddleware<TenantContextMiddleware>();

// Your Minimal API endpoints follow
app.MapGet("/tenants/{tenantId}/data", (string tenantId, ITenantContextAccessor accessor) =>
{
    var context = accessor.Current;
    return Results.Ok(new { tenantId = context!.Scope });
});
```

**Important:** Order matters! `ProblemDetailsExceptionMiddleware` should be first to catch all unhandled exceptions.

---

## Handling Invariant Violations

All invariant violations in TenantSaas return standardized RFC 7807 Problem Details responses.

### Understanding Problem Details Structure

```json
{
  "type": "urn:tenantsaas:error:{invariant-name}",
  "title": "Human-readable title",
  "status": 4xx or 5xx,
  "detail": "Human-readable explanation",
  "instance": null,
  "invariant_code": "InvariantCodeName",
  "trace_id": "end-to-end-correlation-id",
  "request_id": "request-specific-id",
  "guidance_link": "https://docs.tenantsaas.dev/errors/{error-name}"
}
```

### Available Constants

TenantSaas provides constants for all Problem Details extension keys and invariant codes to avoid hardcoded strings:

```csharp
// Extension key constants (TenantSaas.Core.Errors.ProblemDetailsExtensions)
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

InvariantCodeKey     // "invariant_code"
TraceId              // "trace_id"
RequestId            // "request_id"
GuidanceLink         // "guidance_link"
ConflictingSources   // "conflicting_sources"

// Invariant code constants (TenantSaas.Abstractions.Invariants.InvariantCode)
using TenantSaas.Abstractions.Invariants;

InvariantCode.ContextInitialized           // "ContextInitialized"
InvariantCode.TenantAttributionUnambiguous // "TenantAttributionUnambiguous"
InvariantCode.TenantScopeRequired          // "TenantScopeRequired"
InvariantCode.BreakGlassExplicitAndAudited // "BreakGlassExplicitAndAudited"
InvariantCode.DisclosureSafe               // "DisclosureSafe"
```

### Client-Side Error Handling

#### C# Client Example

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TenantSaas.Abstractions.Invariants;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

public class TenantSaasClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TenantSaasClient> _logger;

    public TenantSaasClient(HttpClient httpClient, ILogger<TenantSaasClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<TEntity>> GetEntityAsync(string entityId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/entities/{entityId}");

            if (response.IsSuccessStatusCode)
            {
                var entity = await response.Content.ReadFromJsonAsync<TEntity>();
                return Result<TEntity>.Success(entity!);
            }

            // Parse Problem Details from error response
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return HandleProblemDetails(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling TenantSaas API");
            return Result<TEntity>.Failure("Unexpected error occurred");
        }
    }

    private Result<TEntity> HandleProblemDetails(ProblemDetails? problemDetails)
    {
        if (problemDetails is null)
        {
            return Result<TEntity>.Failure("Unknown error occurred");
        }

        // Extract invariant code for specific handling using standard extension keys
        var invariantCode = problemDetails.Extensions?[InvariantCodeKey]?.ToString();
        var traceId = problemDetails.Extensions?[TraceId]?.ToString();

        // Log for support correlation
        _logger.LogWarning(
            "API error: {InvariantCode}, Status: {Status}, TraceId: {TraceId}, Detail: {Detail}",
            invariantCode,
            problemDetails.Status,
            traceId,
            problemDetails.Detail);

        switch (invariantCode)
        {
            case InvariantCode.ContextInitialized:
                // Missing tenant context - may need to re-authenticate or retry
                return Result<TEntity>.Failure("Authentication required. Please sign in again.");

            case InvariantCode.TenantAttributionUnambiguous:
                // Ambiguous attribution - fix request headers/parameters
                var sources = problemDetails.Extensions?[ConflictingSources];
                return Result<TEntity>.Failure(
                    $"Request has conflicting tenant information: {sources}. Please provide only one tenant identifier.");

            case InvariantCode.TenantScopeRequired:
                // Operation requires tenant scope but context doesn't have it
                return Result<TEntity>.Failure("This operation requires a tenant context.");

            case "InternalServerError":
                // Unhandled server error - provide trace ID for support
                return Result<TEntity>.Failure(
                    $"An unexpected error occurred. Please contact support with trace ID: {traceId}");

            default:
                // Unknown error - return generic message with trace ID
                return Result<TEntity>.Failure(
                    $"{problemDetails.Title}. Trace ID: {traceId}");
        }
    }
}
```

#### JavaScript/TypeScript Client Example

```typescript
interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string | null;
  invariant_code?: string;
  trace_id?: string;
  request_id?: string;
  guidance_link?: string;
  conflicting_sources?: string[];
}

class TenantSaasClient {
  constructor(private baseUrl: string, private logger: Logger) {}

  async getEntity(entityId: string): Promise<Result<Entity>> {
    try {
      const response = await fetch(`${this.baseUrl}/api/entities/${entityId}`, {
        headers: {
          'Accept': 'application/json',
          'X-Tenant-Id': this.getTenantId()
        }
      });

      if (response.ok) {
        const entity = await response.json() as Entity;
        return { success: true, data: entity };
      }

      // Parse Problem Details
      const problemDetails = await response.json() as ProblemDetails;
      return this.handleProblemDetails(problemDetails);
    } catch (error) {
      this.logger.error('Unexpected error calling TenantSaas API', error);
      return { success: false, error: 'Unexpected error occurred' };
    }
  }

  private handleProblemDetails(problemDetails: ProblemDetails): Result<Entity> {
    // Log for debugging and support
    this.logger.warn('API error', {
      invariantCode: problemDetails.invariant_code,
      status: problemDetails.status,
      traceId: problemDetails.trace_id,
      detail: problemDetails.detail
    });

    switch (problemDetails.invariant_code) {
      case 'ContextInitialized':
        return {
          success: false,
          error: 'Authentication required. Please sign in again.',
          requiresAuth: true
        };

      case 'TenantAttributionUnambiguous':
        return {
          success: false,
          error: `Request has conflicting tenant information: ${problemDetails.conflicting_sources?.join(', ')}. Please provide only one tenant identifier.`
        };

      case 'TenantScopeRequired':
        return {
          success: false,
          error: 'This operation requires a tenant context.'
        };

      case 'InternalServerError':
        return {
          success: false,
          error: `An unexpected error occurred. Please contact support with trace ID: ${problemDetails.trace_id}`
        };

      default:
        return {
          success: false,
          error: `${problemDetails.title}. Trace ID: ${problemDetails.trace_id}`
        };
    }
  }

  private getTenantId(): string {
    // Implement tenant ID retrieval from session, token, or context
    return localStorage.getItem('tenantId') || '';
  }
}
```

---

## Error Handling Best Practices

### 1. Always Capture trace_id

The `trace_id` is your lifeline for debugging. Always log it and include it in support requests.

```csharp
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

_logger.LogError(
    "Operation failed. TraceId: {TraceId}, InvariantCode: {InvariantCode}",
    problemDetails.Extensions[TraceId],
    problemDetails.Extensions[InvariantCodeKey]);
```

### 2. Use invariant_code for Specific Handling

Don't rely on HTTP status codes alone - use `invariant_code` for precise error handling:

```csharp
// ❌ BAD - Multiple errors can have same status
if (response.StatusCode == 403)
{
    // Could be TenantScopeRequired OR BreakGlassRequired
}

// ✅ GOOD - Precise error identification
if (invariantCode == "TenantScopeRequired")
{
    // Specific handling for missing tenant scope
}
```

### 3. Display User-Friendly Messages

Problem Details `detail` field is human-readable but technical. Translate to user-friendly language:

```csharp
using TenantSaas.Abstractions.Invariants;

var userMessage = invariantCode switch
{
    InvariantCode.ContextInitialized => "Please sign in to continue.",
    InvariantCode.TenantAttributionUnambiguous => "We couldn't identify your organization. Please check your request.",
    InvariantCode.TenantScopeRequired => "This operation requires organization context.",
    _ => "An error occurred. Please try again or contact support."
};
```

### 4. Provide Support Context

When showing errors to users, always provide a way to report issues with trace_id:

```html
<div class="error-message">
  <p>An error occurred. Please try again.</p>
  <details>
    <summary>Technical Details</summary>
    <p>Error ID: <code>{{traceId}}</code></p>
    <p>Time: {{timestamp}}</p>
    <button onclick="copyToClipboard('{{traceId}}')">Copy Error ID</button>
  </details>
</div>
```

### 5. Log Structured Data

Use structured logging to make errors searchable and correlatable:

```csharp
_logger.LogWarning(
    "API request failed. Url: {Url}, InvariantCode: {InvariantCode}, TraceId: {TraceId}, RequestId: {RequestId}",
    request.RequestUri,
    invariantCode,
    traceId,
    requestId);
```

---

## Logging Error Responses

### Server-Side Logging

When logging Problem Details responses, include all correlation fields:

```csharp
_logger.LogWarning(
    "Returning Problem Details response. Path: {Path}, InvariantCode: {InvariantCode}, Status: {Status}, TraceId: {TraceId}, RequestId: {RequestId}",
    httpContext.Request.Path,
    invariantCode,
    problemDetails.Status,
    traceId,
    requestId);
```

### Client-Side Logging

Log errors with enough context for debugging but without sensitive data:

```csharp
// ✅ GOOD - Structured logging without sensitive data
_logger.LogError(
    "API call failed. Endpoint: {Endpoint}, InvariantCode: {InvariantCode}, TraceId: {TraceId}",
    "/api/entities",
    invariantCode,
    traceId);

// ❌ BAD - Logging sensitive information
_logger.LogError(
    "Failed for tenant {TenantId}",  // DON'T log tenant IDs
    tenantId);
```

---

## Extracting invariant_code and trace_id for Support Tickets

When users report issues, guide them to provide error correlation IDs:

### Support Ticket Template

```
Subject: Error in TenantSaas Application

Error ID (trace_id): [Extracted from error message]
Request ID (request_id): [Extracted if available]
Error Code (invariant_code): [Extracted from error]
Timestamp: [When error occurred]
Description: [What the user was trying to do]
```

### Extraction Example (C#)

```csharp
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

public class ErrorReport
{
    public string TraceId { get; set; }
    public string? RequestId { get; set; }
    public string InvariantCode { get; set; }
    public string Timestamp { get; set; }
    public string UserDescription { get; set; }

    public static ErrorReport FromProblemDetails(ProblemDetails pd)
    {
        return new ErrorReport
        {
            TraceId = pd.Extensions[ProblemDetailsExtensions.TraceId]?.ToString() ?? "unknown",
            RequestId = pd.Extensions.ContainsKey(RequestId)
                ? pd.Extensions[RequestId]?.ToString()
                : null,
            InvariantCode = pd.Extensions[InvariantCodeKey]?.ToString() ?? "unknown",
            Timestamp = DateTimeOffset.UtcNow.ToString("O"),
            UserDescription = "" // Filled by user
        };
    }

    public string ToSupportTicket()
    {
        var ticket = $@"
Error ID (trace_id): {TraceId}
Error Code (invariant_code): {InvariantCode}
Timestamp: {Timestamp}";

        if (RequestId != null)
        {
            ticket += $"\nRequest ID (request_id): {RequestId}";
        }

        return ticket;
    }
}
```

---

## Break-Glass Enforcement

Break-glass is a controlled mechanism for privileged operations that bypasses normal tenant isolation rules. TenantSaas enforces explicit, auditable break-glass declarations.

### When to Use Break-Glass

Use break-glass for:
- Production incident response (on-call debugging cross-tenant issues)
- Support operations (authorized staff helping customers)
- Administrative tasks (data migrations, compliance audits)

**Never use break-glass for:**
- Normal application features
- Background jobs with proper tenant context
- Operations that can be scoped to a single tenant

### Enforcing Break-Glass

```csharp
using TenantSaas.Abstractions.BreakGlass;
using TenantSaas.Core.Enforcement;

app.MapPost("/admin/debug/customer-data", async (
    [FromHeader(Name = "X-BreakGlass-Actor")] string actorId,
    [FromHeader(Name = "X-BreakGlass-Reason")] string reason,
    [FromHeader(Name = "X-BreakGlass-Scope")] string declaredScope,
    [FromHeader(Name = "X-BreakGlass-Target-Tenant")] string? targetTenantRef,
    [FromServices] IBoundaryGuard boundaryGuard,
    [FromServices] ITenantContextAccessor accessor) =>
{
    // Create break-glass declaration from headers
    var declaration = new BreakGlassDeclaration(
        actorId: actorId,
        reason: reason,
        declaredScope: declaredScope,
        targetTenantRef: targetTenantRef,
        timestamp: DateTimeOffset.UtcNow);

    // Enforce break-glass before proceeding
    var enforcementResult = await boundaryGuard.RequireBreakGlassAsync(
        declaration,
        accessor.Current?.TraceId ?? Guid.NewGuid().ToString());

    if (!enforcementResult.IsSuccess)
    {
        // Refusal: missing or invalid declaration
        return Results.Problem(
            Core.Errors.ProblemDetailsFactory.ForBreakGlassRequired(
                enforcementResult.TraceId!,
                accessor.Current?.RequestId,
                enforcementResult.Detail));
    }

    // Break-glass approved - proceed with privileged operation
    // This will be logged as EventId 1007 (BreakGlassInvoked)
    var customerData = await GetCustomerDataCrossTenant(targetTenantRef);
    return Results.Ok(customerData);
});
```

### Break-Glass Declaration Requirements

All fields are **required**. Missing or empty fields result in HTTP 403 refusal.

| Field | Description | Example |
|-------|-------------|---------|
| `actorId` | Identity of person invoking break-glass (email, employee ID) | `"alice@example.com"` |
| `reason` | Business justification (incident number, ticket ID) | `"Production incident #12345"` |
| `declaredScope` | What operation is being performed | `"Debug customer_id=cust-999 checkout failure"` |
| `targetTenantRef` | Target tenant (or `null` for cross-tenant) | `"tenant-alpha"` or `null` |
| `timestamp` | When declaration was created | `DateTimeOffset.UtcNow` |

### Break-Glass Client Example

```csharp
public class AdminClient
{
    private readonly HttpClient _httpClient;

    public async Task<Result<CustomerData>> DebugCustomerCheckout(
        string actorEmail,
        string incidentNumber,
        string customerId,
        string? tenantId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/admin/debug/customer-data");
        
        // Add break-glass headers
        request.Headers.Add("X-BreakGlass-Actor", actorEmail);
        request.Headers.Add("X-BreakGlass-Reason", $"Production incident {incidentNumber}");
        request.Headers.Add("X-BreakGlass-Scope", $"Debug customer_id={customerId} checkout failure");
        
        if (tenantId != null)
        {
            request.Headers.Add("X-BreakGlass-Target-Tenant", tenantId);
        }

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            
            if (problemDetails?.Extensions.ContainsKey("invariant_code") == true &&
                problemDetails.Extensions["invariant_code"]?.ToString() == "BreakGlassExplicitAndAudited")
            {
                return Result.Failure<CustomerData>(
                    "Break-glass declaration rejected: " + problemDetails.Detail);
            }

            return Result.Failure<CustomerData>("Unexpected error: " + problemDetails?.Detail);
        }

        var data = await response.Content.ReadFromJsonAsync<CustomerData>();
        return Result.Success(data!);
    }
}
```

### Break-Glass Audit Events

All break-glass attempts (successful and failed) are logged with structured audit events:

**Successful Break-Glass** (EventId 1007):
```
LogLevel.Warning: "Break-glass invoked: actor={Actor}, reason={Reason}, scope={Scope}, tenant_ref={TenantRef}, trace_id={TraceId}, audit_code=BreakGlassInvoked"
```

**Denied Break-Glass** (EventId 1010):
```
LogLevel.Error: "Break-glass attempt denied: trace_id={TraceId}, request_id={RequestId}, invariant_code=BreakGlassExplicitAndAudited, reason={Reason}"
```

### Integrating with Audit Sinks (Optional)

For compliance requirements, implement `IBreakGlassAuditSink` to emit break-glass events to external systems:

```csharp
using TenantSaas.Abstractions.BreakGlass;

public class ComplianceAuditSink : IBreakGlassAuditSink
{
    private readonly ILogger<ComplianceAuditSink> _logger;
    private readonly HttpClient _auditSystemClient;

    public ComplianceAuditSink(
        ILogger<ComplianceAuditSink> logger,
        HttpClient auditSystemClient)
    {
        _logger = logger;
        _auditSystemClient = auditSystemClient;
    }

    public async Task EmitAsync(BreakGlassAuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _auditSystemClient.PostAsJsonAsync(
                "/api/audit-events",
                new
                {
                    event_type = "break_glass_invoked",
                    actor = auditEvent.Actor,
                    reason = auditEvent.Reason,
                    scope = auditEvent.Scope,
                    tenant_ref = auditEvent.TenantRef,
                    trace_id = auditEvent.TraceId,
                    invariant_code = auditEvent.InvariantCode,
                    operation = auditEvent.OperationName,
                    audit_code = auditEvent.AuditCode,
                    timestamp = auditEvent.Timestamp
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Break-glass audit event sent to compliance system: trace_id={TraceId}, audit_code={AuditCode}",
                auditEvent.TraceId,
                auditEvent.AuditCode);
        }
        catch (Exception ex)
        {
            // Audit sink failures do not block the operation
            _logger.LogError(ex,
                "Failed to send break-glass audit event to compliance system: trace_id={TraceId}",
                auditEvent.TraceId);
        }
    }
}

// Register in DI
builder.Services.AddHttpClient<IBreakGlassAuditSink, ComplianceAuditSink>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuditSystemUrl"]!);
});
```

**Important:** Audit sink failures are logged but **do not block** the break-glass operation. Enforcement happens before audit emission.

---

## Testing Integration

This section demonstrates how to test your TenantSaas integration with proper error handling validation.

### Required Test Dependencies

Add these NuGet packages to your test project:

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

### Test Project Setup

Configure your test project to use `WebApplicationFactory`:

```csharp
// In your test project, reference the main project
// and ensure Program class is accessible
public partial class Program { } // Add to Program.cs if not present
```

### Unit Testing Error Handling

Test that your client properly handles Problem Details responses:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TenantSaas.Abstractions.Invariants;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

[Fact]
public async Task Client_ReceivesContextNotInitialized_ReturnsAuthenticationRequired()
{
    // Arrange - Create mock HTTP response with Problem Details
    var problemDetails = new ProblemDetails
    {
        Type = "urn:tenantsaas:error:context-initialized",
        Title = "Tenant context not initialized",
        Status = 401,
        Detail = "Tenant context must be initialized before operations can proceed.",
    };
    problemDetails.Extensions[InvariantCodeKey] = InvariantCode.ContextInitialized;
    problemDetails.Extensions[TraceId] = "test-trace-123";
    problemDetails.Extensions[RequestId] = "test-request-456";

    var handler = new TestHttpMessageHandler(
        HttpStatusCode.Unauthorized,
        JsonContent.Create(problemDetails));

    var client = new TenantSaasClient(
        new HttpClient(handler) { BaseAddress = new Uri("http://localhost") },
        Mock.Of<ILogger<TenantSaasClient>>());

    // Act
    var result = await client.GetEntityAsync("entity-1");

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("authentication required", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Test HTTP message handler for unit testing HTTP clients.
/// </summary>
public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly HttpContent _content;

    public TestHttpMessageHandler(HttpStatusCode statusCode, HttpContent content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = _content
        });
    }
}
```

### Integration Testing Middleware

Test that middleware correctly returns Problem Details for invariant violations:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

public class TenantSaasIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TenantSaasIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Request_WithMissingTenantContext_ReturnsProblemDetails()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Request without tenant attribution
        var response = await client.GetAsync("/api/tenants/test-tenant/data");

        // Assert - Should return 422 (ambiguous attribution when no source provided)
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey(InvariantCodeKey);
        problemDetails.Extensions.Should().ContainKey(TraceId);
        problemDetails.Type.Should().StartWith("urn:tenantsaas:error:");
    }

    [Fact]
    public async Task Request_WithValidTenant_Succeeds()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Request with valid tenant in route
        var response = await client.GetAsync("/tenants/valid-tenant/data");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthEndpoint_BypassesTenantRequirement()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Testing Error Extraction

Test that your error extraction logic works correctly:

```csharp
using TenantSaas.Abstractions.Invariants;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

[Fact]
public void ErrorReport_FromProblemDetails_ExtractsAllFields()
{
    // Arrange
    var problemDetails = new ProblemDetails
    {
        Type = "urn:tenantsaas:error:context-initialized",
        Title = "Tenant context not initialized",
        Status = 401,
        Detail = "Test detail"
    };
    problemDetails.Extensions[InvariantCodeKey] = InvariantCode.ContextInitialized;
    problemDetails.Extensions[TraceId] = "abc-123";
    problemDetails.Extensions[RequestId] = "req-456";

    // Act
    var report = ErrorReport.FromProblemDetails(problemDetails);

    // Assert
    report.TraceId.Should().Be("abc-123");
    report.RequestId.Should().Be("req-456");
    report.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
}

---

## References

- [Error Catalog](./error-catalog.md) - Complete list of all error codes and responses
- [Trust Contract Documentation](./trust-contract.md) - Invariant definitions and policies
- [RFC 7807: Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)

---

## Support

For questions or issues:
1. Check the [Error Catalog](./error-catalog.md) for specific error codes
2. Review logs for `trace_id` and `invariant_code`
3. Create support ticket with correlation IDs

---

## Structured Logging and Correlation

TenantSaas emits structured logs for all enforcement decisions, enabling audit trails and incident correlation.

### Required Structured Fields

All enforcement logs include these required fields:

- `tenant_ref`: Disclosure-safe tenant identifier
- `trace_id`: End-to-end correlation ID
- `request_id`: Request-scoped ID (null for non-request execution)
- `invariant_code`: Invariant code for violations/refusals (null for success)
- `event_name`: Event type (ContextInitialized, RefusalEmitted, etc.)
- `severity`: Information, Warning, or Error
- `execution_kind`: request, background, admin, or scripted
- `scope_type`: Tenant, NoTenant, or SharedSystem

### Example Correlation Pattern

**Problem Details Response:**
```json
{
  "type": "urn:tenantsaas:error:tenant-attribution-unambiguous",
  "status": 422,
  "invariant_code": "TenantAttributionUnambiguous",
  "trace_id": "abc123",
  "request_id": "req-456"
}
```

**Corresponding Log:**
```json
{
  "event_name": "RefusalEmitted",
  "trace_id": "abc123",
  "request_id": "req-456",
  "invariant_code": "TenantAttributionUnambiguous",
  "http_status": 422
}
```

Join by: `logs.trace_id = problemDetails.trace_id`
