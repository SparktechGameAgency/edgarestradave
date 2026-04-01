//////using System.Collections;
//////using System.Collections.Generic;
//////using UnityEngine;
//////using UnityEngine.EventSystems;
//////using UnityEngine.UI;

//////// ══════════════════════════════════════════════════════════════════════════════
//////// ToolDropZone.cs
//////// Attach this to each area that accepts a tool drop.
//////// e.g.  PhotoFrame, WastePile, GrassPatch, MudPatch
////////
//////// INSPECTOR FIELDS:
////////   acceptToolID      → must match the toolID on the correct DraggableTool
////////                       e.g. PhotoFrame zone → 2  (because Hammer = 2)
////////   zoneImage         → the Image you want to HIDE instantly on drop
////////                       (the dirty/broken version)
////////   zoneAnimation     → GameObject with legacy Animation component
////////                       plays the cleaning/fixing animation — starts INACTIVE
////////   zoneClipName      → exact clip name on zoneAnimation
////////   completionPanel   → assign on ONE zone only — shown when all zones done
////////
//////// HOW TO SET UP zoneAnimation:
////////   1. Select the zone GameObject (e.g. PhotoFrame)
////////   2. Add Component → Animation   ← legacy, NOT Animator
////////   3. Drag your clip into the Animation field
////////   4. Uncheck "Play Automatically"
////////   5. Set the GameObject INACTIVE
////////   6. Drag it into zoneAnimation field
//////// ══════════════════════════════════════════════════════════════════════════════

//////[RequireComponent(typeof(Image))]
//////[RequireComponent(typeof(CanvasGroup))]
//////public class ToolDropZone : MonoBehaviour,
//////    IDropHandler, IPointerEnterHandler, IPointerExitHandler
//////{
//////    [Header("Identity")]
//////    [Tooltip("Must match the toolID on the DraggableTool that belongs here.")]
//////    public int acceptToolID;

//////    [Header("Zone Visuals")]
//////    [Tooltip("The dirty/broken image — hidden INSTANTLY when the correct tool is dropped.")]
//////    public GameObject zoneImage;

//////    [Header("Zone Animation")]
//////    [Tooltip("GameObject with a legacy Animation component. Starts INACTIVE.\n" +
//////             "This plays the cleaning/fixing animation.")]
//////    public GameObject zoneAnimation;

//////    [Tooltip("Exact name of the animation clip on zoneAnimation.")]
//////    public string zoneClipName;

//////    [Header("Completion")]
//////    [Tooltip("Assign your completion panel/image on ONE zone only.\n" +
//////             "It appears instantly when ALL zones are done. Starts INACTIVE.")]
//////    public GameObject completionPanel;

//////    [Header("Hint Highlight (optional)")]
//////    [Tooltip("A child Image that glows yellow when the matching tool is being dragged.\n" +
//////             "Leave empty if you don't need a glow effect.")]
//////    public Image hintGlow;

//////    // ── Static: track all zones ───────────────────────────────────────────
//////    // NOTE: Static lists persist across scene reloads in Unity.
//////    // We clear _all in Awake and rely on OnDestroy to keep it clean.
//////    private static readonly List<ToolDropZone> _all = new();
//////    private static int _doneCount = 0;

//////    // ── Private ───────────────────────────────────────────────────────────
//////    private bool _isDone = false;

//////    static readonly Color _yellow = new Color(1f, 0.92f, 0f, 0.55f);
//////    static readonly Color _green = new Color(0.2f, 1f, 0.2f, 0.65f);

//////    // ─────────────────────────────────────────────────────────────────────
//////    void Awake()
//////    {
//////        // Guard against duplicate registration (e.g. after scene reload
//////        // without proper cleanup). This also fixes the stale-static bug.
//////        if (!_all.Contains(this))
//////            _all.Add(this);

//////        if (zoneImage != null) zoneImage.SetActive(true);
//////        if (zoneAnimation != null) zoneAnimation.SetActive(false);
//////        if (completionPanel != null) completionPanel.SetActive(false);
//////        if (hintGlow != null) hintGlow.enabled = false;
//////    }

