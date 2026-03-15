////////using DG.Tweening;
////////using System.Collections.Generic;
////////using UnityEngine;
////////using UnityEngine.SceneManagement;

////////public class GameManager : MonoBehaviour
////////{
////////    public List<GameObject> allPanel;

////////    private Stack<GameObject> panelHistory = new Stack<GameObject>();
////////    public void ShowPanel(GameObject panelToShow)
////////    {

////////        foreach (GameObject panel in allPanel)
////////            panel.SetActive(false);

////////        if (panelToShow != null)
////////        {
////////            AnimatePanelFade(panelToShow);
////////            panelHistory.Push(panelToShow);
////////        }
////////    }

////////    public void AnimatePanelFade(GameObject panel)
////////    {
////////        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
////////        if (canvasGroup == null)
////////            canvasGroup = panel.AddComponent<CanvasGroup>();

////////        panel.transform.localScale = Vector3.one * 0.4f;
////////        canvasGroup.alpha = 1f;
////////        panel.SetActive(true);

////////        // Animate both scale and fade
////////        panel.transform.DOScale(1f, .4f).SetEase(Ease.OutBack);
////////        canvasGroup.DOFade(1f, 0.3f);
////////    }

////////    public void CloseCurrectPanel()
////////    {
////////        if (panelHistory.Count > 0)
////////        {
////////            GameObject closePanel = panelHistory.Pop();

////////            // Animate closing before deactivating
////////            closePanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => closePanel.SetActive(false));

////////            if (panelHistory.Count > 0)
////////            {
////////                GameObject previosPanel = panelHistory.Peek();
////////                AnimatePanelFade(previosPanel);
////////            }
////////            else
////////            {
////////                //AnimatePanelFade(mainMenuPanel);
////////                //panelHistory.Push(mainMenuPanel);
////////            }
////////        }
////////    }

////////    public void LoadScene(int sceneId)
////////    {
////////        SceneManager.LoadScene(sceneId);
////////    }
////////}


//////using DG.Tweening;
//////using System.Collections.Generic;
//////using UnityEngine;
//////using UnityEngine.SceneManagement;
//////using UnityEngine.UI;

//////public class GameManager : MonoBehaviour
//////{
//////    public static GameManager Instance;

//////    public List<GameObject> allPanel;
//////    private Stack<GameObject> panelHistory = new Stack<GameObject>();

//////    [Header("Congratulation Panel")]
//////    public GameObject congratsPanel;
//////    public Image congratsImage;
//////    public Sprite originalColoredSprite;

//////    void Awake()
//////    {
//////        Instance = this;
//////    }

//////    void Start()
//////    {
//////        if (congratsPanel != null)
//////            congratsPanel.SetActive(false);
//////    }

//////    // ✅ Check if all drop areas are colored
//////    public void CheckAllColored()
//////    {
//////        ColorDropArea[] allDropAreas = FindObjectsOfType<ColorDropArea>();

//////        foreach (ColorDropArea area in allDropAreas)
//////        {
//////            if (!area.IsColored) return;
//////        }

//////        // ✅ All colored - show congratulations!
//////        ShowCongrats();
//////    }

//////    void ShowCongrats()
//////    {
//////        if (congratsPanel == null) return;

//////        // ✅ Set original colored image
//////        if (congratsImage != null && originalColoredSprite != null)
//////            congratsImage.sprite = originalColoredSprite;

//////        // ✅ Show panel with animation
//////        AnimatePanelFade(congratsPanel);
//////        panelHistory.Push(congratsPanel);

//////        // ✅ Start pulsing the image
//////        if (congratsImage != null)
//////        {
//////            congratsImage.transform.DOKill();
//////            congratsImage.transform.localScale = Vector3.one;
//////            congratsImage.transform
//////                .DOScale(1.08f, 0.8f)
//////                .SetEase(Ease.InOutSine)
//////                .SetLoops(-1, LoopType.Yoyo)
//////                .SetDelay(0.5f); // Wait for panel animation first
//////        }
//////    }

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
//////        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
//////        if (canvasGroup == null)
//////            canvasGroup = panel.AddComponent<CanvasGroup>();

//////        panel.transform.localScale = Vector3.one * 0.4f;
//////        canvasGroup.alpha = 1f;
//////        panel.SetActive(true);

//////        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
//////        canvasGroup.DOFade(1f, 0.3f);
//////    }

//////    public void CloseCurrectPanel()
//////    {
//////        if (panelHistory.Count > 0)
//////        {
//////            GameObject closePanel = panelHistory.Pop();

