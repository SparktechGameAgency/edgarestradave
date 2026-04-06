////////////using UnityEngine;
////////////using UnityEngine.UI;
////////////using System.Collections;
////////////using System.Collections.Generic;

////////////public class PigPenZone : MonoBehaviour
////////////{
////////////    [Header("Identity")]
////////////    public int acceptToolID = 0;

////////////    [Header("Zone Animation (plays ONCE)")]
////////////    public Sprite[] zoneFrames;
////////////    [Range(1f, 30f)]
////////////    public float zoneFPS = 6f;

////////////    [Header("Tool Animation Pose")]

////////////    [Header("Completion Panel")]
////////////    public GameObject allCleanPanel;

////////////    [Header("Pixel Detection")]
////////////    [Range(0f, 1f)]
////////////    public float alphaThreshold = 0.1f;

////////////    // ── Static registry ───────────────────────────────────────────────────────
////////////    private static readonly List<PigPenZone> allZones = new List<PigPenZone>();
////////////    public static IReadOnlyList<PigPenZone> AllZones => allZones;
////////////    private static int cleanedCount = 0;

////////////    // ── Private state ─────────────────────────────────────────────────────────
////////////    private RectTransform rt;
////////////    private Image img;
////////////    private Sprite defaultSprite;

////////////    private bool isCleaning;
////////////    private bool isClean;

////////////    public bool IsCleaning => isCleaning;
////////////    public bool IsClean => isClean;
////////////    public int AcceptToolID => acceptToolID;

////////////    // ── Lifecycle ─────────────────────────────────────────────────────────────

////////////    void Awake()
////////////    {
////////////        rt = GetComponent<RectTransform>();
////////////        img = GetComponent<Image>();
////////////        defaultSprite = img != null ? img.sprite : null;
////////////    }

////////////    // Clears statics on every Play-mode start (and domain reload).
////////////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
////////////    static void ClearStatics() { allZones.Clear(); cleanedCount = 0; }

////////////    void OnEnable() { if (!allZones.Contains(this)) allZones.Add(this); }
////////////    void OnDisable() { allZones.Remove(this); }
////////////    void OnDestroy() { allZones.Remove(this); }

////////////    // ── Hit testing ───────────────────────────────────────────────────────────

////////////    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
////////////    {
////////////        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
////////////            return false;

////////////        if (img == null || img.sprite == null) return true;

////////////        Sprite sprite = img.sprite;
////////////        Texture2D tex = sprite.texture;

////////////        if (!tex.isReadable)
////////////        {
////////////            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': texture '{tex.name}' is not " +
////////////                             "readable. Enable Read/Write in Import Settings.", this);
////////////            return true;
////////////        }

////////////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
////////////            rt, screenPos, uiCamera, out Vector2 local);

////////////        Rect r = rt.rect;
////////////        float nx = (local.x - r.x) / r.width;
////////////        float ny = (local.y - r.y) / r.height;

////////////        int px = Mathf.Clamp((int)(sprite.rect.x + nx * sprite.rect.width),
////////////                             (int)sprite.rect.x, (int)sprite.rect.xMax - 1);
////////////        int py = Mathf.Clamp((int)(sprite.rect.y + ny * sprite.rect.height),
////////////                             (int)sprite.rect.y, (int)sprite.rect.yMax - 1);

////////////        return tex.GetPixel(px, py).a >= alphaThreshold;
////////////    }

////////////    // ── Highlight ─────────────────────────────────────────────────────────────

////////////    public void SetHighlight(bool on)
////////////    {
////////////        if (img != null)
////////////            img.color = on ? new Color(1f, 1f, 0.5f) : Color.white;
////////////    }

////////////    // ── Cleaning ──────────────────────────────────────────────────────────────

////////////    public bool TryClean(PigPenTool tool)
////////////    {
////////////        if (isCleaning || isClean) return false;
////////////        if (tool.GetToolID() != acceptToolID) return false;

////////////        isCleaning = true;
////////////        StartCoroutine(CleanSequence(tool));
////////////        return true;
////////////    }

////////////    IEnumerator CleanSequence(PigPenTool tool)
////////////    {
////////////        tool.StartToolAnimation();

////////////        yield return StartCoroutine(PlayZoneOnce());

////////////        tool.ReturnHome();

////////////        isCleaning = false;
////////////        isClean = true;
////////////        cleanedCount++;

////////////        allZones.RemoveAll(z => z == null);

////////////        if (cleanedCount >= allZones.Count && allCleanPanel != null)
////////////            allCleanPanel.SetActive(true);
////////////    }

////////////    IEnumerator PlayZoneOnce()
////////////    {
////////////        if (zoneFrames == null || zoneFrames.Length == 0)
////////////        {
////////////            yield return new WaitForSeconds(2f);
////////////            yield break;
////////////        }

////////////        float delay = 1f / Mathf.Max(zoneFPS, 1f);
////////////        foreach (Sprite frame in zoneFrames)
////////////        {
////////////            if (img == null) yield break;
////////////            if (frame != null) img.sprite = frame;
////////////            yield return new WaitForSeconds(delay);
////////////        }
////////////    }

////////////    // ── Reset ─────────────────────────────────────────────────────────────────

////////////    public void ResetZone()
////////////    {
////////////        StopAllCoroutines();
////////////        isCleaning = false;
////////////        isClean = false;
////////////        cleanedCount = 0;

////////////        if (img != null)
////////////        {
////////////            img.sprite = defaultSprite;
////////////            img.color = Color.white;
////////////        }
////////////    }
////////////}

//////////using UnityEngine;
//////////using UnityEngine.UI;
//////////using System.Collections;
//////////using System.Collections.Generic;

//////////public class PigPenZone : MonoBehaviour
//////////{
//////////    [Header("Identity")]
//////////    public int acceptToolID = 0;
//////////    public int taskID = 0;          // matches PigPenTool.taskID to identify which task this zone belongs to

//////////    [Header("Zone Animation (plays ONCE)")]
//////////    public Sprite[] zoneFrames;
//////////    [Range(1f, 30f)]
//////////    public float zoneFPS = 6f;

//////////    [Header("Tool Animation Path")]
//////////    [Tooltip("Empty child RectTransforms placed in the Scene. The tool travels through them while cleaning this zone.")]
//////////    public RectTransform[] toolAnimPath;

//////////    [Header("Completion Panel")]
//////////    public GameObject allCleanPanel;

//////////    [Header("Pixel Detection")]
//////////    [Range(0f, 1f)]
//////////    public float alphaThreshold = 0.1f;

//////////    // ── Static registry ───────────────────────────────────────────────────────
//////////    private static readonly List<PigPenZone> allZones = new List<PigPenZone>();
//////////    public static IReadOnlyList<PigPenZone> AllZones => allZones;
//////////    private static int cleanedCount = 0;