//////    void OnDestroy()
//////    {
//////        _all.Remove(this);
//////        // If all zones are gone (e.g. scene unloaded) reset counter so
//////        // a fresh scene starts cleanly.
//////        if (_all.Count == 0)
//////            _doneCount = 0;
//////    }

//////    // ── Called by DraggableTool.OnBeginDrag / OnEndDrag ───────────────────
//////    public static void ShowHints(int toolID, bool show)
//////    {
//////        foreach (var zone in _all)
//////        {
//////            if (zone._isDone) continue;
//////            if (zone.acceptToolID != toolID) continue;
//////            if (zone.hintGlow == null) continue;

//////            zone.hintGlow.enabled = show;
//////            zone.hintGlow.color = _yellow;
//////        }
//////    }

//////    // ── Hover: change glow to green when correct tool hovers ─────────────
//////    public void OnPointerEnter(PointerEventData e)
//////    {
//////        if (_isDone || hintGlow == null) return;
//////        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
//////        if (tool == null || tool.toolID != acceptToolID) return;

//////        hintGlow.enabled = true;
//////        hintGlow.color = _green;
//////    }

//////    public void OnPointerExit(PointerEventData e)
//////    {
//////        if (_isDone || hintGlow == null) return;
//////        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
//////        if (tool == null || tool.toolID != acceptToolID) return;

//////        hintGlow.color = _yellow;
//////    }

//////    // ── Drop ──────────────────────────────────────────────────────────────
//////    public void OnDrop(PointerEventData e)
//////    {
//////        if (_isDone) return;

//////        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
//////        if (tool == null) return;

//////        if (tool.toolID == acceptToolID)
//////        {
//////            // Correct tool — run the cleaning sequence
//////            // We set isAnimating on the tool HERE (before OnEndDrag fires)
//////            // so that DraggableTool.OnEndDrag knows not to run PlayOnceAndReturn.
//////            StartCoroutine(RunSequence(tool));
//////        }
//////        else
//////        {
//////            // Wrong tool — shake this zone as feedback
//////            StartCoroutine(Shake());
//////        }
//////    }

//////    // ── Main sequence ─────────────────────────────────────────────────────
//////    IEnumerator RunSequence(DraggableTool tool)
//////    {
//////        _isDone = true;
//////        if (hintGlow != null) hintGlow.enabled = false;

//////        // 1. Hide the dirty/broken image INSTANTLY
//////        if (zoneImage != null)
//////            zoneImage.SetActive(false);

//////        // 2. Start tool animation (loops while zone cleans)
//////        //    This also sets tool.isAnimating = true so OnEndDrag skips SnapHome.
//////        tool.PlayToolAnim();

//////        // 3. Activate and play the zone cleaning animation ONCE
//////        float clipLength = 0f;

//////        if (zoneAnimation != null)
//////        {
//////            zoneAnimation.SetActive(true);
//////            Animation anim = zoneAnimation.GetComponent<Animation>();

//////            if (anim != null && !string.IsNullOrEmpty(zoneClipName))
//////            {
//////                anim[zoneClipName].wrapMode = WrapMode.Once;
//////                anim.Play(zoneClipName);

//////                AnimationClip clip = anim.GetClip(zoneClipName);
//////                if (clip != null) clipLength = clip.length;
//////            }
//////        }

//////        // 4. Wait for cleaning animation to finish
//////        yield return new WaitForSeconds(clipLength);

//////        // zoneAnimation stays active → frozen on last frame = cleaned look

//////        // 5. Stop tool animation and return icon to bar fully usable
//////        tool.StopToolAnim();

//////        // 6. Check if ALL zones are complete
//////        _doneCount++;
//////        if (_doneCount >= _all.Count)
//////        {
//////            foreach (var z in _all)
//////            {
//////                if (z.completionPanel != null)
//////                {
//////                    z.completionPanel.SetActive(true);
//////                    break;
//////                }
//////            }
//////        }
//////    }

