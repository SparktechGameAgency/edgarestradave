////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;
////using UnityEngine.EventSystems;
////using UnityEngine.UI;

////[RequireComponent(typeof(Image))]
////[RequireComponent(typeof(CanvasGroup))]
////public class CleaningDropZone : MonoBehaviour,
////    IDropHandler,
////    IPointerEnterHandler,
////    IPointerExitHandler
////{
////    [Header("Task Identity")]
////    public string requiredTool;    // "broom" | "hammer" | "shovel" | "brush"
////    public string taskName;        // "waste"  | "frame"  | "grass"  | "mud"

////    [Header("Cleaning Animation")]
////    [Tooltip("Animator that plays the cleaning animation for this task.")]
////    public Animator taskAnimator;

////    [Tooltip("Trigger name inside the Animator. Leave blank if it auto-plays.")]
////    public string animTrigger = "";

////    [Tooltip("How long the animation plays before freezing on the last frame.")]
////    public float animDuration = 1.5f;

////    [Header("Highlight (optional)")]
////    public Image highlightImage;

////    [Header("Audio (optional)")]
////    public AudioClip successSound;
////    public AudioClip wrongToolSound;

////    // ── Static registry ────────────────────────────────────────────────────
////    private static readonly List<CleaningDropZone> _allZones = new();

////    // ── Persistent coroutine runner ────────────────────────────────────────
////    private static CleaningCoroutineRunner _runner;

////    // ── Private state ──────────────────────────────────────────────────────
////    private CanvasGroup _canvasGroup;
////    private AudioSource _audio;

////    private bool _isDone = false;
////    private bool _isHighlit = false;

////    private static readonly Color _normalHighlight = new Color(1f, 1f, 0f, 0.55f);
////    private static readonly Color _hoverHighlight = new Color(0f, 1f, 0f, 0.70f);

////    // ── Unity lifecycle ────────────────────────────────────────────────────
////    void Awake()
////    {
////        _canvasGroup = GetComponent<CanvasGroup>();
////        _audio = GetComponent<AudioSource>();

////        if (!_allZones.Contains(this))
////            _allZones.Add(this);

////        if (_runner == null)
////        {
////            var go = new GameObject("_CleaningCoroutineRunner");
////            DontDestroyOnLoad(go);
////            _runner = go.AddComponent<CleaningCoroutineRunner>();
////        }

////        if (highlightImage != null) highlightImage.enabled = false;

////        // Hide task animator at start
////        if (taskAnimator != null)
////            taskAnimator.gameObject.SetActive(false);
////    }

////    void Start()
////    {
////        if (taskAnimator != null)
////            taskAnimator.gameObject.SetActive(false);
////    }

////    void OnDestroy() => _allZones.Remove(this);

////    // ── Static highlight ───────────────────────────────────────────────────
////    public static void HighlightValidZones(string toolType, bool on)
////    {
////        foreach (var zone in _allZones)
////        {
////            if (zone == null || zone._isDone) continue;
////            if (zone.requiredTool != toolType) continue;
////            zone.SetHighlight(on, _normalHighlight);
////        }
////    }

////    // ── Hover ──────────────────────────────────────────────────────────────
////    public void OnPointerEnter(PointerEventData eventData)
////    {
////        if (_isDone) return;
////        if (!IsMatchingToolDragging(eventData)) return;
////        SetHighlight(true, _hoverHighlight);
////    }

////    public void OnPointerExit(PointerEventData eventData)
////    {
////        if (_isDone) return;
////        if (!IsMatchingToolDragging(eventData)) return;
////        SetHighlight(_isHighlit, _normalHighlight);
////    }

////    // ── Drop ───────────────────────────────────────────────────────────────
////    public void OnDrop(PointerEventData eventData)
////    {
////        if (_isDone) return;

////        DraggableTool droppedTool = eventData.pointerDrag?.GetComponent<DraggableTool>();
////        if (droppedTool == null) return;

////        if (droppedTool.toolType == requiredTool)
////            HandleSuccessfulDrop(droppedTool);
////        else
////            _runner.Run(FlashHighlightRed());
////    }

////    // ── Successful drop ────────────────────────────────────────────────────
////    void HandleSuccessfulDrop(DraggableTool tool)
////    {
////        _isDone = true;

