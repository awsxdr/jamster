using System.Linq.Expressions;
using System.Text.Json;

using jamster.engine.tests.EventHandling;
using Func;

using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Extensions;
using jamster.engine.Reducers;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberHidesStaticFromOuterClass

namespace jamster.engine.tests.GameGeneration;

public class GameSimulator(SimulatorGame game)
{
    private const float TicksPerSecond = 10;
    private const int TickIncrement = (int)(Tick.TicksPerSecond / TicksPerSecond);
    private const float ValidationIntervalInSeconds = 1.333f; // Interval not aligned to seconds to avoid missing alignment issues

    private GameState _gameState = new();
    private List<Event> _events = new();
    private Tick _tick = 0;

    public Event[] SimulateGame()
    {
        _gameState = new();
        _events = new();
        _tick = 0;

        _tick += Random.Shared.Next(Tick.TicksPerSecond * 60);
        _gameState.Teams.HomeTeam = ToGameTeam(game.HomeTeam.DomainTeam);
        _events.Add(new TeamSet(_tick, new(TeamSide.Home, _gameState.Teams.HomeTeam)));

        _tick += Random.Shared.Next(Tick.TicksPerSecond * 60);
        _gameState.Teams.AwayTeam = ToGameTeam(game.AwayTeam.DomainTeam);
        _events.Add(new TeamSet(_tick, new(TeamSide.Away, _gameState.Teams.AwayTeam)));

        _gameState.Lineups.HomeTeamLineup = PickLineup(TeamSide.Home);
        _gameState.Lineups.AwayTeamLineup = PickLineup(TeamSide.Away);

        var lastValidationTick = _tick;
        
        do
        {
            Action tickMethod = _gameState.Stage switch
            {
                Stage.BeforeGame => TickBeforeGame,
                Stage.Lineup => TickLineup,
                Stage.Jam => TickJam,
                Stage.Timeout => TickTimeout,
                Stage.AfterTimeout => TickAfterTimeout,
                Stage.Intermission => TickIntermission,
                _ => throw new Exception($"Unexpected game stage during simulation: {_gameState.Stage}")
            };
            tickMethod();

            TickLineupTracker();

            if (_tick - lastValidationTick > Tick.FromSeconds(ValidationIntervalInSeconds) && _events.All(e => e.Tick != _tick))
            {
                lastValidationTick = _tick;
                _events.Add(GenerateValidationEvent());
            }

            _tick += TickIncrement;
        }
        while (_gameState.Stage != Stage.AfterGame);

        foreach (var @event in _events)
        {
            if (@event is ValidateStateFakeEvent) continue;

            Console.WriteLine(@event.HasBody
                ? $"{@event.Tick}: {@event.GetType().Name} {JsonSerializer.Serialize(@event.GetBodyObject()!)}"
                : $"{@event.Tick}: {@event.GetType().Name}");
        }

        return _events.ToArray();
    }

