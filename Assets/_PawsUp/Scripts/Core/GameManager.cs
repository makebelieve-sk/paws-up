using System;
using UnityEngine;

namespace PawsUp.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Dialogue,
        Cutscene,
        Investigation, // smell sense active
        Inventory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameState _currentState = GameState.Playing;
        public GameState CurrentState => _currentState;

        public event Action<GameState, GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            var oldState = _currentState;
            _currentState = newState;
            OnGameStateChanged?.Invoke(oldState, newState);
        }

        public bool IsPlaying => _currentState == GameState.Playing;
        public bool IsInDialogue => _currentState == GameState.Dialogue;
        public bool IsPaused => _currentState == GameState.Paused;
    }
}