////        SetHighlight(false, Color.clear);
////        HighlightValidZones(requiredTool, false);

////        PlaySound(successSound);

////        // Play the tool animation (broom swings, hammer hits…)
////        tool.StartToolAnim();

////        // Play this zone's cleaning animation
////        PlayCleaningAnim();

////        // After animDuration: freeze both animations on their last frame
////        _runner.Run(FreezeAfterDelay(tool, animDuration));
////    }

////    IEnumerator FreezeAfterDelay(DraggableTool tool, float delay)
////    {
////        yield return new WaitForSeconds(delay);

////        // Freeze tool animation on last frame (Animator disabled, NOT Rebind)
////        tool.StopAnimAndMarkUsed();

////        // Freeze the task cleaning animation on last frame
////        FreezeCleaningAnim();

////        // Tell GameManager this task is done — it will show AllCleanPanel when all 4 complete
////        GameManager.Instance?.OnCleaningTaskCompleted(taskName);
////    }

////    // ── Cleaning animation ─────────────────────────────────────────────────
////    void PlayCleaningAnim()
////    {
////        if (taskAnimator == null) return;

////        taskAnimator.gameObject.SetActive(true);
////        taskAnimator.enabled = true;

////        if (!string.IsNullOrEmpty(animTrigger))
////        {
////            foreach (var p in taskAnimator.parameters)
////            {
////                if (p.name == animTrigger &&
////                    p.type == AnimatorControllerParameterType.Trigger)
////                {
////                    taskAnimator.SetTrigger(animTrigger);
////                    break;
////                }
////            }
////        }
////    }

////    // Disable Animator component only — keeps the last frame visible
////    void FreezeCleaningAnim()
////    {
////        if (taskAnimator == null) return;
////        taskAnimator.enabled = false;   // freeze on last frame — DO NOT Rebind
////    }

////    // ── Wrong tool ─────────────────────────────────────────────────────────
////    IEnumerator FlashHighlightRed()
////    {
////        PlaySound(wrongToolSound);
////        SetHighlight(true, new Color(1f, 0f, 0f, 0.55f));
////        yield return new WaitForSeconds(0.4f);
////        if (!_isDone) SetHighlight(false, Color.clear);
////    }

////    // ── Reset ──────────────────────────────────────────────────────────────
////    public void ResetZone()
////    {
////        _isDone = false;

////        if (taskAnimator != null)
////        {
////            taskAnimator.gameObject.SetActive(true);
////            taskAnimator.enabled = true;
////            taskAnimator.Rebind();
////            taskAnimator.Update(0f);
////            taskAnimator.enabled = false;
////            taskAnimator.gameObject.SetActive(false);
////        }

////        SetHighlight(false, Color.clear);
////        transform.localScale = Vector3.one;
////    }

////    // ── Helpers ────────────────────────────────────────────────────────────
////    void SetHighlight(bool show, Color color)
////    {
////        _isHighlit = show;
////        if (highlightImage == null) return;
////        highlightImage.enabled = show;
////        if (show) highlightImage.color = color;
////    }

////    bool IsMatchingToolDragging(PointerEventData eventData)
////    {
////        var tool = eventData.pointerDrag?.GetComponent<DraggableTool>();
////        return tool != null && tool.toolType == requiredTool;
////    }

////    void PlaySound(AudioClip clip)
////    {
////        if (clip == null || _audio == null) return;
////        _audio.PlayOneShot(clip);
////    }
////}

////// ── Persistent coroutine runner ────────────────────────────────────────────
////public class CleaningCoroutineRunner : MonoBehaviour
////{
////    public void Run(IEnumerator routine) => StartCoroutine(routine);
////}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//[RequireComponent(typeof(Image))]
//[RequireComponent(typeof(CanvasGroup))]
//public class CleaningDropZone : MonoBehaviour,
//    IDropHandler,
//    IPointerEnterHandler,
//    IPointerExitHandler
//{
//    [Header("Task Identity")]
//    public string requiredTool;    // "broom" | "hammer" | "shovel" | "brush"
//    public string taskName;        // "waste"  | "frame"  | "grass"  | "mud"