//////    // ── Wrong-tool shake feedback ─────────────────────────────────────────
//////    IEnumerator Shake()
//////    {
//////        Vector3 orig = transform.localPosition;
//////        float elapsed = 0f;
//////        while (elapsed < 0.3f)
//////        {
//////            transform.localPosition = orig + new Vector3(Random.Range(-8f, 8f), 0, 0);
//////            elapsed += Time.deltaTime;
//////            yield return null;
//////        }
//////        transform.localPosition = orig;
//////    }

//////    // ── Static reset (called from GameManager) ────────────────────────────
//////    public static void ResetAll()
//////    {
//////        _doneCount = 0;
//////        foreach (var z in _all) z.ResetZone();
//////    }

//////    void ResetZone()
//////    {
//////        StopAllCoroutines();
//////        _isDone = false;

//////        if (zoneImage != null) zoneImage.SetActive(true);
//////        if (completionPanel != null) completionPanel.SetActive(false);

//////        if (zoneAnimation != null)
//////        {
//////            Animation anim = zoneAnimation.GetComponent<Animation>();
//////            if (anim != null) anim.Stop();
//////            zoneAnimation.SetActive(false);
//////        }

//////        if (hintGlow != null) hintGlow.enabled = false;
//////    }
//////}

////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;
////using UnityEngine.EventSystems;
////using UnityEngine.UI;

////// ══════════════════════════════════════════════════════════════════════════════
////// ToolDropZone.cs
////// Attach to each drop area (PhotoFrame, WastePile, GrassPatch, MudPatch).
//////
////// INSPECTOR FIELDS:
//////   acceptToolID   → must match toolID on the correct DraggableTool
//////                    e.g. PhotoFrame zone → 2  (Hammer = 2)
//////   zoneImage      → dirty/broken image to HIDE instantly on correct drop
//////   zoneAnimation  → GameObject with legacy Animation — plays the fix anim
//////                    set it INACTIVE in the scene
//////   zoneClipName   → exact clip name on zoneAnimation
//////   completionPanel→ shown when ALL zones are done (assign on ONE zone only)
//////   hintGlow       → optional child Image that glows when matching tool drags
////// ══════════════════════════════════════════════════════════════════════════════

////[RequireComponent(typeof(Image))]
////[RequireComponent(typeof(CanvasGroup))]
////public class ToolDropZone : MonoBehaviour,
////    IDropHandler, IPointerEnterHandler, IPointerExitHandler
////{
////    [Header("Identity")]
////    public int acceptToolID;

////    [Header("Zone Visuals")]
////    public GameObject zoneImage;

////    [Header("Zone Animation")]
////    public GameObject zoneAnimation;
////    public string zoneClipName;

////    [Header("Completion")]
////    public GameObject completionPanel;

////    [Header("Hint Highlight (optional)")]
////    public Image hintGlow;

////    // ── statics ───────────────────────────────────────────────────────────
////    private static readonly List<ToolDropZone> _all = new();
////    private static int _doneCount = 0;

////    // ── private ───────────────────────────────────────────────────────────
////    private bool _isDone = false;

////    static readonly Color _yellow = new Color(1f, 0.92f, 0f, 0.55f);
////    static readonly Color _green = new Color(0.2f, 1f, 0.2f, 0.65f);

////    // ─────────────────────────────────────────────────────────────────────
////    void Awake()
////    {
////        if (!_all.Contains(this)) _all.Add(this);

////        if (zoneImage != null) zoneImage.SetActive(true);
////        if (zoneAnimation != null) zoneAnimation.SetActive(false);
////        if (completionPanel != null) completionPanel.SetActive(false);
////        if (hintGlow != null) hintGlow.enabled = false;
////    }

////    void OnDestroy()
////    {
////        _all.Remove(this);
////        if (_all.Count == 0) _doneCount = 0;
////    }

////    // ── Hint glow (called by DraggableTool on begin/end drag) ─────────────
////    public static void ShowHints(int toolID, bool show)
////    {
////        foreach (var zone in _all)
////        {
////            if (zone._isDone) continue;
////            if (zone.acceptToolID != toolID) continue;
////            if (zone.hintGlow == null) continue;

////            zone.hintGlow.enabled = show;
////            zone.hintGlow.color = _yellow;
////        }
////    }

