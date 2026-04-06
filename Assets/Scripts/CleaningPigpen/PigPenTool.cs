//////using UnityEngine;
//////using UnityEngine.EventSystems;
//////using UnityEngine.UI;
//////using System.Collections;
//////using System.Collections.Generic;

//////public class PigPenTool : MonoBehaviour,
//////    IBeginDragHandler, IDragHandler, IEndDragHandler
//////{
//////    [Header("Identity")]
//////    public int toolID = 0;

//////    [Header("References")]
//////    public Canvas rootCanvas;

//////    [Header("Tool Animation (loops while cleaning)")]
//////    public Sprite[] toolFrames;
//////    [Range(1f, 30f)]
//////    public float toolFPS = 6f;
//////    [Range(0.1f, 5f)]
//////    [Tooltip("Uniform scale of this tool's RectTransform during the cleaning animation.")]
//////    public float toolAnimScale = 1f;
//////    [Range(50f, 1000f)]
//////    [Tooltip("Speed (pixels/sec) at which the tool moves between path waypoints.")]
//////    public float toolMoveSpeed = 300f;
//////    [Tooltip("Empty child RectTransforms placed in the Scene. The tool loops through them while cleaning.")]
//////    public RectTransform[] toolAnimPath;

//////    private static readonly List<PigPenTool> allInstances = new List<PigPenTool>();

//////    private RectTransform rt;
//////    private CanvasGroup cg;
//////    private Image img;

//////    // Home state — captured once in Awake
//////    private Transform homeParent;
//////    private int homeSiblingIndex;
//////    private Vector2 homeAnchoredPosition;
//////    private Vector2 homeSizeDelta;      // never changed after Awake
//////    private Vector3 homeLocalScale;     // never changed after Awake
//////    private Sprite defaultSprite;

//////    // The visual size in world units at startup — used to restore scale after re-parenting
//////    private Vector2 homeWorldSize;

//////    private Coroutine toolAnim;
//////    private Coroutine toolMove;

//////    // ── Lifecycle ─────────────────────────────────────────────────────────────

//////    void Awake()
//////    {
//////        rt = GetComponent<RectTransform>();
//////        cg = GetComponent<CanvasGroup>();
//////        img = GetComponent<Image>();

//////        homeParent = transform.parent;
//////        homeSiblingIndex = transform.GetSiblingIndex();
//////        homeAnchoredPosition = rt.anchoredPosition;
//////        homeSizeDelta = rt.sizeDelta;
//////        homeLocalScale = rt.localScale;
//////        defaultSprite = img != null ? img.sprite : null;

//////        homeWorldSize = new Vector2(
//////            rt.rect.width * rt.lossyScale.x,
//////            rt.rect.height * rt.lossyScale.y);

//////        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
//////        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();
//////    }

//////    // Clears the static list on every Play-mode start (and domain reload).
//////    // Prevents MissingReferenceException when Domain Reload is disabled.
//////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//////    static void ClearStatics() => allInstances.Clear();

//////    void OnEnable() { if (!allInstances.Contains(this)) allInstances.Add(this); }
//////    void OnDisable() { allInstances.Remove(this); }
//////    void OnDestroy() { allInstances.Remove(this); StopToolAnim(); }

//////    // ── Drag ─────────────────────────────────────────────────────────────────

//////    public void OnBeginDrag(PointerEventData e)
//////    {
//////        if (rootCanvas == null) return;

//////        // 1. Remember screen position BEFORE changing parent
//////        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), rt.position);

//////        // 2. Move to root canvas (top of draw order)
//////        transform.SetParent(rootCanvas.transform, true);
//////        transform.SetAsLastSibling();

//////        // 3. Restore same screen position under new parent
//////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//////            rootCanvas.GetComponent<RectTransform>(),
//////            screenPos, GetUICamera(), out Vector2 localPt);
//////        rt.anchoredPosition = localPt;

//////        // 4. Preserve visual size via localScale after re-parent
//////        Vector3 newLossy = rt.lossyScale;
//////        float sx = Mathf.Abs(homeSizeDelta.x * newLossy.x) > 0.0001f
//////                   ? homeWorldSize.x / (homeSizeDelta.x * newLossy.x) : homeLocalScale.x;
//////        float sy = Mathf.Abs(homeSizeDelta.y * newLossy.y) > 0.0001f
//////                   ? homeWorldSize.y / (homeSizeDelta.y * newLossy.y) : homeLocalScale.y;
//////        rt.localScale = new Vector3(sx, sy, 1f);

//////        if (cg != null) cg.blocksRaycasts = false;
//////        ClearAllHighlights();
//////    }

//////    public void OnDrag(PointerEventData e)
//////    {
//////        if (rootCanvas == null) return;
//////        rt.anchoredPosition += e.delta / rootCanvas.scaleFactor;
//////        UpdateZoneHighlights(e.position);
//////    }

//////    public void OnEndDrag(PointerEventData e)
//////    {
//////        if (rootCanvas == null) return;

