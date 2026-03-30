//////using UnityEngine;
//////using UnityEngine.UI;
//////using System.Collections;

//////public class Cowanimationcontroller : MonoBehaviour
//////{
//////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////    [Header("Auto Run")]
//////    public float runSpeed = 200f;

//////    [Header("Jump Physics")]
//////    public float jumpHeight = 250f;  // pixels — how high
//////    public float jumpDuration = 0.7f;  // seconds — time in air
//////    public float jumpForwardSpeed = 400f; // pixels/sec forward during jump
//////    public int maxJumps = 2;     // 1 = single jump, 2 = double jump

//////    [Header("Jump Size")]
//////    [Tooltip("1 = same size as run/idle.  1.3 = 30% bigger.  Drag to match your jump sprites.")]
//////    [Range(0.5f, 3f)]
//////    public float jumpScale = 1f;

//////    [Header("Idle Animation")]
//////    public Sprite[] idleFrames;
//////    public float idleFPS = 12f;

//////    [Header("Run Animation")]
//////    public Sprite[] runFrames;
//////    public float runFPS = 12f;

//////    [Header("Jump Animation")]
//////    public Sprite[] jumpFrames;
//////    public float jumpFPS = 12f;

//////    [Header("Hit Animation")]
//////    public Sprite[] hitFrames;
//////    public float hitFPS = 12f;

//////    [Header("Win / Celebration Animation")]
//////    public Sprite[] winFrames;
//////    public float winFPS = 12f;

//////    // ── Private ────────────────────────────────────────────────
//////    private Image cowImage;
//////    private RectTransform cowRect;
//////    private Vector2 originalSize;
//////    private bool isAlive = false;
//////    private bool hitThenWin = false;
//////    private float groundY;
//////    private bool isJumping = false;
//////    private int jumpsLeft = 0;

//////    private Coroutine animLoop = null;
//////    private Coroutine jumpArcCo = null;

//////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////    [HideInInspector] public float worldX = 0f;
//////    [HideInInspector] public bool isRunning = false;

//////    // ──────────────────────────────────────────────────────────
//////    void Start()
//////    {
//////        isAlive = true;
//////        cowImage = GetComponent<Image>();
//////        cowRect = GetComponent<RectTransform>();
//////        originalSize = cowRect.sizeDelta;
//////        worldX = cowRect.anchoredPosition.x;
//////        groundY = cowRect.anchoredPosition.y;
//////        SetState(CowState.Idle);
//////    }

//////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////    // ──────────────────────────────────────────────────────────
//////    void Update()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Hit) return;

//////        if (isRunning)
//////        {
//////            if (!isJumping)
//////                worldX += runSpeed * Time.deltaTime;

//////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////                PlayJump();
//////        }
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    public void StartRunning()
//////    {
//////        isRunning = true;
//////        jumpsLeft = maxJumps;
//////        SetState(CowState.Run);
//////        Debug.Log("[Cow] StartRunning!");
//////    }

//////    public void StopRunning()
//////    {
//////        isRunning = false;
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    public void PlayJump()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Hit) return;
//////        if (jumpsLeft <= 0) return;

//////        jumpsLeft--;
//////        Debug.Log($"[Cow] Jump! Jumps left: {jumpsLeft}");

//////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
//////        jumpArcCo = StartCoroutine(JumpArc());

//////        SetState(CowState.Jump);
//////    }

//////    public void OnJumpPressed() => PlayJump();
//////    public void OnJumpDown() => PlayJump();

//////    // ──────────────────────────────────────────────────────────
//////    // Called by ObstacleController when cow hits an obstacle
//////    // ──────────────────────────────────────────────────────────
//////    public void PlayHit()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Hit) return;
//////        if (CurrentState == CowState.Win) return;

//////        // Stop any jump in progress
//////        if (jumpArcCo != null)
//////        {
//////            StopCoroutine(jumpArcCo);
//////            jumpArcCo = null;
//////        }
//////        isJumping = false;
//////        isRunning = false;

//////        hitThenWin = false;
//////        SetState(CowState.Hit);
//////    }

//////    // Called by BellTrigger — Hit once then Win celebration
//////    public void PlayHitThenWin()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;

//////        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
//////        isJumping = false;
//////        isRunning = false;
//////        hitThenWin = true;
//////        SetState(CowState.Hit);
//////        Debug.Log("[Cow] PlayHitThenWin!");
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // Called by BellTrigger after waiting for Hit animation to finish.
//////    // Transitions directly to Win (celebration) animation.
//////    // ──────────────────────────────────────────────────────────
//////    public void PlayWinDirect()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;

//////        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
//////        isJumping = false;
//////        isRunning = false;
//////        hitThenWin = false;
//////        SetState(CowState.Win);
//////        Debug.Log("[Cow] PlayWinDirect — celebration!");
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // Jump arc — parabola from current Y back to groundY
//////    // ──────────────────────────────────────────────────────────
//////    IEnumerator JumpArc()
//////    {
//////        isJumping = true;

//////        float elapsed = 0f;
//////        float startY = cowRect.anchoredPosition.y;

//////        while (elapsed < jumpDuration)
//////        {
//////            if (!isAlive) yield break;
//////            if (CurrentState == CowState.Hit) yield break; // stop arc on hit

//////            elapsed += Time.deltaTime;
//////            float t = Mathf.Clamp01(elapsed / jumpDuration);

//////            worldX += jumpForwardSpeed * Time.deltaTime;

//////            // Parabola: startY → peak → groundY
//////            float height = Mathf.Lerp(startY, groundY, t) + jumpHeight * 4f * t * (1f - t);
//////            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, height);

//////            yield return null;
//////        }

//////        // Snap to ground and reset
//////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//////        isJumping = false;
//////        jumpsLeft = maxJumps;
//////        jumpArcCo = null;

//////        if (CurrentState == CowState.Jump)
//////            SetState(isRunning ? CowState.Run : CowState.Idle);

//////        Debug.Log("[Cow] Landed!");
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    void SetState(CowState newState)
//////    {
//////        if (!isAlive) return;
//////        CurrentState = newState;
//////        Debug.Log($"[Cow] → {newState}");

//////        if (animLoop != null) StopCoroutine(animLoop);
//////        animLoop = StartCoroutine(MasterLoop());
//////    }

//////    IEnumerator MasterLoop()
//////    {
//////        int frame = 0;
//////        while (true)
//////        {
//////            if (!isAlive) yield break;

//////            Sprite[] frames = null;
//////            float fps = 12f;

//////            switch (CurrentState)
//////            {
//////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////            }

//////            if (frames == null || frames.Length == 0)
//////            {
//////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////                yield return new WaitForSeconds(0.1f);
//////                continue;
//////            }

//////            if (frame >= frames.Length) frame = 0;

//////            cowImage.sprite = frames[frame];

//////            //// Apply jumpScale only during Jump
//////            //cowRect.sizeDelta = (CurrentState == CowState.Jump)
//////            //    ? originalSize * jumpScale
//////            //    : originalSize;

//////            // Apply jumpScale during Jump AND Win
//////            cowRect.sizeDelta = (CurrentState == CowState.Jump || CurrentState == CowState.Win)
//////                ? originalSize * jumpScale
//////                : originalSize;

//////            frame++;

//////            // Jump → plays once → handled by JumpArc landing
//////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                // Keep looping jump frames until JumpArc finishes
//////            }

//////            // Hit → plays once → Win or Idle
//////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                if (hitThenWin)
//////                {
//////                    hitThenWin = false;
//////                    CurrentState = CowState.Win;
//////                    Debug.Log("[Cow] → Celebration!");
//////                }
//////                else
//////                {
//////                    // Game over — stay in Hit / Idle, GameManager handles panel
//////                    CurrentState = CowState.Idle;
//////                }
//////            }

//////            if (frame >= frames.Length) frame = 0;

//////            yield return new WaitForSeconds(1f / fps);
//////        }
//////    }
//////}
////////////////////using UnityEngine;
////////////////////using UnityEngine.UI;
////////////////////using System.Collections;

////////////////////public class Cowanimationcontroller : MonoBehaviour
////////////////////{
////////////////////    public enum CowState { Idle, SideRun, Jump, Hit, Win }

////////////////////    [Header("Movement")]
////////////////////    public float moveSpeed = 500f;

////////////////////    [Header("Animations")]
////////////////////    public Sprite[] idleFrames;
////////////////////    public Sprite[] runFrames;
////////////////////    public Sprite[] jumpFrames;
////////////////////    public Sprite[] hitFrames;
////////////////////    public Sprite[] winFrames;

////////////////////    public float fps = 12f;

////////////////////    private Image cowImage;
////////////////////    private RectTransform cowRect;

////////////////////    private CowState currentState;

////////////////////    private bool isRunning = false;
////////////////////    private bool isJumping = false;   // ✅ FIX: prevent loop

////////////////////    [HideInInspector] public float worldX = 0f;

////////////////////    void Start()
////////////////////    {
////////////////////        cowImage = GetComponent<Image>();
////////////////////        cowRect = GetComponent<RectTransform>();

////////////////////        worldX = cowRect.anchoredPosition.x;

////////////////////        SetState(CowState.Idle);
////////////////////    }

////////////////////    void Update()
////////////////////    {
////////////////////        if (!isRunning) return;

////////////////////        // ✅ AUTO RUN
////////////////////        worldX += moveSpeed * Time.deltaTime;

////////////////////        // ✅ KEYBOARD JUMP
////////////////////        if (Input.GetKeyDown(KeyCode.Space))
////////////////////            PlayJump();
////////////////////    }

////////////////////    // =========================
////////////////////    // GAME CONTROL
////////////////////    // =========================
////////////////////    public void StartRunning()
////////////////////    {
////////////////////        isRunning = true;
////////////////////        SetState(CowState.SideRun);
////////////////////    }

////////////////////    public void StopRunning()
////////////////////    {
////////////////////        isRunning = false;
////////////////////    }

////////////////////    // =========================
////////////////////    // ACTIONS
////////////////////    // =========================
////////////////////    public void PlayJump()
////////////////////    {
////////////////////        if (isJumping) return;              // 🚫 prevent loop
////////////////////        if (currentState == CowState.Win) return;

////////////////////        isJumping = true;
////////////////////        SetState(CowState.Jump);
////////////////////    }

////////////////////    public void PlayHit()
////////////////////    {
////////////////////        if (currentState == CowState.Win) return;
////////////////////        SetState(CowState.Hit);
////////////////////    }

////////////////////    public void PlayWin()
////////////////////    {
////////////////////        StopRunning();
////////////////////        SetState(CowState.Win);

////////////////////        GameManager.Instance?.SendMessage("ShowCongrats");
////////////////////    }

////////////////////    public void PlayHitThenWin()
////////////////////    {
////////////////////        StartCoroutine(HitThenWinRoutine());
////////////////////    }

////////////////////    IEnumerator HitThenWinRoutine()
////////////////////    {
////////////////////        SetState(CowState.Hit);
////////////////////        yield return new WaitForSeconds(0.5f);
////////////////////        PlayWin();
////////////////////    }

////////////////////    // UI Button
////////////////////    public void OnJumpDown()
////////////////////    {
////////////////////        PlayJump();
////////////////////    }

////////////////////    // =========================
////////////////////    void SetState(CowState state)
////////////////////    {
////////////////////        currentState = state;
////////////////////        StopAllCoroutines();
////////////////////        StartCoroutine(Animate());
////////////////////    }

////////////////////    IEnumerator Animate()
////////////////////    {
////////////////////        Sprite[] frames = idleFrames;

////////////////////        switch (currentState)
////////////////////        {
////////////////////            case CowState.Idle: frames = idleFrames; break;
////////////////////            case CowState.SideRun: frames = runFrames; break;
////////////////////            case CowState.Jump: frames = jumpFrames; break;
////////////////////            case CowState.Hit: frames = hitFrames; break;
////////////////////            case CowState.Win: frames = winFrames; break;
////////////////////        }

////////////////////        int i = 0;

////////////////////        while (true)
////////////////////        {
////////////////////            if (frames == null || frames.Length == 0)
////////////////////                yield break;

////////////////////            cowImage.sprite = frames[i];
////////////////////            i++;

////////////////////            // ✅ JUMP END FIX
////////////////////            if (currentState == CowState.Jump && i >= frames.Length)
////////////////////            {
////////////////////                isJumping = false;                 // 🔥 unlock jump
////////////////////                SetState(CowState.SideRun);        // back to run
////////////////////                yield break;
////////////////////            }

////////////////////            // ✅ HIT END
////////////////////            if (currentState == CowState.Hit && i >= frames.Length)
////////////////////            {
////////////////////                SetState(CowState.SideRun);
////////////////////                yield break;
////////////////////            }

////////////////////            // LOOP states
////////////////////            if (i >= frames.Length)
////////////////////                i = 0;

////////////////////            yield return new WaitForSeconds(1f / fps);
////////////////////        }
////////////////////    }
////////////////////}

//////////////////using UnityEngine;
//////////////////using UnityEngine.UI;
//////////////////using System.Collections;

//////////////////public class Cowanimationcontroller : MonoBehaviour
//////////////////{
//////////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////////////    [Header("Auto Run")]
//////////////////    public float runSpeed = 200f;

//////////////////    [Header("Idle Animation")]
//////////////////    public Sprite[] idleFrames;
//////////////////    public float idleFPS = 12f;

//////////////////    [Header("Run Animation")]
//////////////////    public Sprite[] runFrames;
//////////////////    public float runFPS = 12f;

//////////////////    [Header("Jump Animation")]
//////////////////    public Sprite[] jumpFrames;
//////////////////    public float jumpFPS = 12f;

//////////////////    [Header("Hit Animation")]
//////////////////    public Sprite[] hitFrames;
//////////////////    public float hitFPS = 12f;

//////////////////    [Header("Win / Celebration Animation")]
//////////////////    public Sprite[] winFrames;
//////////////////    public float winFPS = 12f;

//////////////////    private Image cowImage;
//////////////////    private RectTransform cowRect;
//////////////////    private Vector2 originalSize;
//////////////////    private bool isAlive = false;
//////////////////    private bool hitThenWin = false;

//////////////////    // ✅ PUBLIC — BellTrigger reads this to check if cow is jumping
//////////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////////////    [HideInInspector] public float worldX = 0f;
//////////////////    [HideInInspector] public bool isRunning = false;

//////////////////    // ──────────────────────────────────────────────────────────
//////////////////    void Start()
//////////////////    {
//////////////////        isAlive = true;
//////////////////        cowImage = GetComponent<Image>();
//////////////////        cowRect = GetComponent<RectTransform>();
//////////////////        originalSize = cowRect.sizeDelta;
//////////////////        worldX = cowRect.anchoredPosition.x;
//////////////////        SetState(CowState.Idle);

//////////////////        if (GameManager.Instance != null)
//////////////////        {
//////////////////            GameManager.Instance.OnGameStart += StartRunning;
//////////////////            GameManager.Instance.OnGameWin += StopRunning;
//////////////////        }
//////////////////    }

//////////////////    void OnDestroy()
//////////////////    {
//////////////////        isAlive = false;
//////////////////        StopAllCoroutines();
//////////////////        if (GameManager.Instance != null)
//////////////////        {
//////////////////            GameManager.Instance.OnGameStart -= StartRunning;
//////////////////            GameManager.Instance.OnGameWin -= StopRunning;
//////////////////        }
//////////////////    }

//////////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////////////    // ──────────────────────────────────────────────────────────
//////////////////    void Update()
//////////////////    {
//////////////////        if (!isAlive) return;
//////////////////        if (CurrentState == CowState.Win) return;
//////////////////        if (CurrentState == CowState.Hit) return;

//////////////////        if (isRunning)
//////////////////        {
//////////////////            worldX += runSpeed * Time.deltaTime;

//////////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////////////                PlayJump();
//////////////////        }
//////////////////    }

//////////////////    void StartRunning() { isRunning = true; SetState(CowState.Run); }
//////////////////    void StopRunning() { isRunning = false; }

//////////////////    // ──────────────────────────────────────────────────────────
//////////////////    public void PlayJump()
//////////////////    {
//////////////////        if (!isAlive) return;
//////////////////        if (CurrentState == CowState.Win) return;
//////////////////        if (CurrentState == CowState.Jump) return;
//////////////////        if (CurrentState == CowState.Hit) return;
//////////////////        SetState(CowState.Jump);
//////////////////    }

//////////////////    public void PlayHit()
//////////////////    {
//////////////////        if (!isAlive) return;
//////////////////        if (CurrentState == CowState.Hit) return;
//////////////////        if (CurrentState == CowState.Win) return;
//////////////////        hitThenWin = false;
//////////////////        SetState(CowState.Hit);
//////////////////    }

//////////////////    // ✅ Called by BellTrigger — Hit once → then Celebration loops
//////////////////    public void PlayHitThenWin()
//////////////////    {
//////////////////        if (!isAlive) return;
//////////////////        if (CurrentState == CowState.Win) return;
//////////////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////////////        hitThenWin = true;
//////////////////        isRunning = false;
//////////////////        SetState(CowState.Hit);
//////////////////    }

//////////////////    public void OnJumpPressed() => PlayJump();

//////////////////    // ──────────────────────────────────────────────────────────
//////////////////    void SetState(CowState newState)
//////////////////    {
//////////////////        if (!isAlive) return;
//////////////////        CurrentState = newState;
//////////////////        Debug.Log($"[Cow] → {newState}");
//////////////////        StopAllCoroutines();
//////////////////        StartCoroutine(MasterLoop());
//////////////////    }

//////////////////    IEnumerator MasterLoop()
//////////////////    {
//////////////////        int frame = 0;
//////////////////        while (true)
//////////////////        {
//////////////////            if (!isAlive) yield break;

//////////////////            Sprite[] frames = null;
//////////////////            float fps = 12f;

//////////////////            switch (CurrentState)
//////////////////            {
//////////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////////////            }

//////////////////            if (frames == null || frames.Length == 0)
//////////////////            {
//////////////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////////////////                yield return new WaitForSeconds(0.1f);
//////////////////                continue;
//////////////////            }

//////////////////            if (frame >= frames.Length) frame = 0;
//////////////////            cowImage.sprite = frames[frame];
//////////////////            cowRect.sizeDelta = originalSize;
//////////////////            frame++;

//////////////////            // Jump → ONCE → back to Run
//////////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////////////            {
//////////////////                frame = 0;
//////////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////////            }

//////////////////            // Hit → ONCE → Win or Run
//////////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////////////            {
//////////////////                frame = 0;
//////////////////                if (hitThenWin)
//////////////////                {
//////////////////                    hitThenWin = false;
//////////////////                    CurrentState = CowState.Win;
//////////////////                    Debug.Log("[Cow] Hit done → Celebration!");
//////////////////                }
//////////////////                else
//////////////////                {
//////////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////////                }
//////////////////            }

//////////////////            // Idle / Run / Win → loop forever
//////////////////            if (frame >= frames.Length) frame = 0;

//////////////////            yield return new WaitForSeconds(1f / fps);
//////////////////        }
//////////////////    }
//////////////////}
////////////////using UnityEngine;
////////////////using UnityEngine.UI;
////////////////using System.Collections;

////////////////public class Cowanimationcontroller : MonoBehaviour
////////////////{
////////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////////////    [Header("Auto Run")]
////////////////    public float runSpeed = 200f;

////////////////    [Header("Idle Animation")]
////////////////    public Sprite[] idleFrames;
////////////////    public float idleFPS = 12f;

////////////////    [Header("Run Animation")]
////////////////    public Sprite[] runFrames;
////////////////    public float runFPS = 12f;

////////////////    [Header("Jump Animation")]
////////////////    public Sprite[] jumpFrames;
////////////////    public float jumpFPS = 12f;

////////////////    [Header("Hit Animation")]
////////////////    public Sprite[] hitFrames;
////////////////    public float hitFPS = 12f;

////////////////    [Header("Win / Celebration Animation")]
////////////////    public Sprite[] winFrames;
////////////////    public float winFPS = 12f;

////////////////    private Image cowImage;
////////////////    private RectTransform cowRect;
////////////////    private Vector2 originalSize;
////////////////    private bool isAlive = false;
////////////////    private bool hitThenWin = false;

////////////////    // ✅ PUBLIC — GameManager calls StartRunning(), BellTrigger reads CurrentState
////////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////////////    [HideInInspector] public float worldX = 0f;
////////////////    [HideInInspector] public bool isRunning = false;

////////////////    // ──────────────────────────────────────────────────────────
////////////////    void Start()
////////////////    {
////////////////        isAlive = true;
////////////////        cowImage = GetComponent<Image>();
////////////////        cowRect = GetComponent<RectTransform>();
////////////////        originalSize = cowRect.sizeDelta;
////////////////        worldX = cowRect.anchoredPosition.x;
////////////////        SetState(CowState.Idle);
////////////////    }

////////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////////////    // ──────────────────────────────────────────────────────────
////////////////    void Update()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        if (CurrentState == CowState.Hit) return;

////////////////        if (isRunning)
////////////////        {
////////////////            worldX += runSpeed * Time.deltaTime;

////////////////            // Jump — Space or Up Arrow
////////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////////////                PlayJump();
////////////////        }
////////////////    }

////////////////    // ──────────────────────────────────────────────────────────
////////////////    // ✅ Called by GameManager.CountdownRoutine() after GO!!
////////////////    // ──────────────────────────────────────────────────────────
////////////////    public void StartRunning()
////////////////    {
////////////////        isRunning = true;
////////////////        SetState(CowState.Run);
////////////////        Debug.Log("[Cow] StartRunning called by GameManager!");
////////////////    }

////////////////    public void StopRunning()
////////////////    {
////////////////        isRunning = false;
////////////////    }

////////////////    // ──────────────────────────────────────────────────────────
////////////////    // Public methods
////////////////    // ──────────────────────────────────────────────────────────
////////////////    public void PlayIdle()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        SetState(CowState.Idle);
////////////////    }

////////////////    public void PlayJump()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        if (CurrentState == CowState.Jump) return;
////////////////        if (CurrentState == CowState.Hit) return;
////////////////        SetState(CowState.Jump);
////////////////    }

////////////////    public void PlayHit()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Hit) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        hitThenWin = false;
////////////////        SetState(CowState.Hit);
////////////////    }

////////////////    // ✅ Called by BellTrigger — Hit once → Celebration loops
////////////////    public void PlayHitThenWin()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        Debug.Log("[Cow] PlayHitThenWin!");
////////////////        hitThenWin = true;
////////////////        isRunning = false;
////////////////        SetState(CowState.Hit);
////////////////    }

////////////////    // On-screen jump button
////////////////    public void OnJumpPressed() => PlayJump();

////////////////    // ──────────────────────────────────────────────────────────
////////////////    void SetState(CowState newState)
////////////////    {
////////////////        if (!isAlive) return;
////////////////        CurrentState = newState;
////////////////        Debug.Log($"[Cow] → {newState}");
////////////////        StopAllCoroutines();
////////////////        StartCoroutine(MasterLoop());
////////////////    }

////////////////    IEnumerator MasterLoop()
////////////////    {
////////////////        int frame = 0;
////////////////        while (true)
////////////////        {
////////////////            if (!isAlive) yield break;

////////////////            Sprite[] frames = null;
////////////////            float fps = 12f;

