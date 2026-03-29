////using UnityEngine;
////using UnityEngine.UI;
////using System.Collections;

////public class Cowanimationcontroller : MonoBehaviour
////{
////    public enum CowState { Idle, SideRun, Jump, Hit, Win }

////    [Header("Movement")]
////    public float moveSpeed = 200f;
////    public float minX = 0f;
////    public float maxX = 9000f;

////    [Header("Idle Animation")]
////    public Sprite[] idleFrames;
////    public float idleFPS = 12f;

////    [Header("Side Run Animation")]
////    public Sprite[] sideRunFrames;
////    public float sideRunFPS = 12f;

////[Header("Jump Animation")]
////public Sprite[] jumpFrames;
////public float jumpFPS = 12f;

////    [Header("Hit Animation")]
////    public Sprite[] hitFrames;
////    public float hitFPS = 12f;

////    [Header("Win / Celebration Animation")]
////    public Sprite[] winFrames;
////    public float winFPS = 12f;

////    private Image cowImage;
////    private RectTransform cowRect;
////    private Vector2 originalSize;
////    private CowState currentState = CowState.Idle;
////    private bool isAlive = false;
////    private bool hitThenWin = false;  // ✅ after Hit → go to Win



////    [HideInInspector] public float worldX = 0f;

////    // ─────────────────────────────────────────────────────────
////    void Start()
////    {
////        isAlive = true;
////        cowImage = GetComponent<Image>();
////        cowRect = GetComponent<RectTransform>();
////        originalSize = cowRect.sizeDelta;
////        worldX = cowRect.anchoredPosition.x;
////        SetState(CowState.Idle);
////    }

////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////    // ─────────────────────────────────────────────────────────
////    void Update()
////    {
////        if (!isAlive) return;
////        if (currentState == CowState.Win) return;

////        HandleMovement();
////        HandleAnimationSwitch();
////        HandleActionInput();
////    }

////    void HandleMovement()
////    {
////        bool left = moveLeft || Input.GetKey(KeyCode.LeftArrow);
////        bool right = moveRight || Input.GetKey(KeyCode.RightArrow);

////        if (right)
////        {
////            worldX = Mathf.Min(worldX + moveSpeed * Time.deltaTime, maxX);
////            cowRect.localScale = new Vector3(1f, 1f, 1f);
////        }
////        else if (left)
////        {
////            worldX = Mathf.Max(worldX - moveSpeed * Time.deltaTime, minX);
////            cowRect.localScale = new Vector3(-1f, 1f, 1f);
////        }
////    }

////    void HandleAnimationSwitch()
////    {
////        if (currentState == CowState.Hit) return;
////        if (currentState == CowState.Jump) return;

////        bool moving = moveLeft || moveRight ||
////                      Input.GetKey(KeyCode.LeftArrow) ||
////                      Input.GetKey(KeyCode.RightArrow);

////        if (moving && currentState != CowState.SideRun) SetState(CowState.SideRun);
////        if (!moving && currentState == CowState.SideRun) SetState(CowState.Idle);
////    }

////    void HandleActionInput()
////    {
////        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////            PlayJump();
////    }

////    // ─────────────────────────────────────────────────────────
////    // Public methods
////    // ─────────────────────────────────────────────────────────
////    public void PlayIdle()
////    {
////        if (!isAlive) return;
////        hitThenWin = false;
////        SetState(CowState.Idle);
////    }

////    public void PlayJump()
////    {
////        if (!isAlive) return;
////        if (currentState == CowState.Win) return;
////        if (currentState == CowState.Jump) return;
////        if (currentState == CowState.Hit) return;
////        SetState(CowState.Jump);
////    }

////    public void PlayHit()
////    {
////        if (!isAlive) return;
////        if (currentState == CowState.Hit) return;
////        if (currentState == CowState.Win) return;
////        hitThenWin = false;
////        SetState(CowState.Hit);
////    }

////    public void PlayWin()
////    {
////        if (!isAlive) return;
////        Debug.Log("[Cow] PlayWin called!");
////        hitThenWin = false;
////        SetState(CowState.Win);
////    }

////    // ✅ Called by BellTrigger — Hit plays once then Celebration loops
////    public void PlayHitThenWin()
////    {
////        if (!isAlive) return;
////        if (currentState == CowState.Win) return;
////        Debug.Log("[Cow] PlayHitThenWin called!");
////        hitThenWin = true;
////        SetState(CowState.Hit);
////    }