//////        ClearAllHighlights();
//////        if (cg != null) cg.blocksRaycasts = true;

//////        bool cleaned = false;
//////        Camera uiCam = GetUICamera();

//////        foreach (PigPenZone zone in PigPenZone.AllZones)
//////        {
//////            if (zone == null) continue;
//////            if (!zone.ContainsScreenPoint(e.position, uiCam)) continue;
//////            if (zone.TryClean(this)) { cleaned = true; break; }
//////        }

//////        if (!cleaned) ReturnHome();
//////    }

//////    // ── Zone highlights ───────────────────────────────────────────────────────

//////    void UpdateZoneHighlights(Vector2 screenPos)
//////    {
//////        Camera uiCam = GetUICamera();
//////        foreach (PigPenZone zone in PigPenZone.AllZones)
//////        {
//////            if (zone == null) continue;
//////            bool hit = !zone.IsClean
//////                    && !zone.IsCleaning
//////                    && zone.AcceptToolID == toolID
//////                    && zone.ContainsScreenPoint(screenPos, uiCam);
//////            zone.SetHighlight(hit);
//////        }
//////    }

//////    void ClearAllHighlights()
//////    {
//////        foreach (PigPenZone zone in PigPenZone.AllZones)
//////            if (zone != null) zone.SetHighlight(false);
//////    }

//////    // ── Animation ─────────────────────────────────────────────────────────────

//////    /// <summary>
//////    /// Called by PigPenZone. The tool snaps to path[0] then moves through the
//////    /// waypoints in a loop while the frame animation plays.
//////    /// </summary>
//////    public void StartToolAnimation()
//////    {
//////        StopToolAnim();

//////        rt.localScale = new Vector3(toolAnimScale, toolAnimScale, 1f);

//////        if (toolAnimPath != null && toolAnimPath.Length > 0)
//////        {
//////            rt.anchoredPosition = WorldToCanvasPos(toolAnimPath[0].position);
//////            toolMove = StartCoroutine(MoveAlongPath());
//////        }

//////        if (toolFrames != null && toolFrames.Length > 0)
//////            toolAnim = StartCoroutine(PlayToolLoop());
//////    }

//////    Vector2 WorldToCanvasPos(Vector3 worldPos)
//////    {
//////        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), worldPos);
//////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//////            rootCanvas.GetComponent<RectTransform>(),
//////            screenPos, GetUICamera(), out Vector2 local);
//////        return local;
//////    }

//////    IEnumerator MoveAlongPath()
//////    {
//////        for (int i = 1; i < toolAnimPath.Length; i++)
//////        {
//////            Vector2 from = rt.anchoredPosition;
//////            Vector2 to = WorldToCanvasPos(toolAnimPath[i].position);
//////            float dist = Vector2.Distance(from, to);
//////            float dur = dist / Mathf.Max(toolMoveSpeed, 1f);
//////            float t = 0f;

//////            while (t < dur)
//////            {
//////                t += Time.deltaTime;
//////                rt.anchoredPosition = Vector2.Lerp(from, to, t / dur);
//////                yield return null;
//////            }

//////            rt.anchoredPosition = to;
//////        }
//////    }

//////    IEnumerator PlayToolLoop()
//////    {
//////        float delay = 1f / Mathf.Max(toolFPS, 1f);
//////        int index = 0;
//////        while (true)
//////        {
//////            if (img == null) yield break;
//////            img.sprite = toolFrames[index % toolFrames.Length];
//////            index++;
//////            yield return new WaitForSeconds(delay);
//////        }
//////    }

//////    void StopToolAnim()
//////    {
//////        if (toolAnim != null) { StopCoroutine(toolAnim); toolAnim = null; }
//////        if (toolMove != null) { StopCoroutine(toolMove); toolMove = null; }
//////    }

//////    // ── Return home ───────────────────────────────────────────────────────────

//////    public void ReturnHome()
//////    {
//////        StopToolAnim();
//////        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
//////        if (cg != null) cg.blocksRaycasts = true;

//////        rt.localScale = homeLocalScale;

//////        transform.SetParent(homeParent);
//////        transform.SetSiblingIndex(homeSiblingIndex);

//////        StartCoroutine(SmoothGoHome());
//////    }

//////    IEnumerator SmoothGoHome()
//////    {
//////        float t = 0f, dur = 0.35f;
//////        Vector2 from = rt.anchoredPosition;
//////        while (t < dur)
//////        {
//////            t += Time.deltaTime;
//////            rt.anchoredPosition = Vector2.Lerp(from, homeAnchoredPosition,
//////                                  Mathf.SmoothStep(0f, 1f, t / dur));
//////            yield return null;
//////        }
//////        rt.anchoredPosition = homeAnchoredPosition;
//////    }

//////    // ── Reset ─────────────────────────────────────────────────────────────────

