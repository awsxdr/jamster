namespace amethyst.Domain;

public record ActivityData(ClientActivity Activity, string? GameId, string LanguageCode);
public record UnknownActivity() : ActivityData(ClientActivity.Unknown, null, "en");
public record ScoreboardActivity(string? GameId, string LanguageCode, bool UseSidebars, bool UseNameBackgrounds) : ActivityData(ClientActivity.Scoreboard, GameId, LanguageCode);
public record StreamOverlayActivity(string? GameId, string LanguageCode, double Scale, bool UseBackground, string BackgroundColor) : ActivityData(ClientActivity.StreamOverlay, GameId, LanguageCode);
public record PenaltyWhiteboardActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.PenaltyWhiteboard, GameId, LanguageCode);
public record ScoreboardOperatorActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.ScoreboardOperator, GameId, LanguageCode);
public record PenaltyLineupControlActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.PenaltyLineupControl, GameId, LanguageCode);
public record PenaltyControlActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.PenaltyControl, GameId, LanguageCode);
public record LineupControlActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.LineupControl, GameId, LanguageCode);
public record BoxTimingActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.BoxTiming, GameId, LanguageCode);
public record OtherActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.Other, GameId, LanguageCode);

public enum ClientActivity
{
    Unknown,
    Scoreboard,
    StreamOverlay,
    PenaltyWhiteboard,
    ScoreboardOperator,
    PenaltyLineupControl,
    PenaltyControl,
    LineupControl,
    BoxTiming,
    Other,
}