    private Event GenerateValidationEvent() =>
        new ValidateStateFakeEvent(_tick, [
            new PeriodClockState(
                _gameState.Clocks.PeriodClock.Running,
                _gameState.Stage == Stage.BeforeGame 
                    || _gameState is { Stage: Stage.Lineup or Stage.Timeout or Stage.AfterTimeout, Jam: 0 }
                    || _gameState.Clocks.PeriodClock.Running && _gameState.Clocks.PeriodClock.TicksPassedAtLastStart + _tick - _gameState.Clocks.PeriodClock.LastStartTick >= Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds)
                    || !_gameState.Clocks.PeriodClock.Running && _gameState.Clocks.PeriodClock.PassedTicks >= Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds),
                _gameState.Clocks.PeriodClock.PassedTicks > 0,
                _gameState.Clocks.PeriodClock.LastStartTick,
                _gameState.Clocks.PeriodClock.TicksPassedAtLastStart,
                _gameState.Clocks.PeriodClock.Running
                    ? Math.Min(_gameState.Clocks.PeriodClock.TicksPassedAtLastStart + _tick - _gameState.Clocks.PeriodClock.LastStartTick, Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds))
                    : _gameState.Clocks.PeriodClock.PassedTicks
            ),
            new JamClockState(
                _gameState.Clocks.JamClock.Running,
                _gameState.Clocks.JamClock.LastStartTick,
                _gameState.Clocks.JamClock.Running
                    ? _gameState.Clocks.JamClock.TicksPassedAtLastStart + _tick - _gameState.Clocks.JamClock.LastStartTick
                    : _gameState.Clocks.JamClock.PassedTicks,
                true,
                !_gameState.JamInfo.HomeTeam.Called && !_gameState.JamInfo.AwayTeam.Called && _gameState.Clocks.JamClock.PassedTicks.Seconds == Rules.DefaultRules.JamRules.DurationInSeconds
            ),
            new LineupClockState(
                _gameState.Clocks.LineupClock.Running,
                _gameState.Clocks.LineupClock.LastStartTick,
                _gameState.Clocks.LineupClock.Running
                    ? _gameState.Clocks.LineupClock.TicksPassedAtLastStart + _tick - _gameState.Clocks.LineupClock.LastStartTick
                    : _gameState.Clocks.LineupClock.PassedTicks
            ),
            new IntermissionClockState(
                _gameState.Clocks.IntermissionClock.Running,
                _gameState.Stage is not Stage.Intermission || _gameState.Clocks.IntermissionClock.PassedTicks >= Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds),
                Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds),
                _gameState.Stage is Stage.Intermission or Stage.AfterGame ? _gameState.Clocks.IntermissionClock.LastStartTick + Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds) : 0,
                _gameState.Stage is Stage.Intermission or Stage.AfterGame ? (_gameState.Clocks.IntermissionClock.LastStartTick + Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds) - _tick).Seconds : 0
            ),
            ("Home", new PenaltyBoxState(
                _gameState.Lineups.HomeTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: not null })
                    .Select(s => s!.Number)
                    .ToArray(),
                _gameState.Lineups.HomeTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: null })
                    .Select(s => s!.Number)
                    .ToArray()
                )),
            ("Away", new PenaltyBoxState(
                _gameState.Lineups.AwayTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: not null })
                    .Select(s => s!.Number)
                    .ToArray(),
                _gameState.Lineups.AwayTeamLineup.Skaters
                    .Where(s => s?.Penalty is { EntryTick: null })
                    .Select(s => s!.Number)
                    .ToArray()
            )),
            ("Home", new JamLineupState(
                _gameState.Sheets.HomeSheets.LineupSheet[^1].JammerNumber,
                _gameState.Sheets.HomeSheets.LineupSheet[^1].PivotNumber,
                [.._gameState.Sheets.HomeSheets.LineupSheet[^1].BlockerNumbers]
            )),
            ("Away", new JamLineupState(
                _gameState.Sheets.AwaySheets.LineupSheet[^1].JammerNumber,
                _gameState.Sheets.AwaySheets.LineupSheet[^1].PivotNumber,
                [.._gameState.Sheets.AwaySheets.LineupSheet[^1].BlockerNumbers]
            )),
            ("Home", new TeamScoreState(_gameState.Scores.HomeScore.GameTotal, _gameState.Scores.HomeScore.JamTotal)),
            ("Away", new TeamScoreState(_gameState.Scores.AwayScore.GameTotal, _gameState.Scores.AwayScore.JamTotal)),
            ("Home", new TeamJamStatsState(
                _gameState.JamInfo.HomeTeam.Lead,
                _gameState.JamInfo.HomeTeam.Lost,
                _gameState.JamInfo.HomeTeam.Called,
                _gameState.Sheets.HomeSheets.ScoreSheet[^1].StarPassTrip != null,
                _gameState.JamInfo.HomeTeam.CompletedInitial)
            ),
            ("Away", new TeamJamStatsState(
                _gameState.JamInfo.AwayTeam.Lead,
                _gameState.JamInfo.AwayTeam.Lost,
                _gameState.JamInfo.AwayTeam.Called,
                _gameState.Sheets.AwaySheets.ScoreSheet[^1].StarPassTrip != null,
                _gameState.JamInfo.AwayTeam.CompletedInitial)
            ),
            new GameStageState(_gameState.Stage is Stage.AfterGame ? Stage.Intermission : _gameState.Stage, _gameState.Period + (_gameState.Stage == Stage.BeforeGame ? 1 : 0), _gameState.Jam, _gameState.TotalJam, false),
        ]);

    private void TickBeforeGame()
    {
        const float gameStartChance = 1f / (TicksPerSecond * 30);
        const float startLineupOnGameStartChance = 0.5f;

        if (!RandomTrigger(gameStartChance))
            return;

        if (RandomTrigger(startLineupOnGameStartChance))
        {
            LogDebug("Lineup starting before game");
            _events.Add(new IntermissionEnded(_tick));
            _gameState.Stage = Stage.Lineup;
            _gameState.Period = 1;
            _gameState.Jam = 0;
            _gameState.TotalJam = 0;
            _gameState.Clocks.LineupClock = new() { Running = true, LastStartTick = _tick };
            StartClock(c => c.LineupClock, reset: true);
        }
        else
        {
            LogDebug("Starting jam");
            _events.Add(new JamStarted(_tick));
            _gameState.Stage = Stage.Jam;
            _gameState.Period = 1;
            _gameState.Jam = 1;
            _gameState.TotalJam = 1;
            StartClock(c => c.JamClock, reset: true);
            StartClock(c => c.PeriodClock, reset: true);
        }
    }

    private void TickLineup()
    {
        const float timeoutChance = 1 / 150f / TicksPerSecond;

        TickClock(c => c.LineupClock);
        TickClock(c => c.PeriodClock, Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds));

        if (_gameState.Clocks.PeriodClock.PassedTicks >= Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds))
        {
            if (_gameState.Period == 1)
            {
                LogDebug("Period expired, entering intermission");
                _gameState.Stage = Stage.Intermission;
                _gameState.Clocks.PeriodClock.Running = false;
                _gameState.Clocks.PeriodClock.PassedTicks = Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds);
                _gameState.Clocks.LineupClock.Running = false;
                StartClock(c => c.IntermissionClock, reset: true);
                _events.Add(new DebugFakeEvent(_tick, "Intermission start"));
            }
            else
            {
                LogDebug("Period expired, ending game");
                _gameState.Stage = Stage.AfterGame;
                _gameState.Clocks = new();
            }

            return;
        }

        var passedSeconds = _gameState.Clocks.LineupClock.PassedTicks / (float) Tick.TicksPerSecond;

        if (passedSeconds >= Rules.DefaultRules.LineupRules.DurationInSeconds)
        {
            LogDebug("Starting jam");
            _events.Add(new JamStarted(_tick));
            _gameState.Stage = Stage.Jam;
            StopClock(c => c.LineupClock);
            StartClock(c => c.PeriodClock);
            StartClock(c => c.JamClock, reset: true, align: true);
            _gameState.Jam += 1;
            _gameState.TotalJam += 1;
            _gameState.JamInfo = new();
            _gameState.Scores.HomeScore.JamTotal = 0;
            _gameState.Scores.AwayScore.JamTotal = 0;

            return;
        }

        if (RandomTrigger(timeoutChance))
        {
            LogDebug("Starting timeout");

            if (_gameState.Clocks.PeriodClock.Running)
            {
                StopClock(c => c.LineupClock, align: true);
                StartClock(c => c.TimeoutClock, reset: true, align: true);
                StopClock(c => c.PeriodClock, align: true);
            }
            else
            {
                StartClock(c => c.TimeoutClock, align: true, alignTo: _gameState.Clocks.LineupClock);
                StopClock(c => c.LineupClock);
            }

            _gameState.Stage = Stage.Timeout;
            _gameState.TimeoutInfo = new();

            _events.Add(new TimeoutStarted(_tick));

            return;
        }
    }

    private void TickJam()
    {
        const float penaltyChance = 1 / 180f / TicksPerSecond;
        const float completeTripChance = 1 / 15f / TicksPerSecond;
        const float boxEntryChance = 1 / 15f / TicksPerSecond;
        const float callChance = 1 / 160f / TicksPerSecond;

        TickClock(c => c.JamClock);
        TickClock(c => c.PeriodClock, Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds));

        if (_gameState.Clocks.JamClock.PassedTicks.Seconds >= Rules.DefaultRules.JamRules.DurationInSeconds)
        {
            EndJam("expired");
            _gameState.Clocks.JamClock.PassedTicks = Tick.FromSeconds(Rules.DefaultRules.JamRules.DurationInSeconds);
            return;
        }

        if ((_gameState.Lineups.HomeTeamLineup.Skaters[0]?.Penalty == null || _gameState.Lineups.AwayTeamLineup.Skaters[0]?.Penalty == null) && RandomTrigger(completeTripChance))
        {
            var tripTeamSide =
                _gameState.Lineups.HomeTeamLineup.Skaters[0]?.Penalty != null ? TeamSide.Away
                : _gameState.Lineups.AwayTeamLineup.Skaters[0]?.Penalty != null ? TeamSide.Home
                : Random.Shared.Next(0, 2) == 0 ? TeamSide.Home : TeamSide.Away;

            var tripTeamJamInfo = (tripTeamSide == TeamSide.Home ? _gameState.JamInfo.HomeTeam : _gameState.JamInfo.AwayTeam);
            var tripTeamScoreSheet = (tripTeamSide == TeamSide.Home
                ? _gameState.Sheets.HomeSheets.ScoreSheet
                : _gameState.Sheets.AwaySheets.ScoreSheet);

            var leadAvailable = !_gameState.JamInfo.HomeTeam.Lead && !_gameState.JamInfo.AwayTeam.Lead && !tripTeamJamInfo.Lost;
            if (leadAvailable)
            {
                tripTeamJamInfo.Lead = true;
                tripTeamJamInfo.CompletedInitial = true;
                tripTeamScoreSheet[^1].Lead = true;
                tripTeamScoreSheet[^1].NoInitial = false;
                tripTeamScoreSheet[^1].TripScores.Add(0);
                _events.Add(new LeadMarked(_tick, new(tripTeamSide, true)));
                LogDebug($"Lead earned for {tripTeamSide} team");
            }
            else
            {
                if (tripTeamScoreSheet[^1].TripScores.Count == 0)
                {
                    tripTeamJamInfo.CompletedInitial = true;
                    tripTeamScoreSheet[^1].NoInitial = false;
                    _events.Add(new InitialTripCompleted(_tick, new(tripTeamSide, true)));
                    LogDebug($"Initial trip completed for {tripTeamSide} team");
                }
                else
                {
                    SetTripScore(tripTeamSide, 4);
                }
                tripTeamScoreSheet[^1].TripScores.Add(0);
            }
        }

        if (RandomTrigger(penaltyChance))
        {
            AddRandomPenalty();
        }

        if (
            (_gameState.JamInfo.HomeTeam.Lead || _gameState.JamInfo.AwayTeam.Lead) 
            && RandomTrigger(callChance * (_gameState.JamInfo.HomeTeam.CompletedInitial && _gameState.JamInfo.AwayTeam.CompletedInitial ? 4 : 1))
            && Rules.DefaultRules.JamRules.DurationInSeconds - _gameState.Clocks.JamClock.PassedTicks.Seconds > 1 /* Hard to calculate state when called in the last second */)
        {
            _gameState.JamInfo.HomeTeam.Called = _gameState.JamInfo.HomeTeam.Lead;
            _gameState.JamInfo.AwayTeam.Called = _gameState.JamInfo.AwayTeam.Lead;
            _gameState.Sheets.HomeSheets.ScoreSheet[^1].Called = _gameState.Sheets.HomeSheets.ScoreSheet[^1].Lead;
            _gameState.Sheets.AwaySheets.ScoreSheet[^1].Called = _gameState.Sheets.AwaySheets.ScoreSheet[^1].Lead;
            
            EndJam("called");
            var alignedTick = AlignToClock(_tick, _gameState.Clocks.PeriodClock);
            _gameState.Clocks.JamClock.PassedTicks = alignedTick - _gameState.Clocks.JamClock.LastStartTick;
            LogDebug($"Jam ended at {alignedTick} with last start tick of {_gameState.Clocks.JamClock.LastStartTick}. Passed ticks is {_gameState.Clocks.JamClock.PassedTicks}");
            _events.Add(new CallMarked(_tick, new(_gameState.JamInfo.HomeTeam.Lead ? TeamSide.Home : TeamSide.Away, true)));
        }

        for (var i = 0; i < 5; ++i)
        {
            if (_gameState.Lineups.HomeTeamLineup.Skaters[i] != null)
                TickBoxVisit(_gameState.Lineups.HomeTeamLineup.Skaters[i]!, TeamSide.Home);
            if (_gameState.Lineups.AwayTeamLineup.Skaters[i] != null)
                TickBoxVisit(_gameState.Lineups.AwayTeamLineup.Skaters[i]!, TeamSide.Away);
        }

        return;

        void TickBoxVisit(LineupSkater skater, TeamSide teamSide)
        {
            var spaceInBox = teamSide == TeamSide.Home
                ? _gameState.Lineups.HomeTeamLineup.Skaters[1..4].Count(s => s?.Penalty is { EntryTick: not null }) < 2
                : _gameState.Lineups.AwayTeamLineup.Skaters[1..4].Count(s => s?.Penalty is { EntryTick: not null }) < 2;

            if (!spaceInBox)
                return;

            if (skater.Penalty is null)
                return;

            if (skater.Penalty is { EntryTick: not null })
            {
                skater.Penalty.TicksServed += TickIncrement;
                var penaltyComplete = skater.Penalty is { EntryTick: not null } &&
                                      skater.Penalty.TicksServed > Tick.FromSeconds(30 * skater.Penalty.PenaltyCount);

                if (penaltyComplete)
                {
                    _events.Add(new SkaterReleasedFromBox(_tick, new(teamSide, skater.Number)));
                    skater.Penalty = null;
                    LogDebug($"Skater {skater.Number} on {teamSide} released from box");
                    return;
                }
            }

            if (skater.Penalty is not { EntryTick: null })
                return;

            if (!RandomTrigger(boxEntryChance))
                return;

            LogDebug($"Skater {skater.Number} on {teamSide} sat in box");
            _events.Add(new SkaterSatInBox(_tick, new(teamSide, skater.Number)));
            skater.Penalty.EntryTick = _tick;
        }

        void SetTripScore(TeamSide side, int score)
        {
            var tripTeamScoreSheet = side == TeamSide.Home
                ? _gameState.Sheets.HomeSheets.ScoreSheet
                : _gameState.Sheets.AwaySheets.ScoreSheet;

            tripTeamScoreSheet[^1].GameTotal += score;
            tripTeamScoreSheet[^1].JamTotal += score;
            tripTeamScoreSheet[^1].TripScores[^1] = score;
            if (side == TeamSide.Home)
            {
                _gameState.Scores.HomeScore.GameTotal += score;
                _gameState.Scores.HomeScore.JamTotal += score;
            }
            else
            {
                _gameState.Scores.AwayScore.GameTotal += score;
                _gameState.Scores.AwayScore.JamTotal += score;
            }

            LogDebug($"Adding {score} points for {side} team");
            _events.Add(new ScoreModifiedRelative(_tick, new(side, score)));

        }

        void EndJam(string endReason)
        {
            StopClock(c => c.JamClock);

            if(_gameState.JamInfo.HomeTeam.CompletedInitial)
                SetTripScore(TeamSide.Home, Random.Shared.Next(5));
            if (_gameState.JamInfo.AwayTeam.CompletedInitial)
                SetTripScore(TeamSide.Away, Random.Shared.Next(5));

            if (_gameState.Clocks.PeriodClock.PassedTicks >= Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds))
            {
                LogDebug($"Lineups: {JsonSerializer.Serialize(_gameState.Lineups)}");

                if (_gameState.Period == 1)
                {
                    LogDebug($"Jam {endReason} and period expired, starting intermission");
                    _gameState.Stage = Stage.Intermission;
                    StopClock(c => c.PeriodClock, maxTick: Tick.FromSeconds(Rules.DefaultRules.PeriodRules.DurationInSeconds));
                    StartClock(c => c.LineupClock, reset: true, align: true);
                    StopClock(c => c.LineupClock);
                    _gameState.Clocks.LineupClock.PassedTicks = -1;
                    StartClock(c => c.IntermissionClock, reset: true, align: true);
                    _events.Add(new DebugFakeEvent(_tick, "Intermission start"));
                }
                else
                {
                    LogDebug($"Jam {endReason} and period expired, ending game");
                    _gameState.Stage = Stage.AfterGame;
                    _gameState.Clocks = new();
                }

                AddJam();
                return;
            }

            LogDebug($"Jam {endReason}, starting lineup");
            _gameState.Stage = Stage.Lineup;
            StartClock(c => c.LineupClock, reset: true, align: true);
            AddJam();
            _gameState.Lineups.HomeTeamLineup = PickLineup(TeamSide.Home);
            _gameState.Lineups.AwayTeamLineup = PickLineup(TeamSide.Away);
        }
    }

    private void AddRandomPenalty()
    {
        var penaltySide = RandomTrigger(.5f) ? TeamSide.Home : TeamSide.Away;
        var lineup = penaltySide == TeamSide.Home ? _gameState.Lineups.HomeTeamLineup : _gameState.Lineups.AwayTeamLineup;
        var availableSkaterIndexes = lineup.Skaters
            .Select((x, i) => (Index: i, InBox: x?.Penalty?.EntryTick != null))
            .Where(x => !x.InBox)
            .Select(x => x.Index)
            .ToArray();

        if (availableSkaterIndexes.Length == 0)
            return;

        var penaltySkaterIndex = availableSkaterIndexes.Random();
        var scoreSheet = penaltySide == TeamSide.Home
            ? _gameState.Sheets.HomeSheets.ScoreSheet
            : _gameState.Sheets.AwaySheets.ScoreSheet;

        var starPass = scoreSheet[^1].StarPassTrip != null;

        if (penaltySkaterIndex == 0 && !starPass /*Jammer*/)
        {
            var jamInfo = penaltySide == TeamSide.Home ? _gameState.JamInfo.HomeTeam : _gameState.JamInfo.AwayTeam;
            var lost = jamInfo.Lost || jamInfo.Lead || (!_gameState.JamInfo.HomeTeam.Lead && !_gameState.JamInfo.AwayTeam.Lead);
            jamInfo.Lost = lost;
            scoreSheet[^1].Lost = lost;
        }

        var lineupJam = penaltySide == TeamSide.Home
            ? _gameState.Sheets.HomeSheets.LineupSheet[^1]
            : _gameState.Sheets.AwaySheets.LineupSheet[^1];

        var skater = lineup.Skaters[penaltySkaterIndex];

        if (skater == null)
            return;

        if (!lineupJam.SkaterNumbers.Contains(skater.Number))
        {
            if (penaltySkaterIndex < 2)
            {
                // No good way of validating skaters auto-added as blockers when they should be jammers or pivots. Just add them to the jam
                _events.Add(new SkaterOnTrack(_tick, new(penaltySide, skater.Number, penaltySkaterIndex == 0 ? SkaterPosition.Jammer : SkaterPosition.Pivot)));
                if (penaltySkaterIndex == 0)
                    lineupJam.JammerNumber = skater.Number;
                else
                    lineupJam.PivotNumber = skater.Number;
            }
            else
            {
                LogDebug($"Adding skater {lineup.Skaters[penaltySkaterIndex]!.Number} on {penaltySide} team to jam due to penalty");
                lineupJam.BlockerNumbers = lineupJam.BlockerNumbers.Where(b => b != null).Append(skater.Number).Pad(3, null).ToArray();
            }
        }

        LogDebug($"Skater {skater.Number} on {penaltySide} team penalized");

        _events.Add(new PenaltyAssessed(_tick, new(penaltySide, skater.Number, "X")));
        skater.Penalty = new();
    }

    private void TickTimeout()
    {
        const float setTimeoutChance = 1 / 5f / TicksPerSecond;
        const float officialTimeoutEndChance = 1 / 30f / TicksPerSecond;

        TickClock(c => c.TimeoutClock);

        if (_gameState.TimeoutInfo.Type == TimeoutType.Untyped && RandomTrigger(setTimeoutChance))
        {
            var timeoutType = Random.Shared.Next(100) switch
            {
                < 50 => TimeoutType.Official,
                < 90 => TimeoutType.Team,
                _ => TimeoutType.Review
            };

            var timeoutTeam = RandomTrigger(.5f) ? TeamSide.Home : TeamSide.Away;
            var teamTimeoutsRemaining = timeoutTeam == TeamSide.Home
                ? _gameState.TimeoutInfo.HomeTeam.RemainingTimeouts
                : _gameState.TimeoutInfo.AwayTeam.RemainingTimeouts;
            var teamReviewStatus = timeoutTeam == TeamSide.Home
                ? _gameState.TimeoutInfo.HomeTeam.ReviewStatus
                : _gameState.TimeoutInfo.AwayTeam.ReviewStatus;

            switch (timeoutType)
            {
                case TimeoutType.Official:
                    LogDebug("Setting timeout type to Official");
                    _gameState.TimeoutInfo.Type = TimeoutType.Official;
                    _events.Add(new TimeoutTypeSet(_tick, new(TimeoutType.Official, null)));
                    break;

                case TimeoutType.Team when teamTimeoutsRemaining > 0:
                    LogDebug($"Setting timeout type to Team for {timeoutType} team");
                    _gameState.TimeoutInfo.Type = TimeoutType.Team;
                    _gameState.TimeoutInfo.Side = timeoutTeam;
                    if (timeoutTeam == TeamSide.Home) 
                        _gameState.TimeoutInfo.HomeTeam.RemainingTimeouts--;
                    else 
                        _gameState.TimeoutInfo.AwayTeam.RemainingTimeouts--;
                    _events.Add(new TimeoutTypeSet(_tick, new(TimeoutType.Team, timeoutTeam)));
                    break;

                case TimeoutType.Review when teamReviewStatus != ReviewStatus.Used:
                    LogDebug($"Setting timeout type to Review for {timeoutType} team");
                    _gameState.TimeoutInfo.Type = TimeoutType.Review;
                    _gameState.TimeoutInfo.Side = timeoutTeam;
                    _events.Add(new TimeoutTypeSet(_tick, new(TimeoutType.Review, timeoutTeam)));
                    break;
            }
        }

        if (_gameState.TimeoutInfo.Type == TimeoutType.Official && RandomTrigger(officialTimeoutEndChance))
        {
            _gameState.Stage = Stage.AfterTimeout;
            _events.Add(new TimeoutEnded(_tick));
        }

        if (_gameState.TimeoutInfo.Type != TimeoutType.Official && _gameState.Clocks.TimeoutClock.PassedTicks.Seconds >= Rules.DefaultRules.TimeoutRules.TeamTimeoutDurationInSeconds)
        {
            _gameState.Stage = Stage.AfterTimeout;
            _events.Add(new TimeoutEnded(_tick));
        }
    }

    private void TickAfterTimeout()
    {
        const float jamStartChance = 1 / 20f / TicksPerSecond;

        TickClock(c => c.TimeoutClock);

        if (RandomTrigger(jamStartChance))
        {
            LogDebug("Starting jam");
            _events.Add(new JamStarted(_tick));
            _gameState.Stage = Stage.Jam;
            StopClock(c => c.TimeoutClock);
            _gameState.Clocks.TimeoutClock.PassedTicks -= 1;
            StartClock(c => c.PeriodClock);
            StartClock(c => c.JamClock, reset: true, align: true);
            _gameState.Jam += 1;
            _gameState.TotalJam += 1;
            _gameState.JamInfo = new();
            _gameState.Scores.HomeScore.JamTotal = 0;
            _gameState.Scores.AwayScore.JamTotal = 0;
        }
    }

    private void TickIntermission()
    {
        TickClock(c => c.IntermissionClock);

        if (_gameState.Clocks.IntermissionClock.PassedTicks >= Tick.FromSeconds(Rules.DefaultRules.IntermissionRules.DurationInSeconds))
        {
            _gameState.Stage = Stage.AfterGame;
        }
    }

    private void TickLineupTracker()
    {
        const float recordSkaterChance = 0.2f / TicksPerSecond;

        if (_gameState.Stage is Stage.Intermission or Stage.AfterGame)
            return;

        if (_gameState.Clocks.LineupClock.Running && _gameState.Clocks.LineupClock.PassedTicks < Tick.FromSeconds(5))
            return;

        if (_gameState.Sheets.HomeSheets.LineupSheet[^1].SkaterNumbers.Length < 5 && RandomTrigger(recordSkaterChance * (_gameState.Stage == Stage.Jam ? 2 : 1)))
            SetCurrentJam(TeamSide.Home);

        if (_gameState.Sheets.AwaySheets.LineupSheet[^1].SkaterNumbers.Length < 5 && RandomTrigger(recordSkaterChance * (_gameState.Stage == Stage.Jam ? 2 : 1)))
            SetCurrentJam(TeamSide.Away);

        return;

        (string Number, int Position) GetSkaterToLineup(TeamLineup lineup, string[] currentlyTrackedSkaters)
        {
            var skaterNumber = lineup.Skaters.ExceptBy(currentlyTrackedSkaters, x => x?.Number).ToArray().Random()!;

            var position = lineup.Skaters
                .Select((n, i) => (n?.Number, Position: i))
                .Single(p => p.Number == skaterNumber.Number)
                .Position;

            return (skaterNumber.Number, position);
        }

        void SetCurrentJam(TeamSide teamSide)
        {
            var jams = (teamSide == TeamSide.Home ? _gameState.Sheets.HomeSheets : _gameState.Sheets.AwaySheets).LineupSheet;
            var jamLine = jams[^1];
            var (skater, position) = GetSkaterToLineup(teamSide == TeamSide.Home ? _gameState.Lineups.HomeTeamLineup : _gameState.Lineups.AwayTeamLineup, jamLine.SkaterNumbers.ToArray());
            switch (position)
            {
                case 0:
                    jamLine.JammerNumber = skater;
                    break;

                case 1:
                    jamLine.PivotNumber = skater;
                    break;

                default:
                    jamLine.BlockerNumbers = jamLine.BlockerNumbers.Where(b => b != null).Append(skater).Pad(3, null).ToArray();
                    break;
            }
            LogDebug($"Lining up \"{skater}\" as {position switch { 0 => "jammer", 1 => "pivot", _ => "blocker" }} for {teamSide} team in jam {jams.Count}. Lineup: {jamLine.SkaterNumbers.Map(string.Join, ", ")}");
            _events.Add(new SkaterOnTrack(_tick, new(
                teamSide,
                skater,
                position switch
                {
                    0 => SkaterPosition.Jammer,
                    1 => SkaterPosition.Pivot,
                    _ => SkaterPosition.Blocker
                })));
        }
    }

    private TeamLineup ClearLineup(TeamSide teamSide)
    {
        var currentLineup = teamSide switch
        {
            TeamSide.Home => _gameState.Lineups.HomeTeamLineup,
            TeamSide.Away => _gameState.Lineups.AwayTeamLineup,
            _ => throw new Exception()
        };

        var lineup = currentLineup.Skaters.Select(s => s?.Penalty is null ? null : s).Pad(5, null).ToArray();

        return new() { Skaters = lineup };
    }

    private TeamLineup PickLineup(TeamSide teamSide)
    {
        var (team, currentLineup, lineupSheet) = teamSide switch
        {
            TeamSide.Home => (_gameState.Teams.HomeTeam, _gameState.Lineups.HomeTeamLineup, _gameState.Sheets.HomeSheets.LineupSheet),
            TeamSide.Away => (_gameState.Teams.AwayTeam, _gameState.Lineups.AwayTeamLineup, _gameState.Sheets.AwaySheets.LineupSheet),
            _ => throw new Exception()
        };

        var sortedRoster = team.Roster.OrderBy(s => lineupSheet.SelectMany(j => j.SkaterNumbers).Count(n => s.Number == n)).ToArray();

        var lineup = currentLineup.Skaters.Select(s => s?.Penalty is null ? null : s).Pad(5, null).ToArray();

        for (var i = 0; i < 5; ++i)
        {
            if (lineup[i] is not null)
                continue;

            var skaterNumber = sortedRoster.Where(s => lineup.All(l => l?.Number != s.Number)).ToArray().RandomFavorStart()!.Number;
            lineup[i] = new() { Number = skaterNumber };
        }

        return new() { Skaters = lineup };
    }

    private static bool RandomTrigger(float chance) =>
        Random.Shared.NextSingle() <= chance;

    private static GameTeam ToGameTeam(Team team) => new(
        team.Names,
        team.Colors.Values.First(),
        team.Roster.Select(s => new GameSkater(s.Number, s.Name, true)).ToList());

    private void LogDebug(string message) =>
        Console.WriteLine($"{_tick}: {message}");

    private class GameState
    {
        public Stage Stage { get; set; } = Stage.BeforeGame;
        public int Period { get; set; } = 0;
        public int Jam { get; set; } = 0;
        public int TotalJam { get; set; } = 0;
        public TeamJamInfo JamInfo { get; set; } = new();
        public TimeoutInfo TimeoutInfo { get; set; } = new();
        public Teams Teams { get; set; } = new();
        public Scores Scores { get; set; } = new();
        public Lineups Lineups { get; set; } = new();
        public Clocks Clocks { get; set; } = new();
        public TeamsSheets Sheets { get; set; } = new();
    }

    private class TeamJamInfo
    {
        public JamInfo HomeTeam { get; set; } = new();
        public JamInfo AwayTeam { get; set; } = new();
        public bool Injury { get; set; } = false;
    }

    private class JamInfo
    {
        public bool Lost { get; set; } = false;
        public bool Lead { get; set; } = false;
        public bool Called { get; set; } = false;
        public bool CompletedInitial { get; set; } = false;
    }

    private class TimeoutInfo
    {
        public TimeoutType Type { get; set; } = TimeoutType.Untyped;
        public TeamSide? Side { get; set; } = null;
        public TeamTimeoutInfo HomeTeam { get; set; } = new();
        public TeamTimeoutInfo AwayTeam { get; set; } = new();
    }

    private class TeamTimeoutInfo
    {
        public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Unused;
        public int RemainingTimeouts { get; set; } = Rules.DefaultRules.TimeoutRules.TeamTimeoutAllowance;
    }

    private class Teams
    {
        public GameTeam HomeTeam { get; set; } = new([], new (Color.White, Color.Black), []);
        public GameTeam AwayTeam { get; set; } = new([], new(Color.Black, Color.White), []);
    }

    private class Clock
    {
        public Tick PassedTicks { get; set; } = 0;
        public bool Running { get; set; } = false;
        public Tick LastStartTick { get; set; } = 0;
        public Tick TicksPassedAtLastStart { get; set; } = 0;
    }

    private class Clocks
    {
        public Clock PeriodClock { get; set; } = new();
        public Clock JamClock { get; set; } = new();
        public Clock LineupClock { get; set; } = new();
        public Clock IntermissionClock { get; set; } = new();
        public Clock TimeoutClock { get; set; } = new();
    }

    private class Penalty
    {
        public Tick? EntryTick { get; set; } = null;
        public int PenaltyCount { get; set; } = 1;
        public Tick TicksServed { get; set; } = 0;
    }

    private class LineupSkater
    {
        public string Number { get; set; } = string.Empty;
        public Penalty? Penalty { get; set; } = null;
    }

    private class TeamLineup
    {
        public LineupSkater?[] Skaters { get; set; } = [];
    }

    private class Lineups
    {
        public TeamLineup HomeTeamLineup { get; set; } = new();
        public TeamLineup AwayTeamLineup { get; set; } = new();
    }

    private class Scores
    {
        public TeamScore HomeScore { get; set; } = new();
        public TeamScore AwayScore { get; set; } = new();
    }

    private class TeamScore
    {
        public int GameTotal { get; set; } = 0;
        public int JamTotal { get; set; } = 0;
    }

    private class Sheets
    {
        public List<ScoreSheetJam> ScoreSheet { get; set; } = [new() { Period = 1, Jam = 1 }];
        public List<LineupSheetJam> LineupSheet { get; set; } = [new() { Period = 1, Jam = 1 }];
        public List<PenaltySheetLine> PenaltySheet { get; set; } = [];
    }

    private class ScoreSheetJam
    {
        public int Period { get; set; } = 0;
        public int Jam { get; set; } = 0;
        public string JammerNumber { get; set; } = string.Empty;
        public string PivotNumber { get; set; } = string.Empty;
        public bool Lost { get; set; } = false;
        public bool Lead { get; set; } = false;
        public bool Called { get; set; } = false;
        public bool Injury { get; set; } = false;
        public bool NoInitial { get; set; } = true;
        public List<int> TripScores { get; set; } = [];
        public int? StarPassTrip { get; set; } = null;
        public int JamTotal { get; set; } = 0;
        public int GameTotal { get; set; } = 0;
    }

    private class LineupSheetJam
    {
        public int Period { get; set; } = 0;
        public int Jam { get; set; } = 0;
        public bool HasStarPass { get; set; } = false;
        public string? JammerNumber { get; set; } = null;
        public string? PivotNumber { get; set; } = null;
        public string?[] BlockerNumbers { get; set; } = [null, null, null];

        public string[] SkaterNumbers => ((string?[])[JammerNumber, PivotNumber, ..BlockerNumbers]).Where(s => s != null).ToArray()!;
    }

    private class PenaltySheetLine
    {
        public string SkaterNumber { get; set; } = string.Empty;
        public List<engine.Reducers.Penalty> Penalties { get; set; } = new();
    }

    private class TeamsSheets
    {
        public Sheets HomeSheets { get; set; } = new();
        public Sheets AwaySheets { get; set; } = new();
    }

    private void TickClock(Expression<Func<Clocks, Clock>> clockSelector, Tick? maxTick = null)
    {
        var clockName = (clockSelector.Body as MemberExpression)!.Member.Name;
        var clockProperty = typeof(Clocks).GetProperty(clockName)!;
        var clock = (Clock)clockProperty.GetValue(_gameState.Clocks)!;

        if (clock.Running)
        {
            clock.PassedTicks = clock.TicksPassedAtLastStart + _tick - clock.LastStartTick;
            if (maxTick != null && clock.PassedTicks > maxTick)
                clock.PassedTicks = (Tick)maxTick;
        }

        clockProperty.SetValue(_gameState.Clocks, clock);
    }

    private void StartClock(Expression<Func<Clocks, Clock>> clockSelector, bool reset = false, bool align = false, Clock? alignTo = null)
    {
        var clockName = (clockSelector.Body as MemberExpression)!.Member.Name;
        var clockProperty = typeof(Clocks).GetProperty(clockName)!;
        var clock = (Clock)clockProperty.GetValue(_gameState.Clocks)!;
        var alignedTick = AlignToClock(_tick, alignTo ?? _gameState.Clocks.PeriodClock);

        if (!clock.Running)
        {
            clock.Running = true;
            clock.LastStartTick = align ? alignedTick : _tick;
            clock.PassedTicks = (reset, align) switch
            {
                (true, _) => 0,
                (false, true) => _tick - alignedTick,
                _ => clock.PassedTicks
            };
            clock.TicksPassedAtLastStart = clock.PassedTicks;
        }

        clockProperty.SetValue(_gameState.Clocks, clock);
    }

    private void StopClock(Expression<Func<Clocks, Clock>> clockSelector, bool align = false, Tick? maxTick = null)
    {
        var clockName = (clockSelector.Body as MemberExpression)!.Member.Name;
        var clockProperty = typeof(Clocks).GetProperty(clockName)!;
        var clock = (Clock)clockProperty.GetValue(_gameState.Clocks)!;

        if (clock.Running)
        {
            clock.Running = false;
            clock.PassedTicks = align
                ? clock.TicksPassedAtLastStart + AlignToClock(_tick, _gameState.Clocks.PeriodClock) - clock.LastStartTick
                : clock.TicksPassedAtLastStart + _tick - clock.LastStartTick;

            if (maxTick != null && clock.PassedTicks > maxTick)
                clock.PassedTicks = (Tick)maxTick;
        }

        clockProperty.SetValue(_gameState.Clocks, clock);
    }


    private static Tick AlignToClock(Tick tick, Clock clock) =>
        clock.LastStartTick + (long)Math.Round((tick - clock.LastStartTick) / (float)Tick.TicksPerSecond) * Tick.TicksPerSecond;

    private void AddJam()
    {
        _gameState.Sheets.HomeSheets.ScoreSheet.Add(new());
        _gameState.Sheets.HomeSheets.LineupSheet.Add(new());
        MovePenalizedSkatersToNextJam(TeamSide.Home);
        _gameState.Sheets.AwaySheets.ScoreSheet.Add(new());
        _gameState.Sheets.AwaySheets.LineupSheet.Add(new());
        MovePenalizedSkatersToNextJam(TeamSide.Away);
    }

    private void MovePenalizedSkatersToNextJam(TeamSide teamSide)
    {
        var lineupSheet = teamSide == TeamSide.Home
            ? _gameState.Sheets.HomeSheets.LineupSheet
            : _gameState.Sheets.AwaySheets.LineupSheet;

        var teamLineup = teamSide == TeamSide.Home
            ? _gameState.Lineups.HomeTeamLineup
            : _gameState.Lineups.AwayTeamLineup;

        foreach (var penalizedSkater in teamLineup.Skaters.Where(s => s?.Penalty != null))
        {
            if (penalizedSkater!.Penalty!.EntryTick == null)
            {
                penalizedSkater.Penalty!.EntryTick = _tick;
                _events.Add(new SkaterSatInBox(_tick, new(teamSide, penalizedSkater.Number)));
            }

            var lastJam = lineupSheet[^2];
            var position =
                lastJam.JammerNumber == penalizedSkater.Number ? SkaterPosition.Jammer
                : lastJam.PivotNumber == penalizedSkater.Number ? SkaterPosition.Pivot
                : SkaterPosition.Blocker;
            var thisJam = lineupSheet[^1];

            switch (position)
            {
                case SkaterPosition.Jammer:
                    thisJam.JammerNumber = penalizedSkater.Number;
                    break;

                case SkaterPosition.Pivot:
                    thisJam.PivotNumber = penalizedSkater.Number;
                    break;

                default:
                    thisJam.BlockerNumbers = thisJam.BlockerNumbers.Where(s => s != null).Append(penalizedSkater.Number).Pad(3, null).ToArray();
                    break;
            }

            LogDebug($"Skater {penalizedSkater.Number} in box as {position} between jams");
        }
    }
}