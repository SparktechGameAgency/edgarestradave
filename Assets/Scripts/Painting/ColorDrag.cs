//////using System.Collections.Generic;
//////using Unity.VisualScripting.Dependencies.Sqlite;
//////using UnityEditor;
//////using UnityEngine;
//////using UnityEngine.EventSystems;
//////using UnityEngine.UI;
//////using static UnityEngine.Rendering.DebugUI.Table;

//////public class ColorDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//////{
//////    public Color dragColor;

//////    private RectTransform rectTransform;
//////    private Canvas rootCanvas;
//////    private CanvasGroup canvasGroup;
//////    private Transform originalParent;
//////    private int originalSiblingIndex;
//////    private Vector2 originalAnchoredPos;

//////    void Start()
//////    {
//////        rectTransform = GetComponent<RectTransform>();
//////        canvasGroup = GetComponent<CanvasGroup>();
//////        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
//////    }

//////    public void OnBeginDrag(PointerEventData eventData)
//////    {
//////        originalParent = transform.parent;
//////        originalSiblingIndex = transform.GetSiblingIndex();
//////        originalAnchoredPos = rectTransform.anchoredPosition;

//////        transform.SetParent(rootCanvas.transform, true);
//////        transform.SetAsLastSibling();
//////        canvasGroup.blocksRaycasts = false;
//////    }

//////    public void OnDrag(PointerEventData eventData)
//////    {
//////        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
//////    }

//////    public void OnEndDrag(PointerEventData eventData)
//////    {
//////        canvasGroup.blocksRaycasts = true;

//////        ColorDropArea[] allDropAreas = FindObjectsOfType<ColorDropArea>();

//////        ColorDropArea bestMatch = null;
//////        int highestSiblingIndex = -1;

//////        foreach (ColorDropArea dropArea in allDropAreas)
//////        {
//////            if (dropArea.IsColored) continue;

//////            if (!IsVisiblePixelAtPosition(dropArea, eventData.position))
//////                continue;

//////            int siblingIndex = dropArea.transform.GetSiblingIndex();
//////            Debug.Log($"[HIT] {dropArea.gameObject.name} sibling={siblingIndex}");

//////            if (siblingIndex > highestSiblingIndex)
//////            {
//////                highestSiblingIndex = siblingIndex;
//////                bestMatch = dropArea;
//////            }
//////        }

//////        if (bestMatch != null)
//////        {
//////            Debug.Log($"[ColorDrag] Coloring: {bestMatch.gameObject.name}");
//////            bestMatch.OnDrop(eventData);
//////        }
//////        else
//////        {
//////            Debug.Log("[ColorDrag] Nothing found - empty area");
//////        }

//////        transform.SetParent(originalParent, true);
//////        transform.SetSiblingIndex(originalSiblingIndex);
//////        rectTransform.anchoredPosition = originalAnchoredPos;
//////    }

//////    private bool IsVisiblePixelAtPosition(ColorDropArea dropArea, Vector2 screenPos)
//////    {
//////        Image img = dropArea.GetComponent<Image>();
//////        if (img == null || img.sprite == null)
//////        {
//////            Debug.Log($"[Pixel] {dropArea.gameObject.name} - no image/sprite");
//////            return false;
//////        }

//////        RectTransform rt = img.GetComponent<RectTransform>();

//////        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//////            ? null : Camera.main;

//////        // ✅ Check if point is inside rect at all
//////        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
//////        {
//////            Debug.Log($"[Pixel] {dropArea.gameObject.name} - outside rect");
//////            return false;
//////        }

//////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//////            rt, screenPos, cam, out Vector2 localPoint
//////        );

//////        Rect rect = rt.rect;
//////        float normalizedX = (localPoint.x - rect.x) / rect.width;
//////        float normalizedY = (localPoint.y - rect.y) / rect.height;

//////        normalizedX = Mathf.Clamp01(normalizedX);
//////        normalizedY = Mathf.Clamp01(normalizedY);

//////        Sprite sprite = img.sprite;
//////        Texture2D tex = sprite.texture;
//////        Rect spriteRect = sprite.textureRect;

//////        int px = Mathf.RoundToInt(spriteRect.x + normalizedX * spriteRect.width);
//////        int py = Mathf.RoundToInt(spriteRect.y + normalizedY * spriteRect.height);

//////        px = Mathf.Clamp(px, 0, tex.width - 1);
//////        py = Mathf.Clamp(py, 0, tex.height - 1);

//////        Color pixel = tex.GetPixel(px, py);

