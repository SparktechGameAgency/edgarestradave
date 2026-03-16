//using DG.Tweening;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;

//    public List<GameObject> allPanel;
//    private Stack<GameObject> panelHistory = new Stack<GameObject>();

//    [Header("Congratulation Panel")]
//    public GameObject congratsPanel;

//    [Header("Pulsing GameObject")]
//    public GameObject pulsingObject;

//    // ========================
//    // ✅ PAINTING GAME
//    // ========================
//    [Header("Painting Game - Drop Areas")]
//    public List<ColorDropArea> allDropAreas = new List<ColorDropArea>();
//    private int coloredCount = 0;

//    // ========================
//    // ✅ REPAIR GAME
//    // ========================
//    [Header("Repair Game - Repair Slots")]
//    public List<RepairSlot> allRepairSlots = new List<RepairSlot>();
//    private int repairedCount = 0;

//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        if (congratsPanel != null)
//            congratsPanel.SetActive(false);

//        if (pulsingObject != null)
//            pulsingObject.SetActive(false);

//        coloredCount = 0;
//        repairedCount = 0;

//        Debug.Log($"[GameManager] Painting areas: {allDropAreas.Count}");
//        Debug.Log($"[GameManager] Repair slots: {allRepairSlots.Count}");
//    }

//    // ========================
//    // ✅ PAINTING GAME
//    // ========================

//    public void OnZoneColored()
//    {
//        coloredCount++;
//        Debug.Log($"[GameManager] Painting: {coloredCount}/{allDropAreas.Count}");

//        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
//        {
//            Debug.Log("[GameManager] ALL PAINTED! Showing Congrats!");
//            ShowCongrats();
//        }
//    }

//    // ========================
//    // ✅ REPAIR GAME
//    // ========================

//    public void OnSlotRepaired()
//    {
//        repairedCount++;
//        Debug.Log($"[GameManager] Repaired: {repairedCount}/{allRepairSlots.Count}");

//        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
//        {
//            Debug.Log("[GameManager] ALL REPAIRED! Showing Congrats!");
//            ShowCongrats();
//        }
//    }

//    // ========================
//    // ✅ CONGRATS
//    // ========================

//    void ShowCongrats()
//    {
//        if (congratsPanel != null)
//        {
//            AnimatePanelFade(congratsPanel);
//            panelHistory.Push(congratsPanel);
//        }
//        else
//        {
//            Debug.LogError("[GameManager] congratsPanel is not assigned!");
//            return;
//        }

//        if (pulsingObject != null)
//        {
//            pulsingObject.SetActive(true);
//            pulsingObject.transform.DOKill();
//            pulsingObject.transform.localScale = Vector3.one;

//            pulsingObject.transform
//                .DOScale(1.15f, 0.8f)
//                .SetEase(Ease.InOutSine)
//                .SetLoops(-1, LoopType.Yoyo)
//                .SetDelay(0.5f);

//            // ✅ Stop pulsing after 3 seconds
//            DOVirtual.DelayedCall(3.5f, () =>
//            {
//                pulsingObject.transform.DOKill();
//                pulsingObject.transform
//                    .DOScale(Vector3.one, 0.3f)
//                    .SetEase(Ease.OutBack);
//            });
//        }
//    }

//    // ========================
//    // ✅ PANEL MANAGEMENT
//    // ========================

//    public void ShowPanel(GameObject panelToShow)
//    {
//        foreach (GameObject panel in allPanel)
//            panel.SetActive(false);

//        if (panelToShow != null)
//        {
//            AnimatePanelFade(panelToShow);
//            panelHistory.Push(panelToShow);
//        }
//    }

//    public void AnimatePanelFade(GameObject panel)
//    {
//        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
//        if (canvasGroup == null)
//            canvasGroup = panel.AddComponent<CanvasGroup>();

//        panel.transform.localScale = Vector3.one * 0.4f;
//        canvasGroup.alpha = 0f;
//        panel.SetActive(true);

//        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
//        canvasGroup.DOFade(1f, 0.3f);
//    }

//    public void CloseCurrectPanel()
//    {
//        if (panelHistory.Count > 0)
//        {
//            GameObject closePanel = panelHistory.Pop();

//            if (closePanel == congratsPanel && pulsingObject != null)
//            {
//                pulsingObject.transform.DOKill();
//                pulsingObject.transform.localScale = Vector3.one;
//                pulsingObject.SetActive(false);
//            }

//            closePanel.transform
//                .DOScale(Vector3.zero, 0.25f)
//                .SetEase(Ease.InBack)
//                .OnComplete(() => closePanel.SetActive(false));

//            if (panelHistory.Count > 0)
//            {
//                GameObject previousPanel = panelHistory.Peek();
//                AnimatePanelFade(previousPanel);
//            }
//        }
//    }

//    // ========================
//    // ✅ RESET
//    // ========================

//    public void ResetPaintingGame()
//    {
//        coloredCount = 0;

//        foreach (ColorDropArea area in allDropAreas)
//            area.ResetZone();

//        if (congratsPanel != null)
//            congratsPanel.SetActive(false);

//        if (pulsingObject != null)
//        {
//            pulsingObject.transform.DOKill();
//            pulsingObject.SetActive(false);
//        }
//    }

//    public void ResetRepairGame()
//    {
//        repairedCount = 0;

//        foreach (RepairSlot slot in allRepairSlots)
//            slot.ResetSlot();

//        if (congratsPanel != null)
//            congratsPanel.SetActive(false);

//        if (pulsingObject != null)
//        {
//            pulsingObject.transform.DOKill();
//            pulsingObject.SetActive(false);
//        }
//    }

//    // ========================
//    // ✅ SCENE MANAGEMENT
//    // ========================

//    public void LoadScene(int sceneId)
//    {
//        SceneManager.LoadScene(sceneId);
//    }
//}
////```

////## What Changed
////```
////✅ Removed: using Unity.Collections  ← was causing warning
////✅ Removed: all commented out code   ← cleaner file
////✅ Kept everything else exactly same
////```

////## Inspector — Repair Scene
////```
////GameManager:
////  All Repair Slots (6):
////    Element 0 → exhaust
////    Element 1 → body
////    Element 2 → engine
////    Element 3 → front1
////    Element 4 → front2
////    Element 5 → backwheel
////  Congrats Panel → CongratsPanel
////  Pulsing Object → any GameObject
///

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<GameObject> allPanel;
    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    [Header("Congratulation Panel")]
    public GameObject congratsPanel;

    [Header("Pulsing GameObject")]
    public GameObject pulsingObject;

    [Header("Dog Animation")]
    public DogAnimationController dogController;

    // ========================
    // ✅ PAINTING GAME
    // ========================
    [Header("Painting Game - Drop Areas")]
    public List<ColorDropArea> allDropAreas = new List<ColorDropArea>();
    private int coloredCount = 0;

    // ========================
    // ✅ REPAIR GAME
    // ========================
    [Header("Repair Game - Repair Slots")]
    public List<RepairSlot> allRepairSlots = new List<RepairSlot>();
    private int repairedCount = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (congratsPanel != null)
            congratsPanel.SetActive(false);

        if (pulsingObject != null)
            pulsingObject.SetActive(false);

        coloredCount = 0;
        repairedCount = 0;

        Debug.Log($"[GameManager] Painting areas: {allDropAreas.Count}");
        Debug.Log($"[GameManager] Repair slots: {allRepairSlots.Count}");
    }

    // ========================
    // ✅ PAINTING GAME
    // ========================

    public void OnZoneColored()
    {
        coloredCount++;
        Debug.Log($"[GameManager] Painting: {coloredCount}/{allDropAreas.Count}");

        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
        {
            Debug.Log("[GameManager] ALL PAINTED!");
            ShowCongrats();
        }
    }

    // ========================
    // ✅ REPAIR GAME
    // ========================

    public void OnSlotRepaired()
    {
        repairedCount++;
        Debug.Log($"[GameManager] Repaired: {repairedCount}/{allRepairSlots.Count}");

        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
        {
            Debug.Log("[GameManager] ALL REPAIRED!");
            ShowCongrats();
        }
    }

    // ========================
    // ✅ CONGRATS
    // ========================

    //void ShowCongrats()
    //{
    //    if (congratsPanel != null)
    //    {
    //        AnimatePanelFade(congratsPanel);
    //        panelHistory.Push(congratsPanel);
    //    }
    //    else
    //    {
    //        Debug.LogError("[GameManager] congratsPanel not assigned!");
    //        return;
    //    }

    //    // ✅ Stop idle - play congrats animation
    //    if (dogController != null)
    //        dogController.PlayCongrats();

    //    if (pulsingObject != null)
    //    {
    //        pulsingObject.SetActive(true);
    //        pulsingObject.transform.DOKill();
    //        pulsingObject.transform.localScale = Vector3.one;

    //        pulsingObject.transform
    //            .DOScale(1.15f, 0.8f)
    //            .SetEase(Ease.InOutSine)
    //            .SetLoops(-1, LoopType.Yoyo)
    //            .SetDelay(0.5f);

    //        DOVirtual.DelayedCall(3.5f, () =>
    //        {
    //            pulsingObject.transform.DOKill();
    //            pulsingObject.transform
    //                .DOScale(Vector3.one, 0.3f)
    //                .SetEase(Ease.OutBack);
    //        });
    //    }
    //}

    void ShowCongrats()
    {
        Debug.Log("[GameManager] ShowCongrats called!");

        // ✅ Show panel FIRST - no early return!
        if (congratsPanel != null)
        {
            AnimatePanelFade(congratsPanel);
            panelHistory.Push(congratsPanel);
        }
        else
        {
            Debug.LogError("[GameManager] congratsPanel not assigned!");
        }

        // ✅ Dog - only if assigned
        if (dogController != null)
        {
            Debug.Log("[GameManager] Calling PlayCongrats!");
            dogController.PlayCongrats();
        }
        else
        {
            Debug.LogError("[GameManager] dogController IS NULL!");
        }

        // ✅ Pulsing object
        if (pulsingObject != null)
        {
            pulsingObject.SetActive(true);
            pulsingObject.transform.DOKill();
            pulsingObject.transform.localScale = Vector3.one;

            pulsingObject.transform
                .DOScale(1.15f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(0.5f);

            DOVirtual.DelayedCall(3.5f, () =>
            {
                pulsingObject.transform.DOKill();
                pulsingObject.transform
                    .DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack);
            });
        }
    }

    // ========================
    // ✅ PANEL MANAGEMENT
    // ========================

    public void ShowPanel(GameObject panelToShow)
    {
        foreach (GameObject panel in allPanel)
            panel.SetActive(false);

        if (panelToShow != null)
        {
            AnimatePanelFade(panelToShow);
            panelHistory.Push(panelToShow);
        }
    }

    public void AnimatePanelFade(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        panel.transform.localScale = Vector3.one * 0.4f;
        canvasGroup.alpha = 0f;
        panel.SetActive(true);

        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, 0.3f);
    }

    public void CloseCurrectPanel()
    {
        if (panelHistory.Count > 0)
        {
            GameObject closePanel = panelHistory.Pop();

            if (closePanel == congratsPanel)
            {
                // ✅ Return to idle when congrats closes
                if (dogController != null)
                    dogController.PlayIdle();

                if (pulsingObject != null)
                {
                    pulsingObject.transform.DOKill();
                    pulsingObject.transform.localScale = Vector3.one;
                    pulsingObject.SetActive(false);
                }
            }

            closePanel.transform
                .DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() => closePanel.SetActive(false));

            if (panelHistory.Count > 0)
            {
                GameObject previousPanel = panelHistory.Peek();
                AnimatePanelFade(previousPanel);
            }
        }
    }

    // ========================
    // ✅ RESET
    // ========================

    public void ResetPaintingGame()
    {
        coloredCount = 0;
        foreach (ColorDropArea area in allDropAreas)
            area.ResetZone();

        if (congratsPanel != null)
            congratsPanel.SetActive(false);

        if (pulsingObject != null)
        {
            pulsingObject.transform.DOKill();
            pulsingObject.SetActive(false);
        }

        if (dogController != null)
            dogController.PlayIdle();
    }

    public void ResetRepairGame()
    {
        repairedCount = 0;
        foreach (RepairSlot slot in allRepairSlots)
            slot.ResetSlot();

        if (congratsPanel != null)
            congratsPanel.SetActive(false);

        if (pulsingObject != null)
        {
            pulsingObject.transform.DOKill();
            pulsingObject.SetActive(false);
        }

        if (dogController != null)
            dogController.PlayIdle();
    }

    // ========================
    // ✅ SCENE MANAGEMENT
    // ========================

    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
//```

//## Inspector Setup
//```
//GameManager:
//Dog Controller → drag DogIdle here ✅

//Each RepairSlot:
//  Dog Controller → drag DogIdle here ✅

//DogIdle → DogAnimationController:
//  Idle Frames    → drag idle frames ✅
//  Wrong Frames   → drag wrong frames ✅
//  Congrats Frames→ drag congrats frames ✅