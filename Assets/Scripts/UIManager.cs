using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIManager : MonoBehaviour
{
  public SpawnOnOrbit SpawnOnOrbit;
  public OrbitManager OrbitManager;

  public BeatPrompt BeatPromptTemplate;

  public List<BeatPrompt> BeatPrompts = new List<BeatPrompt>();
  
  private void OnEnable()
  {
    foreach (var beat in SpawnOnOrbit.Beats)
    {
    var beatPromptInstance =  Instantiate(BeatPromptTemplate, null);
    beatPromptInstance.Beat = beat;
    beatPromptInstance.transform.position =OrbitManager.OrbitPointFromNormalisedPosition(  OrbitManager.MainOrbit,beat.Position);
    
    BeatPrompts.Add(beatPromptInstance);
    }
  }

  private void OnDisable()
  {
    foreach (var beatPrompt in BeatPrompts)
    {
      if (beatPrompt.gameObject != null)
      {
        DestroyImmediate(beatPrompt.gameObject);
      }
    }
    
    BeatPrompts.Clear();
  }
}