////////////////            switch (CurrentState)
////////////////            {
////////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////////////            }

////////////////            if (frames == null || frames.Length == 0)
////////////////            {
////////////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////////////////                yield return new WaitForSeconds(0.1f);
////////////////                continue;
////////////////            }

////////////////            if (frame >= frames.Length) frame = 0;
////////////////            cowImage.sprite = frames[frame];
////////////////            cowRect.sizeDelta = originalSize;
////////////////            frame++;

////////////////            // Jump → ONCE → back to Run
////////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////////////            {
////////////////                frame = 0;
////////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////////            }

////////////////            // Hit → ONCE → Win or Run
////////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////////////            {
////////////////                frame = 0;
////////////////                if (hitThenWin)
////////////////                {
////////////////                    hitThenWin = false;
////////////////                    CurrentState = CowState.Win;
////////////////                    Debug.Log("[Cow] → Celebration!");
////////////////                }
////////////////                else
////////////////                {
////////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////////                }
////////////////            }

////////////////            // Idle / Run / Win → loop forever
////////////////            if (frame >= frames.Length) frame = 0;

////////////////            yield return new WaitForSeconds(1f / fps);
////////////////        }
////////////////    }
////////////////}
//////////////using UnityEngine;
//////////////using UnityEngine.UI;
//////////////using System.Collections;

//////////////public class Cowanimationcontroller : MonoBehaviour
//////////////{
//////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////////    [Header("Auto Run")]
//////////////    public float runSpeed = 200f;

//////////////    [Header("Idle Animation")]
//////////////    public Sprite[] idleFrames;
//////////////    public float idleFPS = 12f;

//////////////    [Header("Run Animation")]
//////////////    public Sprite[] runFrames;
//////////////    public float runFPS = 12f;

//////////////    [Header("Jump Animation")]
//////////////    public Sprite[] jumpFrames;
//////////////    public float jumpFPS = 12f;

//////////////    [Header("Hit Animation")]
//////////////    public Sprite[] hitFrames;
//////////////    public float hitFPS = 12f;

//////////////    [Header("Win / Celebration Animation")]
//////////////    public Sprite[] winFrames;
//////////////    public float winFPS = 12f;

//////////////    private Image cowImage;
//////////////    private RectTransform cowRect;
//////////////    private Vector2 originalSize;
//////////////    private bool isAlive = false;
//////////////    private bool hitThenWin = false;

//////////////    // BellTrigger reads CurrentState to know if cow is jumping
//////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////////    [HideInInspector] public float worldX = 0f;
//////////////    [HideInInspector] public bool isRunning = false;

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Start()
//////////////    {
//////////////        isAlive = true;
//////////////        cowImage = GetComponent<Image>();
//////////////        cowRect = GetComponent<RectTransform>();
//////////////        originalSize = cowRect.sizeDelta;
//////////////        worldX = cowRect.anchoredPosition.x;
//////////////        SetState(CowState.Idle);
//////////////    }

//////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Update()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Hit) return;

//////////////        if (isRunning)
//////////////        {
//////////////            worldX += runSpeed * Time.deltaTime;

//////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////////                PlayJump();
//////////////        }
//////////////    }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    // Called by GameManager countdown after "GO!!"
//////////////    // ──────────────────────────────────────────────────────────
//////////////    public void StartRunning()
//////////////    {
//////////////        isRunning = true;
//////////////        SetState(CowState.Run);
//////////////        Debug.Log("[Cow] StartRunning!");
//////////////    }

//////////////    public void StopRunning()
//////////////    {
//////////////        isRunning = false;
//////////////    }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    // Public actions
//////////////    // ──────────────────────────────────────────────────────────
//////////////    public void PlayIdle()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        SetState(CowState.Idle);
//////////////    }

//////////////    public void PlayJump()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Jump) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        Debug.Log("[Cow] PlayJump!");
//////////////        SetState(CowState.Jump);
//////////////    }

//////////////    // ── UI Button hooks ────────────────────────────────────────
//////////////    // Both names wired here so either old or new button reference works
//////////////    public void OnJumpPressed() => PlayJump();
//////////////    public void OnJumpDown() => PlayJump(); // legacy alias — keep button working

//////////////    // ──────────────────────────────────────────────────────────
//////////////    public void PlayHit()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        hitThenWin = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    // Called by BellTrigger: plays Hit once, then loops Win/celebration
//////////////    public void PlayHitThenWin()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////////        hitThenWin = true;
//////////////        isRunning = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void SetState(CowState newState)
//////////////    {
//////////////        if (!isAlive) return;
//////////////        CurrentState = newState;
//////////////        Debug.Log($"[Cow] State → {newState}");
//////////////        StopAllCoroutines();
//////////////        StartCoroutine(MasterLoop());
//////////////    }

//////////////    IEnumerator MasterLoop()
//////////////    {
//////////////        int frame = 0;
//////////////        while (true)
//////////////        {
//////////////            if (!isAlive) yield break;

//////////////            Sprite[] frames = null;
//////////////            float fps = 12f;

//////////////            switch (CurrentState)
//////////////            {
//////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////////            }

//////////////            if (frames == null || frames.Length == 0)
//////////////            {
//////////////                Debug.LogError($"[Cow] {CurrentState} frames are EMPTY — assign sprites in Inspector!");
//////////////                yield return new WaitForSeconds(0.1f);
//////////////                continue;
//////////////            }

//////////////            if (frame >= frames.Length) frame = 0;
//////////////            cowImage.sprite = frames[frame];
//////////////            cowRect.sizeDelta = originalSize;
//////////////            frame++;

//////////////            // Jump plays ONCE then returns to Run
//////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////            }

//////////////            // Hit plays ONCE then either Win or Run
//////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                if (hitThenWin)
//////////////                {
//////////////                    hitThenWin = false;
//////////////                    CurrentState = CowState.Win;
//////////////                    Debug.Log("[Cow] Hit done → Win/Celebration!");
//////////////                }
//////////////                else
//////////////                {
//////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////                }
//////////////            }

//////////////            // Idle / Run / Win loop forever
//////////////            if (frame >= frames.Length) frame = 0;

//////////////            yield return new WaitForSeconds(1f / fps);
//////////////        }
//////////////    }
//////////////}

//////////////using UnityEngine;
//////////////using UnityEngine.UI;
//////////////using System.Collections;

//////////////public class Cowanimationcontroller : MonoBehaviour
//////////////{
//////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////////    [Header("Auto Run")]
//////////////    public float runSpeed = 200f;

//////////////    [Header("Idle Animation")]
//////////////    public Sprite[] idleFrames;
//////////////    public float idleFPS = 12f;

//////////////    [Header("Run Animation")]
//////////////    public Sprite[] runFrames;
//////////////    public float runFPS = 12f;

//////////////    [Header("Jump Animation")]
//////////////    public Sprite[] jumpFrames;
//////////////    public float jumpFPS = 12f;

//////////////    [Header("Jump Size")]
//////////////    [Tooltip("1 = same as run/idle size.  1.3 = 30% bigger.  Try values between 1.0 and 2.0.")]
//////////////    [Range(0.5f, 3f)]
//////////////    public float jumpScale = 1f;   // ← drag this slider in the Inspector

//////////////    [Header("Hit Animation")]
//////////////    public Sprite[] hitFrames;
//////////////    public float hitFPS = 12f;

//////////////    [Header("Win / Celebration Animation")]
//////////////    public Sprite[] winFrames;
//////////////    public float winFPS = 12f;

//////////////    private Image cowImage;
//////////////    private RectTransform cowRect;
//////////////    private Vector2 originalSize;
//////////////    private bool isAlive = false;
//////////////    private bool hitThenWin = false;

//////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////////    [HideInInspector] public float worldX = 0f;
//////////////    [HideInInspector] public bool isRunning = false;

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Start()
//////////////    {
//////////////        isAlive = true;
//////////////        cowImage = GetComponent<Image>();
//////////////        cowRect = GetComponent<RectTransform>();
//////////////        originalSize = cowRect.sizeDelta;
//////////////        worldX = cowRect.anchoredPosition.x;
//////////////        SetState(CowState.Idle);
//////////////    }

//////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Update()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Hit) return;

//////////////        if (isRunning)
//////////////        {
//////////////            worldX += runSpeed * Time.deltaTime;

//////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////////                PlayJump();
//////////////        }
//////////////    }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    public void StartRunning()
//////////////    {
//////////////        isRunning = true;
//////////////        SetState(CowState.Run);
//////////////        Debug.Log("[Cow] StartRunning!");
//////////////    }

//////////////    public void StopRunning()
//////////////    {
//////////////        isRunning = false;
//////////////    }

//////////////    public void PlayJump()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Jump) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        Debug.Log("[Cow] Jump!");
//////////////        SetState(CowState.Jump);
//////////////    }

//////////////    public void OnJumpPressed() => PlayJump();
//////////////    public void OnJumpDown() => PlayJump();

//////////////    public void PlayHit()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        hitThenWin = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    public void PlayHitThenWin()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////////        hitThenWin = true;
//////////////        isRunning = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void SetState(CowState newState)
//////////////    {
//////////////        if (!isAlive) return;
//////////////        CurrentState = newState;
//////////////        Debug.Log($"[Cow] → {newState}");
//////////////        StopAllCoroutines();
//////////////        StartCoroutine(MasterLoop());
//////////////    }

//////////////    IEnumerator MasterLoop()
//////////////    {
//////////////        int frame = 0;
//////////////        while (true)
//////////////        {
//////////////            if (!isAlive) yield break;

//////////////            Sprite[] frames = null;
//////////////            float fps = 12f;

//////////////            switch (CurrentState)
//////////////            {
//////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////////            }

//////////////            if (frames == null || frames.Length == 0)
//////////////            {
//////////////                Debug.LogError($"[Cow] {CurrentState} frames are EMPTY!");
//////////////                yield return new WaitForSeconds(0.1f);
//////////////                continue;
//////////////            }

//////////////            if (frame >= frames.Length) frame = 0;

//////////////            cowImage.sprite = frames[frame];

//////////////            // Apply jumpScale only during Jump, otherwise restore original size
//////////////            cowRect.sizeDelta = (CurrentState == CowState.Jump)
//////////////                ? originalSize * jumpScale
//////////////                : originalSize;

//////////////            frame++;

//////////////            // Jump plays once → back to Run
//////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////            }

//////////////            // Hit plays once → Win or Run
//////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                if (hitThenWin)
//////////////                {
//////////////                    hitThenWin = false;
//////////////                    CurrentState = CowState.Win;
//////////////                    Debug.Log("[Cow] → Celebration!");
//////////////                }
//////////////                else
//////////////                {
//////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////                }
//////////////            }

//////////////            // Idle / Run / Win loop forever
//////////////            if (frame >= frames.Length) frame = 0;

//////////////            yield return new WaitForSeconds(1f / fps);
//////////////        }
//////////////    }
//////////////}

////////////using UnityEngine;
////////////using UnityEngine.UI;
////////////using System.Collections;

////////////public class Cowanimationcontroller : MonoBehaviour
////////////{
////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////////    [Header("Auto Run")]
////////////    public float runSpeed = 200f;

////////////    [Header("Jump Physics")]
////////////    public float jumpHeight = 250f;   // how high the cow goes (pixels)
////////////    public float jumpDuration = 0.7f;   // total time in air (seconds)

////////////    [Header("Idle Animation")]
////////////    public Sprite[] idleFrames;
////////////    public float idleFPS = 12f;

////////////    [Header("Run Animation")]
////////////    public Sprite[] runFrames;
////////////    public float runFPS = 12f;

////////////    [Header("Jump Animation")]
////////////    public Sprite[] jumpFrames;
////////////    public float jumpFPS = 12f;

////////////    [Header("Hit Animation")]
////////////    public Sprite[] hitFrames;
////////////    public float hitFPS = 12f;

////////////    [Header("Win / Celebration Animation")]
////////////    public Sprite[] winFrames;
////////////    public float winFPS = 12f;

////////////    private Image cowImage;
////////////    private RectTransform cowRect;
////////////    private Vector2 originalSize;
////////////    private bool isAlive = false;
////////////    private bool hitThenWin = false;

////////////    // ✅ PUBLIC — GameManager calls StartRunning(), BellTrigger reads CurrentState
////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////////    [HideInInspector] public float worldX = 0f;
////////////    [HideInInspector] public bool isRunning = false;

////////////    // ✅ Ground Y — saved on Start so jump always returns to same Y
////////////    private float groundY;

////////////    // ──────────────────────────────────────────────────────────
////////////    void Start()
////////////    {
////////////        isAlive = true;
////////////        cowImage = GetComponent<Image>();
////////////        cowRect = GetComponent<RectTransform>();
////////////        originalSize = cowRect.sizeDelta;
////////////        worldX = cowRect.anchoredPosition.x;
////////////        groundY = cowRect.anchoredPosition.y;   // ✅ remember ground level
////////////        SetState(CowState.Idle);
////////////    }

////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////////    // ──────────────────────────────────────────────────────────
////////////    void Update()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Hit) return;

////////////        if (isRunning)
////////////        {
////////////            worldX += runSpeed * Time.deltaTime;

////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////////                PlayJump();
////////////        }
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    // Called by GameManager after countdown
////////////    // ──────────────────────────────────────────────────────────
////////////    public void StartRunning()
////////////    {
////////////        isRunning = true;
////////////        SetState(CowState.Run);
////////////        Debug.Log("[Cow] StartRunning!");
////////////    }

////////////    public void StopRunning() => isRunning = false;

////////////    // ──────────────────────────────────────────────────────────
////////////    // Public methods
////////////    // ──────────────────────────────────────────────────────────
////////////    public void PlayJump()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Jump) return;  // no double jump
////////////        if (CurrentState == CowState.Hit) return;
////////////        Debug.Log("[Cow] Jump!");
////////////        StartCoroutine(JumpArc());   // ✅ move cow up and down
////////////        SetState(CowState.Jump);
////////////    }

////////////    public void OnJumpPressed() => PlayJump();
////////////    public void OnJumpDown() => PlayJump();

////////////    public void PlayHit()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Hit) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        hitThenWin = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    public void PlayHitThenWin()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        Debug.Log("[Cow] PlayHitThenWin!");
////////////        hitThenWin = true;
////////////        isRunning = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    // ✅ Jump Arc — smooth parabola up and back down
////////////    // ──────────────────────────────────────────────────────────
////////////    IEnumerator JumpArc()
////////////    {
////////////        float elapsed = 0f;

////////////        while (elapsed < jumpDuration)
////////////        {
////////////            elapsed += Time.deltaTime;

////////////            // Normalised time 0→1
////////////            float t = elapsed / jumpDuration;

////////////            // Parabola: peaks at t=0.5, returns to 0 at t=1
////////////            float height = jumpHeight * 4f * t * (1f - t);

////////////            // ✅ Only change Y — X is handled by BackgroundScroller
////////////            cowRect.anchoredPosition = new Vector2(
////////////                cowRect.anchoredPosition.x,
////////////                groundY + height
////////////            );

////////////            yield return null;
////////////        }

////////////        // ✅ Snap exactly back to ground when done
////////////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    void SetState(CowState newState)
////////////    {
////////////        if (!isAlive) return;
////////////        CurrentState = newState;
////////////        Debug.Log($"[Cow] → {newState}");
////////////        StopAllCoroutines();
////////////        StartCoroutine(MasterLoop());
////////////    }

////////////    IEnumerator MasterLoop()
////////////    {
////////////        int frame = 0;
////////////        while (true)
////////////        {
////////////            if (!isAlive) yield break;

////////////            Sprite[] frames = null;
////////////            float fps = 12f;

////////////            switch (CurrentState)
////////////            {
////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////////            }

////////////            if (frames == null || frames.Length == 0)
////////////            {
////////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////////////                yield return new WaitForSeconds(0.1f);
////////////                continue;
////////////            }

////////////            if (frame >= frames.Length) frame = 0;
////////////            cowImage.sprite = frames[frame];
////////////            cowRect.sizeDelta = originalSize;
////////////            frame++;

////////////            // Jump → ONCE → back to Run
////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////            }

////////////            // Hit → ONCE → Win or Run
////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                if (hitThenWin)
////////////                {
////////////                    hitThenWin = false;
////////////                    CurrentState = CowState.Win;
////////////                    Debug.Log("[Cow] → Celebration!");
////////////                }
////////////                else
////////////                {
////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////                }
////////////            }

////////////            // Idle / Run / Win → loop forever
////////////            if (frame >= frames.Length) frame = 0;

////////////            yield return new WaitForSeconds(1f / fps);
////////////        }
////////////    }
////////////}

//////////using UnityEngine;
//////////using UnityEngine.UI;
//////////using System.Collections;

//////////public class Cowanimationcontroller : MonoBehaviour
//////////{
//////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////    [Header("Auto Run")]
//////////    public float runSpeed = 200f;

//////////    [Header("Jump Physics")]
//////////    public float jumpHeight = 250f;   // pixels — how high the cow goes
//////////    public float jumpDuration = 0.7f;   // seconds — time in the air

//////////    [Header("Idle Animation")]
//////////    public Sprite[] idleFrames;
//////////    public float idleFPS = 12f;

//////////    [Header("Run Animation")]
//////////    public Sprite[] runFrames;
//////////    public float runFPS = 12f;

//////////    [Header("Jump Animation")]
//////////    public Sprite[] jumpFrames;
//////////    public float jumpFPS = 12f;

//////////    [Header("Hit Animation")]
//////////    public Sprite[] hitFrames;
//////////    public float hitFPS = 12f;

//////////    [Header("Win / Celebration Animation")]
//////////    public Sprite[] winFrames;
//////////    public float winFPS = 12f;

//////////    // ── Private ────────────────────────────────────────────────
//////////    private Image cowImage;
//////////    private RectTransform cowRect;
//////////    private Vector2 originalSize;
//////////    private bool isAlive = false;
//////////    private bool hitThenWin = false;
//////////    private float groundY;   // Y position at ground level

//////////    // ✅ Separate coroutine references so JumpArc is never killed by StopAllCoroutines
//////////    private Coroutine animLoop = null;
//////////    private Coroutine jumpArcCo = null;

//////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////    [HideInInspector] public float worldX = 0f;
//////////    [HideInInspector] public bool isRunning = false;

//////////    // ──────────────────────────────────────────────────────────
//////////    void Start()
//////////    {
//////////        isAlive = true;
//////////        cowImage = GetComponent<Image>();
//////////        cowRect = GetComponent<RectTransform>();
//////////        originalSize = cowRect.sizeDelta;
//////////        worldX = cowRect.anchoredPosition.x;
//////////        groundY = cowRect.anchoredPosition.y;  // ✅ save ground level once
//////////        SetState(CowState.Idle);
//////////    }

//////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////    // ──────────────────────────────────────────────────────────
//////////    void Update()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Hit) return;

//////////        if (isRunning)
//////////        {
//////////            worldX += runSpeed * Time.deltaTime;

//////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////                PlayJump();
//////////        }
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    public void StartRunning()
//////////    {
//////////        isRunning = true;
//////////        SetState(CowState.Run);
//////////        Debug.Log("[Cow] StartRunning!");
//////////    }

//////////    public void StopRunning() => isRunning = false;

//////////    // ──────────────────────────────────────────────────────────
//////////    public void PlayJump()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Jump) return;
//////////        if (CurrentState == CowState.Hit) return;

//////////        Debug.Log("[Cow] Jump!");

//////////        // ✅ Stop any previous arc, start a fresh one independently
//////////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
//////////        jumpArcCo = StartCoroutine(JumpArc());

//////////        // ✅ Switch animation to Jump (only kills animLoop, NOT jumpArcCo)
//////////        SetState(CowState.Jump);
//////////    }

//////////    public void OnJumpPressed() => PlayJump();
//////////    public void OnJumpDown() => PlayJump();

//////////    public void PlayHit()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Hit) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        hitThenWin = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    public void PlayHitThenWin()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////        hitThenWin = true;
//////////        isRunning = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    // ✅ JumpArc — runs INDEPENDENTLY, never cancelled by SetState
//////////    // ──────────────────────────────────────────────────────────
//////////    IEnumerator JumpArc()
//////////    {
//////////        float elapsed = 0f;

//////////        while (elapsed < jumpDuration)
//////////        {
//////////            if (!isAlive) yield break;

//////////            elapsed += Time.deltaTime;
//////////            float t = Mathf.Clamp01(elapsed / jumpDuration);

//////////            // Smooth parabola: 0 → peak → 0
//////////            float height = jumpHeight * 4f * t * (1f - t);

//////////            // ✅ Only move Y — BackgroundScroller handles X
//////////            cowRect.anchoredPosition = new Vector2(
//////////                cowRect.anchoredPosition.x,
//////////                groundY + height
//////////            );

//////////            yield return null;
//////////        }

//////////        // ✅ Snap back to ground exactly
//////////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//////////        jumpArcCo = null;

//////////        Debug.Log("[Cow] Jump arc finished — back on ground");
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    // SetState only stops animLoop — NEVER jumpArcCo
//////////    // ──────────────────────────────────────────────────────────
//////////    void SetState(CowState newState)
//////////    {
//////////        if (!isAlive) return;
//////////        CurrentState = newState;
//////////        Debug.Log($"[Cow] → {newState}");

//////////        // ✅ Stop only the animation loop, not the jump arc
//////////        if (animLoop != null) StopCoroutine(animLoop);
//////////        animLoop = StartCoroutine(MasterLoop());
//////////    }

//////////    IEnumerator MasterLoop()
//////////    {
//////////        int frame = 0;
//////////        while (true)
//////////        {
//////////            if (!isAlive) yield break;

//////////            Sprite[] frames = null;
//////////            float fps = 12f;

//////////            switch (CurrentState)
//////////            {
//////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////            }

//////////            if (frames == null || frames.Length == 0)
//////////            {
//////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////////                yield return new WaitForSeconds(0.1f);
//////////                continue;
//////////            }

//////////            if (frame >= frames.Length) frame = 0;
//////////            cowImage.sprite = frames[frame];
//////////            cowRect.sizeDelta = originalSize;
//////////            frame++;

//////////            // Jump → ONCE → back to Run
//////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////            }

//////////            // Hit → ONCE → Win or Run
//////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                if (hitThenWin)
//////////                {
//////////                    hitThenWin = false;
//////////                    CurrentState = CowState.Win;
//////////                    Debug.Log("[Cow] → Celebration!");
//////////                }
//////////                else
//////////                {
//////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////                }
//////////            }

//////////            // Idle / Run / Win → loop forever
//////////            if (frame >= frames.Length) frame = 0;

//////////            yield return new WaitForSeconds(1f / fps);
//////////        }
//////////    }
//////////}


////////using UnityEngine;
////////using UnityEngine.UI;
////////using System.Collections;

////////public class Cowanimationcontroller : MonoBehaviour
////////{
////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////    [Header("Auto Run")]
////////    public float runSpeed = 200f;

////////    [Header("Jump Physics")]
////////    public float jumpHeight = 250f;   // pixels — how high
////////    public float jumpDuration = 0.7f;   // seconds — time in air
////////    public float jumpForwardSpeed = 400f;  // pixels/sec forward during jump (faster than runSpeed)

////////    [Header("Idle Animation")]
////////    public Sprite[] idleFrames;
////////    public float idleFPS = 12f;

