using System.Collections.Concurrent;
using jamster.Domain;
using DotNext.Collections.Generic;

namespace jamster.Services;

public interface IKeyFrameService
{
    public delegate IKeyFrameService Factory(IGameStateStore stateStore);

    void CaptureKeyFrame();
    void CaptureKeyFrameAtTick(Tick tick);
    Option<KeyFrame> GetKeyFrameBefore(Tick tick);
    void ClearFramesAfter(Tick tick);
}

public class KeyFrameService(IGameStateStore stateStore, ISystemTime systemTime, ILogger<KeyFrameService> logger) : IKeyFrameService
{
    private readonly ConcurrentDictionary<long, KeyFrame> _frames = new();

    public void CaptureKeyFrame()
    {
        Tick tick = systemTime.GetTick();
        CaptureKeyFrameAtTick(tick);
    }

    public void CaptureKeyFrameAtTick(Tick tick)
    {
        if (_frames.ContainsKey(tick))
            return;

        var frame = new KeyFrame(tick, stateStore.GetAllStates());

        logger.LogDebug("Capturing key frame at {tick}", tick);

        _frames[tick] = frame;
    }

    public Option<KeyFrame> GetKeyFrameBefore(Tick tick)
    {
        var key = _frames.Keys.Where(k => k < tick).Order().LastOrNone();

        if (!key.HasValue)
            return Option.None<KeyFrame>();

        return Option.Some(_frames[key.Value]);
    }

    public void ClearFramesAfter(Tick tick)
    {
        var keysToRemove = _frames.Keys.Where(k => k > tick).ToArray();

        foreach (var key in keysToRemove)
            _frames.Remove(key, out _);
    }
}