//////////    // ── Private state ─────────────────────────────────────────────────────────
//////////    private RectTransform rt;
//////////    private Image img;
//////////    private Sprite defaultSprite;

//////////    private bool isCleaning;
//////////    private bool isClean;

//////////    public bool IsCleaning => isCleaning;
//////////    public bool IsClean => isClean;
//////////    public int AcceptToolID => acceptToolID;
//////////    public int TaskID => taskID;

//////////    // ── Lifecycle ─────────────────────────────────────────────────────────────

//////////    void Awake()
//////////    {
//////////        rt = GetComponent<RectTransform>();
//////////        img = GetComponent<Image>();
//////////        defaultSprite = img != null ? img.sprite : null;
//////////    }

//////////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//////////    static void ClearStatics() { allZones.Clear(); cleanedCount = 0; }

//////////    void OnEnable() { if (!allZones.Contains(this)) allZones.Add(this); }
//////////    void OnDisable() { allZones.Remove(this); }
//////////    void OnDestroy() { allZones.Remove(this); }

//////////    // ── Hit testing ───────────────────────────────────────────────────────────

//////////    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
//////////    {
//////////        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
//////////            return false;

//////////        if (img == null || img.sprite == null) return true;

//////////        Sprite sprite = img.sprite;
//////////        Texture2D tex = sprite.texture;

//////////        if (!tex.isReadable)
//////////        {
//////////            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': texture '{tex.name}' is not " +
//////////                             "readable. Enable Read/Write in Import Settings.", this);
//////////            return true;
//////////        }

//////////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//////////            rt, screenPos, uiCamera, out Vector2 local);

//////////        Rect r = rt.rect;
//////////        float nx = (local.x - r.x) / r.width;
//////////        float ny = (local.y - r.y) / r.height;

//////////        int px = Mathf.Clamp((int)(sprite.rect.x + nx * sprite.rect.width),
//////////                             (int)sprite.rect.x, (int)sprite.rect.xMax - 1);
//////////        int py = Mathf.Clamp((int)(sprite.rect.y + ny * sprite.rect.height),
//////////                             (int)sprite.rect.y, (int)sprite.rect.yMax - 1);

//////////        return tex.GetPixel(px, py).a >= alphaThreshold;
//////////    }

//////////    // ── Highlight ─────────────────────────────────────────────────────────────

//////////    public void SetHighlight(bool on)
//////////    {
//////////        if (img != null)
//////////            img.color = on ? new Color(1f, 1f, 0.5f) : Color.white;
//////////    }

//////////    // ── Cleaning ──────────────────────────────────────────────────────────────

//////////    public bool TryClean(PigPenTool tool)
//////////    {
//////////        if (isCleaning || isClean) return false;
//////////        if (tool.GetToolID() != acceptToolID) return false;
//////////        if (tool.GetTaskID() != taskID) return false;   // also verify taskID matches

//////////        isCleaning = true;
//////////        StartCoroutine(CleanSequence(tool));
//////////        return true;
//////////    }

//////////    IEnumerator CleanSequence(PigPenTool tool)
//////////    {
//////////        // Pass this zone's own path to the tool
//////////        tool.StartToolAnimation(toolAnimPath);

//////////        yield return StartCoroutine(PlayZoneOnce());

//////////        tool.ReturnHome();

//////////        isCleaning = false;
//////////        isClean = true;
//////////        cleanedCount++;

//////////        allZones.RemoveAll(z => z == null);

//////////        if (cleanedCount >= allZones.Count && allCleanPanel != null)
//////////            allCleanPanel.SetActive(true);
//////////    }

//////////    IEnumerator PlayZoneOnce()
//////////    {
//////////        if (zoneFrames == null || zoneFrames.Length == 0)
//////////        {
//////////            yield return new WaitForSeconds(2f);
//////////            yield break;
//////////        }

//////////        float delay = 1f / Mathf.Max(zoneFPS, 1f);
//////////        foreach (Sprite frame in zoneFrames)
//////////        {
//////////            if (img == null) yield break;
//////////            if (frame != null) img.sprite = frame;
//////////            yield return new WaitForSeconds(delay);
//////////        }
//////////    }

//////////    // ── Reset ─────────────────────────────────────────────────────────────────

//////////    public void ResetZone()
//////////    {
//////////        StopAllCoroutines();
//////////        isCleaning = false;
//////////        isClean = false;
//////////        cleanedCount = 0;

//////////        if (img != null)
//////////        {
//////////            img.sprite = defaultSprite;
//////////            img.color = Color.white;
//////////        }
//////////    }
//////////}

////////using UnityEngine;
////////using UnityEngine.UI;
////////using System.Collections;
////////using System.Collections.Generic;

////////public class PigPenZone : MonoBehaviour
////////{
////////    [Header("Identity")]
////////    public int acceptToolID = 0;
////////    public int taskID = 0;          // matches PigPenTool.taskID to identify which task this zone belongs to

////////    [Header("Zone Animation (plays ONCE)")]
////////    public Sprite[] zoneFrames;
////////    [Range(1f, 30f)]
////////    public float zoneFPS = 6f;

////////    [Header("Tool Animation Path")]
////////    [Tooltip("Empty child RectTransforms placed in the Scene. The tool travels through them while cleaning this zone.")]
////////    public RectTransform[] toolAnimPath;

////////    [Header("Completion Panel")]
////////    public GameObject allCleanPanel;

////////    [Header("Pixel Detection")]
////////    [Range(0f, 1f)]
////////    public float alphaThreshold = 0.1f;

////////    // ── Static registry ───────────────────────────────────────────────────────
////////    private static readonly List<PigPenZone> allZones = new List<PigPenZone>();
////////    public static IReadOnlyList<PigPenZone> AllZones => allZones;
////////    private static int cleanedCount = 0;

////////    // ── Private state ─────────────────────────────────────────────────────────
////////    private RectTransform rt;
////////    private Image img;
////////    private Sprite defaultSprite;

////////    private bool isCleaning;
////////    private bool isClean;

////////    public bool IsCleaning => isCleaning;
////////    public bool IsClean => isClean;
////////    public int AcceptToolID => acceptToolID;
////////    public int TaskID => taskID;

////////    // ── Lifecycle ─────────────────────────────────────────────────────────────

////////    void Awake()
////////    {
////////        rt = GetComponent<RectTransform>();
////////        img = GetComponent<Image>();
////////        defaultSprite = img != null ? img.sprite : null;
////////    }

////////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
////////    static void ClearStatics() { allZones.Clear(); cleanedCount = 0; }

////////    void OnEnable() { if (!allZones.Contains(this)) allZones.Add(this); }
////////    void OnDisable() { allZones.Remove(this); }
////////    void OnDestroy() { allZones.Remove(this); }

////////    // ── Hit testing ───────────────────────────────────────────────────────────

