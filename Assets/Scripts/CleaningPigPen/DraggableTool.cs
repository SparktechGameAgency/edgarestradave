//using System.Collections;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//[RequireComponent(typeof(Image))]
//[RequireComponent(typeof(CanvasGroup))]
//public class DraggableTool : MonoBehaviour,
//    IBeginDragHandler,
//    IDragHandler,
//    IEndDragHandler,
//    IPointerEnterHandler,
//    IPointerExitHandler
//{
//    [Header("Tool Identity")]
//    [Tooltip("Must match the 'requiredTool' field on the correct DropZone.")]
//    public string toolType;

//    [Header("References")]
//    public Canvas rootCanvas;

//    [Header("Tool Animation")]
//    [Tooltip("Animator on the SCENE version of this tool (broom, hammer, shovel, brush).")]
//    public Animator toolAnimator;

//    [Tooltip("Trigger parameter name in the Animator (e.g. 'Swing').")]
//    public string toolAnimTrigger = "Swing";

//    [Header("Audio (optional)")]
//    public AudioClip pickUpSound;
//    public AudioClip snapBackSound;

//    // ── Private state ──────────────────────────────────────────────────────
//    private RectTransform _rect;
//    private CanvasGroup _canvasGroup;
//    private AudioSource _audio;

//    private Vector2 _originalAnchoredPos;
//    private Transform _originalParent;
//    private int _originalSiblingIndex;

//    private bool _isAnimating = false;
//    private bool _isDragging = false;
//    private bool _isUsed = false;

//    // ── GLOBAL LOCK — only one tool animates at a time ─────────────────────
//    private static DraggableTool _currentlyAnimatingTool = null;

//    // ── Unity lifecycle ────────────────────────────────────────────────────
//    void Awake()
//    {
//        _rect = GetComponent<RectTransform>();
//        _canvasGroup = GetComponent<CanvasGroup>();
//        _audio = GetComponent<AudioSource>();

//        if (rootCanvas == null)
//            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
//    }

//    void Start()
//    {
//        _originalAnchoredPos = _rect.anchoredPosition;
//        _originalParent = transform.parent;
//        _originalSiblingIndex = transform.GetSiblingIndex();

//        // Hide scene tool at start — only shown while animating
//        if (toolAnimator != null)
//        {
//            toolAnimator.gameObject.SetActive(false);
//            toolAnimator.enabled = false;
//        }
//    }

//    // ── Hover ──────────────────────────────────────────────────────────────
//    public void OnPointerEnter(PointerEventData _)
//    {
//        if (_isAnimating || _isDragging || _isUsed) return;
//        StopAllCoroutines();
//        StartCoroutine(ScaleTo(1.15f, 0.12f));
//    }

//    public void OnPointerExit(PointerEventData _)
//    {
//        if (_isDragging) return;
//        StopAllCoroutines();
//        StartCoroutine(ScaleTo(1f, 0.1f));
//    }

//    // ── Drag ───────────────────────────────────────────────────────────────
//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        if (_isUsed || _isAnimating || _currentlyAnimatingTool != null) return;

//        _isDragging = true;
//        StopAllCoroutines();
//        transform.localScale = Vector3.one;

//        transform.SetParent(rootCanvas.transform, true);
//        _canvasGroup.alpha = 0.85f;
//        _canvasGroup.blocksRaycasts = false;

//        PlaySound(pickUpSound);
//        CleaningDropZone.HighlightValidZones(toolType, true);
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        if (!_isDragging) return;
//        _rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        if (!_isDragging) return;
//        _isDragging = false;

//        CleaningDropZone.HighlightValidZones(toolType, false);

//        if (!_isAnimating && !_isUsed)
//            SnapBack();

//        _canvasGroup.blocksRaycasts = true;
//    }

//    // ── Called by DropZone: show scene tool and play its animation ─────────
//    public void StartToolAnim()
//    {
//        if (_currentlyAnimatingTool != null && _currentlyAnimatingTool != this)
//        {
//            Debug.LogWarning($"[DraggableTool] '{name}' blocked — " +
//                             $"'{_currentlyAnimatingTool.name}' still animating.");
//            return;
//        }

//        _currentlyAnimatingTool = this;
//        _isAnimating = true;

//        // Return toolbar icon to slot (greyed while animating)
//        ReturnToParent();
//        _rect.anchoredPosition = _originalAnchoredPos;
//        _canvasGroup.alpha = 0.35f;
//        _canvasGroup.interactable = false;
//        _canvasGroup.blocksRaycasts = false;
//        transform.localScale = Vector3.one;

