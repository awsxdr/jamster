using FluentAssertions;
using FluentAssertions.Primitives;
using Func;

namespace amethyst.tests;

public static class AssertionsExtensions
{
    public static AndWhichConstraint<ObjectAssertions, Success> BeSuccess(this ObjectAssertions @this) =>
        @this.BeAssignableTo<Success>();

    public static AndWhichConstraint<ObjectAssertions, Success<TValue>> BeSuccess<TValue>(this ObjectAssertions @this) =>
        @this.BeAssignableTo<Success<TValue>>();

    public static AndWhichConstraint<ObjectAssertions, Success<TValue>> BeSuccess<TValue>(this ObjectAssertions @this, out TValue value)
    {
        var result = @this.BeAssignableTo<Success<TValue>>();

        value = result.Subject.Value;

        return result;
    }

    public static AndWhichConstraint<ObjectAssertions, Failure> BeFailure(this ObjectAssertions @this) =>
        @this.BeAssignableTo<Failure>();

    public static AndWhichConstraint<ObjectAssertions, Failure<TError>> BeFailure<TError>(this ObjectAssertions @this)
        where TError : ResultError
        =>
            @this.BeAssignableTo<Failure<TError>>();
}