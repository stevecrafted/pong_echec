namespace pong_shared.Models
{
    public enum GameStateType
    {
        WaitingForPlayers,
        InProgress,
        GameOver
    }

    public class GameState
    {
        public GameStateType StateType { get; set; }
    }
}
