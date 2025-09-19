using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace jamster.Domain;

public sealed class KeyFrame(Tick tick, IDictionary<string, object> states) : IReadOnlyDictionary<string, object>
{
    private readonly IDictionary<string, object> _states = CloneStates(states);

    public IEnumerable<string> Keys => _states.Keys;
    public IEnumerable<object> Values => _states.Values;
    public Tick Tick => tick;

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _states.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_states).GetEnumerator();
    public int Count => _states.Count;
    public bool ContainsKey(string key) => _states.ContainsKey(key);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _states.TryGetValue(key, out value);
    public object this[string key] => _states[key];

    private static IDictionary<string, object> CloneStates(IDictionary<string, object> states) =>
        states.Select(x => x).ToDictionary(k => k.Key, v => v.Value);
}