//////            // ✅ Kill pulsing if closing congrats panel
//////            if (closePanel == congratsPanel && congratsImage != null)
//////                congratsImage.transform.DOKill();

//////            closePanel.transform
//////                .DOScale(Vector3.zero, 0.25f)
//////                .SetEase(Ease.InBack)
//////                .OnComplete(() => closePanel.SetActive(false));

//////            if (panelHistory.Count > 0)
//////            {
//////                GameObject previousPanel = panelHistory.Peek();
//////                AnimatePanelFade(previousPanel);
//////            }
//////        }
//////    }

//////    public void LoadScene(int sceneId)
//////    {
//////        SceneManager.LoadScene(sceneId);
//////    }
//////}
////using DG.Tweening;
////using System.Collections.Generic;
////using Unity.VisualScripting;
////using UnityEngine;
////using UnityEngine.SceneManagement;
////using UnityEngine.UI;
////using static Unity.Collections.Unicode;

////public class GameManager : MonoBehaviour
////{
////    public static GameManager Instance;

////    public List<GameObject> allPanel;
////    private Stack<GameObject> panelHistory = new Stack<GameObject>();

////    [Header("Congratulation Panel")]
////    public GameObject congratsPanel;
////    public Image congratsImage;
////    public Sprite originalColoredSprite;

////    // ✅ Manual registration list
////    private List<ColorDropArea> registeredDropAreas = new List<ColorDropArea>();

////    void Awake()
////    {
////        Instance = this;
////    }

////    void Start()
////    {
////        if (congratsPanel != null)
////            congratsPanel.SetActive(false);

////        // ✅ Auto find all drop areas in scene on start
////        RegisterAllDropAreas();

////        Debug.Log($"[GameManager] Registered {registeredDropAreas.Count} drop areas");
////    }

////    void RegisterAllDropAreas()
////    {
////        registeredDropAreas.Clear();
////        ColorDropArea[] areas = FindObjectsOfType<ColorDropArea>();
////        foreach (ColorDropArea area in areas)
////        {
////            registeredDropAreas.Add(area);
////            Debug.Log($"[GameManager] Registered: {area.gameObject.name}");
////        }
////    }

////    // ✅ Called by ColorDropArea on every successful drop
////    public void CheckAllColored()
////    {
////        int colored = 0;
////        int total = registeredDropAreas.Count;

////        foreach (ColorDropArea area in registeredDropAreas)
////        {
////            if (area.IsColored) colored++;
////        }

////        Debug.Log($"[GameManager] Colored: {colored}/{total}");

////        if (colored >= total && total > 0)
////        {
////            Debug.Log("[GameManager] ALL COLORED - Showing Congrats!");
////            ShowCongrats();
////        }
////    }

////    void ShowCongrats()
////    {
////        if (congratsPanel == null)
////        {
////            Debug.LogError("[GameManager] congratsPanel is NULL! Assign it in Inspector!");
////            return;
////        }

////        if (congratsImage != null && originalColoredSprite != null)
////            congratsImage.sprite = originalColoredSprite;

////        AnimatePanelFade(congratsPanel);
////        panelHistory.Push(congratsPanel);

////        if (congratsImage != null)
////        {
////            congratsImage.transform.DOKill();
////            congratsImage.transform.localScale = Vector3.one;
////            congratsImage.transform
////                .DOScale(1.08f, 0.8f)
////                .SetEase(Ease.InOutSine)
////                .SetLoops(-1, LoopType.Yoyo)
////                .SetDelay(0.5f);
////        }
////    }

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
////        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
////        if (canvasGroup == null)
////            canvasGroup = panel.AddComponent<CanvasGroup>();

////        panel.transform.localScale = Vector3.one * 0.4f;
////        canvasGroup.alpha = 0f;
////        panel.SetActive(true);

////        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
////        canvasGroup.DOFade(1f, 0.3f);
////    }

////    public void CloseCurrectPanel()
////    {
////        if (panelHistory.Count > 0)
////        {
////            GameObject closePanel = panelHistory.Pop();

////            if (closePanel == congratsPanel && congratsImage != null)
////                congratsImage.transform.DOKill();

////            closePanel.transform
////                .DOScale(Vector3.zero, 0.25f)
////                .SetEase(Ease.InBack)
////                .OnComplete(() => closePanel.SetActive(false));

////            if (panelHistory.Count > 0)
////            {
////                GameObject previousPanel = panelHistory.Peek();
////                AnimatePanelFade(previousPanel);
////            }
////        }
////    }

////    public void LoadScene(int sceneId)
////    {
////        SceneManager.LoadScene(sceneId);
////    }
////}
//////```

