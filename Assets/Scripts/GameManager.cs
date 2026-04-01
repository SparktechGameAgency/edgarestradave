//////using DG.Tweening;
//////using System.Collections;
//////using System.Collections.Generic;
//////using UnityEngine;
//////using UnityEngine.SceneManagement;
//////using UnityEngine.UI;
//////using TMPro;

//////public class GameManager : MonoBehaviour
//////{
//////    public static GameManager Instance;

//////    public List<GameObject> allPanel;
//////    private Stack<GameObject> panelHistory = new Stack<GameObject>();

//////    // ========================
//////    // PANELS
//////    // ========================
//////    [Header("Congratulation Panel")]
//////    public GameObject congratsPanel;
//////    [Tooltip("Drag the text or image inside the Congrats panel that should pulse.")]
//////    public GameObject congratsPulsingObject;

//////    [Header("Game Over Panel")]
//////    public GameObject gameOverPanel;
//////    [Tooltip("Drag the 'Game Over' text or icon inside the panel that should pulse.")]
//////    public GameObject gameOverPulsingObject;

//////    [Header("Dog Animation")]
//////    public DogAnimationController dogController;

//////    // ========================
//////    // PAINTING GAME
//////    // ========================
//////    [Header("Painting Game - Drop Areas")]
//////    public List<ColorDropArea> allDropAreas = new List<ColorDropArea>();
//////    private int coloredCount = 0;

//////    // ========================
//////    // REPAIR GAME
//////    // ========================
//////    [Header("Repair Game - Repair Slots")]
//////    public List<RepairSlot> allRepairSlots = new List<RepairSlot>();
//////    private int repairedCount = 0;

//////    // ========================
//////    // RUNNER GAME
//////    // ========================
//////    [Header("Runner Game")]
//////    public Cowanimationcontroller cow;
//////    [Tooltip("Drag the Start Button GameObject here.")]
//////    public GameObject startButton;
//////    public GameObject instruction;
//////    [Tooltip("Drag the text/image INSIDE the start button that should pulse.")]
//////    public GameObject startButtonPulsingObject;
//////    public TMP_Text countdownText;

//////    private bool gameOverShown = false;

//////    // ========================
//////    // CLEANING PIGPEN GAME
//////    // ========================
//////    [Header("Cleaning Pigpen Game - Drop Zones")]
//////    [Tooltip("Drag all CleaningDropZone GameObjects here (Waste, Frame, Grass, Mud).")]
//////    public List<CleaningDropZone> allCleaningZones = new List<CleaningDropZone>();

//////    [Tooltip("Drag all DraggableTool GameObjects here (Broom, Hammer, Shovel, Brush).")]
//////    public List<DraggableTool> allTools = new List<DraggableTool>();

//////    private int _cleaningTasksDone = 0;

//////    // ──────────────────────────────────────────────────────────
//////    void Awake()
//////    {
//////        Instance = this;
//////    }

//////    void Start()
//////    {
//////        if (congratsPanel != null) congratsPanel.SetActive(false);
//////        if (gameOverPanel != null) gameOverPanel.SetActive(false);

//////        gameOverShown = false;
//////        coloredCount = 0;
//////        repairedCount = 0;
//////        _cleaningTasksDone = 0;

//////        if (startButtonPulsingObject != null)
//////            startButtonPulsingObject.SetActive(true);
//////    }

//////    // ========================
//////    // PAINTING GAME
//////    // ========================
//////    public void OnZoneColored()
//////    {
//////        coloredCount++;
//////        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
//////            ShowCongrats();
//////    }

//////    // ========================
//////    // REPAIR GAME
//////    // ========================
//////    public void OnSlotRepaired()
//////    {
//////        repairedCount++;
//////        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
//////            ShowCongrats();
//////    }

