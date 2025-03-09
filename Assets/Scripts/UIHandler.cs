using System;
using System.Collections.Generic;
using Events;
using Gameplay;
using Gameplay.TrackEvents;
using SGS29.Utilities;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
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
  public Canvas HUDCanvas = null;
  public CanvasGroup GameOverSplash = null;
  
  public BeatPrompt BeatPromptTemplate;
  public List<BeatPrompt> BeatPrompts = new List<BeatPrompt>();

  public BeatActionSpriteDictionary ActionSprites = new BeatActionSpriteDictionary();

  public BeatStateGameObjectDictionary AttemptSplashes = new BeatStateGameObjectDictionary();

  public List<Image> AttemptPips = new List<Image>();

  public AttemptSystem AttemptSystem = null;
  
  
  private void OnEnable()
  {
    SM.Instance<EventManager>().RegisterListener<NewLevel>(OnNewLevel);
    
    SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    SM.Instance<EventManager>().RegisterListener<LevelPassed>(OnLevelPassed);
    
    SM.Instance<EventManager>().RegisterListener<GameOver>(OnGameOver);
    
    SM.Instance<EventManager>().RegisterListener<TrackStarted>(OnTrackStarted);
    
    SM.Instance<EventManager>().RegisterListener<TrackFailed>(OnTrackFailed);
    
    SM.Instance<EventManager>().RegisterListener<TrackResetEvent>(OnTrackReset);
  }

  private void OnDisable()
  {
    SM.Instance<EventManager>().UnregisterListener<NewLevel>(OnNewLevel);
    
    SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    SM.Instance<EventManager>().UnregisterListener<LevelPassed>(OnLevelPassed);
    
    SM.Instance<EventManager>().RegisterListener<GameOver>(OnGameOver);
    
    SM.Instance<EventManager>().UnregisterListener<TrackStarted>(OnTrackStarted);
    
    SM.Instance<EventManager>().UnregisterListener<TrackFailed>(OnTrackFailed);
    
    SM.Instance<EventManager>().UnregisterListener<TrackResetEvent>(OnTrackReset);
  }
  
  private void OnTrackStarted(TrackStarted context)
  {
    for (int i = 0; i < AttemptPips.Count; i++)
    {
      if (i < AttemptSystem._remainingAttempts)
      {
        AttemptPips[i].gameObject.SetActive(true);
        continue;
      }
      
      AttemptPips[i].gameObject.SetActive(false);
    }
  }

  private void OnTrackFailed(TrackFailed context)
  {
    Debug.Log("Responding to track fai;");
    
    for (int i = 0; i < AttemptPips.Count; i++)
    {
      Debug.Log("Track failed, removing a pip");
      
      if (i < AttemptSystem._remainingAttempts-1)
      {
        AttemptPips[i].gameObject.SetActive(true);
        continue;
      }
      
      AttemptPips[i].gameObject.SetActive(false);
    }
    
    foreach (var prompt in BeatPrompts)
    {
      Debug.LogFormat($"UI:Trying to set alpha");
      prompt.CanvasGroup.alpha = 0.25f;
      prompt.transform.localScale = Vector3.one * 0.25f;
    }
    
    Debug.LogWarningFormat($"Track was failed even after running out of attempts");
  }
  
  private void OnGameOver(GameOver context)
  {
    var gameOverSplash = Instantiate(GameOverSplash, HUDCanvas.transform);
  }


  private void OnTrackReset(TrackResetEvent context)
  {
    Debug.Log("UI:Track was reset");
      foreach (var prompt in BeatPrompts)
      {
        prompt.CanvasGroup.alpha = 1f;
        prompt.transform.localScale = Vector3.one;
      }
  }

  private void OnLevelPassed(LevelPassed context)
  {
    foreach (var prompt in BeatPrompts)
    {
      Destroy(prompt.gameObject);
    }
    
    BeatPrompts.Clear();
    
    foreach (var beat in context.Level.ExitTrack.GetNormalizedBeatTimes())
    {
      if (beat.Key.Action != BeatAction.Empty)
      {
        var beatPromptInstance =  Instantiate(BeatPromptTemplate, null);
        beatPromptInstance.Beat = beat.Key;

        Sprite actionSprite = null;
        
        if(ActionSprites.TryGetValue(beat.Key.Action, out actionSprite))
        {
          beatPromptInstance.FormatPrompt(true,true);
          beatPromptInstance.PromptImageA.sprite = ActionSprites[beat.Key.Action];    
          beatPromptInstance.PromptImageB.sprite = ActionSprites[beat.Key.Action];    
        }
        
        beatPromptInstance.transform.position = OrbitHelpers.OrbitPointFromNormalisedPosition( context.Level.World.Orbit,beat.Value);
      }
    }
  }
  
  private void OnNewLevel(NewLevel context)
  {
    foreach (var beat in context.Level.Track.GetNormalizedBeatTimes())
    {
      if (beat.Key.Action != BeatAction.Empty)
      {
        var beatPromptInstance =  Instantiate(BeatPromptTemplate, null);
        beatPromptInstance.Beat = beat.Key;

        Sprite actionSprite = null;
        
        if(ActionSprites.TryGetValue(beat.Key.Action, out actionSprite))
        {
          beatPromptInstance.FormatPrompt(true,false);
          beatPromptInstance.PromptImageA.sprite = ActionSprites[beat.Key.Action];    
        }
        
        beatPromptInstance.transform.position = OrbitHelpers.OrbitPointFromNormalisedPosition( context.Level.World.Orbit, beat.Value);
        
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
