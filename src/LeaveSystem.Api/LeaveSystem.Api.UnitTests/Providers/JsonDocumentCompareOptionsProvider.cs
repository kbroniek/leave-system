using System.Text.Json;
using FluentAssertions.Equivalency;

namespace LeaveSystem.Api.UnitTests.Providers;

public static class JsonDocumentCompareOptionsProvider
{
    public static Func<EquivalencyAssertionOptions<TEntity>, EquivalencyAssertionOptions<TEntity>> Get<TEntity>(string propertyName) =>
        o => o
            .Using<JsonDocument>(ctx =>
                ctx.Subject.RootElement.GetRawText().Should().BeEquivalentTo(ctx.Expectation.RootElement.GetRawText()))
            .When(info => info.Path.EndsWith(propertyName));
}