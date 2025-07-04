using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace amethyst.Domain;

public sealed class KeyFrame(Tick tick, IDictionary<string, object> states) : IReadOnlyDictionary<string, object>
{
    public IEnumerable<string> Keys => states.Keys;
    public IEnumerable<object> Values => states.Values;
    public Tick Tick => tick;

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => states.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)states).GetEnumerator();
    public int Count => states.Count;
    public bool ContainsKey(string key) => states.ContainsKey(key);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => states.TryGetValue(key, out value);
    public object this[string key] => states[key];

    private static IDictionary<string, object> CloneStates(IDictionary<string, object> states) =>
        states.Select(x => x).ToDictionary(k => k.Key, v => v.Value);
}