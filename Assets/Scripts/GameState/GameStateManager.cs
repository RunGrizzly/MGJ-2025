using System;
using Events;
using SGS29.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameStateManager : MonoSingleton<GameStateManager>
{
    private InputSystem_Actions _actions;
    private EventManager _eventManager;
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public GameState currentState;

    public void Start()
    {
        _eventManager = SM.Instance<EventManager>();
        _eventManager.RegisterListener<GameOver>(_ =>
        {
            if (currentState == GameState.Playing)
            {
                ChangeState();
            }
        });
        
        _actions = new InputSystem_Actions();
        _actions.Ship.Transfer.Enable();
        _actions.Ship.Transfer.performed += OnAction;
        
        currentState = GameState.MainMenu;
    }
    
    private void ChangeState()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                currentState = GameState.Playing;
                _eventManager.DispatchEvent(new GameStarted());
                _actions.Ship.Transfer.performed -= OnAction;
                break;
            case GameState.Playing:
                currentState = GameState.GameOver;
                break;
            case GameState.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnAction(InputAction.CallbackContext _)
    {
        ChangeState();
    }
}