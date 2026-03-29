////using UnityEngine;
////using UnityEngine.UI;
////using System.Collections;

////public class CowAnimationController : MonoBehaviour
////{
////    public enum CowState { Idle, SideRun, Jump, Hit, Win }

////    [Header("Movement")]
////    public float moveSpeed = 200f;
////    public float minX = -600f;
////    public float maxX = 600f;

////    [Header("Idle Animation")]
////    public Sprite[] idleFrames;
////    public float idleFPS = 12f;

////    [Header("Side Run Animation")]
////    public Sprite[] sideRunFrames;
////    public float sideRunFPS = 12f;

////    [Header("Jump Animation")]
////    public Sprite[] jumpFrames;
////    public float jumpFPS = 12f;

////    [Header("Hit Animation")]
////    public Sprite[] hitFrames;
////    public float hitFPS = 12f;

////    [Header("Win Animation")]
////    public Sprite[] winFrames;
////    public float winFPS = 12f;

////    private Image cowImage;
////    private RectTransform cowRect;
////    private Vector2 originalSize;
////    private CowState currentState = CowState.Idle;
////    private bool moveLeft = false;
////    private bool moveRight = false;

////    // ✅ This is the real fix — bool flag the coroutine can safely read
////    private bool isAlive = false;

////    // ─────────────────────────────────────────────────────────
////    void Start()
////    {
////        isAlive = true;
////        cowImage = GetComponent<Image>();
////        cowRect = GetComponent<RectTransform>();
////        originalSize = cowRect.sizeDelta;
////        SetState(CowState.Idle);
////    }

////    void OnDestroy()
////    {
////        // ✅ Flag goes false BEFORE coroutine can touch destroyed objects
////        isAlive = false;
////        StopAllCoroutines();
////    }

////    void OnDisable()
////    {
////        isAlive = false;
////        StopAllCoroutines();
////    }

////    void OnEnable()
////    {
////        // ✅ Re-enable flag if object comes back
////        if (cowImage != null && cowRect != null)
////            isAlive = true;
////    }

////    // ─────────────────────────────────────────────────────────
////    void Update()
////    {
////        if (!isAlive) return;
////        if (currentState == CowState.Win) return;

////        HandleMovement();
////        HandleAnimationSwitch();
////    }

////    // ─────────────────────────────────────────────────────────
////    // Movement
////    // ─────────────────────────────────────────────────────────
////    //void HandleMovement()
////    //{
////    //    bool left = Input.GetKey(KeyCode.LeftArrow);
////    //    bool right = Input.GetKey(KeyCode.RightArrow);

////    //    if (right)
////    //    {
////    //        Vector2 pos = cowRect.anchoredPosition;
////    //        pos.x = Mathf.Min(pos.x + moveSpeed * Time.deltaTime, maxX);
////    //        cowRect.anchoredPosition = pos;
////    //        cowRect.localScale = new Vector3(1f, 1f, 1f);   // face right
////    //    }
////    //    else if (left)
////    //    {
////    //        Vector2 pos = cowRect.anchoredPosition;
////    //        pos.x = Mathf.Max(pos.x - moveSpeed * Time.deltaTime, minX);
////    //        cowRect.anchoredPosition = pos;
////    //        cowRect.localScale = new Vector3(-1f, 1f, 1f);  // face left (flip)
////    //    }
////    //}

////    void HandleMovement()
////    {
////        bool left = moveLeft || Input.GetKey(KeyCode.LeftArrow);
////        bool right = moveRight || Input.GetKey(KeyCode.RightArrow);

////        if (right)
////        {
////            Vector2 pos = cowRect.anchoredPosition;
////            pos.x = Mathf.Min(pos.x + moveSpeed * Time.deltaTime, maxX);
////            cowRect.anchoredPosition = pos;
////            cowRect.localScale = new Vector3(1f, 1f, 1f);
////        }
////        else if (left)
////        {
////            Vector2 pos = cowRect.anchoredPosition;
////            pos.x = Mathf.Max(pos.x - moveSpeed * Time.deltaTime, minX);
////            cowRect.anchoredPosition = pos;
////            cowRect.localScale = new Vector3(-1f, 1f, 1f);
////        }
////    }

////    // ─────────────────────────────────────────────────────────
////    // Animation switching
////    // ─────────────────────────────────────────────────────────
////    void HandleAnimationSwitch()
////    {
////        if (currentState == CowState.Hit) return;
////        if (currentState == CowState.Jump) return;

