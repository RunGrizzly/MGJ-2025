using System;
using System.Collections.Generic;
using Events;
using Gameplay;
using SGS29.Utilities;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using TrackEvents;
using UnityEngine.UI;

[System.Serializable]
public class BeatActionSpriteDictionary : SerializableDictionaryBase<BeatAction, Sprite>
{
  
}


[System.Serializable]
public class BeatStateGameObjectDictionary : SerializableDictionaryBase<Beat.States, GameObject>
{
  
}

public class UIHandler : MonoBehaviour
{
  public CanvasGroup HUDCanvas = null;
  
  public CanvasGroup GameOverSplash = null;
  private CanvasGroup m_gameOverSplash = null;

  
  public CanvasGroup TransitionSplash = null;
  private CanvasGroup m_transitionSplash = null;

  public CanvasGroup MainMenuSplash = null;
  private CanvasGroup m_mainMenuSplash = null;
  
  public CanvasGroup LaunchPrepareSplash = null;
  private CanvasGroup m_launchPrepareSplash = null;
  
  public BeatPrompt BeatPromptTemplate;
  public List<BeatPrompt> BeatPrompts = new List<BeatPrompt>();

  public BeatActionSpriteDictionary ActionSprites = new BeatActionSpriteDictionary();

  public BeatStateGameObjectDictionary AttemptSplashes = new BeatStateGameObjectDictionary();

  public Image AttemptPipTemplate = null;
  public List<Image> AttemptPips = new List<Image>();
  public Transform AttemptPipGroup = null;
  
  public TextMeshProUGUI ClearedDisplay = null;
  
  private void OnEnable()
  {
    //Gameplay Events
    SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    //Track lifecycle events
    SM.Instance<EventManager>().RegisterListener<TrackPassed>(OnTrackPassed);
    
    SM.Instance<EventManager>().RegisterListener<TrackStarted>(OnTrackStarted);
    
    SM.Instance<EventManager>().RegisterListener<TrackFailed>(OnTrackFailed);
    
    SM.Instance<EventManager>().RegisterListener<TrackResetEvent>(OnTrackReset);
    
    //Transition lifecycle events
    SM.Instance<EventManager>().RegisterListener<TransitionStarted>(OnTransitionStarted);
    
    SM.Instance<EventManager>().RegisterListener<TransitionEnded>(OnTransitionEnded);
    
    //Run life cycle events
    SM.Instance<EventManager>().RegisterListener<RunStaged>(OnRunStaged);
    
    SM.Instance<EventManager>().RegisterListener<RunUpdate>(OnRunUpdate);
    
    SM.Instance<EventManager>().RegisterListener<RunEnded>(OnRunEnded);
  }
  
  private void OnDisable()
  {
    //Gameplay Events
    SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    //Track lifecycle events
    SM.Instance<EventManager>().UnregisterListener<TrackPassed>(OnTrackPassed);
    
    SM.Instance<EventManager>().UnregisterListener<TrackStarted>(OnTrackStarted);
    
    SM.Instance<EventManager>().UnregisterListener<TrackFailed>(OnTrackFailed);
    
    SM.Instance<EventManager>().UnregisterListener<TrackResetEvent>(OnTrackReset);
   
    //Transition lifecycle events
    SM.Instance<EventManager>().UnregisterListener<TransitionStarted>(OnTransitionStarted);
    
    SM.Instance<EventManager>().UnregisterListener<TransitionEnded>(OnTransitionEnded);

    //Run life cycle events
    SM.Instance<EventManager>().UnregisterListener<RunStaged>(OnRunStaged);
    
    SM.Instance<EventManager>().UnregisterListener<RunUpdate>(OnRunUpdate);
    
    SM.Instance<EventManager>().UnregisterListener<RunEnded>(OnRunEnded);
  }
  
  //The run was updated - sync UI info
  private void OnRunUpdate(RunUpdate context)
  {
    ClearedDisplay.text = context.Run.TracksPassed.ToString();

    foreach (var pip in AttemptPips)
    {
      Destroy(pip.gameObject);
    }

    AttemptPips = new List<Image>();
    
    for (int i = 0; i < context.Run.RemainingAttempts; i++)
    {
     AttemptPips.Add(Instantiate(AttemptPipTemplate, AttemptPipGroup));
    }
  }