////////    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
////////    {
////////        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
////////            return false;

////////        if (img == null || img.sprite == null) return true;

////////        Sprite sprite = img.sprite;
////////        Texture2D tex = sprite.texture;

////////        if (!tex.isReadable)
////////        {
////////            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': texture '{tex.name}' is not " +
////////                             "readable. Enable Read/Write in Import Settings.", this);
////////            return true;
////////        }

////////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
////////            rt, screenPos, uiCamera, out Vector2 local);

////////        Rect r = rt.rect;
////////        float nx = (local.x - r.x) / r.width;
////////        float ny = (local.y - r.y) / r.height;

////////        int px = Mathf.Clamp((int)(sprite.rect.x + nx * sprite.rect.width),
////////                             (int)sprite.rect.x, (int)sprite.rect.xMax - 1);
////////        int py = Mathf.Clamp((int)(sprite.rect.y + ny * sprite.rect.height),
////////                             (int)sprite.rect.y, (int)sprite.rect.yMax - 1);

////////        return tex.GetPixel(px, py).a >= alphaThreshold;
////////    }

////////    // ── Highlight ─────────────────────────────────────────────────────────────

////////    public void SetHighlight(bool on)
////////    {
////////        if (img != null)
////////            img.color = on ? new Color(1f, 1f, 0.5f) : Color.white;
////////    }

////////    // ── Cleaning ──────────────────────────────────────────────────────────────

////////    public bool TryClean(PigPenTool tool)
////////    {
////////        if (isCleaning || isClean) return false;
////////        if (tool.GetToolID() != acceptToolID) return false;
////////        if (!tool.HasTaskID(taskID)) return false;   // tool must include this zone's taskID

////////        isCleaning = true;
////////        StartCoroutine(CleanSequence(tool));
////////        return true;
////////    }

////////    IEnumerator CleanSequence(PigPenTool tool)
////////    {
////////        // Pass this zone's own path to the tool
////////        tool.StartToolAnimation(toolAnimPath);

////////        yield return StartCoroutine(PlayZoneOnce());

////////        tool.ReturnHome();

////////        isCleaning = false;
////////        isClean = true;
////////        cleanedCount++;

////////        allZones.RemoveAll(z => z == null);

////////        if (cleanedCount >= allZones.Count && allCleanPanel != null)
////////            allCleanPanel.SetActive(true);
////////    }

////////    IEnumerator PlayZoneOnce()
////////    {
////////        if (zoneFrames == null || zoneFrames.Length == 0)
////////        {
////////            yield return new WaitForSeconds(2f);
////////            yield break;
////////        }

////////        float delay = 1f / Mathf.Max(zoneFPS, 1f);
////////        foreach (Sprite frame in zoneFrames)
////////        {
////////            if (img == null) yield break;
////////            if (frame != null) img.sprite = frame;
////////            yield return new WaitForSeconds(delay);
////////        }
////////    }

////////    // ── Reset ─────────────────────────────────────────────────────────────────

////////    public void ResetZone()
////////    {
////////        StopAllCoroutines();
////////        isCleaning = false;
////////        isClean = false;
////////        cleanedCount = 0;

////////        if (img != null)
////////        {
////////            img.sprite = defaultSprite;
////////            img.color = Color.white;
////////        }
////////    }
////////}

//////using UnityEngine;
//////using UnityEngine.UI;
//////using System.Collections;
//////using System.Collections.Generic;

//////public class PigPenZone : MonoBehaviour
//////{
//////    [Header("Identity")]
//////    public int acceptToolID = 0;
//////    public int taskID = 0;          // matches PigPenTool.taskID to identify which task this zone belongs to

//////    [Header("Zone Animation (plays ONCE)")]
//////    public Sprite[] zoneFrames;
//////    [Range(1f, 30f)]
//////    public float zoneFPS = 6f;

//////    [Header("Tool Animation Path")]
//////    [Tooltip("Empty child RectTransforms placed in the Scene. The tool travels through them while cleaning this zone.")]
//////    public RectTransform[] toolAnimPath;

//////    [Header("Completion Panel")]
//////    public GameObject allCleanPanel;

//////    [Header("Pixel Detection")]
//////    [Range(0f, 1f)]
//////    public float alphaThreshold = 0.1f;

//////    // ── Static registry ───────────────────────────────────────────────────────
//////    private static readonly List<PigPenZone> allZones = new List<PigPenZone>();
//////    public static IReadOnlyList<PigPenZone> AllZones => allZones;
//////    private static int cleanedCount = 0;

//////    // ── Private state ─────────────────────────────────────────────────────────
//////    private RectTransform rt;
//////    private Image img;
//////    private Sprite defaultSprite;

//////    private bool isCleaning;
//////    private bool isClean;

//////    public bool IsCleaning => isCleaning;
//////    public bool IsClean => isClean;
//////    public int AcceptToolID => acceptToolID;
//////    public int TaskID => taskID;

//////    // ── Lifecycle ─────────────────────────────────────────────────────────────

//////    void Awake()
//////    {
//////        rt = GetComponent<RectTransform>();
//////        img = GetComponent<Image>();
//////        defaultSprite = img != null ? img.sprite : null;
//////    }

//////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//////    static void ClearStatics() { allZones.Clear(); cleanedCount = 0; }

//////    void OnEnable() { if (!allZones.Contains(this)) allZones.Add(this); }
//////    void OnDisable() { allZones.Remove(this); }
//////    void OnDestroy() { allZones.Remove(this); }

//////    // ── Hit testing ───────────────────────────────────────────────────────────

//////    /// <summary>
//////    /// Returns true only when the screen point lands on a non-transparent pixel
//////    /// of this zone's sprite. <paramref name="pixelAlpha"/> is set to the sampled
//////    /// alpha (0 on any miss) so callers can compare overlap candidates.
//////    ///
//////    /// Unlike the old ContainsScreenPoint this method NEVER falls back to true —
//////    /// if the texture is unreadable it logs an error and returns false so that a
//////    /// zone with a broken import setting cannot accidentally accept a drop.
//////    /// </summary>
//////    public bool PixelHit(Vector2 screenPos, Camera uiCamera, out float pixelAlpha)
//////    {
//////        pixelAlpha = 0f;

//////        // 1. Coarse rect check first (cheap early-out)
//////        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
//////            return false;

//////        // 2. No image or sprite — treat the whole rect as solid
//////        if (img == null || img.sprite == null)
//////        {
//////            pixelAlpha = 1f;
//////            return true;
//////        }

//////        Sprite sprite = img.sprite;
//////        Texture2D tex = sprite.texture;

//////        // 3. Unreadable texture — refuse the hit and warn loudly
//////        if (!tex.isReadable)
//////        {
//////            Debug.LogError($"[PigPenZone] '{gameObject.name}': texture '{tex.name}' is NOT " +
//////                           "readable. Open the texture's Import Settings and enable " +
//////                           "Read/Write so pixel-accurate detection works.", this);
//////            return false;   // ← no silent fallback; zone is skipped entirely
//////        }

