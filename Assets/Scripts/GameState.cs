public class GameState
{
    private bool isAlive = false;
    private bool isRedMagnetActive = false;
    private bool isGreenHighJumpActive = false;
    private bool isBlueDoubleScoreActive = false;
    private bool isDoubleTapActive = false;

    private int coins = 0;
    private int score = 0;

    private State currentState = State.Start;
    private Scene currentScene = Scene.MainMenu;

    public bool IsAlive { get { return isAlive; } set { isAlive = value; } }
    public bool IsRedMagnetActive { get { return isRedMagnetActive; } set { isRedMagnetActive = value; } }
    public bool IsGreenHighJumpActive { get { return isGreenHighJumpActive; } set { isGreenHighJumpActive = value; } }
    public bool IsBlueDoubleScoreActive { get { return isBlueDoubleScoreActive; } set { isBlueDoubleScoreActive = value; } }
    public bool IsDoubleTapActive { get { return isDoubleTapActive; } set { isDoubleTapActive = value; } }
    public int Coins { get { return coins; } set { coins = value; } }
    public int Score { get { return score; } set { score = value; } }
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