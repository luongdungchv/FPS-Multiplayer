public interface IPlayersHaveState
{
    void RevertAllPlayerStates(int tickCount);
    void RestoreAllPlayerStates();
}