//////        // ✅ Full debug output
//////        Debug.Log($"[Pixel] {dropArea.gameObject.name} | " +
//////                  $"TexSize=({tex.width},{tex.height}) | " +
//////                  $"SpriteRect=({spriteRect.x:F0},{spriteRect.y:F0}," +
//////                  $"{spriteRect.width:F0},{spriteRect.height:F0}) | " +
//////                  $"Norm=({normalizedX:F2},{normalizedY:F2}) | " +
//////                  $"Px=({px},{py}) | " +
//////                  $"RGBA=({pixel.r:F2},{pixel.g:F2},{pixel.b:F2},{pixel.a:F2})");

//////        // ✅ Lower threshold to catch edge cases
//////        return pixel.a > 0.01f;
//////    }
//////}
////using System.Collections.Generic;
////using System.Net.NetworkInformation;
////using Unity.VisualScripting;
////using Unity.VisualScripting.Dependencies.Sqlite;
////using UnityEngine;
////using UnityEngine.EventSystems;
////using UnityEngine.UI;

////public class ColorDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
////{
////    public Color dragColor;

////    private RectTransform rectTransform;
////    private Canvas rootCanvas;
////    private CanvasGroup canvasGroup;
////    private Transform originalParent;
////    private int originalSiblingIndex;
////    private Vector2 originalAnchoredPos;
////    private ColorDropArea currentPreviewArea = null;

////    void Start()
////    {
////        rectTransform = GetComponent<RectTransform>();
////        canvasGroup = GetComponent<CanvasGroup>();
////        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
////    }

////    public void OnBeginDrag(PointerEventData eventData)
////    {
////        originalParent = transform.parent;
////        originalSiblingIndex = transform.GetSiblingIndex();
////        originalAnchoredPos = rectTransform.anchoredPosition;

////        transform.SetParent(rootCanvas.transform, true);
////        transform.SetAsLastSibling();
////        canvasGroup.blocksRaycasts = false;
////    }

////    public void OnDrag(PointerEventData eventData)
////    {
////        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

////        // ✅ Update preview every frame based on pixel check
////        ColorDropArea hoveredArea = GetDropAreaUnderPointer(eventData.position);

////        if (hoveredArea != currentPreviewArea)
////        {
////            if (currentPreviewArea != null)
////                currentPreviewArea.HidePreview();

////            if (hoveredArea != null)
////                hoveredArea.ShowPreview(dragColor);

////            currentPreviewArea = hoveredArea;
////        }
////    }

////    public void OnEndDrag(PointerEventData eventData)
////    {
////        canvasGroup.blocksRaycasts = true;

////        if (currentPreviewArea != null)
////        {
////            currentPreviewArea.HidePreview();
////            currentPreviewArea = null;
////        }

////        ColorDropArea bestMatch = GetDropAreaUnderPointer(eventData.position);

////        if (bestMatch != null)
////        {
////            Debug.Log($"[ColorDrag] Coloring: {bestMatch.gameObject.name}");
////            bestMatch.OnDrop(eventData);
////        }
////        else
////        {
////            Debug.Log("[ColorDrag] Nothing found - empty area");
////        }

////        transform.SetParent(originalParent, true);
////        transform.SetSiblingIndex(originalSiblingIndex);
////        rectTransform.anchoredPosition = originalAnchoredPos;
////    }

////    private ColorDropArea GetDropAreaUnderPointer(Vector2 screenPos)
////    {
////        ColorDropArea[] allDropAreas = FindObjectsOfType<ColorDropArea>();

////        ColorDropArea bestMatch = null;
////        int highestSiblingIndex = -1;

////        foreach (ColorDropArea dropArea in allDropAreas)
////        {
////            if (dropArea.IsColored) continue;
////            if (!IsVisiblePixelAtPosition(dropArea, screenPos)) continue;

////            int siblingIndex = dropArea.transform.GetSiblingIndex();
////            if (siblingIndex > highestSiblingIndex)
////            {
////                highestSiblingIndex = siblingIndex;
////                bestMatch = dropArea;
////            }
////        }

////        return bestMatch;
////    }

////    private bool IsVisiblePixelAtPosition(ColorDropArea dropArea, Vector2 screenPos)
////    {
////        Image img = dropArea.GetComponent<Image>();
////        if (img == null || img.sprite == null) return false;

////        RectTransform rt = img.GetComponent<RectTransform>();

////        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
////            ? null : Camera.main;

////        // ✅ Must be inside rect first
////        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
////            return false;

////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
////            rt, screenPos, cam, out Vector2 localPoint
////        );

////        Rect rect = rt.rect;
////        float normalizedX = Mathf.Clamp01((localPoint.x - rect.x) / rect.width);
////        float normalizedY = Mathf.Clamp01((localPoint.y - rect.y) / rect.height);

////        Sprite sprite = img.sprite;
////        Texture2D tex = sprite.texture;
////        Rect spriteRect = sprite.textureRect;

