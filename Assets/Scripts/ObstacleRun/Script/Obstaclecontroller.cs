////////using UnityEngine;

/////////// <summary>
/////////// Attach this script to every obstacle Image in the scene.
/////////// 
/////////// IMPORTANT — Unity Hierarchy setup:
///////////   Make each obstacle a CHILD of BGImage so it scrolls
///////////   automatically with the background. No extra scroll code needed.
///////////
/////////// Inspector setup:
///////////   Cow Rect       → drag the Cow RectTransform
///////////   Cow Controller → drag the Cow GameObject
/////////// </summary>
////////public class ObstacleController : MonoBehaviour
////////{
////////    [Header("References")]
////////    public RectTransform cowRect;
////////    public Cowanimationcontroller cowController;

////////    [Header("Collision Shrink")]
////////    [Tooltip("Shrinks the obstacle hitbox on all sides so it feels fair. Try 10-20.")]
////////    public float hitboxPadding = 15f;

////////    private RectTransform obstacleRect;
////////    private bool triggered = false;

////////    // ──────────────────────────────────────────────────────────
////////    void Start()
////////    {
////////        obstacleRect = GetComponent<RectTransform>();
////////    }

////////    void Update()
////////    {
////////        if (triggered) return;
////////        if (cowRect == null || cowController == null) return;

////////        // Don't check while game is already over or cow has won
////////        if (cowController.CurrentState == Cowanimationcontroller.CowState.Win) return;
////////        if (cowController.CurrentState == Cowanimationcontroller.CowState.Hit) return;

////////        if (Overlaps(cowRect, obstacleRect))
////////        {
////////            triggered = true;
////////            Debug.Log($"[Obstacle] Cow hit {gameObject.name}!");

////////            // Tell cow to play Hit animation
////////            cowController.PlayHit();

////////            // Tell GameManager to show Game Over panel
////////            if (GameManager.Instance != null)
////////                GameManager.Instance.ShowGameOver();
////////            else
////////                Debug.LogWarning("[Obstacle] GameManager.Instance is null!");
////////        }
////////    }

////////    // ──────────────────────────────────────────────────────────
////////    // Shrinks each rect inward by hitboxPadding before checking
////////    // overlap so collisions feel fair and not cheap.
////////    // ──────────────────────────────────────────────────────────
////////    bool Overlaps(RectTransform a, RectTransform b)
////////    {
////////        Rect ra = GetWorldRect(a);
////////        Rect rb = GetWorldRect(b);

////////        // Shrink obstacle hitbox by padding
////////        rb.xMin += hitboxPadding;
////////        rb.xMax -= hitboxPadding;
////////        rb.yMin += hitboxPadding;
////////        rb.yMax -= hitboxPadding;

////////        // Shrink cow hitbox slightly too for fairness
////////        ra.xMin += hitboxPadding * 0.5f;
////////        ra.xMax -= hitboxPadding * 0.5f;

////////        return ra.Overlaps(rb);
////////    }

////////    Rect GetWorldRect(RectTransform rt)
////////    {
////////        Vector3[] c = new Vector3[4];
////////        rt.GetWorldCorners(c);
////////        return new Rect(c[0].x, c[0].y, c[2].x - c[0].x, c[2].y - c[0].y);
////////    }
////////}


//////using UnityEngine;
//////using UnityEngine.UI;

///////// <summary>
///////// Attach to every obstacle Image.
///////// Obstacle must be a CHILD of BGImage so it scrolls automatically.
///////// Drag Cow Rect and Cow Controller in the Inspector.
///////// </summary>
//////public class ObstacleController : MonoBehaviour
//////{
//////    [Header("References")]
//////    public RectTransform cowRect;
//////    public Cowanimationcontroller cowController;

//////    [Header("Collision Shrink (pixels)")]
//////    [Tooltip("Shrinks the hitbox on all sides so it feels fair. Try 10-20.")]
//////    public float hitboxPadding = 15f;

//////    private RectTransform obstacleRect;
//////    private bool triggered = false;

//////    // ──────────────────────────────────────────────────────────
//////    void Start()
//////    {
//////        obstacleRect = GetComponent<RectTransform>();

//////        // Auto-find cow references if not set in Inspector
//////        if (cowController == null)
//////            cowController = FindObjectOfType<Cowanimationcontroller>();

//////        if (cowRect == null && cowController != null)
//////            cowRect = cowController.GetComponent<RectTransform>();

//////        if (cowController == null)
//////            Debug.LogError($"[Obstacle] {gameObject.name}: CowController not found! Drag it in the Inspector.");
//////    }

//////    void Update()
//////    {
//////        if (triggered) return;
//////        if (cowController == null || cowRect == null) return;

