using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private GameDataSO _gameData;
    [SerializeField] private GameState _currentGameState;
    private NetworkVariable<int> _gameTimer = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _gameTimer.Value = _gameData.GameTimer;
            SetTimerTextRpc();
            InvokeRepeating(nameof(DecreaseTimer), 1f, 1f);
        }

        _gameTimer.OnValueChanged += OnTimerChanged;
    }

    private void OnTimerChanged(int previousValue, int newValue)
    {
        TimerUI.Instance.SetTimerUI(newValue);

        if(IsServer && newValue <= 0)
        {
            ChangeGameState(GameState.GameOver);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetTimerTextRpc()
    {
        TimerUI.Instance.SetTimerUI(_gameTimer.Value);
    }

    private void DecreaseTimer()
    {
        if (IsServer && _currentGameState == GameState.Playing)
        {
            _gameTimer.Value--;

            if (_gameTimer.Value <= 0)
            {
                CancelInvoke(nameof(DecreaseTimer));
            }
        }
    }

    public void ChangeGameState(GameState newGameState)
    {
        if (!IsServer) return;
        ChangeGameStateRpc(newGameState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeGameStateRpc(GameState newGameState)
    {
        _currentGameState = newGameState;
        OnGameStateChanged?.Invoke(newGameState);
        Debug.Log($"Game State: {newGameState}");
    }

    public GameState GetGameState()
    {
        return _currentGameState;
    }

}