//////        // 4. Convert screen point to normalised sprite UV
//////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//////            rt, screenPos, uiCamera, out Vector2 local);

//////        Rect r = rt.rect;
//////        float nx = (local.x - r.x) / r.width;
//////        float ny = (local.y - r.y) / r.height;

//////        int px = Mathf.Clamp((int)(sprite.rect.x + nx * sprite.rect.width),
//////                             (int)sprite.rect.x, (int)sprite.rect.xMax - 1);
//////        int py = Mathf.Clamp((int)(sprite.rect.y + ny * sprite.rect.height),
//////                             (int)sprite.rect.y, (int)sprite.rect.yMax - 1);

//////        pixelAlpha = tex.GetPixel(px, py).a;
//////        return pixelAlpha >= alphaThreshold;
//////    }

//////    /// <summary>Legacy wrapper kept so any external code still compiles.</summary>
//////    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
//////        => PixelHit(screenPos, uiCamera, out _);

//////    // ── Highlight ─────────────────────────────────────────────────────────────

//////    public void SetHighlight(bool on)
//////    {
//////        if (img != null)
//////            img.color = on ? new Color(1f, 1f, 0.5f) : Color.white;
//////    }

//////    // ── Cleaning ──────────────────────────────────────────────────────────────

//////    public bool TryClean(PigPenTool tool)
//////    {
//////        if (isCleaning || isClean) return false;
//////        if (tool.GetToolID() != acceptToolID) return false;
//////        if (!tool.HasTaskID(taskID)) return false;   // tool must include this zone's taskID

//////        isCleaning = true;
//////        StartCoroutine(CleanSequence(tool));
//////        return true;
//////    }

//////    IEnumerator CleanSequence(PigPenTool tool)
//////    {
//////        // Pass this zone's own path to the tool
//////        tool.StartToolAnimation(toolAnimPath);

//////        yield return StartCoroutine(PlayZoneOnce());

//////        tool.ReturnHome();

//////        isCleaning = false;
//////        isClean = true;
//////        cleanedCount++;

//////        allZones.RemoveAll(z => z == null);

//////        if (cleanedCount >= allZones.Count && allCleanPanel != null)
//////            allCleanPanel.SetActive(true);
//////    }

//////    IEnumerator PlayZoneOnce()
//////    {
//////        if (zoneFrames == null || zoneFrames.Length == 0)
//////        {
//////            yield return new WaitForSeconds(2f);
//////            yield break;
//////        }

//////        float delay = 1f / Mathf.Max(zoneFPS, 1f);
//////        foreach (Sprite frame in zoneFrames)
//////        {
//////            if (img == null) yield break;
//////            if (frame != null) img.sprite = frame;
//////            yield return new WaitForSeconds(delay);
//////        }
//////    }

//////    // ── Reset ─────────────────────────────────────────────────────────────────

//////    public void ResetZone()
//////    {
//////        StopAllCoroutines();
//////        isCleaning = false;
//////        isClean = false;
//////        cleanedCount = 0;

//////        if (img != null)
//////        {
//////            img.sprite = defaultSprite;
//////            img.color = Color.white;
//////        }
//////    }
//////}

////using UnityEngine;
////using UnityEngine.UI;
////using System.Collections;
////using System.Collections.Generic;

////public class PigPenZone : MonoBehaviour
////{
////    [Header("Identity")]
////    public int acceptToolID = 0;
////    public int taskID = 0;

////    [Header("Zone Animation (plays ONCE)")]
////    public Sprite[] zoneFrames;
////    [Range(1f, 30f)]
////    public float zoneFPS = 6f;

////    [Header("Tool Animation Path")]
////    [Tooltip("Empty child RectTransforms placed in the Scene. The tool travels through them while cleaning this zone.")]
////    public RectTransform[] toolAnimPath;

////    [Header("Per-Zone Star Animation")]
////    [Tooltip("The star GameObject positioned at this zone (needs an Image component).")]
////    public GameObject starObject;
////    public Sprite[] starFrames;
////    [Range(1f, 30f)]
////    public float starFPS = 12f;
////    [Tooltip("How many times the star animation plays after THIS zone is cleaned.")]
////    [Min(1)]
////    public int starPlayCount = 2;

////    [Header("Final Star Animation (plays after ALL zones are cleaned)")]
////    [Tooltip("Star object for the all-clean celebration. Can be the same as the per-zone star or a different one.")]
////    public GameObject finalStarObject;
////    public Sprite[] finalStarFrames;
////    [Range(1f, 30f)]
////    public float finalStarFPS = 12f;
////    [Tooltip("How many times the final star animation plays before the All Clean panel appears.")]
////    [Min(1)]
////    public int finalStarPlayCount = 1;

////    [Header("Completion Panel")]
////    public GameObject allCleanPanel;

////    [Header("Pixel Detection")]
////    [Range(0f, 1f)]
////    public float alphaThreshold = 0.1f;

////    // ── Static registry ───────────────────────────────────────────────────────
////    private static readonly List<PigPenZone> allZones = new List<PigPenZone>();
////    public static IReadOnlyList<PigPenZone> AllZones => allZones;
////    private static int cleanedCount = 0;

////    // ── Private state ─────────────────────────────────────────────────────────
////    private RectTransform rt;
////    private Image img;
////    private Sprite defaultSprite;

////    private bool isCleaning;
////    private bool isClean;

////    public bool IsCleaning => isCleaning;
////    public bool IsClean => isClean;
////    public int AcceptToolID => acceptToolID;
////    public int TaskID => taskID;

////    // ── Lifecycle ─────────────────────────────────────────────────────────────

////    void Awake()
////    {
////        rt = GetComponent<RectTransform>();
////        img = GetComponent<Image>();
////        defaultSprite = img != null ? img.sprite : null;

////        // Stars start hidden
////        if (starObject != null) starObject.SetActive(false);
////        if (finalStarObject != null) finalStarObject.SetActive(false);
////    }

////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
////    static void ClearStatics() { allZones.Clear(); cleanedCount = 0; }

////    void OnEnable() { if (!allZones.Contains(this)) allZones.Add(this); }
////    void OnDisable() { allZones.Remove(this); }
////    void OnDestroy() { allZones.Remove(this); }

////    // ── Hit testing ───────────────────────────────────────────────────────────

////    /// <summary>
////    /// Returns true only when the screen point lands on a non-transparent pixel
////    /// of this zone's sprite. pixelAlpha is set to the sampled alpha (0 on miss)
////    /// so callers can compare overlapping candidates and pick the best hit.
////    /// Never silently falls back to true — unreadable textures log an error and
////    /// return false so a bad import setting cannot accidentally accept a drop.
////    /// </summary>
////    public bool PixelHit(Vector2 screenPos, Camera uiCamera, out float pixelAlpha)
////    {
////        pixelAlpha = 0f;

