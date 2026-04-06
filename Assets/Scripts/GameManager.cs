////using DG.Tweening;
////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;
////using UnityEngine.SceneManagement;
////using UnityEngine.UI;
////using TMPro;

////public class GameManager : MonoBehaviour
////{
////    public static GameManager Instance;

////    public List<GameObject> allPanel;
////    private Stack<GameObject> panelHistory = new Stack<GameObject>();

////    // ========================
////    // PANELS
////    // ========================
////    [Header("Congratulation Panel")]
////    public GameObject congratsPanel;
////    public GameObject congratsPulsingObject;

////    [Header("Game Over Panel")]
////    public GameObject gameOverPanel;
////    public GameObject gameOverPulsingObject;

////    [Header("Dog Animation")]
////    public DogAnimationController dogController;

////    // ========================
////    // PAINTING GAME
////    // ========================
////    [Header("Painting Game - Drop Areas")]
////    public List<ColorDropArea> allDropAreas = new List<ColorDropArea>();
////    private int coloredCount = 0;

////    // ========================
////    // REPAIR GAME
////    // ========================
////    [Header("Repair Game - Repair Slots")]
////    public List<RepairSlot> allRepairSlots = new List<RepairSlot>();
////    private int repairedCount = 0;

////    // ========================
////    // RUNNER GAME
////    // ========================
////    [Header("Runner Game")]
////    public Cowanimationcontroller cow;
////    public GameObject startButton;
////    public GameObject instruction;
////    public GameObject startButtonPulsingObject;
////    public TMP_Text countdownText;

////    private bool gameOverShown = false;

////    // ========================
////    // CLEANING PIGPEN GAME
////    // ========================
////    [Header("Cleaning Pigpen Game")]
////    [Tooltip("Drag all DraggableTool toolbar icons here (Broom, Hammer, Brush, Shovel).")]
////    public List<PigPenTool> allTools = new List<PigPenTool>();

////    [Tooltip("The all-clean panel shown when every zone is finished. " +
////             "Assign the same GameObject set in PigPenZone → Completion Panel.")]
////    public GameObject allCleanPanel;

////    [Tooltip("Delay in seconds between the all-clean panel appearing and the congratulations panel appearing.")]
////    public float congratsDelay = 1f;

////    // ──────────────────────────────────────────────────────────
////    void Awake()
////    {
////        Instance = this;
////    }

////    void Start()
////    {
////        if (congratsPanel != null) congratsPanel.SetActive(false);
////        if (gameOverPanel != null) gameOverPanel.SetActive(false);
////        if (allCleanPanel != null) allCleanPanel.SetActive(false);

////        gameOverShown = false;
////        coloredCount = 0;
////        repairedCount = 0;

////        if (startButtonPulsingObject != null)
////            startButtonPulsingObject.SetActive(true);
////    }

////    // ========================
////    // PAINTING GAME
////    // ========================
////    public void OnZoneColored()
////    {
////        coloredCount++;
////        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
////            ShowCongrats();
////    }

////    // ========================
////    // REPAIR GAME
////    // ========================
////    public void OnSlotRepaired()
////    {
////        repairedCount++;
////        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
////            ShowCongrats();
////    }

////    // ========================
////    // RUNNER GAME
////    // ========================
////    public void StartRunnerGame()
////    {
////        if (startButton != null) startButton.SetActive(false);
////        if (instruction != null) instruction.SetActive(false);
////        StartCoroutine(CountdownRoutine());
////    }

////    IEnumerator CountdownRoutine()
////    {
////        countdownText.gameObject.SetActive(true);
////        countdownText.text = "3"; yield return new WaitForSeconds(1f);
////        countdownText.text = "2"; yield return new WaitForSeconds(1f);
////        countdownText.text = "1"; yield return new WaitForSeconds(1f);
////        countdownText.text = "GO!!"; yield return new WaitForSeconds(1f);
////        countdownText.gameObject.SetActive(false);
////        cow.StartRunning();
////    }

////    public void ShowGameOver()
////    {
////        if (gameOverShown) return;
////        gameOverShown = true;

