//////using UnityEngine;
//////using UnityEngine.UI;
//////using DG.Tweening;

//////public class RepairSlot : MonoBehaviour
//////{
//////    [Header("Slot Identity")]
//////    public string requiredPartID;

//////    [Header("Slot Visuals")]
//////    public Image repairedPartImage;

//////    public bool isRepaired = false;
//////    private Color originalColor;

//////    void Start()
//////    {
//////        if (repairedPartImage != null)
//////        {
//////            // ✅ Save RGB but force alpha=1 for later use
//////            originalColor = repairedPartImage.color;
//////            originalColor.a = 1f;

//////            // ✅ Completely hidden at start
//////            repairedPartImage.color = new Color(
//////                originalColor.r,
//////                originalColor.g,
//////                originalColor.b,
//////                0f
//////            );
//////        }

//////        isRepaired = false;
//////    }

//////    // ✅ Show 50% only when correct part hovers
//////    public void ShowPreview()
//////    {
//////        if (isRepaired) return;
//////        if (repairedPartImage == null) return;

//////        repairedPartImage.color = new Color(
//////            originalColor.r,
//////            originalColor.g,
//////            originalColor.b,
//////            0.5f
//////        );
//////    }

//////    // ✅ Hide back to invisible
//////    public void HidePreview()
//////    {
//////        if (isRepaired) return;
//////        if (repairedPartImage == null) return;

//////        repairedPartImage.color = new Color(
//////            originalColor.r,
//////            originalColor.g,
//////            originalColor.b,
//////            0f
//////        );
//////    }

//////    // ✅ Fully appear on correct drop
//////    public void Repair(RepairDrag drag)
//////    {
//////        isRepaired = true;

//////        if (repairedPartImage != null)
//////        {
//////            repairedPartImage.DOFade(1f, 0.4f)
//////                .SetEase(Ease.OutQuad);

//////            repairedPartImage.transform.localScale = Vector3.one * 0.5f;
//////            repairedPartImage.transform
//////                .DOScale(1f, 0.4f)
//////                .SetEase(Ease.OutBack);
//////        }

//////        drag.gameObject.SetActive(false);
//////        Debug.Log($"[RepairSlot] SUCCESS - {requiredPartID}!");

//////        if (GameManager.Instance != null)
//////            GameManager.Instance.OnSlotRepaired();
//////    }

//////    public void WrongDrop()
//////    {
//////        transform.DOKill();
//////        transform.DOShakePosition(0.5f, 20f, 15)
//////            .SetEase(Ease.OutElastic);
//////    }

//////    public void ResetSlot()
//////    {
//////        isRepaired = false;

//////        if (repairedPartImage != null)
//////        {
//////            repairedPartImage.color = new Color(
//////                originalColor.r,
//////                originalColor.g,
//////                originalColor.b,
//////                0f
//////            );
//////            repairedPartImage.transform.localScale = Vector3.one;
//////        }
//////    }
//////}

////using UnityEngine;
////using UnityEngine.UI;
////using DG.Tweening;

////public class RepairSlot : MonoBehaviour
////{
////    [Header("Slot Identity")]
////    public string requiredPartID;

////    [Header("Slot Visuals")]
////    public Image repairedPartImage;

////    public bool isRepaired = false;
////    private Color originalColor;

////    void Awake()
////    {
////        if (repairedPartImage != null)
////        {
////            originalColor = repairedPartImage.color;
////            originalColor.a = 1f;

////            // ✅ Force completely invisible
////            repairedPartImage.color = new Color(
////                originalColor.r,
////                originalColor.g,
////                originalColor.b,
////                0f
////            );
////        }
////    }

////    void Start()
////    {
////        isRepaired = false;

////        // ✅ Double force hide at start
////        ForceHide();
////    }

////    void ForceHide()
////    {
////        if (repairedPartImage == null) return;
////        repairedPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
////            0f
////        );
////    }

////    public void ShowPreview()
////    {
////        if (isRepaired) return;
////        if (repairedPartImage == null) return;

////        repairedPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
////            0.5f
////        );
////    }

////    public void HidePreview()
////    {
////        if (isRepaired) return;
////        if (repairedPartImage == null) return;

////        repairedPartImage.color = new Color(
////            originalColor.r,
////            originalColor.g,
////            originalColor.b,
////            0f
////        );
////    }

////    public void Repair(RepairDrag drag)
////    {
////        isRepaired = true;

