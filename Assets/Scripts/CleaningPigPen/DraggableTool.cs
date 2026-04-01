//////using System.Collections;
//////using UnityEngine;
//////using UnityEngine.EventSystems;
//////using UnityEngine.UI;

//////// ══════════════════════════════════════════════════════════════════════════════
//////// DraggableTool.cs
//////// Attach this to: Broom, Hammer, Brush, Shovel  (the toolbar icons)
////////
//////// INSPECTOR FIELDS:
////////   toolID          → a number you choose: 1=Broom 2=Hammer 3=Brush 4=Shovel
////////   rootCanvas      → drag your Canvas here
////////   toolAnimation   → the scene GameObject that shows the tool animating
////////                     (e.g. the Broom swinging in the scene)
////////                     needs a legacy Animation component — starts INACTIVE
////////   toolClipName    → exact name of the clip on toolAnimation
////////
//////// BEHAVIOUR:
////////   • Drop on a matching ToolDropZone  → zone cleaning sequence runs,
////////     tool loops while zone animates, then both stop and tool returns to bar.
////////   • Drop ANYWHERE else               → tool animation plays ONCE,
////////     then icon snaps back to the bar, ready to use again.
//////// ══════════════════════════════════════════════════════════════════════════════

//////[RequireComponent(typeof(Image))]
//////[RequireComponent(typeof(CanvasGroup))]
//////public class DraggableTool : MonoBehaviour,
//////    IBeginDragHandler, IDragHandler, IEndDragHandler,
//////    IPointerEnterHandler, IPointerExitHandler
//////{
//////    [Header("Identity")]
//////    [Tooltip("Give each tool a unique number. e.g. 1=Broom 2=Hammer 3=Brush 4=Shovel")]
//////    public int toolID;

//////    [Header("References")]
//////    public Canvas rootCanvas;

//////    [Header("Tool Scene Animation")]
//////    [Tooltip("The scene GameObject with a legacy Animation component.\nMust start INACTIVE.")]
//////    public GameObject toolAnimation;

//////    [Tooltip("Exact clip name on the Animation component above.")]
//////    public string toolClipName;

//////    // ── internals ─────────────────────────────────────────────────────────
//////    private RectTransform _rect;
//////    private CanvasGroup _cg;

//////    private Vector2 _homePos;
//////    private Transform _homeParent;
//////    private int _homeSibling;

//////    // isAnimating is read by ToolDropZone too (HideInInspector keeps it out of Inspector clutter)
//////    [HideInInspector] public bool isAnimating = false;
//////    private bool _isDragging = false;

//////    // ─────────────────────────────────────────────────────────────────────
//////    void Awake()
//////    {
//////        _rect = GetComponent<RectTransform>();
//////        _cg = GetComponent<CanvasGroup>();

//////        if (rootCanvas == null)
//////            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
//////    }

//////    void Start()
//////    {
//////        _homePos = _rect.anchoredPosition;
//////        _homeParent = transform.parent;
//////        _homeSibling = transform.GetSiblingIndex();

//////        // Hide scene tool at start — shown only while animating
//////        if (toolAnimation != null)
//////            toolAnimation.SetActive(false);
//////    }

//////    // ── Hover highlight ───────────────────────────────────────────────────
//////    public void OnPointerEnter(PointerEventData _)
//////    {
//////        if (isAnimating || _isDragging) return;
//////        transform.localScale = Vector3.one * 1.1f;
//////    }

//////    public void OnPointerExit(PointerEventData _)
//////    {
//////        if (_isDragging) return;
//////        transform.localScale = Vector3.one;
//////    }

//////    // ── Drag ──────────────────────────────────────────────────────────────
//////    public void OnBeginDrag(PointerEventData e)
//////    {
//////        if (isAnimating) return;   // block drag while playing

//////        _isDragging = true;
//////        transform.localScale = Vector3.one;

//////        // Move to top of canvas so it renders above everything else
//////        transform.SetParent(rootCanvas.transform, true);
//////        _cg.alpha = 0.85f;
//////        _cg.blocksRaycasts = false;   // lets pointer hit drop zones beneath

