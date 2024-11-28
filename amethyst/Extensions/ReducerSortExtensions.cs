using amethyst.Reducers;

namespace amethyst.Extensions;

public static class ReducerSortExtensions
{
    public static IEnumerable<IReducer> SortReducers(this IEnumerable<IReducer> reducers)
    {
        reducers = reducers.ToArray();

        var reducersDetails =
            reducers
                .Where(r => r is IDependsOnState)
                .Select(r =>
                {
                    var stateType = r.GetDefaultState().GetType();

                    return new ReducerDetails(
                        Reducer: r,
                        Dependencies:
                        r.GetType()
                            .GetInterfaces()
                            .Where(i => i.IsGenericType)
                            .Where(i => i.GetGenericTypeDefinition() == typeof(IDependsOnState<>))
                            .Select(i => i.GetGenericArguments().Single())
                            .Where(t => t != stateType)
                            .ToArray(),
                        State: stateType);
                })
                .ToArray();

        var result = new List<ReducerDetails>();
        var unsortedReducers = new HashSet<ReducerDetails>(reducersDetails);

        while (unsortedReducers.Any())
        {
            var unsortedReducersOnlyDependentOnSortedReducers =
                unsortedReducers
                    .Where(r => r.Dependencies.All(d => result.Any(x => x.State == d)))
                    .ToArray();

            if (!unsortedReducersOnlyDependentOnSortedReducers.Any())
                throw new CyclicalReducerDependenciesException();

            result.AddRange(unsortedReducersOnlyDependentOnSortedReducers);

            foreach(var selectedReducer in unsortedReducersOnlyDependentOnSortedReducers)
                unsortedReducers.Remove(selectedReducer);
        }

        return result.Select(r => r.Reducer);
    }

    private record ReducerDetails(IReducer Reducer, Type[] Dependencies, Type State);

    public sealed class NonReducerTypeInCollection()
        : Exception("All elements of the enumerable must inherit Reducer<>");
    public sealed class CyclicalReducerDependenciesException : Exception;
}