////        if (gameOverPanel == null) return;
////        AnimatePanelFade(gameOverPanel);

////        if (gameOverPulsingObject != null)
////        {
////            DOVirtual.DelayedCall(0.5f, () =>
////            {
////                if (gameOverPulsingObject == null) return;
////                gameOverPulsingObject.transform.DOKill();
////                gameOverPulsingObject.transform.localScale = Vector3.one;
////                gameOverPulsingObject.transform
////                    .DOScale(1.15f, 0.6f)
////                    .SetEase(Ease.InOutSine)
////                    .SetLoops(-1, LoopType.Yoyo);
////            });
////        }
////    }

////    public void RestartGame()
////    {
////        DOTween.KillAll();
////        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
////    }

////    public void ShowRunnerCongrats()
////    {
////        ShowCongrats();
////    }

////    // ========================
////    // CLEANING PIGPEN GAME
////    // ========================

////    /// <summary>
////    /// Resets all zones and tools. Wire to "Play Again" on the congratsPanel.
////    /// Also re-triggers the intro sequence on the first zone that has intro assets.
////    /// </summary>
////    public void ResetCleaningGame()
////    {
////        StopAllCoroutines();

////        // Reset all zones (clears sprites, star, pig, intro UI, chat box)
////        foreach (PigPenZone zone in PigPenZone.AllZones)
////            if (zone != null) zone.ResetZone();

////        // Reset all tool icons
////        foreach (var tool in allTools)
////            if (tool != null) tool.ResetTool();

////        // Hide panels (allCleanPanel may already be hidden by ShowCongratsAfterDelay)
////        if (allCleanPanel != null) { allCleanPanel.SetActive(false); allCleanPanel.transform.localScale = Vector3.one; }
////        if (congratsPanel != null) congratsPanel.SetActive(false);

////        // Re-trigger the intro on the first zone that has a pig assigned
////        foreach (PigPenZone zone in PigPenZone.AllZones)
////        {
////            if (zone != null && zone.pigObject != null)
////            {
////                zone.StartIntro();
////                break;
////            }
////        }

////        Debug.Log("[GameManager] Cleaning game reset.");
////    }

////    /// <summary>
////    /// Called by PigPenZone after the all-clean panel zoom-in begins.
////    /// Waits congratsDelay seconds (enough for the zoom to finish + a pause),
////    /// then shows the congratulations panel.
////    /// </summary>
////    public void ShowPigPenCongrats()
////    {
////        StartCoroutine(ShowCongratsAfterDelay());
////    }

////    IEnumerator ShowCongratsAfterDelay()
////    {
////        // Give the AllClean panel zoom-in (0.45 s) time to land, then wait the inspector delay
////        float totalWait = Mathf.Max(0.5f, congratsDelay);
////        yield return new WaitForSeconds(totalWait);

////        // Hide the all-clean panel before the congrats panel appears
////        if (allCleanPanel != null)
////        {
////            allCleanPanel.transform
////                .DOScale(0f, 0.2f)
////                .SetEase(Ease.InBack)
////                .OnComplete(() => allCleanPanel.SetActive(false));
////            yield return new WaitForSeconds(0.25f);
////        }

////        ShowCongrats();
////    }

////    // ========================
////    // SHARED CONGRATS LOGIC
////    // ========================
////    void ShowCongrats()
////    {
////        if (congratsPanel != null)
////        {
////            AnimatePanelFade(congratsPanel);
////            panelHistory.Push(congratsPanel);
////        }

////        if (dogController != null) dogController.PlayCongrats();

////        if (congratsPulsingObject != null)
////        {
////            DOVirtual.DelayedCall(0.5f, () =>
////            {
////                if (congratsPulsingObject == null) return;
////                congratsPulsingObject.transform.DOKill();
////                congratsPulsingObject.transform.localScale = Vector3.one;
////                congratsPulsingObject.transform
////                    .DOScale(1.15f, 0.8f)
////                    .SetEase(Ease.InOutSine)
////                    .SetLoops(-1, LoopType.Yoyo);
////            });
////        }
////    }

////    // ========================
////    // PANEL MANAGEMENT
////    // ========================
////    public void ShowPanel(GameObject panelToShow)
////    {
////        foreach (GameObject panel in allPanel)
////            panel.SetActive(false);

////        if (panelToShow != null)
////        {
////            AnimatePanelFade(panelToShow);
////            panelHistory.Push(panelToShow);
////        }
////    }

////    public void AnimatePanelFade(GameObject panel)
////    {
////        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
////        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

////        panel.transform.localScale = Vector3.one * 0.4f;
////        cg.alpha = 0f;
////        panel.SetActive(true);

////        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
////        cg.DOFade(1f, 0.3f);
////    }

////    public void CloseCurrectPanel()
////    {
////        if (panelHistory.Count == 0) return;

////        GameObject closePanel = panelHistory.Pop();

////        if (closePanel == congratsPanel)
////        {
////            if (dogController != null) dogController.PlayIdle();
////            if (congratsPulsingObject != null)
////            {
////                congratsPulsingObject.transform.DOKill();
////                congratsPulsingObject.transform.localScale = Vector3.one;
////            }
////        }

////        closePanel.transform
////            .DOScale(Vector3.zero, 0.25f)
////            .SetEase(Ease.InBack)
////            .OnComplete(() => closePanel.SetActive(false));

////        if (panelHistory.Count > 0)
////            AnimatePanelFade(panelHistory.Peek());
////    }

////    // ========================
////    // RESET
////    // ========================
////    public void ResetPaintingGame()
////    {
////        coloredCount = 0;
////        foreach (var area in allDropAreas) area.ResetZone();
////        if (congratsPanel != null) congratsPanel.SetActive(false);
////        if (dogController != null) dogController.PlayIdle();
////    }

////    public void ResetRepairGame()
////    {
////        repairedCount = 0;
////        foreach (var slot in allRepairSlots) slot.ResetSlot();
////        if (congratsPanel != null) congratsPanel.SetActive(false);
////        if (dogController != null) dogController.PlayIdle();
////    }

////    public void LoadScene(int sceneId) => SceneManager.LoadScene(sceneId);
////}

//using DG.Tweening;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using TMPro;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;

//    public List<GameObject> allPanel;
//    private Stack<GameObject> panelHistory = new Stack<GameObject>();

//    // ========================
//    // PANELS
//    // ========================
//    [Header("Congratulation Panel")]
//    public GameObject congratsPanel;
//    public GameObject congratsPulsingObject;

//    [Header("Game Over Panel")]
//    public GameObject gameOverPanel;
//    public GameObject gameOverPulsingObject;

//    [Header("Dog Animation")]
//    public DogAnimationController dogController;

//    // ========================
//    // PAINTING GAME
//    // ========================
//    [Header("Painting Game - Drop Areas")]
//    public List<ColorDropArea> allDropAreas = new List<ColorDropArea>();
//    private int coloredCount = 0;

//    // ========================
//    // REPAIR GAME
//    // ========================
//    [Header("Repair Game - Repair Slots")]
//    public List<RepairSlot> allRepairSlots = new List<RepairSlot>();
//    private int repairedCount = 0;

//    // ========================
//    // RUNNER GAME
//    // ========================
//    [Header("Runner Game")]
//    public Cowanimationcontroller cow;
//    public GameObject startButton;
//    public GameObject instruction;
//    public GameObject startButtonPulsingObject;
//    public TMP_Text countdownText;

//    private bool gameOverShown = false;

//    // ========================
//    // CLEANING PIGPEN GAME
//    // ========================
//    [Header("Cleaning Pigpen Game")]
//    [Tooltip("Drag all DraggableTool toolbar icons here (Broom, Hammer, Brush, Shovel).")]
//    public List<PigPenTool> allTools = new List<PigPenTool>();

//    // ──────────────────────────────────────────────────────────
//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        if (congratsPanel != null) congratsPanel.SetActive(false);
//        if (gameOverPanel != null) gameOverPanel.SetActive(false);

