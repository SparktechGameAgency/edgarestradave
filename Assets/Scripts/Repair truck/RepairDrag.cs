//////using UnityEditor;
//////using UnityEngine;
//////using UnityEngine.EventSystems;
//////using UnityEngine.UI;

//////public class RepairDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//////{
//////    [Header("Part Identity")]
//////    public string partID;

//////    private RectTransform rectTransform;
//////    private Canvas rootCanvas;
//////    private CanvasGroup canvasGroup;
//////    private Transform originalParent;
//////    private int originalSiblingIndex;
//////    private Vector2 originalAnchoredPos;
//////    private RepairSlot currentPreviewSlot = null;

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

//////        // ✅ Hide ALL slots at drag start
//////        HideAllPreviews();
//////    }

//////    public void OnDrag(PointerEventData eventData)
//////    {
//////        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

//////        // ✅ Only get MATCHING slot under pointer
//////        RepairSlot hoveredSlot = GetMatchingSlotUnderPointer(eventData.position);

//////        if (hoveredSlot != currentPreviewSlot)
//////        {
//////            // ✅ Hide all first
//////            HideAllPreviews();

//////            // ✅ Show ONLY if correct slot
//////            if (hoveredSlot != null)
//////                hoveredSlot.ShowPreview();

//////            currentPreviewSlot = hoveredSlot;
//////        }
//////    }

//////    public void OnEndDrag(PointerEventData eventData)
//////    {
//////        canvasGroup.blocksRaycasts = true;

//////        // ✅ Hide all previews
//////        HideAllPreviews();
//////        currentPreviewSlot = null;

//////        // ✅ Only drop on MATCHING slot
//////        RepairSlot targetSlot = GetMatchingSlotUnderPointer(eventData.position);

//////        if (targetSlot != null)
//////        {
//////            // ✅ Correct slot - repair!
//////            targetSlot.Repair(this);
//////        }
//////        else
//////        {
//////            // ✅ Wrong slot - shake
//////            RepairSlot wrongSlot = GetAnySlotUnderPointer(eventData.position);
//////            if (wrongSlot != null)
//////                wrongSlot.WrongDrop();

//////            // ✅ Return to original position
//////            transform.SetParent(originalParent, true);
//////            transform.SetSiblingIndex(originalSiblingIndex);
//////            rectTransform.anchoredPosition = originalAnchoredPos;
//////            return;
//////        }

//////        transform.SetParent(originalParent, true);
//////        transform.SetSiblingIndex(originalSiblingIndex);
//////        rectTransform.anchoredPosition = originalAnchoredPos;
//////    }

//////    void HideAllPreviews()
//////    {
//////        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();
//////        foreach (RepairSlot slot in allSlots)
//////            slot.HidePreview();
//////    }

//////    // ✅ Only returns slot if partID matches
//////    private RepairSlot GetMatchingSlotUnderPointer(Vector2 screenPos)
//////    {
//////        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

//////        foreach (RepairSlot slot in allSlots)
//////        {
//////            if (slot.isRepaired) continue;
//////            if (slot.requiredPartID != partID) continue;

//////            RectTransform rt = slot.GetComponent<RectTransform>();
//////            if (rt == null) continue;

//////            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//////                ? null : Camera.main;

//////            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
//////                return slot;
//////        }

//////        return null;
//////    }

//////    // ✅ Returns any slot (for wrong drop shake)
//////    private RepairSlot GetAnySlotUnderPointer(Vector2 screenPos)
//////    {
//////        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

//////        foreach (RepairSlot slot in allSlots)
//////        {
//////            if (slot.isRepaired) continue;

//////            RectTransform rt = slot.GetComponent<RectTransform>();
//////            if (rt == null) continue;

//////            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//////                ? null : Camera.main;

//////            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
//////                return slot;
//////        }

//////        return null;
//////    }
//////}


////using UnityEngine;
////using UnityEngine.EventSystems;
////using UnityEngine.UI;

////public class RepairDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
////{
////    [Header("Part Identity")]
////    public string partID;

////    private RectTransform rectTransform;
////    private Canvas rootCanvas;
////    private CanvasGroup canvasGroup;
////    private Transform originalParent;
////    private int originalSiblingIndex;
////    private Vector2 originalAnchoredPos;
////    private RepairSlot currentPreviewSlot = null;

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

////        // ✅ Hide everything when drag starts
////        HideAllPreviews();
////        currentPreviewSlot = null;
////    }