////    // ── Hover: turn glow green when correct tool is over this zone ────────
////    public void OnPointerEnter(PointerEventData e)
////    {
////        if (_isDone || hintGlow == null) return;
////        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
////        if (tool == null || tool.toolID != acceptToolID) return;

////        hintGlow.enabled = true;
////        hintGlow.color = _green;
////    }

////    public void OnPointerExit(PointerEventData e)
////    {
////        if (_isDone || hintGlow == null) return;
////        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
////        if (tool == null || tool.toolID != acceptToolID) return;

////        hintGlow.color = _yellow;
////    }

////    // ── Drop ──────────────────────────────────────────────────────────────
////    public void OnDrop(PointerEventData e)
////    {
////        if (_isDone) return;

////        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
////        if (tool == null) return;

////        if (tool.toolID == acceptToolID)
////            StartCoroutine(RunSequence(tool));  // correct tool
////        else
////            StartCoroutine(Shake());            // wrong tool — shake feedback
////    }

////    // ── Correct drop sequence ─────────────────────────────────────────────
////    IEnumerator RunSequence(DraggableTool tool)
////    {
////        _isDone = true;
////        if (hintGlow != null) hintGlow.enabled = false;

////        // 1. Hide the dirty/broken image instantly
////        if (zoneImage != null) zoneImage.SetActive(false);

////        // 2. Tell the tool to play its animation once and return to the bar.
////        //    This also sets tool.isAnimating = true so DraggableTool.OnEndDrag
////        //    knows not to snap back on its own.
////        tool.PlayToolAnim();

////        // 3. Play the zone's cleaning animation (once)
////        float clipLength = 0f;
////        if (zoneAnimation != null)
////        {
////            zoneAnimation.SetActive(true);
////            Animation anim = zoneAnimation.GetComponent<Animation>();
////            if (anim != null && !string.IsNullOrEmpty(zoneClipName))
////            {
////                AnimationClip clip = anim.GetClip(zoneClipName);
////                if (clip != null) clipLength = clip.length;

////                anim[zoneClipName].wrapMode = WrapMode.Once;
////                anim.Play(zoneClipName);
////            }
////        }

////        // 4. Wait for zone animation to finish
////        yield return new WaitForSeconds(clipLength);

////        // zoneAnimation stays active — frozen on its last frame (cleaned look)

////        // 5. Check completion
////        _doneCount++;
////        if (_doneCount >= _all.Count)
////        {
////            foreach (var z in _all)
////            {
////                if (z.completionPanel != null)
////                {
////                    z.completionPanel.SetActive(true);
////                    break;
////                }
////            }
////        }
////    }

////    // ── Wrong tool shake ──────────────────────────────────────────────────
////    IEnumerator Shake()
////    {
////        Vector3 orig = transform.localPosition;
////        float elapsed = 0f;
////        while (elapsed < 0.3f)
////        {
////            transform.localPosition = orig + new Vector3(Random.Range(-8f, 8f), 0, 0);
////            elapsed += Time.deltaTime;
////            yield return null;
////        }
////        transform.localPosition = orig;
////    }

////    // ── Reset (GameManager "Play Again") ──────────────────────────────────
////    public static void ResetAll()
////    {
////        _doneCount = 0;
////        foreach (var z in _all) z.ResetZone();
////    }

////    void ResetZone()
////    {
////        StopAllCoroutines();
////        _isDone = false;

////        if (zoneImage != null) zoneImage.SetActive(true);
////        if (completionPanel != null) completionPanel.SetActive(false);
////        if (hintGlow != null) hintGlow.enabled = false;

////        if (zoneAnimation != null)
////        {
////            Animation anim = zoneAnimation.GetComponent<Animation>();
////            if (anim != null) anim.Stop();
////            zoneAnimation.SetActive(false);
////        }
////    }
////}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//// ══════════════════════════════════════════════════════════════════════════════
//// ToolDropZone.cs
////
//// KEY FIX: alphaHitTestMinimumThreshold = 0.1f  (set in Awake)
////   Tells Unity to ignore pointer events on transparent pixels.
////   The drop zone will only respond where the image is actually visible.
////
//// REQUIRED TEXTURE SETTING (do this once per sprite in the Project window):
////   Select the sprite → Inspector → Advanced → Read/Write: ✓ enabled
////   Without this Unity cannot sample the pixel and the threshold is ignored.
//// ══════════════════════════════════════════════════════════════════════════════