//////        // Light up matching drop zones
//////        ToolDropZone.ShowHints(toolID, true);
//////    }

//////    public void OnDrag(PointerEventData e)
//////    {
//////        if (!_isDragging) return;
//////        _rect.anchoredPosition += e.delta / rootCanvas.scaleFactor;
//////    }

//////    public void OnEndDrag(PointerEventData e)
//////    {
//////        if (!_isDragging) return;
//////        _isDragging = false;

//////        ToolDropZone.ShowHints(toolID, false);
//////        _cg.blocksRaycasts = true;

//////        // ── KEY FIX ──────────────────────────────────────────────────────
//////        // If a ToolDropZone already claimed this drop it will have called
//////        // PlayToolAnim() which sets isAnimating = true before OnEndDrag runs.
//////        // In that case we do nothing here — the zone's RunSequence owns the
//////        // lifecycle.
//////        //
//////        // If isAnimating is still false nobody claimed the drop, so we play
//////        // the tool animation ONCE ourselves and then return to the bar.
//////        // ─────────────────────────────────────────────────────────────────
//////        if (!isAnimating)
//////            StartCoroutine(PlayOnceAndReturn());
//////    }

//////    // ── Called by ToolDropZone on a CORRECT drop ─────────────────────────
//////    // Loops the tool animation while the zone's cleaning animation plays.
//////    public void PlayToolAnim()
//////    {
//////        isAnimating = true;

//////        // Put icon back in toolbar, dim it while busy
//////        ReturnHome();
//////        _cg.alpha = 0.4f;
//////        _cg.interactable = false;
//////        _cg.blocksRaycasts = false;
//////        transform.localScale = Vector3.one;

//////        if (toolAnimation != null)
//////        {
//////            toolAnimation.SetActive(true);
//////            Animation anim = toolAnimation.GetComponent<Animation>();
//////            if (anim != null && !string.IsNullOrEmpty(toolClipName))
//////            {
//////                anim[toolClipName].wrapMode = WrapMode.Loop;
//////                anim.Play(toolClipName);
//////            }
//////        }
//////    }

//////    // ── Called by ToolDropZone when its zone animation finishes ──────────
//////    // Stops the looping tool animation and fully restores the icon.
//////    public void StopToolAnim()
//////    {
//////        isAnimating = false;

//////        if (toolAnimation != null)
//////        {
//////            Animation anim = toolAnimation.GetComponent<Animation>();
//////            if (anim != null) anim.Stop();
//////            toolAnimation.SetActive(false);
//////        }

//////        // Fully restore — player can pick it up again
//////        _cg.alpha = 1f;
//////        _cg.interactable = true;
//////        _cg.blocksRaycasts = true;
//////        transform.localScale = Vector3.one;
//////    }

//////    // ── Plays animation ONCE then returns icon to bar ─────────────────────
//////    // Used when the tool is dropped somewhere that is NOT a ToolDropZone.
//////    IEnumerator PlayOnceAndReturn()
//////    {
//////        isAnimating = true;

//////        // Snap icon back to toolbar and dim it while animating
//////        ReturnHome();
//////        _cg.alpha = 0.4f;
//////        _cg.interactable = false;
//////        _cg.blocksRaycasts = false;
//////        transform.localScale = Vector3.one;

//////        float clipLength = 0f;

//////        if (toolAnimation != null)
//////        {
//////            toolAnimation.SetActive(true);
//////            Animation anim = toolAnimation.GetComponent<Animation>();

//////            if (anim != null && !string.IsNullOrEmpty(toolClipName))
//////            {
//////                AnimationClip clip = anim.GetClip(toolClipName);
//////                if (clip != null) clipLength = clip.length;

//////                anim[toolClipName].wrapMode = WrapMode.Once;
//////                anim.Play(toolClipName);
//////            }
//////        }

//////        // Wait for the full animation to finish
//////        yield return new WaitForSeconds(clipLength);

//////        // Hide tool scene object
//////        if (toolAnimation != null)
//////        {
//////            Animation anim = toolAnimation.GetComponent<Animation>();
//////            if (anim != null) anim.Stop();
//////            toolAnimation.SetActive(false);
//////        }