//        gameOverShown = false;
//        coloredCount = 0;
//        repairedCount = 0;

//        if (startButtonPulsingObject != null)
//            startButtonPulsingObject.SetActive(true);
//    }

//    // ========================
//    // PAINTING GAME
//    // ========================
//    public void OnZoneColored()
//    {
//        coloredCount++;
//        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
//            ShowCongrats();
//    }

//    // ========================
//    // REPAIR GAME
//    // ========================
//    public void OnSlotRepaired()
//    {
//        repairedCount++;
//        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
//            ShowCongrats();
//    }

//    // ========================
//    // RUNNER GAME
//    // ========================
//    public void StartRunnerGame()
//    {
//        if (startButton != null) startButton.SetActive(false);
//        if (instruction != null) instruction.SetActive(false);
//        StartCoroutine(CountdownRoutine());
//    }

//    IEnumerator CountdownRoutine()
//    {
//        countdownText.gameObject.SetActive(true);
//        countdownText.text = "3"; yield return new WaitForSeconds(1f);
//        countdownText.text = "2"; yield return new WaitForSeconds(1f);
//        countdownText.text = "1"; yield return new WaitForSeconds(1f);
//        countdownText.text = "GO!!"; yield return new WaitForSeconds(1f);
//        countdownText.gameObject.SetActive(false);
//        cow.StartRunning();
//    }

//    public void ShowGameOver()
//    {
//        if (gameOverShown) return;
//        gameOverShown = true;

//        if (gameOverPanel == null) return;
//        AnimatePanelFade(gameOverPanel);

//        if (gameOverPulsingObject != null)
//        {
//            DOVirtual.DelayedCall(0.5f, () =>
//            {
//                if (gameOverPulsingObject == null) return;
//                gameOverPulsingObject.transform.DOKill();
//                gameOverPulsingObject.transform.localScale = Vector3.one;
//                gameOverPulsingObject.transform
//                    .DOScale(1.15f, 0.6f)
//                    .SetEase(Ease.InOutSine)
//                    .SetLoops(-1, LoopType.Yoyo);
//            });
//        }
//    }

//    public void RestartGame()
//    {
//        DOTween.KillAll();
//        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//    }

//    public void ShowRunnerCongrats()
//    {
//        ShowCongrats();
//    }

//    // ========================
//    // CLEANING PIGPEN GAME
//    // ========================

//    /// <summary>
//    /// Resets all zones and tools. Wire to "Play Again" on the congratsPanel.
//    /// Also re-triggers the intro sequence on the first zone that has intro assets.
//    /// </summary>
//    public void ResetCleaningGame()
//    {
//        StopAllCoroutines();

//        // Reset all zones (clears sprites, star, pig, intro UI, chat box)
//        foreach (PigPenZone zone in PigPenZone.AllZones)
//            if (zone != null) zone.ResetZone();

//        // Reset all tool icons
//        foreach (var tool in allTools)
//            if (tool != null) tool.ResetTool();

//        // Hide panels
//        if (congratsPanel != null) congratsPanel.SetActive(false);

//        // Re-trigger the intro on the first zone that has a pig assigned
//        foreach (PigPenZone zone in PigPenZone.AllZones)
//        {
//            if (zone != null && zone.pigObject != null)
//            {
//                zone.StartIntro();
//                break;
//            }
//        }

//        Debug.Log("[GameManager] Cleaning game reset.");
//    }

//    /// <summary>
//    /// Called by PigPenZone when all zones are cleaned.
//    /// </summary>
//    public void ShowPigPenCongrats()
//    {
//        ShowCongrats();
//    }

//    // ========================
//    // SHARED CONGRATS LOGIC
//    // ========================
//    void ShowCongrats()
//    {
//        if (congratsPanel != null)
//        {
//            AnimatePanelFade(congratsPanel);
//            panelHistory.Push(congratsPanel);
//        }

//        if (dogController != null) dogController.PlayCongrats();

