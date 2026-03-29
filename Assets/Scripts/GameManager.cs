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

//////    [Header("Congratulation Panel")]
//////    public GameObject congratsPanel;

//////    [Header("Game Over Panel")]
//////    public GameObject gameOverPanel;   // ← assign your Game Over panel here

//////    [Header("Pulsing GameObject")]
//////    public GameObject pulsingObject;

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
//////    public GameObject startButton;
//////    public TMP_Text countdownText;

//////    private bool gameOverShown = false;

//////    // ──────────────────────────────────────────────────────────
//////    void Awake() { Instance = this; }

//////    void Start()
//////    {
//////        if (congratsPanel != null) congratsPanel.SetActive(false);
//////        if (gameOverPanel != null) gameOverPanel.SetActive(false);
//////        if (pulsingObject != null) pulsingObject.SetActive(false);

//////        gameOverShown = false;
//////        coloredCount = 0;
//////        repairedCount = 0;
//////    }

//////    // ========================
//////    // PAINTING GAME
//////    // ========================
//////    public void OnZoneColored()
//////    {
//////        coloredCount++;
//////        Debug.Log($"[GameManager] Painting: {coloredCount}/{allDropAreas.Count}");
//////        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
//////            ShowCongrats();
//////    }

//////    // ========================
//////    // REPAIR GAME
//////    // ========================
//////    public void OnSlotRepaired()
//////    {
//////        repairedCount++;
//////        Debug.Log($"[GameManager] Repaired: {repairedCount}/{allRepairSlots.Count}");
//////        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
//////            ShowCongrats();
//////    }

//////    // ========================
//////    // RUNNER GAME
//////    // ========================

//////    public void StartRunnerGame()
//////    {
//////        if (startButton != null) startButton.SetActive(false);
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

//////    // ──────────────────────────────────────────────────────────
//////    // Called by ObstacleController when cow hits an obstacle
//////    // ──────────────────────────────────────────────────────────
//////    public void ShowGameOver()
//////    {
//////        if (gameOverShown) return; // prevent double-trigger
//////        gameOverShown = true;

//////        Debug.Log("[GameManager] Game Over!");

//////        if (gameOverPanel != null)
//////            AnimatePanelFade(gameOverPanel);
//////        else
//////            Debug.LogError("[GameManager] gameOverPanel not assigned in Inspector!");
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // Called by the Restart button on the Game Over panel
//////    // ──────────────────────────────────────────────────────────
//////    public void RestartGame()
//////    {
//////        DOTween.KillAll();
//////        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // Called by BellTrigger after cow hits the bell
//////    // ──────────────────────────────────────────────────────────
//////    public void ShowRunnerCongrats()
//////    {
//////        Debug.Log("[GameManager] ShowRunnerCongrats!");
//////        ShowCongrats();
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
//////            Debug.LogError("[GameManager] congratsPanel not assigned in Inspector!");

//////        if (dogController != null) dogController.PlayCongrats();

//////        if (pulsingObject != null)
//////        {
//////            pulsingObject.SetActive(true);
//////            pulsingObject.transform.DOKill();
//////            pulsingObject.transform.localScale = Vector3.one;

//////            pulsingObject.transform
//////                .DOScale(1.15f, 0.8f)
//////                .SetEase(Ease.InOutSine)
//////                .SetLoops(-1, LoopType.Yoyo)
//////                .SetDelay(0.5f);

//////            DOVirtual.DelayedCall(3.5f, () =>
//////            {
//////                pulsingObject.transform.DOKill();
//////                pulsingObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
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
//////            if (pulsingObject != null)
//////            {
//////                pulsingObject.transform.DOKill();
//////                pulsingObject.transform.localScale = Vector3.one;
//////                pulsingObject.SetActive(false);
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
//////        if (pulsingObject != null) { pulsingObject.transform.DOKill(); pulsingObject.SetActive(false); }
//////        if (dogController != null) dogController.PlayIdle();
//////    }

//////    public void ResetRepairGame()
//////    {
//////        repairedCount = 0;
//////        foreach (var slot in allRepairSlots) slot.ResetSlot();
//////        if (congratsPanel != null) congratsPanel.SetActive(false);
//////        if (pulsingObject != null) { pulsingObject.transform.DOKill(); pulsingObject.SetActive(false); }
//////        if (dogController != null) dogController.PlayIdle();
//////    }