////        if (repairedPartImage != null)
////        {
////            repairedPartImage.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
////            repairedPartImage.transform.localScale = Vector3.one * 0.5f;
////            repairedPartImage.transform
////                .DOScale(1f, 0.4f)
////                .SetEase(Ease.OutBack);
////        }

////        drag.gameObject.SetActive(false);
////        Debug.Log($"[RepairSlot] SUCCESS - {requiredPartID}!");

////        if (GameManager.Instance != null)
////            GameManager.Instance.OnSlotRepaired();
////    }

////    public void WrongDrop()
////    {
////        transform.DOKill();
////        transform.DOShakePosition(0.5f, 20f, 15)
////            .SetEase(Ease.OutElastic);
////    }

////    public void ResetSlot()
////    {
////        isRepaired = false;
////        ForceHide();
////        if (repairedPartImage != null)
////            repairedPartImage.transform.localScale = Vector3.one;
////    }
////}

//using UnityEngine;
//using UnityEngine.UI;
//using DG.Tweening;

//public class RepairSlot : MonoBehaviour
//{
//    [Header("Slot Identity")]
//    public string requiredPartID;

//    [Header("Slot Visuals")]
//    public Image repairedPartImage;

//    [Header("Dog Animation")]
//    public DogAnimationController dogController; // ✅ Drag DogIdle here

//    public bool isRepaired = false;
//    private Color originalColor;

//    void Awake()
//    {
//        if (repairedPartImage != null)
//        {
//            originalColor = repairedPartImage.color;
//            originalColor.a = 1f;
//            repairedPartImage.color = new Color(
//                originalColor.r,
//                originalColor.g,
//                originalColor.b,
//                0f
//            );
//        }
//    }

//    void Start()
//    {
//        isRepaired = false;
//    }

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

//    public void Repair(RepairDrag drag)
//    {
//        isRepaired = true;

//        if (repairedPartImage != null)
//        {
//            repairedPartImage.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
//            repairedPartImage.transform.localScale = Vector3.one * 0.5f;
//            repairedPartImage.transform
//                .DOScale(1f, 0.4f)
//                .SetEase(Ease.OutBack);
//        }

//        drag.gameObject.SetActive(false);
//        Debug.Log($"[RepairSlot] SUCCESS - {requiredPartID}!");

//        if (GameManager.Instance != null)
//            GameManager.Instance.OnSlotRepaired();
//    }

//    public void WrongDrop()
//    {
//        // ✅ Shake slot
//        transform.DOKill();
//        transform.DOShakePosition(0.5f, 20f, 15)
//            .SetEase(Ease.OutElastic);

//        // ✅ Play wrong dog animation
//        if (dogController != null)
//            dogController.PlayWrong();

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
////DogIdle GameObject:
////  ✅ Image component
////  ✅ DogAnimationController script
////     Idle Frames → drag all idle frames
////     Wrong Frames → drag all wrong frames
////     Idle FPS → 12
////     Wrong FPS → 12

////Each RepairSlot:
////  ✅ Dog Controller → drag DogIdle here
////```

////## How It Works
////```
////Game starts        → idle loops forever ✅
////Wrong part dropped → idle STOPS ✅
////                   → wrong animation plays once ✅
////Wrong anim ends    → idle resumes automatically ✅
////Correct drop       → idle keeps playing ✅
///

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RepairSlot : MonoBehaviour
{
    [Header("Slot Identity")]
    public string requiredPartID;

    [Header("Slot Visuals")]
    public Image repairedPartImage;

    [Header("Dog Animation")]
    public DogAnimationController dogController;

    public bool isRepaired = false;
    private Color originalColor;

    void Awake()
    {
        if (repairedPartImage != null)
        {
            originalColor = repairedPartImage.color;
            originalColor.a = 1f;
            repairedPartImage.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0f
            );
        }
    }

    void Start()
    {
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
            repairedPartImage.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
            repairedPartImage.transform.localScale = Vector3.one * 0.5f;
            repairedPartImage.transform
                .DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack);
        }

        drag.gameObject.SetActive(false);
        Debug.Log($"[RepairSlot] SUCCESS - {requiredPartID}!");

        if (GameManager.Instance != null)
            GameManager.Instance.OnSlotRepaired();
    }

    public void WrongDrop()
    {
        transform.DOKill();
        transform.DOShakePosition(0.5f, 20f, 15)
            .SetEase(Ease.OutElastic);

        // ✅ Play wrong dog animation
        if (dogController != null)
            dogController.PlayWrong();

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