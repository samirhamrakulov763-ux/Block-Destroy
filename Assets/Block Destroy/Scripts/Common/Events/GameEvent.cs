using UnityEngine;

// Base class for all game events
public abstract class GameEvent { }

// Score related events
public class ScoreChangedEvent : GameEvent
{
    public int NewScore { get; }
    public int Delta { get; }

    public ScoreChangedEvent(int newScore, int delta)
    {
        NewScore = newScore;
        Delta = delta;
    }
}

// Turn related events
public class TurnChangedEvent : GameEvent
{
    public int TurnNumber { get; }

    public TurnChangedEvent(int turnNumber)
    {
        TurnNumber = turnNumber;
    }
}

// Game state events
public class GameStartEvent : GameEvent { }

public class GameOverEvent : GameEvent
{
    public int FinalScore { get; }
    public int FinalTurn { get; }

    public GameOverEvent(int finalScore, int finalTurn)
    {
        FinalScore = finalScore;
        FinalTurn = finalTurn;
    }
}

// Player action events
public class BallShotEvent : GameEvent
{
    public int BallCount { get; }

    public BallShotEvent(int ballCount)
    {
        BallCount = ballCount;
    }
}

public class BallReturnedEvent : GameEvent
{
    public int RemainingBalls { get; }

    public BallReturnedEvent(int remainingBalls)
    {
        RemainingBalls = remainingBalls;
    }
}

// Block related events
public class BlockDestroyedEvent : GameEvent
{
    public int BlockValue { get; }
    public Vector3 Position { get; }

    public BlockDestroyedEvent(int blockValue, Vector3 position)
    {
        BlockValue = blockValue;
        Position = position;
    }
}

public class BlockHitEvent : GameEvent
{
    public int RemainingHP { get; }
    public Vector3 Position { get; }

    public BlockHitEvent(int remainingHP, Vector3 position)
    {
        RemainingHP = remainingHP;
        Position = position;
    }
}

// Combo and bonus events
public class ComboEvent : GameEvent
{
    public int ComboCount { get; }

    public ComboEvent(int comboCount)
    {
        ComboCount = comboCount;
    }
}

public class AllClearEvent : GameEvent { }

public class LuckyBonusEvent : GameEvent { }

// Scene management events
public class SceneLoadRequestEvent : GameEvent
{
    public string SceneName { get; }
    public bool IsRefresh { get; }

    public SceneLoadRequestEvent(string sceneName, bool isRefresh = false)
    {
        SceneName = sceneName;
        IsRefresh = isRefresh;
    }
}

public class SceneLoadedEvent : GameEvent
{
    public string SceneName { get; }

    public SceneLoadedEvent(string sceneName)
    {
        SceneName = sceneName;
    }
}

// Currency events
public class CoinChangedEvent : GameEvent
{
    public int NewAmount { get; }
    public int Delta { get; }

    public CoinChangedEvent(int newAmount, int delta)
    {
        NewAmount = newAmount;
        Delta = delta;
    }
}

public class GemChangedEvent : GameEvent
{
    public int NewAmount { get; }
    public int Delta { get; }

    public GemChangedEvent(int newAmount, int delta)
    {
        NewAmount = newAmount;
        Delta = delta;
    }
}

// UI events
public class PanelOpenedEvent : GameEvent
{
    public string PanelName { get; }

    public PanelOpenedEvent(string panelName)
    {
        PanelName = panelName;
    }
}

public class PanelClosedEvent : GameEvent
{
    public string PanelName { get; }

    public PanelClosedEvent(string panelName)
    {
        PanelName = panelName;
    }
}

public class PopupOpenedEvent : GameEvent
{
    public string PopupName { get; }

    public PopupOpenedEvent(string popupName)
    {
        PopupName = popupName;
    }
}

public class PopupClosedEvent : GameEvent
{
    public string PopupName { get; }

    public PopupClosedEvent(string popupName)
    {
        PopupName = popupName;
    }
}

// Sound events
public class PlaySoundEffectEvent : GameEvent
{
    public string SoundName { get; }

    public PlaySoundEffectEvent(string soundName)
    {
        SoundName = soundName;
    }
}

public class PlayBGMEvent : GameEvent
{
    public string BGMName { get; }

    public PlayBGMEvent(string bgmName)
    {
        BGMName = bgmName;
    }
}

// Rocket/Power-up events
public class RocketReadyEvent : GameEvent { }

public class RocketUsedEvent : GameEvent { }