//////    // ========================
//////    // RUNNER GAME
//////    // ========================
//////    public void StartRunnerGame()
//////    {
//////        if (startButton != null) startButton.SetActive(false);
//////        if (instruction != null) instruction.SetActive(false);
//////        StartCoroutine(CountdownRoutine());
//////    }

//////    IEnumerator CountdownRoutine()
//////    {
//////        countdownText.gameObject.SetActive(true);

//////        countdownText.text = "3"; yield return new WaitForSeconds(1f);
//////        countdownText.text = "2"; yield return new WaitForSeconds(1f);
//////        countdownText.text = "1"; yield return new WaitForSeconds(1f);
//////        countdownText.text = "GO!!"; yield return new WaitForSeconds(1f);

//////        countdownText.gameObject.SetActive(false);
//////        cow.StartRunning();
//////    }

//////    public void ShowGameOver()
//////    {
//////        if (gameOverShown) return;
//////        gameOverShown = true;

//////        Debug.Log("[GameManager] ☠️ Game Over!");

//////        if (gameOverPanel == null)
//////        {
//////            Debug.LogError("[GameManager] gameOverPanel is not assigned in the Inspector!");
//////            return;
//////        }

//////        AnimatePanelFade(gameOverPanel);

//////        if (gameOverPulsingObject != null)
//////        {
//////            DOVirtual.DelayedCall(0.5f, () =>
//////            {
//////                if (gameOverPulsingObject == null) return;
//////                gameOverPulsingObject.transform.DOKill();
//////                gameOverPulsingObject.transform.localScale = Vector3.one;
//////                gameOverPulsingObject.transform
//////                    .DOScale(1.15f, 0.6f)
//////                    .SetEase(Ease.InOutSine)
//////                    .SetLoops(-1, LoopType.Yoyo);
//////            });
//////        }
//////    }

//////    public void RestartGame()
//////    {
//////        DOTween.KillAll();
//////        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//////    }

//////    public void ShowRunnerCongrats()
//////    {
//////        Debug.Log("[GameManager] ShowRunnerCongrats!");
//////        ShowCongrats();
//////    }

//////    // ========================
//////    // CLEANING PIGPEN GAME
//////    // ========================

//////    /// <summary>
//////    /// Called by CleaningDropZone whenever a task is successfully completed.
//////    /// Shows congrats once all zones are done.
//////    /// </summary>
//////    public void OnCleaningTaskCompleted(string taskName)
//////    {
//////        _cleaningTasksDone++;

//////        Debug.Log($"[GameManager] Cleaning task done: '{taskName}'" +
//////                  $" ({_cleaningTasksDone}/{allCleaningZones.Count})");

//////        if (allCleaningZones.Count > 0 &&
//////            _cleaningTasksDone >= allCleaningZones.Count)
//////        {
//////            // Small delay so the last cleaning animation finishes before congrats appears.
//////            // Uses a coroutine — GameManager stays active so this is safe.
//////            StartCoroutine(ShowCongratsDelayed(0.6f));
//////        }
//////    }

//////    IEnumerator ShowCongratsDelayed(float delay)
//////    {
//////        yield return new WaitForSeconds(delay);
//////        ShowCongrats();
//////    }

//////    /// <summary>
//////    /// Resets every drop zone and every tool back to their initial state.
//////    /// Wire this to a "Play Again" button on the Congrats panel.
//////    /// </summary>
//////    public void ResetCleaningGame()
//////    {
//////        _cleaningTasksDone = 0;

//////        foreach (var zone in allCleaningZones)
//////            if (zone != null) zone.ResetZone();

//////        foreach (var tool in allTools)
//////            if (tool != null) tool.ResetTool();

//////        if (congratsPanel != null) congratsPanel.SetActive(false);
//////        if (dogController != null) dogController.PlayIdle();
//////    }

//////    // ========================
//////    // SHARED CONGRATS LOGIC
//////    // ========================
//////    void ShowCongrats()
//////    {
//////        Debug.Log("[GameManager] ShowCongrats called!");