//[RequireComponent(typeof(Image))]
//[RequireComponent(typeof(CanvasGroup))]
//public class ToolDropZone : MonoBehaviour,
//    IDropHandler, IPointerEnterHandler, IPointerExitHandler
//{
//    [Header("Identity")]
//    public int acceptToolID;

//    [Header("Zone Visuals")]
//    public GameObject zoneImage;

//    [Header("Zone Animation")]
//    public GameObject zoneAnimation;
//    public string zoneClipName;

//    [Header("Completion")]
//    public GameObject completionPanel;

//    [Header("Hint Highlight (optional)")]
//    public Image hintGlow;

//    // ── statics ───────────────────────────────────────────────────────────
//    private static readonly List<ToolDropZone> _all = new();
//    private static int _doneCount = 0;

//    // ── private ───────────────────────────────────────────────────────────
//    private bool _isDone = false;
//    private Image _image;

//    static readonly Color _yellow = new Color(1f, 0.92f, 0f, 0.55f);
//    static readonly Color _green = new Color(0.2f, 1f, 0.2f, 0.65f);

//    // ─────────────────────────────────────────────────────────────────────
//    void Awake()
//    {
//        if (!_all.Contains(this)) _all.Add(this);

//        _image = GetComponent<Image>();

//        // ── CORE FIX ─────────────────────────────────────────────────────
//        // Only register pointer/drop events on pixels that are at least
//        // 10 % opaque. Fully transparent parts of the image are ignored,
//        // so overlapping zones no longer steal each other's drops.
//        //
//        // The sprite texture MUST have Read/Write enabled:
//        //   Project window → select sprite → Inspector → Advanced
//        //   → tick "Read/Write"
//        // ─────────────────────────────────────────────────────────────────
//        _image.alphaHitTestMinimumThreshold = 0.1f;

//        if (zoneImage != null) zoneImage.SetActive(true);
//        if (zoneAnimation != null) zoneAnimation.SetActive(false);
//        if (completionPanel != null) completionPanel.SetActive(false);
//        if (hintGlow != null) hintGlow.enabled = false;
//    }

//    void OnDestroy()
//    {
//        _all.Remove(this);
//        if (_all.Count == 0) _doneCount = 0;
//    }

//    // ── Hint glow ─────────────────────────────────────────────────────────
//    public static void ShowHints(int toolID, bool show)
//    {
//        foreach (var zone in _all)
//        {
//            if (zone._isDone) continue;
//            if (zone.acceptToolID != toolID) continue;
//            if (zone.hintGlow == null) continue;

//            zone.hintGlow.enabled = show;
//            zone.hintGlow.color = _yellow;
//        }
//    }

//    // ── Hover ─────────────────────────────────────────────────────────────
//    public void OnPointerEnter(PointerEventData e)
//    {
//        if (_isDone || hintGlow == null) return;
//        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
//        if (tool == null || tool.toolID != acceptToolID) return;

//        hintGlow.enabled = true;
//        hintGlow.color = _green;
//    }

//    public void OnPointerExit(PointerEventData e)
//    {
//        if (_isDone || hintGlow == null) return;
//        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
//        if (tool == null || tool.toolID != acceptToolID) return;

//        hintGlow.color = _yellow;
//    }

//    // ── Drop ──────────────────────────────────────────────────────────────
//    public void OnDrop(PointerEventData e)
//    {
//        if (_isDone) return;

//        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
//        if (tool == null) return;

//        if (tool.toolID == acceptToolID)
//            StartCoroutine(RunSequence(tool));
//        else
//            StartCoroutine(Shake());
//    }

//    // ── Correct drop sequence ─────────────────────────────────────────────
//    IEnumerator RunSequence(DraggableTool tool)
//    {
//        _isDone = true;
//        if (hintGlow != null) hintGlow.enabled = false;

//        // 1. Hide the dirty/broken image instantly
//        if (zoneImage != null) zoneImage.SetActive(false);

