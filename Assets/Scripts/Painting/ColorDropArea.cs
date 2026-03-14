//////using UnityEngine;
//////using UnityEngine.EventSystems;
//////using UnityEngine.UI;

//////public class ColorDropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
//////{
//////    [Header("Settings")]
//////    public bool requireCorrectColor = false;
//////    public Color correctColor;

//////    [Header("Target Image to Color")]
//////    public Image targetPartImage;

//////    public bool IsColored { get; private set; }

//////    void Start()
//////    {
//////        if (targetPartImage == null)
//////            targetPartImage = GetComponent<Image>();

//////        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
//////        targetPartImage.raycastTarget = true;
//////        IsColored = false;
//////    }

//////    public void OnPointerEnter(PointerEventData eventData)
//////    {
//////        if (IsColored) return;
//////        if (eventData.pointerDrag == null) return;

//////        ColorDrag drag = eventData.pointerDrag.GetComponent<ColorDrag>();
//////        if (drag == null) return;

//////        targetPartImage.color = new Color(
//////            drag.dragColor.r,
//////            drag.dragColor.g,
//////            drag.dragColor.b,
//////            0.5f
//////        );
//////    }

//////    public void OnPointerExit(PointerEventData eventData)
//////    {
//////        if (IsColored) return;
//////        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
//////    }

//////    public void OnDrop(PointerEventData eventData)
//////    {
//////        Debug.Log($"[DropArea] OnDrop called on {gameObject.name}!");
//////        if (IsColored) return;

//////        ColorDrag drag = eventData.pointerDrag?.GetComponent<ColorDrag>();
//////        if (drag == null)
//////        {
//////            Debug.Log("[DropArea] drag is NULL!");
//////            return;
//////        }

//////        if (requireCorrectColor && !ColorsMatch(drag.dragColor, correctColor))
//////        {
//////            Debug.Log($"[DropArea] Wrong color!");
//////            targetPartImage.color = new Color(1f, 1f, 1f, 0f);
//////            return;
//////        }

//////        targetPartImage.color = new Color(
//////            drag.dragColor.r,
//////            drag.dragColor.g,
//////            drag.dragColor.b,
//////            1f
//////        );
//////        IsColored = true;
//////        Debug.Log($"[DropArea] SUCCESS - {gameObject.name} colored!");
//////    }

//////    private bool ColorsMatch(Color a, Color b, float tolerance = 0.1f)
//////    {
//////        return Mathf.Abs(a.r - b.r) < tolerance &&
//////               Mathf.Abs(a.g - b.g) < tolerance &&
//////               Mathf.Abs(a.b - b.b) < tolerance;
//////    }

//////    public void ResetZone()
//////    {
//////        IsColored = false;
//////        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
//////    }
//////}

////using UnityEngine;
////using UnityEngine.EventSystems;
////using UnityEngine.UI;

////public class ColorDropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
////{
////    [Header("Settings")]
////    public bool requireCorrectColor = false;
////    public Color correctColor;

////    [Header("Target Image to Color")]
////    public Image targetPartImage;

////    public bool IsColored { get; private set; }

////    void Start()
////    {
////        if (targetPartImage == null)
////            targetPartImage = GetComponent<Image>();

////        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
////        targetPartImage.raycastTarget = true;
////        IsColored = false;
////    }

////    // ✅ Called by ColorDrag every frame while hovering
////    public void ShowPreview(Color color)
////    {
////        if (IsColored) return;
////        targetPartImage.color = new Color(color.r, color.g, color.b, 0.5f);
////    }

////    // ✅ Called by ColorDrag when leaving this area
////    public void HidePreview()
////    {
////        if (IsColored) return;
////        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
////    }

////    public void OnPointerEnter(PointerEventData eventData)
////    {
////        if (IsColored) return;
////        if (eventData.pointerDrag == null) return;

////        ColorDrag drag = eventData.pointerDrag.GetComponent<ColorDrag>();
////        if (drag == null) return;

////        ShowPreview(drag.dragColor);
////    }

////    public void OnPointerExit(PointerEventData eventData)
////    {
////        if (IsColored) return;
////        HidePreview();
////    }

////    public void OnDrop(PointerEventData eventData)
////    {
////        Debug.Log($"[DropArea] OnDrop called on {gameObject.name}!");
////        if (IsColored) return;

////        ColorDrag drag = eventData.pointerDrag?.GetComponent<ColorDrag>();
////        if (drag == null)
////        {
////            Debug.Log("[DropArea] drag is NULL!");
////            return;
////        }

////        if (requireCorrectColor && !ColorsMatch(drag.dragColor, correctColor))
////        {
////            Debug.Log($"[DropArea] Wrong color!");
////            HidePreview();
////            return;
////        }

////        // ✅ Lock color permanently
////        targetPartImage.color = new Color(
////            drag.dragColor.r,
////            drag.dragColor.g,
////            drag.dragColor.b,
////            1f
////        );
////        IsColored = true;
////        Debug.Log($"[DropArea] SUCCESS - {gameObject.name} colored!");
////    }

