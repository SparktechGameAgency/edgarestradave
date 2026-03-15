//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class RepairDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//{
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
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

//        // ✅ Show preview on any slot under pointer
//        RepairSlot hoveredSlot = GetSlotUnderPointer(eventData.position);

//        if (hoveredSlot != currentPreviewSlot)
//        {
//            // ✅ Hide all previews
//            RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();
//            foreach (RepairSlot slot in allSlots)
//                slot.HidePreview();

//            // ✅ Show preview on any hovered slot
//            if (hoveredSlot != null)
//                hoveredSlot.ShowPreview();

//            currentPreviewSlot = hoveredSlot;
//        }
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        canvasGroup.blocksRaycasts = true;

//        // ✅ Hide all previews
//        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();
//        foreach (RepairSlot slot in allSlots)
//            slot.HidePreview();

//        currentPreviewSlot = null;

//        // ✅ Drop on any slot - no partID check
//        RepairSlot targetSlot = GetSlotUnderPointer(eventData.position);

//        if (targetSlot != null)
//        {
//            targetSlot.Repair(this);
//        }

//        transform.SetParent(originalParent, true);
//        transform.SetSiblingIndex(originalSiblingIndex);
//        rectTransform.anchoredPosition = originalAnchoredPos;
//    }

//    private RepairSlot GetSlotUnderPointer(Vector2 screenPos)
//    {
//        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

//        foreach (RepairSlot slot in allSlots)
//        {
//            if (slot.isRepaired) continue;

//            RectTransform rt = slot.GetComponent<RectTransform>();
//            if (rt == null) continue;

//            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//                ? null : Camera.main;

//            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
//                return slot;
//        }

//        return null;
//    }
//}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RepairDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Part Identity")]
    public string partID; // e.g. "body", "engine", "exhaust"

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
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;

        // ✅ Only show preview on MATCHING slot
        RepairSlot hoveredSlot = GetMatchingSlotUnderPointer(eventData.position);

        if (hoveredSlot != currentPreviewSlot)
        {
            // ✅ Hide all previews first
            HideAllPreviews();

            // ✅ Show only if correct slot
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

        // ✅ Only drop on MATCHING slot
        RepairSlot targetSlot = GetMatchingSlotUnderPointer(eventData.position);

        if (targetSlot != null)
        {
            targetSlot.Repair(this);
        }
        else
        {
            // ✅ Check if dropped on WRONG slot - shake it
            RepairSlot wrongSlot = GetAnySlotUnderPointer(eventData.position);
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

    // ✅ Only returns slot if partID matches
    private RepairSlot GetMatchingSlotUnderPointer(Vector2 screenPos)
    {
        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

        foreach (RepairSlot slot in allSlots)
        {
            if (slot.isRepaired) continue;

            // ✅ Must match partID
            if (slot.requiredPartID != partID) continue;

            RectTransform rt = slot.GetComponent<RectTransform>();
            if (rt == null) continue;

            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : Camera.main;

            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
                return slot;
        }

        return null;
    }

    // ✅ Returns ANY slot (for wrong drop shake)
    private RepairSlot GetAnySlotUnderPointer(Vector2 screenPos)
    {
        RepairSlot[] allSlots = FindObjectsOfType<RepairSlot>();

        foreach (RepairSlot slot in allSlots)
        {
            if (slot.isRepaired) continue;

            RectTransform rt = slot.GetComponent<RectTransform>();
            if (rt == null) continue;

            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : Camera.main;

            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam))
                return slot;
        }

        return null;
    }
}