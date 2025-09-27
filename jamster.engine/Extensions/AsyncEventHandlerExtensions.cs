using jamster.engine.Services;

namespace jamster.engine.Extensions;

public static class AsyncEventHandlerExtensions
{
    public static Task InvokeHandlersAsync<TEventArgs>(this AsyncEventHandler<TEventArgs>? @this, object sender, TEventArgs e) =>
        Task.WhenAll(
            @this?.GetInvocationList()
                .Select(i => (Task)i.DynamicInvoke(sender, e)!)
            ?? [Task.CompletedTask]);
}
