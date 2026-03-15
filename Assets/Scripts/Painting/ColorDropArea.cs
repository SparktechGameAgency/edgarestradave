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

////    // ✅ Store original sprite color
////    private Color originalColor;

////    void Start()
////    {
////        if (targetPartImage == null)
////            targetPartImage = GetComponent<Image>();

////        // ✅ Save original color before hiding
////        originalColor = targetPartImage.color;

////        // ✅ Hide at start - keep original RGB, just set alpha=0
////        targetPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
////            0f
////        );

////        targetPartImage.raycastTarget = true;
////        IsColored = false;
////    }

////    public void ShowPreview(Color color)
////    {
////        if (IsColored) return;

////        // ✅ Show original sprite color at half opacity
////        targetPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
////            0.5f
////        );
////    }

////    public void HidePreview()
////    {
////        if (IsColored) return;

////        // ✅ Hide back to transparent
////        targetPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
////            0f
////        );
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
////        if (IsColored) return;

////        ColorDrag drag = eventData.pointerDrag?.GetComponent<ColorDrag>();
////        if (drag == null) return;

////        if (requireCorrectColor && !ColorsMatch(drag.dragColor, correctColor))
////        {
////            Debug.Log($"[DropArea] Wrong color for {gameObject.name}!");
////            HidePreview();
////            return;
////        }

////        // ✅ Reveal original sprite color fully
////        targetPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
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
////        targetPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
////            0f
////        );
////    }
////}
//////```

//////## How It Works Now
//////```
//////Before game starts → sprite visible with original colors ✅
//////Game starts        → alpha=0, sprite hidden ✅
//////Drag over fence    → alpha=0.5, original brown shows as preview ✅
//////Drop correct color → alpha=1, original brown fully visible ✅
//////Drop wrong color   → alpha=0, hidden again ✅
//////```

//////## Important — Set Sprite Color in Inspector

//////Before playing, make sure each sprite's **Image → Color is WHITE (1,1,1,1)**:
//////```
//////Fench  → Image → Color = (1,1,1,1) white ✅
//////Grass1 → Image → Color = (1,1,1,1) white ✅


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

//    private Color originalColor;

//    void Start()
//    {
//        if (targetPartImage == null)
//            targetPartImage = GetComponent<Image>();

//        originalColor = targetPartImage.color;

//        targetPartImage.color = new Color(
//            originalColor.r,
//            originalColor.g,
//            originalColor.b,
//            0f
//        );

//        targetPartImage.raycastTarget = true;
//        IsColored = false;
//    }

//    public void ShowPreview(Color color)
//    {
//        if (IsColored) return;

//        targetPartImage.color = new Color(
//            originalColor.r,
//            originalColor.g,
//            originalColor.b,
//            0.5f
//        );
//    }

//    public void HidePreview()
//    {
//        if (IsColored) return;

//        targetPartImage.color = new Color(
//            originalColor.r,
//            originalColor.g,
//            originalColor.b,
//            0f
//        );
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

//    //public void OnDrop(PointerEventData eventData)
//    //{
//    //    if (IsColored) return;

//    //    ColorDrag drag = eventData.pointerDrag?.GetComponent<ColorDrag>();
//    //    if (drag == null) return;

//    //    if (requireCorrectColor && !ColorsMatch(drag.dragColor, correctColor))
//    //    {
//    //        Debug.Log($"[DropArea] Wrong color for {gameObject.name}!");
//    //        HidePreview();
//    //        return;
//    //    }

//    //    targetPartImage.color = new Color(
//    //        originalColor.r,
//    //        originalColor.g,
//    //        originalColor.b,
//    //        1f
//    //    );
//    //    IsColored = true;
//    //    Debug.Log($"[DropArea] SUCCESS - {gameObject.name} colored!");

//    //    // ✅ Check if all zones are completed
//    //    if (GameManager.Instance != null)
//    //        GameManager.Instance.CheckAllColored();
//    //}
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
//            originalColor.r,
//            originalColor.g,
//            originalColor.b,
//            1f
//        );
//        IsColored = true;
//        Debug.Log($"[DropArea] SUCCESS - {gameObject.name} colored!");

//        // ✅ Tell GameManager one zone is done
//        if (GameManager.Instance != null)
//            GameManager.Instance.OnZoneColored();
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
//        targetPartImage.color = new Color(
//            originalColor.r,
//            originalColor.g,
//            originalColor.b,
//            0f
//        );
//    }
//}
////```

////## Unity Setup For Congrats Panel
////```
////Canvas
////  └── CongratsPanel          ← assign to GameManager.congratsPanel
////        ├── Background        (dark semi-transparent)
////        ├── CongratsImage     ← assign to GameManager.congratsImage
////        │    (pulsing colored picture)
////        ├── Text "🎉 Congratulations! 🎉"
////        └── CloseButton       → OnClick → GameManager.CloseCurrectPanel()
////```

////**GameManager Inspector:**
////```
////Congrats Panel        → CongratsPanel GameObject
////Congrats Image        → CongratsImage component
////Original Colored Sprite → your colored reference PNG
////```

////## How It Works
////```
////Last zone colored → CheckAllColored() → all IsColored=true
////                 → ShowCongrats()
////                 → Panel bounces in ✅
////                 → Image pulses forever ✅
////Close button     → CloseCurrectPanel()
////                 → Pulse stops ✅
////                 → Panel shrinks out ✅
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

    private Color originalColor;

    void Start()
    {
        if (targetPartImage == null)
            targetPartImage = GetComponent<Image>();

        originalColor = targetPartImage.color;

        targetPartImage.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            0f
        );

        targetPartImage.raycastTarget = true;
        IsColored = false;
    }

    public void ShowPreview(Color color)
    {
        if (IsColored) return;
        targetPartImage.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            0.5f
        );
    }

    public void HidePreview()
    {
        if (IsColored) return;
        targetPartImage.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            0f
        );
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

        if (requireCorrectColor && !ColorsMatch(drag.dragColor, correctColor))
        {
            Debug.Log($"[DropArea] Wrong color for {gameObject.name}!");
            HidePreview();
            return;
        }

        targetPartImage.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            1f
        );
        IsColored = true;
        Debug.Log($"[DropArea] SUCCESS - {gameObject.name} colored!");

        // ✅ Notify GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.OnZoneColored();
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
        targetPartImage.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            0f
        );
    }
}
//```

//---

//## Inspector Setup

//**GameManager:**
//```
//Congrats Panel    → your CongratsPanel GameObject
//Pulsing Object    → any GameObject (image/star/character)

//All Drop Areas (15):
//  Element 0  → Fench
//  Element 1  → Grass1
//  Element 2  → Grass2
//  Element 3  → Grass3
//  Element 4  → Grass4
//  Element 5  → Sky
//  Element 6  → Trees
//  Element 7  → House
//  Element 8  → Cows
//  Element 9  → BrownCow
//  Element 10 → WhiteCow
//  Element 11 → (add remaining)
//  Element 12 → ...
//  Element 13 → ...
//  Element 14 → ...
//```

//To add them quickly:
//```
//Click lock icon 🔒 on Inspector
//Select all 15 GameObjects in Hierarchy
//Drag them all into "All Drop Areas" list at once
//```

//## How It Works
//```
//Game starts     → coloredCount = 0, panel hidden ✅
//Color 1 done    → coloredCount = 1  (1/15) ✅
//Color 2 done    → coloredCount = 2  (2/15) ✅
//...
//Color 15 done   → coloredCount = 15 (15/15)
//                → CongratsPanel appears! 🎉
//                → PulsingObject bounces forever! 💫
//Close button    → panel hides, pulse stops ✅