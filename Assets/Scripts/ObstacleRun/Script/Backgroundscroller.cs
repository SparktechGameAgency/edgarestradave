//////using UnityEngine;
//////using UnityEngine.UI;

///////// <summary>
///////// Attach this to the background Image GameObject.
///////// No camera movement needed — this scrolls the background
///////// left/right to follow the cow, keeping the cow on screen.
///////// </summary>
//////public class BackgroundScroller : MonoBehaviour
//////{
//////    [Header("References")]
//////    public RectTransform cowRect;           // drag your Cow here
//////    public RectTransform backgroundRect;    // drag the background Image here

//////    [Header("Scroll Settings")]
//////    public float followStartX = 643f;   // cow anchoredPosition.x where scrolling begins
//////    public float maxScrollX = -6815f;  // how far left the background can scroll (negative)
//////    public float smoothSpeed = 10f;    // how fast background catches up

//////    // ─────────────────────────────────────────────────────────
//////    private float bgStartX;               // background's original anchoredPosition.x

//////    void Start()
//////    {
//////        bgStartX = backgroundRect.anchoredPosition.x;
//////    }

//////    void LateUpdate()
//////    {
//////        if (cowRect == null || backgroundRect == null) return;

//////        float cowX = cowRect.anchoredPosition.x;

//////        // Don't scroll until cow passes the threshold
//////        if (cowX < followStartX) return;

//////        // How far the cow has gone past the threshold
//////        float overflow = cowX - followStartX;

//////        // Target background position (move LEFT = negative X)
//////        float targetBgX = bgStartX - overflow;

//////        // ✅ Clamp so background never scrolls too far
//////        targetBgX = Mathf.Max(targetBgX, maxScrollX);

//////        // Smooth scroll
//////        float smoothedX = Mathf.Lerp(
//////            backgroundRect.anchoredPosition.x,
//////            targetBgX,
//////            smoothSpeed * Time.deltaTime
//////        );

//////        backgroundRect.anchoredPosition = new Vector2(smoothedX, backgroundRect.anchoredPosition.y);
//////    }
//////}

////using UnityEngine;
////using UnityEngine.UI;

/////// <summary>
/////// Scrolls the background EXACTLY in sync with the cow — no lag, no lerp.
/////// Attach to the background Image GameObject.
/////// </summary>
////public class BackgroundScroller : MonoBehaviour
////{
////    [Header("References")]
////    public RectTransform cowRect;
////    public RectTransform backgroundRect;

////    [Header("Scroll Settings")]
////    public float followStartX = 643f;     // cow X where scrolling begins
////    public float maxScrollX = -6815f;   // how far left background can go

////    // ─────────────────────────────────────────────────────────
////    private float bgStartX;

////    void Start()
////    {
////        bgStartX = backgroundRect.anchoredPosition.x;
////    }

////    void LateUpdate()
////    {
////        if (cowRect == null || backgroundRect == null) return;

////        float cowX = cowRect.anchoredPosition.x;

////        if (cowX <= followStartX)
////        {
////            // Reset background to start if cow goes back
////            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
////            return;
////        }

////        // ✅ Direct 1:1 — background moves left exactly as much as cow moves right
////        float overflow = cowX - followStartX;
////        float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);

////        // ✅ Set directly — no Lerp, no lag
////        backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
////    }
////}

//using UnityEngine;

///// <summary>
///// Locks the cow at followStartX on screen.
///// Instead of the cow moving right, the background scrolls left.
///// Classic endless runner illusion.
///// Attach to the background Image GameObject.
///// </summary>
//public class BackgroundScroller : MonoBehaviour
//{
//    [Header("References")]
//    public RectTransform cowRect;
//    public RectTransform backgroundRect;

//    [Header("Scroll Settings")]
//    public float followStartX = 643f;     // cow X where it gets locked and bg starts scrolling
//    public float maxScrollX = -6815f;   // how far left background can go (negative)

//    private float bgStartX;

//    void Start()
//    {
//        bgStartX = backgroundRect.anchoredPosition.x;
//    }

//    void LateUpdate()
//    {
//        if (cowRect == null || backgroundRect == null) return;

//        float cowX = cowRect.anchoredPosition.x;

//        // Cow hasn't reached threshold yet — background stays still
//        if (cowX <= followStartX)
//        {
//            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
//            return;
//        }

//        // ✅ How far cow has gone past the lock point
//        float overflow = cowX - followStartX;

//        // ✅ Lock cow at followStartX — it never visually moves past this point
//        cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

//        // ✅ Scroll background left by the same amount
//        float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);
//        backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
//    }
//}

using UnityEngine;

/// <summary>
/// Owns the visual position of both the cow and background.
/// CowAnimationController only tracks worldX.
/// This script reads worldX and decides:
///   - where the cow appears on screen (anchoredPosition)
///   - where the background sits (anchoredPosition)
/// No snapping, no jumping.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("References")]
    public CowAnimationController cowController;   // drag Cow here
    public RectTransform cowRect;         // drag Cow RectTransform here
    public RectTransform backgroundRect;  // drag background Image here

    [Header("Scroll Settings")]
    public float followStartX = 643f;    // worldX where cow locks and bg starts scrolling
    public float maxScrollX = -6815f;  // bg can never go left of this

    private float bgStartX;
    private float cowStartX;   // cow's initial screen X (where it sits before threshold)

    void Start()
    {
        bgStartX = backgroundRect.anchoredPosition.x;
        cowStartX = cowRect.anchoredPosition.x;

        // Sync worldX to wherever cow is placed in the scene
        cowController.worldX = cowStartX;
    }

    void LateUpdate()
    {
        if (cowController == null || cowRect == null || backgroundRect == null) return;

        float worldX = cowController.worldX;

        if (worldX <= followStartX)
        {
            // ✅ Cow moves freely on screen, background stays still
            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
        }
        else
        {
            // ✅ Cow locked at followStartX on screen
            cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

            // ✅ Background scrolls left by exactly how far cow went past threshold
            float overflow = worldX - followStartX;
            float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);
            backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
        }
    }
}