////        //bool moving = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
////        bool moving = moveLeft || moveRight ||
////              Input.GetKey(KeyCode.LeftArrow) ||
////              Input.GetKey(KeyCode.RightArrow);

////        if (moving && currentState != CowState.SideRun)
////            SetState(CowState.SideRun);

////        if (!moving && currentState == CowState.SideRun)
////            SetState(CowState.Idle);
////    }

////    // ─────────────────────────────────────────────────────────
////    // Public methods
////    // ─────────────────────────────────────────────────────────
////    public void PlayIdle()
////    {
////        if (!isAlive) return;
////        SetState(CowState.Idle);
////    }

////    public void PlayJump()
////    {
////        if (!isAlive) return;
////        if (currentState == CowState.Win) return;
////        SetState(CowState.Jump);
////    }

////    public void PlayHit()
////    {
////        if (!isAlive) return;
////        if (currentState == CowState.Hit) return;
////        if (currentState == CowState.Win) return;
////        SetState(CowState.Hit);
////    }

////    public void PlayWin()
////    {
////        if (!isAlive) return;
////        Debug.Log("[Cow] PlayWin called!");
////        SetState(CowState.Win);
////    }

////    // RIGHT BUTTON
////    public void OnRightDown()
////    {
////        moveRight = true;
////    }

////    public void OnRightUp()
////    {
////        moveRight = false;
////    }

////    // LEFT BUTTON
////    public void OnLeftDown()
////    {
////        moveLeft = true;
////    }

////    public void OnLeftUp()
////    {
////        moveLeft = false;
////    }

////    // ─────────────────────────────────────────────────────────
////    // State machine
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
////            // ✅ Bool flag check — safe inside coroutines unlike "this == null"
////            if (!isAlive) yield break;

////            Sprite[] frames = null;
////            float fps = 12f;

////            switch (currentState)
////            {
////                case CowState.Idle:
////                    frames = idleFrames; fps = idleFPS; break;
////                case CowState.SideRun:
////                    frames = sideRunFrames; fps = sideRunFPS; break;
////                case CowState.Jump:
////                    frames = jumpFrames; fps = jumpFPS; break;
////                case CowState.Hit:
////                    frames = hitFrames; fps = hitFPS; break;
////                case CowState.Win:
////                    frames = winFrames; fps = winFPS; break;
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

////            // ✅ Hit  → plays ONCE → back to Idle
////            if (currentState == CowState.Hit && frame >= frames.Length)
////            {
////                frame = 0;
////                currentState = CowState.Idle;
////            }

////            // ✅ Jump → plays ONCE → back to Idle
////            if (currentState == CowState.Jump && frame >= frames.Length)
////            {
////                frame = 0;
////                currentState = CowState.Idle;
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

//public class CowAnimationController : MonoBehaviour
//{
//    public enum CowState { Idle, SideRun, Jump, Hit, Win }

//    [Header("Movement")]
//    public float moveSpeed = 200f;
//    public float minX = -600f;    // left screen boundary
//    public float maxX = 9000f;   // world X limit (not screen — cow can go far right)

//    [Header("Idle Animation")]
//    public Sprite[] idleFrames;
//    public float idleFPS = 12f;

//    [Header("Side Run Animation")]
//    public Sprite[] sideRunFrames;
//    public float sideRunFPS = 12f;

//    [Header("Jump Animation")]
//    public Sprite[] jumpFrames;
//    public float jumpFPS = 12f;

//    [Header("Hit Animation")]
//    public Sprite[] hitFrames;
//    public float hitFPS = 12f;

//    [Header("Win Animation")]
//    public Sprite[] winFrames;
//    public float winFPS = 12f;

//    private Image cowImage;
//    private RectTransform cowRect;
//    private Vector2 originalSize;
//    private CowState currentState = CowState.Idle;
//    private bool isAlive = false;

//    // ✅ Tracks the cow's TRUE world X position (separate from screen position)
//    //    BackgroundScroller locks cowRect.anchoredPosition.x at followStartX
//    //    but we still need to know how far the cow has "really" moved
//    [HideInInspector]
//    public float worldX = 0f;

//    private bool moveLeft = false;
//    private bool moveRight = false;

