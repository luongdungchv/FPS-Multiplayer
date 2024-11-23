public interface IPlayersHaveState
{
    void RevertAllPlayerStates(int tickCount, NetworkFPSPlayer excludedPlayer);
    void RestoreAllPlayerStates();
}