////////    [Header("Run Animation")]
////////    public Sprite[] runFrames;
////////    public float runFPS = 12f;

////////    [Header("Jump Animation")]
////////    public Sprite[] jumpFrames;
////////    public float jumpFPS = 12f;

////////    [Header("Hit Animation")]
////////    public Sprite[] hitFrames;
////////    public float hitFPS = 12f;

////////    [Header("Win / Celebration Animation")]
////////    public Sprite[] winFrames;
////////    public float winFPS = 12f;

////////    // ── Private ────────────────────────────────────────────────
////////    private Image cowImage;
////////    private RectTransform cowRect;
////////    private Vector2 originalSize;
////////    private bool isAlive = false;
////////    private bool hitThenWin = false;
////////    private float groundY;
////////    private bool isJumping = false;   // ✅ blocks Update from adding runSpeed during jump

////////    private Coroutine animLoop = null;
////////    private Coroutine jumpArcCo = null;

////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////    [HideInInspector] public float worldX = 0f;
////////    [HideInInspector] public bool isRunning = false;

////////    // ──────────────────────────────────────────────────────────
////////    void Start()
////////    {
////////        isAlive = true;
////////        cowImage = GetComponent<Image>();
////////        cowRect = GetComponent<RectTransform>();
////////        originalSize = cowRect.sizeDelta;
////////        worldX = cowRect.anchoredPosition.x;
////////        groundY = cowRect.anchoredPosition.y;
////////        SetState(CowState.Idle);
////////    }

////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////    // ──────────────────────────────────────────────────────────
////////    void Update()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        if (CurrentState == CowState.Hit) return;

////////        if (isRunning && !isJumping)
////////        {
////////            // ✅ Normal run speed — JumpArc takes over worldX during jump
////////            worldX += runSpeed * Time.deltaTime;

////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////                PlayJump();
////////        }
////////        else if (isRunning && isJumping)
////////        {
////////            // ✅ Still allow jump input to be read but worldX is driven by JumpArc
////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////                Debug.Log("[Cow] Already jumping!");
////////        }
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    public void StartRunning()
////////    {
////////        isRunning = true;
////////        SetState(CowState.Run);
////////        Debug.Log("[Cow] StartRunning!");
////////    }

////////    public void StopRunning() => isRunning = false;

////////    // ──────────────────────────────────────────────────────────
////////    public void PlayJump()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        if (CurrentState == CowState.Jump) return;
////////        if (CurrentState == CowState.Hit) return;

////////        Debug.Log("[Cow] Jump!");

////////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
////////        jumpArcCo = StartCoroutine(JumpArc());

////////        SetState(CowState.Jump);
////////    }

////////    public void OnJumpPressed() => PlayJump();
////////    public void OnJumpDown() => PlayJump();

////////    public void PlayHit()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Hit) return;
////////        if (CurrentState == CowState.Win) return;
////////        hitThenWin = false;
////////        SetState(CowState.Hit);
////////    }

////////    public void PlayHitThenWin()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        Debug.Log("[Cow] PlayHitThenWin!");
////////        hitThenWin = true;
////////        isRunning = false;
////////        SetState(CowState.Hit);
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    // ✅ JumpArc — moves cow UP (Y) and FORWARD (worldX) simultaneously
////////    // ──────────────────────────────────────────────────────────
////////    IEnumerator JumpArc()
////////    {
////////        isJumping = true;   // ✅ pause Update from adding runSpeed
////////        float elapsed = 0f;

////////        while (elapsed < jumpDuration)
////////        {
////////            if (!isAlive) yield break;

////////            elapsed += Time.deltaTime;
////////            float t = Mathf.Clamp01(elapsed / jumpDuration);

////////            // ✅ Move forward faster than normal run during jump
////////            worldX += jumpForwardSpeed * Time.deltaTime;

////////            // ✅ Parabola arc upward
////////            float height = jumpHeight * 4f * t * (1f - t);
////////            cowRect.anchoredPosition = new Vector2(
////////                cowRect.anchoredPosition.x,
////////                groundY + height
////////            );

////////            yield return null;
////////        }

////////        // ✅ Snap back to ground
////////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////////        isJumping = false;
////////        jumpArcCo = null;

////////        Debug.Log("[Cow] Landed!");
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    void SetState(CowState newState)
////////    {
////////        if (!isAlive) return;
////////        CurrentState = newState;
////////        Debug.Log($"[Cow] → {newState}");

////////        if (animLoop != null) StopCoroutine(animLoop);
////////        animLoop = StartCoroutine(MasterLoop());
////////    }

////////    IEnumerator MasterLoop()
////////    {
////////        int frame = 0;
////////        while (true)
////////        {
////////            if (!isAlive) yield break;

////////            Sprite[] frames = null;
////////            float fps = 12f;

////////            switch (CurrentState)
////////            {
////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////            }

////////            if (frames == null || frames.Length == 0)
////////            {
////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////////                yield return new WaitForSeconds(0.1f);
////////                continue;
////////            }

////////            if (frame >= frames.Length) frame = 0;
////////            cowImage.sprite = frames[frame];
////////            cowRect.sizeDelta = originalSize;
////////            frame++;

////////            // Jump → ONCE → back to Run
////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////            {
////////                frame = 0;
////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////            }

////////            // Hit → ONCE → Win or Run
////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////            {
////////                frame = 0;
////////                if (hitThenWin)
////////                {
////////                    hitThenWin = false;
////////                    CurrentState = CowState.Win;
////////                    Debug.Log("[Cow] → Celebration!");
////////                }
////////                else
////////                {
////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////                }
////////            }

////////            if (frame >= frames.Length) frame = 0;

////////            yield return new WaitForSeconds(1f / fps);
////////        }
////////    }
////////}

//////using UnityEngine;
//////using UnityEngine.UI;
//////using System.Collections;

//////public class Cowanimationcontroller : MonoBehaviour
//////{
//////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////    [Header("Auto Run")]
//////    public float runSpeed = 200f;

//////    [Header("Jump Physics")]
//////    public float jumpHeight = 250f;   // pixels — how high
//////    public float jumpDuration = 0.7f;   // seconds — time in air
//////    public float jumpForwardSpeed = 400f;   // pixels/sec forward during jump
//////    public int maxJumps = 2;      // ✅ 1 = single jump, 2 = double jump

//////    [Header("Idle Animation")]
//////    public Sprite[] idleFrames;
//////    public float idleFPS = 12f;

//////    [Header("Run Animation")]
//////    public Sprite[] runFrames;
//////    public float runFPS = 12f;

//////    [Header("Jump Animation")]
//////    public Sprite[] jumpFrames;
//////    public float jumpFPS = 12f;

//////    [Header("Hit Animation")]
//////    public Sprite[] hitFrames;
//////    public float hitFPS = 12f;

//////    [Header("Win / Celebration Animation")]
//////    public Sprite[] winFrames;
//////    public float winFPS = 12f;

//////    // ── Private ────────────────────────────────────────────────
//////    private Image cowImage;
//////    private RectTransform cowRect;
//////    private Vector2 originalSize;
//////    private bool isAlive = false;
//////    private bool hitThenWin = false;
//////    private float groundY;
//////    private bool isJumping = false;

//////    // ✅ Double jump counter
//////    private int jumpsLeft = 0;

//////    private Coroutine animLoop = null;
//////    private Coroutine jumpArcCo = null;

//////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////    [HideInInspector] public float worldX = 0f;
//////    [HideInInspector] public bool isRunning = false;

//////    // ──────────────────────────────────────────────────────────
//////    void Start()
//////    {
//////        isAlive = true;
//////        cowImage = GetComponent<Image>();
//////        cowRect = GetComponent<RectTransform>();
//////        originalSize = cowRect.sizeDelta;
//////        worldX = cowRect.anchoredPosition.x;
//////        groundY = cowRect.anchoredPosition.y;
//////        SetState(CowState.Idle);
//////    }

//////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////    // ──────────────────────────────────────────────────────────
//////    void Update()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Hit) return;

//////        if (isRunning)
//////        {
//////            if (!isJumping)
//////                worldX += runSpeed * Time.deltaTime;

//////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////                PlayJump();
//////        }
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    public void StartRunning()
//////    {
//////        isRunning = true;
//////        jumpsLeft = maxJumps;   // ✅ reset on game start
//////        SetState(CowState.Run);
//////        Debug.Log("[Cow] StartRunning!");
//////    }

//////    public void StopRunning() => isRunning = false;

//////    // ──────────────────────────────────────────────────────────
//////    public void PlayJump()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Hit) return;

//////        // ✅ Allow jump only if jumpsLeft > 0
//////        if (jumpsLeft <= 0) return;

//////        jumpsLeft--;
//////        Debug.Log($"[Cow] Jump! Jumps left: {jumpsLeft}");

//////        // ✅ Restart arc from CURRENT Y position for double jump feel
//////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
//////        jumpArcCo = StartCoroutine(JumpArc());

//////        SetState(CowState.Jump);
//////    }

//////    public void OnJumpPressed() => PlayJump();
//////    public void OnJumpDown() => PlayJump();

//////    public void PlayHit()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Hit) return;
//////        if (CurrentState == CowState.Win) return;
//////        hitThenWin = false;
//////        SetState(CowState.Hit);
//////    }

//////    public void PlayHitThenWin()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        Debug.Log("[Cow] PlayHitThenWin!");
//////        hitThenWin = true;
//////        isRunning = false;
//////        SetState(CowState.Hit);
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // JumpArc — arcs from CURRENT Y so double jump stacks naturally
//////    // ──────────────────────────────────────────────────────────
//////    IEnumerator JumpArc()
//////    {
//////        isJumping = true;

//////        float elapsed = 0f;
//////        float startY = cowRect.anchoredPosition.y;   // ✅ start from wherever cow is now
//////        float peakY = startY + jumpHeight;

//////        while (elapsed < jumpDuration)
//////        {
//////            if (!isAlive) yield break;

//////            elapsed += Time.deltaTime;
//////            float t = Mathf.Clamp01(elapsed / jumpDuration);

//////            // ✅ Move forward faster than run
//////            worldX += jumpForwardSpeed * Time.deltaTime;

//////            // ✅ Parabola from startY → peak → groundY
//////            float height = Mathf.Lerp(startY, groundY, t) + jumpHeight * 4f * t * (1f - t);
//////            cowRect.anchoredPosition = new Vector2(
//////                cowRect.anchoredPosition.x,
//////                height
//////            );

//////            yield return null;
//////        }

//////        // Snap to ground
//////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//////        isJumping = false;
//////        jumpsLeft = maxJumps;   // ✅ reset jumps when landed
//////        jumpArcCo = null;
//////        Debug.Log("[Cow] Landed! Jumps reset.");
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    void SetState(CowState newState)
//////    {
//////        if (!isAlive) return;
//////        CurrentState = newState;
//////        Debug.Log($"[Cow] → {newState}");

//////        if (animLoop != null) StopCoroutine(animLoop);
//////        animLoop = StartCoroutine(MasterLoop());
//////    }

//////    IEnumerator MasterLoop()
//////    {
//////        int frame = 0;
//////        while (true)
//////        {
//////            if (!isAlive) yield break;

//////            Sprite[] frames = null;
//////            float fps = 12f;

//////            switch (CurrentState)
//////            {
//////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////            }

//////            if (frames == null || frames.Length == 0)
//////            {
//////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////                yield return new WaitForSeconds(0.1f);
//////                continue;
//////            }

//////            if (frame >= frames.Length) frame = 0;
//////            cowImage.sprite = frames[frame];
//////            cowRect.sizeDelta = originalSize;
//////            frame++;

//////            // Jump → ONCE → back to Run
//////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////            }

//////            // Hit → ONCE → Win or Run
//////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                if (hitThenWin)
//////                {
//////                    hitThenWin = false;
//////                    CurrentState = CowState.Win;
//////                    Debug.Log("[Cow] → Celebration!");
//////                }
//////                else
//////                {
//////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////                }
//////            }

//////            if (frame >= frames.Length) frame = 0;
//////            yield return new WaitForSeconds(1f / fps);
//////        }
//////    }
//////}

////using UnityEngine;
////using UnityEngine.UI;
////using System.Collections;

////public class Cowanimationcontroller : MonoBehaviour
////{
////    public enum CowState { Idle, Run, Jump, Hit, Win }

////    [Header("Auto Run")]
////    public float runSpeed = 200f;

////    [Header("Jump Physics")]
////    public float jumpHeight = 250f;  // pixels — how high
////    public float jumpDuration = 0.7f;  // seconds — time in air
////    public float jumpForwardSpeed = 400f; // pixels/sec forward during jump
////    public int maxJumps = 2;     // 1 = single jump, 2 = double jump

////    [Header("Jump Size")]
////    [Tooltip("1 = same size as run/idle.  1.3 = 30% bigger.  Drag to match your jump sprites.")]
////    [Range(0.5f, 3f)]
////    public float jumpScale = 1f;

////    [Header("Win Size")]
////    [Tooltip("1 = same size as run/idle.  1.3 = 30% bigger.  Drag to match your win sprites.")]
////    [Range(0.5f, 3f)]
////    public float winScale = 1f;

////    [Header("Idle Animation")]
////    public Sprite[] idleFrames;
////    public float idleFPS = 12f;

////    [Header("Run Animation")]
////    public Sprite[] runFrames;
////    public float runFPS = 12f;

////    [Header("Jump Animation")]
////    public Sprite[] jumpFrames;
////    public float jumpFPS = 12f;

////    [Header("Hit Animation")]
////    public Sprite[] hitFrames;
////    public float hitFPS = 12f;

////    [Header("Win / Celebration Animation")]
////    public Sprite[] winFrames;
////    public float winFPS = 12f;

////    // ── Private ────────────────────────────────────────────────
////    private Image cowImage;
////    private RectTransform cowRect;
////    private Vector2 originalSize;
////    private bool isAlive = false;
////    private bool hitThenWin = false;
////    private float groundY;
////    private bool isJumping = false;
////    private int jumpsLeft = 0;

////    private Coroutine animLoop = null;
////    private Coroutine jumpArcCo = null;

////    public CowState CurrentState { get; private set; } = CowState.Idle;

////    [HideInInspector] public float worldX = 0f;
////    [HideInInspector] public bool isRunning = false;

////    // ──────────────────────────────────────────────────────────
////    void Start()
////    {
////        isAlive = true;
////        cowImage = GetComponent<Image>();
////        cowRect = GetComponent<RectTransform>();
////        originalSize = cowRect.sizeDelta;
////        worldX = cowRect.anchoredPosition.x;
////        groundY = cowRect.anchoredPosition.y;
////        SetState(CowState.Idle);
////    }

////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////    // ──────────────────────────────────────────────────────────
////    void Update()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        if (CurrentState == CowState.Hit) return;

////        if (isRunning)
////        {
////            if (!isJumping)
////                worldX += runSpeed * Time.deltaTime;

////            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
////                PlayJump();
////        }
////    }

////    // ──────────────────────────────────────────────────────────
////    public void StartRunning()
////    {
////        isRunning = true;
////        jumpsLeft = maxJumps;
////        SetState(CowState.Run);
////        Debug.Log("[Cow] StartRunning!");
////    }

////    public void StopRunning()
////    {
////        isRunning = false;
////    }

////    // ──────────────────────────────────────────────────────────
////    public void PlayJump()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        if (CurrentState == CowState.Hit) return;
////        if (jumpsLeft <= 0) return;

////        jumpsLeft--;
////        Debug.Log($"[Cow] Jump! Jumps left: {jumpsLeft}");

////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
////        jumpArcCo = StartCoroutine(JumpArc());

////        SetState(CowState.Jump);
////    }

////    public void OnJumpPressed() => PlayJump();
////    public void OnJumpDown() => PlayJump();

////    // ──────────────────────────────────────────────────────────
////    // Called by ObstacleController when cow hits an obstacle
////    // ──────────────────────────────────────────────────────────
////    public void PlayHit()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Hit) return;
////        if (CurrentState == CowState.Win) return;

////        // Stop any jump in progress
////        if (jumpArcCo != null)
////        {
////            StopCoroutine(jumpArcCo);
////            jumpArcCo = null;
////        }
////        isJumping = false;
////        // ✅ Snap Y back to ground so hit animation does not play mid-air
////        if (cowRect != null)
////            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////        isRunning = false;

////        hitThenWin = false;
////        SetState(CowState.Hit);
////    }

////    // Called by BellTrigger — Hit once then Win celebration
////    public void PlayHitThenWin()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;

////        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
////        isJumping = false;
////        isRunning = false;
////        if (cowRect != null)
////            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////        hitThenWin = true;
////        SetState(CowState.Hit);
////        Debug.Log("[Cow] PlayHitThenWin!");
////    }

////    // ──────────────────────────────────────────────────────────
////    // Called by BellTrigger after waiting for Hit animation to finish.
////    // Transitions directly to Win (celebration) animation.
////    // ──────────────────────────────────────────────────────────
////    public void PlayWinDirect()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;

////        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
////        isJumping = false;
////        isRunning = false;
////        hitThenWin = false;

////        // ✅ Apply scale immediately so it's visible on the very first frame
////        if (cowRect != null)
////            cowRect.sizeDelta = originalSize * winScale;

////        SetState(CowState.Win);
////        Debug.Log("[Cow] PlayWinDirect — celebration!");
////    }

////    // ──────────────────────────────────────────────────────────
////    // Jump arc — parabola from current Y back to groundY
////    // ──────────────────────────────────────────────────────────
////    IEnumerator JumpArc()
////    {
////        isJumping = true;

////        float elapsed = 0f;
////        float startY = cowRect.anchoredPosition.y;

////        while (elapsed < jumpDuration)
////        {
////            if (!isAlive) yield break;
////            if (CurrentState == CowState.Hit) yield break; // stop arc on hit

////            elapsed += Time.deltaTime;
////            float t = Mathf.Clamp01(elapsed / jumpDuration);

////            worldX += jumpForwardSpeed * Time.deltaTime;

////            // Parabola: startY → peak → groundY
////            float height = Mathf.Lerp(startY, groundY, t) + jumpHeight * 4f * t * (1f - t);
////            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, height);

////            yield return null;
////        }

////        // Snap to ground and reset
////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////        isJumping = false;
////        jumpsLeft = maxJumps;
////        jumpArcCo = null;

////        if (CurrentState == CowState.Jump)
////            SetState(isRunning ? CowState.Run : CowState.Idle);

////        Debug.Log("[Cow] Landed!");
////    }

////    // ──────────────────────────────────────────────────────────
////    void SetState(CowState newState)
////    {
////        if (!isAlive) return;
////        CurrentState = newState;
////        Debug.Log($"[Cow] → {newState}");

////        if (animLoop != null) StopCoroutine(animLoop);
////        animLoop = StartCoroutine(MasterLoop());
////    }

////    IEnumerator MasterLoop()
////    {
////        int frame = 0;
////        while (true)
////        {
////            if (!isAlive) yield break;

////            Sprite[] frames = null;
////            float fps = 12f;

////            switch (CurrentState)
////            {
////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////                case CowState.Hit: frames = jumpFrames; fps = jumpFPS; break;
////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////            }

////            if (frames == null || frames.Length == 0)
////            {
////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////                yield return new WaitForSeconds(0.1f);
////                continue;
////            }

////            if (frame >= frames.Length) frame = 0;

////            cowImage.sprite = frames[frame];

////            // Apply scale based on state
////            if (CurrentState == CowState.Jump)
////                cowRect.sizeDelta = originalSize * jumpScale;
////            else if (CurrentState == CowState.Win)
////                cowRect.sizeDelta = originalSize * winScale;
////            else
////                cowRect.sizeDelta = originalSize;





////            frame++;

////            // Jump → plays once → handled by JumpArc landing
////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////            {
////                frame = 0;
////                // Keep looping jump frames until JumpArc finishes
////            }

////            // Hit → plays once → Win or Idle
////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////            {
////                frame = 0;
////                if (hitThenWin)
////                {
////                    hitThenWin = false;
////                    CurrentState = CowState.Win;
////                    Debug.Log("[Cow] → Celebration!");
////                }
////                else
////                {
////                    // Game over — stay in Hit / Idle, GameManager handles panel
////                    CurrentState = CowState.Idle;
////                }
////            }

////            if (frame >= frames.Length) frame = 0;

////            yield return new WaitForSeconds(1f / fps);
////        }
////    }
////}

//////////////////using UnityEngine;
//////////////////using UnityEngine.UI;
//////////////////using System.Collections;

//////////////////public class Cowanimationcontroller : MonoBehaviour
//////////////////{
//////////////////    public enum CowState { Idle, SideRun, Jump, Hit, Win }

//////////////////    [Header("Movement")]
//////////////////    public float moveSpeed = 500f;

//////////////////    [Header("Animations")]
//////////////////    public Sprite[] idleFrames;
//////////////////    public Sprite[] runFrames;
//////////////////    public Sprite[] jumpFrames;
//////////////////    public Sprite[] hitFrames;
//////////////////    public Sprite[] winFrames;

//////////////////    public float fps = 12f;

//////////////////    private Image cowImage;
//////////////////    private RectTransform cowRect;

//////////////////    private CowState currentState;

//////////////////    private bool isRunning = false;
//////////////////    private bool isJumping = false;   // ✅ FIX: prevent loop

//////////////////    [HideInInspector] public float worldX = 0f;

//////////////////    void Start()
//////////////////    {
//////////////////        cowImage = GetComponent<Image>();
//////////////////        cowRect = GetComponent<RectTransform>();

//////////////////        worldX = cowRect.anchoredPosition.x;

//////////////////        SetState(CowState.Idle);
//////////////////    }

//////////////////    void Update()
//////////////////    {
//////////////////        if (!isRunning) return;

//////////////////        // ✅ AUTO RUN
//////////////////        worldX += moveSpeed * Time.deltaTime;

//////////////////        // ✅ KEYBOARD JUMP
//////////////////        if (Input.GetKeyDown(KeyCode.Space))
//////////////////            PlayJump();
//////////////////    }

//////////////////    // =========================
//////////////////    // GAME CONTROL
//////////////////    // =========================
//////////////////    public void StartRunning()
//////////////////    {
//////////////////        isRunning = true;
//////////////////        SetState(CowState.SideRun);
//////////////////    }

//////////////////    public void StopRunning()
//////////////////    {
//////////////////        isRunning = false;
//////////////////    }

//////////////////    // =========================
//////////////////    // ACTIONS
//////////////////    // =========================
//////////////////    public void PlayJump()
//////////////////    {
//////////////////        if (isJumping) return;              // 🚫 prevent loop
//////////////////        if (currentState == CowState.Win) return;

//////////////////        isJumping = true;
//////////////////        SetState(CowState.Jump);
//////////////////    }

//////////////////    public void PlayHit()
//////////////////    {
//////////////////        if (currentState == CowState.Win) return;
//////////////////        SetState(CowState.Hit);
//////////////////    }

//////////////////    public void PlayWin()
//////////////////    {
//////////////////        StopRunning();
//////////////////        SetState(CowState.Win);

//////////////////        GameManager.Instance?.SendMessage("ShowCongrats");
//////////////////    }

//////////////////    public void PlayHitThenWin()
//////////////////    {
//////////////////        StartCoroutine(HitThenWinRoutine());
//////////////////    }

