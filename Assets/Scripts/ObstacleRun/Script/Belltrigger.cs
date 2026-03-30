

//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class BellTrigger : MonoBehaviour
//{
//    [Header("Cow Hit Zone")]
//    [Tooltip("Drag the CowHitZone child RectTransform from inside the Cow.")]
//    public RectTransform cowHitZone;

//    [Header("Bell Hit Zones")]
//    [Tooltip("CowHitZone touching this → triggers Hit animation only (like an obstacle).")]
//    public RectTransform bellHitZone;

//    [Tooltip("CowHitZone touching this → triggers bell + all animal celebrations.")]
//    public RectTransform bellHitingZone;

//    [Header("References")]
//    public Cowanimationcontroller cowController;

//    [Header("Friends — assign all 4 animals")]
//    [Tooltip("Drag: White Cow, Horse, Chicken, Pig (order doesn't matter)")]
//    public FriendAnimationController[] friends;

//    [Header("Bell Ring Animation")]
//    public Sprite[] bellRingFrames;
//    public float bellFPS = 12f;

//    [Header("Timing")]
//    [Tooltip("Duration of the Hit animation before celebrations start. hitFrames.Length / hitFPS.")]
//    public float hitAnimationDuration = 0.5f;

//    [Tooltip("Seconds after celebrations start before showing the Congrats panel.")]
//    public float congratsDelay = 1.2f;

//    [Tooltip("Seconds to wait before the bell ring animation starts after BellHitZone is touched.")]
//    public float bellAnimationDelay = 0.3f;

//    // ── Private ────────────────────────────────────────────────
//    private Image bellImage;
//    private bool hitTriggered = false;
//    private bool celebrationTriggered = false;

//    // ──────────────────────────────────────────────────────────
//    void Start()
//    {
//        bellImage = GetComponent<Image>();
//        gameObject.SetActive(true);

//        if (cowController == null)
//            cowController = Object.FindFirstObjectByType<Cowanimationcontroller>();

//        // Auto-find CowHitZone if not assigned
//        if (cowHitZone == null && cowController != null)
//        {
//            Transform found = cowController.transform.Find("CowHitZone");
//            if (found != null) cowHitZone = found.GetComponent<RectTransform>();
//        }

//        if (cowHitZone == null)
//            Debug.LogError("[Bell] CowHitZone not assigned! " +
//                           "Create a child on the Cow named 'CowHitZone' and drag it in.");
//        if (bellHitZone == null)
//            Debug.LogError("[Bell] BellHitZone not assigned! " +
//                           "Create a child on the Bell for the hit trigger and drag it in.");
//        if (bellHitingZone == null)
//            Debug.LogError("[Bell] BellHitingZone not assigned! " +
//                           "Create a child on the Bell for the celebration trigger and drag it in.");
//        if (cowController == null)
//            Debug.LogError("[Bell] Cowanimationcontroller not found!");
//    }

//    void Update()
//    {
//        if (cowController == null || cowHitZone == null) return;

//        Cowanimationcontroller.CowState s = cowController.CurrentState;
//        if (s == Cowanimationcontroller.CowState.Win) return;

//        // ── Zone 1: BellHitZone → Hit animation + delayed bell ring ──
//        // Only triggers when cow is running or jumping (not already hit)
//        if (!hitTriggered && bellHitZone != null &&
//            s == Cowanimationcontroller.CowState.Run ||
//            !hitTriggered && bellHitZone != null &&
//            s == Cowanimationcontroller.CowState.Jump)
//        {
//            if (Overlaps(cowHitZone, bellHitZone))
//            {
//                hitTriggered = true;
//                Debug.Log("[Bell] CowHitZone touched BellHitZone → Hit + delayed bell!");
//                cowController.PlayHit();
//                StartCoroutine(DelayedBellAnimation());
//            }
//        }

//        // ── Zone 2: BellHitingZone → Celebration ───────────────
//        // Triggers on Run, Jump, OR Hit state (cow may already be in hit from Zone 1)
//        if (!celebrationTriggered && bellHitingZone != null &&
//            s != Cowanimationcontroller.CowState.Idle &&
//            Overlaps(cowHitZone, bellHitingZone))
//        {
//            celebrationTriggered = true;
//            Debug.Log("[Bell] CowHitZone touched BellHitingZone → Celebration!");
//            StartCoroutine(CelebrationSequence());
//        }
//    }

