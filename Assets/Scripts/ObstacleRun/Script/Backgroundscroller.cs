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

//    private float bgStartX;
//    private float cowStartX;

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

//        float worldX = cowController.worldX;

//        if (worldX <= followStartX)
//        {
//            // ✅ Only set X — never touch Y (JumpArc controls Y)
//            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
//            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
//        }
//        else
//        {
//            // ✅ Lock cow X — only X, never Y
//            cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

//            // ✅ Scroll background — worldX already moves faster during jump
//            float overflow = worldX - followStartX;
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

    [Tooltip("Set this to the actual pixel width of your background image minus the screen width. " +
             "E.g. if your BG image is 8000px wide and screen is 1080px, set this to -6920. " +
             "Make it MORE negative to allow more scrolling.")]
    public float maxScrollX = -11000f;   // ✅ FIX: was -6815, now deep enough for full run

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
            // Cow walks freely before the follow threshold
            cowRect.anchoredPosition = new Vector2(worldX, cowRect.anchoredPosition.y);
            backgroundRect.anchoredPosition = new Vector2(bgStartX, backgroundRect.anchoredPosition.y);
        }
        else
        {
            // Lock cow on screen, scroll background instead
            cowRect.anchoredPosition = new Vector2(followStartX, cowRect.anchoredPosition.y);

            float overflow = worldX - followStartX;
            float targetBgX = Mathf.Max(bgStartX - overflow, maxScrollX);
            backgroundRect.anchoredPosition = new Vector2(targetBgX, backgroundRect.anchoredPosition.y);
        }
    }
}