//////////////////    IEnumerator HitThenWinRoutine()
//////////////////    {
//////////////////        SetState(CowState.Hit);
//////////////////        yield return new WaitForSeconds(0.5f);
//////////////////        PlayWin();
//////////////////    }

//////////////////    // UI Button
//////////////////    public void OnJumpDown()
//////////////////    {
//////////////////        PlayJump();
//////////////////    }

//////////////////    // =========================
//////////////////    void SetState(CowState state)
//////////////////    {
//////////////////        currentState = state;
//////////////////        StopAllCoroutines();
//////////////////        StartCoroutine(Animate());
//////////////////    }

//////////////////    IEnumerator Animate()
//////////////////    {
//////////////////        Sprite[] frames = idleFrames;

//////////////////        switch (currentState)
//////////////////        {
//////////////////            case CowState.Idle: frames = idleFrames; break;
//////////////////            case CowState.SideRun: frames = runFrames; break;
//////////////////            case CowState.Jump: frames = jumpFrames; break;
//////////////////            case CowState.Hit: frames = hitFrames; break;
//////////////////            case CowState.Win: frames = winFrames; break;
//////////////////        }

//////////////////        int i = 0;

//////////////////        while (true)
//////////////////        {
//////////////////            if (frames == null || frames.Length == 0)
//////////////////                yield break;

//////////////////            cowImage.sprite = frames[i];
//////////////////            i++;

//////////////////            // ✅ JUMP END FIX
//////////////////            if (currentState == CowState.Jump && i >= frames.Length)
//////////////////            {
//////////////////                isJumping = false;                 // 🔥 unlock jump
//////////////////                SetState(CowState.SideRun);        // back to run
//////////////////                yield break;
//////////////////            }

//////////////////            // ✅ HIT END
//////////////////            if (currentState == CowState.Hit && i >= frames.Length)
//////////////////            {
//////////////////                SetState(CowState.SideRun);
//////////////////                yield break;
//////////////////            }

//////////////////            // LOOP states
//////////////////            if (i >= frames.Length)
//////////////////                i = 0;

//////////////////            yield return new WaitForSeconds(1f / fps);
//////////////////        }
//////////////////    }
//////////////////}

////////////////using UnityEngine;
////////////////using UnityEngine.UI;
////////////////using System.Collections;

////////////////public class Cowanimationcontroller : MonoBehaviour
////////////////{
////////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////////////    [Header("Auto Run")]
////////////////    public float runSpeed = 200f;

////////////////    [Header("Idle Animation")]
////////////////    public Sprite[] idleFrames;
////////////////    public float idleFPS = 12f;

////////////////    [Header("Run Animation")]
////////////////    public Sprite[] runFrames;
////////////////    public float runFPS = 12f;

////////////////    [Header("Jump Animation")]
////////////////    public Sprite[] jumpFrames;
////////////////    public float jumpFPS = 12f;

////////////////    [Header("Hit Animation")]
////////////////    public Sprite[] hitFrames;
////////////////    public float hitFPS = 12f;

////////////////    [Header("Win / Celebration Animation")]
////////////////    public Sprite[] winFrames;
////////////////    public float winFPS = 12f;

////////////////    private Image cowImage;
////////////////    private RectTransform cowRect;
////////////////    private Vector2 originalSize;
////////////////    private bool isAlive = false;
////////////////    private bool hitThenWin = false;

////////////////    // ✅ PUBLIC — BellTrigger reads this to check if cow is jumping
////////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////////////    [HideInInspector] public float worldX = 0f;
////////////////    [HideInInspector] public bool isRunning = false;

////////////////    // ──────────────────────────────────────────────────────────
////////////////    void Start()
////////////////    {
////////////////        isAlive = true;
////////////////        cowImage = GetComponent<Image>();
////////////////        cowRect = GetComponent<RectTransform>();
////////////////        originalSize = cowRect.sizeDelta;
////////////////        worldX = cowRect.anchoredPosition.x;
////////////////        SetState(CowState.Idle);

////////////////        if (GameManager.Instance != null)
////////////////        {
////////////////            GameManager.Instance.OnGameStart += StartRunning;
////////////////            GameManager.Instance.OnGameWin += StopRunning;
////////////////        }
////////////////    }

////////////////    void OnDestroy()
////////////////    {
////////////////        isAlive = false;
////////////////        StopAllCoroutines();
////////////////        if (GameManager.Instance != null)
////////////////        {
////////////////            GameManager.Instance.OnGameStart -= StartRunning;
////////////////            GameManager.Instance.OnGameWin -= StopRunning;
////////////////        }
////////////////    }

////////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////////////    // ──────────────────────────────────────────────────────────
////////////////    void Update()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        if (CurrentState == CowState.Hit) return;

////////////////        if (isRunning)
////////////////        {
////////////////            worldX += runSpeed * Time.deltaTime;

////////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////////////                PlayJump();
////////////////        }
////////////////    }

////////////////    void StartRunning() { isRunning = true; SetState(CowState.Run); }
////////////////    void StopRunning() { isRunning = false; }

////////////////    // ──────────────────────────────────────────────────────────
////////////////    public void PlayJump()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        if (CurrentState == CowState.Jump) return;
////////////////        if (CurrentState == CowState.Hit) return;
////////////////        SetState(CowState.Jump);
////////////////    }

////////////////    public void PlayHit()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Hit) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        hitThenWin = false;
////////////////        SetState(CowState.Hit);
////////////////    }

////////////////    // ✅ Called by BellTrigger — Hit once → then Celebration loops
////////////////    public void PlayHitThenWin()
////////////////    {
////////////////        if (!isAlive) return;
////////////////        if (CurrentState == CowState.Win) return;
////////////////        Debug.Log("[Cow] PlayHitThenWin!");
////////////////        hitThenWin = true;
////////////////        isRunning = false;
////////////////        SetState(CowState.Hit);
////////////////    }

////////////////    public void OnJumpPressed() => PlayJump();

////////////////    // ──────────────────────────────────────────────────────────
////////////////    void SetState(CowState newState)
////////////////    {
////////////////        if (!isAlive) return;
////////////////        CurrentState = newState;
////////////////        Debug.Log($"[Cow] → {newState}");
////////////////        StopAllCoroutines();
////////////////        StartCoroutine(MasterLoop());
////////////////    }

////////////////    IEnumerator MasterLoop()
////////////////    {
////////////////        int frame = 0;
////////////////        while (true)
////////////////        {
////////////////            if (!isAlive) yield break;

////////////////            Sprite[] frames = null;
////////////////            float fps = 12f;

////////////////            switch (CurrentState)
////////////////            {
////////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////////////            }

////////////////            if (frames == null || frames.Length == 0)
////////////////            {
////////////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////////////////                yield return new WaitForSeconds(0.1f);
////////////////                continue;
////////////////            }

////////////////            if (frame >= frames.Length) frame = 0;
////////////////            cowImage.sprite = frames[frame];
////////////////            cowRect.sizeDelta = originalSize;
////////////////            frame++;

////////////////            // Jump → ONCE → back to Run
////////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////////////            {
////////////////                frame = 0;
////////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////////            }

////////////////            // Hit → ONCE → Win or Run
////////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////////////            {
////////////////                frame = 0;
////////////////                if (hitThenWin)
////////////////                {
////////////////                    hitThenWin = false;
////////////////                    CurrentState = CowState.Win;
////////////////                    Debug.Log("[Cow] Hit done → Celebration!");
////////////////                }
////////////////                else
////////////////                {
////////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////////                }
////////////////            }

////////////////            // Idle / Run / Win → loop forever
////////////////            if (frame >= frames.Length) frame = 0;

////////////////            yield return new WaitForSeconds(1f / fps);
////////////////        }
////////////////    }
////////////////}
//////////////using UnityEngine;
//////////////using UnityEngine.UI;
//////////////using System.Collections;

//////////////public class Cowanimationcontroller : MonoBehaviour
//////////////{
//////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////////    [Header("Auto Run")]
//////////////    public float runSpeed = 200f;

//////////////    [Header("Idle Animation")]
//////////////    public Sprite[] idleFrames;
//////////////    public float idleFPS = 12f;

//////////////    [Header("Run Animation")]
//////////////    public Sprite[] runFrames;
//////////////    public float runFPS = 12f;

//////////////    [Header("Jump Animation")]
//////////////    public Sprite[] jumpFrames;
//////////////    public float jumpFPS = 12f;

//////////////    [Header("Hit Animation")]
//////////////    public Sprite[] hitFrames;
//////////////    public float hitFPS = 12f;

//////////////    [Header("Win / Celebration Animation")]
//////////////    public Sprite[] winFrames;
//////////////    public float winFPS = 12f;

//////////////    private Image cowImage;
//////////////    private RectTransform cowRect;
//////////////    private Vector2 originalSize;
//////////////    private bool isAlive = false;
//////////////    private bool hitThenWin = false;

//////////////    // ✅ PUBLIC — GameManager calls StartRunning(), BellTrigger reads CurrentState
//////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////////    [HideInInspector] public float worldX = 0f;
//////////////    [HideInInspector] public bool isRunning = false;

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Start()
//////////////    {
//////////////        isAlive = true;
//////////////        cowImage = GetComponent<Image>();
//////////////        cowRect = GetComponent<RectTransform>();
//////////////        originalSize = cowRect.sizeDelta;
//////////////        worldX = cowRect.anchoredPosition.x;
//////////////        SetState(CowState.Idle);
//////////////    }

//////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Update()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Hit) return;

//////////////        if (isRunning)
//////////////        {
//////////////            worldX += runSpeed * Time.deltaTime;

//////////////            // Jump — Space or Up Arrow
//////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////////                PlayJump();
//////////////        }
//////////////    }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    // ✅ Called by GameManager.CountdownRoutine() after GO!!
//////////////    // ──────────────────────────────────────────────────────────
//////////////    public void StartRunning()
//////////////    {
//////////////        isRunning = true;
//////////////        SetState(CowState.Run);
//////////////        Debug.Log("[Cow] StartRunning called by GameManager!");
//////////////    }

//////////////    public void StopRunning()
//////////////    {
//////////////        isRunning = false;
//////////////    }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    // Public methods
//////////////    // ──────────────────────────────────────────────────────────
//////////////    public void PlayIdle()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        SetState(CowState.Idle);
//////////////    }

//////////////    public void PlayJump()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Jump) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        SetState(CowState.Jump);
//////////////    }

//////////////    public void PlayHit()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        hitThenWin = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    // ✅ Called by BellTrigger — Hit once → Celebration loops
//////////////    public void PlayHitThenWin()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////////        hitThenWin = true;
//////////////        isRunning = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    // On-screen jump button
//////////////    public void OnJumpPressed() => PlayJump();

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void SetState(CowState newState)
//////////////    {
//////////////        if (!isAlive) return;
//////////////        CurrentState = newState;
//////////////        Debug.Log($"[Cow] → {newState}");
//////////////        StopAllCoroutines();
//////////////        StartCoroutine(MasterLoop());
//////////////    }

//////////////    IEnumerator MasterLoop()
//////////////    {
//////////////        int frame = 0;
//////////////        while (true)
//////////////        {
//////////////            if (!isAlive) yield break;

//////////////            Sprite[] frames = null;
//////////////            float fps = 12f;

//////////////            switch (CurrentState)
//////////////            {
//////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////////            }

//////////////            if (frames == null || frames.Length == 0)
//////////////            {
//////////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////////////                yield return new WaitForSeconds(0.1f);
//////////////                continue;
//////////////            }

//////////////            if (frame >= frames.Length) frame = 0;
//////////////            cowImage.sprite = frames[frame];
//////////////            cowRect.sizeDelta = originalSize;
//////////////            frame++;

//////////////            // Jump → ONCE → back to Run
//////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////            }

//////////////            // Hit → ONCE → Win or Run
//////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                if (hitThenWin)
//////////////                {
//////////////                    hitThenWin = false;
//////////////                    CurrentState = CowState.Win;
//////////////                    Debug.Log("[Cow] → Celebration!");
//////////////                }
//////////////                else
//////////////                {
//////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////                }
//////////////            }

//////////////            // Idle / Run / Win → loop forever
//////////////            if (frame >= frames.Length) frame = 0;

//////////////            yield return new WaitForSeconds(1f / fps);
//////////////        }
//////////////    }
//////////////}
////////////using UnityEngine;
////////////using UnityEngine.UI;
////////////using System.Collections;

////////////public class Cowanimationcontroller : MonoBehaviour
////////////{
////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////////    [Header("Auto Run")]
////////////    public float runSpeed = 200f;

////////////    [Header("Idle Animation")]
////////////    public Sprite[] idleFrames;
////////////    public float idleFPS = 12f;

////////////    [Header("Run Animation")]
////////////    public Sprite[] runFrames;
////////////    public float runFPS = 12f;

////////////    [Header("Jump Animation")]
////////////    public Sprite[] jumpFrames;
////////////    public float jumpFPS = 12f;

////////////    [Header("Hit Animation")]
////////////    public Sprite[] hitFrames;
////////////    public float hitFPS = 12f;

////////////    [Header("Win / Celebration Animation")]
////////////    public Sprite[] winFrames;
////////////    public float winFPS = 12f;

////////////    private Image cowImage;
////////////    private RectTransform cowRect;
////////////    private Vector2 originalSize;
////////////    private bool isAlive = false;
////////////    private bool hitThenWin = false;

////////////    // BellTrigger reads CurrentState to know if cow is jumping
////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////////    [HideInInspector] public float worldX = 0f;
////////////    [HideInInspector] public bool isRunning = false;

////////////    // ──────────────────────────────────────────────────────────
////////////    void Start()
////////////    {
////////////        isAlive = true;
////////////        cowImage = GetComponent<Image>();
////////////        cowRect = GetComponent<RectTransform>();
////////////        originalSize = cowRect.sizeDelta;
////////////        worldX = cowRect.anchoredPosition.x;
////////////        SetState(CowState.Idle);
////////////    }

////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////////    // ──────────────────────────────────────────────────────────
////////////    void Update()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Hit) return;

////////////        if (isRunning)
////////////        {
////////////            worldX += runSpeed * Time.deltaTime;

////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////////                PlayJump();
////////////        }
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    // Called by GameManager countdown after "GO!!"
////////////    // ──────────────────────────────────────────────────────────
////////////    public void StartRunning()
////////////    {
////////////        isRunning = true;
////////////        SetState(CowState.Run);
////////////        Debug.Log("[Cow] StartRunning!");
////////////    }

////////////    public void StopRunning()
////////////    {
////////////        isRunning = false;
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    // Public actions
////////////    // ──────────────────────────────────────────────────────────
////////////    public void PlayIdle()
////////////    {
////////////        if (!isAlive) return;
////////////        SetState(CowState.Idle);
////////////    }

////////////    public void PlayJump()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Jump) return;
////////////        if (CurrentState == CowState.Hit) return;
////////////        Debug.Log("[Cow] PlayJump!");
////////////        SetState(CowState.Jump);
////////////    }

////////////    // ── UI Button hooks ────────────────────────────────────────
////////////    // Both names wired here so either old or new button reference works
////////////    public void OnJumpPressed() => PlayJump();
////////////    public void OnJumpDown() => PlayJump(); // legacy alias — keep button working

////////////    // ──────────────────────────────────────────────────────────
////////////    public void PlayHit()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Hit) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        hitThenWin = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    // Called by BellTrigger: plays Hit once, then loops Win/celebration
////////////    public void PlayHitThenWin()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        Debug.Log("[Cow] PlayHitThenWin!");
////////////        hitThenWin = true;
////////////        isRunning = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    void SetState(CowState newState)
////////////    {
////////////        if (!isAlive) return;
////////////        CurrentState = newState;
////////////        Debug.Log($"[Cow] State → {newState}");
////////////        StopAllCoroutines();
////////////        StartCoroutine(MasterLoop());
////////////    }

////////////    IEnumerator MasterLoop()
////////////    {
////////////        int frame = 0;
////////////        while (true)
////////////        {
////////////            if (!isAlive) yield break;

////////////            Sprite[] frames = null;
////////////            float fps = 12f;

////////////            switch (CurrentState)
////////////            {
////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////////            }

////////////            if (frames == null || frames.Length == 0)
////////////            {
////////////                Debug.LogError($"[Cow] {CurrentState} frames are EMPTY — assign sprites in Inspector!");
////////////                yield return new WaitForSeconds(0.1f);
////////////                continue;
////////////            }

////////////            if (frame >= frames.Length) frame = 0;
////////////            cowImage.sprite = frames[frame];
////////////            cowRect.sizeDelta = originalSize;
////////////            frame++;

////////////            // Jump plays ONCE then returns to Run
////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////            }

////////////            // Hit plays ONCE then either Win or Run
////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                if (hitThenWin)
////////////                {
////////////                    hitThenWin = false;
////////////                    CurrentState = CowState.Win;
////////////                    Debug.Log("[Cow] Hit done → Win/Celebration!");
////////////                }
////////////                else
////////////                {
////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////                }
////////////            }

////////////            // Idle / Run / Win loop forever
////////////            if (frame >= frames.Length) frame = 0;

////////////            yield return new WaitForSeconds(1f / fps);
////////////        }
////////////    }
////////////}

////////////using UnityEngine;
////////////using UnityEngine.UI;
////////////using System.Collections;

////////////public class Cowanimationcontroller : MonoBehaviour
////////////{
////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////////    [Header("Auto Run")]
////////////    public float runSpeed = 200f;

////////////    [Header("Idle Animation")]
////////////    public Sprite[] idleFrames;
////////////    public float idleFPS = 12f;

////////////    [Header("Run Animation")]
////////////    public Sprite[] runFrames;
////////////    public float runFPS = 12f;

////////////    [Header("Jump Animation")]
////////////    public Sprite[] jumpFrames;
////////////    public float jumpFPS = 12f;

////////////    [Header("Jump Size")]
////////////    [Tooltip("1 = same as run/idle size.  1.3 = 30% bigger.  Try values between 1.0 and 2.0.")]
////////////    [Range(0.5f, 3f)]
////////////    public float jumpScale = 1f;   // ← drag this slider in the Inspector

////////////    [Header("Hit Animation")]
////////////    public Sprite[] hitFrames;
////////////    public float hitFPS = 12f;

////////////    [Header("Win / Celebration Animation")]
////////////    public Sprite[] winFrames;
////////////    public float winFPS = 12f;

////////////    private Image cowImage;
////////////    private RectTransform cowRect;
////////////    private Vector2 originalSize;
////////////    private bool isAlive = false;
////////////    private bool hitThenWin = false;

////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////////    [HideInInspector] public float worldX = 0f;
////////////    [HideInInspector] public bool isRunning = false;

////////////    // ──────────────────────────────────────────────────────────
////////////    void Start()
////////////    {
////////////        isAlive = true;
////////////        cowImage = GetComponent<Image>();
////////////        cowRect = GetComponent<RectTransform>();
////////////        originalSize = cowRect.sizeDelta;
////////////        worldX = cowRect.anchoredPosition.x;
////////////        SetState(CowState.Idle);
////////////    }

////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////////    // ──────────────────────────────────────────────────────────
////////////    void Update()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Hit) return;

////////////        if (isRunning)
////////////        {
////////////            worldX += runSpeed * Time.deltaTime;

////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////////                PlayJump();
////////////        }
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    public void StartRunning()
////////////    {
////////////        isRunning = true;
////////////        SetState(CowState.Run);
////////////        Debug.Log("[Cow] StartRunning!");
////////////    }

////////////    public void StopRunning()
////////////    {
////////////        isRunning = false;
////////////    }

////////////    public void PlayJump()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Jump) return;
////////////        if (CurrentState == CowState.Hit) return;
////////////        Debug.Log("[Cow] Jump!");
////////////        SetState(CowState.Jump);
////////////    }

////////////    public void OnJumpPressed() => PlayJump();
////////////    public void OnJumpDown() => PlayJump();

////////////    public void PlayHit()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Hit) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        hitThenWin = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    public void PlayHitThenWin()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        Debug.Log("[Cow] PlayHitThenWin!");
////////////        hitThenWin = true;
////////////        isRunning = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    void SetState(CowState newState)
////////////    {
////////////        if (!isAlive) return;
////////////        CurrentState = newState;
////////////        Debug.Log($"[Cow] → {newState}");
////////////        StopAllCoroutines();
////////////        StartCoroutine(MasterLoop());
////////////    }

////////////    IEnumerator MasterLoop()
////////////    {
////////////        int frame = 0;
////////////        while (true)
////////////        {
////////////            if (!isAlive) yield break;

////////////            Sprite[] frames = null;
////////////            float fps = 12f;

////////////            switch (CurrentState)
////////////            {
////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////////            }

////////////            if (frames == null || frames.Length == 0)
////////////            {
////////////                Debug.LogError($"[Cow] {CurrentState} frames are EMPTY!");
////////////                yield return new WaitForSeconds(0.1f);
////////////                continue;
////////////            }

////////////            if (frame >= frames.Length) frame = 0;

////////////            cowImage.sprite = frames[frame];

////////////            // Apply jumpScale only during Jump, otherwise restore original size
////////////            cowRect.sizeDelta = (CurrentState == CowState.Jump)
////////////                ? originalSize * jumpScale
////////////                : originalSize;

////////////            frame++;

////////////            // Jump plays once → back to Run
////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////            }

////////////            // Hit plays once → Win or Run
////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                if (hitThenWin)
////////////                {
////////////                    hitThenWin = false;
////////////                    CurrentState = CowState.Win;
////////////                    Debug.Log("[Cow] → Celebration!");
////////////                }
////////////                else
////////////                {
////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////                }
////////////            }

////////////            // Idle / Run / Win loop forever
////////////            if (frame >= frames.Length) frame = 0;

////////////            yield return new WaitForSeconds(1f / fps);
////////////        }
////////////    }
////////////}

//////////using UnityEngine;
//////////using UnityEngine.UI;
//////////using System.Collections;

//////////public class Cowanimationcontroller : MonoBehaviour
//////////{
//////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////    [Header("Auto Run")]
//////////    public float runSpeed = 200f;

//////////    [Header("Jump Physics")]
//////////    public float jumpHeight = 250f;   // how high the cow goes (pixels)
//////////    public float jumpDuration = 0.7f;   // total time in air (seconds)

//////////    [Header("Idle Animation")]
//////////    public Sprite[] idleFrames;
//////////    public float idleFPS = 12f;

//////////    [Header("Run Animation")]
//////////    public Sprite[] runFrames;
//////////    public float runFPS = 12f;

//////////    [Header("Jump Animation")]
//////////    public Sprite[] jumpFrames;
//////////    public float jumpFPS = 12f;

//////////    [Header("Hit Animation")]
//////////    public Sprite[] hitFrames;
//////////    public float hitFPS = 12f;

//////////    [Header("Win / Celebration Animation")]
//////////    public Sprite[] winFrames;
//////////    public float winFPS = 12f;

//////////    private Image cowImage;
//////////    private RectTransform cowRect;
//////////    private Vector2 originalSize;
//////////    private bool isAlive = false;
//////////    private bool hitThenWin = false;

//////////    // ✅ PUBLIC — GameManager calls StartRunning(), BellTrigger reads CurrentState
//////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////    [HideInInspector] public float worldX = 0f;
//////////    [HideInInspector] public bool isRunning = false;

