using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public GameState currentState;

    void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.MainMenu:
                // Initialize main menu
                break;
            case GameState.Playing:
                // Start gameplay
                break;
            case GameState.Paused:
                // Pause the game
                break;
            case GameState.GameOver:
                // Show game over screen
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}