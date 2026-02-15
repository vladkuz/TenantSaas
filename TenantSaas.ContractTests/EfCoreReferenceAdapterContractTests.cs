using System.Xml.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;
using TenantSaas.EfCore;

namespace TenantSaas.ContractTests;

public sealed class EfCoreReferenceAdapterContractTests
{
    [Fact]
    public void CoreAndAbstractions_DoNotReferenceEfCorePackages()
    {
        var root = FindRepoRoot();
        var projectFiles = new[]
        {
            Path.Combine(root.FullName, "TenantSaas.Core", "TenantSaas.Core.csproj"),
            Path.Combine(root.FullName, "TenantSaas.Abstractions", "TenantSaas.Abstractions.csproj")
        };

        foreach (var projectFile in projectFiles)
        {
            var document = XDocument.Load(projectFile);
            var packageReferences = document
                .Descendants("PackageReference")
                .Select(node => (string?)node.Attribute("Include"))
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .ToArray();

            packageReferences.Should().NotContain(
                reference => reference!.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal),
                $"{Path.GetFileName(projectFile)} must remain storage-agnostic");
        }
    }

    [Fact]
    public async Task EfCoreAdapter_RequiresBoundaryGuardContextBeforeOperation()
    {
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());
        var accessor = new UninitializedAccessor();
        var adapter = new TenantBoundaryDbContextExecutor(guard, accessor);

        await using var dbContext = CreateDbContext();
        var operationInvoked = false;

        Func<Task> act = async () => await adapter.ExecuteAsync(
            dbContext,
            async (_, _) =>
            {
                operationInvoked = true;
                await Task.Yield();
                return 1;
            });

        var exception = await act.Should().ThrowAsync<TenantBoundaryViolationException>();
        exception.Which.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        operationInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task EfCoreAdapter_ThrowsArgumentNullException_WhenDbContextIsNull()
    {
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());
        var accessor = new InitializedAccessor();
        var adapter = new TenantBoundaryDbContextExecutor(guard, accessor);

        Func<Task> act = async () => await adapter.ExecuteAsync<ReferenceAdapterDbContext, int>(
            null!,
            async (_, _) =>
            {
                await Task.Yield();
                return 1;
            });

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("dbContext");
    }

    [Fact]
    public async Task EfCoreAdapter_ThrowsArgumentNullException_WhenOperationIsNull()
    {
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());
        var accessor = new InitializedAccessor();
        var adapter = new TenantBoundaryDbContextExecutor(guard, accessor);

        await using var dbContext = CreateDbContext();

        Func<Task> act = async () => await adapter.ExecuteAsync<ReferenceAdapterDbContext, int>(
            dbContext,
            null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("operation");
    }

    [Fact]
    public async Task EfCoreAdapter_ExecutesOperationWhenBoundaryGuardSucceeds()
    {
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());
        var accessor = new InitializedAccessor();
        var adapter = new TenantBoundaryDbContextExecutor(guard, accessor);

        await using var dbContext = CreateDbContext();

        var result = await adapter.ExecuteAsync(
            dbContext,
            async (_, _) =>
            {
                await Task.Yield();
                return "ok";
            });

        result.Should().Be("ok");
    }

    private static ReferenceAdapterDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ReferenceAdapterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new ReferenceAdapterDbContext(options);
    }

    private static DirectoryInfo FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "README.md")))
            {
                return current;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("README.md not found from test execution directory.");
    }

    private sealed class ReferenceAdapterDbContext(DbContextOptions<ReferenceAdapterDbContext> options)
        : DbContext(options);

    private sealed class UninitializedAccessor : ITenantContextAccessor
    {
        public TenantContext? Current => null;

        public bool IsInitialized => false;
    }

    private sealed class InitializedAccessor : ITenantContextAccessor
    {
        private static readonly TenantContext Context = TenantContext.ForBackground(
            TenantScope.ForTenant(new TenantId("tenant-efcore-001")),
            "trace-efcore-001");

        public TenantContext? Current => Context;

        public bool IsInitialized => true;
    }
}