//////////    // ✅ Ground Y — saved on Start so jump always returns to same Y
//////////    private float groundY;

//////////    // ──────────────────────────────────────────────────────────
//////////    void Start()
//////////    {
//////////        isAlive = true;
//////////        cowImage = GetComponent<Image>();
//////////        cowRect = GetComponent<RectTransform>();
//////////        originalSize = cowRect.sizeDelta;
//////////        worldX = cowRect.anchoredPosition.x;
//////////        groundY = cowRect.anchoredPosition.y;   // ✅ remember ground level
//////////        SetState(CowState.Idle);
//////////    }

//////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////    // ──────────────────────────────────────────────────────────
//////////    void Update()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Hit) return;

//////////        if (isRunning)
//////////        {
//////////            worldX += runSpeed * Time.deltaTime;

//////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////                PlayJump();
//////////        }
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    // Called by GameManager after countdown
//////////    // ──────────────────────────────────────────────────────────
//////////    public void StartRunning()
//////////    {
//////////        isRunning = true;
//////////        SetState(CowState.Run);
//////////        Debug.Log("[Cow] StartRunning!");
//////////    }

//////////    public void StopRunning() => isRunning = false;

//////////    // ──────────────────────────────────────────────────────────
//////////    // Public methods
//////////    // ──────────────────────────────────────────────────────────
//////////    public void PlayJump()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Jump) return;  // no double jump
//////////        if (CurrentState == CowState.Hit) return;
//////////        Debug.Log("[Cow] Jump!");
//////////        StartCoroutine(JumpArc());   // ✅ move cow up and down
//////////        SetState(CowState.Jump);
//////////    }

//////////    public void OnJumpPressed() => PlayJump();
//////////    public void OnJumpDown() => PlayJump();

//////////    public void PlayHit()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Hit) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        hitThenWin = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    public void PlayHitThenWin()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////        hitThenWin = true;
//////////        isRunning = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    // ✅ Jump Arc — smooth parabola up and back down
//////////    // ──────────────────────────────────────────────────────────
//////////    IEnumerator JumpArc()
//////////    {
//////////        float elapsed = 0f;

//////////        while (elapsed < jumpDuration)
//////////        {
//////////            elapsed += Time.deltaTime;

//////////            // Normalised time 0→1
//////////            float t = elapsed / jumpDuration;

//////////            // Parabola: peaks at t=0.5, returns to 0 at t=1
//////////            float height = jumpHeight * 4f * t * (1f - t);

//////////            // ✅ Only change Y — X is handled by BackgroundScroller
//////////            cowRect.anchoredPosition = new Vector2(
//////////                cowRect.anchoredPosition.x,
//////////                groundY + height
//////////            );

//////////            yield return null;
//////////        }

//////////        // ✅ Snap exactly back to ground when done
//////////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    void SetState(CowState newState)
//////////    {
//////////        if (!isAlive) return;
//////////        CurrentState = newState;
//////////        Debug.Log($"[Cow] → {newState}");
//////////        StopAllCoroutines();
//////////        StartCoroutine(MasterLoop());
//////////    }

//////////    IEnumerator MasterLoop()
//////////    {
//////////        int frame = 0;
//////////        while (true)
//////////        {
//////////            if (!isAlive) yield break;

//////////            Sprite[] frames = null;
//////////            float fps = 12f;

//////////            switch (CurrentState)
//////////            {
//////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////            }

//////////            if (frames == null || frames.Length == 0)
//////////            {
//////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////////                yield return new WaitForSeconds(0.1f);
//////////                continue;
//////////            }

//////////            if (frame >= frames.Length) frame = 0;
//////////            cowImage.sprite = frames[frame];
//////////            cowRect.sizeDelta = originalSize;
//////////            frame++;

//////////            // Jump → ONCE → back to Run
//////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////            }

//////////            // Hit → ONCE → Win or Run
//////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                if (hitThenWin)
//////////                {
//////////                    hitThenWin = false;
//////////                    CurrentState = CowState.Win;
//////////                    Debug.Log("[Cow] → Celebration!");
//////////                }
//////////                else
//////////                {
//////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////                }
//////////            }

//////////            // Idle / Run / Win → loop forever
//////////            if (frame >= frames.Length) frame = 0;

//////////            yield return new WaitForSeconds(1f / fps);
//////////        }
//////////    }
//////////}

////////using UnityEngine;
////////using UnityEngine.UI;
////////using System.Collections;

////////public class Cowanimationcontroller : MonoBehaviour
////////{
////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////    [Header("Auto Run")]
////////    public float runSpeed = 200f;

////////    [Header("Jump Physics")]
////////    public float jumpHeight = 250f;   // pixels — how high the cow goes
////////    public float jumpDuration = 0.7f;   // seconds — time in the air

////////    [Header("Idle Animation")]
////////    public Sprite[] idleFrames;
////////    public float idleFPS = 12f;

////////    [Header("Run Animation")]
////////    public Sprite[] runFrames;
////////    public float runFPS = 12f;

////////    [Header("Jump Animation")]
////////    public Sprite[] jumpFrames;
////////    public float jumpFPS = 12f;

////////    [Header("Hit Animation")]
////////    public Sprite[] hitFrames;
////////    public float hitFPS = 12f;

////////    [Header("Win / Celebration Animation")]
////////    public Sprite[] winFrames;
////////    public float winFPS = 12f;

////////    // ── Private ────────────────────────────────────────────────
////////    private Image cowImage;
////////    private RectTransform cowRect;
////////    private Vector2 originalSize;
////////    private bool isAlive = false;
////////    private bool hitThenWin = false;
////////    private float groundY;   // Y position at ground level

////////    // ✅ Separate coroutine references so JumpArc is never killed by StopAllCoroutines
////////    private Coroutine animLoop = null;
////////    private Coroutine jumpArcCo = null;

////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////    [HideInInspector] public float worldX = 0f;
////////    [HideInInspector] public bool isRunning = false;

////////    // ──────────────────────────────────────────────────────────
////////    void Start()
////////    {
////////        isAlive = true;
////////        cowImage = GetComponent<Image>();
////////        cowRect = GetComponent<RectTransform>();
////////        originalSize = cowRect.sizeDelta;
////////        worldX = cowRect.anchoredPosition.x;
////////        groundY = cowRect.anchoredPosition.y;  // ✅ save ground level once
////////        SetState(CowState.Idle);
////////    }

////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////    // ──────────────────────────────────────────────────────────
////////    void Update()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        if (CurrentState == CowState.Hit) return;

////////        if (isRunning)
////////        {
////////            worldX += runSpeed * Time.deltaTime;

////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////                PlayJump();
////////        }
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    public void StartRunning()
////////    {
////////        isRunning = true;
////////        SetState(CowState.Run);
////////        Debug.Log("[Cow] StartRunning!");
////////    }

////////    public void StopRunning() => isRunning = false;

////////    // ──────────────────────────────────────────────────────────
////////    public void PlayJump()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        if (CurrentState == CowState.Jump) return;
////////        if (CurrentState == CowState.Hit) return;

////////        Debug.Log("[Cow] Jump!");

////////        // ✅ Stop any previous arc, start a fresh one independently
////////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
////////        jumpArcCo = StartCoroutine(JumpArc());

////////        // ✅ Switch animation to Jump (only kills animLoop, NOT jumpArcCo)
////////        SetState(CowState.Jump);
////////    }

////////    public void OnJumpPressed() => PlayJump();
////////    public void OnJumpDown() => PlayJump();

////////    public void PlayHit()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Hit) return;
////////        if (CurrentState == CowState.Win) return;
////////        hitThenWin = false;
////////        SetState(CowState.Hit);
////////    }

////////    public void PlayHitThenWin()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        Debug.Log("[Cow] PlayHitThenWin!");
////////        hitThenWin = true;
////////        isRunning = false;
////////        SetState(CowState.Hit);
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    // ✅ JumpArc — runs INDEPENDENTLY, never cancelled by SetState
////////    // ──────────────────────────────────────────────────────────
////////    IEnumerator JumpArc()
////////    {
////////        float elapsed = 0f;

////////        while (elapsed < jumpDuration)
////////        {
////////            if (!isAlive) yield break;

////////            elapsed += Time.deltaTime;
////////            float t = Mathf.Clamp01(elapsed / jumpDuration);

////////            // Smooth parabola: 0 → peak → 0
////////            float height = jumpHeight * 4f * t * (1f - t);

////////            // ✅ Only move Y — BackgroundScroller handles X
////////            cowRect.anchoredPosition = new Vector2(
////////                cowRect.anchoredPosition.x,
////////                groundY + height
////////            );

////////            yield return null;
////////        }

////////        // ✅ Snap back to ground exactly
////////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////////        jumpArcCo = null;

////////        Debug.Log("[Cow] Jump arc finished — back on ground");
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    // SetState only stops animLoop — NEVER jumpArcCo
////////    // ──────────────────────────────────────────────────────────
////////    void SetState(CowState newState)
////////    {
////////        if (!isAlive) return;
////////        CurrentState = newState;
////////        Debug.Log($"[Cow] → {newState}");

////////        // ✅ Stop only the animation loop, not the jump arc
////////        if (animLoop != null) StopCoroutine(animLoop);
////////        animLoop = StartCoroutine(MasterLoop());
////////    }

////////    IEnumerator MasterLoop()
////////    {
////////        int frame = 0;
////////        while (true)
////////        {
////////            if (!isAlive) yield break;

////////            Sprite[] frames = null;
////////            float fps = 12f;

////////            switch (CurrentState)
////////            {
////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////            }

////////            if (frames == null || frames.Length == 0)
////////            {
////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////////                yield return new WaitForSeconds(0.1f);
////////                continue;
////////            }

////////            if (frame >= frames.Length) frame = 0;
////////            cowImage.sprite = frames[frame];
////////            cowRect.sizeDelta = originalSize;
////////            frame++;

////////            // Jump → ONCE → back to Run
////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////            {
////////                frame = 0;
////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////            }

////////            // Hit → ONCE → Win or Run
////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////            {
////////                frame = 0;
////////                if (hitThenWin)
////////                {
////////                    hitThenWin = false;
////////                    CurrentState = CowState.Win;
////////                    Debug.Log("[Cow] → Celebration!");
////////                }
////////                else
////////                {
////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////                }
////////            }

////////            // Idle / Run / Win → loop forever
////////            if (frame >= frames.Length) frame = 0;

////////            yield return new WaitForSeconds(1f / fps);
////////        }
////////    }
////////}


//////using UnityEngine;
//////using UnityEngine.UI;
//////using System.Collections;

//////public class Cowanimationcontroller : MonoBehaviour
//////{
//////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////    [Header("Auto Run")]
//////    public float runSpeed = 200f;

//////    [Header("Jump Physics")]
//////    public float jumpHeight = 250f;   // pixels — how high
//////    public float jumpDuration = 0.7f;   // seconds — time in air
//////    public float jumpForwardSpeed = 400f;  // pixels/sec forward during jump (faster than runSpeed)

//////    [Header("Idle Animation")]
//////    public Sprite[] idleFrames;
//////    public float idleFPS = 12f;

//////    [Header("Run Animation")]
//////    public Sprite[] runFrames;
//////    public float runFPS = 12f;

//////    [Header("Jump Animation")]
//////    public Sprite[] jumpFrames;
//////    public float jumpFPS = 12f;

//////    [Header("Hit Animation")]
//////    public Sprite[] hitFrames;
//////    public float hitFPS = 12f;

//////    [Header("Win / Celebration Animation")]
//////    public Sprite[] winFrames;
//////    public float winFPS = 12f;

//////    // ── Private ────────────────────────────────────────────────
//////    private Image cowImage;
//////    private RectTransform cowRect;
//////    private Vector2 originalSize;
//////    private bool isAlive = false;
//////    private bool hitThenWin = false;
//////    private float groundY;
//////    private bool isJumping = false;   // ✅ blocks Update from adding runSpeed during jump

//////    private Coroutine animLoop = null;
//////    private Coroutine jumpArcCo = null;

//////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////    [HideInInspector] public float worldX = 0f;
//////    [HideInInspector] public bool isRunning = false;

//////    // ──────────────────────────────────────────────────────────
//////    void Start()
//////    {
//////        isAlive = true;
//////        cowImage = GetComponent<Image>();
//////        cowRect = GetComponent<RectTransform>();
//////        originalSize = cowRect.sizeDelta;
//////        worldX = cowRect.anchoredPosition.x;
//////        groundY = cowRect.anchoredPosition.y;
//////        SetState(CowState.Idle);
//////    }

//////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////    // ──────────────────────────────────────────────────────────
//////    void Update()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Hit) return;

//////        if (isRunning && !isJumping)
//////        {
//////            // ✅ Normal run speed — JumpArc takes over worldX during jump
//////            worldX += runSpeed * Time.deltaTime;

//////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////                PlayJump();
//////        }
//////        else if (isRunning && isJumping)
//////        {
//////            // ✅ Still allow jump input to be read but worldX is driven by JumpArc
//////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////                Debug.Log("[Cow] Already jumping!");
//////        }
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    public void StartRunning()
//////    {
//////        isRunning = true;
//////        SetState(CowState.Run);
//////        Debug.Log("[Cow] StartRunning!");
//////    }

//////    public void StopRunning() => isRunning = false;

//////    // ──────────────────────────────────────────────────────────
//////    public void PlayJump()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Jump) return;
//////        if (CurrentState == CowState.Hit) return;

//////        Debug.Log("[Cow] Jump!");

//////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
//////        jumpArcCo = StartCoroutine(JumpArc());

//////        SetState(CowState.Jump);
//////    }

//////    public void OnJumpPressed() => PlayJump();
//////    public void OnJumpDown() => PlayJump();

//////    public void PlayHit()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Hit) return;
//////        if (CurrentState == CowState.Win) return;
//////        hitThenWin = false;
//////        SetState(CowState.Hit);
//////    }

//////    public void PlayHitThenWin()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        Debug.Log("[Cow] PlayHitThenWin!");
//////        hitThenWin = true;
//////        isRunning = false;
//////        SetState(CowState.Hit);
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // ✅ JumpArc — moves cow UP (Y) and FORWARD (worldX) simultaneously
//////    // ──────────────────────────────────────────────────────────
//////    IEnumerator JumpArc()
//////    {
//////        isJumping = true;   // ✅ pause Update from adding runSpeed
//////        float elapsed = 0f;

//////        while (elapsed < jumpDuration)
//////        {
//////            if (!isAlive) yield break;

//////            elapsed += Time.deltaTime;
//////            float t = Mathf.Clamp01(elapsed / jumpDuration);

//////            // ✅ Move forward faster than normal run during jump
//////            worldX += jumpForwardSpeed * Time.deltaTime;

//////            // ✅ Parabola arc upward
//////            float height = jumpHeight * 4f * t * (1f - t);
//////            cowRect.anchoredPosition = new Vector2(
//////                cowRect.anchoredPosition.x,
//////                groundY + height
//////            );

//////            yield return null;
//////        }

//////        // ✅ Snap back to ground
//////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//////        isJumping = false;
//////        jumpArcCo = null;

//////        Debug.Log("[Cow] Landed!");
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    void SetState(CowState newState)
//////    {
//////        if (!isAlive) return;
//////        CurrentState = newState;
//////        Debug.Log($"[Cow] → {newState}");

//////        if (animLoop != null) StopCoroutine(animLoop);
//////        animLoop = StartCoroutine(MasterLoop());
//////    }

//////    IEnumerator MasterLoop()
//////    {
//////        int frame = 0;
//////        while (true)
//////        {
//////            if (!isAlive) yield break;

//////            Sprite[] frames = null;
//////            float fps = 12f;

//////            switch (CurrentState)
//////            {
//////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////            }

//////            if (frames == null || frames.Length == 0)
//////            {
//////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////                yield return new WaitForSeconds(0.1f);
//////                continue;
//////            }

//////            if (frame >= frames.Length) frame = 0;
//////            cowImage.sprite = frames[frame];
//////            cowRect.sizeDelta = originalSize;
//////            frame++;

//////            // Jump → ONCE → back to Run
//////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////            }

//////            // Hit → ONCE → Win or Run
//////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                if (hitThenWin)
//////                {
//////                    hitThenWin = false;
//////                    CurrentState = CowState.Win;
//////                    Debug.Log("[Cow] → Celebration!");
//////                }
//////                else
//////                {
//////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////                }
//////            }

//////            if (frame >= frames.Length) frame = 0;

//////            yield return new WaitForSeconds(1f / fps);
//////        }
//////    }
//////}

////using UnityEngine;
////using UnityEngine.UI;
////using System.Collections;

////public class Cowanimationcontroller : MonoBehaviour
////{
////    public enum CowState { Idle, Run, Jump, Hit, Win }

////    [Header("Auto Run")]
////    public float runSpeed = 200f;

////    [Header("Jump Physics")]
////    public float jumpHeight = 250f;   // pixels — how high
////    public float jumpDuration = 0.7f;   // seconds — time in air
////    public float jumpForwardSpeed = 400f;   // pixels/sec forward during jump
////    public int maxJumps = 2;      // ✅ 1 = single jump, 2 = double jump

////    [Header("Idle Animation")]
////    public Sprite[] idleFrames;
////    public float idleFPS = 12f;

////    [Header("Run Animation")]
////    public Sprite[] runFrames;
////    public float runFPS = 12f;

////    [Header("Jump Animation")]
////    public Sprite[] jumpFrames;
////    public float jumpFPS = 12f;

////    [Header("Hit Animation")]
////    public Sprite[] hitFrames;
////    public float hitFPS = 12f;

////    [Header("Win / Celebration Animation")]
////    public Sprite[] winFrames;
////    public float winFPS = 12f;

////    // ── Private ────────────────────────────────────────────────
////    private Image cowImage;
////    private RectTransform cowRect;
////    private Vector2 originalSize;
////    private bool isAlive = false;
////    private bool hitThenWin = false;
////    private float groundY;
////    private bool isJumping = false;

////    // ✅ Double jump counter
////    private int jumpsLeft = 0;

////    private Coroutine animLoop = null;
////    private Coroutine jumpArcCo = null;

////    public CowState CurrentState { get; private set; } = CowState.Idle;

////    [HideInInspector] public float worldX = 0f;
////    [HideInInspector] public bool isRunning = false;

////    // ──────────────────────────────────────────────────────────
////    void Start()
////    {
////        isAlive = true;
////        cowImage = GetComponent<Image>();
////        cowRect = GetComponent<RectTransform>();
////        originalSize = cowRect.sizeDelta;
////        worldX = cowRect.anchoredPosition.x;
////        groundY = cowRect.anchoredPosition.y;
////        SetState(CowState.Idle);
////    }

////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////    // ──────────────────────────────────────────────────────────
////    void Update()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        if (CurrentState == CowState.Hit) return;

////        if (isRunning)
////        {
////            if (!isJumping)
////                worldX += runSpeed * Time.deltaTime;

////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////                PlayJump();
////        }
////    }

////    // ──────────────────────────────────────────────────────────
////    public void StartRunning()
////    {
////        isRunning = true;
////        jumpsLeft = maxJumps;   // ✅ reset on game start
////        SetState(CowState.Run);
////        Debug.Log("[Cow] StartRunning!");
////    }

////    public void StopRunning() => isRunning = false;

////    // ──────────────────────────────────────────────────────────
////    public void PlayJump()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        if (CurrentState == CowState.Hit) return;

////        // ✅ Allow jump only if jumpsLeft > 0
////        if (jumpsLeft <= 0) return;

////        jumpsLeft--;
////        Debug.Log($"[Cow] Jump! Jumps left: {jumpsLeft}");

////        // ✅ Restart arc from CURRENT Y position for double jump feel
////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
////        jumpArcCo = StartCoroutine(JumpArc());

////        SetState(CowState.Jump);
////    }

////    public void OnJumpPressed() => PlayJump();
////    public void OnJumpDown() => PlayJump();

////    public void PlayHit()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Hit) return;
////        if (CurrentState == CowState.Win) return;
////        hitThenWin = false;
////        SetState(CowState.Hit);
////    }

////    public void PlayHitThenWin()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        Debug.Log("[Cow] PlayHitThenWin!");
////        hitThenWin = true;
////        isRunning = false;
////        SetState(CowState.Hit);
////    }

////    // ──────────────────────────────────────────────────────────
////    // JumpArc — arcs from CURRENT Y so double jump stacks naturally
////    // ──────────────────────────────────────────────────────────
////    IEnumerator JumpArc()
////    {
////        isJumping = true;

////        float elapsed = 0f;
////        float startY = cowRect.anchoredPosition.y;   // ✅ start from wherever cow is now
////        float peakY = startY + jumpHeight;

////        while (elapsed < jumpDuration)
////        {
////            if (!isAlive) yield break;

////            elapsed += Time.deltaTime;
////            float t = Mathf.Clamp01(elapsed / jumpDuration);

////            // ✅ Move forward faster than run
////            worldX += jumpForwardSpeed * Time.deltaTime;

////            // ✅ Parabola from startY → peak → groundY
////            float height = Mathf.Lerp(startY, groundY, t) + jumpHeight * 4f * t * (1f - t);
////            cowRect.anchoredPosition = new Vector2(
////                cowRect.anchoredPosition.x,
////                height
////            );

////            yield return null;
////        }

////        // Snap to ground
////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////        isJumping = false;
////        jumpsLeft = maxJumps;   // ✅ reset jumps when landed
////        jumpArcCo = null;
////        Debug.Log("[Cow] Landed! Jumps reset.");
////    }

////    // ──────────────────────────────────────────────────────────
////    void SetState(CowState newState)
////    {
////        if (!isAlive) return;
////        CurrentState = newState;
////        Debug.Log($"[Cow] → {newState}");

////        if (animLoop != null) StopCoroutine(animLoop);
////        animLoop = StartCoroutine(MasterLoop());
////    }

////    IEnumerator MasterLoop()
////    {
////        int frame = 0;
////        while (true)
////        {
////            if (!isAlive) yield break;

////            Sprite[] frames = null;
////            float fps = 12f;

////            switch (CurrentState)
////            {
////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////            }

////            if (frames == null || frames.Length == 0)
////            {
////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////                yield return new WaitForSeconds(0.1f);
////                continue;
////            }

////            if (frame >= frames.Length) frame = 0;
////            cowImage.sprite = frames[frame];
////            cowRect.sizeDelta = originalSize;
////            frame++;

////            // Jump → ONCE → back to Run
////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////            {
////                frame = 0;
////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////            }

////            // Hit → ONCE → Win or Run
////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////            {
////                frame = 0;
////                if (hitThenWin)
////                {
////                    hitThenWin = false;
////                    CurrentState = CowState.Win;
////                    Debug.Log("[Cow] → Celebration!");
////                }
////                else
////                {
////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////                }
////            }

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
//    public enum CowState { Idle, Run, Jump, Hit, Win }

//    [Header("Auto Run")]
//    public float runSpeed = 200f;

//    [Header("Jump Physics")]
//    public float jumpHeight = 250f;  // pixels — how high
//    public float jumpDuration = 0.7f;  // seconds — time in air
//    public float jumpForwardSpeed = 400f; // pixels/sec forward during jump
//    public int maxJumps = 2;     // 1 = single jump, 2 = double jump

