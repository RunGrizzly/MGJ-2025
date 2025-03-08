using System.Collections.Generic;
using Events;
using Gameplay;
using Gameplay.TrackEvents;
using SGS29.Utilities;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

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
  
  public BeatPrompt BeatPromptTemplate;
  private List<BeatPrompt> BeatPrompts = new List<BeatPrompt>();

  public BeatActionSpriteDictionary ActionSprites = new BeatActionSpriteDictionary();

  public BeatStateGameObjectDictionary AttemptSplashes = new BeatStateGameObjectDictionary();
  
  
  
  private void OnEnable()
  {
    SM.Instance<EventManager>().RegisterListener<NewLevel>(OnNewLevel);
    SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    SM.Instance<EventManager>().RegisterListener<LevelPassed>(OnLevelPassed);
  }

  private void OnDisable()
  {
    SM.Instance<EventManager>().UnregisterListener<NewLevel>(OnNewLevel);
    SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    SM.Instance<EventManager>().UnregisterListener<LevelPassed>(OnLevelPassed);
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
        
        beatPromptInstance.transform.position = OrbitHelpers.OrbitPointFromNormalisedPosition( context.Level.World.Orbit,beat.Value);
        
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
