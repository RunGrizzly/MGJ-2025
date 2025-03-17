#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using SGS29.Utilities;
using TrackEvents;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Gameplay
{
    public class TrackPlayer : MonoBehaviour
    {
        public PlayableTrack ActiveTrack = null;
        
        //Always have the next track prepared
        //public PlayableTrack NextTrack { get; private set; }
        
        private InputSystem_Actions _actions;
        
        public float _progress;//Normalised progress
        
        //The currently playing track
        private Coroutine currentlyPlaying = null;
        
        public float speedchangefactor = 1;
        
        [SerializeField] private PlayerShipFramer _playerShip = null;

        public PlayableTrack ExitTrack = null;
        
        private void OnEnable()
        {
            _actions = new InputSystem_Actions();
            _actions.Ship.Action1.performed += _ => OnAction(BeatAction.Action1);
            _actions.Ship.Action2.performed += _ => OnAction(BeatAction.Action2);
            _actions.Ship.Action3.performed += _ => OnAction(BeatAction.Action3);
            _actions.Ship.Action4.performed += _ => OnAction(BeatAction.Action4);
            _actions.Ship.Progress.performed += _ => OnAction(BeatAction.Transfer);
            
            //_actions.Ship.Enable();
            
            // SM.Instance<EventManager>().RegisterListener<RunEnded>(OnRunEnded);
        }

        private void OnRunEnded(RunEnded context)
        {
            //End the track coroutine
            ActiveTrack = null;
            
            _actions.Ship.Disable();
            
            //Stop listening to inputs
            SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
            SM.Instance<EventManager>().UnregisterListener<RunEnded>(OnRunEnded);
        }

        private void OnDrawGizmos()
        {
            if (ActiveTrack == null || ActiveTrack.World == null || ActiveTrack.World.Orbit ==null)
            {
                return;
            }

            Orbit newOrbit = ActiveTrack.World.Orbit;
            
            var normalisedBeatTimes = ActiveTrack.NormalisedBeatTimes();
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, 0f), 5f);

            var sphereSize = 20f;
            
             for (int i = 0; i < ActiveTrack.Beats.Count; i++)
             {
                 var beat = ActiveTrack.Beats[i];
                 
                 if (beat.Action == BeatAction.Empty)
                 {
                     Gizmos.color = Color.black;
                     sphereSize = 0.5f;
                 }
            
                 else if (beat.Action == BeatAction.Transfer)
                 {
                     Gizmos.color = Color.cyan;
                     sphereSize = 1f;
                 }
            
                 else
                 {
                     if (beat.State == Beat.States.Upcoming)
                     {
                         sphereSize = 1f;
                         Gizmos.color = Color.gray;
                         Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, normalisedBeatTimes.ElementAt(i).Value), sphereSize);
                         
                         sphereSize = 0.5f;
                         Gizmos.color = Color.blue;
                         Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, normalisedBeatTimes.ElementAt(i).Value-ActiveTrack.ActiveDefinition.Buffer), sphereSize);
                         Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, normalisedBeatTimes.ElementAt(i).Value+ActiveTrack.ActiveDefinition.Buffer), sphereSize);
                     }
                     
                     else if (beat.State == Beat.States.Failed || beat.State == Beat.States.Missed)
                     {
                         sphereSize = 1f;
                         Gizmos.color = Color.yellow;
                         Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, normalisedBeatTimes.ElementAt(i).Value), sphereSize);
                     }
                     else
                     {
                         sphereSize = 1f;
                         Gizmos.color = Color.green; 
                         Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, normalisedBeatTimes.ElementAt(i).Value), sphereSize);
                     }
                     
                 }
                 
                 
                 
                 Gizmos.color = Color.red;
            
                 var currentBeat =ActiveTrack.GetCurrentBeat(_progress);
                 
                 if (currentBeat != null)
                 {
                     Gizmos.DrawWireSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, currentBeat.NormalisedTime), 1.5f);
                 }
            
                 Gizmos.color = Color.white;
            
                 Gizmos.DrawWireSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, _progress), 1.5f);      
            }
        }
        
        private IEnumerator PlayTrack(float startTime)
        {
            //Active track now listens
            ActiveTrack.Init();
            
            yield return new WaitWhile(()=>ActiveTrack.Beats.Count == 0);
            
            //Player listens to inputs
            // _actions.Ship.Enable();
            // SM.Instance<EventManager>().RegisterListener<RunEnded>(OnRunEnded);
          
            //A cache so we can failsafe and know if the current track changed
            PlayableTrack trackInfo = ActiveTrack;
            
            _progress = startTime;
            
            //Set bps mod to dead duration bps
            var bpsmod = (1-ActiveTrack.ActiveDefinition.ActiveBeatRange)/ActiveTrack.ActiveDefinition.DeadDuration;

            float deadTime = 0;
            float activeTime = 0;
            
            var currentBeat = ActiveTrack.GetCurrentBeat(_progress);
            
            //Play main loop
            while (ActiveTrack == trackInfo)
            { 
                string i = currentBeat == null ? "is" : "is not";
                Debug.LogFormat($"Current beat {i} null");

              
                //We check if we missed the current beat before updating
                //We also do NOT want to consider missed when the track is not valid
                if (_progress > (currentBeat.NormalisedTime + (ActiveTrack.ActiveDefinition.Buffer/2)) && ActiveTrack.IsValid)
                {
                    //We passed the current beat this frame
                    if (currentBeat.State != Beat.States.Success)
                    {
                     //Set the state of the beat
                     currentBeat.State = Beat.States.Missed;
                
                     //Pass along a beat attempt event
                     var beatEvent = new BeatAttemptEvent(currentBeat);
                     SM.Instance<EventManager>().DispatchEvent(beatEvent);
                     
                     Debug.LogFormat($"Track player detected a missed beat");
                    }
                }
                
                currentBeat = ActiveTrack.GetCurrentBeat(_progress);
                
                //A zone buffer is removed for prepare time
                if (_progress >= 1)
                {
                    Debug.LogFormat($"WE ARE OVER NORMALISED LIMIT");
                    //Finished a revolution
                    SM.Instance<EventManager>().DispatchEvent(new TrackResetEvent(ActiveTrack));
                    _progress = 0;
                }
                
                GameManager.ins._playerShip.position = OrbitHelpers.OrbitPointFromNormalisedPosition(ActiveTrack.World.Orbit, _progress);
                GameManager.ins._playerShip.rotation = Quaternion.Slerp(GameManager.ins._playerShip.rotation,OrbitHelpers.ForwardRotationFromNormalisePosition(ActiveTrack.World.Orbit, _progress ),Time.deltaTime*speedchangefactor);
             
                if (ActiveTrack.InActiveSpace(_progress,0.05f))
                {
                    //bpsmod = Mathf.Lerp(bpsmod, ActiveTrack.Definition.ActiveBeatRange/ActiveTrack.Definition.ActiveDuration,Time.deltaTime*speedchangefactor);
                    bpsmod = ActiveTrack.ActiveDefinition.ActiveBeatRange / ActiveTrack.ActiveDefinition.ActiveDuration;
                    
                    activeTime += Time.deltaTime;
                    deadTime = 0;
                    
                    //Debug.LogFormat($"Active time = {activeTime}");
                }
                
                else
                {
                    //bpsmod = Mathf.Lerp(bpsmod, (1-ActiveTrack.Definition.ActiveBeatRange)/ActiveTrack.Definition.DeadDuration,Time.deltaTime*speedchangefactor);
                    bpsmod =  (1-ActiveTrack.ActiveDefinition.ActiveBeatRange)/ActiveTrack.ActiveDefinition.DeadDuration;
                    
                    deadTime += Time.deltaTime;
                    activeTime = 0;
                    
                    //Debug.LogFormat($"Dead time = {deadTime}");
                }

                _progress += (bpsmod *Time.deltaTime);
                
                yield return new WaitForFixedUpdate();
            }
        }
        
        //Start playing a new track
        public void Play(PlayableTrack track, float startTime = 0)
        {
            ActiveTrack = track;
            
           //  //Make new exit tracks
           //  var randomTrackDefinition = GameManager.ins.TrackGenerator.GetRandomTrackDefinition( GameManager.ins.Runs[0].DifficultyRamp);
           //    
           //  WorldParams worldParams = new WorldParams(10, Vector3.zero, 1f);
           //  PlayableTrack exitTrack = new PlayableTrack(randomTrackDefinition, worldParams, track.Index+1);
           //  
           //  ExitTracks =
           //  
           currentlyPlaying = StartCoroutine(PlayTrack(startTime));
           
           _actions.Ship.Enable();
           SM.Instance<EventManager>().RegisterListener<RunEnded>(OnRunEnded);
           SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
           
           SM.Instance<EventManager>().DispatchEvent(new TrackStarted(ActiveTrack));
           
        
            //This should respond
            // _actions.Ship.Enable();
        }
        
        private void OnBeatAttempt(BeatAttemptEvent context)
        {
            if (context.Beat.State == Beat.States.Failed || context.Beat.State == Beat.States.Missed )
            {
                Debug.LogFormat($"Dispatching a beat attempt on a missed beat");
                SM.Instance<EventManager>().DispatchEvent(new TrackFailed(ActiveTrack));
            }
            else if(context.Beat.Action == BeatAction.Transfer)
            {
               //Do transfer
               Debug.LogFormat($"We should clean up and do a transfer now");
               
               //Active track stops listening
               SM.Instance<EventManager>().UnregisterListener<RunEnded>(OnRunEnded);
               SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
               
               
               ActiveTrack.Kill();
               PlayExitTrack();
               
            }
            else if(context.Beat.Action == BeatAction.Empty)
            {
               //Somehow you were allowed to attempt an action on an empty beat.
               //Since "current beat" does not take into account empty beats
               //You should never get this far
            }
            else
            {
                //Only actions left
                if (CheckWinCondition())
                {
                    Debug.LogFormat("WE WON");
                    
                    //We have went through every beat and they are all passed
                    SM.Instance<EventManager>().DispatchEvent(new TrackPassed(ActiveTrack));   
                }
            }
        }
        
        private void OnAction(BeatAction action)
        {
            //The track is in a state of invalidity
            //EG - We are recovering after a fail
            //Track reset will flip this back on
            if (!ActiveTrack.IsValid)
            {
                return;
            }

            var currentBeat = ActiveTrack.GetCurrentBeat(_progress);
            
            //If there is no beat to be hit
            if (currentBeat == null)
            {
                //This indicates a problem
                //Since "current" beat always has a fallback
                return;
            }
            
            //Check the context action against the expected current beat action
            
            //If the correct action was pressed
            if (action == currentBeat.Action)
            {
                //Get distance
                var distance =Mathf.Abs(currentBeat.NormalisedTime - _progress);
                
                //The correct action was pressed within the timing buffer
                if (distance < ActiveTrack.ActiveDefinition.Buffer)
                {
                    //Assess timing here
                    //
                    //Depending on the distance from the buffer we can delineate "good" vs "bad" timings
                    //
                    
                    currentBeat.SetState(Beat.States.Success);
                    SM.Instance<EventManager>().DispatchEvent(new BeatAttemptEvent(currentBeat));
                }
                
                //The correct action was pressed but  outside the timing buffer
                else
                {
                    currentBeat.SetState(Beat.States.Missed);
                    SM.Instance<EventManager>().DispatchEvent(new BeatAttemptEvent(currentBeat));
                }
            }
            
            //The wrong action was pressed
            else
            {
                currentBeat.SetState(Beat.States.Failed);
                SM.Instance<EventManager>().DispatchEvent(new BeatAttemptEvent(currentBeat));
            }
        }
        
        private bool CheckWinCondition()
        {
            //Update the current track state
            for ( int i = 0; i < ActiveTrack.Beats.Count; i++)
            {
                var beat = ActiveTrack.Beats[i];
                
                if (beat.Action != BeatAction.Empty) 
                {
                    if (beat.State == Beat.States.Success)
                    {
                        Debug.LogFormat($"Win condition: Beat number {i} is successful");
                        continue;
                    }
                    else
                    {
                        Debug.Log($"Win condition: Beat number {i} is unsuccessful");
                        return false;
                    }
                }
            }
            return true;
        }
        
        private void PlayExitTrack()
        {
            SM.Instance<EventManager>().UnregisterListener<RunEnded>(OnRunEnded);
            
            var currentOrbit = ActiveTrack.World.Orbit;
            //Get the next track from the run
            var nextOrbit = GameManager.ins.Runs[0].NextTrack.World.Orbit;
            
            //Create positions
            //The current player position
            var start = OrbitHelpers.OrbitPointFromNormalisedPosition(currentOrbit, _progress);
            
            //The new position at norm 0
            var end = OrbitHelpers.OrbitPointFromNormalisedPosition(nextOrbit, 0f);
            
            //Kill the current track coroutine
            ActiveTrack = null;
            
            //Some intermediate positions
            var inta = Vector3.Lerp(start, end, 0.25f);
            var intb = Vector3.Lerp(start, end, 0.5f);
            var intc = Vector3.Lerp(start, end, 0.75f);
            
            Vector3[] splinePos = new[] { start, inta, intb, intc, end };
            var ltspline = new LTSpline(splinePos, false);
            
            //Stop listening to inputs
           _actions.Ship.Disable();
            
            //Create a new spline
            //var spline = new Spline();
            
            var position = GameManager.ins._playerShip.transform.position;
            
            //Do a transfer
            LeanTween.moveSpline(GameManager.ins._playerShip.gameObject, ltspline, 4f)
            .setOnUpdate((float val) =>
            {
                //Cache new transforms
                var newPosition = GameManager.ins._playerShip.transform.position;
                var newForward = ( newPosition-position ).normalized;
                
                //Look at the calculated directional forward
                GameManager.ins._playerShip.rotation =Quaternion.Slerp(  GameManager.ins._playerShip.rotation, quaternion.LookRotation(newForward,Vector3.up),Time.deltaTime);
                
                //Update new position
                position = GameManager.ins._playerShip.transform.position;
            })
            .setOnComplete(()=>
            {
                //When the spline is complete call the transition end event
                SM.Instance<EventManager>().DispatchEvent(new TransitionEnded());
                // Play(newPlayableTrack ,startTime:0);
            });  
            
            //Call transition start
            SM.Instance<EventManager>().DispatchEvent(new TransitionStarted());
            
            // //Create two knots
            // var knot1 = new BezierKnot(pos1, 0f, 1000f);
            // var knot3 = new BezierKnot(pos3, 0f, 750f);
            //
            // //Add knots and shit to the spline
            // spline.Add(knot1, TangentMode.Mirrored);
            // spline.Add(pos2);
            // spline.Add(knot3, TangentMode.Mirrored);
            //
            //
            //
            //
            // //This was all on game manager
            // var splineAnimate = GameManager.ins.SplineAnimate;
            // var splineContainer = GameManager.ins.SplineContainer;
            //
            // GameManager.ins.SplineContainer.Spline = spline;
            // splineAnimate.Container = splineContainer;
            //
            // splineAnimate.Duration = 7.5f;
            // splineAnimate.Loop = SplineAnimate.LoopMode.Once;
            // splineAnimate.ElapsedTime = 0f;
            // splineAnimate.Play();
            // splineAnimate.Completed += OnTransitionEnded;
            //
            // //currentState = GameplayState.Transitioning;
            //
            // //_levels.Add(_levelGenerator.Generate(_levels.Count));
            //
            // //Something to do with the transition spline
            
       
        }
        

    //A transition ended
    private void OnTransitionEnded()
    {
        //CurrentTrack.SetState(PlayableTrack.States.NotPlaying);
        
        //Manually setting progress?
        _progress = 0;
        

        
        //SM.Instance<EventManager>().DispatchEvent(new TransitionEnded());
    }
    }
}