//////        // Restore icon fully — ready to use again
//////        isAnimating = false;
//////        _cg.alpha = 1f;
//////        _cg.interactable = true;
//////        _cg.blocksRaycasts = true;
//////        transform.localScale = Vector3.one;
//////    }

//////    // ── Reset (called by GameManager "Play Again") ────────────────────────
//////    public void ResetTool()
//////    {
//////        StopAllCoroutines();
//////        isAnimating = false;
//////        _isDragging = false;

//////        if (toolAnimation != null)
//////        {
//////            Animation anim = toolAnimation.GetComponent<Animation>();
//////            if (anim != null) anim.Stop();
//////            toolAnimation.SetActive(false);
//////        }

//////        ReturnHome();
//////        _cg.alpha = 1f;
//////        _cg.interactable = true;
//////        _cg.blocksRaycasts = true;
//////        transform.localScale = Vector3.one;
//////    }

//////    // ── Helpers ───────────────────────────────────────────────────────────
//////    void ReturnHome()
//////    {
//////        transform.SetParent(_homeParent, true);
//////        transform.SetSiblingIndex(_homeSibling);
//////        _rect.anchoredPosition = _homePos;
//////    }
//////}

////using System.Collections;
////using UnityEngine;
////using UnityEngine.EventSystems;
////using UnityEngine.UI;

////// ══════════════════════════════════════════════════════════════════════════════
////// DraggableTool.cs
//////
////// Attach to each toolbar tool (Hammer, Broom, Brush, Shovel).
////// The Animation component must also be on THIS same GameObject.
//////
////// INSPECTOR FIELDS:
//////   toolID       → unique number per tool  (1=Broom, 2=Hammer, 3=Brush, 4=Shovel)
//////   rootCanvas   → drag your root Canvas here
//////   clipName     → exact name of the animation clip on this GameObject
//////
////// BEHAVIOUR:
//////   Drag tool → drop anywhere → animation plays once → icon returns to bar.
//////   Drop on a matching ToolDropZone → zone cleaning also runs at the same time.
////// ══════════════════════════════════════════════════════════════════════════════

////[RequireComponent(typeof(Image))]
////[RequireComponent(typeof(CanvasGroup))]
////public class DraggableTool : MonoBehaviour,
////    IBeginDragHandler, IDragHandler, IEndDragHandler,
////    IPointerEnterHandler, IPointerExitHandler
////{
////    [Header("Identity")]
////    public int toolID;

////    [Header("References")]
////    public Canvas rootCanvas;

////    [Header("Animation")]
////    [Tooltip("Exact name of the animation clip on THIS GameObject's Animation component.")]
////    public string clipName;

////    // ── private ───────────────────────────────────────────────────────────
////    private RectTransform _rect;
////    private CanvasGroup _cg;
////    private Animation _anim;

////    private Vector2 _homePos;
////    private Transform _homeParent;
////    private int _homeSibling;

////    [HideInInspector] public bool isAnimating = false;
////    private bool _isDragging = false;

////    // ─────────────────────────────────────────────────────────────────────
////    void Awake()
////    {
////        _rect = GetComponent<RectTransform>();
////        _cg = GetComponent<CanvasGroup>();
////        _anim = GetComponent<Animation>();   // Animation lives on this same object
////    }

////    void Start()
////    {
////        _homePos = _rect.anchoredPosition;
////        _homeParent = transform.parent;
////        _homeSibling = transform.GetSiblingIndex();
////    }

////    // ── Hover ─────────────────────────────────────────────────────────────
////    public void OnPointerEnter(PointerEventData _)
////    {
////        if (isAnimating || _isDragging) return;
////        transform.localScale = Vector3.one * 1.1f;
////    }

////    public void OnPointerExit(PointerEventData _)
////    {
////        if (_isDragging) return;
////        transform.localScale = Vector3.one;
////    }

////    // ── Drag ──────────────────────────────────────────────────────────────
////    public void OnBeginDrag(PointerEventData e)
////    {
////        if (isAnimating) return;