////    public void OnDrag(PointerEventData eventData)
////    {
////        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

////        // ✅ Strictly check pointer is INSIDE slot rect
////        RepairSlot hoveredSlot = GetMatchingSlotUnderPointer(eventData.position);

////        if (hoveredSlot != currentPreviewSlot)
////        {
////            HideAllPreviews();

////            if (hoveredSlot != null)
////                hoveredSlot.ShowPreview();

////            currentPreviewSlot = hoveredSlot;
////        }
////    }

////    public void OnEndDrag(PointerEventData eventData)
////    {
////        canvasGroup.blocksRaycasts = true;
////        HideAllPreviews();
////        currentPreviewSlot = null;

////        RepairSlot targetSlot = GetMatchingSlotUnderPointer(eventData.position);

////        if (targetSlot != null)
////        {
////            targetSlot.Repair(this);
////        }
////        else
////        {
////            RepairSlot wrongSlot = GetAnySlotUnderPointer(eventData.position);
////            if (wrongSlot != null)
////                wrongSlot.WrongDrop();
////        }

////        transform.SetParent(originalParent, true);
////        transform.SetSiblingIndex(originalSiblingIndex);
////        rectTransform.anchoredPosition = originalAnchoredPos;
////    }

////    void HideAllPreviews()
////    {
////        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();
////        foreach (RepairSlot slot in allSlots)
////            slot.HidePreview();
////    }

////    private RepairSlot GetMatchingSlotUnderPointer(Vector2 screenPos)
////    {
////        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

////        foreach (RepairSlot slot in allSlots)
////        {
////            if (slot.isRepaired) continue;
////            if (slot.requiredPartID != partID) continue;

////            RectTransform rt = slot.GetComponent<RectTransform>();
////            if (rt == null) continue;

////            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
////                ? null : Camera.main;

////            // ✅ Strict check - pointer must be INSIDE rect
////            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
////                return slot;
////        }
////        return null;
////    }

////    private RepairSlot GetAnySlotUnderPointer(Vector2 screenPos)
////    {
////        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

////        foreach (RepairSlot slot in allSlots)
////        {
////            if (slot.isRepaired) continue;

////            RectTransform rt = slot.GetComponent<RectTransform>();
////            if (rt == null) continue;

////            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
////                ? null : Camera.main;

////            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
////                return slot;
////        }
////        return null;
////    }
////}
//////```

//////## Also Fix In Unity — Resize Slot RectTransform

//////The slot RectTransform might be too big. Check:
//////```
//////Select each RepairSlot in Hierarchy
//////Press T (Rect Tool) in Scene view
//////Make sure the blue rect exactly covers
//////ONLY the slot area on the truck ✅
//////```

//////## Inspector — Force All Slot Images
//////```
//////Every RepairSlot Image:
//////Color → R:255 G: 255 B: 255 A: 255
/////
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class RepairDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//{
//    [Header("Part Identity")]
//    public string partID;

//    private RectTransform rectTransform;
//    private Canvas rootCanvas;
//    private CanvasGroup canvasGroup;
//    private Transform originalParent;
//    private int originalSiblingIndex;
//    private Vector2 originalAnchoredPos;
//    private RepairSlot currentPreviewSlot = null;

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

//        HideAllPreviews();
//        currentPreviewSlot = null;
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

//        // ✅ Only show preview when over VISIBLE pixel of matching slot
//        RepairSlot hoveredSlot = GetMatchingSlotUnderPointer(eventData.position);

//        if (hoveredSlot != currentPreviewSlot)
//        {
//            HideAllPreviews();

//            if (hoveredSlot != null)
//                hoveredSlot.ShowPreview();

//            currentPreviewSlot = hoveredSlot;
//        }
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        canvasGroup.blocksRaycasts = true;
//        HideAllPreviews();
//        currentPreviewSlot = null;

//        RepairSlot targetSlot = GetMatchingSlotUnderPointer(eventData.position);

//        if (targetSlot != null)
//        {
//            targetSlot.Repair(this);
//        }
//        else
//        {
//            RepairSlot wrongSlot = GetAnyVisibleSlotUnderPointer(eventData.position);
//            if (wrongSlot != null)
//                wrongSlot.WrongDrop();
//        }

//        transform.SetParent(originalParent, true);
//        transform.SetSiblingIndex(originalSiblingIndex);
//        rectTransform.anchoredPosition = originalAnchoredPos;
//    }

//    void HideAllPreviews()
//    {
//        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();
//        foreach (RepairSlot slot in allSlots)
//            slot.HidePreview();
//    }

