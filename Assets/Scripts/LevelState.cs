public class LevelState
{
    private bool isAlive = true;

    private bool isRedMagnetActive = false;
    private bool isGreenHighJumpActive = false;
    private bool isBlueDoubleScoreActive = false;
    private bool isDoubleTapActive = false;

    private int coins = 0;
    private float score = 0;

    public bool IsAlive { get { return isAlive; } set { isAlive = value; } }

    public bool IsRedMagnetActive { get { return isRedMagnetActive; } set { isRedMagnetActive = value; } }
    public bool IsGreenHighJumpActive { get { return isGreenHighJumpActive; } set { isGreenHighJumpActive = value; } }
    public bool IsBlueDoubleScoreActive { get { return isBlueDoubleScoreActive; } set { isBlueDoubleScoreActive = value; } }
    public bool IsDoubleTapActive { get { return isDoubleTapActive; } set { isDoubleTapActive = value; } }

    public int Coins { get { return coins; } set { coins = value; } }
    public float Score { get { return score; } set { score = value; } }
}