//        // 2. Tell the tool to play its animation once then return to the bar
//        tool.PlayToolAnim();

//        // 3. Play the zone cleaning animation once
//        float clipLength = 0f;
//        if (zoneAnimation != null)
//        {
//            zoneAnimation.SetActive(true);
//            Animation anim = zoneAnimation.GetComponent<Animation>();
//            if (anim != null && !string.IsNullOrEmpty(zoneClipName))
//            {
//                AnimationClip clip = anim.GetClip(zoneClipName);
//                if (clip != null) clipLength = clip.length;

//                anim[zoneClipName].wrapMode = WrapMode.Once;
//                anim.Play(zoneClipName);
//            }
//        }

//        // 4. Wait for zone animation to finish
//        yield return new WaitForSeconds(clipLength);

//        // zoneAnimation stays active — frozen on last frame (cleaned look)

//        // 5. Check if all zones are done
//        _doneCount++;
//        if (_doneCount >= _all.Count)
//        {
//            foreach (var z in _all)
//            {
//                if (z.completionPanel != null)
//                {
//                    z.completionPanel.SetActive(true);
//                    break;
//                }
//            }
//        }
//    }

//    // ── Wrong tool shake ──────────────────────────────────────────────────
//    IEnumerator Shake()
//    {
//        Vector3 orig = transform.localPosition;
//        float elapsed = 0f;
//        while (elapsed < 0.3f)
//        {
//            transform.localPosition = orig + new Vector3(Random.Range(-8f, 8f), 0, 0);
//            elapsed += Time.deltaTime;
//            yield return null;
//        }
//        transform.localPosition = orig;
//    }

//    // ── Reset ─────────────────────────────────────────────────────────────
//    public static void ResetAll()
//    {
//        _doneCount = 0;
//        foreach (var z in _all) z.ResetZone();
//    }

//    void ResetZone()
//    {
//        StopAllCoroutines();
//        _isDone = false;

//        if (zoneImage != null) zoneImage.SetActive(true);
//        if (completionPanel != null) completionPanel.SetActive(false);
//        if (hintGlow != null) hintGlow.enabled = false;

