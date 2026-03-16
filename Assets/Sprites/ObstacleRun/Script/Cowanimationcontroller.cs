using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CowAnimationController : MonoBehaviour
{
    public enum CowState { Idle, Run, Jump, Hit, Win }

    [Header("Idle Animation")]
    public Sprite[] idleFrames;
    public float idleFPS = 12f;

    [Header("Run Animation")]
    public Sprite[] runFrames;
    public float runFPS = 12f;

    [Header("Jump Animation")]
    public Sprite[] jumpFrames;
    public float jumpFPS = 12f;

    [Header("Hit Animation")]
    public Sprite[] hitFrames;
    public float hitFPS = 12f;

    [Header("Win Animation")]
    public Sprite[] winFrames;
    public float winFPS = 12f;

    private Image cowImage;
    private RectTransform cowRect;
    private Vector2 originalSize;

    // ✅ Current state controls which loop runs
    private CowState currentState = CowState.Idle;

    void Start()
    {
        cowImage = GetComponent<Image>();
        cowRect = GetComponent<RectTransform>();
        originalSize = cowRect.sizeDelta;
        SetState(CowState.Idle);
    }

    public void PlayIdle()
    {
        SetState(CowState.Idle);
    }

    public void PlayRun()
    {
        if (currentState == CowState.Win) return;
        SetState(CowState.Run);
    }

    public void PlayJump()
    {
        if (currentState == CowState.Win) return;
        SetState(CowState.Jump);
    }

    public void PlayHit()
    {
        if (currentState == CowState.Hit) return;
        if (currentState == CowState.Win) return;
        SetState(CowState.Hit);
    }

    public void PlayWin()
    {
        Debug.Log("[Cow] PlayWin called!");
        SetState(CowState.Win);
    }

    void SetState(CowState newState)
    {
        currentState = newState;
        Debug.Log($"[Cow] State changed to: {newState}");

        // ✅ Stop ALL then start master loop
        StopAllCoroutines();
        StartCoroutine(MasterLoop());
    }

    // ✅ Single master loop - checks state every frame
    IEnumerator MasterLoop()
    {
        int frame = 0;

        while (true)
        {
            Sprite[] frames = null;
            float fps = 12f;

            // ✅ Pick correct frames based on state
            switch (currentState)
            {
                case CowState.Idle:
                    frames = idleFrames;
                    fps = idleFPS;
                    break;
                case CowState.Run:
                    frames = runFrames;
                    fps = runFPS;
                    break;
                case CowState.Jump:
                    frames = jumpFrames;
                    fps = jumpFPS;
                    break;
                case CowState.Hit:
                    frames = hitFrames;
                    fps = hitFPS;
                    break;
                case CowState.Win:
                    frames = winFrames;
                    fps = winFPS;
                    break;
            }

            if (frames == null || frames.Length == 0)
            {
                Debug.LogError($"[Cow] {currentState} frames are EMPTY!");
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // ✅ Clamp frame index
            if (frame >= frames.Length)
                frame = 0;

            cowImage.sprite = frames[frame];
            cowRect.sizeDelta = originalSize;
            frame++;

            // ✅ Hit plays ONCE then returns to Run
            if (currentState == CowState.Hit && frame >= frames.Length)
            {
                frame = 0;
                currentState = CowState.Run;
            }

            // ✅ Jump plays ONCE then returns to Run
            if (currentState == CowState.Jump && frame >= frames.Length)
            {
                frame = 0;
                currentState = CowState.Run;
            }

            // ✅ Idle, Run and Win loop forever
            if (frame >= frames.Length)
                frame = 0;

            yield return new WaitForSeconds(1f / fps);
        }
    }
}

//## How It Works
//
// SetState(Idle)  → StopAllCoroutines → MasterLoop starts
//                → plays idle frames forever ✅
//
// SetState(Run)   → StopAllCoroutines → MasterLoop starts
//                → plays run frames forever ✅
//
// SetState(Jump)  → StopAllCoroutines → MasterLoop starts
//                → plays jump frames ONCE → auto switches to Run ✅
//
// SetState(Hit)   → StopAllCoroutines → MasterLoop starts
//                → plays hit frames ONCE → auto switches to Run ✅
//
// SetState(Win)   → StopAllCoroutines → MasterLoop starts
//                → plays win frames forever ✅
//
//## Console messages to expect
//
// "[Cow] State changed to: Idle"   → on Start ✅
// "[Cow] State changed to: Run"    → when game begins ✅
// "[Cow] PlayWin called!"          → when bell reached ✅
// "[Cow] Jump frames are EMPTY!"   → frames not assigned ❌
// "[Cow] Hit frames are EMPTY!"    → frames not assigned ❌