////        // ✅ Account for sprite pivot and texture rect
////        int px = Mathf.RoundToInt(spriteRect.x + normalizedX * spriteRect.width);
////        int py = Mathf.RoundToInt(spriteRect.y + normalizedY * spriteRect.height);

////        px = Mathf.Clamp(px, 0, tex.width - 1);
////        py = Mathf.Clamp(py, 0, tex.height - 1);

////        Color pixel = tex.GetPixel(px, py);

////        Debug.Log($"[Pixel] {dropArea.gameObject.name} | " +
////                  $"norm=({normalizedX:F2},{normalizedY:F2}) | " +
////                  $"px=({px},{py}) | alpha={pixel.a:F2}");

////        return pixel.a > 0.01f;
////    }
////}
//////```

//////---

//////## The Real Problem — Share Console Output

//////After dragging, the console will show:
//////```
//////[Pixel] Grass1 | norm = (0.45, 0.32) | px = (256, 140) | alpha = 0.00
//////[Pixel] Fench | norm = (0.45, 0.32) | px = (498, 280) | alpha = 0.00
//////```

//////If** both show alpha=0.00** even when dropping directly on the object, the sprites need to be re-exported from Figma at **exactly the same size as B&W image (953x584)**.

//////This is the **only guaranteed fix** — tell your designer:
//////```
//////Export ALL part sprites at:
//////Canvas size: 953 x 584 px
//////Background: Transparent
//////Format: PNG
/////


//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
//using System.Collections.Generic;

//public class ColorDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//{
//    public Color dragColor;

//    private RectTransform rectTransform;
//    private Canvas rootCanvas;
//    private CanvasGroup canvasGroup;
//    private Transform originalParent;
//    private int originalSiblingIndex;
//    private Vector2 originalAnchoredPos;
//    private ColorDropArea currentPreviewArea = null;

//    void Start()
//    {
//        rectTransform = GetComponent<RectTransform>();
//        canvasGroup = GetComponent<CanvasGroup>();
//        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
//    }

//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        originalParent = transform.parent;
//        originalSiblingIndex = transform.GetSiblingIndex();
//        originalAnchoredPos = rectTransform.anchoredPosition;

//        transform.SetParent(rootCanvas.transform, true);
//        transform.SetAsLastSibling();
//        canvasGroup.blocksRaycasts = false;
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

//        ColorDropArea hoveredArea = GetDropAreaUnderPointer(eventData.position);

//        if (hoveredArea != currentPreviewArea)
//        {
//            // ✅ Hide ALL previews first
//            ColorDropArea[] allAreas = FindObjectsOfType<ColorDropArea>();
//            foreach (ColorDropArea area in allAreas)
//                area.HidePreview();

//            // ✅ Show ONLY on best match
//            if (hoveredArea != null)
//                hoveredArea.ShowPreview(dragColor);

//            currentPreviewArea = hoveredArea;
//        }
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        canvasGroup.blocksRaycasts = true;

//        // ✅ Hide all previews
//        ColorDropArea[] allAreas = FindObjectsOfType<ColorDropArea>();
//        foreach (ColorDropArea area in allAreas)
//            area.HidePreview();

//        currentPreviewArea = null;

//        ColorDropArea bestMatch = GetDropAreaUnderPointer(eventData.position);

//        if (bestMatch != null)
//        {
//            Debug.Log($"[ColorDrag] Coloring: {bestMatch.gameObject.name}");
//            bestMatch.OnDrop(eventData);
//        }
//        else
//        {
//            Debug.Log("[ColorDrag] Nothing found - empty area");
//        }

//        transform.SetParent(originalParent, true);
//        transform.SetSiblingIndex(originalSiblingIndex);
//        rectTransform.anchoredPosition = originalAnchoredPos;
//    }

//    private ColorDropArea GetDropAreaUnderPointer(Vector2 screenPos)
//    {
//        ColorDropArea[] allDropAreas = FindObjectsOfType<ColorDropArea>();

//        ColorDropArea bestMatch = null;
//        int highestSiblingIndex = -1;

//        foreach (ColorDropArea dropArea in allDropAreas)
//        {
//            if (dropArea.IsColored) continue;
//            if (!IsVisiblePixelAtPosition(dropArea, screenPos)) continue;

//            int siblingIndex = dropArea.transform.GetSiblingIndex();
//            if (siblingIndex > highestSiblingIndex)
//            {
//                highestSiblingIndex = siblingIndex;
//                bestMatch = dropArea;
//            }
//        }

//        return bestMatch;
//    }

//    private bool IsVisiblePixelAtPosition(ColorDropArea dropArea, Vector2 screenPos)
//    {
//        Image img = dropArea.GetComponent<Image>();
//        if (img == null || img.sprite == null) return false;

//        RectTransform rt = img.GetComponent<RectTransform>();