//        if (zoneAnimation != null)
//        {
//            Animation anim = zoneAnimation.GetComponent<Animation>();
//            if (anim != null) anim.Stop();
//            zoneAnimation.SetActive(false);
//        }
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ══════════════════════════════════════════════════════════════════════════════
// ToolDropZone.cs
//
// KEY FIX: alphaHitTestMinimumThreshold = 0.1f  (set in Awake)
//   Tells Unity to ignore pointer events on transparent pixels.
//   The drop zone will only respond where the image is actually visible.
//
// REQUIRED TEXTURE SETTING (do this once per sprite in the Project window):
//   Select the sprite → Inspector → Advanced → Read/Write: ✓ enabled
//   Without this Unity cannot sample the pixel and the threshold is ignored.
// ══════════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class ToolDropZone : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Identity")]
    public int acceptToolID;

    [Header("Zone Visuals")]
    public GameObject zoneImage;

    [Header("Zone Animation")]
    public GameObject zoneAnimation;
    public string zoneClipName;

    [Header("Completion")]
    public GameObject completionPanel;

    [Header("Hint Highlight (optional)")]
    public Image hintGlow;

    // ── statics ───────────────────────────────────────────────────────────
    private static readonly List<ToolDropZone> _all = new();
    private static int _doneCount = 0;

    // ── private ───────────────────────────────────────────────────────────
    private bool _isDone = false;
    private Image _image;

    static readonly Color _yellow = new Color(1f, 0.92f, 0f, 0.55f);
    static readonly Color _green = new Color(0.2f, 1f, 0.2f, 0.65f);

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (!_all.Contains(this)) _all.Add(this);

        _image = GetComponent<Image>();

        // ── CORE FIX ─────────────────────────────────────────────────────
        // Only register pointer/drop events on pixels that are at least
        // 10 % opaque. Fully transparent parts of the image are ignored,
        // so overlapping zones no longer steal each other's drops.
        //
        // The sprite texture MUST have Read/Write enabled:
        //   Project window → select sprite → Inspector → Advanced
        //   → tick "Read/Write"
        // ─────────────────────────────────────────────────────────────────
        _image.alphaHitTestMinimumThreshold = 0.1f;

        if (zoneImage != null) zoneImage.SetActive(true);
        if (zoneAnimation != null) zoneAnimation.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
        if (hintGlow != null) hintGlow.enabled = false;
    }

    void OnDestroy()
    {
        _all.Remove(this);
        if (_all.Count == 0) _doneCount = 0;
    }

    // ── Hint glow ─────────────────────────────────────────────────────────
    public static void ShowHints(int toolID, bool show)
    {
        foreach (var zone in _all)
        {
            if (zone._isDone) continue;
            if (zone.acceptToolID != toolID) continue;
            if (zone.hintGlow == null) continue;

            zone.hintGlow.enabled = show;
            zone.hintGlow.color = _yellow;
        }
    }

    // ── Hover ─────────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData e)
    {
        if (_isDone || hintGlow == null) return;
        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
        if (tool == null || tool.toolID != acceptToolID) return;

        hintGlow.enabled = true;
        hintGlow.color = _green;
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (_isDone || hintGlow == null) return;
        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
        if (tool == null || tool.toolID != acceptToolID) return;

        hintGlow.color = _yellow;
    }

    // ── Drop ──────────────────────────────────────────────────────────────
    public void OnDrop(PointerEventData e)
    {
        if (_isDone) return;

        var tool = e.pointerDrag?.GetComponent<DraggableTool>();
        if (tool == null) return;

        if (tool.toolID == acceptToolID)
            StartCoroutine(RunSequence(tool));
        else
            StartCoroutine(Shake());
    }

    // ── Correct drop sequence ─────────────────────────────────────────────
    IEnumerator RunSequence(DraggableTool tool)
    {
        _isDone = true;
        if (hintGlow != null) hintGlow.enabled = false;

        // 1. Hide the dirty/broken image instantly
        if (zoneImage != null) zoneImage.SetActive(false);

        // 2. Tell the tool to play BOTH its toolbar icon animation AND its
        //    scene animation simultaneously. isAnimating is set inside PlayToolAnim.
        tool.PlayToolAnim();

        // 3. Play the zone cleaning animation once
        float clipLength = 0f;
        if (zoneAnimation != null)
        {
            zoneAnimation.SetActive(true);
            Animation anim = zoneAnimation.GetComponent<Animation>();
            if (anim != null && !string.IsNullOrEmpty(zoneClipName))
            {
                AnimationClip clip = anim.GetClip(zoneClipName);
                if (clip != null) clipLength = clip.length;

                anim[zoneClipName].wrapMode = WrapMode.Once;
                anim.Play(zoneClipName);
            }
        }

        // 4. Wait for zone animation to finish
        yield return new WaitForSeconds(clipLength);

        // zoneAnimation stays active — frozen on last frame (cleaned look)

        // 5. Stop the scene tool animation now that the zone is done
        tool.StopToolAnim();

        // 6. Check if all zones are done
        _doneCount++;
        if (_doneCount >= _all.Count)
        {
            foreach (var z in _all)
            {
                if (z.completionPanel != null)
                {
                    z.completionPanel.SetActive(true);
                    break;
                }
            }
        }
    }

    // ── Wrong tool shake ──────────────────────────────────────────────────
    IEnumerator Shake()
    {
        Vector3 orig = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            transform.localPosition = orig + new Vector3(Random.Range(-8f, 8f), 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = orig;
    }

    // ── Reset ─────────────────────────────────────────────────────────────
    public static void ResetAll()
    {
        _doneCount = 0;
        foreach (var z in _all) z.ResetZone();
    }

    void ResetZone()
    {
        StopAllCoroutines();
        _isDone = false;

        if (zoneImage != null) zoneImage.SetActive(true);
        if (completionPanel != null) completionPanel.SetActive(false);
        if (hintGlow != null) hintGlow.enabled = false;

        if (zoneAnimation != null)
        {
            Animation anim = zoneAnimation.GetComponent<Animation>();
            if (anim != null) anim.Stop();
            zoneAnimation.SetActive(false);
        }
    }
}