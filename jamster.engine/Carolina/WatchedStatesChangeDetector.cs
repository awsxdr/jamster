using System.Text.RegularExpressions;

namespace jamster.engine.Carolina;

public class WatchedStatesChangeDetector
{
    private readonly Func<Dictionary<string, object>, Task> _changeHandler;
    private readonly Regex[] _keyDetectors;

    public WatchedStatesChangeDetector(string[] watchedStates, Func<Dictionary<string, object>, Task> changeHandler)
    {
        _changeHandler = changeHandler;

        _keyDetectors = watchedStates.Select(k => new Regex(k
                    .Replace(".", "\\.")
                    .Replace("(", "\\(")
                    .Replace(")", "\\)")
                    .Replace("*", "[\\w\\-]+")
                    .Map(s => s.EndsWith("[\\w\\-]+") ? s[..^3] + ".*" : s),
                RegexOptions.Compiled))
            .ToArray();
    }

    public async Task ProcessChange(IDictionary<string, object> states, string[] changedKeys)
    {
        var relevantKeys = changedKeys.Where(k => _keyDetectors.Any(d => d.IsMatch(k))).ToArray();

        if (relevantKeys.Length == 0)
            return;

        var relevantStates = new Dictionary<string, object>();

        foreach (var key in relevantKeys)
        {
            relevantStates[key] = states[key];
        }

        await _changeHandler(relevantStates);
    }
}