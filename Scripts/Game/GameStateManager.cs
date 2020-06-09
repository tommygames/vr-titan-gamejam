using System;
using UnityEngine;

public class GameStateManager : Bolt.EntityEventListener<IGameState>
{
    public static GameStateManager Instance
    {
        get; private set;
    }
    
    public System.Action OnGameStateChanged;
    
    public const int COUNTDOWN_TIME = 3;
    public const int GAME_DURATION = 120;
    public const int MIN_PLAYERS_TO_START = 1;
    public const long VFX_THROTTLE = 100;

    private long lastVFXTime;

    public enum GameState
    {
        Lobby = 0,
        Countdown,
        Running,
        GameOver
    };

    private GameState gameState;

    public override void Attached()
    {
        if (entity.isOwner)
        {
            state.State = (int)GameState.Lobby;
        }
        BoltManager.Instance.RegisterGameState(entity);
        state.AddCallback("State", StateChanged);
    }
    

    void Awake()
    {
        Instance = this;

        BoltManager.Instance.OnPlayerEntityRegistered += OnPlayerRegistered;

        lastVFXTime = 0;
    }

    void StateChanged()
    {
        gameState = (GameState)Enum.ToObject(typeof(GameState) , state.State);
        if (OnGameStateChanged != null)
        {
            OnGameStateChanged();
        }
    }
    
    public GameState GetGameState()
    {
        return gameState;
    }

    public int GetGameStateStartTime()
    {
        return state.StateStartTime;
    }

    public void SetGameState(GameState newState)
    {
        if (entity.IsOwner)
        {
            if (newState != gameState)
            {
                state.State = (int)newState;
                state.StateStartTime = GetNow();
            }
        }
    }

    private int GetStateDuration()
    {
        switch (gameState)
        {
            case GameState.Countdown:
            {
                return COUNTDOWN_TIME;
            }

            case GameState.Running:
            {
                return GAME_DURATION;
            }

            default:
            {
                return -1;
            }
        }
    }

    public bool DoesStateEnd()
    {
        return gameState == GameState.Countdown || gameState == GameState.Running;
    }
    

    public int GetStateEndTime()
    {
        var duration = GetStateDuration();
        if (duration > 0)
        {
            return state.StateStartTime + (duration);
        }
        else
        {
            return state.StateStartTime + 1000;
        }
    }

    public static int GetNow()
    {
        return (int)ToPosixTime(System.DateTime.UtcNow);
    }
    
    public static long GetNowMS()
    {
        return ToPosixTimeMS(System.DateTime.UtcNow);
    }
    
    private static System.DateTime _epoc = new System.DateTime(1970, 1, 1);
    public static long ToPosixTime( System.DateTime time )
    {
        var span = time - _epoc;
        return (long)span.TotalSeconds;
    }
    
    public static long ToPosixTimeMS( System.DateTime time )
    {
        var span = time - _epoc;
        return (long)span.TotalMilliseconds;
    }
    
    void Update()
    {
        if (BoltNetwork.IsServer)
        {
            var stateEndTime = GetStateEndTime();
            var now = GetNow();
            if (now > stateEndTime)
            {
                if (gameState == GameState.Countdown)
                {
                    SetGameState(GameState.Running);
                }
                else if (gameState == GameState.Running)
                {
                    SetGameState(GameState.GameOver);
                    var gameOverEvent = GameOverEvent.Create();
                    gameOverEvent.WinType = "TimeOut";
                    gameOverEvent.Send();
                }
            }
        }
    }

    void OnPlayerRegistered(BoltEntity player)
    {
        if (BoltManager.Instance.playerList.Count == MIN_PLAYERS_TO_START && gameState == GameState.Lobby)
        {
            SetGameState(GameState.Countdown);
        }
    }
    
    public bool CanPlayVFX()
    {
        var now = GetNowMS();
        if (now > (lastVFXTime + VFX_THROTTLE))
        {
            lastVFXTime = now;
            return true;
        }
        return false;
    }
}
