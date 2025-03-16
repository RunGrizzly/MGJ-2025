using System;
using System.Collections.Generic;
using Events;
using Gameplay;
using SGS29.Utilities;
using TrackEvents;
using UnityEngine;

public enum GameState
{
    InHangar,
    Transitioning,
    OnTrack,
    OnExitTrack,
    Dead
}

public class GameManager : MonoBehaviour
{
    public static GameManager ins = null;

    public TrackPlayer _trackPlayer;

    //Our list of definitions that will be turned into tracks
    public List<TrackDefinition> TrackDefinitions = new List<TrackDefinition>();

    public InputSystem_Actions _actions;
    
    public EventManager _eventManager;
    public UIHandler UIHandler;
    public TrackGenerator TrackGenerator;
    
    public GameState CurrentGameState = GameState.InHangar;

    [SerializeField] private GameObject _shipTemplate;

    public Transform _playerShip = null;

    public RunStager RunStagerTemplate = null;
    private RunStager _activeRunStager = null;
    public List<Run> Runs = new List<Run>();
    
    private void Awake()
    {
        ins = this;
    }
    
    public void OnEnable()
    {
        //Events
        _eventManager = SM.Instance<EventManager>();
        _eventManager.RegisterListener<RunEnded>( OnRunEnded);
        _eventManager.RegisterListener<RunStarted>(OnRunStarted);
        //_eventManager.RegisterListener<TrackPassed>(OnTrackPassed);
     
        //Inputs
        _actions = new InputSystem_Actions();
        
        //Here we will spawn a new run stager
        StageNewRun();
    }

    // private void OnTrackPassed(TrackPassed context)
    // {
    //     // Runs[0].TracksComplete += 1;
    //     // Runs[0].Difficulty += Runs[0].DifficultyRamp;
    //     // _eventManager.DispatchEvent(new RunUpdate(Runs[0]));
    // }

    private void StageNewRun()
    {
        if (_activeRunStager != null)
        {
            Destroy(_activeRunStager.gameObject);
        }
        
        //Spawn a new run stager into the HUD canvas
        //This will do all the transient prep work for staging a new run
        _activeRunStager = Instantiate(RunStagerTemplate,UIHandler.HUDCanvas.transform);
    }
    
    private void OnRunStarted(RunStarted context)
    {
        Runs.Insert(0,context.Run);
    }
    
    private void OnRunEnded(RunEnded context)
    {
        //Maybe we want to go to a summary screen or something?
       StageNewRun();
    }
}

    [Serializable]
    public struct Range
    {
        public float Min;
        public float Max;

        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }