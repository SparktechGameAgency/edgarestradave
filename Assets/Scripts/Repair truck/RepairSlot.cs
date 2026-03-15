//using UnityEngine;
//using UnityEngine.UI;
//using DG.Tweening;

//public class RepairSlot : MonoBehaviour
//{
//    [Header("Slot Identity")]
//    public string requiredPartID;

//    [Header("Slot Visuals")]
//    public Image repairedPartImage;

//    public bool isRepaired = false;
//    private Color originalColor;

//    void Start()
//    {
//        if (repairedPartImage != null)
//        {
//            originalColor = repairedPartImage.color;

//            // ✅ Hide at start
//            repairedPartImage.color = new Color(
//                originalColor.r,
//                originalColor.g,
//                originalColor.b,
//                0f
//            );
//        }

//        isRepaired = false;
//    }

//    // ✅ Show preview at half opacity
//    public void ShowPreview()
//    {
//        if (isRepaired) return;
//        if (repairedPartImage == null) return;

//        repairedPartImage.color = new Color(
//            originalColor.r,
//            originalColor.g,
//            originalColor.b,
//            0.5f
//        );
//    }

//    // ✅ Hide preview
//    public void HidePreview()
//    {
//        if (isRepaired) return;
//        if (repairedPartImage == null) return;

//        repairedPartImage.color = new Color(
//            originalColor.r,
//            originalColor.g,
//            originalColor.b,
//            0f
//        );
//    }

//    // ✅ Correct part dropped
//    public void Repair(RepairDrag drag)
//    {
//        isRepaired = true;

//        if (repairedPartImage != null)
//        {
//            // ✅ Fade in fully
//            repairedPartImage.DOFade(1f, 0.4f)
//                .SetEase(Ease.OutQuad);

//            // ✅ Bounce effect
//            repairedPartImage.transform.localScale = Vector3.one * 0.5f;
//            repairedPartImage.transform
//                .DOScale(1f, 0.4f)
//                .SetEase(Ease.OutBack);
//        }

//        // ✅ Hide toolbar part
//        drag.gameObject.SetActive(false);

//        Debug.Log($"[RepairSlot] SUCCESS - {requiredPartID} repaired!");

//        // ✅ Notify GameManager
//        if (GameManager.Instance != null)
//            GameManager.Instance.OnSlotRepaired();
//    }

//    // ✅ Wrong part dropped - just shake
//    public void WrongDrop()
//    {
//        transform.DOKill();
//        transform.DOShakePosition(0.5f, 20f, 15)
//            .SetEase(Ease.OutElastic);

//        Debug.Log($"[RepairSlot] Wrong part on {gameObject.name}!");
//    }

//    public void ResetSlot()
//    {
//        isRepaired = false;

//        if (repairedPartImage != null)
//        {
//            repairedPartImage.color = new Color(
//                originalColor.r,
//                originalColor.g,
//                originalColor.b,
//                0f
//            );
//            repairedPartImage.transform.localScale = Vector3.one;
//        }
//    }
//}
////```

////## Inspector Setup
////```
////Each toolbar part:
////  ✅ RepairDrag script
////  ✅ CanvasGroup component  ← REQUIRED
////  ✅ partID = "exhaust"

////Each slot on truck:
////  ✅ RepairSlot script
////  ✅ requiredPartID = "exhaust"
////  ✅ repairedPartImage = assigned
////  ✅ Raycast Target = ON

////GameManager:
////  ✅ allRepairSlots - add all 6 slots
////```

////## How It Works — Exactly Like Painting
////```
////Drag part over slot  → preview at 50% opacity ✅
////Drag away            → preview hides ✅
////Drop correct part    → fades in fully + bounce ✅
////Drop wrong part      → slot shakes ✅
////All 6 repaired       → CongratsPanel appears 🎉
///


using DG.Tweening;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class RepairSlot : MonoBehaviour
{
    [Header("Slot Identity")]
    public string requiredPartID; // must match RepairDrag.partID

    [Header("Slot Visuals")]
    public Image repairedPartImage;

    public bool isRepaired = false;
    private Color originalColor;

    void Start()
    {
        if (repairedPartImage != null)
        {
            originalColor = repairedPartImage.color;

            // ✅ Hide at start
            repairedPartImage.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0f
            );
        }

        isRepaired = false;
    }

    public void ShowPreview()
    {
        if (isRepaired) return;
        if (repairedPartImage == null) return;

        repairedPartImage.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            0.5f
        );
    }

    public void HidePreview()
    {
        if (isRepaired) return;
        if (repairedPartImage == null) return;

        repairedPartImage.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            0f
        );
    }

    public void Repair(RepairDrag drag)
    {
        isRepaired = true;

        if (repairedPartImage != null)
        {
            repairedPartImage.DOFade(1f, 0.4f)
                .SetEase(Ease.OutQuad);

            repairedPartImage.transform.localScale = Vector3.one * 0.5f;
            repairedPartImage.transform
                .DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack);
        }

        // ✅ Hide toolbar part
        drag.gameObject.SetActive(false);

        Debug.Log($"[RepairSlot] SUCCESS - {requiredPartID} repaired!");

        if (GameManager.Instance != null)
            GameManager.Instance.OnSlotRepaired();
    }

    public void WrongDrop()
    {
        transform.DOKill();
        transform.DOShakePosition(0.5f, 20f, 15)
            .SetEase(Ease.OutElastic);

        Debug.Log($"[RepairSlot] Wrong part on {gameObject.name}!");
    }

    public void ResetSlot()
    {
        isRepaired = false;

        if (repairedPartImage != null)
        {
            repairedPartImage.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0f
            );
            repairedPartImage.transform.localScale = Vector3.one;
        }
    }
}
//```

//## Inspector Setup
//```
//Toolbar parts → RepairDrag:
//  exhaust  → partID = "exhaust"
//  body     → partID = "body"
//  engine   → partID = "engine"
//  front1   → partID = "front1"
//  front2   → partID = "front2"
//  backwheel→ partID = "backwheel"

//Slots on truck → RepairSlot:
//  exhaust slot  → requiredPartID = "exhaust"
//  body slot     → requiredPartID = "body"
//  engine slot   → requiredPartID = "engine"
//  front1 slot   → requiredPartID = "front1"
//  front2 slot   → requiredPartID = "front2"
//  backwheel slot→ requiredPartID = "backwheel"
//```

//## How It Works
//```
//Drag body part
//  → hover over body slot   → preview 50% ✅
//  → hover over other slots → no preview ✅
//  → drop on body slot      → repairs ✅
//  → drop on wrong slot     → shakes ✅
//All 6 done → CongratsPanel 🎉