//////        // Only check while cow is actively running or jumping
//////        CowState s = cowController.CurrentState;
//////        if (s == Cowanimationcontroller.CowState.Win) return;
//////        if (s == Cowanimationcontroller.CowState.Hit) return;
//////        if (s == Cowanimationcontroller.CowState.Idle) return;

//////        if (!Overlaps(cowRect, obstacleRect)) return;

//////        // ── Hit! ───────────────────────────────────────────────
//////        triggered = true;
//////        Debug.Log($"[Obstacle] {gameObject.name} hit the cow!");

//////        cowController.PlayHit();

//////        // Try GameManager first, fallback to direct panel show
//////        if (GameManager.Instance != null)
//////        {
//////            GameManager.Instance.ShowGameOver();
//////        }
//////        else
//////        {
//////            Debug.LogError("[Obstacle] GameManager.Instance is NULL — " +
//////                           "make sure GameManager script is in the scene and Awake() sets Instance.");
//////        }
//////    }

//////    // ──────────────────────────────────────────────────────────
//////    // Uses anchoredPosition math instead of GetWorldCorners so
//////    // it works correctly on any Canvas render mode.
//////    // ──────────────────────────────────────────────────────────
//////    bool Overlaps(RectTransform a, RectTransform b)
//////    {
//////        Rect ra = GetCanvasRect(a);
//////        Rect rb = GetCanvasRect(b);

//////        // Shrink hitboxes for fairness
//////        ra.xMin += hitboxPadding * 0.5f;
//////        ra.xMax -= hitboxPadding * 0.5f;
//////        rb.xMin += hitboxPadding;
//////        rb.xMax -= hitboxPadding;
//////        rb.yMin += hitboxPadding;
//////        rb.yMax -= hitboxPadding;

//////        return ra.Overlaps(rb);
//////    }

//////    // Converts a RectTransform to a canvas-space Rect
//////    // Works correctly regardless of Canvas render mode
//////    Rect GetCanvasRect(RectTransform rt)
//////    {
//////        Vector3[] corners = new Vector3[4];
//////        rt.GetWorldCorners(corners);

//////        // Find the root Canvas and convert to its local space
//////        Canvas canvas = rt.GetComponentInParent<Canvas>();
//////        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
//////        {
//////            // For Camera/World space canvases, transform to canvas local space
//////            for (int i = 0; i < 4; i++)
//////                corners[i] = canvas.transform.InverseTransformPoint(corners[i]);
//////        }

//////        float xMin = Mathf.Min(corners[0].x, corners[2].x);
//////        float xMax = Mathf.Max(corners[0].x, corners[2].x);
//////        float yMin = Mathf.Min(corners[0].y, corners[2].y);
//////        float yMax = Mathf.Max(corners[0].y, corners[2].y);

//////        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
//////    }

//////    // Helper alias so CowState is accessible without full path
//////    private static class CowState
//////    {
//////        public static Cowanimationcontroller.CowState Win = Cowanimationcontroller.CowState.Win;
//////        public static Cowanimationcontroller.CowState Hit = Cowanimationcontroller.CowState.Hit;
//////        public static Cowanimationcontroller.CowState Idle = Cowanimationcontroller.CowState.Idle;
//////    }
//////}

////using UnityEngine;
////using UnityEngine.UI;

/////// <summary>
/////// Attach to every obstacle Image.
/////// Obstacle must be a CHILD of BGImage so it scrolls automatically.
/////// Drag Cow Rect and Cow Controller in the Inspector.
/////// </summary>
////public class ObstacleController : MonoBehaviour
////{
////    [Header("References")]
////    public RectTransform cowRect;
////    public Cowanimationcontroller cowController;

////    [Header("Collision Shrink (pixels)")]
////    [Tooltip("Shrinks the hitbox on all sides so it feels fair. Try 10-20.")]
////    public float hitboxPadding = 15f;

////    private RectTransform obstacleRect;
////    private bool triggered = false;

////    // ──────────────────────────────────────────────────────────
////    void Start()
////    {
////        obstacleRect = GetComponent<RectTransform>();

////        // Auto-find cow if not assigned in Inspector
////        if (cowController == null)
////            cowController = Object.FindFirstObjectByType<Cowanimationcontroller>();

////        if (cowRect == null && cowController != null)
////            cowRect = cowController.GetComponent<RectTransform>();

////        if (cowController == null)
////            Debug.LogError($"[Obstacle] {gameObject.name}: CowController not found! Drag it in the Inspector.");
////    }

////    void Update()
////    {
////        if (triggered) return;
////        if (cowController == null || cowRect == null) return;

////        // Only check while cow is actively running or jumping
////        Cowanimationcontroller.CowState s = cowController.CurrentState;
////        if (s == Cowanimationcontroller.CowState.Win) return;
////        if (s == Cowanimationcontroller.CowState.Hit) return;
////        if (s == Cowanimationcontroller.CowState.Idle) return;

