﻿using Ardalis.GuardClauses;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LeaveSystem.Shared;

public static class GuardClauseExtensions
{
    [return: NotNull]
    public static T Nill<T>(this IGuardClause _, [NotNull][ValidatedNotNull] T? input, [CallerArgumentExpression("input")] string? parameterName = null, string? message = null)
        where T : class
    {
        return AgainstNill(input, parameterName, message);
    }

    [return: NotNull]
    public static T Nill<T>(this IGuardClause _, [NotNull][ValidatedNotNull] T? input, [CallerArgumentExpression("input")] string? parameterName = null, string? message = null)
        where T : struct
    {
        return AgainstNill(input, parameterName, message).Value;
    }

    [return: NotNull]
    public static T NillAndDefault<T>(this IGuardClause _, [NotNull][ValidatedNotNull] T? input, [CallerArgumentExpression("input")] string? parameterName = null, string? message = null) where T : struct
    {
        var notNull = AgainstNill(input, parameterName, message).Value;
        return Guard.Against.Default(notNull, parameterName, message);
    }

    public static string InvalidEmail(this IGuardClause _, [NotNull][ValidatedNotNull] string? input, [CallerArgumentExpression("input")] string? parameterName = null, string? message = null)
    {
        AgainstNill(input, parameterName, message);
        return Guard.Against.InvalidFormat(input, parameterName!, @"^[-\w\.+]+@([-\w]+\.)+[\w-]{2,4}$", message);
    }

    [return: NotNull]
    private static T AgainstNill<T>([NotNull][ValidatedNotNull] T? input, string? parameterName, string? message)
    {
        if (input is null)
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