////        // 1. Coarse rect check (cheap early-out)
////        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
////            return false;

////        // 2. No image / sprite — treat the whole rect as solid
////        if (img == null || img.sprite == null)
////        {
////            pixelAlpha = 1f;
////            return true;
////        }

////        Sprite sprite = img.sprite;
////        Texture2D tex = sprite.texture;

////        // 3. Unreadable texture — refuse and warn loudly
////        if (!tex.isReadable)
////        {
////            Debug.LogError($"[PigPenZone] '{gameObject.name}': texture '{tex.name}' is NOT " +
////                           "readable. Enable Read/Write in its Texture Import Settings.", this);
////            return false;
////        }

////        // 4. Convert screen point → normalised sprite UV → pixel coords
////        RectTransformUtility.ScreenPointToLocalPointInRectangle(
////            rt, screenPos, uiCamera, out Vector2 local);

////        Rect r = rt.rect;
////        float nx = (local.x - r.x) / r.width;
////        float ny = (local.y - r.y) / r.height;

////        int px = Mathf.Clamp((int)(sprite.rect.x + nx * sprite.rect.width),
////                             (int)sprite.rect.x, (int)sprite.rect.xMax - 1);
////        int py = Mathf.Clamp((int)(sprite.rect.y + ny * sprite.rect.height),
////                             (int)sprite.rect.y, (int)sprite.rect.yMax - 1);

////        pixelAlpha = tex.GetPixel(px, py).a;
////        return pixelAlpha >= alphaThreshold;
////    }

////    /// <summary>Legacy wrapper so any external code still compiles.</summary>
////    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
////        => PixelHit(screenPos, uiCamera, out _);

////    // ── Highlight ─────────────────────────────────────────────────────────────

////    public void SetHighlight(bool on)
////    {
////        if (img != null)
////            img.color = on ? new Color(1f, 1f, 0.5f) : Color.white;
////    }

////    // ── Cleaning ──────────────────────────────────────────────────────────────

////    public bool TryClean(PigPenTool tool)
////    {
////        if (isCleaning || isClean) return false;
////        if (tool.GetToolID() != acceptToolID) return false;
////        if (!tool.HasTaskID(taskID)) return false;

////        isCleaning = true;
////        StartCoroutine(CleanSequence(tool));
////        return true;
////    }

////    IEnumerator CleanSequence(PigPenTool tool)
////    {
////        // 1. Run the tool animation + zone sprite animation in parallel
////        tool.StartToolAnimation(toolAnimPath);
////        yield return StartCoroutine(PlayZoneOnce());

////        // 2. Send the tool home
////        tool.ReturnHome();

////        // 3. Play the per-zone star animation N times
////        yield return StartCoroutine(PlayStarNTimes(starObject, starFrames, starFPS, starPlayCount));

////        // 4. Mark this zone done
////        isCleaning = false;
////        isClean = true;
////        cleanedCount++;

////        allZones.RemoveAll(z => z == null);

////        // 5. If every zone is clean, play the final star then show the panel
////        if (cleanedCount >= allZones.Count)
////        {
////            yield return StartCoroutine(
////                PlayStarNTimes(finalStarObject, finalStarFrames, finalStarFPS, finalStarPlayCount));

////            if (allCleanPanel != null)
////                allCleanPanel.SetActive(true);
////        }
////    }

////    // ── Zone sprite animation ─────────────────────────────────────────────────

////    IEnumerator PlayZoneOnce()
////    {
////        if (zoneFrames == null || zoneFrames.Length == 0)
////        {
////            yield return new WaitForSeconds(2f);
////            yield break;
////        }

////        float delay = 1f / Mathf.Max(zoneFPS, 1f);
////        foreach (Sprite frame in zoneFrames)
////        {
////            if (img == null) yield break;
////            if (frame != null) img.sprite = frame;
////            yield return new WaitForSeconds(delay);
////        }
////    }

////    // ── Star animation ────────────────────────────────────────────────────────

////    /// <summary>
////    /// Shows <paramref name="starGO"/>, plays its sprite sheet <paramref name="times"/>
////    /// times at <paramref name="fps"/> frames per second, then hides it again.
////    /// Safe to call even if any argument is null / empty.
////    /// </summary>
////    IEnumerator PlayStarNTimes(GameObject starGO, Sprite[] frames, float fps, int times)
////    {
////        // Nothing to play — skip instantly
////        if (starGO == null || frames == null || frames.Length == 0 || times <= 0)
////            yield break;

////        Image starImg = starGO.GetComponent<Image>();
////        if (starImg == null)
////        {
////            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': star object " +
////                             $"'{starGO.name}' has no Image component.", this);
////            yield break;
////        }

////        float delay = 1f / Mathf.Max(fps, 1f);

////        starGO.SetActive(true);

////        for (int i = 0; i < times; i++)
////        {
////            foreach (Sprite frame in frames)
////            {
////                if (frame != null) starImg.sprite = frame;
////                yield return new WaitForSeconds(delay);
////            }
////        }

////        starGO.SetActive(false);
////    }

////    // ── Reset ─────────────────────────────────────────────────────────────────

////    public void ResetZone()
////    {
////        StopAllCoroutines();
////        isCleaning = false;
////        isClean = false;
////        cleanedCount = 0;

////        if (img != null)
////        {
////            img.sprite = defaultSprite;
////            img.color = Color.white;
////        }

////        if (starObject != null) starObject.SetActive(false);
////        if (finalStarObject != null) finalStarObject.SetActive(false);
////    }
////}

//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;

//public class PigPenZone : MonoBehaviour
//{
//    [Header("Identity")]
//    public int acceptToolID = 0;
//    public int taskID = 0;

//    [Header("Zone Animation (plays ONCE)")]
//    public Sprite[] zoneFrames;
//    [Range(1f, 30f)]
//    public float zoneFPS = 6f;

//    [Header("Tool Animation Path")]
//    [Tooltip("Empty child RectTransforms placed in the Scene. The tool travels through them while cleaning this zone.")]
//    public RectTransform[] toolAnimPath;

//    [Header("Per-Zone Star Animation")]
//    [Tooltip("The star GameObject positioned at this zone (needs an Image component).")]
//    public GameObject starObject;
//    public Sprite[] starFrames;
//    [Range(1f, 30f)]
//    public float starFPS = 12f;
//    [Tooltip("How many times the star animation plays after THIS zone is cleaned.")]
//    [Min(1)]
//    public int starPlayCount = 2;