////        if (!Overlaps(cowRect, obstacleRect)) return;

////        // ── Hit! ───────────────────────────────────────────────
////        triggered = true;
////        Debug.Log($"[Obstacle] {gameObject.name} hit the cow!");

////        cowController.PlayHit();

////        if (GameManager.Instance != null)
////            GameManager.Instance.ShowGameOver();
////        else
////            Debug.LogError("[Obstacle] GameManager.Instance is NULL — make sure GameManager is in the scene.");
////    }

////    // ──────────────────────────────────────────────────────────
////    bool Overlaps(RectTransform a, RectTransform b)
////    {
////        Rect ra = GetCanvasRect(a);
////        Rect rb = GetCanvasRect(b);

////        // Shrink hitboxes for fairness
////        ra.xMin += hitboxPadding * 0.5f;
////        ra.xMax -= hitboxPadding * 0.5f;
////        rb.xMin += hitboxPadding;
////        rb.xMax -= hitboxPadding;
////        rb.yMin += hitboxPadding;
////        rb.yMax -= hitboxPadding;

////        return ra.Overlaps(rb);
////    }

////    // Works correctly regardless of Canvas render mode
////    Rect GetCanvasRect(RectTransform rt)
////    {
////        Vector3[] corners = new Vector3[4];
////        rt.GetWorldCorners(corners);

////        Canvas canvas = rt.GetComponentInParent<Canvas>();
////        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
////        {
////            for (int i = 0; i < 4; i++)
////                corners[i] = canvas.transform.InverseTransformPoint(corners[i]);
////        }

////        float xMin = Mathf.Min(corners[0].x, corners[2].x);
////        float xMax = Mathf.Max(corners[0].x, corners[2].x);
////        float yMin = Mathf.Min(corners[0].y, corners[2].y);
////        float yMax = Mathf.Max(corners[0].y, corners[2].y);

////        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
////    }
////}

//using UnityEngine;

///// <summary>
///// Attach to every obstacle Image.
///// 
///// Setup in Unity:
/////   1. Add this script to your obstacle Image GameObject
/////   2. Create a child GameObject inside the obstacle → name it "HitZone"
/////   3. Add a RectTransform to HitZone (it gets one automatically as a UI child)
/////   4. Position and resize the HitZone RectTransform visually in the Scene view
/////      to cover only the part of the obstacle you want to be deadly
/////   5. Drag the HitZone into the "Hit Zone" slot in this Inspector
/////   6. Drag Cow Rect and Cow Controller (or leave blank — auto-found)
///// </summary>
//public class ObstacleController : MonoBehaviour
//{
//    [Header("Hit Zone")]
//    [Tooltip("Drag the HitZone child GameObject's RectTransform here.")]
//    public RectTransform hitZone;

//    [Header("Cow References")]
//    public RectTransform cowRect;
//    public Cowanimationcontroller cowController;

//    private bool triggered = false;

//    // ──────────────────────────────────────────────────────────
//    void Start()
//    {
//        // Auto-find cow if not assigned
//        if (cowController == null)
//            cowController = Object.FindFirstObjectByType<Cowanimationcontroller>();

//        if (cowRect == null && cowController != null)
//            cowRect = cowController.GetComponent<RectTransform>();

//        if (hitZone == null)
//            Debug.LogError($"[Obstacle] {gameObject.name}: HitZone is not assigned! " +
//                           "Create a child GameObject, resize it, and drag it into the Hit Zone slot.");

//        if (cowController == null)
//            Debug.LogError($"[Obstacle] {gameObject.name}: CowController not found!");
//    }

//    void Update()
//    {
//        if (triggered) return;
//        if (cowController == null || cowRect == null || hitZone == null) return;

//        // Only check while cow is actively running or jumping
//        Cowanimationcontroller.CowState s = cowController.CurrentState;
//        if (s == Cowanimationcontroller.CowState.Win) return;
//        if (s == Cowanimationcontroller.CowState.Hit) return;
//        if (s == Cowanimationcontroller.CowState.Idle) return;

//        if (!Overlaps(cowRect, hitZone)) return;

//        // ── Hit! ───────────────────────────────────────────────
//        triggered = true;
//        Debug.Log($"[Obstacle] Cow entered HitZone of {gameObject.name}!");

//        cowController.PlayHit();

//        if (GameManager.Instance != null)
//            GameManager.Instance.ShowGameOver();
//        else
//            Debug.LogError("[Obstacle] GameManager.Instance is NULL!");
//    }

