using jamster.engine.Domain;
using jamster.engine.Events;

using OpenQA.Selenium;

namespace jamster.ui.tests.Interactors;

public class ScoreboardOperatorInteractor(IWebDriver driver) : Interactor(driver)
{
    public void ClickGameSelect() =>
        Wait.Until(driver =>
            {
                var gameSelect = driver.FindElement(By.Id("ScoreboardControl.GameSelectMenu"));
                return (gameSelect.Displayed, gameSelect);
            },
            gameSelect => gameSelect.Click());

    public void SelectGame(string gameName) =>
        Wait.Until(driver =>
            {
                var option = driver.FindElement(By.XPath($"//div[@role=\"option\"]/span[text()=\"{gameName}\"]"));
                return (option.Displayed, option);
            },
            option => option.Click());

    public void ClickStart() =>
        Wait.Until(driver =>
            {
                var startButton = driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));
                return (startButton is { Displayed: true, Enabled: true }, startButton);
            },
            startButton => startButton.Click());

    public void ValidateStartEnabled(bool shouldBeEnabled) =>
        Wait.Until(driver =>
        {
            var startButton = driver.FindElement(By.Id("ScoreboardControl.MainControls.StartButton"));
            return startButton.Displayed && startButton.Enabled == shouldBeEnabled;
        });

    public void ClickStop() =>
        Wait.Until(driver =>
            {
                var endButton = driver.FindElement(By.Id("ScoreboardControl.MainControls.StopButton"));
                return (endButton is { Displayed: true, Enabled: true }, endButton);
            },
            endButton => endButton.Click());

    public void ValidateStopEnabled(bool shouldBeEnabled) =>
        Wait.Until(driver =>
        {
            var endButton = driver.FindElement(By.Id("ScoreboardControl.MainControls.StopButton"));
            return endButton.Enabled == shouldBeEnabled;
        });

    public void ClickNewTimeout() =>
        Wait.Until(driver =>
            {
                var newTimeoutButton = driver.FindElement(By.Id("ScoreboardControl.MainControls.TimeoutButton"));
                return (newTimeoutButton is { Displayed: true, Enabled: true }, newTimeoutButton);
            },
            newTimeoutButton => newTimeoutButton.Click());

    public void ValidateNewTimeoutEnabled(bool shouldBeEnabled) =>
        Wait.Until(driver =>
        {
            var newTimeoutButton = driver.FindElement(By.Id("ScoreboardControl.MainControls.TimeoutButton"));
            return newTimeoutButton.Enabled == shouldBeEnabled;
        });

    public void SetLead(TeamSide side) =>
        Wait.Until(driver =>
            {
                var leadButton = driver.FindElement(By.Id($"ScoreboardControl.JamStats.{(side == TeamSide.Home ? "Home" : "Away")}.LeadButton"));
                return (leadButton is { Displayed: true, Enabled: true }, leadButton);
            },
            leadButton => leadButton.Click());

    public void SetLost(TeamSide side) =>
        Wait.Until(driver =>
            {
                var lostButton = driver.FindElement(By.Id($"ScoreboardControl.JamStats.{(side == TeamSide.Home ? "Home" : "Away")}.LostButton"));
                return (lostButton is { Displayed: true, Enabled: true }, lostButton);
            },
            lostButton => lostButton.Click());

    public void SetCall(TeamSide side) =>
        Wait.Until(driver =>
            {
                var callButton = driver.FindElement(By.Id($"ScoreboardControl.JamStats.{(side == TeamSide.Home ? "Home" : "Away")}.CallButton"));
                return (callButton is { Displayed: true, Enabled: true }, callButton);
            },
            callButton => callButton.Click());

    public void SetStarPass(TeamSide side) =>
        Wait.Until(driver =>
            {
                var starPassButton = driver.FindElement(By.Id($"ScoreboardControl.JamStats.{(side == TeamSide.Home ? "Home" : "Away")}.StarPassButton"));
                return (starPassButton is { Displayed: true, Enabled: true }, starPassButton);
            },
            starPassButton => starPassButton.Click());

    public void SetInitialTrip(TeamSide side) =>
        Wait.Until(driver =>
            {
                var initialTripButton = driver.FindElement(By.Id($"ScoreboardControl.JamStats.{(side == TeamSide.Home ? "Home" : "Away")}.InitialButton"));
                return (initialTripButton is { Displayed: true, Enabled: true }, initialTripButton);
            },
            initialTripButton => initialTripButton.Click());

    public void LineupSkater(TeamSide side, SkaterPosition position, string number) =>
        Wait.Until(driver =>
            {
                var lineupButton = driver.FindElement(By.Id($"ScoreboardControl.TeamLineup.{side}.{position}.{number}"));
                return (lineupButton is { Displayed: true, Enabled: true }, lineupButton);
            },
            lineupButton => lineupButton.Click());

    public void SetTripScore(TeamSide side, int? tripScore) =>
        Wait.Until(driver =>
            {
                var tripScoreButton = driver.FindElement(By.Id($"ScoreboardControl.TripScore.{side}.{tripScore?.ToString() ?? "-"}"));
                return (tripScoreButton is { Displayed: true, Enabled: true }, tripScoreButton);
            },
            tripScoreButton => tripScoreButton.Click());

    public void SetTimeoutType(TimeoutType type, TeamSide? team) =>
        Wait.Until(driver =>
            {
                var buttonId = (type, team) switch
                {
                    (TimeoutType.Official, _) => "ScoreboardControl.TimeoutTypePanel.Official",
                    (TimeoutType.Team, TeamSide.Home) => "ScoreboardControl.TimeoutTypePanel.HomeTeamTimeout",
                    (TimeoutType.Team, TeamSide.Away) => "ScoreboardControl.TimeoutTypePanel.AwayTeamTimeout",
                    (TimeoutType.Review, TeamSide.Home) => "ScoreboardControl.TimeoutTypePanel.HomeTeamReview",
                    (TimeoutType.Review, TeamSide.Away) => "ScoreboardControl.TimeoutTypePanel.AwayTeamReview",
                    _ => throw new ArgumentException()
                };

                var timeoutButton = driver.FindElement(By.Id(buttonId));

                return (timeoutButton is { Displayed: true, Enabled: true }, timeoutButton);
            },
            timeoutButton => timeoutButton.Click());
}