////    // On-screen buttons
////    public void OnRightDown() => moveRight = true;
////    public void OnRightUp() => moveRight = false;
////    public void OnLeftDown() => moveLeft = true;
////    public void OnLeftUp() => moveLeft = false;
////    public void OnJumpDown() => PlayJump();

////    // ─────────────────────────────────────────────────────────
////    void SetState(CowState newState)
////    {
////        if (!isAlive) return;
////        currentState = newState;
////        Debug.Log($"[Cow] State → {newState}");
////        StopAllCoroutines();
////        StartCoroutine(MasterLoop());
////    }

////    IEnumerator MasterLoop()
////    {
////        int frame = 0;

////        while (true)
////        {
////            if (!isAlive) yield break;

////            Sprite[] frames = null;
////            float fps = 12f;

////            switch (currentState)
////            {
////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////                case CowState.SideRun: frames = sideRunFrames; fps = sideRunFPS; break;
////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////            }

////            if (frames == null || frames.Length == 0)
////            {
////                Debug.LogError($"[Cow] {currentState} frames are EMPTY!");
////                yield return new WaitForSeconds(0.1f);
////                continue;
////            }

////            if (frame >= frames.Length) frame = 0;

////            cowImage.sprite = frames[frame];
////            cowRect.sizeDelta = originalSize;
////            frame++;

////            // ✅ Jump plays ONCE → back to Idle
////            if (currentState == CowState.Jump && frame >= frames.Length)
////            {
////                frame = 0;
////                currentState = CowState.Idle;
////            }

////            // ✅ Hit plays ONCE → Win (bell) or Idle (normal hit)
////            if (currentState == CowState.Hit && frame >= frames.Length)
////            {
////                frame = 0;
////                if (hitThenWin)
////                {
////                    hitThenWin = false;
////                    currentState = CowState.Win;
////                    Debug.Log("[Cow] Hit done → Celebration!");
////                }
////                else
////                {
////                    currentState = CowState.Idle;
////                }
////            }

////            // ✅ Idle, SideRun, Win → loop forever
////            if (frame >= frames.Length) frame = 0;

////            yield return new WaitForSeconds(1f / fps);
////        }
////    }
////}

//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class Cowanimationcontroller : MonoBehaviour
//{
//    public enum CowState { Idle, SideRun, Jump, Hit, Win }

//    [Header("Movement")]
//    public float moveSpeed = 500f;
//    public float maxX = 9000f;

//    [Header("Animations")]
//    public Sprite[] idleFrames;
//    public Sprite[] runFrames;
//    public Sprite[] jumpFrames;
//    public Sprite[] hitFrames;
//    public Sprite[] winFrames;

//    public float fps = 12f;

//    private Image cowImage;
//    private RectTransform cowRect;

//    private CowState currentState;
//    private bool isRunning = false;

//    [HideInInspector] public float worldX = 0f;

//    void Start()
//    {
//        cowImage = GetComponent<Image>();
//        cowRect = GetComponent<RectTransform>();

//        worldX = cowRect.anchoredPosition.x;
//        SetState(CowState.Idle);
//    }

//    void Update()
//    {
//        if (!isRunning) return;

//        // ✅ AUTO RUN
//        worldX += moveSpeed * Time.deltaTime;

//        // ✅ JUMP INPUT
//        if (Input.GetKeyDown(KeyCode.Space))
//            PlayJump();
//    }

//    // =========================
//    // GAME CONTROL
//    // =========================
//    public void StartRunning()
//    {
//        isRunning = true;
//        SetState(CowState.SideRun);
//    }

//    public void StopRunning()
//    {
//        isRunning = false;
//    }

//    // =========================
//    // STATES
//    // =========================
//    public void PlayJump()
//    {
//        if (currentState == CowState.Jump || currentState == CowState.Win) return;
//        SetState(CowState.Jump);
//    }

//    public void PlayHit()
//    {
//        if (currentState == CowState.Win) return;
//        SetState(CowState.Hit);
//    }

//    public void PlayWin()
//    {
//        StopRunning();
//        SetState(CowState.Win);

//        // ✅ trigger main game manager congrats
//        GameManager.Instance?.SendMessage("ShowCongrats");
//    }

//    public void PlayHitThenWin()
//    {
//        StartCoroutine(HitThenWinRoutine());
//    }