//////    // ========================
//////    // SCENE MANAGEMENT
//////    // ========================
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

////    [Header("Congratulation Panel")]
////    public GameObject congratsPanel;

////    [Header("Game Over Panel")]
////    public GameObject gameOverPanel;   // ← assign your Game Over panel here

////    [Header("Pulsing GameObject")]
////    public GameObject pulsingObject;
////    public GameObject pulsingObject1;
////    public GameObject pulsingObject2;

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
////    public TMP_Text countdownText;

////    private bool gameOverShown = false;

////    // ──────────────────────────────────────────────────────────
////    void Awake() { Instance = this; }

////    void Start()
////    {
////        if (congratsPanel != null) congratsPanel.SetActive(false);
////        if (gameOverPanel != null) gameOverPanel.SetActive(false);
////        if (pulsingObject != null) pulsingObject.SetActive(false);

////        gameOverShown = false;
////        coloredCount = 0;
////        repairedCount = 0;
////    }

////    // ========================
////    // PAINTING GAME
////    // ========================
////    public void OnZoneColored()
////    {
////        coloredCount++;
////        Debug.Log($"[GameManager] Painting: {coloredCount}/{allDropAreas.Count}");
////        if (allDropAreas.Count > 0 && coloredCount >= allDropAreas.Count)
////            ShowCongrats();
////    }

////    // ========================
////    // REPAIR GAME
////    // ========================
////    public void OnSlotRepaired()
////    {
////        repairedCount++;
////        Debug.Log($"[GameManager] Repaired: {repairedCount}/{allRepairSlots.Count}");
////        if (allRepairSlots.Count > 0 && repairedCount >= allRepairSlots.Count)
////            ShowCongrats();
////    }

////    // ========================
////    // RUNNER GAME
////    // ========================

////    public void StartRunnerGame()
////    {
////        if (startButton != null) startButton.SetActive(false);
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

////    // ──────────────────────────────────────────────────────────
////    // Called by ObstacleController when cow hits an obstacle
////    // ──────────────────────────────────────────────────────────
////    public void ShowGameOver()
////    {
////        if (gameOverShown) return; // prevent double-trigger
////        gameOverShown = true;

////        Debug.Log("[GameManager] Game Over!");

////        if (gameOverPanel != null)
////            AnimatePanelFade(gameOverPanel);
////        else
////            Debug.LogError("[GameManager] gameOverPanel not assigned in Inspector!");
////    }

////    // ──────────────────────────────────────────────────────────
////    // Called by the Restart button on the Game Over panel
////    // ──────────────────────────────────────────────────────────
////    public void RestartGame()
////    {
////        DOTween.KillAll();
////        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
////    }

////    // ──────────────────────────────────────────────────────────
////    // Called by BellTrigger after cow hits the bell
////    // ──────────────────────────────────────────────────────────
////    public void ShowRunnerCongrats()
////    {
////        Debug.Log("[GameManager] ShowRunnerCongrats!");
////        ShowCongrats();
////    }

////    // ========================
////    // SHARED CONGRATS LOGIC
////    // ========================
////    void ShowCongrats()
////    {
////        Debug.Log("[GameManager] ShowCongrats called!");

////        if (congratsPanel != null)
////        {
////            AnimatePanelFade(congratsPanel);
////            panelHistory.Push(congratsPanel);
////        }
////        else
////            Debug.LogError("[GameManager] congratsPanel not assigned in Inspector!");

////        if (dogController != null) dogController.PlayCongrats();

////        if (pulsingObject != null)
////        {
////            pulsingObject.SetActive(true);
////            pulsingObject.transform.DOKill();
////            pulsingObject.transform.localScale = Vector3.one;

////            pulsingObject.transform
////                .DOScale(1.15f, 0.8f)
////                .SetEase(Ease.InOutSine)
////                .SetLoops(-1, LoopType.Yoyo)
////                .SetDelay(0.5f);

////            DOVirtual.DelayedCall(3.5f, () =>
////            {
////                pulsingObject.transform.DOKill();
////                pulsingObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
////            });
////        }

////        if (pulsingObject1 != null)
////        {
////            pulsingObject.SetActive(true);
////            pulsingObject.transform.DOKill();
////            pulsingObject.transform.localScale = Vector3.one;