//    [Header("Cleaning Animation")]
//    [Tooltip("The waste/mess GameObject — deactivated instantly when the correct tool is dropped.")]
//    public GameObject dirtyState;

//    [Tooltip("Animator that plays the cleaning animation for this task.")]
//    public Animator taskAnimator;

//    [Tooltip("Trigger name inside the Animator. Leave blank if it auto-plays.")]
//    public string animTrigger = "";

//    [Tooltip("How long the animation plays before freezing on the last frame.")]
//    public float animDuration = 1.5f;

//    [Header("Highlight (optional)")]
//    public Image highlightImage;

//    [Header("Audio (optional)")]
//    public AudioClip successSound;
//    public AudioClip wrongToolSound;

//    // ── Static registry ────────────────────────────────────────────────────
//    private static readonly List<CleaningDropZone> _allZones = new();

//    // ── Persistent coroutine runner ────────────────────────────────────────
//    private static CleaningCoroutineRunner _runner;

//    // ── Private state ──────────────────────────────────────────────────────
//    private CanvasGroup _canvasGroup;
//    private AudioSource _audio;

//    private bool _isDone = false;
//    private bool _isHighlit = false;

//    private static readonly Color _normalHighlight = new Color(1f, 1f, 0f, 0.55f);
//    private static readonly Color _hoverHighlight = new Color(0f, 1f, 0f, 0.70f);

//    // ── Unity lifecycle ────────────────────────────────────────────────────
//    void Awake()
//    {
//        _canvasGroup = GetComponent<CanvasGroup>();
//        _audio = GetComponent<AudioSource>();

//        if (!_allZones.Contains(this))
//            _allZones.Add(this);

//        if (_runner == null)
//        {
//            var go = new GameObject("_CleaningCoroutineRunner");
//            DontDestroyOnLoad(go);
//            _runner = go.AddComponent<CleaningCoroutineRunner>();
//        }

//        if (highlightImage != null) highlightImage.enabled = false;

//        // Hide task animator at start
//        if (taskAnimator != null)
//            taskAnimator.gameObject.SetActive(false);
//    }

//    void Start()
//    {
//        if (taskAnimator != null)
//            taskAnimator.gameObject.SetActive(false);
//    }

//    void OnDestroy() => _allZones.Remove(this);

//    // ── Static highlight ───────────────────────────────────────────────────
//    public static void HighlightValidZones(string toolType, bool on)
//    {
//        foreach (var zone in _allZones)
//        {
//            if (zone == null || zone._isDone) continue;
//            if (zone.requiredTool != toolType) continue;
//            zone.SetHighlight(on, _normalHighlight);
//        }
//    }

//    // ── Hover ──────────────────────────────────────────────────────────────
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (_isDone) return;
//        if (!IsMatchingToolDragging(eventData)) return;
//        SetHighlight(true, _hoverHighlight);
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        if (_isDone) return;
//        if (!IsMatchingToolDragging(eventData)) return;
//        SetHighlight(_isHighlit, _normalHighlight);
//    }

//    // ── Drop ───────────────────────────────────────────────────────────────
//    public void OnDrop(PointerEventData eventData)
//    {
//        if (_isDone) return;

//        DraggableTool droppedTool = eventData.pointerDrag?.GetComponent<DraggableTool>();
//        if (droppedTool == null) return;

//        if (droppedTool.toolType == requiredTool)
//            HandleSuccessfulDrop(droppedTool);
//        else
//            _runner.Run(FlashHighlightRed());
//    }

//    // ── Successful drop ────────────────────────────────────────────────────
//    void HandleSuccessfulDrop(DraggableTool tool)
//    {
//        _isDone = true;

//        SetHighlight(false, Color.clear);
//        HighlightValidZones(requiredTool, false);

//        PlaySound(successSound);

//        // Deactivate the waste/mess GameObject instantly on drop
//        if (dirtyState != null)
//            dirtyState.SetActive(false);

//        // Play the tool animation (broom swings, hammer hits…)
//        tool.StartToolAnim();

//        // Play this zone's cleaning animation
//        PlayCleaningAnim();

//        // After animDuration: freeze both animations on their last frame
//        _runner.Run(FreezeAfterDelay(tool, animDuration));
//    }

