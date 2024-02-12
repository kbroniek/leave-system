using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using NSubstitute;

namespace LeaveSystem.Shared.Extensions;

public static class ArgExtensions
{
    // TODO Move to unit tests.
    public static T IsEquivalentTo<T>(object target) where T : class =>
        Arg.Is<T>(x => x.IsEquivalentTo(target));

    private static bool IsEquivalentTo<T>(this T source, object target) where T : class
    {
        using var scope = new AssertionScope();
        source.Should().BeEquivalentTo(target);
        return !scope.HasFailures();
    }

    public static TSource IsEquivalentTo<TSource, TTarget>(TTarget target, Func<EquivalencyAssertionOptions<TTarget>, EquivalencyAssertionOptions<TTarget>> config) where TSource : class where TTarget : class =>
        Arg.Is<TSource>(x => x.IsEquivalentTo(target, config));

    private static bool IsEquivalentTo<TSource, TTarget>(this TSource source, TTarget target, Func<EquivalencyAssertionOptions<TTarget>, EquivalencyAssertionOptions<TTarget>> config) where TSource : class where TTarget : class
    {
        using var scope = new AssertionScope();
        source.Should().BeEquivalentTo(target, config);
        return scope.Discard().Length != 0;
    }

    public static IEnumerable<T> IsCollectionEquivalentTo<T>(IEnumerable<T> source) =>
        Arg.Is<IEnumerable<T>>(x => x.SequenceEqual(source));
}