//    [Header("Final Star Animation (plays after ALL zones are cleaned)")]
//    [Tooltip("Star object for the all-clean celebration. Can be the same as the per-zone star or a different one.")]
//    public GameObject finalStarObject;
//    public Sprite[] finalStarFrames;
//    [Range(1f, 30f)]
//    public float finalStarFPS = 12f;
//    [Tooltip("How many times the final star animation plays before the All Clean panel appears.")]
//    [Min(1)]
//    public int finalStarPlayCount = 1;

//    [Header("Pig Reaction (shared across all zones)")]
//    [Tooltip("The pig GameObject (needs an Image component). Drag the same object into every zone.")]
//    public GameObject pigObject;
//    public Sprite[] pigFrames;
//    [Range(1f, 30f)]
//    public float pigFPS = 12f;
//    [Tooltip("How many times the pig animation loops while the chat box is visible.")]
//    [Min(1)]
//    public int pigLoopCount = 2;
//    [Tooltip("The chat box GameObject that shows 'Nice Job! Go ahead'. Drag the same object into every zone.")]
//    public GameObject pigChatBox;

//    [Header("Completion Panel")]
//    public GameObject allCleanPanel;

//    [Header("Pixel Detection")]
//    [Range(0f, 1f)]
//    public float alphaThreshold = 0.1f;

//    // ── Static registry ───────────────────────────────────────────────────────
//    private static readonly List<PigPenZone> allZones = new List<PigPenZone>();
//    public static IReadOnlyList<PigPenZone> AllZones => allZones;
//    private static int cleanedCount = 0;

//    // ── Private state ─────────────────────────────────────────────────────────
//    private RectTransform rt;
//    private Image img;
//    private Sprite defaultSprite;

//    private bool isCleaning;
//    private bool isClean;

//    public bool IsCleaning => isCleaning;
//    public bool IsClean => isClean;
//    public int AcceptToolID => acceptToolID;
//    public int TaskID => taskID;

//    // ── Lifecycle ─────────────────────────────────────────────────────────────

//    void Awake()
//    {
//        rt = GetComponent<RectTransform>();
//        img = GetComponent<Image>();
//        defaultSprite = img != null ? img.sprite : null;

//        // Everything starts hidden
//        if (starObject != null) starObject.SetActive(false);
//        if (finalStarObject != null) finalStarObject.SetActive(false);
//        if (pigObject != null) pigObject.SetActive(false);
//        if (pigChatBox != null) pigChatBox.SetActive(false);
//    }

//    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//    static void ClearStatics() { allZones.Clear(); cleanedCount = 0; }

//    void OnEnable() { if (!allZones.Contains(this)) allZones.Add(this); }
//    void OnDisable() { allZones.Remove(this); }
//    void OnDestroy() { allZones.Remove(this); }

//    // ── Hit testing ───────────────────────────────────────────────────────────

//    /// <summary>
//    /// Returns true only when the screen point lands on a non-transparent pixel
//    /// of this zone's sprite. pixelAlpha is set to the sampled alpha (0 on miss)
//    /// so callers can compare overlapping candidates and pick the best hit.
//    /// Never silently falls back to true — unreadable textures log an error and
//    /// return false so a bad import setting cannot accidentally accept a drop.
//    /// </summary>
//    public bool PixelHit(Vector2 screenPos, Camera uiCamera, out float pixelAlpha)
//    {
//        pixelAlpha = 0f;

//        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
//            return false;

//        if (img == null || img.sprite == null)
//        {
//            pixelAlpha = 1f;
//            return true;
//        }

//        Sprite sprite = img.sprite;
//        Texture2D tex = sprite.texture;

//        if (!tex.isReadable)
//        {
//            Debug.LogError($"[PigPenZone] '{gameObject.name}': texture '{tex.name}' is NOT " +
//                           "readable. Enable Read/Write in its Texture Import Settings.", this);
//            return false;
//        }

//        RectTransformUtility.ScreenPointToLocalPointInRectangle(
//            rt, screenPos, uiCamera, out Vector2 local);

//        Rect r = rt.rect;
//        float nx = (local.x - r.x) / r.width;
//        float ny = (local.y - r.y) / r.height;

//        int px = Mathf.Clamp((int)(sprite.rect.x + nx * sprite.rect.width),
//                             (int)sprite.rect.x, (int)sprite.rect.xMax - 1);
//        int py = Mathf.Clamp((int)(sprite.rect.y + ny * sprite.rect.height),
//                             (int)sprite.rect.y, (int)sprite.rect.yMax - 1);

//        pixelAlpha = tex.GetPixel(px, py).a;
//        return pixelAlpha >= alphaThreshold;
//    }

//    /// <summary>Legacy wrapper so any external code still compiles.</summary>
//    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
//        => PixelHit(screenPos, uiCamera, out _);

//    // ── Highlight ─────────────────────────────────────────────────────────────

//    public void SetHighlight(bool on)
//    {
//        if (img != null)
//            img.color = on ? new Color(1f, 1f, 0.5f) : Color.white;
//    }

//    // ── Cleaning ──────────────────────────────────────────────────────────────

//    public bool TryClean(PigPenTool tool)
//    {
//        if (isCleaning || isClean) return false;
//        if (tool.GetToolID() != acceptToolID) return false;
//        if (!tool.HasTaskID(taskID)) return false;

//        isCleaning = true;
//        StartCoroutine(CleanSequence(tool));
//        return true;
//    }

//    IEnumerator CleanSequence(PigPenTool tool)
//    {
//        // 1. Tool animation + zone sprite play
//        tool.StartToolAnimation(toolAnimPath);
//        yield return StartCoroutine(PlayZoneOnce());

//        // 2. Tool goes home
//        tool.ReturnHome();

//        // 3. Per-zone star  AND  pig reaction play at the same time.
//        //    We kick off both coroutines together and then wait for both to finish.
//        Coroutine starRoutine = StartCoroutine(
//            PlayStarNTimes(starObject, starFrames, starFPS, starPlayCount));

//        Coroutine pigRoutine = StartCoroutine(
//            PlayPigReaction());

//        // Wait for the longer of the two to finish
//        yield return starRoutine;
//        yield return pigRoutine;

//        // 4. Mark zone done
//        isCleaning = false;
//        isClean = true;
//        cleanedCount++;

//        allZones.RemoveAll(z => z == null);

//        // 5. All zones clean → final star → panel
//        if (cleanedCount >= allZones.Count)
//        {
//            yield return StartCoroutine(
//                PlayStarNTimes(finalStarObject, finalStarFrames, finalStarFPS, finalStarPlayCount));

//            if (allCleanPanel != null)
//                allCleanPanel.SetActive(true);
//        }
//    }

//    // ── Zone sprite animation ─────────────────────────────────────────────────

//    IEnumerator PlayZoneOnce()
//    {
//        if (zoneFrames == null || zoneFrames.Length == 0)
//        {
//            yield return new WaitForSeconds(2f);
//            yield break;
//        }