//    IEnumerator FreezeAfterDelay(DraggableTool tool, float delay)
//    {
//        yield return new WaitForSeconds(delay);

//        // Freeze tool animation on last frame (Animator disabled, NOT Rebind)
//        tool.StopAnimAndMarkUsed();

//        // Freeze the task cleaning animation on last frame
//        FreezeCleaningAnim();

//        // Tell GameManager this task is done — it will show AllCleanPanel when all 4 complete
//        GameManager.Instance?.OnCleaningTaskCompleted(taskName);
//    }

//    // ── Cleaning animation ─────────────────────────────────────────────────
//    void PlayCleaningAnim()
//    {
//        if (taskAnimator == null) return;

//        taskAnimator.gameObject.SetActive(true);
//        taskAnimator.enabled = true;

//        if (!string.IsNullOrEmpty(animTrigger))
//        {
//            foreach (var p in taskAnimator.parameters)
//            {
//                if (p.name == animTrigger &&
//                    p.type == AnimatorControllerParameterType.Trigger)
//                {
//                    taskAnimator.SetTrigger(animTrigger);
//                    break;
//                }
//            }
//        }
//    }

//    // Disable Animator component only — keeps the last frame visible
//    void FreezeCleaningAnim()
//    {
//        if (taskAnimator == null) return;
//        taskAnimator.enabled = false;   // freeze on last frame — DO NOT Rebind
//    }

//    // ── Wrong tool ─────────────────────────────────────────────────────────
//    IEnumerator FlashHighlightRed()
//    {
//        PlaySound(wrongToolSound);
//        SetHighlight(true, new Color(1f, 0f, 0f, 0.55f));
//        yield return new WaitForSeconds(0.4f);
//        if (!_isDone) SetHighlight(false, Color.clear);
//    }

//    // ── Reset ──────────────────────────────────────────────────────────────
//    public void ResetZone()
//    {
//        _isDone = false;

//        // Restore the waste/mess on reset
//        if (dirtyState != null)
//            dirtyState.SetActive(true);

//        if (taskAnimator != null)
//        {
//            taskAnimator.gameObject.SetActive(true);
//            taskAnimator.enabled = true;
//            taskAnimator.Rebind();
//            taskAnimator.Update(0f);
//            taskAnimator.enabled = false;
//            taskAnimator.gameObject.SetActive(false);
//        }

//        SetHighlight(false, Color.clear);
//        transform.localScale = Vector3.one;
//    }

//    // ── Helpers ────────────────────────────────────────────────────────────
//    void SetHighlight(bool show, Color color)
//    {
//        _isHighlit = show;
//        if (highlightImage == null) return;
//        highlightImage.enabled = show;
//        if (show) highlightImage.color = color;
//    }

//    bool IsMatchingToolDragging(PointerEventData eventData)
//    {
//        var tool = eventData.pointerDrag?.GetComponent<DraggableTool>();
//        return tool != null && tool.toolType == requiredTool;
//    }

//    void PlaySound(AudioClip clip)
//    {
//        if (clip == null || _audio == null) return;
//        _audio.PlayOneShot(clip);
//    }
//}

//// ── Persistent coroutine runner ────────────────────────────────────────────
//public class CleaningCoroutineRunner : MonoBehaviour
//{
//    public void Run(IEnumerator routine) => StartCoroutine(routine);
//}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