//        // Show and trigger the scene tool animator
//        if (toolAnimator != null)
//        {
//            toolAnimator.gameObject.SetActive(true);
//            toolAnimator.enabled = true;

//            if (!string.IsNullOrEmpty(toolAnimTrigger))
//                toolAnimator.SetTrigger(toolAnimTrigger);

//            Debug.Log($"[DraggableTool] ▶ '{name}' animation started (trigger: '{toolAnimTrigger}').");
//        }
//    }

//    // ── Called by DropZone after animDuration: freeze on last frame ────────
//    public void StopAnimAndMarkUsed()
//    {
//        _isAnimating = false;
//        _isUsed = true;

//        // Release global lock
//        if (_currentlyAnimatingTool == this)
//            _currentlyAnimatingTool = null;

//        if (toolAnimator != null)
//        {
//            // Disable Animator WITHOUT Rebind — freezes on last frame,
//            // then hides the broom/hammer GameObject from the scene.
//            toolAnimator.enabled = false;
//            toolAnimator.gameObject.SetActive(false);

//            Debug.Log($"[DraggableTool] ⏹ '{name}' done — hidden.");
//        }

//        // Grey out toolbar icon permanently
//        _canvasGroup.alpha = 0.35f;
//        _canvasGroup.interactable = false;
//        _canvasGroup.blocksRaycasts = false;
//        transform.localScale = Vector3.one;
//    }

//    // ── Reset ──────────────────────────────────────────────────────────────
//    public void ResetTool()
//    {
//        StopAllCoroutines();

//        if (_currentlyAnimatingTool == this)
//            _currentlyAnimatingTool = null;

//        _isAnimating = false;
//        _isDragging = false;
//        _isUsed = false;

//        // Fully reset the scene tool animator and hide it
//        if (toolAnimator != null)
//        {
//            toolAnimator.gameObject.SetActive(true);
//            toolAnimator.enabled = true;
//            toolAnimator.Rebind();
//            toolAnimator.Update(0f);
//            toolAnimator.enabled = false;
//            toolAnimator.gameObject.SetActive(false);
//        }

//        _canvasGroup.alpha = 1f;
//        _canvasGroup.interactable = true;
//        _canvasGroup.blocksRaycasts = true;

//        ReturnToParent();
//        _rect.anchoredPosition = _originalAnchoredPos;
//        transform.localScale = Vector3.one;
//    }

//    // ── Helpers ────────────────────────────────────────────────────────────
//    void SnapBack()
//    {
//        ReturnToParent();
//        _canvasGroup.alpha = 1f;
//        _canvasGroup.interactable = true;
//        _canvasGroup.blocksRaycasts = true;
//        transform.localScale = Vector3.one;

//        StopAllCoroutines();
//        StartCoroutine(MoveToPos(_originalAnchoredPos, 0.25f));
//        PlaySound(snapBackSound);
//    }

//    void ReturnToParent()
//    {
//        transform.SetParent(_originalParent, true);
//        transform.SetSiblingIndex(_originalSiblingIndex);
//    }

//    void PlaySound(AudioClip clip)
//    {
//        if (clip == null || _audio == null) return;
//        _audio.PlayOneShot(clip);
//    }

//    // ── Coroutines ─────────────────────────────────────────────────────────
//    IEnumerator ScaleTo(float target, float duration)
//    {
//        Vector3 start = transform.localScale;
//        Vector3 end = Vector3.one * target;
//        float t = 0f;
//        while (t < 1f)
//        {
//            t += Time.deltaTime / duration;
//            transform.localScale = Vector3.Lerp(start, end,
//                                   Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
//            yield return null;
//        }
//        transform.localScale = end;
//    }

