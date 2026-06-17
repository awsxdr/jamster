namespace jamster.engine.Extensions;

public static class ResultExtensionMethods
{
    public static Result<TResult> Or<TResult>(this Result<TResult> @this, Func<Result<TResult>> or) =>
        @this is Success ? @this : or();

    public static Task<Result<TResult>> Or<TResult>(this Result<TResult> @this, Func<Task<Result<TResult>>> or) =>
        @this is Success ? @this.ToTask() : or();

    public static async Task<Result<TResult>> Or<TResult>(this Task<Result<TResult>> @this, Func<Result<TResult>> or) =>
        await @this as Success<TResult> ?? or();

    public static async Task<Result<TResult>> Or<TResult>(this Task<Result<TResult>> @this, Func<Task<Result<TResult>>> or) =>
        await @this as Success<TResult> ?? await or();
}