//        float delay = 1f / Mathf.Max(zoneFPS, 1f);
//        foreach (Sprite frame in zoneFrames)
//        {
//            if (img == null) yield break;
//            if (frame != null) img.sprite = frame;
//            yield return new WaitForSeconds(delay);
//        }
//    }

//    // ── Star animation ────────────────────────────────────────────────────────

//    IEnumerator PlayStarNTimes(GameObject starGO, Sprite[] frames, float fps, int times)
//    {
//        if (starGO == null || frames == null || frames.Length == 0 || times <= 0)
//            yield break;

//        Image starImg = starGO.GetComponent<Image>();
//        if (starImg == null)
//        {
//            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': star object " +
//                             $"'{starGO.name}' has no Image component.", this);
//            yield break;
//        }

//        float delay = 1f / Mathf.Max(fps, 1f);
//        starGO.SetActive(true);

//        for (int i = 0; i < times; i++)
//        {
//            foreach (Sprite frame in frames)
//            {
//                if (frame != null) starImg.sprite = frame;
//                yield return new WaitForSeconds(delay);
//            }
//        }

//        starGO.SetActive(false);
//    }

//    // ── Pig reaction ──────────────────────────────────────────────────────────

//    IEnumerator PlayPigReaction()
//    {
//        if (pigObject == null) yield break;

//        Image pigImg = pigObject.GetComponent<Image>();
//        if (pigImg == null)
//        {
//            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': pig object " +
//                             $"'{pigObject.name}' has no Image component.", this);
//            yield break;
//        }

//        // Show pig + chat box together
//        pigObject.SetActive(true);
//        if (pigChatBox != null) pigChatBox.SetActive(true);

//        // Play pig sprite sheet pigLoopCount times
//        if (pigFrames != null && pigFrames.Length > 0)
//        {
//            float delay = 1f / Mathf.Max(pigFPS, 1f);
//            for (int i = 0; i < pigLoopCount; i++)
//            {
//                foreach (Sprite frame in pigFrames)
//                {
//                    if (frame != null) pigImg.sprite = frame;
//                    yield return new WaitForSeconds(delay);
//                }
//            }
//        }

//        // Hide pig + chat box
//        pigObject.SetActive(false);
//        if (pigChatBox != null) pigChatBox.SetActive(false);
//    }

//    // ── Reset ─────────────────────────────────────────────────────────────────

//    public void ResetZone()
//    {
//        StopAllCoroutines();
//        isCleaning = false;
//        isClean = false;
//        cleanedCount = 0;

//        if (img != null)
//        {
//            img.sprite = defaultSprite;
//            img.color = Color.white;
//        }