//    IEnumerator HitThenWinRoutine()
//    {
//        SetState(CowState.Hit);
//        yield return new WaitForSeconds(0.5f);

//        PlayWin();
//    }

//    // =========================
//    void SetState(CowState state)
//    {
//        currentState = state;
//        StopAllCoroutines();
//        StartCoroutine(Animate());
//    }

//    IEnumerator Animate()
//    {
//        Sprite[] frames = idleFrames;

//        switch (currentState)
//        {
//            case CowState.Idle: frames = idleFrames; break;
//            case CowState.SideRun: frames = runFrames; break;
//            case CowState.Jump: frames = jumpFrames; break;
//            case CowState.Hit: frames = hitFrames; break;
//            case CowState.Win: frames = winFrames; break;
//        }

//        int i = 0;

//        while (true)
//        {
//            cowImage.sprite = frames[i];
//            i = (i + 1) % frames.Length;

//            yield return new WaitForSeconds(1f / fps);
//        }
//    }
//}


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Cowanimationcontroller : MonoBehaviour
{
    public enum CowState { Idle, SideRun, Jump, Hit, Win }

    [Header("Movement")]
    public float moveSpeed = 500f;

    [Header("Animations")]
    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public Sprite[] jumpFrames;
    public Sprite[] hitFrames;
    public Sprite[] winFrames;

    public float fps = 12f;

    private Image cowImage;
    private RectTransform cowRect;

    private CowState currentState;

    private bool isRunning = false;
    private bool isJumping = false;   // ✅ FIX: prevent loop

    [HideInInspector] public float worldX = 0f;

    void Start()
    {
        cowImage = GetComponent<Image>();
        cowRect = GetComponent<RectTransform>();

        worldX = cowRect.anchoredPosition.x;

        SetState(CowState.Idle);
    }

    void Update()
    {
        if (!isRunning) return;

        // ✅ AUTO RUN
        worldX += moveSpeed * Time.deltaTime;

        // ✅ KEYBOARD JUMP
        if (Input.GetKeyDown(KeyCode.Space))
            PlayJump();
    }

    // =========================
    // GAME CONTROL
    // =========================
    public void StartRunning()
    {
        isRunning = true;
        SetState(CowState.SideRun);
    }

    public void StopRunning()
    {
        isRunning = false;
    }

    // =========================
    // ACTIONS
    // =========================
    public void PlayJump()
    {
        if (isJumping) return;              // 🚫 prevent loop
        if (currentState == CowState.Win) return;

        isJumping = true;
        SetState(CowState.Jump);
    }

    public void PlayHit()
    {
        if (currentState == CowState.Win) return;
        SetState(CowState.Hit);
    }

    public void PlayWin()
    {
        StopRunning();
        SetState(CowState.Win);

        GameManager.Instance?.SendMessage("ShowCongrats");
    }

    public void PlayHitThenWin()
    {
        StartCoroutine(HitThenWinRoutine());
    }

    IEnumerator HitThenWinRoutine()
    {
        SetState(CowState.Hit);
        yield return new WaitForSeconds(0.5f);
        PlayWin();
    }

    // UI Button
    public void OnJumpDown()
    {
        PlayJump();
    }

    // =========================
    void SetState(CowState state)
    {
        currentState = state;
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        Sprite[] frames = idleFrames;

        switch (currentState)
        {
            case CowState.Idle: frames = idleFrames; break;
            case CowState.SideRun: frames = runFrames; break;
            case CowState.Jump: frames = jumpFrames; break;
            case CowState.Hit: frames = hitFrames; break;
            case CowState.Win: frames = winFrames; break;
        }

        int i = 0;

        while (true)
        {
            if (frames == null || frames.Length == 0)
                yield break;

            cowImage.sprite = frames[i];
            i++;

            // ✅ JUMP END FIX
            if (currentState == CowState.Jump && i >= frames.Length)
            {
                isJumping = false;                 // 🔥 unlock jump
                SetState(CowState.SideRun);        // back to run
                yield break;
            }

            // ✅ HIT END
            if (currentState == CowState.Hit && i >= frames.Length)
            {
                SetState(CowState.SideRun);
                yield break;
            }

            // LOOP states
            if (i >= frames.Length)
                i = 0;

            yield return new WaitForSeconds(1f / fps);
        }
    }
}