//    IEnumerator MoveToPos(Vector2 target, float duration)
//    {
//        Vector2 start = _rect.anchoredPosition;
//        float t = 0f;
//        while (t < 1f)
//        {
//            t += Time.deltaTime / duration;
//            _rect.anchoredPosition = Vector2.Lerp(start, target,
//                                     Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
//            yield return null;
//        }
//        _rect.anchoredPosition = target;
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableTool : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Tool Identity")]
    [Tooltip("Must match the 'requiredTool' field on the correct DropZone.")]
    public string toolType;   // "broom" | "hammer" | "shovel" | "brush"

    [Header("References")]
    public Canvas rootCanvas;

    [Header("Tool Animation")]
    [Tooltip("The scene GameObject that shows the tool animating (broom sweeping etc.)." +
             " Assign its Animator here. Starts INACTIVE.")]
    public Animator toolAnimator;

    [Tooltip("Trigger name inside the tool Animator (e.g. 'Clean').")]
    public string toolAnimTrigger = "Clean";

    [Header("Audio (optional)")]
    public AudioClip pickUpSound;
    public AudioClip snapBackSound;

    // ── Private ────────────────────────────────────────────────────────────
    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private AudioSource _audio;

    private Vector2 _originalAnchoredPos;
    private Transform _originalParent;
    private int _originalSiblingIndex;

    private bool _isAnimating = false;
    private bool _isDragging = false;
    private bool _isUsed = false;

    // ── Lifecycle ──────────────────────────────────────────────────────────
    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _audio = GetComponent<AudioSource>();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    void Start()
    {
        _originalAnchoredPos = _rect.anchoredPosition;
        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        // Scene tool starts hidden
        if (toolAnimator != null)
        {
            toolAnimator.gameObject.SetActive(false);
            toolAnimator.enabled = false;
        }
    }

    // ── Hover ──────────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData _)
    {
        if (_isAnimating || _isDragging || _isUsed) return;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(1.15f, 0.12f));
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (_isDragging) return;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(1f, 0.1f));
    }

    // ── Drag ───────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isUsed || _isAnimating) return;

        _isDragging = true;
        StopAllCoroutines();
        transform.localScale = Vector3.one;

        transform.SetParent(rootCanvas.transform, true);
        _canvasGroup.alpha = 0.85f;
        _canvasGroup.blocksRaycasts = false;

        PlaySound(pickUpSound);
        CleaningDropZone.HighlightValidZones(toolType, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        CleaningDropZone.HighlightValidZones(toolType, false);

        if (!_isAnimating && !_isUsed)
            SnapBack();

        _canvasGroup.blocksRaycasts = true;
    }

    // ── Called by DropZone: show scene tool, start its animation ──────────
    public void StartToolAnim()
    {
        _isAnimating = true;

        // Return toolbar icon to its slot, grey it out while animating
        ReturnToParent();
        _rect.anchoredPosition = _originalAnchoredPos;
        _canvasGroup.alpha = 0.35f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.one;

        // Activate and trigger the scene tool
        if (toolAnimator != null)
        {
            toolAnimator.gameObject.SetActive(true);
            toolAnimator.enabled = true;

            if (!string.IsNullOrEmpty(toolAnimTrigger))
                toolAnimator.SetTrigger(toolAnimTrigger);
        }
    }

    // ── Called by DropZone when cleaning animation finishes ───────────────
    // Hides the scene tool. Toolbar icon stays greyed (task is done).
    public void HideToolAfterAnim()
    {
        _isAnimating = false;
        _isUsed = true;

        if (toolAnimator != null)
        {
            toolAnimator.enabled = false;
            toolAnimator.gameObject.SetActive(false);
        }

        // Keep toolbar icon greyed permanently — task completed
        _canvasGroup.alpha = 0.35f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.one;
    }

    // ── Reset ──────────────────────────────────────────────────────────────
    public void ResetTool()
    {
        StopAllCoroutines();

        _isAnimating = false;
        _isDragging = false;
        _isUsed = false;

        if (toolAnimator != null)
        {
            toolAnimator.gameObject.SetActive(true);
            toolAnimator.enabled = true;
            toolAnimator.Rebind();
            toolAnimator.Update(0f);
            toolAnimator.enabled = false;
            toolAnimator.gameObject.SetActive(false);
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        ReturnToParent();
        _rect.anchoredPosition = _originalAnchoredPos;
        transform.localScale = Vector3.one;
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    void SnapBack()
    {
        ReturnToParent();
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        transform.localScale = Vector3.one;

        StopAllCoroutines();
        StartCoroutine(MoveToPos(_originalAnchoredPos, 0.25f));
        PlaySound(snapBackSound);
    }

    void ReturnToParent()
    {
        transform.SetParent(_originalParent, true);
        transform.SetSiblingIndex(_originalSiblingIndex);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null || _audio == null) return;
        _audio.PlayOneShot(clip);
    }

    IEnumerator ScaleTo(float target, float duration)
    {
        Vector3 start = transform.localScale;
        Vector3 end = Vector3.one * target;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(start, end,
                                   Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            yield return null;
        }
        transform.localScale = end;
    }

    IEnumerator MoveToPos(Vector2 target, float duration)
    {
        Vector2 start = _rect.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            _rect.anchoredPosition = Vector2.Lerp(start, target,
                                     Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            yield return null;
        }
        _rect.anchoredPosition = target;
    }
}