//////    public void ResetTool()
//////    {
//////        StopToolAnim();
//////        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
//////        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }
//////        rt.localScale = homeLocalScale;
//////        rt.anchoredPosition = homeAnchoredPosition;
//////        transform.SetParent(homeParent);
//////        transform.SetSiblingIndex(homeSiblingIndex);
//////    }

//////    public static void ResetAll()
//////    {
//////        allInstances.RemoveAll(t => t == null);
//////        foreach (var tool in allInstances) tool.ResetTool();
//////    }

//////    Camera GetUICamera()
//////    {
//////        if (rootCanvas == null) return null;
//////        return rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//////               ? null : rootCanvas.worldCamera;
//////    }

//////    public int GetToolID() => toolID;
//////}

////using UnityEngine;
////using UnityEngine.EventSystems;
////using UnityEngine.UI;
////using System.Collections;
////using System.Collections.Generic;

////public class PigPenTool : MonoBehaviour,
////    IBeginDragHandler, IDragHandler, IEndDragHandler
////{
////    [Header("Identity")]
////    public int toolID = 0;
////    public int taskID = 0;          // matches PigPenZone.taskID to identify which task this tool belongs to

////    [Header("References")]
////    public Canvas rootCanvas;

////    [Header("Tool Animation (loops while cleaning)")]
////    public Sprite[] toolFrames;
////    [Range(1f, 30f)]
////    public float toolFPS = 6f;
////    [Range(0.1f, 5f)]
////    [Tooltip("Uniform scale of this tool's RectTransform during the cleaning animation.")]
////    public float toolAnimScale = 1f;
////    [Range(50f, 1000f)]
////    [Tooltip("Speed (pixels/sec) at which the tool moves between path waypoints.")]
////    public float toolMoveSpeed = 300f;
////    // NOTE: toolAnimPath has moved to PigPenZone. Each zone now owns the
////    //       waypoints the tool follows while cleaning that specific zone.

////    private static readonly List<PigPenTool> allInstances = new List<PigPenTool>();

////    private RectTransform rt;
////    private CanvasGroup cg;
////    private Image img;

////    // Home state — captured once in Awake
////    private Transform homeParent;
////    private int homeSiblingIndex;
////    private Vector2 homeAnchoredPosition;
////    private Vector2 homeSizeDelta;
////    private Vector3 homeLocalScale;
////    private Sprite defaultSprite;

////    // The visual size in world units at startup — used to restore scale after re-parenting
////    private Vector2 homeWorldSize;

////    private Coroutine toolAnim;
////    private Coroutine toolMove;

////    // ── Lifecycle ─────────────────────────────────────────────────────────────

////    void Awake()
////    {
////        rt = GetComponent<RectTransform>();
////        cg = GetComponent<CanvasGroup>();
////        img = GetComponent<Image>();

////        homeParent = transform.parent;
////        homeSiblingIndex = transform.GetSiblingIndex();
////        homeAnchoredPosition = rt.anchoredPosition;
////        homeSizeDelta = rt.sizeDelta;
////        homeLocalScale = rt.localScale;
////        defaultSprite = img != null ? img.sprite : null;

////        homeWorldSize = new Vector2(
////            rt.rect.width * rt.lossyScale.x,
////            rt.rect.height * rt.lossyScale.y);

////        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
////        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();
////    }

////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
////    static void ClearStatics() => allInstances.Clear();

////    void OnEnable() { if (!allInstances.Contains(this)) allInstances.Add(this); }
////    void OnDisable() { allInstances.Remove(this); }
////    void OnDestroy() { allInstances.Remove(this); StopToolAnim(); }

////    // ── Drag ─────────────────────────────────────────────────────────────────

////    public void OnBeginDrag(PointerEventData e)
////    {
////        if (rootCanvas == null) return;

////        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), rt.position);

////        transform.SetParent(rootCanvas.transform, true);
////        transform.SetAsLastSibling();

////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
////            rootCanvas.GetComponent<RectTransform>(),
////            screenPos, GetUICamera(), out Vector2 localPt);
////        rt.anchoredPosition = localPt;

////        Vector3 newLossy = rt.lossyScale;
////        float sx = Mathf.Abs(homeSizeDelta.x * newLossy.x) > 0.0001f
////                   ? homeWorldSize.x / (homeSizeDelta.x * newLossy.x) : homeLocalScale.x;
////        float sy = Mathf.Abs(homeSizeDelta.y * newLossy.y) > 0.0001f
////                   ? homeWorldSize.y / (homeSizeDelta.y * newLossy.y) : homeLocalScale.y;
////        rt.localScale = new Vector3(sx, sy, 1f);

////        if (cg != null) cg.blocksRaycasts = false;
////        ClearAllHighlights();
////    }

////    public void OnDrag(PointerEventData e)
////    {
////        if (rootCanvas == null) return;
////        rt.anchoredPosition += e.delta / rootCanvas.scaleFactor;
////        UpdateZoneHighlights(e.position);
////    }

////    public void OnEndDrag(PointerEventData e)
////    {
////        if (rootCanvas == null) return;