//////## Now Check Console After Last Drop

//////It will show one of these:
//////```
//////Case 1: "[GameManager] Registered 0 drop areas"
//////→ GameManager.Start() runs BEFORE ColorDropArea.Start()
//////→ Fix: move RegisterAllDropAreas() to be called with delay

//////Case 2: "[GameManager] Colored: 5/6"
//////→ One area is not being counted
//////→ Check which one is missing

//////Case 3: congratsPanel is NULL
//////→ Not assigned in Inspector
/////

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
//    public Image congratsImage;
//    public Sprite originalColoredSprite;

//    // ✅ Manually assign ALL ColorDropAreas in Inspector
//    [Header("All Drop Areas In This Scene")]
//    public List<ColorDropArea> allDropAreas;

//    private int coloredCount = 0;

//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        // ✅ Hide congrats at start
//        if (congratsPanel != null)
//            congratsPanel.SetActive(false);

//        coloredCount = 0;

//        Debug.Log($"[GameManager] Total drop areas: {allDropAreas.Count}");
//    }

//    // ✅ Called by ColorDropArea when ONE zone is successfully colored
//    public void OnZoneColored()
//    {
//        coloredCount++;
//        Debug.Log($"[GameManager] Colored: {coloredCount}/{allDropAreas.Count}");

//        // ✅ Check if ALL zones are done
//        if (coloredCount >= allDropAreas.Count)
//        {
//            Debug.Log("[GameManager] ALL DONE - Showing Congrats!");
//            ShowCongrats();
//        }
//    }

//    void ShowCongrats()
//    {
//        if (congratsPanel == null)
//        {
//            Debug.LogError("[GameManager] congratsPanel not assigned!");
//            return;
//        }

//        // ✅ Set colored image
//        if (congratsImage != null && originalColoredSprite != null)
//            congratsImage.sprite = originalColoredSprite;

//        // ✅ Show panel
//        AnimatePanelFade(congratsPanel);
//        panelHistory.Push(congratsPanel);

//        // ✅ Pulse the image
//        if (congratsImage != null)
//        {
//            congratsImage.transform.DOKill();
//            congratsImage.transform.localScale = Vector3.one;
//            congratsImage.transform
//                .DOScale(1.08f, 0.8f)
//                .SetEase(Ease.InOutSine)
//                .SetLoops(-1, LoopType.Yoyo)
//                .SetDelay(0.5f);
//        }
//    }

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

//            if (closePanel == congratsPanel && congratsImage != null)
//                congratsImage.transform.DOKill();

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

//    public void LoadScene(int sceneId)
//    {
//        SceneManager.LoadScene(sceneId);
//    }
//}

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

    [Header("Pulsing GameObject (any GameObject)")]
    public GameObject pulsingObject;

    // ✅ Manually assign all 15 ColorDropAreas in Inspector
    [Header("All 15 Drop Areas")]
    public List<ColorDropArea> allDropAreas = new List<ColorDropArea>();

    private int coloredCount = 0;

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

        Debug.Log($"[GameManager] Tracking {allDropAreas.Count} drop areas");
    }

    // ✅ Called by ColorDropArea every successful drop
    public void OnZoneColored()
    {
        coloredCount++;
        Debug.Log($"[GameManager] Progress: {coloredCount}/{allDropAreas.Count}");

        if (coloredCount >= allDropAreas.Count && allDropAreas.Count > 0)
        {
            Debug.Log("[GameManager] ALL 15 COLORED! Showing Congrats!");
            ShowCongrats();
        }
    }

    void ShowCongrats()
    {
        if (congratsPanel != null)
        {
            AnimatePanelFade(congratsPanel);
            panelHistory.Push(congratsPanel);
        }
        else
        {
            Debug.LogError("[GameManager] congratsPanel is not assigned!");
            return;
        }

        if (pulsingObject != null)
        {
            pulsingObject.SetActive(true);
            pulsingObject.transform.DOKill();
            pulsingObject.transform.localScale = Vector3.one;

            // ✅ Pulse animation
            pulsingObject.transform
                .DOScale(1.15f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(0.5f);

            // ✅ Stop pulsing after 3 seconds
            DOVirtual.DelayedCall(3.5f, () =>
            {
                pulsingObject.transform.DOKill();
                pulsingObject.transform
                    .DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack);
            });
        }
    }

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

            // ✅ Stop pulsing when closing congrats
            if (closePanel == congratsPanel && pulsingObject != null)
            {
                pulsingObject.transform.DOKill();
                pulsingObject.transform.localScale = Vector3.one;
                pulsingObject.SetActive(false);
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

    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}