using FluentAssertions;
using FluentAssertions.Primitives;
using Func;

namespace amethyst.tests;

public static class AssertionsExtensions
{
    public static AndWhichConstraint<ObjectAssertions, Success<TValue>> BeSuccess<TValue>(this ObjectAssertions @this) =>
        @this.BeAssignableTo<Success<TValue>>();

    public static AndWhichConstraint<ObjectAssertions, Failure<TError>> BeFailure<TError>(this ObjectAssertions @this)
        where TError : ResultError
        =>
            @this.BeAssignableTo<Failure<TError>>();
}