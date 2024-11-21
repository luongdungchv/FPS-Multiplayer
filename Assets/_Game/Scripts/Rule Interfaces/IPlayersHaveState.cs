public interface IPlayersHaveState
{
    void RevertAllPlayerStates(int tickCount, NetworkFPSPlayer excluded);
    void RestoreAllPlayerStates();
}