//    // ─────────────────────────────────────────────────────────
//    void Start()
//    {
//        isAlive = true;
//        cowImage = GetComponent<Image>();
//        cowRect = GetComponent<RectTransform>();
//        originalSize = cowRect.sizeDelta;
//        worldX = cowRect.anchoredPosition.x;
//        SetState(CowState.Idle);
//    }

//    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//    void Update()
//    {
//        if (!isAlive) return;
//        if (currentState == CowState.Win) return;

//        HandleMovement();
//        HandleAnimationSwitch();
//    }

//    // ─────────────────────────────────────────────────────────
//    void HandleMovement()
//    {
//        bool left = moveLeft || Input.GetKey(KeyCode.LeftArrow);
//        bool right = moveRight || Input.GetKey(KeyCode.RightArrow);

//        if (right)
//        {
//            // ✅ Move worldX forward (BackgroundScroller reads anchoredPosition.x)
//            worldX = Mathf.Min(worldX + moveSpeed * Time.deltaTime, maxX);
//            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
//            cowRect.localScale = new Vector3(1f, 1f, 1f);
//        }
//        else if (left)
//        {
//            worldX = Mathf.Max(worldX - moveSpeed * Time.deltaTime, minX);
//            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
//            cowRect.localScale = new Vector3(-1f, 1f, 1f);
//        }
//    }

//    void HandleAnimationSwitch()
//    {
//        if (currentState == CowState.Hit) return;
//        if (currentState == CowState.Jump) return;

//        bool moving = moveLeft || moveRight ||
//                      Input.GetKey(KeyCode.LeftArrow) ||
//                      Input.GetKey(KeyCode.RightArrow);

//        if (moving && currentState != CowState.SideRun) SetState(CowState.SideRun);
//        if (!moving && currentState == CowState.SideRun) SetState(CowState.Idle);
//    }

//    // ─────────────────────────────────────────────────────────
//    // Public methods
//    // ─────────────────────────────────────────────────────────
//    public void PlayIdle() { if (!isAlive) return; SetState(CowState.Idle); }
//    public void PlayJump() { if (!isAlive || currentState == CowState.Win) return; SetState(CowState.Jump); }
//    public void PlayHit() { if (!isAlive || currentState == CowState.Hit || currentState == CowState.Win) return; SetState(CowState.Hit); }
//    public void PlayWin() { if (!isAlive) return; Debug.Log("[Cow] PlayWin called!"); SetState(CowState.Win); }

//    public void OnRightDown() => moveRight = true;
//    public void OnRightUp() => moveRight = false;
//    public void OnLeftDown() => moveLeft = true;
//    public void OnLeftUp() => moveLeft = false;

//    // ─────────────────────────────────────────────────────────
//    void SetState(CowState newState)
//    {
//        if (!isAlive) return;
//        currentState = newState;
//        Debug.Log($"[Cow] State → {newState}");
//        StopAllCoroutines();
//        StartCoroutine(MasterLoop());
//    }

//    IEnumerator MasterLoop()
//    {
//        int frame = 0;
//        while (true)
//        {
//            if (!isAlive) yield break;

//            Sprite[] frames = null;
//            float fps = 12f;

//            switch (currentState)
//            {
//                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//                case CowState.SideRun: frames = sideRunFrames; fps = sideRunFPS; break;
//                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//                case CowState.Win: frames = winFrames; fps = winFPS; break;
//            }

//            if (frames == null || frames.Length == 0)
//            {
//                Debug.LogError($"[Cow] {currentState} frames are EMPTY!");
//                yield return new WaitForSeconds(0.1f);
//                continue;
//            }

//            if (frame >= frames.Length) frame = 0;

//            cowImage.sprite = frames[frame];
//            cowRect.sizeDelta = originalSize;
//            frame++;

//            if (currentState == CowState.Hit && frame >= frames.Length)
//            { frame = 0; currentState = CowState.Idle; }

//            if (currentState == CowState.Jump && frame >= frames.Length)
//            { frame = 0; currentState = CowState.Idle; }

//            if (frame >= frames.Length) frame = 0;