////        ClearAllHighlights();
////        if (cg != null) cg.blocksRaycasts = true;

////        bool cleaned = false;
////        Camera uiCam = GetUICamera();

////        foreach (PigPenZone zone in PigPenZone.AllZones)
////        {
////            if (zone == null) continue;
////            if (!zone.ContainsScreenPoint(e.position, uiCam)) continue;
////            if (zone.TryClean(this)) { cleaned = true; break; }
////        }

////        if (!cleaned) ReturnHome();
////    }

////    // ── Zone highlights ───────────────────────────────────────────────────────

////    void UpdateZoneHighlights(Vector2 screenPos)
////    {
////        Camera uiCam = GetUICamera();
////        foreach (PigPenZone zone in PigPenZone.AllZones)
////        {
////            if (zone == null) continue;
////            bool hit = !zone.IsClean
////                    && !zone.IsCleaning
////                    && zone.AcceptToolID == toolID
////                    && zone.TaskID == taskID          // also match by taskID
////                    && zone.ContainsScreenPoint(screenPos, uiCam);
////            zone.SetHighlight(hit);
////        }
////    }

////    void ClearAllHighlights()
////    {
////        foreach (PigPenZone zone in PigPenZone.AllZones)
////            if (zone != null) zone.SetHighlight(false);
////    }

////    // ── Animation ─────────────────────────────────────────────────────────────

////    /// <summary>
////    /// Called by PigPenZone. Receives the zone's own waypoint path so the tool
////    /// travels the correct route for that specific zone/task.
////    /// </summary>
////    public void StartToolAnimation(RectTransform[] animPath)
////    {
////        StopToolAnim();

////        rt.localScale = new Vector3(toolAnimScale, toolAnimScale, 1f);

////        if (animPath != null && animPath.Length > 0)
////        {
////            rt.anchoredPosition = WorldToCanvasPos(animPath[0].position);
////            toolMove = StartCoroutine(MoveAlongPath(animPath));
////        }

////        if (toolFrames != null && toolFrames.Length > 0)
////            toolAnim = StartCoroutine(PlayToolLoop());
////    }

////    Vector2 WorldToCanvasPos(Vector3 worldPos)
////    {
////        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), worldPos);
////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
////            rootCanvas.GetComponent<RectTransform>(),
////            screenPos, GetUICamera(), out Vector2 local);
////        return local;
////    }

////    IEnumerator MoveAlongPath(RectTransform[] path)
////    {
////        for (int i = 1; i < path.Length; i++)
////        {
////            Vector2 from = rt.anchoredPosition;
////            Vector2 to = WorldToCanvasPos(path[i].position);
////            float dist = Vector2.Distance(from, to);
////            float dur = dist / Mathf.Max(toolMoveSpeed, 1f);
////            float t = 0f;

////            while (t < dur)
////            {
////                t += Time.deltaTime;
////                rt.anchoredPosition = Vector2.Lerp(from, to, t / dur);
////                yield return null;
////            }

////            rt.anchoredPosition = to;
////        }
////    }

////    IEnumerator PlayToolLoop()
////    {
////        float delay = 1f / Mathf.Max(toolFPS, 1f);
////        int index = 0;
////        while (true)
////        {
////            if (img == null) yield break;
////            img.sprite = toolFrames[index % toolFrames.Length];
////            index++;
////            yield return new WaitForSeconds(delay);
////        }
////    }

////    void StopToolAnim()
////    {
////        if (toolAnim != null) { StopCoroutine(toolAnim); toolAnim = null; }
////        if (toolMove != null) { StopCoroutine(toolMove); toolMove = null; }
////    }

////    // ── Return home ───────────────────────────────────────────────────────────

////    public void ReturnHome()
////    {
////        StopToolAnim();
////        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
////        if (cg != null) cg.blocksRaycasts = true;

////        rt.localScale = homeLocalScale;

////        transform.SetParent(homeParent);
////        transform.SetSiblingIndex(homeSiblingIndex);

////        StartCoroutine(SmoothGoHome());
////    }

////    IEnumerator SmoothGoHome()
////    {
////        float t = 0f, dur = 0.35f;
////        Vector2 from = rt.anchoredPosition;
////        while (t < dur)
////        {
////            t += Time.deltaTime;
////            rt.anchoredPosition = Vector2.Lerp(from, homeAnchoredPosition,
////                                  Mathf.SmoothStep(0f, 1f, t / dur));
////            yield return null;
////        }
////        rt.anchoredPosition = homeAnchoredPosition;
////    }

////    // ── Reset ─────────────────────────────────────────────────────────────────

////    public void ResetTool()
////    {
////        StopToolAnim();
////        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
////        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }
////        rt.localScale = homeLocalScale;
////        rt.anchoredPosition = homeAnchoredPosition;
////        transform.SetParent(homeParent);
////        transform.SetSiblingIndex(homeSiblingIndex);
////    }

