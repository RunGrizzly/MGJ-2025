using Gameplay;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BeatPrompt : MonoBehaviour
{
    //public Sprite PromptSprite = null;
    public Gameplay.Beat Beat;
    public Image PromptImage = null;

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
    }
}