//            yield return new WaitForSeconds(1f / fps);
//        }
//    }
//}


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CowAnimationController : MonoBehaviour
{
    public enum CowState { Idle, SideRun, Jump, Hit, Win }

    [Header("Movement")]
    public float moveSpeed = 200f;
    public float minX = 0f;       // world left limit
    public float maxX = 9000f;    // world right limit

    [Header("Idle Animation")]
    public Sprite[] idleFrames;
    public float idleFPS = 12f;

    [Header("Side Run Animation")]
    public Sprite[] sideRunFrames;
    public float sideRunFPS = 12f;

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
    private CowState currentState = CowState.Idle;
    private bool isAlive = false;

    private bool moveLeft = false;
    private bool moveRight = false;

    // ✅ worldX is the cow's TRUE logical position
    // BackgroundScroller reads this and decides where to draw the cow on screen
    [HideInInspector] public float worldX = 0f;

    // ─────────────────────────────────────────────────────────
    void Start()
    {
        isAlive = true;
        cowImage = GetComponent<Image>();
        cowRect = GetComponent<RectTransform>();
        originalSize = cowRect.sizeDelta;
        worldX = cowRect.anchoredPosition.x;   // start at whatever position it's placed
        SetState(CowState.Idle);
    }

    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
    void OnDisable() { isAlive = false; StopAllCoroutines(); }
    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

    void Update()
    {
        if (!isAlive) return;
        if (currentState == CowState.Win) return;

        HandleMovement();
        HandleAnimationSwitch();
    }

    // ─────────────────────────────────────────────────────────
    // Only updates worldX — BackgroundScroller sets anchoredPosition
    // ─────────────────────────────────────────────────────────
    void HandleMovement()
    {
        bool left = moveLeft || Input.GetKey(KeyCode.LeftArrow);
        bool right = moveRight || Input.GetKey(KeyCode.RightArrow);

        if (right)
        {
            worldX = Mathf.Min(worldX + moveSpeed * Time.deltaTime, maxX);
            cowRect.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (left)
        {
            worldX = Mathf.Max(worldX - moveSpeed * Time.deltaTime, minX);
            cowRect.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    void HandleAnimationSwitch()
    {
        if (currentState == CowState.Hit) return;
        if (currentState == CowState.Jump) return;

        bool moving = moveLeft || moveRight ||
                      Input.GetKey(KeyCode.LeftArrow) ||
                      Input.GetKey(KeyCode.RightArrow);

        if (moving && currentState != CowState.SideRun) SetState(CowState.SideRun);
        if (!moving && currentState == CowState.SideRun) SetState(CowState.Idle);
    }

    // ─────────────────────────────────────────────────────────
    // Public methods
    // ─────────────────────────────────────────────────────────
    public void PlayIdle() { if (!isAlive) return; SetState(CowState.Idle); }
    public void PlayJump() { if (!isAlive || currentState == CowState.Win) return; SetState(CowState.Jump); }
    public void PlayHit() { if (!isAlive || currentState == CowState.Hit || currentState == CowState.Win) return; SetState(CowState.Hit); }
    public void PlayWin() { if (!isAlive) return; Debug.Log("[Cow] PlayWin called!"); SetState(CowState.Win); }

    public void OnRightDown() => moveRight = true;
    public void OnRightUp() => moveRight = false;
    public void OnLeftDown() => moveLeft = true;
    public void OnLeftUp() => moveLeft = false;

    // ─────────────────────────────────────────────────────────
    void SetState(CowState newState)
    {
        if (!isAlive) return;
        currentState = newState;
        Debug.Log($"[Cow] State → {newState}");
        StopAllCoroutines();
        StartCoroutine(MasterLoop());
    }

    IEnumerator MasterLoop()
    {
        int frame = 0;
        while (true)
        {
            if (!isAlive) yield break;

            Sprite[] frames = null;
            float fps = 12f;

            switch (currentState)
            {
                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
                case CowState.SideRun: frames = sideRunFrames; fps = sideRunFPS; break;
                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
                case CowState.Win: frames = winFrames; fps = winFPS; break;
            }

            if (frames == null || frames.Length == 0)
            {
                Debug.LogError($"[Cow] {currentState} frames are EMPTY!");
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (frame >= frames.Length) frame = 0;

            cowImage.sprite = frames[frame];
            cowRect.sizeDelta = originalSize;
            frame++;

            if (currentState == CowState.Hit && frame >= frames.Length)
            { frame = 0; currentState = CowState.Idle; }

            if (currentState == CowState.Jump && frame >= frames.Length)
            { frame = 0; currentState = CowState.Idle; }

            if (frame >= frames.Length) frame = 0;

            yield return new WaitForSeconds(1f / fps);
        }
    }
}