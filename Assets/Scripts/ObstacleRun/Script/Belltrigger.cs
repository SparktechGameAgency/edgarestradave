////using UnityEngine;
////using UnityEngine.UI;

////public class BellTrigger : MonoBehaviour
////{
////    [Header("References")]
////    public RectTransform cowRect;
////    public Cowanimationcontroller cowController;

////    [Header("Friends (Celebrate Only)")]
////    public FriendAnimationController[] friends;   // ✅ ADD THIS

////    [Header("Bell Settings")]
////    public bool ringOnce = true;

////    private RectTransform bellRect;
////    private bool hasRung = false;

////    // ─────────────────────────────────────────────────────────
////    void Start()
////    {
////        bellRect = GetComponent<RectTransform>();
////    }

////    void Update()
////    {
////        if (ringOnce && hasRung) return;
////        if (cowRect == null || cowController == null) return;

////        if (RectOverlaps(cowRect, bellRect))
////        {
////            hasRung = true;

////            Debug.Log("[Bell] Cow reached bell!");

////            // ✅ Cow animation
////            cowController.PlayHitThenWin();

////            // ✅ Friends celebrate
////            if (friends != null)
////            {
////                foreach (var f in friends)
////                {
////                    if (f != null)
////                        f.PlayCelebrate();
////                }
////            }
////        }
////    }

////    // ─────────────────────────────────────────────────────────
////    bool RectOverlaps(RectTransform a, RectTransform b)
////    {
////        Rect aRect = GetWorldRect(a);
////        Rect bRect = GetWorldRect(b);
////        return aRect.Overlaps(bRect);
////    }

////    Rect GetWorldRect(RectTransform rt)
////    {
////        Vector3[] corners = new Vector3[4];
////        rt.GetWorldCorners(corners);

////        return new Rect(
////            corners[0].x,
////            corners[0].y,
////            corners[2].x - corners[0].x,
////            corners[2].y - corners[0].y
////        );
////    }

////    // ─────────────────────────────────────────────────────────
////    public void ResetBell()
////    {
////        hasRung = false;
////    }
////}

//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

///// <summary>
///// Attach to Bell UI Image.
///// ONLY triggers when cow is in Jump state and overlaps the bell.
///// On trigger: bell plays animation, cow plays HitThenWin, friends celebrate.
///// </summary>
//public class BellTrigger : MonoBehaviour
//{
//    [Header("References")]
//    public RectTransform cowRect;
//    public Cowanimationcontroller cowController;
//    public RectTransform backgroundRect;  // to scroll bell with background

//    [Header("Friends")]
//    public FriendAnimationController[] friends;

//    [Header("Bell Animation")]
//    public Sprite[] bellRingFrames;   // bell ringing sprite frames
//    public float bellFPS = 12f;

//    // ── Private ────────────────────────────────────────────────
//    private RectTransform bellRect;
//    private Image bellImage;
//    private bool hasRung = false;
//    private float bgStartX = 0f;
//    private Vector2 bellStartAnchor;

//    // ──────────────────────────────────────────────────────────
//    void Start()
//    {
//        bellRect = GetComponent<RectTransform>();
//        bellImage = GetComponent<Image>();
//        bellStartAnchor = bellRect.anchoredPosition;

//        if (backgroundRect != null)
//            bgStartX = backgroundRect.anchoredPosition.x;

//        // ✅ Bell always stays active
//        gameObject.SetActive(true);
//    }

//    void Update()
//    {
//        if (hasRung) return;
//        if (cowRect == null || cowController == null) return;

//        // ✅ Scroll bell left with background so it stays at correct world position
//        ScrollWithBackground();

//        // ✅ ONLY detect collision when cow is JUMPING
//        if (cowController.CurrentState != Cowanimationcontroller.CowState.Jump) return;

//        // ✅ Check overlap
//        if (!Overlaps(cowRect, bellRect)) return;

//        // ── Bell hit! ──────────────────────────────────────────
//        hasRung = true;
//        Debug.Log("[Bell] 🔔 Cow jumped and touched the bell!");

