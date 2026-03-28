using UnityEngine;
using UnityEngine.UI;

public class UIIdleAnimation : MonoBehaviour
{
    public Image targetImage;  
    public Sprite[] idleFrames; 
    public float frameRate = 10f; 

    private int currentFrame = 0;
    private float timer;

    void Update()
    {
        if (idleFrames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer = 0f;

            targetImage.sprite = idleFrames[currentFrame];
            currentFrame = (currentFrame + 1) % idleFrames.Length;
        }
    }
}