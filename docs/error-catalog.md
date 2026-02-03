# Error Catalog

This document provides a comprehensive catalog of all invariant violations and their corresponding Problem Details error responses in TenantSaas.

## Overview

All errors in TenantSaas follow [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) format with stable, machine-readable identifiers.

### Stability Guarantee

Within a major version:
- **invariant_code** values are stable
- **type** URIs are stable
- **HTTP status codes** are stable
- Extension field names are stable

Breaking changes to these values require a major version bump and migration guide.

## Standard Problem Details Structure

All Problem Details responses follow this structure. The `type` field uses a URN with kebab-case naming derived from the invariant (e.g., `ContextInitialized` becomes `context-initialized`).

```json
{
  "type": "urn:tenantsaas:error:context-initialized",
  "title": "Human-readable error title",
  "status": 4xx or 5xx,
  "detail": "Human-readable explanation",
  "instance": null,
  "invariant_code": "ContextInitialized",
  "trace_id": "end-to-end-correlation-id",
  "request_id": "request-specific-id",
  "guidance_link": "https://docs.tenantsaas.dev/errors/context-not-initialized"
}
```

**Type URI Pattern:** `urn:tenantsaas:error:{kebab-case-invariant-name}`

| Invariant Code | Type URI |
|----------------|----------|
| `ContextInitialized` | `urn:tenantsaas:error:context-initialized` |
| `TenantAttributionUnambiguous` | `urn:tenantsaas:error:tenant-attribution-unambiguous` |
| `TenantScopeRequired` | `urn:tenantsaas:error:tenant-scope-required` |
| `BreakGlassExplicitAndAudited` | `urn:tenantsaas:error:break-glass-explicit-and-audited` |
| `DisclosureSafe` | `urn:tenantsaas:error:disclosure-safe` |

### Required Extension Fields

| Field | Type | Always Present | Description |
|-------|------|----------------|-------------|
| `invariant_code` | string | ✅ Yes | Stable invariant identifier from `InvariantCode` class |
| `trace_id` | string | ✅ Yes | End-to-end correlation ID spanning distributed systems |
| `request_id` | string | ⚠️ Conditional | Request-specific ID (present for request execution kinds) |
| `guidance_link` | string | ✅ Yes | URL to error documentation and remediation guidance |

### Optional Extension Fields

| Field | Type | When Present | Description |
|-------|------|--------------|-------------|
| `tenant_ref` | string | Conditional | Disclosure-safe tenant reference (only when safe per policy) |
| `conflicting_sources` | array | Conditional | List of conflicting attribution sources (for ambiguous attribution) |

---

## Invariant Violations

### 1. ContextInitialized

**Invariant Code:** `ContextInitialized`

**Description:** Tenant context must be initialized before operations can proceed.

**HTTP Status:** `401 Unauthorized`

**Type URI:** `urn:tenantsaas:error:context-initialized`

**Title:** Tenant context not initialized

**Guidance Link:** https://docs.tenantsaas.dev/errors/context-not-initialized

**When This Occurs:**
- Middleware or application code attempts to access tenant context before it has been initialized
- Request processing begins without proper tenant context setup

**Example Response:**

```json
{
  "type": "urn:tenantsaas:error:context-initialized",
  "title": "Tenant context not initialized",
  "status": 401,
  "detail": "Tenant context must be initialized before operations can proceed.",
  "instance": null,
  "invariant_code": "ContextInitialized",
  "trace_id": "a1b2c3d4e5f6",
  "request_id": "req-12345",
  "guidance_link": "https://docs.tenantsaas.dev/errors/context-not-initialized"
}
```

**Remediation:**
- Ensure `TenantContextMiddleware` is properly configured in the request pipeline
- Verify tenant context is initialized before calling operations that require it
- Check that context initialization isn't being skipped for this request path

---

### 2. TenantAttributionUnambiguous