// ─────────────────────────────────────────────────────────────────────────────
// CleaningDropZone.cs
//
// SEQUENCE (same for all 4 tools):
//
//   1. Drop tool on its zone
//   2. Tool animation   starts ──┐  at the same time
//      Cleaning animation starts ┘
//   3. After animDuration seconds:
//        • Cleaning animation FREEZES on last frame (Animator disabled)
//        • Tool animation DISAPPEARS (scene GameObject hidden)
//   4. When all 4 zones are done → completion panel spawns instantly
//
// INSPECTOR SETUP:
//   Broom  zone → requiredTool = "broom"  | taskAnimator = Wastecleaning
//   Hammer zone → requiredTool = "hammer" | taskAnimator = PhotoRestore
//   Shovel zone → requiredTool = "shovel" | taskAnimator = sovle
//   Brush  zone → requiredTool = "brush"  | taskAnimator = brush
//
//   animDuration = exact length of your cleaning animation clip in seconds
//
// NO dirtyState / cleanState fields — the cleaning animation frozen on its
// last frame IS the final visual. Nothing is hidden or shown afterward.
// ─────────────────────────────────────────────────────────────────────────────

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class CleaningDropZone : MonoBehaviour,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    // ── Inspector ──────────────────────────────────────────────────────────
    [Header("Task Identity")]
    [Tooltip("Must match DraggableTool.toolType.")]
    public string requiredTool;   // "broom" | "hammer" | "shovel" | "brush"

    [Tooltip("Unique name for this task — used to track completion.")]
    public string taskName;       // "waste" | "frame"  | "grass"  | "mud"

    [Header("Cleaning Animation")]
    [Tooltip("Animator for the cleaning animation (Wastecleaning, PhotoRestore, etc.)." +
             " This GameObject must start INACTIVE in the Hierarchy.")]
    public Animator taskAnimator;

    [Tooltip("Trigger parameter name inside the cleaning Animator (e.g. 'Clean').")]
    public string animTrigger = "Clean";

    [Tooltip("Exact duration in seconds of the cleaning animation clip.\n" +
             "After this time the animation freezes on its last frame.")]
    public float animDuration = 1.0f;

    [Header("Completion")]
    [Tooltip("The panel that appears INSTANTLY when all 4 tasks are done.\n" +
             "Assign the panel GameObject here. It must start INACTIVE.")]
    public static GameObject completionPanel;   // set from Inspector via GameManager or assign below

    [Header("Highlight (optional)")]
    [Tooltip("A child Image (glow/outline) that lights up when the matching tool is dragged.")]
    public Image highlightImage;

    [Header("Audio (optional)")]
    public AudioClip successSound;
    public AudioClip wrongToolSound;

    // ── Static registry & completion tracking ─────────────────────────────
    private static readonly List<CleaningDropZone> _allZones = new();
    private static readonly HashSet<string> _completedTasks = new();

    // Assign your completion panel here from the Inspector via this static ref,
    // OR assign it through GameManager. See comment at bottom of file.
    [Header("Completion Panel")]
    [Tooltip("The UI panel (starts inactive) that pops up when all 4 tasks are done.")]
    public GameObject winPanel;   // drag your panel here on ONE of the drop zones (or use GameManager)

    // ── Private ────────────────────────────────────────────────────────────
    private CanvasGroup _canvasGroup;
    private AudioSource _audio;

    private bool _isDone = false;
    private bool _isHighlit = false;

    private static readonly Color _normalHighlight = new Color(1f, 1f, 0f, 0.55f);
    private static readonly Color _hoverHighlight = new Color(0f, 1f, 0f, 0.70f);

    // ── Lifecycle ──────────────────────────────────────────────────────────
    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _audio = GetComponent<AudioSource>();

        _allZones.Add(this);

        // Cleaning animation starts hidden
        if (taskAnimator != null)
            taskAnimator.gameObject.SetActive(false);

        if (highlightImage != null)
            highlightImage.enabled = false;
    }

    void OnDestroy() => _allZones.Remove(this);

    // ── Static: highlight all matching zones while dragging ───────────────
    public static void HighlightValidZones(string toolType, bool on)
    {
        foreach (var zone in _allZones)
        {
            if (zone._isDone) continue;
            if (zone.requiredTool != toolType) continue;
            zone.SetHighlight(on, _normalHighlight);
        }
    }

    // ── Hover ──────────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDone) return;
        if (!IsMatchingToolDragging(eventData)) return;
        SetHighlight(true, _hoverHighlight);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDone) return;
        if (!IsMatchingToolDragging(eventData)) return;
        SetHighlight(true, _normalHighlight);
    }

    // ── Drop ───────────────────────────────────────────────────────────────
    public void OnDrop(PointerEventData eventData)
    {
        if (_isDone) return;

        DraggableTool droppedTool = eventData.pointerDrag?.GetComponent<DraggableTool>();
        if (droppedTool == null) return;

        if (droppedTool.toolType == requiredTool)
            HandleSuccessfulDrop(droppedTool);
        else
            HandleWrongTool();
    }

    // ── Successful drop ────────────────────────────────────────────────────
    void HandleSuccessfulDrop(DraggableTool tool)
    {
        _isDone = true;
        SetHighlight(false, Color.clear);
        HighlightValidZones(requiredTool, false);

        PlaySound(successSound);
        StartCoroutine(PlaySequence(tool));
    }

    // ── Sequence: both anims play → cleaning freezes → tool hides ─────────
    IEnumerator PlaySequence(DraggableTool tool)
    {
        // ── STEP 1: Start BOTH animations at the exact same time ──────────
        tool.StartToolAnim();

        if (taskAnimator != null)
        {
            taskAnimator.gameObject.SetActive(true);
            taskAnimator.enabled = true;

            if (!string.IsNullOrEmpty(animTrigger))
                taskAnimator.SetTrigger(animTrigger);
        }

        // ── STEP 2: Wait for cleaning animation to reach its last frame ───
        yield return new WaitForSeconds(animDuration);

        // ── STEP 3: Freeze cleaning animation on its last frame ───────────
        // Disabling the Animator keeps it locked on the final frame.
        // The GameObject stays visible — this IS the cleaned look.
        if (taskAnimator != null)
            taskAnimator.enabled = false;

        // ── STEP 4: Hide the tool (broom, hammer etc.) immediately ────────
        tool.HideToolAfterAnim();

        // ── STEP 5: Mark task complete and check if all 4 are done ────────
        _completedTasks.Add(taskName);

        if (_completedTasks.Count >= 4)
            ShowCompletionPanel();
    }

    // ── Show the win panel instantly when all 4 tasks done ────────────────
    void ShowCompletionPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    // ── Wrong tool ─────────────────────────────────────────────────────────
    void HandleWrongTool()
    {
        PlaySound(wrongToolSound);
        StartCoroutine(ShakePosition(0.4f, 12f));
        SetHighlight(true, new Color(1f, 0f, 0f, 0.55f));
        StartCoroutine(ClearHighlightAfter(0.4f));
    }

    // ── Reset ──────────────────────────────────────────────────────────────
    public static void ResetAllZones()
    {
        _completedTasks.Clear();
        foreach (var zone in _allZones)
            zone.ResetZone();
    }

    public void ResetZone()
    {
        StopAllCoroutines();
        _isDone = false;

        // Reset and hide cleaning animation
        if (taskAnimator != null)
        {
            taskAnimator.gameObject.SetActive(true);
            taskAnimator.enabled = true;
            taskAnimator.Rebind();
            taskAnimator.Update(0f);
            taskAnimator.enabled = false;
            taskAnimator.gameObject.SetActive(false);
        }

        SetHighlight(false, Color.clear);
        transform.localScale = Vector3.one;

        // Hide win panel on reset
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    void SetHighlight(bool show, Color color)
    {
        _isHighlit = show;
        if (highlightImage == null) return;
        highlightImage.enabled = show;
        if (show) highlightImage.color = color;
    }

    bool IsMatchingToolDragging(PointerEventData eventData)
    {
        var tool = eventData.pointerDrag?.GetComponent<DraggableTool>();
        return tool != null && tool.toolType == requiredTool;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null || _audio == null) return;
        _audio.PlayOneShot(clip);
    }

    IEnumerator ShakePosition(float duration, float strength)
    {
        Vector3 original = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-strength, strength);
            transform.localPosition = original + new Vector3(x, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = original;
    }

    IEnumerator ClearHighlightAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!_isDone) SetHighlight(false, Color.clear);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// HOW TO SET UP THE COMPLETION PANEL
//
// Option A — Simple (no GameManager needed):
//   • Create your panel UI, set it INACTIVE
//   • On the Waste drop zone → drag the panel into the "Win Panel" field
//   • Leave "Win Panel" empty on the other 3 zones
//
// Option B — Via GameManager:
//   • Add this method to your GameManager:
//
//       public void OnAllCleaningTasksCompleted()
//       {
//           completionPanel.SetActive(true);   // your panel reference
//       }
//
//   • Leave "Win Panel" empty on all 4 zones
// ─────────────────────────────────────────────────────────────────────────────