//        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//            ? null : Camera.main;

//        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
//            return false;

//        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//            rt, screenPos, cam, out Vector2 localPoint
//        );

//        Rect rect = rt.rect;
//        float normalizedX = Mathf.Clamp01((localPoint.x - rect.x) / rect.width);
//        float normalizedY = Mathf.Clamp01((localPoint.y - rect.y) / rect.height);

//        Sprite sprite = img.sprite;
//        Texture2D tex = sprite.texture;
//        Rect spriteRect = sprite.textureRect;

//        int px = Mathf.RoundToInt(spriteRect.x + normalizedX * spriteRect.width);
//        int py = Mathf.RoundToInt(spriteRect.y + normalizedY * spriteRect.height);

//        px = Mathf.Clamp(px, 0, tex.width - 1);
//        py = Mathf.Clamp(py, 0, tex.height - 1);

//        Color pixel = tex.GetPixel(px, py);
//        return pixel.a > 0.01f;
//    }
//}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Color dragColor;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 originalAnchoredPos;
    private ColorDropArea currentPreviewArea = null;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalAnchoredPos = rectTransform.anchoredPosition;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

        // ✅ Get best area under pointer using pixel check
        ColorDropArea hoveredArea = GetDropAreaUnderPointer(eventData.position);

        if (hoveredArea != currentPreviewArea)
        {
            // ✅ Hide ALL previews first
            ColorDropArea[] allAreas = FindObjectsOfType<ColorDropArea>();
            foreach (ColorDropArea area in allAreas)
                area.HidePreview();

            // ✅ Show preview on ONLY the correct area
            if (hoveredArea != null)
                hoveredArea.ShowPreview(dragColor);

            currentPreviewArea = hoveredArea;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // ✅ Hide all previews on drop
        ColorDropArea[] allAreas = FindObjectsOfType<ColorDropArea>();
        foreach (ColorDropArea area in allAreas)
            area.HidePreview();

        currentPreviewArea = null;

        ColorDropArea bestMatch = GetDropAreaUnderPointer(eventData.position);

        if (bestMatch != null)
        {
            Debug.Log($"[ColorDrag] Dropping on: {bestMatch.gameObject.name}");
            bestMatch.OnDrop(eventData);
        }
        else
        {
            Debug.Log("[ColorDrag] Nothing found - empty area");
        }

        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPos;
    }

    private ColorDropArea GetDropAreaUnderPointer(Vector2 screenPos)
    {
        ColorDropArea[] allDropAreas = FindObjectsOfType<ColorDropArea>();

        ColorDropArea bestMatch = null;
        int highestSiblingIndex = -1;

        foreach (ColorDropArea dropArea in allDropAreas)
        {
            if (dropArea.IsColored) continue;
            if (!IsVisiblePixelAtPosition(dropArea, screenPos)) continue;

            int siblingIndex = dropArea.transform.GetSiblingIndex();
            if (siblingIndex > highestSiblingIndex)
            {
                highestSiblingIndex = siblingIndex;
                bestMatch = dropArea;
            }
        }

        return bestMatch;
    }

    private bool IsVisiblePixelAtPosition(ColorDropArea dropArea, Vector2 screenPos)
    {
        Image img = dropArea.GetComponent<Image>();
        if (img == null || img.sprite == null) return false;

        RectTransform rt = img.GetComponent<RectTransform>();

        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : Camera.main;

        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
            return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, screenPos, cam, out Vector2 localPoint
        );

        Rect rect = rt.rect;
        float normalizedX = Mathf.Clamp01((localPoint.x - rect.x) / rect.width);
        float normalizedY = Mathf.Clamp01((localPoint.y - rect.y) / rect.height);

        Sprite sprite = img.sprite;
        Texture2D tex = sprite.texture;
        Rect spriteRect = sprite.textureRect;

        int px = Mathf.RoundToInt(spriteRect.x + normalizedX * spriteRect.width);
        int py = Mathf.RoundToInt(spriteRect.y + normalizedY * spriteRect.height);

        px = Mathf.Clamp(px, 0, tex.width - 1);
        py = Mathf.Clamp(py, 0, tex.height - 1);

        Color pixel = tex.GetPixel(px, py);
        return pixel.a > 0.01f;
    }
}
//```

//## How It Works Now
//```
//Drag ANY color over fence
//  → fence pixel visible → ShowPreview ✅ (always shows)
//  → Drop wrong color    → HidePreview, nothing locked ✅
//  → Drop correct color  → Locked permanently ✅

//Drag ANY color over grass
//  → grass pixel visible → ShowPreview ✅ (always shows)
//  → Drop wrong color    → HidePreview, nothing locked ✅
//  → Drop correct color  → Locked permanently ✅