//    // ──────────────────────────────────────────────────────────
//    // Checks if the cow RectTransform overlaps the HitZone rect
//    // ──────────────────────────────────────────────────────────
//    bool Overlaps(RectTransform a, RectTransform b)
//    {
//        return GetCanvasRect(a).Overlaps(GetCanvasRect(b));
//    }

//    Rect GetCanvasRect(RectTransform rt)
//    {
//        Vector3[] corners = new Vector3[4];
//        rt.GetWorldCorners(corners);

//        Canvas canvas = rt.GetComponentInParent<Canvas>();
//        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
//        {
//            for (int i = 0; i < 4; i++)
//                corners[i] = canvas.transform.InverseTransformPoint(corners[i]);
//        }

//        float xMin = Mathf.Min(corners[0].x, corners[2].x);
//        float xMax = Mathf.Max(corners[0].x, corners[2].x);
//        float yMin = Mathf.Min(corners[0].y, corners[2].y);
//        float yMax = Mathf.Max(corners[0].y, corners[2].y);

//        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
//    }
//}

using UnityEngine;

/// <summary>
/// Attach to every obstacle Image.
///
/// Setup in Unity:
///
///  OBSTACLE HitZone:
///   1. Select your obstacle Image → right-click → UI → Empty → name it "HitZone"
///   2. Resize and position HitZone in the Scene view to cover the deadly area
///   3. Drag HitZone into the "Obstacle Hit Zone" slot below
///
///  COW HitZone:
///   1. Select the Cow GameObject → right-click → UI → Empty → name it "CowHitZone"
///   2. Resize and position CowHitZone in the Scene view (e.g. just the cow's body, not horns/tail)
///   3. Drag CowHitZone into the "Cow Hit Zone" slot below
///   4. You only need to do this ONCE — then reuse the same CowHitZone on every obstacle
/// </summary>
public class ObstacleController : MonoBehaviour
{
    [Header("Cow References")]
    [Tooltip("Drag the CowHitZone child GameObject from inside the Cow.")]
    public RectTransform cowHitZone;
    public Cowanimationcontroller cowController;

    [Header("Obstacle Hit Zone")]
    [Tooltip("Drag the HitZone child GameObject from inside this obstacle.")]
    public RectTransform obstacleHitZone;

    private bool triggered = false;

    // ──────────────────────────────────────────────────────────
    void Start()
    {
        // Auto-find cow controller if not assigned
        if (cowController == null)
            cowController = Object.FindFirstObjectByType<Cowanimationcontroller>();

        // Auto-find CowHitZone if not assigned
        if (cowHitZone == null && cowController != null)
        {
            Transform found = cowController.transform.Find("CowHitZone");
            if (found != null)
                cowHitZone = found.GetComponent<RectTransform>();
        }

        if (cowHitZone == null)
            Debug.LogError($"[Obstacle] {gameObject.name}: CowHitZone not assigned! " +
                           "Create a child on the Cow named 'CowHitZone' and drag it in.");

        if (obstacleHitZone == null)
            Debug.LogError($"[Obstacle] {gameObject.name}: Obstacle HitZone not assigned! " +
                           "Create a child on this obstacle and drag it in.");

        if (cowController == null)
            Debug.LogError($"[Obstacle] {gameObject.name}: CowController not found!");
    }

    void Update()
    {
        if (triggered) return;
        if (cowController == null || cowHitZone == null || obstacleHitZone == null) return;

        Cowanimationcontroller.CowState s = cowController.CurrentState;
        if (s == Cowanimationcontroller.CowState.Win) return;
        if (s == Cowanimationcontroller.CowState.Hit) return;
        if (s == Cowanimationcontroller.CowState.Idle) return;

        if (!Overlaps(cowHitZone, obstacleHitZone)) return;

        // ── Hit! ───────────────────────────────────────────────
        triggered = true;
        Debug.Log($"[Obstacle] CowHitZone touched HitZone of {gameObject.name}!");

        cowController.PlayHit();

        if (GameManager.Instance != null)
            GameManager.Instance.ShowGameOver();
        else
            Debug.LogError("[Obstacle] GameManager.Instance is NULL!");
    }

    // ──────────────────────────────────────────────────────────
    bool Overlaps(RectTransform a, RectTransform b)
    {
        return GetCanvasRect(a).Overlaps(GetCanvasRect(b));
    }

    Rect GetCanvasRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Canvas canvas = rt.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            for (int i = 0; i < 4; i++)
                corners[i] = canvas.transform.InverseTransformPoint(corners[i]);
        }

        float xMin = Mathf.Min(corners[0].x, corners[2].x);
        float xMax = Mathf.Max(corners[0].x, corners[2].x);
        float yMin = Mathf.Min(corners[0].y, corners[2].y);
        float yMax = Mathf.Max(corners[0].y, corners[2].y);

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}