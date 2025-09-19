using jamster.DataStores;

namespace jamster.Services;

public interface IFirstRunConfigurator
{
    Task PerformFirstRunTasksIfRequired();
}

public class FirstRunConfigurator(IGameDiscoveryService gameDiscoveryService, ISystemStateStore systemStateStore) : IFirstRunConfigurator
{
    public async Task PerformFirstRunTasksIfRequired()
    {
        if (!await IsFirstRun())
            return;

        var newGame = await gameDiscoveryService.GetGame(new GameInfo(Guid.NewGuid(), "Black vs White"));

        await systemStateStore.SetCurrentGame(newGame.Id);
    }

    private async Task<bool> IsFirstRun() =>
        !(await gameDiscoveryService.GetGames()).Any();
}