//    // ──────────────────────────────────────────────────────────
//    // CELEBRATION SEQUENCE
//    // ──────────────────────────────────────────────────────────
//    IEnumerator CelebrationSequence()
//    {
//        // Step 1 — Stop running, play hit if not already playing
//        cowController.StopRunning();
//        if (cowController.CurrentState != Cowanimationcontroller.CowState.Hit)
//            cowController.PlayHit();
//        Debug.Log("[Bell] Step 1: Waiting for hit animation...");

//        // Step 2 — Wait until hit animation finishes (cow leaves Hit state)
//        // Uses hitAnimationDuration as a reliable fixed wait matching your hit frame count
//        yield return new WaitForSeconds(hitAnimationDuration);

//        // Step 3 — All celebrations fire SIMULTANEOUSLY
//        Debug.Log("[Bell] Step 3: All celebrations!");
//        cowController.PlayWinDirect();          // Brown cow → Win
//        StartCoroutine(PlayBellAnimation());    // Bell → ring

//        if (friends != null)
//            foreach (var f in friends)
//                if (f != null) f.PlayCelebrate(); // All friends at once

//        // Step 4 — Congrats panel
//        yield return new WaitForSeconds(congratsDelay);

//        if (GameManager.Instance != null)
//            GameManager.Instance.ShowRunnerCongrats();
//        else
//            Debug.LogWarning("[Bell] GameManager.Instance is null — assign it in the scene!");
//    }

//    // ──────────────────────────────────────────────────────────
//    // Waits bellAnimationDelay seconds then starts the bell ring
//    // ──────────────────────────────────────────────────────────
//    IEnumerator DelayedBellAnimation()
//    {
//        yield return new WaitForSeconds(bellAnimationDelay);
//        StartCoroutine(PlayBellAnimation());
//    }

//    // ──────────────────────────────────────────────────────────
//    IEnumerator PlayBellAnimation()
//    {
//        if (bellRingFrames == null || bellRingFrames.Length == 0)
//        {
//            Debug.LogWarning("[Bell] No bell ring frames assigned!");
//            yield break;
//        }

//        int frame = 0;
//        while (true)
//        {
//            if (bellImage != null)
//                bellImage.sprite = bellRingFrames[frame];
//            frame = (frame + 1) % bellRingFrames.Length;
//            yield return new WaitForSeconds(1f / bellFPS);
//        }
//    }

//    // ──────────────────────────────────────────────────────────
//    bool Overlaps(RectTransform a, RectTransform b)
//    {
//        return GetWorldRect(a).Overlaps(GetWorldRect(b));
//    }

//    Rect GetWorldRect(RectTransform rt)
//    {
//        Vector3[] c = new Vector3[4];
//        rt.GetWorldCorners(c);
//        return new Rect(c[0].x, c[0].y, c[2].x - c[0].x, c[2].y - c[0].y);
//    }

//    public void ResetBell()
//    {
//        hitTriggered = false;
//        celebrationTriggered = false;
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Attach to Bell UI Image (child of background so it scrolls).
///
/// Unity Hierarchy setup:
///   Bell (this script)
///   ├── BellHitZone      → cow touching this plays Hit animation only
///   └── BellHitingZone   → cow touching this triggers full celebration + congrats panel
///
///   Cow
///   └── CowHitZone       → drag into Inspector below
/// </summary>
public class BellTrigger : MonoBehaviour
{
    [Header("Cow Hit Zone")]
    public RectTransform cowHitZone;

    [Header("Bell Hit Zones")]
    public RectTransform bellHitZone;       // triggers hit animation only
    public RectTransform bellHitingZone;    // triggers full celebration

    [Header("References")]
    public Cowanimationcontroller cowController;

    [Header("Friends")]
    public FriendAnimationController[] friends;

    [Header("Bell Ring Animation")]
    public Sprite[] bellRingFrames;
    public float bellFPS = 12f;

    [Header("Timing")]
    [Tooltip("Seconds to wait after hit starts before celebrations fire. Match to: hitFrames / hitFPS.")]
    public float hitAnimationDuration = 0.5f;

    [Tooltip("Seconds after celebrations start before congrats panel appears.")]
    public float congratsDelay = 1.0f;

    [Tooltip("Seconds after BellHitZone contact before bell ring animation starts.")]
    public float bellAnimationDelay = 0.3f;

    // ── Private ────────────────────────────────────────────────
    private Image bellImage;
    private bool hitTriggered = false;
    private bool celebrationTriggered = false;