//        if (congratsPulsingObject != null)
//        {
//            DOVirtual.DelayedCall(0.5f, () =>
//            {
//                if (congratsPulsingObject == null) return;
//                congratsPulsingObject.transform.DOKill();
//                congratsPulsingObject.transform.localScale = Vector3.one;
//                congratsPulsingObject.transform
//                    .DOScale(1.15f, 0.8f)
//                    .SetEase(Ease.InOutSine)
//                    .SetLoops(-1, LoopType.Yoyo);
//            });
//        }
//    }

//    // ========================
//    // PANEL MANAGEMENT
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
//        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
//        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

//        panel.transform.localScale = Vector3.one * 0.4f;
//        cg.alpha = 0f;
//        panel.SetActive(true);

//        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
//        cg.DOFade(1f, 0.3f);
//    }

//    public void CloseCurrectPanel()
//    {
//        if (panelHistory.Count == 0) return;

//        GameObject closePanel = panelHistory.Pop();

//        if (closePanel == congratsPanel)
//        {
//            if (dogController != null) dogController.PlayIdle();
//            if (congratsPulsingObject != null)
//            {
//                congratsPulsingObject.transform.DOKill();
//                congratsPulsingObject.transform.localScale = Vector3.one;
//            }
//        }

//        closePanel.transform
//            .DOScale(Vector3.zero, 0.25f)
//            .SetEase(Ease.InBack)
//            .OnComplete(() => closePanel.SetActive(false));

//        if (panelHistory.Count > 0)
//            AnimatePanelFade(panelHistory.Peek());
//    }

//    // ========================
//    // RESET
//    // ========================
//    public void ResetPaintingGame()
//    {
//        coloredCount = 0;
//        foreach (var area in allDropAreas) area.ResetZone();
//        if (congratsPanel != null) congratsPanel.SetActive(false);
//        if (dogController != null) dogController.PlayIdle();
//    }

//    public void ResetRepairGame()
//    {
//        repairedCount = 0;
//        foreach (var slot in allRepairSlots) slot.ResetSlot();
//        if (congratsPanel != null) congratsPanel.SetActive(false);
//        if (dogController != null) dogController.PlayIdle();
//    }