  private void OnRunStaged(RunStaged context)
  {
    if (m_gameOverSplash != null)
    {
      Destroy(m_gameOverSplash.gameObject);
    }
    
    if (m_mainMenuSplash == null)
    { 
      m_mainMenuSplash = Instantiate(MainMenuSplash, HUDCanvas.transform);
      m_mainMenuSplash.GetComponent<Animator>().SetTrigger("Blink");
    }
    
    HUDCanvas.alpha = 0;
  }
  
  private void OnTransitionStarted(TransitionStarted transitionStarted)
  {
    LeanTween.value(HUDCanvas.gameObject, 1, 0, 0.65f).setEase(LeanTweenType.easeInExpo)
      .setOnUpdate((val) =>
      {
        HUDCanvas.alpha = val;
      })
      .setOnComplete(() =>
      {
        HUDCanvas.alpha = 0;
      });

    if (m_launchPrepareSplash != null)
    { 
     Destroy(m_launchPrepareSplash.gameObject);
    }
    
    if (m_transitionSplash == null)
    { 
      m_transitionSplash = Instantiate(TransitionSplash, HUDCanvas.transform);
      m_transitionSplash.GetComponent<Animator>().SetTrigger("Blink");
    }
  }
  
  private void OnTransitionEnded(TransitionEnded transitionEnded)
  {
    if (m_transitionSplash != null)
    {
      LeanTween.value(HUDCanvas.gameObject, m_transitionSplash.alpha, 0, 0.65f).setEase(LeanTweenType.easeInExpo)
        .setOnUpdate((val) =>
        {
          m_transitionSplash.alpha = val;
        })
        .setOnComplete(() =>
        {
          Destroy(m_transitionSplash.gameObject);
        });
    }
  }
  
  private void OnTrackStarted(TrackStarted context)
  {
      foreach (var beat in context.Track.Beats)
      {
        if (beat.Action != BeatAction.Empty)
        {
          var beatPromptInstance = Instantiate(BeatPromptTemplate, null);
          beatPromptInstance.Beat = beat;

          Sprite actionSprite = null;

          if (ActionSprites.TryGetValue(beat.Action, out actionSprite))
          {
            beatPromptInstance.FormatPrompt(true, false);
            beatPromptInstance.PromptImageA.sprite = ActionSprites[beat.Action];
          }

          beatPromptInstance.transform.position = OrbitHelpers.OrbitPointFromNormalisedPosition(context.Track.World.Orbit, beat.NormalisedTime);

          BeatPrompts.Add(beatPromptInstance);
        }
      }
    
    LeanTween.value(HUDCanvas.gameObject, HUDCanvas.alpha, 1, 0.65f).setEase(LeanTweenType.easeInExpo)
      .setOnUpdate((val) =>
      {
        HUDCanvas.alpha = val;
      })
      .setOnComplete(() =>
      {
        HUDCanvas.alpha = 1;
      });
    
    if (m_mainMenuSplash != null)
    { 
      Destroy(m_mainMenuSplash.gameObject);
    }
  }

  private void OnTrackFailed(TrackFailed context)
  {
    Debug.Log("Responding to track fail");
    Debug.Log("Track failed, removing a pip");
    
    foreach (var prompt in BeatPrompts)
    {
      //Debug.LogFormat($"UI:Trying to set alpha");
      prompt.CanvasGroup.alpha = 0.25f;
      prompt.transform.localScale = Vector3.one * 0.25f;
    }
    
    if (m_launchPrepareSplash != null)
    {
      Destroy(m_launchPrepareSplash.gameObject);
    }
  }
  
  private void OnRunEnded(RunEnded context)
  {
    if (m_gameOverSplash != null)
    {
      Destroy(m_gameOverSplash.gameObject);
    }
    
    m_gameOverSplash = Instantiate(GameOverSplash, HUDCanvas.transform);
  }