//    [Header("Jump Size")]
//    [Tooltip("1 = same size as run/idle.  1.3 = 30% bigger.  Drag to match your jump sprites.")]
//    [Range(0.5f, 3f)]
//    public float jumpScale = 1f;

//    [Header("Win Size")]
//    [Tooltip("1 = same size as run/idle.  1.3 = 30% bigger.  Drag to match your win sprites.")]
//    [Range(0.5f, 3f)]
//    public float winScale = 1f;

//    [Header("Idle Animation")]
//    public Sprite[] idleFrames;
//    public float idleFPS = 12f;

//    [Header("Run Animation")]
//    public Sprite[] runFrames;
//    public float runFPS = 12f;

//    [Header("Jump Animation")]
//    public Sprite[] jumpFrames;
//    public float jumpFPS = 12f;

//    [Header("Hit Animation")]
//    public Sprite[] hitFrames;
//    public float hitFPS = 12f;

//    [Header("Hit Bounce")]
//    [Tooltip("How high the cow bounces up when hit plays. 0 = no bounce, 150 = noticeable bump.")]
//    public float hitBounceHeight = 80f;
//    [Tooltip("How long the bounce takes in seconds.")]
//    public float hitBounceDuration = 0.35f;

//    [Header("Win / Celebration Animation")]
//    public Sprite[] winFrames;
//    public float winFPS = 12f;

//    // ── Private ────────────────────────────────────────────────
//    private Image cowImage;
//    private RectTransform cowRect;
//    private Vector2 originalSize;
//    private bool isAlive = false;
//    private bool hitThenWin = false;
//    private float groundY;
//    private bool isJumping = false;
//    private int jumpsLeft = 0;

//    private Coroutine animLoop = null;
//    private Coroutine jumpArcCo = null;

//    public CowState CurrentState { get; private set; } = CowState.Idle;

//    [HideInInspector] public float worldX = 0f;
//    [HideInInspector] public bool isRunning = false;

//    // ──────────────────────────────────────────────────────────
//    void Start()
//    {
//        isAlive = true;
//        cowImage = GetComponent<Image>();
//        cowRect = GetComponent<RectTransform>();
//        originalSize = cowRect.sizeDelta;
//        worldX = cowRect.anchoredPosition.x;
//        groundY = cowRect.anchoredPosition.y;
//        SetState(CowState.Idle);
//    }

//    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//    // ──────────────────────────────────────────────────────────
//    void Update()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Win) return;
//        if (CurrentState == CowState.Hit) return;

//        if (isRunning)
//        {
//            if (!isJumping)
//                worldX += runSpeed * Time.deltaTime;

//            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//                PlayJump();
//        }
//    }

//    // ──────────────────────────────────────────────────────────
//    public void StartRunning()
//    {
//        isRunning = true;
//        jumpsLeft = maxJumps;
//        SetState(CowState.Run);
//        Debug.Log("[Cow] StartRunning!");
//    }

//    public void StopRunning()
//    {
//        isRunning = false;
//    }

//    // ──────────────────────────────────────────────────────────
//    public void PlayJump()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Win) return;
//        if (CurrentState == CowState.Hit) return;
//        if (jumpsLeft <= 0) return;

//        jumpsLeft--;
//        Debug.Log($"[Cow] Jump! Jumps left: {jumpsLeft}");

//        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
//        jumpArcCo = StartCoroutine(JumpArc());

//        SetState(CowState.Jump);
//    }

//    public void OnJumpPressed() => PlayJump();
//    public void OnJumpDown() => PlayJump();

//    // ──────────────────────────────────────────────────────────
//    // Called by ObstacleController when cow hits an obstacle
//    // ──────────────────────────────────────────────────────────
//    public void PlayHit()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Hit) return;
//        if (CurrentState == CowState.Win) return;

//        // Stop any jump in progress
//        if (jumpArcCo != null)
//        {
//            StopCoroutine(jumpArcCo);
//            jumpArcCo = null;
//        }
//        isJumping = false;
//        isRunning = false;
//        hitThenWin = false;

//        // ✅ Snap to ground first, then play controllable bounce arc
//        if (cowRect != null)
//            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);

//        SetState(CowState.Hit);
//        StartCoroutine(HitBounceArc());
//    }

//    // ──────────────────────────────────────────────────────────
//    // Hit bounce arc — parabola up then back to groundY
//    // Controlled by hitBounceHeight and hitBounceDuration in Inspector
//    // ──────────────────────────────────────────────────────────
//    IEnumerator HitBounceArc()
//    {
//        if (hitBounceHeight <= 0f) yield break;   // 0 = no bounce, skip

//        float elapsed = 0f;
//        float startY = groundY;

//        while (elapsed < hitBounceDuration)
//        {
//            if (!isAlive) yield break;

//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / hitBounceDuration);

//            // Parabola: groundY → peak → groundY
//            float y = startY + hitBounceHeight * 4f * t * (1f - t);
//            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, y);

//            yield return null;
//        }

//        // Snap back to ground
//        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//    }

//    // Called by BellTrigger — Hit once then Win celebration
//    public void PlayHitThenWin()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Win) return;

//        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
//        isJumping = false;
//        isRunning = false;
//        if (cowRect != null)
//            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//        hitThenWin = true;
//        SetState(CowState.Hit);
//        Debug.Log("[Cow] PlayHitThenWin!");
//    }

//    // ──────────────────────────────────────────────────────────
//    // Called by BellTrigger after waiting for Hit animation to finish.
//    // Transitions directly to Win (celebration) animation.
//    // ──────────────────────────────────────────────────────────
//    public void PlayWinDirect()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Win) return;

//        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
//        isJumping = false;
//        isRunning = false;
//        hitThenWin = false;

//        // ✅ Apply scale immediately so it's visible on the very first frame
//        if (cowRect != null)
//            cowRect.sizeDelta = originalSize * winScale;

//        SetState(CowState.Win);
//        Debug.Log("[Cow] PlayWinDirect — celebration!");
//    }

//    // ──────────────────────────────────────────────────────────
//    // Jump arc — parabola from current Y back to groundY
//    // ──────────────────────────────────────────────────────────
//    IEnumerator JumpArc()
//    {
//        isJumping = true;

//        float elapsed = 0f;
//        float startY = cowRect.anchoredPosition.y;

//        while (elapsed < jumpDuration)
//        {
//            if (!isAlive) yield break;
//            if (CurrentState == CowState.Hit) yield break; // stop arc on hit

//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / jumpDuration);

//            worldX += jumpForwardSpeed * Time.deltaTime;

//            // Parabola: startY → peak → groundY
//            float height = Mathf.Lerp(startY, groundY, t) + jumpHeight * 4f * t * (1f - t);
//            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, height);

//            yield return null;
//        }

//        // Snap to ground and reset
//        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//        isJumping = false;
//        jumpsLeft = maxJumps;
//        jumpArcCo = null;

//        if (CurrentState == CowState.Jump)
//            SetState(isRunning ? CowState.Run : CowState.Idle);

//        Debug.Log("[Cow] Landed!");
//    }

//    // ──────────────────────────────────────────────────────────
//    void SetState(CowState newState)
//    {
//        if (!isAlive) return;
//        CurrentState = newState;
//        Debug.Log($"[Cow] → {newState}");

//        if (animLoop != null) StopCoroutine(animLoop);
//        animLoop = StartCoroutine(MasterLoop());
//    }

//    IEnumerator MasterLoop()
//    {
//        int frame = 0;
//        while (true)
//        {
//            if (!isAlive) yield break;

//            Sprite[] frames = null;
//            float fps = 12f;

//            switch (CurrentState)
//            {
//                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//                case CowState.Run: frames = runFrames; fps = runFPS; break;
//                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//                case CowState.Hit: frames = jumpFrames; fps = jumpFPS; break;
//                case CowState.Win: frames = winFrames; fps = winFPS; break;
//            }

//            if (frames == null || frames.Length == 0)
//            {
//                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//                yield return new WaitForSeconds(0.1f);
//                continue;
//            }

//            if (frame >= frames.Length) frame = 0;

//            cowImage.sprite = frames[frame];

//            // Apply scale based on state
//            if (CurrentState == CowState.Jump)
//                cowRect.sizeDelta = originalSize * jumpScale;
//            else if (CurrentState == CowState.Win)
//                cowRect.sizeDelta = originalSize * winScale;
//            else
//                cowRect.sizeDelta = originalSize;





//            frame++;

//            // Jump → plays once → handled by JumpArc landing
//            if (CurrentState == CowState.Jump && frame >= frames.Length)
//            {
//                frame = 0;
//                // Keep looping jump frames until JumpArc finishes
//            }

//            // Hit → plays once → Win or Idle
//            if (CurrentState == CowState.Hit && frame >= frames.Length)
//            {
//                frame = 0;
//                if (hitThenWin)
//                {
//                    hitThenWin = false;
//                    CurrentState = CowState.Win;
//                    Debug.Log("[Cow] → Celebration!");
//                }
//                else
//                {
//                    // Game over — stay in Hit / Idle, GameManager handles panel
//                    CurrentState = CowState.Idle;
//                }
//            }

//            if (frame >= frames.Length) frame = 0;

//            yield return new WaitForSeconds(1f / fps);
//        }
//    }
//}

////////////////using UnityEngine;
////////////////using UnityEngine.UI;
////////////////using System.Collections;

////////////////public class Cowanimationcontroller : MonoBehaviour
////////////////{
////////////////    public enum CowState { Idle, SideRun, Jump, Hit, Win }

////////////////    [Header("Movement")]
////////////////    public float moveSpeed = 500f;

////////////////    [Header("Animations")]
////////////////    public Sprite[] idleFrames;
////////////////    public Sprite[] runFrames;
////////////////    public Sprite[] jumpFrames;
////////////////    public Sprite[] hitFrames;
////////////////    public Sprite[] winFrames;

////////////////    public float fps = 12f;

////////////////    private Image cowImage;
////////////////    private RectTransform cowRect;

////////////////    private CowState currentState;

////////////////    private bool isRunning = false;
////////////////    private bool isJumping = false;   // ✅ FIX: prevent loop

////////////////    [HideInInspector] public float worldX = 0f;

////////////////    void Start()
////////////////    {
////////////////        cowImage = GetComponent<Image>();
////////////////        cowRect = GetComponent<RectTransform>();

////////////////        worldX = cowRect.anchoredPosition.x;

////////////////        SetState(CowState.Idle);
////////////////    }

////////////////    void Update()
////////////////    {
////////////////        if (!isRunning) return;

////////////////        // ✅ AUTO RUN
////////////////        worldX += moveSpeed * Time.deltaTime;

////////////////        // ✅ KEYBOARD JUMP
////////////////        if (Input.GetKeyDown(KeyCode.Space))
////////////////            PlayJump();
////////////////    }

////////////////    // =========================
////////////////    // GAME CONTROL
////////////////    // =========================
////////////////    public void StartRunning()
////////////////    {
////////////////        isRunning = true;
////////////////        SetState(CowState.SideRun);
////////////////    }

////////////////    public void StopRunning()
////////////////    {
////////////////        isRunning = false;
////////////////    }

////////////////    // =========================
////////////////    // ACTIONS
////////////////    // =========================
////////////////    public void PlayJump()
////////////////    {
////////////////        if (isJumping) return;              // 🚫 prevent loop
////////////////        if (currentState == CowState.Win) return;

////////////////        isJumping = true;
////////////////        SetState(CowState.Jump);
////////////////    }

////////////////    public void PlayHit()
////////////////    {
////////////////        if (currentState == CowState.Win) return;
////////////////        SetState(CowState.Hit);
////////////////    }

////////////////    public void PlayWin()
////////////////    {
////////////////        StopRunning();
////////////////        SetState(CowState.Win);

////////////////        GameManager.Instance?.SendMessage("ShowCongrats");
////////////////    }

////////////////    public void PlayHitThenWin()
////////////////    {
////////////////        StartCoroutine(HitThenWinRoutine());
////////////////    }

////////////////    IEnumerator HitThenWinRoutine()
////////////////    {
////////////////        SetState(CowState.Hit);
////////////////        yield return new WaitForSeconds(0.5f);
////////////////        PlayWin();
////////////////    }

////////////////    // UI Button
////////////////    public void OnJumpDown()
////////////////    {
////////////////        PlayJump();
////////////////    }

////////////////    // =========================
////////////////    void SetState(CowState state)
////////////////    {
////////////////        currentState = state;
////////////////        StopAllCoroutines();
////////////////        StartCoroutine(Animate());
////////////////    }

////////////////    IEnumerator Animate()
////////////////    {
////////////////        Sprite[] frames = idleFrames;

////////////////        switch (currentState)
////////////////        {
////////////////            case CowState.Idle: frames = idleFrames; break;
////////////////            case CowState.SideRun: frames = runFrames; break;
////////////////            case CowState.Jump: frames = jumpFrames; break;
////////////////            case CowState.Hit: frames = hitFrames; break;
////////////////            case CowState.Win: frames = winFrames; break;
////////////////        }

////////////////        int i = 0;

////////////////        while (true)
////////////////        {
////////////////            if (frames == null || frames.Length == 0)
////////////////                yield break;

////////////////            cowImage.sprite = frames[i];
////////////////            i++;

////////////////            // ✅ JUMP END FIX
////////////////            if (currentState == CowState.Jump && i >= frames.Length)
////////////////            {
////////////////                isJumping = false;                 // 🔥 unlock jump
////////////////                SetState(CowState.SideRun);        // back to run
////////////////                yield break;
////////////////            }

////////////////            // ✅ HIT END
////////////////            if (currentState == CowState.Hit && i >= frames.Length)
////////////////            {
////////////////                SetState(CowState.SideRun);
////////////////                yield break;
////////////////            }

////////////////            // LOOP states
////////////////            if (i >= frames.Length)
////////////////                i = 0;

////////////////            yield return new WaitForSeconds(1f / fps);
////////////////        }
////////////////    }
////////////////}

//////////////using UnityEngine;
//////////////using UnityEngine.UI;
//////////////using System.Collections;

//////////////public class Cowanimationcontroller : MonoBehaviour
//////////////{
//////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////////    [Header("Auto Run")]
//////////////    public float runSpeed = 200f;

//////////////    [Header("Idle Animation")]
//////////////    public Sprite[] idleFrames;
//////////////    public float idleFPS = 12f;

//////////////    [Header("Run Animation")]
//////////////    public Sprite[] runFrames;
//////////////    public float runFPS = 12f;

//////////////    [Header("Jump Animation")]
//////////////    public Sprite[] jumpFrames;
//////////////    public float jumpFPS = 12f;

//////////////    [Header("Hit Animation")]
//////////////    public Sprite[] hitFrames;
//////////////    public float hitFPS = 12f;

//////////////    [Header("Win / Celebration Animation")]
//////////////    public Sprite[] winFrames;
//////////////    public float winFPS = 12f;

//////////////    private Image cowImage;
//////////////    private RectTransform cowRect;
//////////////    private Vector2 originalSize;
//////////////    private bool isAlive = false;
//////////////    private bool hitThenWin = false;

//////////////    // ✅ PUBLIC — BellTrigger reads this to check if cow is jumping
//////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////////    [HideInInspector] public float worldX = 0f;
//////////////    [HideInInspector] public bool isRunning = false;

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Start()
//////////////    {
//////////////        isAlive = true;
//////////////        cowImage = GetComponent<Image>();
//////////////        cowRect = GetComponent<RectTransform>();
//////////////        originalSize = cowRect.sizeDelta;
//////////////        worldX = cowRect.anchoredPosition.x;
//////////////        SetState(CowState.Idle);

//////////////        if (GameManager.Instance != null)
//////////////        {
//////////////            GameManager.Instance.OnGameStart += StartRunning;
//////////////            GameManager.Instance.OnGameWin += StopRunning;
//////////////        }
//////////////    }

//////////////    void OnDestroy()
//////////////    {
//////////////        isAlive = false;
//////////////        StopAllCoroutines();
//////////////        if (GameManager.Instance != null)
//////////////        {
//////////////            GameManager.Instance.OnGameStart -= StartRunning;
//////////////            GameManager.Instance.OnGameWin -= StopRunning;
//////////////        }
//////////////    }

//////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void Update()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Hit) return;

//////////////        if (isRunning)
//////////////        {
//////////////            worldX += runSpeed * Time.deltaTime;

//////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////////                PlayJump();
//////////////        }
//////////////    }

//////////////    void StartRunning() { isRunning = true; SetState(CowState.Run); }
//////////////    void StopRunning() { isRunning = false; }

//////////////    // ──────────────────────────────────────────────────────────
//////////////    public void PlayJump()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        if (CurrentState == CowState.Jump) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        SetState(CowState.Jump);
//////////////    }

//////////////    public void PlayHit()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Hit) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        hitThenWin = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    // ✅ Called by BellTrigger — Hit once → then Celebration loops
//////////////    public void PlayHitThenWin()
//////////////    {
//////////////        if (!isAlive) return;
//////////////        if (CurrentState == CowState.Win) return;
//////////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////////        hitThenWin = true;
//////////////        isRunning = false;
//////////////        SetState(CowState.Hit);
//////////////    }

//////////////    public void OnJumpPressed() => PlayJump();

//////////////    // ──────────────────────────────────────────────────────────
//////////////    void SetState(CowState newState)
//////////////    {
//////////////        if (!isAlive) return;
//////////////        CurrentState = newState;
//////////////        Debug.Log($"[Cow] → {newState}");
//////////////        StopAllCoroutines();
//////////////        StartCoroutine(MasterLoop());
//////////////    }

//////////////    IEnumerator MasterLoop()
//////////////    {
//////////////        int frame = 0;
//////////////        while (true)
//////////////        {
//////////////            if (!isAlive) yield break;

//////////////            Sprite[] frames = null;
//////////////            float fps = 12f;

//////////////            switch (CurrentState)
//////////////            {
//////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////////            }

//////////////            if (frames == null || frames.Length == 0)
//////////////            {
//////////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////////////                yield return new WaitForSeconds(0.1f);
//////////////                continue;
//////////////            }

//////////////            if (frame >= frames.Length) frame = 0;
//////////////            cowImage.sprite = frames[frame];
//////////////            cowRect.sizeDelta = originalSize;
//////////////            frame++;

//////////////            // Jump → ONCE → back to Run
//////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////            }

//////////////            // Hit → ONCE → Win or Run
//////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////////            {
//////////////                frame = 0;
//////////////                if (hitThenWin)
//////////////                {
//////////////                    hitThenWin = false;
//////////////                    CurrentState = CowState.Win;
//////////////                    Debug.Log("[Cow] Hit done → Celebration!");
//////////////                }
//////////////                else
//////////////                {
//////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////////                }
//////////////            }

//////////////            // Idle / Run / Win → loop forever
//////////////            if (frame >= frames.Length) frame = 0;

//////////////            yield return new WaitForSeconds(1f / fps);
//////////////        }
//////////////    }
//////////////}
////////////using UnityEngine;
////////////using UnityEngine.UI;
////////////using System.Collections;

////////////public class Cowanimationcontroller : MonoBehaviour
////////////{
////////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////////    [Header("Auto Run")]
////////////    public float runSpeed = 200f;

////////////    [Header("Idle Animation")]
////////////    public Sprite[] idleFrames;
////////////    public float idleFPS = 12f;

////////////    [Header("Run Animation")]
////////////    public Sprite[] runFrames;
////////////    public float runFPS = 12f;

////////////    [Header("Jump Animation")]
////////////    public Sprite[] jumpFrames;
////////////    public float jumpFPS = 12f;

////////////    [Header("Hit Animation")]
////////////    public Sprite[] hitFrames;
////////////    public float hitFPS = 12f;

////////////    [Header("Win / Celebration Animation")]
////////////    public Sprite[] winFrames;
////////////    public float winFPS = 12f;

////////////    private Image cowImage;
////////////    private RectTransform cowRect;
////////////    private Vector2 originalSize;
////////////    private bool isAlive = false;
////////////    private bool hitThenWin = false;

////////////    // ✅ PUBLIC — GameManager calls StartRunning(), BellTrigger reads CurrentState
////////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////////    [HideInInspector] public float worldX = 0f;
////////////    [HideInInspector] public bool isRunning = false;

////////////    // ──────────────────────────────────────────────────────────
////////////    void Start()
////////////    {
////////////        isAlive = true;
////////////        cowImage = GetComponent<Image>();
////////////        cowRect = GetComponent<RectTransform>();
////////////        originalSize = cowRect.sizeDelta;
////////////        worldX = cowRect.anchoredPosition.x;
////////////        SetState(CowState.Idle);
////////////    }

////////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////////    // ──────────────────────────────────────────────────────────
////////////    void Update()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Hit) return;

////////////        if (isRunning)
////////////        {
////////////            worldX += runSpeed * Time.deltaTime;

////////////            // Jump — Space or Up Arrow
////////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////////                PlayJump();
////////////        }
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    // ✅ Called by GameManager.CountdownRoutine() after GO!!
////////////    // ──────────────────────────────────────────────────────────
////////////    public void StartRunning()
////////////    {
////////////        isRunning = true;
////////////        SetState(CowState.Run);
////////////        Debug.Log("[Cow] StartRunning called by GameManager!");
////////////    }

////////////    public void StopRunning()
////////////    {
////////////        isRunning = false;
////////////    }

////////////    // ──────────────────────────────────────────────────────────
////////////    // Public methods
////////////    // ──────────────────────────────────────────────────────────
////////////    public void PlayIdle()
////////////    {
////////////        if (!isAlive) return;
////////////        SetState(CowState.Idle);
////////////    }

////////////    public void PlayJump()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        if (CurrentState == CowState.Jump) return;
////////////        if (CurrentState == CowState.Hit) return;
////////////        SetState(CowState.Jump);
////////////    }

////////////    public void PlayHit()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Hit) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        hitThenWin = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    // ✅ Called by BellTrigger — Hit once → Celebration loops
////////////    public void PlayHitThenWin()
////////////    {
////////////        if (!isAlive) return;
////////////        if (CurrentState == CowState.Win) return;
////////////        Debug.Log("[Cow] PlayHitThenWin!");
////////////        hitThenWin = true;
////////////        isRunning = false;
////////////        SetState(CowState.Hit);
////////////    }

////////////    // On-screen jump button
////////////    public void OnJumpPressed() => PlayJump();

////////////    // ──────────────────────────────────────────────────────────
////////////    void SetState(CowState newState)
////////////    {
////////////        if (!isAlive) return;
////////////        CurrentState = newState;
////////////        Debug.Log($"[Cow] → {newState}");
////////////        StopAllCoroutines();
////////////        StartCoroutine(MasterLoop());
////////////    }

////////////    IEnumerator MasterLoop()
////////////    {
////////////        int frame = 0;
////////////        while (true)
////////////        {
////////////            if (!isAlive) yield break;

////////////            Sprite[] frames = null;
////////////            float fps = 12f;

////////////            switch (CurrentState)
////////////            {
////////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////////            }

////////////            if (frames == null || frames.Length == 0)
////////////            {
////////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////////////                yield return new WaitForSeconds(0.1f);
////////////                continue;
////////////            }

////////////            if (frame >= frames.Length) frame = 0;
////////////            cowImage.sprite = frames[frame];
////////////            cowRect.sizeDelta = originalSize;
////////////            frame++;

