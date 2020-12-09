public class GameState
{
    private bool isAlive = false;
    private int coins = 0;

    private State currentState = State.Start;
    private Scene currentScene = Scene.MainMenu;

    public bool IsAlive { get { return isAlive; } set { isAlive = value; } }
    public int Coins { get { return coins; } set { coins = value; } }
    public State CurrentState { get { return currentState; } set { currentState = value; } }
    public Scene CurrentScene { get { return currentScene; } set { currentScene = value; } }

    public enum State
    {
        Start,
        OnPlay,
        Paused,
        GameOver,
    }
    public enum Scene
    {
        MainMenu = 0,
        Game = 1,
    }
}