////        _isDragging = true;
////        transform.localScale = Vector3.one;

////        transform.SetParent(rootCanvas.transform, true);
////        _cg.alpha = 0.85f;
////        _cg.blocksRaycasts = false;   // so the pointer can hit drop zones underneath

////        ToolDropZone.ShowHints(toolID, true);
////    }

////    public void OnDrag(PointerEventData e)
////    {
////        if (!_isDragging) return;
////        _rect.anchoredPosition += e.delta / rootCanvas.scaleFactor;
////    }

////    public void OnEndDrag(PointerEventData e)
////    {
////        if (!_isDragging) return;
////        _isDragging = false;

////        ToolDropZone.ShowHints(toolID, false);
////        _cg.blocksRaycasts = true;

////        // Always play the animation on drop — whether it landed on a zone or not.
////        // ToolDropZone.OnDrop calls PlayToolAnim() first (before OnEndDrag fires),
////        // so isAnimating will already be true if a zone claimed it — we skip here
////        // to avoid double-starting the coroutine.
////        if (!isAnimating)
////            StartCoroutine(PlayOnceAndReturn());
////    }

////    // ── Called by ToolDropZone on a correct drop ──────────────────────────
////    public void PlayToolAnim()
////    {
////        if (isAnimating) return;
////        StartCoroutine(PlayOnceAndReturn());
////    }

////    // ── Core routine ──────────────────────────────────────────────────────
////    IEnumerator PlayOnceAndReturn()
////    {
////        isAnimating = true;

////        // Snap icon back to its home slot while animating
////        ReturnHome();
////        _cg.alpha = 0.4f;
////        _cg.interactable = false;
////        _cg.blocksRaycasts = false;
////        transform.localScale = Vector3.one;

////        // Play the clip once and wait for it to finish
////        float clipLength = 0f;
////        if (_anim != null && !string.IsNullOrEmpty(clipName))
////        {
////            AnimationClip clip = _anim.GetClip(clipName);
////            if (clip != null)
////            {
////                clipLength = clip.length;
////                _anim[clipName].wrapMode = WrapMode.Once;
////                _anim.Play(clipName);
////            }
////        }

////        yield return new WaitForSeconds(clipLength);

////        if (_anim != null) _anim.Stop();

////        // Restore icon — ready to drag again
////        isAnimating = false;
////        _cg.alpha = 1f;
////        _cg.interactable = true;
////        _cg.blocksRaycasts = true;
////        transform.localScale = Vector3.one;
////    }

////    // ── Called by ToolDropZone after zone animation finishes ──────────────
////    // The coroutine already handles its own cleanup, so nothing extra needed.
////    public void StopToolAnim() { }

////    // ── Reset ─────────────────────────────────────────────────────────────
////    public void ResetTool()
////    {
////        StopAllCoroutines();
////        isAnimating = false;
////        _isDragging = false;

////        if (_anim != null) _anim.Stop();

////        ReturnHome();
////        _cg.alpha = 1f;
////        _cg.interactable = true;
////        _cg.blocksRaycasts = true;
////        transform.localScale = Vector3.one;
////    }

////    // ── Helpers ───────────────────────────────────────────────────────────
////    void ReturnHome()
////    {
////        transform.SetParent(_homeParent, true);
////        transform.SetSiblingIndex(_homeSibling);
////        _rect.anchoredPosition = _homePos;
////    }
////}

//using System.Collections;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//// ══════════════════════════════════════════════════════════════════════════════
//// DraggableTool.cs
////
//// Attach to each toolbar tool (Hammer, Broom, Brush, Shovel).
//// The legacy Animation component must be on THIS same GameObject.
////
//// INSPECTOR FIELDS:
////   toolID     → unique number per tool (e.g. 1=Broom, 2=Hammer, 3=Brush, 4=Shovel)
////   rootCanvas → drag your root Canvas here
////   clipName   → exact name of the animation clip on this GameObject
////
//// BEHAVIOUR:
////   • Drop on the MATCHING ToolDropZone → animation plays once → icon returns to bar.
////   • Drop anywhere else (wrong zone / empty space) → snaps straight back, no animation.
//// ══════════════════════════════════════════════════════════════════════════════