////////////            // Jump → ONCE → back to Run
////////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////            }

////////////            // Hit → ONCE → Win or Run
////////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////////            {
////////////                frame = 0;
////////////                if (hitThenWin)
////////////                {
////////////                    hitThenWin = false;
////////////                    CurrentState = CowState.Win;
////////////                    Debug.Log("[Cow] → Celebration!");
////////////                }
////////////                else
////////////                {
////////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////////                }
////////////            }

////////////            // Idle / Run / Win → loop forever
////////////            if (frame >= frames.Length) frame = 0;

////////////            yield return new WaitForSeconds(1f / fps);
////////////        }
////////////    }
////////////}
//////////using UnityEngine;
//////////using UnityEngine.UI;
//////////using System.Collections;

//////////public class Cowanimationcontroller : MonoBehaviour
//////////{
//////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////    [Header("Auto Run")]
//////////    public float runSpeed = 200f;

//////////    [Header("Idle Animation")]
//////////    public Sprite[] idleFrames;
//////////    public float idleFPS = 12f;

//////////    [Header("Run Animation")]
//////////    public Sprite[] runFrames;
//////////    public float runFPS = 12f;

//////////    [Header("Jump Animation")]
//////////    public Sprite[] jumpFrames;
//////////    public float jumpFPS = 12f;

//////////    [Header("Hit Animation")]
//////////    public Sprite[] hitFrames;
//////////    public float hitFPS = 12f;

//////////    [Header("Win / Celebration Animation")]
//////////    public Sprite[] winFrames;
//////////    public float winFPS = 12f;

//////////    private Image cowImage;
//////////    private RectTransform cowRect;
//////////    private Vector2 originalSize;
//////////    private bool isAlive = false;
//////////    private bool hitThenWin = false;

//////////    // BellTrigger reads CurrentState to know if cow is jumping
//////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////    [HideInInspector] public float worldX = 0f;
//////////    [HideInInspector] public bool isRunning = false;

//////////    // ──────────────────────────────────────────────────────────
//////////    void Start()
//////////    {
//////////        isAlive = true;
//////////        cowImage = GetComponent<Image>();
//////////        cowRect = GetComponent<RectTransform>();
//////////        originalSize = cowRect.sizeDelta;
//////////        worldX = cowRect.anchoredPosition.x;
//////////        SetState(CowState.Idle);
//////////    }

//////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////    // ──────────────────────────────────────────────────────────
//////////    void Update()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Hit) return;

//////////        if (isRunning)
//////////        {
//////////            worldX += runSpeed * Time.deltaTime;

//////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////                PlayJump();
//////////        }
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    // Called by GameManager countdown after "GO!!"
//////////    // ──────────────────────────────────────────────────────────
//////////    public void StartRunning()
//////////    {
//////////        isRunning = true;
//////////        SetState(CowState.Run);
//////////        Debug.Log("[Cow] StartRunning!");
//////////    }

//////////    public void StopRunning()
//////////    {
//////////        isRunning = false;
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    // Public actions
//////////    // ──────────────────────────────────────────────────────────
//////////    public void PlayIdle()
//////////    {
//////////        if (!isAlive) return;
//////////        SetState(CowState.Idle);
//////////    }

//////////    public void PlayJump()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Jump) return;
//////////        if (CurrentState == CowState.Hit) return;
//////////        Debug.Log("[Cow] PlayJump!");
//////////        SetState(CowState.Jump);
//////////    }

//////////    // ── UI Button hooks ────────────────────────────────────────
//////////    // Both names wired here so either old or new button reference works
//////////    public void OnJumpPressed() => PlayJump();
//////////    public void OnJumpDown() => PlayJump(); // legacy alias — keep button working

//////////    // ──────────────────────────────────────────────────────────
//////////    public void PlayHit()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Hit) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        hitThenWin = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    // Called by BellTrigger: plays Hit once, then loops Win/celebration
//////////    public void PlayHitThenWin()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////        hitThenWin = true;
//////////        isRunning = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    void SetState(CowState newState)
//////////    {
//////////        if (!isAlive) return;
//////////        CurrentState = newState;
//////////        Debug.Log($"[Cow] State → {newState}");
//////////        StopAllCoroutines();
//////////        StartCoroutine(MasterLoop());
//////////    }

//////////    IEnumerator MasterLoop()
//////////    {
//////////        int frame = 0;
//////////        while (true)
//////////        {
//////////            if (!isAlive) yield break;

//////////            Sprite[] frames = null;
//////////            float fps = 12f;

//////////            switch (CurrentState)
//////////            {
//////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////            }

//////////            if (frames == null || frames.Length == 0)
//////////            {
//////////                Debug.LogError($"[Cow] {CurrentState} frames are EMPTY — assign sprites in Inspector!");
//////////                yield return new WaitForSeconds(0.1f);
//////////                continue;
//////////            }

//////////            if (frame >= frames.Length) frame = 0;
//////////            cowImage.sprite = frames[frame];
//////////            cowRect.sizeDelta = originalSize;
//////////            frame++;

//////////            // Jump plays ONCE then returns to Run
//////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////            }

//////////            // Hit plays ONCE then either Win or Run
//////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                if (hitThenWin)
//////////                {
//////////                    hitThenWin = false;
//////////                    CurrentState = CowState.Win;
//////////                    Debug.Log("[Cow] Hit done → Win/Celebration!");
//////////                }
//////////                else
//////////                {
//////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////                }
//////////            }

//////////            // Idle / Run / Win loop forever
//////////            if (frame >= frames.Length) frame = 0;

//////////            yield return new WaitForSeconds(1f / fps);
//////////        }
//////////    }
//////////}

//////////using UnityEngine;
//////////using UnityEngine.UI;
//////////using System.Collections;

//////////public class Cowanimationcontroller : MonoBehaviour
//////////{
//////////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////////    [Header("Auto Run")]
//////////    public float runSpeed = 200f;

//////////    [Header("Idle Animation")]
//////////    public Sprite[] idleFrames;
//////////    public float idleFPS = 12f;

//////////    [Header("Run Animation")]
//////////    public Sprite[] runFrames;
//////////    public float runFPS = 12f;

//////////    [Header("Jump Animation")]
//////////    public Sprite[] jumpFrames;
//////////    public float jumpFPS = 12f;

//////////    [Header("Jump Size")]
//////////    [Tooltip("1 = same as run/idle size.  1.3 = 30% bigger.  Try values between 1.0 and 2.0.")]
//////////    [Range(0.5f, 3f)]
//////////    public float jumpScale = 1f;   // ← drag this slider in the Inspector

//////////    [Header("Hit Animation")]
//////////    public Sprite[] hitFrames;
//////////    public float hitFPS = 12f;

//////////    [Header("Win / Celebration Animation")]
//////////    public Sprite[] winFrames;
//////////    public float winFPS = 12f;

//////////    private Image cowImage;
//////////    private RectTransform cowRect;
//////////    private Vector2 originalSize;
//////////    private bool isAlive = false;
//////////    private bool hitThenWin = false;

//////////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////////    [HideInInspector] public float worldX = 0f;
//////////    [HideInInspector] public bool isRunning = false;

//////////    // ──────────────────────────────────────────────────────────
//////////    void Start()
//////////    {
//////////        isAlive = true;
//////////        cowImage = GetComponent<Image>();
//////////        cowRect = GetComponent<RectTransform>();
//////////        originalSize = cowRect.sizeDelta;
//////////        worldX = cowRect.anchoredPosition.x;
//////////        SetState(CowState.Idle);
//////////    }

//////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////////    // ──────────────────────────────────────────────────────────
//////////    void Update()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Hit) return;

//////////        if (isRunning)
//////////        {
//////////            worldX += runSpeed * Time.deltaTime;

//////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////////                PlayJump();
//////////        }
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    public void StartRunning()
//////////    {
//////////        isRunning = true;
//////////        SetState(CowState.Run);
//////////        Debug.Log("[Cow] StartRunning!");
//////////    }

//////////    public void StopRunning()
//////////    {
//////////        isRunning = false;
//////////    }

//////////    public void PlayJump()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        if (CurrentState == CowState.Jump) return;
//////////        if (CurrentState == CowState.Hit) return;
//////////        Debug.Log("[Cow] Jump!");
//////////        SetState(CowState.Jump);
//////////    }

//////////    public void OnJumpPressed() => PlayJump();
//////////    public void OnJumpDown() => PlayJump();

//////////    public void PlayHit()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Hit) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        hitThenWin = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    public void PlayHitThenWin()
//////////    {
//////////        if (!isAlive) return;
//////////        if (CurrentState == CowState.Win) return;
//////////        Debug.Log("[Cow] PlayHitThenWin!");
//////////        hitThenWin = true;
//////////        isRunning = false;
//////////        SetState(CowState.Hit);
//////////    }

//////////    // ──────────────────────────────────────────────────────────
//////////    void SetState(CowState newState)
//////////    {
//////////        if (!isAlive) return;
//////////        CurrentState = newState;
//////////        Debug.Log($"[Cow] → {newState}");
//////////        StopAllCoroutines();
//////////        StartCoroutine(MasterLoop());
//////////    }

//////////    IEnumerator MasterLoop()
//////////    {
//////////        int frame = 0;
//////////        while (true)
//////////        {
//////////            if (!isAlive) yield break;

//////////            Sprite[] frames = null;
//////////            float fps = 12f;

//////////            switch (CurrentState)
//////////            {
//////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////////            }

//////////            if (frames == null || frames.Length == 0)
//////////            {
//////////                Debug.LogError($"[Cow] {CurrentState} frames are EMPTY!");
//////////                yield return new WaitForSeconds(0.1f);
//////////                continue;
//////////            }

//////////            if (frame >= frames.Length) frame = 0;

//////////            cowImage.sprite = frames[frame];

//////////            // Apply jumpScale only during Jump, otherwise restore original size
//////////            cowRect.sizeDelta = (CurrentState == CowState.Jump)
//////////                ? originalSize * jumpScale
//////////                : originalSize;

//////////            frame++;

//////////            // Jump plays once → back to Run
//////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////            }

//////////            // Hit plays once → Win or Run
//////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////////            {
//////////                frame = 0;
//////////                if (hitThenWin)
//////////                {
//////////                    hitThenWin = false;
//////////                    CurrentState = CowState.Win;
//////////                    Debug.Log("[Cow] → Celebration!");
//////////                }
//////////                else
//////////                {
//////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////////                }
//////////            }

//////////            // Idle / Run / Win loop forever
//////////            if (frame >= frames.Length) frame = 0;

//////////            yield return new WaitForSeconds(1f / fps);
//////////        }
//////////    }
//////////}

////////using UnityEngine;
////////using UnityEngine.UI;
////////using System.Collections;

////////public class Cowanimationcontroller : MonoBehaviour
////////{
////////    public enum CowState { Idle, Run, Jump, Hit, Win }

////////    [Header("Auto Run")]
////////    public float runSpeed = 200f;

////////    [Header("Jump Physics")]
////////    public float jumpHeight = 250f;   // how high the cow goes (pixels)
////////    public float jumpDuration = 0.7f;   // total time in air (seconds)

////////    [Header("Idle Animation")]
////////    public Sprite[] idleFrames;
////////    public float idleFPS = 12f;

////////    [Header("Run Animation")]
////////    public Sprite[] runFrames;
////////    public float runFPS = 12f;

////////    [Header("Jump Animation")]
////////    public Sprite[] jumpFrames;
////////    public float jumpFPS = 12f;

////////    [Header("Hit Animation")]
////////    public Sprite[] hitFrames;
////////    public float hitFPS = 12f;

////////    [Header("Win / Celebration Animation")]
////////    public Sprite[] winFrames;
////////    public float winFPS = 12f;

////////    private Image cowImage;
////////    private RectTransform cowRect;
////////    private Vector2 originalSize;
////////    private bool isAlive = false;
////////    private bool hitThenWin = false;

////////    // ✅ PUBLIC — GameManager calls StartRunning(), BellTrigger reads CurrentState
////////    public CowState CurrentState { get; private set; } = CowState.Idle;

////////    [HideInInspector] public float worldX = 0f;
////////    [HideInInspector] public bool isRunning = false;

////////    // ✅ Ground Y — saved on Start so jump always returns to same Y
////////    private float groundY;

////////    // ──────────────────────────────────────────────────────────
////////    void Start()
////////    {
////////        isAlive = true;
////////        cowImage = GetComponent<Image>();
////////        cowRect = GetComponent<RectTransform>();
////////        originalSize = cowRect.sizeDelta;
////////        worldX = cowRect.anchoredPosition.x;
////////        groundY = cowRect.anchoredPosition.y;   // ✅ remember ground level
////////        SetState(CowState.Idle);
////////    }

////////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////////    // ──────────────────────────────────────────────────────────
////////    void Update()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        if (CurrentState == CowState.Hit) return;

////////        if (isRunning)
////////        {
////////            worldX += runSpeed * Time.deltaTime;

////////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////////                PlayJump();
////////        }
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    // Called by GameManager after countdown
////////    // ──────────────────────────────────────────────────────────
////////    public void StartRunning()
////////    {
////////        isRunning = true;
////////        SetState(CowState.Run);
////////        Debug.Log("[Cow] StartRunning!");
////////    }

////////    public void StopRunning() => isRunning = false;

////////    // ──────────────────────────────────────────────────────────
////////    // Public methods
////////    // ──────────────────────────────────────────────────────────
////////    public void PlayJump()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        if (CurrentState == CowState.Jump) return;  // no double jump
////////        if (CurrentState == CowState.Hit) return;
////////        Debug.Log("[Cow] Jump!");
////////        StartCoroutine(JumpArc());   // ✅ move cow up and down
////////        SetState(CowState.Jump);
////////    }

////////    public void OnJumpPressed() => PlayJump();
////////    public void OnJumpDown() => PlayJump();

////////    public void PlayHit()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Hit) return;
////////        if (CurrentState == CowState.Win) return;
////////        hitThenWin = false;
////////        SetState(CowState.Hit);
////////    }

////////    public void PlayHitThenWin()
////////    {
////////        if (!isAlive) return;
////////        if (CurrentState == CowState.Win) return;
////////        Debug.Log("[Cow] PlayHitThenWin!");
////////        hitThenWin = true;
////////        isRunning = false;
////////        SetState(CowState.Hit);
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    // ✅ Jump Arc — smooth parabola up and back down
////////    // ──────────────────────────────────────────────────────────
////////    IEnumerator JumpArc()
////////    {
////////        float elapsed = 0f;

////////        while (elapsed < jumpDuration)
////////        {
////////            elapsed += Time.deltaTime;

////////            // Normalised time 0→1
////////            float t = elapsed / jumpDuration;

////////            // Parabola: peaks at t=0.5, returns to 0 at t=1
////////            float height = jumpHeight * 4f * t * (1f - t);

////////            // ✅ Only change Y — X is handled by BackgroundScroller
////////            cowRect.anchoredPosition = new Vector2(
////////                cowRect.anchoredPosition.x,
////////                groundY + height
////////            );

////////            yield return null;
////////        }

////////        // ✅ Snap exactly back to ground when done
////////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    void SetState(CowState newState)
////////    {
////////        if (!isAlive) return;
////////        CurrentState = newState;
////////        Debug.Log($"[Cow] → {newState}");
////////        StopAllCoroutines();
////////        StartCoroutine(MasterLoop());
////////    }

////////    IEnumerator MasterLoop()
////////    {
////////        int frame = 0;
////////        while (true)
////////        {
////////            if (!isAlive) yield break;

////////            Sprite[] frames = null;
////////            float fps = 12f;

////////            switch (CurrentState)
////////            {
////////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////////            }

////////            if (frames == null || frames.Length == 0)
////////            {
////////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////////                yield return new WaitForSeconds(0.1f);
////////                continue;
////////            }

////////            if (frame >= frames.Length) frame = 0;
////////            cowImage.sprite = frames[frame];
////////            cowRect.sizeDelta = originalSize;
////////            frame++;

////////            // Jump → ONCE → back to Run
////////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////////            {
////////                frame = 0;
////////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////            }

////////            // Hit → ONCE → Win or Run
////////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////////            {
////////                frame = 0;
////////                if (hitThenWin)
////////                {
////////                    hitThenWin = false;
////////                    CurrentState = CowState.Win;
////////                    Debug.Log("[Cow] → Celebration!");
////////                }
////////                else
////////                {
////////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////////                }
////////            }

////////            // Idle / Run / Win → loop forever
////////            if (frame >= frames.Length) frame = 0;

////////            yield return new WaitForSeconds(1f / fps);
////////        }
////////    }
////////}

//////using UnityEngine;
//////using UnityEngine.UI;
//////using System.Collections;

//////public class Cowanimationcontroller : MonoBehaviour
//////{
//////    public enum CowState { Idle, Run, Jump, Hit, Win }

//////    [Header("Auto Run")]
//////    public float runSpeed = 200f;

//////    [Header("Jump Physics")]
//////    public float jumpHeight = 250f;   // pixels — how high the cow goes
//////    public float jumpDuration = 0.7f;   // seconds — time in the air

//////    [Header("Idle Animation")]
//////    public Sprite[] idleFrames;
//////    public float idleFPS = 12f;

//////    [Header("Run Animation")]
//////    public Sprite[] runFrames;
//////    public float runFPS = 12f;

//////    [Header("Jump Animation")]
//////    public Sprite[] jumpFrames;
//////    public float jumpFPS = 12f;

//////    [Header("Hit Animation")]
//////    public Sprite[] hitFrames;
//////    public float hitFPS = 12f;

//////    [Header("Win / Celebration Animation")]
//////    public Sprite[] winFrames;
//////    public float winFPS = 12f;

//////    // ── Private ────────────────────────────────────────────────
//////    private Image cowImage;
//////    private RectTransform cowRect;
//////    private Vector2 originalSize;
//////    private bool isAlive = false;
//////    private bool hitThenWin = false;
//////    private float groundY;   // Y position at ground level

//////    // ✅ Separate coroutine references so JumpArc is never killed by StopAllCoroutines
//////    private Coroutine animLoop = null;
//////    private Coroutine jumpArcCo = null;

//////    public CowState CurrentState { get; private set; } = CowState.Idle;

//////    [HideInInspector] public float worldX = 0f;
//////    [HideInInspector] public bool isRunning = false;

//////    // ──────────────────────────────────────────────────────────
//////    void Start()
//////    {
//////        isAlive = true;
//////        cowImage = GetComponent<Image>();
//////        cowRect = GetComponent<RectTransform>();
//////        originalSize = cowRect.sizeDelta;
//////        worldX = cowRect.anchoredPosition.x;
//////        groundY = cowRect.anchoredPosition.y;  // ✅ save ground level once
//////        SetState(CowState.Idle);
//////    }

//////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//////    // ──────────────────────────────────────────────────────────
//////    void Update()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Hit) return;

//////        if (isRunning)
//////        {
//////            worldX += runSpeed * Time.deltaTime;

//////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//////                PlayJump();
//////        }
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    public void StartRunning()
//////    {
//////        isRunning = true;
//////        SetState(CowState.Run);
//////        Debug.Log("[Cow] StartRunning!");
//////    }

//////    public void StopRunning() => isRunning = false;

//////    // ──────────────────────────────────────────────────────────
//////    public void PlayJump()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        if (CurrentState == CowState.Jump) return;
//////        if (CurrentState == CowState.Hit) return;

//////        Debug.Log("[Cow] Jump!");

//////        // ✅ Stop any previous arc, start a fresh one independently
//////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
//////        jumpArcCo = StartCoroutine(JumpArc());

//////        // ✅ Switch animation to Jump (only kills animLoop, NOT jumpArcCo)
//////        SetState(CowState.Jump);
//////    }

//////    public void OnJumpPressed() => PlayJump();
//////    public void OnJumpDown() => PlayJump();

//////    public void PlayHit()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Hit) return;
//////        if (CurrentState == CowState.Win) return;
//////        hitThenWin = false;
//////        SetState(CowState.Hit);
//////    }

//////    public void PlayHitThenWin()
//////    {
//////        if (!isAlive) return;
//////        if (CurrentState == CowState.Win) return;
//////        Debug.Log("[Cow] PlayHitThenWin!");
//////        hitThenWin = true;
//////        isRunning = false;
//////        SetState(CowState.Hit);
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // ✅ JumpArc — runs INDEPENDENTLY, never cancelled by SetState
//////    // ──────────────────────────────────────────────────────────
//////    IEnumerator JumpArc()
//////    {
//////        float elapsed = 0f;

//////        while (elapsed < jumpDuration)
//////        {
//////            if (!isAlive) yield break;

//////            elapsed += Time.deltaTime;
//////            float t = Mathf.Clamp01(elapsed / jumpDuration);

//////            // Smooth parabola: 0 → peak → 0
//////            float height = jumpHeight * 4f * t * (1f - t);

//////            // ✅ Only move Y — BackgroundScroller handles X
//////            cowRect.anchoredPosition = new Vector2(
//////                cowRect.anchoredPosition.x,
//////                groundY + height
//////            );

//////            yield return null;
//////        }

//////        // ✅ Snap back to ground exactly
//////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//////        jumpArcCo = null;

//////        Debug.Log("[Cow] Jump arc finished — back on ground");
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // SetState only stops animLoop — NEVER jumpArcCo
//////    // ──────────────────────────────────────────────────────────
//////    void SetState(CowState newState)
//////    {
//////        if (!isAlive) return;
//////        CurrentState = newState;
//////        Debug.Log($"[Cow] → {newState}");

//////        // ✅ Stop only the animation loop, not the jump arc
//////        if (animLoop != null) StopCoroutine(animLoop);
//////        animLoop = StartCoroutine(MasterLoop());
//////    }

//////    IEnumerator MasterLoop()
//////    {
//////        int frame = 0;
//////        while (true)
//////        {
//////            if (!isAlive) yield break;

//////            Sprite[] frames = null;
//////            float fps = 12f;

//////            switch (CurrentState)
//////            {
//////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//////                case CowState.Run: frames = runFrames; fps = runFPS; break;
//////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//////                case CowState.Win: frames = winFrames; fps = winFPS; break;
//////            }

//////            if (frames == null || frames.Length == 0)
//////            {
//////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//////                yield return new WaitForSeconds(0.1f);
//////                continue;
//////            }

//////            if (frame >= frames.Length) frame = 0;
//////            cowImage.sprite = frames[frame];
//////            cowRect.sizeDelta = originalSize;
//////            frame++;

//////            // Jump → ONCE → back to Run
//////            if (CurrentState == CowState.Jump && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////            }

//////            // Hit → ONCE → Win or Run
//////            if (CurrentState == CowState.Hit && frame >= frames.Length)
//////            {
//////                frame = 0;
//////                if (hitThenWin)
//////                {
//////                    hitThenWin = false;
//////                    CurrentState = CowState.Win;
//////                    Debug.Log("[Cow] → Celebration!");
//////                }
//////                else
//////                {
//////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//////                }
//////            }

//////            // Idle / Run / Win → loop forever
//////            if (frame >= frames.Length) frame = 0;

//////            yield return new WaitForSeconds(1f / fps);
//////        }
//////    }
//////}


////using UnityEngine;
////using UnityEngine.UI;
////using System.Collections;