////    public static void ResetAll()
////    {
////        allInstances.RemoveAll(t => t == null);
////        foreach (var tool in allInstances) tool.ResetTool();
////    }

////    Camera GetUICamera()
////    {
////        if (rootCanvas == null) return null;
////        return rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
////               ? null : rootCanvas.worldCamera;
////    }

////    public int GetToolID() => toolID;
////    public int GetTaskID() => taskID;
////}

//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;

//public class PigPenTool : MonoBehaviour,
//    IBeginDragHandler, IDragHandler, IEndDragHandler
//{
//    [Header("Identity")]
//    public int toolID = 0;
//    [Tooltip("All task IDs this tool is allowed to work on (e.g. hammer can have taskID 0 = frame AND taskID 1 = wall).")]
//    public int[] taskIDs = new int[0];  // one tool can belong to multiple tasks

//    [Header("References")]
//    public Canvas rootCanvas;

//    [Header("Tool Animation (loops while cleaning)")]
//    public Sprite[] toolFrames;
//    [Range(1f, 30f)]
//    public float toolFPS = 6f;
//    [Range(0.1f, 5f)]
//    [Tooltip("Uniform scale of this tool's RectTransform during the cleaning animation.")]
//    public float toolAnimScale = 1f;
//    [Range(50f, 1000f)]
//    [Tooltip("Speed (pixels/sec) at which the tool moves between path waypoints.")]
//    public float toolMoveSpeed = 300f;
//    // NOTE: toolAnimPath has moved to PigPenZone. Each zone now owns the
//    //       waypoints the tool follows while cleaning that specific zone.

//    private static readonly List<PigPenTool> allInstances = new List<PigPenTool>();

//    private RectTransform rt;
//    private CanvasGroup cg;
//    private Image img;

//    // Home state — captured once in Awake
//    private Transform homeParent;
//    private int homeSiblingIndex;
//    private Vector2 homeAnchoredPosition;
//    private Vector2 homeSizeDelta;
//    private Vector3 homeLocalScale;
//    private Sprite defaultSprite;

//    // The visual size in world units at startup — used to restore scale after re-parenting
//    private Vector2 homeWorldSize;

//    private Coroutine toolAnim;
//    private Coroutine toolMove;

//    // ── Lifecycle ─────────────────────────────────────────────────────────────

//    void Awake()
//    {
//        rt = GetComponent<RectTransform>();
//        cg = GetComponent<CanvasGroup>();
//        img = GetComponent<Image>();

//        homeParent = transform.parent;
//        homeSiblingIndex = transform.GetSiblingIndex();
//        homeAnchoredPosition = rt.anchoredPosition;
//        homeSizeDelta = rt.sizeDelta;
//        homeLocalScale = rt.localScale;
//        defaultSprite = img != null ? img.sprite : null;

//        homeWorldSize = new Vector2(
//            rt.rect.width * rt.lossyScale.x,
//            rt.rect.height * rt.lossyScale.y);

//        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
//        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();
//    }

//    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//    static void ClearStatics() => allInstances.Clear();

//    void OnEnable() { if (!allInstances.Contains(this)) allInstances.Add(this); }
//    void OnDisable() { allInstances.Remove(this); }
//    void OnDestroy() { allInstances.Remove(this); StopToolAnim(); }

//    // ── Drag ─────────────────────────────────────────────────────────────────

//    public void OnBeginDrag(PointerEventData e)
//    {
//        if (rootCanvas == null) return;

//        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), rt.position);

//        transform.SetParent(rootCanvas.transform, true);
//        transform.SetAsLastSibling();

//        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//            rootCanvas.GetComponent<RectTransform>(),
//            screenPos, GetUICamera(), out Vector2 localPt);
//        rt.anchoredPosition = localPt;

//        Vector3 newLossy = rt.lossyScale;
//        float sx = Mathf.Abs(homeSizeDelta.x * newLossy.x) > 0.0001f
//                   ? homeWorldSize.x / (homeSizeDelta.x * newLossy.x) : homeLocalScale.x;
//        float sy = Mathf.Abs(homeSizeDelta.y * newLossy.y) > 0.0001f
//                   ? homeWorldSize.y / (homeSizeDelta.y * newLossy.y) : homeLocalScale.y;
//        rt.localScale = new Vector3(sx, sy, 1f);

//        if (cg != null) cg.blocksRaycasts = false;
//        ClearAllHighlights();
//    }

//    public void OnDrag(PointerEventData e)
//    {
//        if (rootCanvas == null) return;
//        rt.anchoredPosition += e.delta / rootCanvas.scaleFactor;
//        UpdateZoneHighlights(e.position);
//    }

//    public void OnEndDrag(PointerEventData e)
//    {
//        if (rootCanvas == null) return;

//        ClearAllHighlights();
//        if (cg != null) cg.blocksRaycasts = true;

//        bool cleaned = false;
//        Camera uiCam = GetUICamera();

