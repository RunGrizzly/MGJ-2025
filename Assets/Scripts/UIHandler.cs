using System.Collections.Generic;
using Events;
using Gameplay;
using SGS29.Utilities;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public BeatPrompt BeatPromptTemplate;

    public List<BeatPrompt> BeatPrompts = new List<BeatPrompt>();


    private void OnEnable()
    {
        SM.Instance<EventManager>().RegisterListener<NewLevel>(OnNewLevel);
        SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
    }

    private void OnDisable()
    {
        SM.Instance<EventManager>().UnregisterListener<NewLevel>(OnNewLevel);
        SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
    }

    private void OnNewLevel(NewLevel context)
    {
        foreach (var (beat, normalizedPosition) in context.Level.Track.GetNormalizedBeatTimes())
        {
            if (beat.Action != BeatAction.Empty)
            {
                var beatPromptInstance = Instantiate(BeatPromptTemplate, null);
                beatPromptInstance.Beat = beat;

                beatPromptInstance.transform.position =
                    OrbitHelpers.OrbitPointFromNormalisedPosition(context.Level.World.Orbit, normalizedPosition);
                BeatPrompts.Add(beatPromptInstance);
            }
        }
    }

    private void OnBeatAttempt(BeatAttemptEvent context)
    {
        foreach (var beatPrompt in BeatPrompts)
        {
            if (beatPrompt.Beat == context.Beat)
            {
                if (context.Beat.State == Gameplay.Beat.States.Success)
                {
                    LeanTween.value(1f, 1.2f, 0.45f)
                        .setOnUpdate((val) => beatPrompt.transform.localScale = Vector3.one * val)
                        .setOnComplete(() => beatPrompt.transform.localScale = Vector3.one);
                }

                else if (context.Beat.State == Gameplay.Beat.States.Failed)
                {
                }

                else if (context.Beat.State == Gameplay.Beat.States.Missed)
                {
                }

                return;
            }
        }
    }

    private void Start()
    {
        // foreach (var beat in SpawnOnOrbit.Beats)
        // {
        // var beatPromptInstance =  Instantiate(BeatPromptTemplate, null);
        // beatPromptInstance.Beat = beat;
        //
        // Orbit targetOrbit = new Orbit()
        //
        // beatPromptInstance.transform.position =OrbitHelpers.OrbitPointFromNormalisedPosition(  OrbitManager.MainOrbit,beat.Position);
        //
        // BeatPrompts.Add(beatPromptInstance);
        // }
    }

    // private void OnDisable()
    // {
    //   foreach (var beatPrompt in BeatPrompts)
    //   {
    //     if (beatPrompt.gameObject != null)
    //     {
    //       DestroyImmediate(beatPrompt.gameObject);
    //     }
    //   }
    //   
    //   BeatPrompts.Clear();
    // }
}