////            pulsingObject.transform
////                .DOScale(1.15f, 0.8f)
////                .SetEase(Ease.InOutSine)
////                .SetLoops(-1, LoopType.Yoyo)
////                .SetDelay(0.5f);

////            DOVirtual.DelayedCall(3.5f, () =>
////            {
////                pulsingObject.transform.DOKill();
////                pulsingObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
////            });
////        }

////        if (pulsingObject2 != null)
////        {
////            pulsingObject.SetActive(true);
////            pulsingObject.transform.DOKill();
////            pulsingObject.transform.localScale = Vector3.one;

////            pulsingObject.transform
////                .DOScale(1.15f, 0.8f)
////                .SetEase(Ease.InOutSine)
////                .SetLoops(-1, LoopType.Yoyo)
////                .SetDelay(0.5f);

////            DOVirtual.DelayedCall(3.5f, () =>
////            {
////                pulsingObject.transform.DOKill();
////                pulsingObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
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
////            if (pulsingObject != null)
////            {
////                pulsingObject.transform.DOKill();
////                pulsingObject.transform.localScale = Vector3.one;
////                pulsingObject.SetActive(false);
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
////        if (pulsingObject != null) { pulsingObject.transform.DOKill(); pulsingObject.SetActive(false); }
////        if (dogController != null) dogController.PlayIdle();
////    }

////    public void ResetRepairGame()
////    {
////        repairedCount = 0;
////        foreach (var slot in allRepairSlots) slot.ResetSlot();
////        if (congratsPanel != null) congratsPanel.SetActive(false);
////        if (pulsingObject != null) { pulsingObject.transform.DOKill(); pulsingObject.SetActive(false); }
////        if (dogController != null) dogController.PlayIdle();
////    }

////    // ========================
////    // SCENE MANAGEMENT
////    // ========================
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
//    [Tooltip("Drag the text or image inside the Congrats panel that should pulse.")]
//    public GameObject congratsPulsingObject;   // e.g. a star, trophy, or the text

//    [Header("Game Over Panel")]
//    public GameObject gameOverPanel;
//    [Tooltip("Drag the 'Game Over' text or icon inside the panel that should pulse.")]
//    public GameObject gameOverPulsingObject;   // e.g. the Game Over TMP text

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
//    [Tooltip("Drag the Start Button GameObject here.")]
//    public GameObject startButton;
//    [Tooltip("Drag the text/image INSIDE the start button that should pulse.")]
//    public GameObject startButtonPulsingObject; // e.g. the TMP text or icon inside the button
//    public TMP_Text countdownText;

//    private bool gameOverShown = false;

//    // ──────────────────────────────────────────────────────────
//    void Awake()
//    {
//        // ✅ Set Instance immediately in Awake so other scripts
//        //    can safely call GameManager.Instance in their Start()
//        Instance = this;
//    }

//    void Start()
//    {
//        if (congratsPanel != null) congratsPanel.SetActive(false);
//        if (gameOverPanel != null) gameOverPanel.SetActive(false);

//        gameOverShown = false;
//        coloredCount = 0;
//        repairedCount = 0;

//        // ✅ Start button pulsing begins immediately on game start
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

//    // ──────────────────────────────────────────────────────────
//    // Called by ObstacleController when cow hits an obstacle
//    // ──────────────────────────────────────────────────────────
//    public void ShowGameOver()
//    {
//        if (gameOverShown) return;
//        gameOverShown = true;

//        Debug.Log("[GameManager] ☠️ Game Over!");

//        if (gameOverPanel == null)
//        {
//            Debug.LogError("[GameManager] gameOverPanel is not assigned in the Inspector!");
//            return;
//        }

//        // Show panel with pop-in animation
//        AnimatePanelFade(gameOverPanel);

//        // ✅ Start pulsing the Game Over text/icon after panel appears
//        if (gameOverPulsingObject != null)
//        {
//            // Delay pulse until panel fade-in finishes (0.4s)
//            DOVirtual.DelayedCall(0.5f, () =>
//            {
//                if (gameOverPulsingObject != null)
//                {
//                    gameOverPulsingObject.transform.DOKill();
//                    gameOverPulsingObject.transform.localScale = Vector3.one;
//                    gameOverPulsingObject.transform
//                        .DOScale(1.15f, 0.6f)
//                        .SetEase(Ease.InOutSine)
//                        .SetLoops(-1, LoopType.Yoyo);
//                }
//            });
//        }
//    }