//////        if (congratsPanel != null)
//////        {
//////            AnimatePanelFade(congratsPanel);
//////            panelHistory.Push(congratsPanel);
//////        }
//////        else
//////        {
//////            Debug.LogError("[GameManager] congratsPanel not assigned in Inspector!");
//////        }

//////        if (dogController != null) dogController.PlayCongrats();

//////        if (congratsPulsingObject != null)
//////        {
//////            DOVirtual.DelayedCall(0.5f, () =>
//////            {
//////                if (congratsPulsingObject == null) return;
//////                congratsPulsingObject.transform.DOKill();
//////                congratsPulsingObject.transform.localScale = Vector3.one;
//////                congratsPulsingObject.transform
//////                    .DOScale(1.15f, 0.8f)
//////                    .SetEase(Ease.InOutSine)
//////                    .SetLoops(-1, LoopType.Yoyo);
//////            });
//////        }
//////    }

//////    // ========================
//////    // PANEL MANAGEMENT
//////    // ========================
//////    public void ShowPanel(GameObject panelToShow)
//////    {
//////        foreach (GameObject panel in allPanel)
//////            panel.SetActive(false);

//////        if (panelToShow != null)
//////        {
//////            AnimatePanelFade(panelToShow);
//////            panelHistory.Push(panelToShow);
//////        }
//////    }

//////    public void AnimatePanelFade(GameObject panel)
//////    {
//////        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
//////        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

//////        panel.transform.localScale = Vector3.one * 0.4f;
//////        cg.alpha = 0f;
//////        panel.SetActive(true);

//////        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
//////        cg.DOFade(1f, 0.3f);
//////    }

//////    public void CloseCurrectPanel()
//////    {
//////        if (panelHistory.Count == 0) return;

//////        GameObject closePanel = panelHistory.Pop();

//////        if (closePanel == congratsPanel)
//////        {
//////            if (dogController != null) dogController.PlayIdle();

//////            if (congratsPulsingObject != null)
//////            {
//////                congratsPulsingObject.transform.DOKill();
//////                congratsPulsingObject.transform.localScale = Vector3.one;
//////            }
//////        }

//////        closePanel.transform
//////            .DOScale(Vector3.zero, 0.25f)
//////            .SetEase(Ease.InBack)
//////            .OnComplete(() => closePanel.SetActive(false));

//////        if (panelHistory.Count > 0)
//////            AnimatePanelFade(panelHistory.Peek());
//////    }

//////    // ========================
//////    // RESET
//////    // ========================
//////    public void ResetPaintingGame()
//////    {
//////        coloredCount = 0;
//////        foreach (var area in allDropAreas) area.ResetZone();
//////        if (congratsPanel != null) congratsPanel.SetActive(false);
//////        if (dogController != null) dogController.PlayIdle();
//////    }

//////    public void ResetRepairGame()
//////    {
//////        repairedCount = 0;
//////        foreach (var slot in allRepairSlots) slot.ResetSlot();
//////        if (congratsPanel != null) congratsPanel.SetActive(false);
//////        if (dogController != null) dogController.PlayIdle();
//////    }

//////    public void LoadScene(int sceneId) => SceneManager.LoadScene(sceneId);
//////}

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
////    [Tooltip("Drag all 4 CleaningDropZone GameObjects here (Waste, Frame, Grass, Mud).")]
////    public List<CleaningDropZone> allCleaningZones = new List<CleaningDropZone>();

////    [Tooltip("Drag all 4 DraggableTool GameObjects here (Broom, Hammer, Shovel, Brush).")]
////    public List<DraggableTool> allTools = new List<DraggableTool>();

////    [Tooltip("Panel shown when all 4 tasks are complete — put your clean scene image here.")]
////    public GameObject allCleanPanel;

////    private int _cleaningTasksDone = 0;

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
////        _cleaningTasksDone = 0;

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
////    /// Called by CleaningDropZone after each task's animation freezes.
////    /// When all 4 are done, shows the AllClean panel.
////    /// </summary>
////    public void OnCleaningTaskCompleted(string taskName)
////    {
////        _cleaningTasksDone++;