////public class Cowanimationcontroller : MonoBehaviour
////{
////    public enum CowState { Idle, Run, Jump, Hit, Win }

////    [Header("Auto Run")]
////    public float runSpeed = 200f;

////    [Header("Jump Physics")]
////    public float jumpHeight = 250f;   // pixels — how high
////    public float jumpDuration = 0.7f;   // seconds — time in air
////    public float jumpForwardSpeed = 400f;  // pixels/sec forward during jump (faster than runSpeed)

////    [Header("Idle Animation")]
////    public Sprite[] idleFrames;
////    public float idleFPS = 12f;

////    [Header("Run Animation")]
////    public Sprite[] runFrames;
////    public float runFPS = 12f;

////    [Header("Jump Animation")]
////    public Sprite[] jumpFrames;
////    public float jumpFPS = 12f;

////    [Header("Hit Animation")]
////    public Sprite[] hitFrames;
////    public float hitFPS = 12f;

////    [Header("Win / Celebration Animation")]
////    public Sprite[] winFrames;
////    public float winFPS = 12f;

////    // ── Private ────────────────────────────────────────────────
////    private Image cowImage;
////    private RectTransform cowRect;
////    private Vector2 originalSize;
////    private bool isAlive = false;
////    private bool hitThenWin = false;
////    private float groundY;
////    private bool isJumping = false;   // ✅ blocks Update from adding runSpeed during jump

////    private Coroutine animLoop = null;
////    private Coroutine jumpArcCo = null;

////    public CowState CurrentState { get; private set; } = CowState.Idle;

////    [HideInInspector] public float worldX = 0f;
////    [HideInInspector] public bool isRunning = false;

////    // ──────────────────────────────────────────────────────────
////    void Start()
////    {
////        isAlive = true;
////        cowImage = GetComponent<Image>();
////        cowRect = GetComponent<RectTransform>();
////        originalSize = cowRect.sizeDelta;
////        worldX = cowRect.anchoredPosition.x;
////        groundY = cowRect.anchoredPosition.y;
////        SetState(CowState.Idle);
////    }

////    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
////    void OnDisable() { isAlive = false; StopAllCoroutines(); }
////    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

////    // ──────────────────────────────────────────────────────────
////    void Update()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        if (CurrentState == CowState.Hit) return;

////        if (isRunning && !isJumping)
////        {
////            // ✅ Normal run speed — JumpArc takes over worldX during jump
////            worldX += runSpeed * Time.deltaTime;

////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////                PlayJump();
////        }
////        else if (isRunning && isJumping)
////        {
////            // ✅ Still allow jump input to be read but worldX is driven by JumpArc
////            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
////                Debug.Log("[Cow] Already jumping!");
////        }
////    }

////    // ──────────────────────────────────────────────────────────
////    public void StartRunning()
////    {
////        isRunning = true;
////        SetState(CowState.Run);
////        Debug.Log("[Cow] StartRunning!");
////    }

////    public void StopRunning() => isRunning = false;

////    // ──────────────────────────────────────────────────────────
////    public void PlayJump()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        if (CurrentState == CowState.Jump) return;
////        if (CurrentState == CowState.Hit) return;

////        Debug.Log("[Cow] Jump!");

////        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
////        jumpArcCo = StartCoroutine(JumpArc());

////        SetState(CowState.Jump);
////    }

////    public void OnJumpPressed() => PlayJump();
////    public void OnJumpDown() => PlayJump();

////    public void PlayHit()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Hit) return;
////        if (CurrentState == CowState.Win) return;
////        hitThenWin = false;
////        SetState(CowState.Hit);
////    }

////    public void PlayHitThenWin()
////    {
////        if (!isAlive) return;
////        if (CurrentState == CowState.Win) return;
////        Debug.Log("[Cow] PlayHitThenWin!");
////        hitThenWin = true;
////        isRunning = false;
////        SetState(CowState.Hit);
////    }

////    // ──────────────────────────────────────────────────────────
////    // ✅ JumpArc — moves cow UP (Y) and FORWARD (worldX) simultaneously
////    // ──────────────────────────────────────────────────────────
////    IEnumerator JumpArc()
////    {
////        isJumping = true;   // ✅ pause Update from adding runSpeed
////        float elapsed = 0f;

////        while (elapsed < jumpDuration)
////        {
////            if (!isAlive) yield break;

////            elapsed += Time.deltaTime;
////            float t = Mathf.Clamp01(elapsed / jumpDuration);

////            // ✅ Move forward faster than normal run during jump
////            worldX += jumpForwardSpeed * Time.deltaTime;

////            // ✅ Parabola arc upward
////            float height = jumpHeight * 4f * t * (1f - t);
////            cowRect.anchoredPosition = new Vector2(
////                cowRect.anchoredPosition.x,
////                groundY + height
////            );

////            yield return null;
////        }

////        // ✅ Snap back to ground
////        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
////        isJumping = false;
////        jumpArcCo = null;

////        Debug.Log("[Cow] Landed!");
////    }

////    // ──────────────────────────────────────────────────────────
////    void SetState(CowState newState)
////    {
////        if (!isAlive) return;
////        CurrentState = newState;
////        Debug.Log($"[Cow] → {newState}");

////        if (animLoop != null) StopCoroutine(animLoop);
////        animLoop = StartCoroutine(MasterLoop());
////    }

////    IEnumerator MasterLoop()
////    {
////        int frame = 0;
////        while (true)
////        {
////            if (!isAlive) yield break;

////            Sprite[] frames = null;
////            float fps = 12f;

////            switch (CurrentState)
////            {
////                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
////                case CowState.Run: frames = runFrames; fps = runFPS; break;
////                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
////                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
////                case CowState.Win: frames = winFrames; fps = winFPS; break;
////            }

////            if (frames == null || frames.Length == 0)
////            {
////                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
////                yield return new WaitForSeconds(0.1f);
////                continue;
////            }

////            if (frame >= frames.Length) frame = 0;
////            cowImage.sprite = frames[frame];
////            cowRect.sizeDelta = originalSize;
////            frame++;

////            // Jump → ONCE → back to Run
////            if (CurrentState == CowState.Jump && frame >= frames.Length)
////            {
////                frame = 0;
////                CurrentState = isRunning ? CowState.Run : CowState.Idle;
////            }

////            // Hit → ONCE → Win or Run
////            if (CurrentState == CowState.Hit && frame >= frames.Length)
////            {
////                frame = 0;
////                if (hitThenWin)
////                {
////                    hitThenWin = false;
////                    CurrentState = CowState.Win;
////                    Debug.Log("[Cow] → Celebration!");
////                }
////                else
////                {
////                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
////                }
////            }

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
//    public enum CowState { Idle, Run, Jump, Hit, Win }

//    [Header("Auto Run")]
//    public float runSpeed = 200f;

//    [Header("Jump Physics")]
//    public float jumpHeight = 250f;   // pixels — how high
//    public float jumpDuration = 0.7f;   // seconds — time in air
//    public float jumpForwardSpeed = 400f;   // pixels/sec forward during jump
//    public int maxJumps = 2;      // ✅ 1 = single jump, 2 = double jump

//    [Header("Idle Animation")]
//    public Sprite[] idleFrames;
//    public float idleFPS = 12f;

//    [Header("Run Animation")]
//    public Sprite[] runFrames;
//    public float runFPS = 12f;

//    [Header("Jump Animation")]
//    public Sprite[] jumpFrames;
//    public float jumpFPS = 12f;

//    [Header("Hit Animation")]
//    public Sprite[] hitFrames;
//    public float hitFPS = 12f;

//    [Header("Win / Celebration Animation")]
//    public Sprite[] winFrames;
//    public float winFPS = 12f;

//    // ── Private ────────────────────────────────────────────────
//    private Image cowImage;
//    private RectTransform cowRect;
//    private Vector2 originalSize;
//    private bool isAlive = false;
//    private bool hitThenWin = false;
//    private float groundY;
//    private bool isJumping = false;

//    // ✅ Double jump counter
//    private int jumpsLeft = 0;

//    private Coroutine animLoop = null;
//    private Coroutine jumpArcCo = null;

//    public CowState CurrentState { get; private set; } = CowState.Idle;

//    [HideInInspector] public float worldX = 0f;
//    [HideInInspector] public bool isRunning = false;

//    // ──────────────────────────────────────────────────────────
//    void Start()
//    {
//        isAlive = true;
//        cowImage = GetComponent<Image>();
//        cowRect = GetComponent<RectTransform>();
//        originalSize = cowRect.sizeDelta;
//        worldX = cowRect.anchoredPosition.x;
//        groundY = cowRect.anchoredPosition.y;
//        SetState(CowState.Idle);
//    }

//    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
//    void OnDisable() { isAlive = false; StopAllCoroutines(); }
//    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

//    // ──────────────────────────────────────────────────────────
//    void Update()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Win) return;
//        if (CurrentState == CowState.Hit) return;

//        if (isRunning)
//        {
//            if (!isJumping)
//                worldX += runSpeed * Time.deltaTime;

//            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
//                PlayJump();
//        }
//    }

//    // ──────────────────────────────────────────────────────────
//    public void StartRunning()
//    {
//        isRunning = true;
//        jumpsLeft = maxJumps;   // ✅ reset on game start
//        SetState(CowState.Run);
//        Debug.Log("[Cow] StartRunning!");
//    }

//    public void StopRunning() => isRunning = false;

//    // ──────────────────────────────────────────────────────────
//    public void PlayJump()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Win) return;
//        if (CurrentState == CowState.Hit) return;

//        // ✅ Allow jump only if jumpsLeft > 0
//        if (jumpsLeft <= 0) return;

//        jumpsLeft--;
//        Debug.Log($"[Cow] Jump! Jumps left: {jumpsLeft}");

//        // ✅ Restart arc from CURRENT Y position for double jump feel
//        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
//        jumpArcCo = StartCoroutine(JumpArc());

//        SetState(CowState.Jump);
//    }

//    public void OnJumpPressed() => PlayJump();
//    public void OnJumpDown() => PlayJump();

//    public void PlayHit()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Hit) return;
//        if (CurrentState == CowState.Win) return;
//        hitThenWin = false;
//        SetState(CowState.Hit);
//    }

//    public void PlayHitThenWin()
//    {
//        if (!isAlive) return;
//        if (CurrentState == CowState.Win) return;
//        Debug.Log("[Cow] PlayHitThenWin!");
//        hitThenWin = true;
//        isRunning = false;
//        SetState(CowState.Hit);
//    }

//    // ──────────────────────────────────────────────────────────
//    // JumpArc — arcs from CURRENT Y so double jump stacks naturally
//    // ──────────────────────────────────────────────────────────
//    IEnumerator JumpArc()
//    {
//        isJumping = true;

//        float elapsed = 0f;
//        float startY = cowRect.anchoredPosition.y;   // ✅ start from wherever cow is now
//        float peakY = startY + jumpHeight;

//        while (elapsed < jumpDuration)
//        {
//            if (!isAlive) yield break;

//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / jumpDuration);

//            // ✅ Move forward faster than run
//            worldX += jumpForwardSpeed * Time.deltaTime;

//            // ✅ Parabola from startY → peak → groundY
//            float height = Mathf.Lerp(startY, groundY, t) + jumpHeight * 4f * t * (1f - t);
//            cowRect.anchoredPosition = new Vector2(
//                cowRect.anchoredPosition.x,
//                height
//            );

//            yield return null;
//        }

//        // Snap to ground
//        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
//        isJumping = false;
//        jumpsLeft = maxJumps;   // ✅ reset jumps when landed
//        jumpArcCo = null;
//        Debug.Log("[Cow] Landed! Jumps reset.");
//    }

//    // ──────────────────────────────────────────────────────────
//    void SetState(CowState newState)
//    {
//        if (!isAlive) return;
//        CurrentState = newState;
//        Debug.Log($"[Cow] → {newState}");

//        if (animLoop != null) StopCoroutine(animLoop);
//        animLoop = StartCoroutine(MasterLoop());
//    }

//    IEnumerator MasterLoop()
//    {
//        int frame = 0;
//        while (true)
//        {
//            if (!isAlive) yield break;

//            Sprite[] frames = null;
//            float fps = 12f;

//            switch (CurrentState)
//            {
//                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
//                case CowState.Run: frames = runFrames; fps = runFPS; break;
//                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
//                case CowState.Hit: frames = hitFrames; fps = hitFPS; break;
//                case CowState.Win: frames = winFrames; fps = winFPS; break;
//            }

//            if (frames == null || frames.Length == 0)
//            {
//                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
//                yield return new WaitForSeconds(0.1f);
//                continue;
//            }

//            if (frame >= frames.Length) frame = 0;
//            cowImage.sprite = frames[frame];
//            cowRect.sizeDelta = originalSize;
//            frame++;

//            // Jump → ONCE → back to Run
//            if (CurrentState == CowState.Jump && frame >= frames.Length)
//            {
//                frame = 0;
//                CurrentState = isRunning ? CowState.Run : CowState.Idle;
//            }

//            // Hit → ONCE → Win or Run
//            if (CurrentState == CowState.Hit && frame >= frames.Length)
//            {
//                frame = 0;
//                if (hitThenWin)
//                {
//                    hitThenWin = false;
//                    CurrentState = CowState.Win;
//                    Debug.Log("[Cow] → Celebration!");
//                }
//                else
//                {
//                    CurrentState = isRunning ? CowState.Run : CowState.Idle;
//                }
//            }

//            if (frame >= frames.Length) frame = 0;
//            yield return new WaitForSeconds(1f / fps);
//        }
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Cowanimationcontroller : MonoBehaviour
{
    public enum CowState { Idle, Run, Jump, Hit, Win }

    [Header("Auto Run")]
    public float runSpeed = 200f;

    [Header("Jump Physics")]
    public float jumpHeight = 250f;  // pixels — how high
    public float jumpDuration = 0.7f;  // seconds — time in air
    public float jumpForwardSpeed = 400f; // pixels/sec forward during jump
    public int maxJumps = 2;     // 1 = single jump, 2 = double jump

    [Header("Jump Size")]
    [Tooltip("1 = same size as run/idle.  1.3 = 30% bigger.  Drag to match your jump sprites.")]
    [Range(0.5f, 3f)]
    public float jumpScale = 1f;

    [Header("Win Size")]
    [Tooltip("1 = same size as run/idle.  1.3 = 30% bigger.  Drag to match your win sprites.")]
    [Range(0.5f, 3f)]
    public float winScale = 1f;

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

    [Header("Hit Bounce")]
    [Tooltip("How high the cow bounces up when hit plays. 0 = no bounce, 150 = noticeable bump.")]
    public float hitBounceHeight = 80f;
    [Tooltip("How long the bounce takes in seconds.")]
    public float hitBounceDuration = 0.35f;

    [Header("Win / Celebration Animation")]
    public Sprite[] winFrames;
    public float winFPS = 12f;

    // ── Private ────────────────────────────────────────────────
    private Image cowImage;
    private RectTransform cowRect;
    private Vector2 originalSize;
    private bool isAlive = false;
    private bool hitThenWin = false;
    private float groundY;
    private bool isJumping = false;
    private int jumpsLeft = 0;

    private Coroutine animLoop = null;
    private Coroutine jumpArcCo = null;

    public CowState CurrentState { get; private set; } = CowState.Idle;

    [HideInInspector] public float worldX = 0f;
    [HideInInspector] public bool isRunning = false;

    // ──────────────────────────────────────────────────────────
    void Start()
    {
        isAlive = true;
        cowImage = GetComponent<Image>();
        cowRect = GetComponent<RectTransform>();
        originalSize = cowRect.sizeDelta;
        worldX = cowRect.anchoredPosition.x;
        groundY = cowRect.anchoredPosition.y;
        SetState(CowState.Idle);
    }

    void OnDestroy() { isAlive = false; StopAllCoroutines(); }
    void OnDisable() { isAlive = false; StopAllCoroutines(); }
    void OnEnable() { if (cowImage != null && cowRect != null) isAlive = true; }

    // ──────────────────────────────────────────────────────────
    void Update()
    {
        if (!isAlive) return;
        if (CurrentState == CowState.Win) return;
        if (CurrentState == CowState.Hit) return;

        if (isRunning)
        {
            if (!isJumping)
                worldX += runSpeed * Time.deltaTime;

            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                PlayJump();
        }
    }

    // ──────────────────────────────────────────────────────────
    public void StartRunning()
    {
        isRunning = true;
        jumpsLeft = maxJumps;
        SetState(CowState.Run);
        Debug.Log("[Cow] StartRunning!");
    }

    public void StopRunning()
    {
        isRunning = false;
    }

    // ──────────────────────────────────────────────────────────
    public void PlayJump()
    {
        if (!isAlive) return;
        if (CurrentState == CowState.Win) return;
        if (CurrentState == CowState.Hit) return;
        if (jumpsLeft <= 0) return;

        jumpsLeft--;
        Debug.Log($"[Cow] Jump! Jumps left: {jumpsLeft}");

        if (jumpArcCo != null) StopCoroutine(jumpArcCo);
        jumpArcCo = StartCoroutine(JumpArc());

        SetState(CowState.Jump);
    }

    public void OnJumpPressed() => PlayJump();
    public void OnJumpDown() => PlayJump();

    // ──────────────────────────────────────────────────────────
    // Called by ObstacleController when cow hits an obstacle
    // ──────────────────────────────────────────────────────────
    public void PlayHit()
    {
        if (!isAlive) return;
        if (CurrentState == CowState.Hit) return;
        if (CurrentState == CowState.Win) return;

        // Stop any jump in progress
        if (jumpArcCo != null)
        {
            StopCoroutine(jumpArcCo);
            jumpArcCo = null;
        }
        isJumping = false;
        isRunning = false;
        hitThenWin = false;

        // ✅ Snap to ground first, then play controllable bounce arc
        if (cowRect != null)
            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);

        SetState(CowState.Hit);
        StartCoroutine(HitBounceArc());
    }

    // ──────────────────────────────────────────────────────────
    // Hit bounce arc — parabola up then back to groundY
    // Controlled by hitBounceHeight and hitBounceDuration in Inspector
    // ──────────────────────────────────────────────────────────
    IEnumerator HitBounceArc()
    {
        if (hitBounceHeight <= 0f) yield break;   // 0 = no bounce, skip

        float elapsed = 0f;
        float startY = groundY;

        while (elapsed < hitBounceDuration)
        {
            if (!isAlive) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hitBounceDuration);

            // Parabola: groundY → peak → groundY
            float y = startY + hitBounceHeight * 4f * t * (1f - t);
            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, y);

            yield return null;
        }

        // Snap back to ground
        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
    }

    // Called by BellTrigger — Hit once then Win celebration
    public void PlayHitThenWin()
    {
        if (!isAlive) return;
        if (CurrentState == CowState.Win) return;

        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
        isJumping = false;
        isRunning = false;
        if (cowRect != null)
            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
        hitThenWin = true;
        SetState(CowState.Hit);
        Debug.Log("[Cow] PlayHitThenWin!");
    }

    // ──────────────────────────────────────────────────────────
    // Called by BellTrigger after waiting for Hit animation to finish.
    // Transitions directly to Win (celebration) animation.
    // ──────────────────────────────────────────────────────────
    public void PlayWinDirect()
    {
        if (!isAlive) return;
        if (CurrentState == CowState.Win) return;

        if (jumpArcCo != null) { StopCoroutine(jumpArcCo); jumpArcCo = null; }
        isJumping = false;
        isRunning = false;
        hitThenWin = false;

        // ✅ Apply scale immediately so it's visible on the very first frame
        if (cowRect != null)
            cowRect.sizeDelta = originalSize * winScale;

        SetState(CowState.Win);
        Debug.Log("[Cow] PlayWinDirect — celebration!");
    }

    // ──────────────────────────────────────────────────────────
    // Jump arc — parabola from current Y back to groundY
    // ──────────────────────────────────────────────────────────
    IEnumerator JumpArc()
    {
        isJumping = true;

        float elapsed = 0f;
        float startY = cowRect.anchoredPosition.y;

        while (elapsed < jumpDuration)
        {
            if (!isAlive) yield break;
            if (CurrentState == CowState.Hit) yield break; // stop arc on hit

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);

            worldX += jumpForwardSpeed * Time.deltaTime;

            // Parabola: startY → peak → groundY
            float height = Mathf.Lerp(startY, groundY, t) + jumpHeight * 4f * t * (1f - t);
            cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, height);

            yield return null;
        }

        // Snap to ground and reset
        cowRect.anchoredPosition = new Vector2(cowRect.anchoredPosition.x, groundY);
        isJumping = false;
        jumpsLeft = maxJumps;
        jumpArcCo = null;

        if (CurrentState == CowState.Jump)
            SetState(isRunning ? CowState.Run : CowState.Idle);

        Debug.Log("[Cow] Landed!");
    }

    // ──────────────────────────────────────────────────────────
    void SetState(CowState newState)
    {
        if (!isAlive) return;
        CurrentState = newState;
        Debug.Log($"[Cow] → {newState}");

        if (animLoop != null) StopCoroutine(animLoop);
        animLoop = StartCoroutine(MasterLoop());
    }

    IEnumerator MasterLoop()
    {
        int frame = 0;
        while (true)
        {
            if (!isAlive) yield break;

            Sprite[] frames = null;
            float fps = 12f;

            switch (CurrentState)
            {
                case CowState.Idle: frames = idleFrames; fps = idleFPS; break;
                case CowState.Run: frames = runFrames; fps = runFPS; break;
                case CowState.Jump: frames = jumpFrames; fps = jumpFPS; break;
                case CowState.Hit: frames = jumpFrames; fps = jumpFPS; break;
                case CowState.Win: frames = winFrames; fps = winFPS; break;
            }

            if (frames == null || frames.Length == 0)
            {
                Debug.LogError($"[Cow] {CurrentState} frames EMPTY!");
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (frame >= frames.Length) frame = 0;

            cowImage.sprite = frames[frame];

            // Apply scale based on state
            if (CurrentState == CowState.Jump)
                cowRect.sizeDelta = originalSize * jumpScale;
            else if (CurrentState == CowState.Win)
                cowRect.sizeDelta = originalSize * winScale;
            else
                cowRect.sizeDelta = originalSize;





            frame++;

            // Jump → plays once → handled by JumpArc landing
            if (CurrentState == CowState.Jump && frame >= frames.Length)
            {
                frame = 0;
                // Keep looping jump frames until JumpArc finishes
            }

            // Hit → plays once → Win or Idle
            if (CurrentState == CowState.Hit && frame >= frames.Length)
            {
                frame = 0;
                if (hitThenWin)
                {
                    // PlayHitThenWin path — auto transition to Win
                    hitThenWin = false;
                    CurrentState = CowState.Win;
                    Debug.Log("[Cow] → Celebration!");
                }
                // ✅ FIX: Do NOT override CurrentState here if PlayWinDirect()
                // already changed it externally (e.g. BellTrigger celebration sequence).
                // Only fall to Idle if state is still Hit (nobody changed it).
                else if (CurrentState == CowState.Hit)
                {
                    // Game over path — no external win triggered, go to Idle
                    CurrentState = CowState.Idle;
                }
            }

            if (frame >= frames.Length) frame = 0;

            yield return new WaitForSeconds(1f / fps);
        }
    }
}