//        // 1. Bell rings
//        StartCoroutine(PlayBellAnimation());

//        // 2. Cow: Hit → then Win/Celebration
//        cowController.PlayHitThenWin();

//        // 3. Friends celebrate (staggered by their celebrateDelay)
//        if (friends != null)
//            foreach (var f in friends)
//                if (f != null) f.PlayCelebrate();
//    }

//    // ──────────────────────────────────────────────────────────
//    // Keep bell glued to its position on the scrolling background
//    // ──────────────────────────────────────────────────────────
//    void ScrollWithBackground()
//    {
//        if (backgroundRect == null) return;
//        float bgOffset = backgroundRect.anchoredPosition.x - bgStartX;
//        bellRect.anchoredPosition = new Vector2(
//            bellStartAnchor.x + bgOffset,
//            bellStartAnchor.y
//        );
//    }

//    // ──────────────────────────────────────────────────────────
//    // Bell rings and loops forever after being hit
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
//            bellImage.sprite = bellRingFrames[frame];
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

//    public void ResetBell() => hasRung = false;
//}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Attach to Bell UI Image.
/// 
/// IMPORTANT — Unity Hierarchy setup:
///   Make the Bell a CHILD of the background Image so it scrolls automatically.
///   Do NOT assign backgroundRect — the scroll code has been removed to prevent
///   the bell from being moved twice (once by the parent, once by code).
///
/// Triggers ONLY when cow is in Jump state and overlaps the bell.
/// On trigger:
///   1. Bell plays ring animation
///   2. Brown cow (main) plays Hit → Win
///   3. All friends (chicken, horse, pig, white cow, etc.) celebrate
///   4. Congrats panel appears via GameManager
/// </summary>
public class BellTrigger : MonoBehaviour
{
    [Header("References")]
    public RectTransform cowRect;
    public Cowanimationcontroller cowController;

    [Header("Friends — assign all 5 animals here")]
    [Tooltip("Drag: Brown Cow, Chicken, Horse, Pig, White Cow (order doesn't matter)")]
    public FriendAnimationController[] friends;

    [Header("Bell Ring Animation")]
    public Sprite[] bellRingFrames;
    public float bellFPS = 12f;

    [Header("Congrats Delay")]
    [Tooltip("Seconds to wait after bell hit before showing the Congrats panel")]
    public float congratsDelay = 1.2f;

    // ── Private ────────────────────────────────────────────────
    private RectTransform bellRect;
    private Image bellImage;
    private bool hasRung = false;

    // ──────────────────────────────────────────────────────────
    void Start()
    {
        bellRect = GetComponent<RectTransform>();
        bellImage = GetComponent<Image>();

        // Bell must always be visible from the start
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (hasRung) return;
        if (cowRect == null || cowController == null) return;

        // Only detect collision while cow is mid-jump
        if (cowController.CurrentState != Cowanimationcontroller.CowState.Jump) return;

        if (!Overlaps(cowRect, bellRect)) return;

        // ── Bell hit! ──────────────────────────────────────────
        hasRung = true;
        Debug.Log("[Bell] Cow jumped and hit the bell!");

        // 1. Bell rings
        StartCoroutine(PlayBellAnimation());

        // 2. Cow: Hit → Win (celebration)
        cowController.PlayHitThenWin();

        // 3. All friends celebrate simultaneously
        if (friends != null)
            foreach (var f in friends)
                if (f != null) f.PlayCelebrate();

        // 4. Show congrats panel after a short delay
        StartCoroutine(ShowCongratsAfterDelay());
    }

    // ──────────────────────────────────────────────────────────
    IEnumerator ShowCongratsAfterDelay()
    {
        yield return new WaitForSeconds(congratsDelay);

        if (GameManager.Instance != null)
            GameManager.Instance.ShowRunnerCongrats();
        else
            Debug.LogWarning("[Bell] GameManager.Instance is null — congrats panel won't show!");
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
        while (true) // loops forever after being hit
        {
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

    public void ResetBell() => hasRung = false;
}