//    // ✅ Returns matching slot only if pointer is over visible pixel
//    private RepairSlot GetMatchingSlotUnderPointer(Vector2 screenPos)
//    {
//        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

//        foreach (RepairSlot slot in allSlots)
//        {
//            if (slot.isRepaired) continue;
//            if (slot.requiredPartID != partID) continue;

//            // ✅ Check pixel transparency
//            if (IsVisiblePixelAtPosition(slot, screenPos))
//                return slot;
//        }

//        return null;
//    }

//    // ✅ Returns any slot with visible pixel (for wrong drop)
//    private RepairSlot GetAnyVisibleSlotUnderPointer(Vector2 screenPos)
//    {
//        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

//        foreach (RepairSlot slot in allSlots)
//        {
//            if (slot.isRepaired) continue;

//            if (IsVisiblePixelAtPosition(slot, screenPos))
//                return slot;
//        }

//        return null;
//    }

//    // ✅ Same pixel check as painting game
//    private bool IsVisiblePixelAtPosition(RepairSlot slot, Vector2 screenPos)
//    {
//        Image img = slot.repairedPartImage;
//        if (img == null || img.sprite == null) return false;

//        RectTransform rt = img.GetComponent<RectTransform>();
//        if (rt == null) return false;

//        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//            ? null : Camera.main;

//        // ✅ Must be inside rect first
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

//        Debug.Log($"[RepairDrag] {slot.gameObject.name} " +
//                  $"px=({px},{py}) alpha={pixel.a:F2}");

//        // ✅ Only visible if alpha > threshold
//        return pixel.a > 0.1f;
//    }
//}
////```

////## Also — Enable Read/Write On Each Sprite

////Select** every slot sprite** (body, exhaust, engine etc.) in Project:
////```
////Inspector → Texture Import Settings
////✅ Read/Write Enabled
////✅ Alpha Source: Input Texture Alpha
////✅ Alpha Is Transparent
////Format: RGBA 32 bit
////→ Apply
////```

////## How It Works Now
////```
////Drag body part over empty area  → no preview ✅
////Drag body part over body pixels → preview 50% ✅
////Drag body part over other part  → no preview ✅
////Drop on body pixels             → fully visible ✅
////Drop on wrong part pixels       → shake ✅
///

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RepairDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Part Identity")]
    public string partID;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 originalAnchoredPos;
    private RepairSlot currentPreviewSlot = null;

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

        HideAllPreviews();
        currentPreviewSlot = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

        RepairSlot hoveredSlot = GetMatchingSlotUnderPointer(eventData.position);

        if (hoveredSlot != currentPreviewSlot)
        {
            HideAllPreviews();

            if (hoveredSlot != null)
                hoveredSlot.ShowPreview();

            currentPreviewSlot = hoveredSlot;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        HideAllPreviews();
        currentPreviewSlot = null;

        RepairSlot targetSlot = GetMatchingSlotUnderPointer(eventData.position);

        if (targetSlot != null)
        {
            targetSlot.Repair(this);
        }
        else
        {
            RepairSlot wrongSlot = GetAnyVisibleSlotUnderPointer(eventData.position);
            if (wrongSlot != null)
                wrongSlot.WrongDrop();
        }

        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPos;
    }

    void HideAllPreviews()
    {
        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();
        foreach (RepairSlot slot in allSlots)
            slot.HidePreview();
    }

    private RepairSlot GetMatchingSlotUnderPointer(Vector2 screenPos)
    {
        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

        foreach (RepairSlot slot in allSlots)
        {
            if (slot.isRepaired) continue;
            if (slot.requiredPartID != partID) continue;
            if (IsVisiblePixelAtPosition(slot, screenPos))
                return slot;
        }

        return null;
    }

    private RepairSlot GetAnyVisibleSlotUnderPointer(Vector2 screenPos)
    {
        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

        foreach (RepairSlot slot in allSlots)
        {
            if (slot.isRepaired) continue;
            if (IsVisiblePixelAtPosition(slot, screenPos))
                return slot;
        }

        return null;
    }

    private bool IsVisiblePixelAtPosition(RepairSlot slot, Vector2 screenPos)
    {
        Image img = slot.repairedPartImage;
        if (img == null || img.sprite == null) return false;

        RectTransform rt = img.GetComponent<RectTransform>();
        if (rt == null) return false;

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
        return pixel.a > 0.1f;
    }
}