//        foreach (PigPenZone zone in PigPenZone.AllZones)
//        {
//            if (zone == null) continue;
//            if (!zone.ContainsScreenPoint(e.position, uiCam)) continue;
//            if (zone.TryClean(this)) { cleaned = true; break; }
//        }

//        if (!cleaned) ReturnHome();
//    }

//    // ── Zone highlights ───────────────────────────────────────────────────────

//    void UpdateZoneHighlights(Vector2 screenPos)
//    {
//        Camera uiCam = GetUICamera();
//        foreach (PigPenZone zone in PigPenZone.AllZones)
//        {
//            if (zone == null) continue;
//            bool hit = !zone.IsClean
//                    && !zone.IsCleaning
//                    && zone.AcceptToolID == toolID
//                    && HasTaskID(zone.TaskID)         // tool must include the zone's taskID
//                    && zone.ContainsScreenPoint(screenPos, uiCam);
//            zone.SetHighlight(hit);
//        }
//    }

//    void ClearAllHighlights()
//    {
//        foreach (PigPenZone zone in PigPenZone.AllZones)
//            if (zone != null) zone.SetHighlight(false);
//    }

//    // ── Animation ─────────────────────────────────────────────────────────────

//    /// <summary>
//    /// Called by PigPenZone. Receives the zone's own waypoint path so the tool
//    /// travels the correct route for that specific zone/task.
//    /// </summary>
//    public void StartToolAnimation(RectTransform[] animPath)
//    {
//        StopToolAnim();

//        rt.localScale = new Vector3(toolAnimScale, toolAnimScale, 1f);

//        if (animPath != null && animPath.Length > 0)
//        {
//            rt.anchoredPosition = WorldToCanvasPos(animPath[0].position);
//            toolMove = StartCoroutine(MoveAlongPath(animPath));
//        }

//        if (toolFrames != null && toolFrames.Length > 0)
//            toolAnim = StartCoroutine(PlayToolLoop());
//    }

//    Vector2 WorldToCanvasPos(Vector3 worldPos)
//    {
//        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), worldPos);
//        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//            rootCanvas.GetComponent<RectTransform>(),
//            screenPos, GetUICamera(), out Vector2 local);
//        return local;
//    }

//    IEnumerator MoveAlongPath(RectTransform[] path)
//    {
//        for (int i = 1; i < path.Length; i++)
//        {
//            Vector2 from = rt.anchoredPosition;
//            Vector2 to = WorldToCanvasPos(path[i].position);
//            float dist = Vector2.Distance(from, to);
//            float dur = dist / Mathf.Max(toolMoveSpeed, 1f);
//            float t = 0f;

//            while (t < dur)
//            {
//                t += Time.deltaTime;
//                rt.anchoredPosition = Vector2.Lerp(from, to, t / dur);
//                yield return null;
//            }

//            rt.anchoredPosition = to;
//        }
//    }

//    IEnumerator PlayToolLoop()
//    {
//        float delay = 1f / Mathf.Max(toolFPS, 1f);
//        int index = 0;
//        while (true)
//        {
//            if (img == null) yield break;
//            img.sprite = toolFrames[index % toolFrames.Length];
//            index++;
//            yield return new WaitForSeconds(delay);
//        }
//    }

//    void StopToolAnim()
//    {
//        if (toolAnim != null) { StopCoroutine(toolAnim); toolAnim = null; }
//        if (toolMove != null) { StopCoroutine(toolMove); toolMove = null; }
//    }

//    // ── Return home ───────────────────────────────────────────────────────────

//    public void ReturnHome()
//    {
//        StopToolAnim();
//        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
//        if (cg != null) cg.blocksRaycasts = true;

//        rt.localScale = homeLocalScale;

//        transform.SetParent(homeParent);
//        transform.SetSiblingIndex(homeSiblingIndex);

//        StartCoroutine(SmoothGoHome());
//    }

//    IEnumerator SmoothGoHome()
//    {
//        float t = 0f, dur = 0.35f;
//        Vector2 from = rt.anchoredPosition;
//        while (t < dur)
//        {
//            t += Time.deltaTime;
//            rt.anchoredPosition = Vector2.Lerp(from, homeAnchoredPosition,
//                                  Mathf.SmoothStep(0f, 1f, t / dur));
//            yield return null;
//        }
//        rt.anchoredPosition = homeAnchoredPosition;
//    }

//    // ── Reset ─────────────────────────────────────────────────────────────────

//    public void ResetTool()
//    {
//        StopToolAnim();
//        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
//        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }
//        rt.localScale = homeLocalScale;
//        rt.anchoredPosition = homeAnchoredPosition;
//        transform.SetParent(homeParent);
//        transform.SetSiblingIndex(homeSiblingIndex);
//    }

//    public static void ResetAll()
//    {
//        allInstances.RemoveAll(t => t == null);
//        foreach (var tool in allInstances) tool.ResetTool();
//    }

//    Camera GetUICamera()
//    {
//        if (rootCanvas == null) return null;
//        return rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
//               ? null : rootCanvas.worldCamera;
//    }