//        if (starObject != null) starObject.SetActive(false);
//        if (finalStarObject != null) finalStarObject.SetActive(false);
//        if (pigObject != null) pigObject.SetActive(false);
//        if (pigChatBox != null) pigChatBox.SetActive(false);
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PigPenZone : MonoBehaviour
{
    [Header("Identity")]
    public int acceptToolID = 0;
    public int taskID = 0;

    [Header("Zone Animation (plays ONCE)")]
    public Sprite[] zoneFrames;
    [Range(1f, 30f)]
    public float zoneFPS = 6f;

    [Header("Tool Animation Path")]
    [Tooltip("Empty child RectTransforms placed in the Scene. The tool travels through them while cleaning this zone.")]
    public RectTransform[] toolAnimPath;

    [Header("Per-Zone Star Animation")]
    [Tooltip("The star GameObject positioned at this zone (needs an Image component).")]
    public GameObject starObject;
    public Sprite[] starFrames;
    [Range(1f, 30f)]
    public float starFPS = 12f;
    [Tooltip("How many times the star animation plays after THIS zone is cleaned.")]
    [Min(1)]
    public int starPlayCount = 2;

    [Header("Final Star Animation (plays after ALL zones are cleaned)")]
    [Tooltip("Star object for the all-clean celebration. Can be the same as the per-zone star or a different one.")]
    public GameObject finalStarObject;
    public Sprite[] finalStarFrames;
    [Range(1f, 30f)]
    public float finalStarFPS = 12f;
    [Tooltip("How many times the final star animation plays before the All Clean panel appears.")]
    [Min(1)]
    public int finalStarPlayCount = 1;

    [Header("Pig Reaction (shared across all zones — drag the same object into every zone)")]
    [Tooltip("The pig GameObject (needs an Image component).")]
    public GameObject pigObject;
    public Sprite[] pigFrames;
    [Range(1f, 30f)]
    public float pigFPS = 12f;
    [Tooltip("How many seconds the pig and chat box stay visible (the sprite sheet loops continuously for this duration).")]
    [Min(0.1f)]
    public float pigDisplayDuration = 3f;
    [Tooltip("The chat box GameObject showing 'Nice Job! Go ahead'.")]
    public GameObject pigChatBox;

    [Header("Completion Panel")]
    public GameObject allCleanPanel;

    [Header("Pixel Detection")]
    [Range(0f, 1f)]
    public float alphaThreshold = 0.1f;

    // ── Static registry ───────────────────────────────────────────────────────
    private static readonly List<PigPenZone> allZones = new List<PigPenZone>();
    public static IReadOnlyList<PigPenZone> AllZones => allZones;
    private static int cleanedCount = 0;

    // ── Private state ─────────────────────────────────────────────────────────
    private RectTransform rt;
    private Image img;
    private Sprite defaultSprite;

    private bool isCleaning;
    private bool isClean;

    public bool IsCleaning => isCleaning;
    public bool IsClean => isClean;
    public int AcceptToolID => acceptToolID;
    public int TaskID => taskID;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        defaultSprite = img != null ? img.sprite : null;

        if (starObject != null) starObject.SetActive(false);
        if (finalStarObject != null) finalStarObject.SetActive(false);
        if (pigObject != null) pigObject.SetActive(false);
        if (pigChatBox != null) pigChatBox.SetActive(false);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ClearStatics() { allZones.Clear(); cleanedCount = 0; }

    void OnEnable() { if (!allZones.Contains(this)) allZones.Add(this); }
    void OnDisable() { allZones.Remove(this); }
    void OnDestroy() { allZones.Remove(this); }

    // ── Hit testing ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true only when the screen point lands on a non-transparent pixel
    /// of this zone's sprite. pixelAlpha is set to the sampled alpha (0 on miss)
    /// so callers can compare overlapping candidates and pick the best hit.
    /// Never silently falls back to true — unreadable textures log an error and
    /// return false so a bad import setting cannot accidentally accept a drop.
    /// </summary>
    public bool PixelHit(Vector2 screenPos, Camera uiCamera, out float pixelAlpha)
    {
        pixelAlpha = 0f;

        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, uiCamera))
            return false;

        if (img == null || img.sprite == null)
        {
            pixelAlpha = 1f;
            return true;
        }

        Sprite sprite = img.sprite;
        Texture2D tex = sprite.texture;

        if (!tex.isReadable)
        {
            Debug.LogError($"[PigPenZone] '{gameObject.name}': texture '{tex.name}' is NOT " +
                           "readable. Enable Read/Write in its Texture Import Settings.", this);
            return false;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, screenPos, uiCamera, out Vector2 local);

        Rect r = rt.rect;
        float nx = (local.x - r.x) / r.width;
        float ny = (local.y - r.y) / r.height;

        int px = Mathf.Clamp((int)(sprite.rect.x + nx * sprite.rect.width),
                             (int)sprite.rect.x, (int)sprite.rect.xMax - 1);
        int py = Mathf.Clamp((int)(sprite.rect.y + ny * sprite.rect.height),
                             (int)sprite.rect.y, (int)sprite.rect.yMax - 1);

        pixelAlpha = tex.GetPixel(px, py).a;
        return pixelAlpha >= alphaThreshold;
    }

    /// <summary>Legacy wrapper so any external code still compiles.</summary>
    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
        => PixelHit(screenPos, uiCamera, out _);

    // ── Highlight ─────────────────────────────────────────────────────────────

    public void SetHighlight(bool on)
    {
        if (img != null)
            img.color = on ? new Color(1f, 1f, 0.5f) : Color.white;
    }

    // ── Cleaning ──────────────────────────────────────────────────────────────

    public bool TryClean(PigPenTool tool)
    {
        if (isCleaning || isClean) return false;
        if (tool.GetToolID() != acceptToolID) return false;
        if (!tool.HasTaskID(taskID)) return false;

        isCleaning = true;
        StartCoroutine(CleanSequence(tool));
        return true;
    }

    IEnumerator CleanSequence(PigPenTool tool)
    {
        // 1. Tool animation + zone sprite play together
        tool.StartToolAnimation(toolAnimPath);
        yield return StartCoroutine(PlayZoneOnce());

        // 2. Tool goes home
        tool.ReturnHome();

        // 3. Star and pig reaction run independently — neither waits for the other.
        //    We start both, then wait for each separately so they truly overlap
        //    and the zone only proceeds once BOTH are fully done.
        bool starDone = false;
        bool pigDone = false;

        StartCoroutine(PlayStarNTimes(starObject, starFrames, starFPS, starPlayCount,
                                      () => starDone = true));
        StartCoroutine(PlayPigReaction(() => pigDone = true));

        yield return new WaitUntil(() => starDone && pigDone);

        // 4. Mark zone done
        isCleaning = false;
        isClean = true;
        cleanedCount++;

        allZones.RemoveAll(z => z == null);

        // 5. All zones clean → final star → all-clean panel → congrats panel
        if (cleanedCount >= allZones.Count)
        {
            bool finalStarDone = false;
            StartCoroutine(PlayStarNTimes(finalStarObject, finalStarFrames, finalStarFPS,
                                          finalStarPlayCount, () => finalStarDone = true));
            yield return new WaitUntil(() => finalStarDone);

            if (allCleanPanel != null)
                allCleanPanel.SetActive(true);

            // Tell GameManager to show the congratulations panel
            if (GameManager.Instance != null)
                GameManager.Instance.ShowPigPenCongrats();
        }
    }

    // ── Zone sprite animation ─────────────────────────────────────────────────

    IEnumerator PlayZoneOnce()
    {
        if (zoneFrames == null || zoneFrames.Length == 0)
        {
            yield return new WaitForSeconds(2f);
            yield break;
        }

        float delay = 1f / Mathf.Max(zoneFPS, 1f);
        foreach (Sprite frame in zoneFrames)
        {
            if (img == null) yield break;
            if (frame != null) img.sprite = frame;
            yield return new WaitForSeconds(delay);
        }
    }

    // ── Star animation ────────────────────────────────────────────────────────

    /// <summary>
    /// Shows the star, plays its sprite sheet <paramref name="times"/> times,
    /// hides it, then invokes <paramref name="onDone"/> so the caller knows
    /// this independent coroutine has finished.
    /// </summary>
    IEnumerator PlayStarNTimes(GameObject starGO, Sprite[] frames, float fps,
                               int times, System.Action onDone = null)
    {
        if (starGO == null || frames == null || frames.Length == 0 || times <= 0)
        {
            onDone?.Invoke();
            yield break;
        }

        Image starImg = starGO.GetComponent<Image>();
        if (starImg == null)
        {
            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': star object " +
                             $"'{starGO.name}' has no Image component.", this);
            onDone?.Invoke();
            yield break;
        }

        float delay = 1f / Mathf.Max(fps, 1f);
        starGO.SetActive(true);

        for (int i = 0; i < times; i++)
            foreach (Sprite frame in frames)
            {
                if (frame != null) starImg.sprite = frame;
                yield return new WaitForSeconds(delay);
            }

        starGO.SetActive(false);
        onDone?.Invoke();
    }

    // ── Pig reaction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Shows the pig + chat box, loops the sprite sheet continuously for
    /// <see cref="pigDisplayDuration"/> seconds, hides both, then invokes <paramref name="onDone"/>.
    /// Runs independently of the star — neither waits for the other.
    /// </summary>
    IEnumerator PlayPigReaction(System.Action onDone = null)
    {
        if (pigObject == null)
        {
            onDone?.Invoke();
            yield break;
        }

        Image pigImg = pigObject.GetComponent<Image>();
        if (pigImg == null)
        {
            Debug.LogWarning($"[PigPenZone] '{gameObject.name}': pig object " +
                             $"'{pigObject.name}' has no Image component.", this);
            onDone?.Invoke();
            yield break;
        }

        pigObject.SetActive(true);
        if (pigChatBox != null) pigChatBox.SetActive(true);

        if (pigFrames != null && pigFrames.Length > 0)
        {
            float delay = 1f / Mathf.Max(pigFPS, 1f);
            float elapsed = 0f;
            int frame = 0;
            while (elapsed < pigDisplayDuration)
            {
                if (pigFrames[frame % pigFrames.Length] != null)
                    pigImg.sprite = pigFrames[frame % pigFrames.Length];
                frame++;
                yield return new WaitForSeconds(delay);
                elapsed += delay;
            }
        }

        pigObject.SetActive(false);
        if (pigChatBox != null) pigChatBox.SetActive(false);

        onDone?.Invoke();
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    public void ResetZone()
    {
        StopAllCoroutines();
        isCleaning = false;
        isClean = false;
        cleanedCount = 0;

        if (img != null)
        {
            img.sprite = defaultSprite;
            img.color = Color.white;
        }

        if (starObject != null) starObject.SetActive(false);
        if (finalStarObject != null) finalStarObject.SetActive(false);
        if (pigObject != null) pigObject.SetActive(false);
        if (pigChatBox != null) pigChatBox.SetActive(false);
    }
}