//[RequireComponent(typeof(Image))]
//[RequireComponent(typeof(CanvasGroup))]
//public class DraggableTool : MonoBehaviour,
//    IBeginDragHandler, IDragHandler, IEndDragHandler,
//    IPointerEnterHandler, IPointerExitHandler
//{
//    [Header("Identity")]
//    public int toolID;

//    [Header("References")]
//    public Canvas rootCanvas;

//    [Header("Animation")]
//    [Tooltip("Exact name of the clip on THIS GameObject's Animation component.")]
//    public string clipName;

//    // ── private ───────────────────────────────────────────────────────────
//    private RectTransform _rect;
//    private CanvasGroup _cg;
//    private Animation _anim;

//    private Vector2 _homePos;
//    private Transform _homeParent;
//    private int _homeSibling;

//    [HideInInspector] public bool isAnimating = false;
//    private bool _isDragging = false;

//    // ─────────────────────────────────────────────────────────────────────
//    void Awake()
//    {
//        _rect = GetComponent<RectTransform>();
//        _cg = GetComponent<CanvasGroup>();
//        _anim = GetComponent<Animation>();
//    }

//    void Start()
//    {
//        _homePos = _rect.anchoredPosition;
//        _homeParent = transform.parent;
//        _homeSibling = transform.GetSiblingIndex();
//    }

//    // ── Hover ─────────────────────────────────────────────────────────────
//    public void OnPointerEnter(PointerEventData _)
//    {
//        if (isAnimating || _isDragging) return;
//        transform.localScale = Vector3.one * 1.1f;
//    }

//    public void OnPointerExit(PointerEventData _)
//    {
//        if (_isDragging) return;
//        transform.localScale = Vector3.one;
//    }

//    // ── Drag ──────────────────────────────────────────────────────────────
//    public void OnBeginDrag(PointerEventData e)
//    {
//        if (isAnimating) return;

//        _isDragging = true;
//        transform.localScale = Vector3.one;

//        // Move to top of canvas so it renders above everything
//        transform.SetParent(rootCanvas.transform, true);
//        _cg.alpha = 0.85f;
//        _cg.blocksRaycasts = false; // lets pointer reach the drop zone underneath

//        ToolDropZone.ShowHints(toolID, true);
//    }

//    public void OnDrag(PointerEventData e)
//    {
//        if (!_isDragging) return;
//        _rect.anchoredPosition += e.delta / rootCanvas.scaleFactor;
//    }

//    public void OnEndDrag(PointerEventData e)
//    {
//        if (!_isDragging) return;
//        _isDragging = false;

//        ToolDropZone.ShowHints(toolID, false);
//        _cg.blocksRaycasts = true;

//        // If a ToolDropZone already claimed this drop it will have called
//        // PlayToolAnim(), setting isAnimating = true before OnEndDrag fires.
//        // In that case do nothing — the zone owns the rest of the sequence.
//        // Otherwise snap straight back to the bar.
//        if (!isAnimating)
//            SnapHome();
//    }

//    // ── Called by ToolDropZone when the correct tool is dropped ──────────
//    public void PlayToolAnim()
//    {
//        if (isAnimating) return;
//        StartCoroutine(PlayOnceAndReturn());
//    }

//    // ── Plays the animation once then fully restores the icon ─────────────
//    IEnumerator PlayOnceAndReturn()
//    {
//        isAnimating = true;

//        // Return icon to toolbar slot and dim it while busy
//        ReturnHome();
//        _cg.alpha = 0.4f;
//        _cg.interactable = false;
//        _cg.blocksRaycasts = false;
//        transform.localScale = Vector3.one;

//        // Play clip once and wait for it to finish
//        float clipLength = 0f;
//        if (_anim != null && !string.IsNullOrEmpty(clipName))
//        {
//            AnimationClip clip = _anim.GetClip(clipName);
//            if (clip != null)
//            {
//                clipLength = clip.length;
//                _anim[clipName].wrapMode = WrapMode.Once;
//                _anim.Play(clipName);
//            }
//        }

