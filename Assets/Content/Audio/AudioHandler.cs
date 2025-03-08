using System;
using System.Collections;
using Events;
using Gameplay;
using Gameplay.TrackEvents;
using RotaryHeart.Lib.SerializableDictionary;
using SGS29.Utilities;
using UnityEngine;

[System.Serializable]
public class StringAudioClipDictionary : SerializableDictionaryBase<string, AudioClip>
{
}


public class AudioHandler : MonoBehaviour
{
  public StringAudioClipDictionary Soundscapes = new StringAudioClipDictionary();

  public StringAudioClipDictionary Stings = new StringAudioClipDictionary();
  
  [SerializeField]
  private AudioSource SoundscapeSourceA = null;
  
  [SerializeField]
  private AudioSource SoundscapeSourceB = null;

  [SerializeField]
  private AudioSource StingAudioSource = null;
  
  public string SoundscapeID = "";

  [Range(0,1)]
  public float GameTempo = 0;

  public float SpeedFactor = 0.05f;
  
  public Vector2 Crossfade = new Vector2(0.4f,0.8f);


  private Coroutine fadeIn = null;
  private Coroutine fadeOut = null;
  
   private void OnEnable()
  {
    SM.Instance<EventManager>().RegisterListener<NewLevel>(OnNewLevel);
    SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
    SoundscapeSourceB.Pause();
    EvaluateSoundscape();   
  }

  private void OnDisable()
  {
    SM.Instance<EventManager>().UnregisterListener<NewLevel>(OnNewLevel);
    SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
  }

  private void OnBeatAttempt(BeatAttemptEvent context)
  {
    switch (context.Beat.State)
    {
      case Beat.States.Failed:
        case Beat.States.Missed:
          PlaySting("stingbad");
          break;
        
      case Beat.States.Success:
          PlaySting("stinggood");
          break;
    }
  }
  
  
  private void OnNewLevel(NewLevel context)
  {
   
  }
  
  private void PlaySting(string stingID)
  {
    StingAudioSource.Stop();
    
    AudioClip newSting = null;
    
    if (Stings.TryGetValue(stingID, out newSting))
    {
      StingAudioSource.clip = newSting;
      StingAudioSource.Play();
    }
  }

  private void EvaluateSoundscape()
  {
    string speedID = "Slowest";

    if (GameTempo >= 0.75)
    {
      speedID = "fastest";
      
    }
    
    else if (GameTempo >= 0.5)
    {
      speedID = "fast";
    }
    
    else if (GameTempo >= 0.25f)
    {
      speedID = "slow";
    }

    else
    {
      speedID = "slowest";
    }
    
    
    SetSoundscape(speedID);
  }
  
  private void SetSoundscape(string soundScapeID)
  {
    AudioClip newAudioClip = null;
  
    AudioSource oldSource = SoundscapeSourceA.volume >= 0.9f ? SoundscapeSourceA : SoundscapeSourceB;
    AudioSource newSource = oldSource == SoundscapeSourceA? SoundscapeSourceB : SoundscapeSourceA;
    
    if(Soundscapes.TryGetValue(soundScapeID,out newAudioClip))
    {

      if (oldSource.clip == newAudioClip)
      {
        return;
      }

      // if (oldSource.clip != newAudioClip)
      // {
        
        newSource.clip = newAudioClip;
        if (fadeIn != null)
        {
          StopCoroutine(fadeIn);
        }
        fadeIn =StartCoroutine(FadeAudioIn(newSource,oldSource, Crossfade.x));

        if (fadeOut != null)
        {
          StopCoroutine(fadeOut);
        }
        
        fadeOut = StartCoroutine(FadeAudioOut(oldSource, Crossfade.y));
    }
  }
  
  private IEnumerator FadeAudioIn( AudioSource newSource, AudioSource oldSource,float duration)
  {
    newSource.volume = 0f;
    float t = 0;

    newSource.Play();
    
    while (t<1)
    {
      newSource.volume = Mathf.Lerp(newSource.volume, 1, t/duration);


      t += Time.deltaTime;
      yield return null;
    }

    fadeIn = null;
  }
  
  private IEnumerator FadeAudioOut( AudioSource source, float duration)
  {
    source.volume = 1;
    float t = 0;

    while (t<1)
    {
      source.volume = Mathf.Lerp(source.volume, 0, t/duration);


      t += Time.deltaTime;
      yield return null;
    }

    source.Pause();
    fadeOut = null;
  }
}