    // ──────────────────────────────────────────────────────────
    void Start()
    {
        bellImage = GetComponent<Image>();

        if (cowController == null)
            cowController = Object.FindFirstObjectByType<Cowanimationcontroller>();

        if (cowHitZone == null && cowController != null)
        {
            Transform found = cowController.transform.Find("CowHitZone");
            if (found != null) cowHitZone = found.GetComponent<RectTransform>();
        }

        if (bellHitZone == null)
            Debug.LogError("[Bell] BellHitZone not assigned in Inspector!");
        if (bellHitingZone == null)
            Debug.LogError("[Bell] BellHitingZone not assigned in Inspector!");
        if (cowHitZone == null)
            Debug.LogError("[Bell] CowHitZone not assigned in Inspector!");
        if (cowController == null)
            Debug.LogError("[Bell] CowController not found!");
    }

    void Update()
    {
        if (cowController == null || cowHitZone == null) return;

        Cowanimationcontroller.CowState s = cowController.CurrentState;

        // ── Zone 1: BellHitZone → Hit animation + delayed bell ─
        if (!hitTriggered
            && bellHitZone != null
            && (s == Cowanimationcontroller.CowState.Run || s == Cowanimationcontroller.CowState.Jump)
            && Overlaps(cowHitZone, bellHitZone))
        {
            hitTriggered = true;
            Debug.Log("[Bell] ✅ Zone1 hit! Playing hit animation.");
            cowController.PlayHit();
            StartCoroutine(DelayedBellAnimation());
        }

        // ── Zone 2: BellHitingZone → Full celebration ──────────
        if (!celebrationTriggered
            && bellHitingZone != null
            && s != Cowanimationcontroller.CowState.Win
            && s != Cowanimationcontroller.CowState.Idle
            && Overlaps(cowHitZone, bellHitingZone))
        {
            celebrationTriggered = true;
            Debug.Log("[Bell] ✅ Zone2 hit! Starting celebration sequence.");
            StartCoroutine(CelebrationSequence());
        }
    }

    // ──────────────────────────────────────────────────────────
    IEnumerator DelayedBellAnimation()
    {
        yield return new WaitForSeconds(bellAnimationDelay);
        StartCoroutine(PlayBellAnimation());
    }

    // ──────────────────────────────────────────────────────────
    IEnumerator CelebrationSequence()
    {
        Debug.Log("[Bell] CelebrationSequence started.");

        // Stop cow and play hit if not already hitting
        cowController.StopRunning();
        if (cowController.CurrentState != Cowanimationcontroller.CowState.Hit)
        {
            Debug.Log("[Bell] Cow not in Hit state — calling PlayHit.");
            cowController.PlayHit();
        }
        else
        {
            Debug.Log("[Bell] Cow already in Hit state — skipping PlayHit.");
        }

        // Wait for hit animation to finish
        Debug.Log($"[Bell] Waiting {hitAnimationDuration}s for hit animation...");
        yield return new WaitForSeconds(hitAnimationDuration);

        // All celebrations simultaneously
        Debug.Log("[Bell] Hit done — firing all celebrations now!");
        cowController.PlayWinDirect();
        StartCoroutine(PlayBellAnimation());

        if (friends != null)
            foreach (var f in friends)
                if (f != null) f.PlayCelebrate();

        // Wait then show congrats panel
        Debug.Log($"[Bell] Waiting {congratsDelay}s before congrats panel...");
        yield return new WaitForSeconds(congratsDelay);

        Debug.Log($"[Bell] GameManager.Instance = {GameManager.Instance}");
        if (GameManager.Instance != null)
        {
            Debug.Log("[Bell] Calling ShowRunnerCongrats!");
            GameManager.Instance.ShowRunnerCongrats();
        }
        else
        {
            Debug.LogError("[Bell] GameManager.Instance is NULL! " +
                           "Make sure GameManager script is in the scene.");
        }
    }

    // ──────────────────────────────────────────────────────────
    IEnumerator PlayBellAnimation()
    {
        if (bellRingFrames == null || bellRingFrames.Length == 0)
        {
            Debug.LogWarning("[Bell] No bell ring frames assigned!");
            yield break;
        }

        int frame = 0;
        while (true)
        {
            if (bellImage != null)
                bellImage.sprite = bellRingFrames[frame];
            frame = (frame + 1) % bellRingFrames.Length;
            yield return new WaitForSeconds(1f / bellFPS);
        }
    }

    // ──────────────────────────────────────────────────────────
    bool Overlaps(RectTransform a, RectTransform b)
    {
        return GetWorldRect(a).Overlaps(GetWorldRect(b));
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        return new Rect(c[0].x, c[0].y, c[2].x - c[0].x, c[2].y - c[0].y);
    }

    public void ResetBell()
    {
        hitTriggered = false;
        celebrationTriggered = false;
    }
}