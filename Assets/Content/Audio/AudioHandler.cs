using System;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

[System.Serializable]
public class StringAudioClipDictionary : SerializableDictionaryBase<string, AudioClip>
{
}


public class AudioHandler : MonoBehaviour
{
  public StringAudioClipDictionary Soundscapes = new StringAudioClipDictionary();

  [SerializeField]
  private AudioSource SoundscapeSourceA = null;
  
  [SerializeField]
  private AudioSource SoundscapeSourceB = null;
  
  public string SoundscapeID = "";

  [Range(0,1)]
  public float GameTempo = 0;

  public float SpeedFactor = 0.05f;
  
  public Vector2 Crossfade = new Vector2(0.4f,0.8f);


  private Coroutine fadeIn = null;
  private Coroutine fadeOut = null;
  
  private void OnEnable()
  {
    SoundscapeSourceB.Pause();
    EvaluateSoundscape();   
  }

  private void OnValidate()
  {
    // GameTempo = Mathf.Repeat(GameTempo + Time.deltaTime*SpeedFactor, 1);
    EvaluateSoundscape();
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
        //oldSource.clip = null;

        // LeanTween.value(gameObject, 0f, 1f, Crossfade.x)
        // .setOnUpdate((val) =>
        // {
        //   newSource.volume = val;
        // })
        // .setOnComplete(() =>
        // {
        //   newSource.volume = 1;
        //   
        //   LeanTween.value(gameObject, 1f, 0f, Crossfade.y)
        //     .setOnUpdate((val) =>
        //     {
        //       oldSource.volume = val;
        //     })
        //     .setOnComplete(() =>
        //     {
        //       oldSource.Stop();
        //       oldSource.clip = null;
        //     });
        // });


      //}
    }
  }

  private void FixedUpdate()
  {
    // GameTempo = Mathf.Repeat(GameTempo + Time.deltaTime*SpeedFactor, 1);
    // EvaluateSoundscape();
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