**Invariant Code:** `TenantAttributionUnambiguous`

**Description:** Tenant attribution from available sources must be unambiguous.

**HTTP Status:** `422 Unprocessable Entity`

**Type URI:** `urn:tenantsaas:error:tenant-attribution-unambiguous`

**Title:** Tenant attribution is ambiguous

**Guidance Link:** https://docs.tenantsaas.dev/errors/attribution-ambiguous

**When This Occurs:**
- Multiple attribution sources (route parameter, header, token claim, etc.) provide conflicting tenant IDs
- Attribution precedence rules don't resolve the conflict

**Example Response:**

```json
{
  "type": "urn:tenantsaas:error:tenant-attribution-unambiguous",
  "title": "Tenant attribution is ambiguous",
  "status": 422,
  "detail": "Tenant attribution from available sources is ambiguous.",
  "instance": null,
  "invariant_code": "TenantAttributionUnambiguous",
  "trace_id": "a1b2c3d4e5f6",
  "request_id": "req-12345",
  "guidance_link": "https://docs.tenantsaas.dev/errors/attribution-ambiguous",
  "conflicting_sources": ["route-parameter", "header-value"]
}
```

**Remediation:**
- Review attribution sources in the request (route params, headers, tokens)
- Ensure only one attribution source provides a tenant ID, or configure precedence rules
- Check attribution resolver configuration for proper precedence handling

---

### 3. TenantScopeRequired

**Invariant Code:** `TenantScopeRequired`

**Description:** Operation requires an explicit tenant scope.

**HTTP Status:** `403 Forbidden`

**Type URI:** `urn:tenantsaas:error:tenant-scope-required`

**Title:** Tenant scope required

**Guidance Link:** https://docs.tenantsaas.dev/errors/tenant-scope-required

**When This Occurs:**
- Operation requires tenant-scoped context but context has no-tenant or shared-system scope
- Attempt to perform tenant-specific operation outside tenant scope

**Example Response:**

```json
{
  "type": "urn:tenantsaas:error:tenant-scope-required",
  "title": "Tenant scope required",
  "status": 403,
  "detail": "Operation requires an explicit tenant scope.",
  "instance": null,
  "invariant_code": "TenantScopeRequired",
  "trace_id": "a1b2c3d4e5f6",
  "request_id": "req-12345",
  "guidance_link": "https://docs.tenantsaas.dev/errors/tenant-scope-required"
}
```

**Remediation:**
- Ensure request includes proper tenant attribution
- Verify operation is called with tenant-scoped context
- Check that no-tenant or shared-system scopes aren't being used incorrectly

---

### 4. BreakGlassExplicitAndAudited

**Invariant Code:** `BreakGlassExplicitAndAudited`

**Description:** Break-glass must be explicitly declared with actor identity and reason.

**HTTP Status:** `403 Forbidden`

**Type URI:** `urn:tenantsaas:error:break-glass-explicit-and-audited`

**Title:** Break-glass must be explicit

**Guidance Link:** https://docs.tenantsaas.dev/errors/break-glass-required

**When This Occurs:**
- Cross-tenant or privileged operation attempted without break-glass declaration
- Break-glass declaration is missing required fields (actor, reason)

**Example Response:**

```json
{
  "type": "urn:tenantsaas:error:break-glass-explicit-and-audited",
  "title": "Break-glass must be explicit",
  "status": 403,
  "detail": "Break-glass must be explicitly declared with actor identity and reason.",
  "instance": null,
  "invariant_code": "BreakGlassExplicitAndAudited",
  "trace_id": "a1b2c3d4e5f6",
  "request_id": "req-12345",
  "guidance_link": "https://docs.tenantsaas.dev/errors/break-glass-required"
}
```

**Remediation:**
- Provide explicit break-glass declaration with actor identity and reason
- Ensure audit event emission is configured for break-glass operations
- Review cross-tenant operation requirements and authorization