  private void OnTrackReset(TrackResetEvent context)
  {
   //Delete and remake?
   foreach (var prompt in BeatPrompts)
   {
     Destroy(prompt.gameObject);
   }

   BeatPrompts = new List<BeatPrompt>();
   
   foreach (var beat in context.Track.Beats)
   {
     if (beat.Action != BeatAction.Empty)
     {
       var beatPromptInstance = Instantiate(BeatPromptTemplate, null);
       beatPromptInstance.Beat = beat;

       Sprite actionSprite = null;

       if (ActionSprites.TryGetValue(beat.Action, out actionSprite))
       {
         beatPromptInstance.FormatPrompt(true, false);
         beatPromptInstance.PromptImageA.sprite = ActionSprites[beat.Action];
       }

       beatPromptInstance.transform.position = OrbitHelpers.OrbitPointFromNormalisedPosition(context.Track.World.Orbit, beat.NormalisedTime);

       BeatPrompts.Add(beatPromptInstance);
     }
   }
   
      foreach (var prompt in BeatPrompts)
      {
        prompt.CanvasGroup.alpha = 1f;
        prompt.transform.localScale = Vector3.one;
      }
      
      //Destroy wait for reset splash
      
      
  }

  //This should be tracks
  private void OnTrackPassed(TrackPassed context)
  {
    if (m_launchPrepareSplash == null)
    { 
      m_launchPrepareSplash = Instantiate(LaunchPrepareSplash, HUDCanvas.transform);
      m_launchPrepareSplash.GetComponent<Animator>().SetTrigger("Blink");
    }
  }
  
  // private void OnNewLevel(NewLevel context)
  // {
  //   foreach (var beat in context.Level.Track.NormalisedBeatTimes())
  //   {
  //     if (beat.Key.Action != BeatAction.Empty)
  //     {
  //       var beatPromptInstance =  Instantiate(BeatPromptTemplate, null);
  //       beatPromptInstance.Beat = beat.Key;
  //
  //       Sprite actionSprite = null;
  //       
  //       if(ActionSprites.TryGetValue(beat.Key.Action, out actionSprite))
  //       {
  //         beatPromptInstance.FormatPrompt(true,false);
  //         beatPromptInstance.PromptImageA.sprite = ActionSprites[beat.Key.Action];    
  //       }
  //       
  //       beatPromptInstance.transform.position = OrbitHelpers.OrbitPointFromNormalisedPosition( context.Level.World.Orbit, beat.Value);
  //       
  //       BeatPrompts.Add(beatPromptInstance);
  //     }
  //   }
  //}

  private void OnBeatAttempt(BeatAttemptEvent context)
  {
        GameObject attemptSplash = null;
        GameObject newSplash = null;
        
        if (context.Beat.State == Gameplay.Beat.States.Success)
        {
          if (AttemptSplashes.TryGetValue(Beat.States.Success,out attemptSplash))
          {
           newSplash = Instantiate(attemptSplash,HUDCanvas.transform);
          }
        }
        
        else if (context.Beat.State == Gameplay.Beat.States.Failed)
        {
          if (AttemptSplashes.TryGetValue(Beat.States.Failed,out attemptSplash))
          {
           newSplash = Instantiate(attemptSplash,HUDCanvas.transform);
          }
        }
        
        else if (context.Beat.State == Gameplay.Beat.States.Missed)
        {
          if (AttemptSplashes.TryGetValue(Beat.States.Missed,out attemptSplash))
          {
           newSplash =Instantiate(attemptSplash,HUDCanvas.transform);
          }
        }

        if (newSplash != null)
        {
          LeanTween.value(gameObject, 1f, 1.055f, 0.45f)
            .setEase(LeanTweenType.punch)
            .setOnUpdate((float val) => newSplash.transform.GetChild(0).localScale = Vector3.one * val);
          
          LeanTween.value(gameObject, 1f, 0, 0.55f)
            .setOnUpdate((float val) => newSplash.GetComponent<CanvasGroup>().alpha =  val)
            .setOnComplete(() => Destroy(newSplash));
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

