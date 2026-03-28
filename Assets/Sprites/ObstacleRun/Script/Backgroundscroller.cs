using UnityEngine;

public class Backgroundscroller : MonoBehaviour
{
    [Header("References")]
    public Cowanimationcontroller cowController;   // drag Cow here
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

        if (cowController != null)
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