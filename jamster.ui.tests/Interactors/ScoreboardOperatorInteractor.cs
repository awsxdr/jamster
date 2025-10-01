using jamster.engine.Domain;

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

    public void ClickStop() =>
        Wait.Until(driver =>
            {
                var endButton = driver.FindElement(By.Id("ScoreboardControl.MainControls.StopButton"));
                return (endButton is { Displayed: true, Enabled: true }, endButton);
            },
            endButton => endButton.Click());

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
}