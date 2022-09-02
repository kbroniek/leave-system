using Ardalis.GuardClauses;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LeaveSystem.Shared;

public static class GuardClauseExtensions
{
    [return: NotNull]
    public static T Nill<T>(this IGuardClause _, [NotNull][ValidatedNotNull] T input, [CallerArgumentExpression("input")] string? parameterName = null, string? message = null)
    {
        if (input == null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(parameterName);
            }

            throw new ArgumentNullException(parameterName, message);
        }

        return input;
    }
}
