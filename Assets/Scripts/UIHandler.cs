using System;
using System.Collections.Generic;
using Events;
using Gameplay;
using Gameplay.TrackEvents;
using SGS29.Utilities;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
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
  
  public CanvasGroup TransitionSplash = null;
  private CanvasGroup m_transitionSplash = null;

  public CanvasGroup MainMenuSplash = null;
  private CanvasGroup m_mainMenuSplash = null;
  
  public BeatPrompt BeatPromptTemplate;
  public List<BeatPrompt> BeatPrompts = new List<BeatPrompt>();

  public BeatActionSpriteDictionary ActionSprites = new BeatActionSpriteDictionary();

  public BeatStateGameObjectDictionary AttemptSplashes = new BeatStateGameObjectDictionary();

  public List<Image> AttemptPips = new List<Image>();

  public AttemptSystem AttemptSystem = null;

  public TextMeshProUGUI ClearedDisplay = null;
  
  private void OnEnable()
  {
    SM.Instance<EventManager>().RegisterListener<NewLevel>(OnNewLevel);
    
    SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    SM.Instance<EventManager>().RegisterListener<LevelPassed>(OnLevelPassed);
    
    SM.Instance<EventManager>().RegisterListener<GameOver>(OnGameOver);
    
    SM.Instance<EventManager>().RegisterListener<TrackStarted>(OnTrackStarted);
    
    SM.Instance<EventManager>().RegisterListener<TrackFailed>(OnTrackFailed);
    
    SM.Instance<EventManager>().RegisterListener<TrackResetEvent>(OnTrackReset);
    
    SM.Instance<EventManager>().RegisterListener<GameManager.TransitionStarted>(OnTransitionStarted);
    
    SM.Instance<EventManager>().RegisterListener<GameManager.TransitionEnded>(OnTransitionEnded);
    
    SM.Instance<EventManager>().RegisterListener<MainMenu>(OnMainMenu);
  }

  private void OnDisable()
  {
    SM.Instance<EventManager>().UnregisterListener<NewLevel>(OnNewLevel);
    
    SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    SM.Instance<EventManager>().UnregisterListener<LevelPassed>(OnLevelPassed);
    
    SM.Instance<EventManager>().UnregisterListener<GameOver>(OnGameOver);
    
    SM.Instance<EventManager>().UnregisterListener<TrackStarted>(OnTrackStarted);
    
    SM.Instance<EventManager>().UnregisterListener<TrackFailed>(OnTrackFailed);
    
    SM.Instance<EventManager>().UnregisterListener<TrackResetEvent>(OnTrackReset);
    
    SM.Instance<EventManager>().UnregisterListener<GameManager.TransitionStarted>(OnTransitionStarted);
    
    SM.Instance<EventManager>().UnregisterListener<GameManager.TransitionEnded>(OnTransitionEnded);

    SM.Instance<EventManager>().UnregisterListener<MainMenu>(OnMainMenu);
  }

  private void OnMainMenu(MainMenu context)
  {
    if (m_mainMenuSplash == null)
    { 
      m_mainMenuSplash = Instantiate(MainMenuSplash, HUDCanvas.transform);
      m_mainMenuSplash.GetComponent<Animator>().SetTrigger("Blink");
    }
    
    HUDCanvas.alpha = 0;
    
    
  }

  private void OnTransitionStarted(GameManager.TransitionStarted context)
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

    if (m_transitionSplash == null)
    { 
      m_transitionSplash = Instantiate(TransitionSplash, HUDCanvas.transform);
      m_transitionSplash.GetComponent<Animator>().SetTrigger("Blink");
    }
  }
  
  private void OnTransitionEnded(GameManager.TransitionEnded obj)
  {
    foreach (var prompt in BeatPrompts)
    {
      prompt.CanvasGroup.alpha = 0;
    }
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
    
    LeanTween.value(HUDCanvas.gameObject, HUDCanvas.alpha, 1, 0.65f).setEase(LeanTweenType.easeInExpo)
      .setOnUpdate((val) =>
      {
        HUDCanvas.alpha = val;
      })
      .setOnComplete(() =>
      {
        HUDCanvas.alpha = 1;
      });

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
    
    if (m_mainMenuSplash != null)
    { 
      Destroy(m_mainMenuSplash.gameObject);
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

    ClearedDisplay.text = (context.Level.Number+1).ToString();

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