////        Debug.Log($"[GameManager] ✅ '{taskName}' done  " +
////                  $"({_cleaningTasksDone}/{allCleaningZones.Count})");

////        if (_cleaningTasksDone >= allCleaningZones.Count)
////            ShowAllCleanPanel();
////    }

////    void ShowAllCleanPanel()
////    {
////        if (allCleanPanel == null)
////        {
////            Debug.LogError("[GameManager] allCleanPanel is not assigned in the Inspector!");
////            return;
////        }

////        Debug.Log("[GameManager] 🐷 All tasks complete — showing AllClean panel!");
////        AnimatePanelFade(allCleanPanel);
////        panelHistory.Push(allCleanPanel);
////    }

////    /// <summary>
////    /// Resets all zones and tools for replay. Wire to a Play Again button.
////    /// </summary>
////    public void ResetCleaningGame()
////    {
////        _cleaningTasksDone = 0;

////        foreach (var zone in allCleaningZones)
////            if (zone != null) zone.ResetZone();

////        foreach (var tool in allTools)
////            if (tool != null) tool.ResetTool();

////        if (allCleanPanel != null) allCleanPanel.SetActive(false);
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
//    [Tooltip("Drag all 4 DraggableTool GameObjects here (Broom, Hammer, Brush, Sovle).")]
//    public List<DraggableTool> allTools = new List<DraggableTool>();

//    [Tooltip("The image/panel that appears instantly when all 4 tasks are done.\n" +
//             "Starts INACTIVE. This is shown by CleaningDropZone automatically.\n" +
//             "Assign the same GameObject here for the Reset button to work.")]
//    public GameObject allCleanPanel;

//    // ──────────────────────────────────────────────────────────
//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        if (congratsPanel != null) congratsPanel.SetActive(false);
//        if (gameOverPanel != null) gameOverPanel.SetActive(false);
//        if (allCleanPanel != null) allCleanPanel.SetActive(false);

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
//    /// Resets all zones and all tools back to their start state.
//    /// Wire this to your "Play Again" button on the allCleanPanel.
//    /// </summary>
//    public void ResetCleaningGame()
//    {
//        // Reset all drop zones via the static method
//        CleaningDropZone.ResetAll();

//        // Reset all toolbar tool icons
//        foreach (var tool in allTools)
//            if (tool != null) tool.ResetTool();

//        // Hide the completion panel
//        if (allCleanPanel != null)
//            allCleanPanel.SetActive(false);

//        Debug.Log("[GameManager] Cleaning game reset.");
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
    [Header("Cleaning Pigpen Game")]
    [Tooltip("Drag all 4 DraggableTool toolbar icons here (Broom, Hammer, Brush, Shovel).")]
    public List<DraggableTool> allTools = new List<DraggableTool>();

    [Tooltip("The completion panel/image shown when all 4 tasks are done.\n" +
             "Assign the same GameObject that you put in ToolDropZone → Completion Panel.\n" +
             "Needed here so the Play Again / Reset button can hide it.")]
    public GameObject allCleanPanel;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (congratsPanel != null) congratsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (allCleanPanel != null) allCleanPanel.SetActive(false);

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
    /// Resets all 4 drop zones and all 4 toolbar tools back to their start state.
    /// Wire this to your "Play Again" button on the allCleanPanel.
    /// </summary>
    public void ResetCleaningGame()
    {
        // Reset all ToolDropZones via the static method
        ToolDropZone.ResetAll();

        // Reset all toolbar tool icons
        foreach (var tool in allTools)
            if (tool != null) tool.ResetTool();

        // Hide the completion panel
        if (allCleanPanel != null)
            allCleanPanel.SetActive(false);

        Debug.Log("[GameManager] Cleaning game reset.");
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