---

### 5. DisclosureSafe

**Invariant Code:** `DisclosureSafe`

**Description:** Tenant information disclosure must follow safe disclosure policy.

**HTTP Status:** `500 Internal Server Error`

**Type URI:** `urn:tenantsaas:error:disclosure-safe`

**Title:** Tenant disclosure policy violation

**Guidance Link:** https://docs.tenantsaas.dev/errors/disclosure-unsafe

**When This Occurs:**
- Attempt to include tenant ID in error response when disclosure is unsafe
- Violation of disclosure policy (logging, error messages, audit events)

**Example Response:**

```json
{
  "type": "urn:tenantsaas:error:disclosure-safe",
  "title": "Tenant disclosure policy violation",
  "status": 500,
  "detail": "Tenant information disclosure must follow safe disclosure policy.",
  "instance": null,
  "invariant_code": "DisclosureSafe",
  "trace_id": "a1b2c3d4e5f6",
  "request_id": "req-12345",
  "guidance_link": "https://docs.tenantsaas.dev/errors/disclosure-unsafe"
}
```

**Remediation:**
- Use safe-state values (`unknown`, `sensitive`, `cross_tenant`, `opaque`) instead of actual tenant IDs
- Review disclosure policy rules before including tenant references
- Ensure error messages and logs don't leak sensitive tenant information

---

## Using the Error Catalog

### Client-Side Error Handling

```csharp
// Parse Problem Details from response
var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

// Check invariant code for specific handling
var invariantCode = problemDetails?.Extensions?["invariant_code"]?.ToString();

switch (invariantCode)
{
    case "ContextInitialized":
        // Handle missing context - may need to re-authenticate
        break;
    
    case "TenantAttributionUnambiguous":
        // Handle ambiguous attribution - fix request headers/params
        var sources = problemDetails.Extensions["conflicting_sources"];
        break;
    
    case "InternalServerError":
        // Log trace_id for support
        var traceId = problemDetails.Extensions["trace_id"];
        Logger.LogError("Internal error occurred. Trace ID: {TraceId}", traceId);
        break;
}
```

### Logging Error Responses

```csharp
// Always log trace_id and invariant_code for support correlation
Logger.LogWarning(
    "API error: {InvariantCode}, Status: {Status}, TraceId: {TraceId}",
    problemDetails.Extensions["invariant_code"],
    problemDetails.Status,
    problemDetails.Extensions["trace_id"]);
```

### Support Ticket Creation

When creating support tickets, always include:
1. **trace_id** - For end-to-end correlation across distributed systems
2. **request_id** - For request-specific correlation (if present)
3. **invariant_code** - For quick error classification
4. **timestamp** - When the error occurred

---

## Synthetic Error Codes

### InternalServerError

**Note:** This is a *synthetic* invariant code used exclusively for unhandled exceptions. It is **not** part of the `InvariantCode` registry as it represents unexpected failures rather than invariant violations.

**HTTP Status:** `500 Internal Server Error`

**Type URI:** `urn:tenantsaas:error:internal-server-error`

**Title:** Internal server error

**When This Occurs:**
- Unhandled exception in request pipeline
- Unexpected system failure

**Example Response:**

```json
{
  "type": "urn:tenantsaas:error:internal-server-error",
  "title": "Internal server error",
  "status": 500,
  "detail": "An unexpected error occurred. Please contact support with the trace ID.",
  "instance": null,
  "invariant_code": "InternalServerError",
  "trace_id": "a1b2c3d4e5f6",
  "request_id": "req-12345"
}
```

**Important:** Exception details are **never** included in the response. Use `trace_id` to correlate with server logs for debugging.

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| v1 | 2026-02-01 | Initial catalog with 5 invariants |

---

## References

- [RFC 7807: Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)
- [Trust Contract Documentation](./trust-contract.md)
- [Integration Guide](./integration-guide.md)