//    public int GetToolID() => toolID;

//    /// <summary>Returns true if this tool is registered for the given taskID.</summary>
//    public bool HasTaskID(int id)
//    {
//        if (taskIDs == null) return false;
//        foreach (int t in taskIDs)
//            if (t == id) return true;
//        return false;
//    }
//}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PigPenTool : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Identity")]
    public int toolID = 0;
    [Tooltip("All task IDs this tool is allowed to work on (e.g. hammer can have taskID 0 = frame AND taskID 1 = wall).")]
    public int[] taskIDs = new int[0];  // one tool can belong to multiple tasks

    [Header("References")]
    public Canvas rootCanvas;

    [Header("Tool Animation (loops while cleaning)")]
    public Sprite[] toolFrames;
    [Range(1f, 30f)]
    public float toolFPS = 6f;
    [Range(0.1f, 5f)]
    [Tooltip("Uniform scale of this tool's RectTransform during the cleaning animation.")]
    public float toolAnimScale = 1f;
    [Range(50f, 1000f)]
    [Tooltip("Speed (pixels/sec) at which the tool moves between path waypoints.")]
    public float toolMoveSpeed = 300f;
    // NOTE: toolAnimPath has moved to PigPenZone. Each zone now owns the
    //       waypoints the tool follows while cleaning that specific zone.

    private static readonly List<PigPenTool> allInstances = new List<PigPenTool>();

    private RectTransform rt;
    private CanvasGroup cg;
    private Image img;

    // Home state — captured once in Awake
    private Transform homeParent;
    private int homeSiblingIndex;
    private Vector2 homeAnchoredPosition;
    private Vector2 homeSizeDelta;
    private Vector3 homeLocalScale;
    private Sprite defaultSprite;

    // The visual size in world units at startup — used to restore scale after re-parenting
    private Vector2 homeWorldSize;

    private Coroutine toolAnim;
    private Coroutine toolMove;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        img = GetComponent<Image>();

        homeParent = transform.parent;
        homeSiblingIndex = transform.GetSiblingIndex();
        homeAnchoredPosition = rt.anchoredPosition;
        homeSizeDelta = rt.sizeDelta;
        homeLocalScale = rt.localScale;
        defaultSprite = img != null ? img.sprite : null;

        homeWorldSize = new Vector2(
            rt.rect.width * rt.lossyScale.x,
            rt.rect.height * rt.lossyScale.y);

        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ClearStatics() => allInstances.Clear();

    void OnEnable() { if (!allInstances.Contains(this)) allInstances.Add(this); }
    void OnDisable() { allInstances.Remove(this); }
    void OnDestroy() { allInstances.Remove(this); StopToolAnim(); }

    // ── Drag ─────────────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData e)
    {
        if (rootCanvas == null) return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), rt.position);

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos, GetUICamera(), out Vector2 localPt);
        rt.anchoredPosition = localPt;

        Vector3 newLossy = rt.lossyScale;
        float sx = Mathf.Abs(homeSizeDelta.x * newLossy.x) > 0.0001f
                   ? homeWorldSize.x / (homeSizeDelta.x * newLossy.x) : homeLocalScale.x;
        float sy = Mathf.Abs(homeSizeDelta.y * newLossy.y) > 0.0001f
                   ? homeWorldSize.y / (homeSizeDelta.y * newLossy.y) : homeLocalScale.y;
        rt.localScale = new Vector3(sx, sy, 1f);

        if (cg != null) cg.blocksRaycasts = false;
        ClearAllHighlights();
    }

    public void OnDrag(PointerEventData e)
    {
        if (rootCanvas == null) return;
        rt.anchoredPosition += e.delta / rootCanvas.scaleFactor;
        UpdateZoneHighlights(e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (rootCanvas == null) return;

        ClearAllHighlights();
        if (cg != null) cg.blocksRaycasts = true;

        Camera uiCam = GetUICamera();

        // ── Find the best-matching zone ───────────────────────────────────────
        // Never break on the first rect hit. Score every zone and pick the most
        // specific match so overlapping zones can't steal a drop.
        //
        // Score:
        //   2 = pixel hit  AND  toolID + taskID both match  ← only this triggers clean
        //   1 = pixel hit  AND  only one ID matches          ← rejected, returns home
        //   0 = rect hit but pixel is transparent            ← rejected
        //  -1 = outside rect entirely                        ← ignored
        //
        // Tie-break: highest pixel alpha = most "solid" sprite wins.

        PigPenZone bestZone = null;
        int bestScore = -1;
        float bestAlpha = -1f;

        foreach (PigPenZone zone in PigPenZone.AllZones)
        {
            if (zone == null || zone.IsClean || zone.IsCleaning) continue;

            if (!zone.PixelHit(e.position, uiCam, out float alpha)) continue;

            bool idOk = zone.AcceptToolID == toolID;
            bool taskOk = HasTaskID(zone.TaskID);
            int score = (idOk && taskOk) ? 2 : (idOk || taskOk) ? 1 : 0;

            if (score > bestScore || (score == bestScore && alpha > bestAlpha))
            {
                bestScore = score;
                bestAlpha = alpha;
                bestZone = zone;
            }
        }

        // Only clean on a full ID+task match
        bool cleaned = bestScore == 2 && bestZone != null && bestZone.TryClean(this);
        if (!cleaned) ReturnHome();
    }

    // ── Zone highlights ───────────────────────────────────────────────────────

    void UpdateZoneHighlights(Vector2 screenPos)
    {
        Camera uiCam = GetUICamera();
        foreach (PigPenZone zone in PigPenZone.AllZones)
        {
            if (zone == null) continue;
            bool hit = !zone.IsClean
                    && !zone.IsCleaning
                    && zone.AcceptToolID == toolID
                    && HasTaskID(zone.TaskID)
                    && zone.PixelHit(screenPos, uiCam, out _);   // pixel-accurate highlight
            zone.SetHighlight(hit);
        }
    }

    void ClearAllHighlights()
    {
        foreach (PigPenZone zone in PigPenZone.AllZones)
            if (zone != null) zone.SetHighlight(false);
    }

    // ── Animation ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by PigPenZone. Receives the zone's own waypoint path so the tool
    /// travels the correct route for that specific zone/task.
    /// </summary>
    public void StartToolAnimation(RectTransform[] animPath)
    {
        StopToolAnim();

        rt.localScale = new Vector3(toolAnimScale, toolAnimScale, 1f);

        if (animPath != null && animPath.Length > 0)
        {
            rt.anchoredPosition = WorldToCanvasPos(animPath[0].position);
            toolMove = StartCoroutine(MoveAlongPath(animPath));
        }

        if (toolFrames != null && toolFrames.Length > 0)
            toolAnim = StartCoroutine(PlayToolLoop());
    }

    Vector2 WorldToCanvasPos(Vector3 worldPos)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GetUICamera(), worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos, GetUICamera(), out Vector2 local);
        return local;
    }

    IEnumerator MoveAlongPath(RectTransform[] path)
    {
        for (int i = 1; i < path.Length; i++)
        {
            Vector2 from = rt.anchoredPosition;
            Vector2 to = WorldToCanvasPos(path[i].position);
            float dist = Vector2.Distance(from, to);
            float dur = dist / Mathf.Max(toolMoveSpeed, 1f);
            float t = 0f;

            while (t < dur)
            {
                t += Time.deltaTime;
                rt.anchoredPosition = Vector2.Lerp(from, to, t / dur);
                yield return null;
            }

            rt.anchoredPosition = to;
        }
    }

    IEnumerator PlayToolLoop()
    {
        float delay = 1f / Mathf.Max(toolFPS, 1f);
        int index = 0;
        while (true)
        {
            if (img == null) yield break;
            img.sprite = toolFrames[index % toolFrames.Length];
            index++;
            yield return new WaitForSeconds(delay);
        }
    }

    void StopToolAnim()
    {
        if (toolAnim != null) { StopCoroutine(toolAnim); toolAnim = null; }
        if (toolMove != null) { StopCoroutine(toolMove); toolMove = null; }
    }

    // ── Return home ───────────────────────────────────────────────────────────

    public void ReturnHome()
    {
        StopToolAnim();
        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
        if (cg != null) cg.blocksRaycasts = true;

        rt.localScale = homeLocalScale;

        transform.SetParent(homeParent);
        transform.SetSiblingIndex(homeSiblingIndex);

        StartCoroutine(SmoothGoHome());
    }

    IEnumerator SmoothGoHome()
    {
        float t = 0f, dur = 0.35f;
        Vector2 from = rt.anchoredPosition;
        while (t < dur)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(from, homeAnchoredPosition,
                                  Mathf.SmoothStep(0f, 1f, t / dur));
            yield return null;
        }
        rt.anchoredPosition = homeAnchoredPosition;
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    public void ResetTool()
    {
        StopToolAnim();
        if (img != null && defaultSprite != null) img.sprite = defaultSprite;
        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }
        rt.localScale = homeLocalScale;
        rt.anchoredPosition = homeAnchoredPosition;
        transform.SetParent(homeParent);
        transform.SetSiblingIndex(homeSiblingIndex);
    }

    public static void ResetAll()
    {
        allInstances.RemoveAll(t => t == null);
        foreach (var tool in allInstances) tool.ResetTool();
    }

    Camera GetUICamera()
    {
        if (rootCanvas == null) return null;
        return rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
               ? null : rootCanvas.worldCamera;
    }

    public int GetToolID() => toolID;

    /// <summary>Returns true if this tool is registered for the given taskID.</summary>
    public bool HasTaskID(int id)
    {
        if (taskIDs == null) return false;
        foreach (int t in taskIDs)
            if (t == id) return true;
        return false;
    }
}