//    // ──────────────────────────────────────────────────────────
//    // Restart button on Game Over panel calls this
//    // ──────────────────────────────────────────────────────────
//    public void RestartGame()
//    {
//        DOTween.KillAll();
//        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//    }

//    // ──────────────────────────────────────────────────────────
//    // Called by BellTrigger when cow wins
//    // ──────────────────────────────────────────────────────────
//    public void ShowRunnerCongrats()
//    {
//        Debug.Log("[GameManager] ShowRunnerCongrats!");
//        ShowCongrats();
//    }

//    // ========================
//    // SHARED CONGRATS LOGIC
//    // ========================
//    void ShowCongrats()
//    {
//        Debug.Log("[GameManager] ShowCongrats called!");

//        if (congratsPanel != null)
//        {
//            AnimatePanelFade(congratsPanel);
//            panelHistory.Push(congratsPanel);
//        }
//        else
//        {
//            Debug.LogError("[GameManager] congratsPanel not assigned in Inspector!");
//        }

//        if (dogController != null) dogController.PlayCongrats();

//        // ✅ Pulsing object inside the Congrats panel
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
    [Tooltip("Drag the text or image inside the Congrats panel that should pulse.")]
    public GameObject congratsPulsingObject;   // e.g. a star, trophy, or the text

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    [Tooltip("Drag the 'Game Over' text or icon inside the panel that should pulse.")]
    public GameObject gameOverPulsingObject;   // e.g. the Game Over TMP text

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
    [Tooltip("Drag the Start Button GameObject here.")]
    public GameObject startButton;
    [Tooltip("Drag the text/image INSIDE the start button that should pulse.")]
    public GameObject startButtonPulsingObject; // e.g. the TMP text or icon inside the button
    public TMP_Text countdownText;

    private bool gameOverShown = false;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        // ✅ Set Instance immediately in Awake so other scripts
        //    can safely call GameManager.Instance in their Start()
        Instance = this;
    }

    void Start()
    {
        if (congratsPanel != null) congratsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        gameOverShown = false;
        coloredCount = 0;
        repairedCount = 0;

        // ✅ Start button pulsing begins immediately on game start
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

    // ──────────────────────────────────────────────────────────
    // Called by ObstacleController when cow hits an obstacle
    // ──────────────────────────────────────────────────────────
    public void ShowGameOver()
    {
        if (gameOverShown) return;
        gameOverShown = true;

        Debug.Log("[GameManager] ☠️ Game Over!");

        if (gameOverPanel == null)
        {
            Debug.LogError("[GameManager] gameOverPanel is not assigned in the Inspector!");
            return;
        }

        // Show panel with pop-in animation
        AnimatePanelFade(gameOverPanel);

        // ✅ Start pulsing the Game Over text/icon after panel appears
        if (gameOverPulsingObject != null)
        {
            // Delay pulse until panel fade-in finishes (0.4s)
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (gameOverPulsingObject != null)
                {
                    gameOverPulsingObject.transform.DOKill();
                    gameOverPulsingObject.transform.localScale = Vector3.one;
                    gameOverPulsingObject.transform
                        .DOScale(1.15f, 0.6f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            });
        }
    }

    // ──────────────────────────────────────────────────────────
    // Restart button on Game Over panel calls this
    // ──────────────────────────────────────────────────────────
    public void RestartGame()
    {
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ──────────────────────────────────────────────────────────
    // Called by BellTrigger when cow wins
    // ──────────────────────────────────────────────────────────
    public void ShowRunnerCongrats()
    {
        Debug.Log("[GameManager] ShowRunnerCongrats!");
        ShowCongrats();
    }

    // ========================
    // SHARED CONGRATS LOGIC
    // ========================
    void ShowCongrats()
    {
        Debug.Log("[GameManager] ShowCongrats called!");

        if (congratsPanel != null)
        {
            AnimatePanelFade(congratsPanel);
            panelHistory.Push(congratsPanel);
        }
        else
        {
            Debug.LogError("[GameManager] congratsPanel not assigned in Inspector!");
        }

        if (dogController != null) dogController.PlayCongrats();

        // ✅ Pulsing object inside the Congrats panel
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