//        yield return new WaitForSeconds(clipLength);

//        if (_anim != null) _anim.Stop();

//        // Fully restore — ready to use again
//        isAnimating = false;
//        _cg.alpha = 1f;
//        _cg.interactable = true;
//        _cg.blocksRaycasts = true;
//        transform.localScale = Vector3.one;
//    }

//    // ── No-op kept so ToolDropZone.StopToolAnim() calls still compile ─────
//    public void StopToolAnim() { }

//    // ── Reset (GameManager "Play Again") ──────────────────────────────────
//    public void ResetTool()
//    {
//        StopAllCoroutines();
//        isAnimating = false;
//        _isDragging = false;

//        if (_anim != null) _anim.Stop();

//        ReturnHome();
//        _cg.alpha = 1f;
//        _cg.interactable = true;
//        _cg.blocksRaycasts = true;
//        transform.localScale = Vector3.one;
//    }

//    // ── Helpers ───────────────────────────────────────────────────────────
//    void SnapHome()
//    {
//        ReturnHome();
//        _cg.alpha = 1f;
//        _cg.interactable = true;
//        _cg.blocksRaycasts = true;
//        transform.localScale = Vector3.one;
//    }

//    void ReturnHome()
//    {
//        transform.SetParent(_homeParent, true);
//        transform.SetSiblingIndex(_homeSibling);
//        _rect.anchoredPosition = _homePos;
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ══════════════════════════════════════════════════════════════════════════════
// DraggableTool.cs
//
// Attach to each toolbar tool (Hammer, Broom, Brush, Shovel).
// The legacy Animation component must be on THIS same GameObject.
//
// INSPECTOR FIELDS:
//   toolID           → unique number per tool (e.g. 1=Broom, 2=Hammer, 3=Brush, 4=Shovel)
//   rootCanvas       → drag your root Canvas here
//   clipName         → exact name of the animation clip on this GameObject
//   toolSceneAnim    → the scene GameObject that shows the tool animating
//                      (e.g. Hammeranim, Broomanim). Needs a legacy Animation
//                      component. Must start INACTIVE.
//   toolSceneClipName → exact clip name on toolSceneAnim
//
// BEHAVIOUR:
//   • Drop on the MATCHING ToolDropZone → BOTH the toolbar icon animation AND
//     the scene tool animation play together while the zone cleans.
//     When the zone finishes, both animations stop and the icon returns to the bar.
//   • Drop anywhere else (wrong zone / empty space) → snaps straight back, no animation.
// ══════════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableTool : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Identity")]
    public int toolID;

    [Header("References")]
    public Canvas rootCanvas;

    [Header("Toolbar Icon Animation")]
    [Tooltip("Exact name of the clip on THIS GameObject's Animation component.")]
    public string clipName;

    [Header("Scene Tool Animation")]
    [Tooltip("The scene GameObject that shows the tool animating (e.g. Hammeranim, Broomanim).\nNeeds a legacy Animation component. Must start INACTIVE.")]
    public GameObject toolSceneAnim;

    [Tooltip("Exact clip name on the toolSceneAnim Animation component.")]
    public string toolSceneClipName;

    // ── private ───────────────────────────────────────────────────────────
    private RectTransform _rect;
    private CanvasGroup _cg;
    private Animation _anim;

    private Vector2 _homePos;
    private Transform _homeParent;
    private int _homeSibling;

    [HideInInspector] public bool isAnimating = false;
    private bool _isDragging = false;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
        _anim = GetComponent<Animation>();

        // Make sure the scene tool is hidden at start
        if (toolSceneAnim != null)
            toolSceneAnim.SetActive(false);
    }

    void Start()
    {
        _homePos = _rect.anchoredPosition;
        _homeParent = transform.parent;
        _homeSibling = transform.GetSiblingIndex();
    }

    // ── Hover ─────────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData _)
    {
        if (isAnimating || _isDragging) return;
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (_isDragging) return;
        transform.localScale = Vector3.one;
    }

    // ── Drag ──────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData e)
    {
        if (isAnimating) return;

        _isDragging = true;
        transform.localScale = Vector3.one;

        // Move to top of canvas so it renders above everything
        transform.SetParent(rootCanvas.transform, true);
        _cg.alpha = 0.85f;
        _cg.blocksRaycasts = false; // lets pointer reach the drop zone underneath

        ToolDropZone.ShowHints(toolID, true);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_isDragging) return;
        _rect.anchoredPosition += e.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!_isDragging) return;
        _isDragging = false;

        ToolDropZone.ShowHints(toolID, false);
        _cg.blocksRaycasts = true;

        // If a ToolDropZone already claimed this drop it will have called
        // PlayToolAnim(), setting isAnimating = true before OnEndDrag fires.
        // In that case do nothing — the zone owns the rest of the sequence.
        // Otherwise snap straight back to the bar.
        if (!isAnimating)
            SnapHome();
    }

    // ── Called by ToolDropZone when the correct tool is dropped ──────────
    // Plays BOTH the toolbar icon animation AND the scene tool animation
    // simultaneously. The scene animation loops until StopToolAnim() is called.
    public void PlayToolAnim()
    {
        if (isAnimating) return;

        // Start the toolbar icon animation (plays once, then returns to bar)
        StartCoroutine(PlayOnceAndReturn());

        // Also activate and LOOP the scene tool animation at the same time
        if (toolSceneAnim != null)
        {
            toolSceneAnim.SetActive(true);
            Animation sceneAnim = toolSceneAnim.GetComponent<Animation>();
            if (sceneAnim != null && !string.IsNullOrEmpty(toolSceneClipName))
            {
                sceneAnim[toolSceneClipName].wrapMode = WrapMode.Loop;
                sceneAnim.Play(toolSceneClipName);
            }
        }
    }

    // ── Plays the toolbar icon animation once then fully restores the icon ─
    IEnumerator PlayOnceAndReturn()
    {
        isAnimating = true;

        // Return icon to toolbar slot and dim it while busy
        ReturnHome();
        _cg.alpha = 0.4f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;
        transform.localScale = Vector3.one;

        // Play toolbar icon clip once and wait for it to finish
        float clipLength = 0f;
        if (_anim != null && !string.IsNullOrEmpty(clipName))
        {
            AnimationClip clip = _anim.GetClip(clipName);
            if (clip != null)
            {
                clipLength = clip.length;
                _anim[clipName].wrapMode = WrapMode.Once;
                _anim.Play(clipName);
            }
        }

        yield return new WaitForSeconds(clipLength);

        if (_anim != null) _anim.Stop();

        // Fully restore — ready to use again
        isAnimating = false;
        _cg.alpha = 1f;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;
        transform.localScale = Vector3.one;
    }

    // ── Called by ToolDropZone when zone animation finishes ───────────────
    // Stops the looping scene tool animation and hides it.
    public void StopToolAnim()
    {
        if (toolSceneAnim != null)
        {
            Animation sceneAnim = toolSceneAnim.GetComponent<Animation>();
            if (sceneAnim != null) sceneAnim.Stop();
            toolSceneAnim.SetActive(false);
        }
    }

    // ── Reset (GameManager "Play Again") ──────────────────────────────────
    public void ResetTool()
    {
        StopAllCoroutines();
        isAnimating = false;
        _isDragging = false;

        if (_anim != null) _anim.Stop();

        // Also reset scene tool animation
        if (toolSceneAnim != null)
        {
            Animation sceneAnim = toolSceneAnim.GetComponent<Animation>();
            if (sceneAnim != null) sceneAnim.Stop();
            toolSceneAnim.SetActive(false);
        }

        ReturnHome();
        _cg.alpha = 1f;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;
        transform.localScale = Vector3.one;
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    void SnapHome()
    {
        ReturnHome();
        _cg.alpha = 1f;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;
        transform.localScale = Vector3.one;
    }

    void ReturnHome()
    {
        transform.SetParent(_homeParent, true);
        transform.SetSiblingIndex(_homeSibling);
        _rect.anchoredPosition = _homePos;
    }
}