//    public void LoadScene(int sceneId) => SceneManager.LoadScene(sceneId);
//}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<GameObject> allPanel;
    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    // ========================
    // PANELS
    // ========================
    [Header("Congratulation Panel")]
    public GameObject congratsPanel;
    public GameObject congratsPulsingObject;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public GameObject gameOverPulsingObject;

    [Header("Dog Animation")]
    public DogAnimationController dogController;

    // ========================
    // PAINTING GAME
    // ========================
    [Header("Painting Game - Drop Areas")]
    public List<ColorDropArea> allDropAreas = new List<ColorDropArea>();
    private int coloredCount = 0;

    // ========================
    // REPAIR GAME
    // ========================
    [Header("Repair Game - Repair Slots")]
    public List<RepairSlot> allRepairSlots = new List<RepairSlot>();
    private int repairedCount = 0;

    // ========================
    // RUNNER GAME
    // ========================
    [Header("Runner Game")]
    public Cowanimationcontroller cow;
    public GameObject startButton;
    public GameObject instruction;
    public GameObject startButtonPulsingObject;
    public TMP_Text countdownText;

    private bool gameOverShown = false;

    // ========================
    // CLEANING PIGPEN GAME
    // ========================

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (congratsPanel != null) congratsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        gameOverShown = false;
        coloredCount = 0;
        repairedCount = 0;

        if (startButtonPulsingObject != null)
            startButtonPulsingObject.SetActive(true);
    }

    // ========================
    // PAINTING GAME
    // ========================
    public void OnZoneColored()
    {
        coloredCount++;
        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
            ShowCongrats();
    }

    // ========================
    // REPAIR GAME
    // ========================
    public void OnSlotRepaired()
    {
        repairedCount++;
        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
            ShowCongrats();
    }

    // ========================
    // RUNNER GAME
    // ========================
    public void StartRunnerGame()
    {
        if (startButton != null) startButton.SetActive(false);
        if (instruction != null) instruction.SetActive(false);
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = "3"; yield return new WaitForSeconds(1f);
        countdownText.text = "2"; yield return new WaitForSeconds(1f);
        countdownText.text = "1"; yield return new WaitForSeconds(1f);
        countdownText.text = "GO!!"; yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
        cow.StartRunning();
    }

    public void ShowGameOver()
    {
        if (gameOverShown) return;
        gameOverShown = true;

        if (gameOverPanel == null) return;
        AnimatePanelFade(gameOverPanel);

        if (gameOverPulsingObject != null)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (gameOverPulsingObject == null) return;
                gameOverPulsingObject.transform.DOKill();
                gameOverPulsingObject.transform.localScale = Vector3.one;
                gameOverPulsingObject.transform
                    .DOScale(1.15f, 0.6f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            });
        }
    }

    public void RestartGame()
    {
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowRunnerCongrats()
    {
        ShowCongrats();
    }

    // ========================
    // CLEANING PIGPEN GAME
    // ========================

    /// <summary>
    /// Resets all zones and tools. Wire to "Play Again" on the congratsPanel.
    /// Also re-triggers the intro sequence on the first zone that has intro assets.
    /// </summary>
    public void ResetCleaningGame()
    {
        StopAllCoroutines();

        // Reset all zones (clears sprites, star, pig, intro UI, chat box)
        foreach (PigPenZone zone in PigPenZone.AllZones)
            if (zone != null) zone.ResetZone();

        // Hide panels
        if (congratsPanel != null) congratsPanel.SetActive(false);

        // Re-trigger the intro on the first zone that has a pig assigned
        foreach (PigPenZone zone in PigPenZone.AllZones)
        {
            if (zone != null && zone.pigObject != null)
            {
                zone.StartIntro();
                break;
            }
        }

        Debug.Log("[GameManager] Cleaning game reset.");
    }

    /// <summary>
    /// Called by PigPenZone when all zones are cleaned.
    /// </summary>
    public void ShowPigPenCongrats()
    {
        ShowCongrats();
    }

    // ========================
    // SHARED CONGRATS LOGIC
    // ========================
    void ShowCongrats()
    {
        if (congratsPanel != null)
        {
            AnimatePanelFade(congratsPanel);
            panelHistory.Push(congratsPanel);
        }

        if (dogController != null) dogController.PlayCongrats();

        if (congratsPulsingObject != null)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (congratsPulsingObject == null) return;
                congratsPulsingObject.transform.DOKill();
                congratsPulsingObject.transform.localScale = Vector3.one;
                congratsPulsingObject.transform
                    .DOScale(1.15f, 0.8f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            });
        }
    }

    // ========================
    // PANEL MANAGEMENT
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
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        panel.transform.localScale = Vector3.one * 0.4f;
        cg.alpha = 0f;
        panel.SetActive(true);

        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        cg.DOFade(1f, 0.3f);
    }

    public void CloseCurrectPanel()
    {
        if (panelHistory.Count == 0) return;

        GameObject closePanel = panelHistory.Pop();

        if (closePanel == congratsPanel)
        {
            if (dogController != null) dogController.PlayIdle();
            if (congratsPulsingObject != null)
            {
                congratsPulsingObject.transform.DOKill();
                congratsPulsingObject.transform.localScale = Vector3.one;
            }
        }

        closePanel.transform
            .DOScale(Vector3.zero, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() => closePanel.SetActive(false));

        if (panelHistory.Count > 0)
            AnimatePanelFade(panelHistory.Peek());
    }

    // ========================
    // RESET
    // ========================
    public void ResetPaintingGame()
    {
        coloredCount = 0;
        foreach (var area in allDropAreas) area.ResetZone();
        if (congratsPanel != null) congratsPanel.SetActive(false);
        if (dogController != null) dogController.PlayIdle();
    }

    public void ResetRepairGame()
    {
        repairedCount = 0;
        foreach (var slot in allRepairSlots) slot.ResetSlot();
        if (congratsPanel != null) congratsPanel.SetActive(false);
        if (dogController != null) dogController.PlayIdle();
    }

    public void LoadScene(int sceneId) => SceneManager.LoadScene(sceneId);
}