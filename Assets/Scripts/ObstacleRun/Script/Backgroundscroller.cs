//////using UnityEngine;

//////public class Backgroundscroller : MonoBehaviour
//////{
//////    [Header("References")]
//////    public Cowanimationcontroller cowController;   // drag Cow here
//////    public RectTransform cowRect;         // drag Cow RectTransform here
//////    public RectTransform backgroundRect;  // drag background Image here

//////    [Header("Scroll Settings")]
//////    public float followStartX = 643f;    // worldX where cow locks and bg starts scrolling
//////    public float maxScrollX = -6815f;  // bg can never go left of this

//////    private float bgStartX;
//////    private float cowStartX;   // cow's initial screen X (where it sits before threshold)

//////    void Start()
//////    {
//////        bgStartX = backgroundRect.anchoredPosition.x;
//////        cowStartX = cowRect.anchoredPosition.x;

//////        if (cowController != null)
//////            cowController.worldX = cowStartX;
//////    }
//////    void LateUpdate()
//////    {
//////        if (cowController == null || cowRect == null || backgroundRect == null) return;

//////        float worldX = cowController.worldX;

//////        if (worldX <= followStartX)
//////        {
//////            // ✅ Cow moves freely on screen, background stays still
//////            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
//////            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
//////        }
//////        else
//////        {
//////            // ✅ Cow locked at followStartX on screen
//////            cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

//////            // ✅ Background scrolls left by exactly how far cow went past threshold
//////            float overflow = worldX - followStartX;
//////            float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);
//////            backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
//////        }
//////    }
//////}

////using UnityEngine;

////public class Backgroundscroller : MonoBehaviour
////{
////    [Header("References")]
////    public Cowanimationcontroller cowController;
////    public RectTransform cowRect;
////    public RectTransform backgroundRect;

////    [Header("Scroll Settings")]
////    public float followStartX = 643f;
////    public float maxScrollX = -6815f;

////    private float bgStartX;
////    private float cowStartX;

////    void Start()
////    {
////        bgStartX = backgroundRect.anchoredPosition.x;
////        cowStartX = cowRect.anchoredPosition.x;

////        if (cowController != null)
////            cowController.worldX = cowStartX;
////    }

////    void LateUpdate()
////    {
////        if (cowController == null || cowRect == null || backgroundRect == null) return;

////        float worldX = cowController.worldX;

////        if (worldX <= followStartX)
////        {
////            // ✅ Only set X — never touch Y (jump arc controls Y)
////            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
////            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
////        }
////        else
////        {
////            // ✅ Lock cow X at followStartX — only X, Y stays whatever JumpArc set it to
////            cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

////            // ✅ Scroll background left
////            float overflow = worldX - followStartX;
////            float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);
////            backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
////        }
////    }
////}

//using UnityEngine;

//public class Backgroundscroller : MonoBehaviour
//{
//    [Header("References")]
//    public Cowanimationcontroller cowController;
//    public RectTransform cowRect;
//    public RectTransform backgroundRect;

//    [Header("Scroll Settings")]
//    public float followStartX = 643f;
//    public float maxScrollX = -6815f;

//    [Header("Jump Boost")]
//    [Tooltip("Extra pixels per second the background scrolls while cow is jumping. Makes it look like the cow leaps forward.")]
//    public float jumpScrollBoost = 100f;  // tweak this in Inspector

//    // ── Private ────────────────────────────────────────────────
//    private float bgStartX;
//    private float cowStartX;
//    private float extraScroll = 0f;   // accumulated extra scroll from jump boost

//    void Start()
//    {
//        bgStartX = backgroundRect.anchoredPosition.x;
//        cowStartX = cowRect.anchoredPosition.x;

//        if (cowController != null)
//            cowController.worldX = cowStartX;
//    }

//    void LateUpdate()
//    {
//        if (cowController == null || cowRect == null || backgroundRect == null) return;

//        // ✅ Add extra scroll while cow is jumping
//        if (cowController.CurrentState == Cowanimationcontroller.CowState.Jump)
//        {
//            extraScroll += jumpScrollBoost * Time.deltaTime;
//        }

//        float worldX = cowController.worldX;

//        if (worldX <= followStartX)
//        {
//            // Cow moves freely — no boost needed yet
//            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
//            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
//        }
//        else
//        {
//            // ✅ Lock cow X on screen
//            cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

//            // ✅ Scroll background left — include extra scroll from jump
//            float overflow = (worldX - followStartX) + extraScroll;
//            float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);
//            backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
//        }
//    }
//}

using UnityEngine;

public class Backgroundscroller : MonoBehaviour
{
    [Header("References")]
    public Cowanimationcontroller cowController;
    public RectTransform cowRect;
    public RectTransform backgroundRect;

    [Header("Scroll Settings")]
    public float followStartX = 643f;
    public float maxScrollX = -6815f;

    private float bgStartX;
    private float cowStartX;

    void Start()
    {
        bgStartX = backgroundRect.anchoredPosition.x;
        cowStartX = cowRect.anchoredPosition.x;

        if (cowController != null)
            cowController.worldX = cowStartX;
    }

    void LateUpdate()
    {
        if (cowController == null || cowRect == null || backgroundRect == null) return;

        float worldX = cowController.worldX;

        if (worldX <= followStartX)
        {
            // ✅ Only set X — never touch Y (JumpArc controls Y)
            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
        }
        else
        {
            // ✅ Lock cow X — only X, never Y
            cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

            // ✅ Scroll background — worldX already moves faster during jump
            float overflow = worldX - followStartX;
            float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);
            backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
        }
    }
}