using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteAlways]
public class BeatPrompt : MonoBehaviour
{
    //public Sprite PromptSprite = null;
    public Gameplay.Beat Beat;
     public Image PromptImageA = null;
     public Image PromptImageB = null;

     public CanvasGroup CanvasGroup = null;
     
    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
        Debug.LogFormat(gameObject,$"CanvasGroup alpha is {CanvasGroup.alpha}");
    }

    public void FormatPrompt(bool enableA, bool enableB)
    {
        PromptImageA.enabled = enableA;
        PromptImageB.enabled = enableB;
    }
}