////    private bool ColorsMatch(Color a, Color b, float tolerance = 0.1f)
////    {
////        return Mathf.Abs(a.r - b.r) < tolerance &&
////               Mathf.Abs(a.g - b.g) < tolerance &&
////               Mathf.Abs(a.b - b.b) < tolerance;
////    }

////    public void ResetZone()
////    {
////        IsColored = false;
////        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
////    }
////}

//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class ColorDropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
//{
//    [Header("Settings")]
//    public bool requireCorrectColor = false;
//    public Color correctColor;

//    [Header("Target Image to Color")]
//    public Image targetPartImage;

//    public bool IsColored { get; private set; }

//    void Start()
//    {
//        if (targetPartImage == null)
//            targetPartImage = GetComponent<Image>();

//        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
//        targetPartImage.raycastTarget = true;
//        IsColored = false;
//    }

//    public void ShowPreview(Color color)
//    {
//        if (IsColored) return;

//        // ✅ Only show preview if color matches (when required)
//        if (requireCorrectColor && !ColorsMatch(color, correctColor))
//            return;

//        targetPartImage.color = new Color(color.r, color.g, color.b, 0.5f);
//    }

//    public void HidePreview()
//    {
//        if (IsColored) return;
//        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (IsColored) return;
//        if (eventData.pointerDrag == null) return;

//        ColorDrag drag = eventData.pointerDrag.GetComponent<ColorDrag>();
//        if (drag == null) return;

//        ShowPreview(drag.dragColor);
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        if (IsColored) return;
//        HidePreview();
//    }

//    public void OnDrop(PointerEventData eventData)
//    {
//        if (IsColored) return;

//        ColorDrag drag = eventData.pointerDrag?.GetComponent<ColorDrag>();
//        if (drag == null) return;

//        if (requireCorrectColor && !ColorsMatch(drag.dragColor, correctColor))
//        {
//            Debug.Log($"[DropArea] Wrong color for {gameObject.name}!");
//            HidePreview();
//            return;
//        }

//        targetPartImage.color = new Color(
//            drag.dragColor.r,
//            drag.dragColor.g,
//            drag.dragColor.b,
//            1f
//        );
//        IsColored = true;
//        Debug.Log($"[DropArea] SUCCESS - {gameObject.name} colored!");
//    }

//    private bool ColorsMatch(Color a, Color b, float tolerance = 0.1f)
//    {
//        return Mathf.Abs(a.r - b.r) < tolerance &&
//               Mathf.Abs(a.g - b.g) < tolerance &&
//               Mathf.Abs(a.b - b.b) < tolerance;
//    }

//    public void ResetZone()
//    {
//        IsColored = false;
//        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
//    }
//}
////```

////## Result
////```
////Drag fence color over grass  → grass pixel check passes
////                             → ShowPreview called
////                             → requireCorrectColor = true
////                             → colors don't match → NO preview ✅

////Drag fence color over fence  → fence pixel check passes  
////                             → ShowPreview called
////                             → colors match → preview shown ✅

////Drop fence color on fence    → SUCCESS ✅
////Drop fence color on grass    → Wrong color → nothing happens ✅
///

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorDropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public bool requireCorrectColor = false;
    public Color correctColor;

    [Header("Target Image to Color")]
    public Image targetPartImage;

    public bool IsColored { get; private set; }

    void Start()
    {
        if (targetPartImage == null)
            targetPartImage = GetComponent<Image>();

        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
        targetPartImage.raycastTarget = true;
        IsColored = false;
    }

    public void ShowPreview(Color color)
    {
        if (IsColored) return;

        // ✅ Always show preview for any color
        targetPartImage.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void HidePreview()
    {
        if (IsColored) return;
        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsColored) return;
        if (eventData.pointerDrag == null) return;

        ColorDrag drag = eventData.pointerDrag.GetComponent<ColorDrag>();
        if (drag == null) return;

        ShowPreview(drag.dragColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsColored) return;
        HidePreview();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (IsColored) return;

        ColorDrag drag = eventData.pointerDrag?.GetComponent<ColorDrag>();
        if (drag == null) return;

        // ✅ Only lock color if correct color dropped
        if (requireCorrectColor && !ColorsMatch(drag.dragColor, correctColor))
        {
            Debug.Log($"[DropArea] Wrong color for {gameObject.name}!");
            HidePreview(); // ✅ Hide preview on wrong drop
            return;
        }

        // ✅ Correct color - lock permanently
        targetPartImage.color = new Color(
            drag.dragColor.r,
            drag.dragColor.g,
            drag.dragColor.b,
            1f
        );
        IsColored = true;
        Debug.Log($"[DropArea] SUCCESS - {gameObject.name} colored!");
    }

    private bool ColorsMatch(Color a, Color b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    public void ResetZone()
    {
        IsColored = false;
        targetPartImage.color = new Color(1f, 1f, 1f, 0f);
    }
}