using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using Gameplay;
using SGS29.Utilities;
using TrackEvents;
using UnityEngine;
using UnityEngine.InputSystem;

public class RunUpdate : IEvent
{
    public Run Run { get; }

    public RunUpdate(Run run)
    {
        Run = run;
    }
}

[Serializable]
public class Run
{
    public float Difficulty;
    public float DifficultyRamp;
    
    public int MaxAttempts =10;
    public int RemainingAttempts;

    public List<PlayableTrack> Tracks = new List<PlayableTrack>();
    public List<World> Worlds = new List<World>();

    public int TracksPassed => Tracks.Count(x => x.Passed);
    
    public PlayableTrack NextTrack => Tracks[Tracks.Count - 1];

    public Run(float difficulty, float difficultyRamp)
    {
        Difficulty = difficulty;
        DifficultyRamp = difficultyRamp;
        RemainingAttempts = MaxAttempts;
    }

    public void Start()
    {
        SM.Instance<EventManager>().RegisterListener<TrackFailed>(OnTrackFailed);
        SM.Instance<EventManager>().RegisterListener<TrackPassed>(OnTrackPassed);
        SM.Instance<EventManager>().RegisterListener<TransitionEnded>(OnTransitionEnded);
    }
    
    public void End()
    {
        SM.Instance<EventManager>().UnregisterListener<TrackFailed>(OnTrackFailed);
        SM.Instance<EventManager>().UnregisterListener<TrackPassed>(OnTrackPassed);
        SM.Instance<EventManager>().UnregisterListener<TransitionEnded>(OnTransitionEnded);
        
        SM.Instance<EventManager>().DispatchEvent(new RunEnded());
    }
    
    private void OnTrackPassed(TrackPassed context)
    {
        Difficulty += DifficultyRamp;
        SM.Instance<EventManager>().DispatchEvent(new RunUpdate(this));
        
        
        //We just passed a track
        //So the next track is tracks complete +1
        
        //Add a new world for exiting
        //As long as we add a new world every time a track is passed
        //We will always be one world ahead
        //(We started with an extra world
        WorldParams worldParams = new WorldParams(10, Vector3.zero, 1f);
        Worlds.Add(WorldGenerator.ins.GenerateWorld(worldParams, Worlds.Count));
        
        //Create a new track
        //Get a random playable track
        var randomTrackDefinition = GameManager.ins.TrackGenerator.GetRandomTrackDefinition(Difficulty);
        
        //Set up the first playable track
        //The world gets assigned here with the same index
        Tracks.Add(new PlayableTrack(randomTrackDefinition,world: Worlds[Tracks.Count()], index:Tracks.Count()));
       
        // //Play it on the track player
        // //We play the last added track
        // GameManager.ins._trackPlayer.Play(Tracks[Tracks.Count-1]);
    }

    private void OnTransitionEnded(TransitionEnded context)
    {
        //We play the last added track
        //We know that this is the last track we added - since we added it OnTrackPassed (the prelude to the transition)
        GameManager.ins._trackPlayer.Play(Tracks[Tracks.Count-1]);
    }
    
    private void OnTrackFailed(TrackFailed context)
    {
        RemainingAttempts--;
        SM.Instance<EventManager>().DispatchEvent(new RunUpdate(this));
        
        Debug.Log($"Attempt failed, Remaining attempts: {RemainingAttempts}");
        
        if (RemainingAttempts == 0)
        {
            End();
        }
    }
}

public class RunStager : MonoBehaviour
{
  [SerializeField] private PlayerShipFramer m_shipTemplate = null;
    
    private void OnEnable()
    {
        //Inputs
        var actions = GameManager.ins._actions;

        actions.Ship.Progress.Enable();

        //Subscribe to inputs
        actions.Ship.Progress.performed += OnProgressPerformed;
        
        //Player
        if (GameManager.ins._playerShip != null)
        {
            Destroy(GameManager.ins._playerShip.gameObject);
        }
        
        GameManager.ins._playerShip = Instantiate(m_shipTemplate).transform;
        
        
        //The new run stager consitutes a main menu event
        SM.Instance<EventManager>().DispatchEvent(new RunStaged());
    }

    private void OnProgressPerformed(InputAction.CallbackContext context)
    {
        //We could generate runs here
        StartRun();
    }

    private void OnDisable()
   {
       var actions = GameManager.ins._actions;
       
       //Unsubscribe to inputs
       actions.Ship.Progress.performed -= OnProgressPerformed;
   }
    
   private void StartRun()
   {
       //Create some world params
       //Should get these randomly
 
       
       //Base difficulty
       //Difficulty ramp
       //Difficulty will increase by difficulty ramp every track cleared
       //Difficutly is normalised
       //So a ramp of 0.01 will take 100 runs to get to full difficulty
       Run newRun = new Run(0,0.01f);
 
       //Prepare two worlds
       //These will be index 0 and 1
       //The index is required for positional purposes
       //We should be able to defer this though
       WorldParams worldParams = new WorldParams(10, Vector3.zero, 1f);   
       newRun.Worlds.Add(WorldGenerator.ins.GenerateWorld(worldParams,newRun.Worlds.Count()));
       
       worldParams = new WorldParams(10, Vector3.zero, 1f);
       newRun.Worlds.Add(WorldGenerator.ins.GenerateWorld(worldParams,newRun.Worlds.Count()));
       
       //Get a random playable track
       var randomTrackDefinition = GameManager.ins.TrackGenerator.GetRandomTrackDefinition(newRun.Difficulty);
        
       //Set up the first playable track
       //The world gets assigned here with the same index
       newRun.Tracks.Add(new PlayableTrack(randomTrackDefinition,world: newRun.Worlds[newRun.Tracks.Count()], index:newRun.Tracks.Count()));
       
       //Play it on the track player
       GameManager.ins._trackPlayer.Play(newRun.Tracks[newRun.Tracks.Count-1]);
       
       newRun.Start();
       
       //Broadcast the new run that started
       GameManager.ins._eventManager.DispatchEvent(new RunStarted(newRun));
       
       //Staging done
       //Destroy
       Destroy(gameObject);
   }
}
