//using UnityEngine;
//using UnityEngine.UI;

///// <summary>
///// Attach to the Bell UI Image GameObject.
///// Detects when the cow's RectTransform overlaps the bell's RectTransform.
///// No physics needed — pure UI rect overlap check.
/////
///// Setup:
/////   1. Create UI Image → assign bell sprite → name it "Bell"
/////   2. Attach this script to the Bell Image
/////   3. Drag Cow and CowController into the Inspector fields
///// </summary>
//public class BellTrigger : MonoBehaviour
//{
//    [Header("References")]
//    public RectTransform cowRect;        // drag Cow RectTransform here
//    public Cowanimationcontroller cowController;  // drag Cow script here

//    [Header("Bell Settings")]
//    public bool ringOnce = true;   // trigger only once

//    private RectTransform bellRect;
//    private bool hasRung = false;

//    // ─────────────────────────────────────────────────────────
//    void Start()
//    {
//        bellRect = GetComponent<RectTransform>();
//    }

//    void Update()
//    {
//        if (ringOnce && hasRung) return;
//        if (cowRect == null || cowController == null) return;

//        // ✅ Check if cow rect overlaps bell rect
//        if (RectOverlaps(cowRect, bellRect))
//        {
//            hasRung = true;

//            cowController.PlayHitThenWin();

//            // ✅ Friends celebrate
//            foreach (var f in friends)
//                f.PlayCelebrate();
//        }
//    }

//    // ─────────────────────────────────────────────────────────
//    // Checks if two RectTransforms overlap in UI space
//    // ─────────────────────────────────────────────────────────
//    bool RectOverlaps(RectTransform a, RectTransform b)
//    {
//        Rect aRect = GetWorldRect(a);
//        Rect bRect = GetWorldRect(b);
//        return aRect.Overlaps(bRect);
//    }

//    Rect GetWorldRect(RectTransform rt)
//    {
//        Vector3[] corners = new Vector3[4];
//        rt.GetWorldCorners(corners);
//        // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right
//        return new Rect(
//            corners[0].x,
//            corners[0].y,
//            corners[2].x - corners[0].x,
//            corners[2].y - corners[0].y
//        );
//    }

//    // ─────────────────────────────────────────────────────────
//    // Optional: reset so bell can ring again
//    // ─────────────────────────────────────────────────────────
//    public void ResetBell()
//    {
//        hasRung = false;
//    }
//}

using UnityEngine;
using UnityEngine.UI;

public class BellTrigger : MonoBehaviour
{
    [Header("References")]
    public RectTransform cowRect;
    public Cowanimationcontroller cowController;

    [Header("Friends (Celebrate Only)")]
    public FriendAnimationController[] friends;   // ✅ ADD THIS

    [Header("Bell Settings")]
    public bool ringOnce = true;

    private RectTransform bellRect;
    private bool hasRung = false;

    // ─────────────────────────────────────────────────────────
    void Start()
    {
        bellRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (ringOnce && hasRung) return;
        if (cowRect == null || cowController == null) return;

        if (RectOverlaps(cowRect, bellRect))
        {
            hasRung = true;

            Debug.Log("[Bell] Cow reached bell!");

            // ✅ Cow animation
            cowController.PlayHitThenWin();

            // ✅ Friends celebrate
            if (friends != null)
            {
                foreach (var f in friends)
                {
                    if (f != null)
                        f.PlayCelebrate();
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect aRect = GetWorldRect(a);
        Rect bRect = GetWorldRect(b);
        return aRect.Overlaps(bRect);
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        return new Rect(
            corners[0].x,
            corners[0].y,
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );
    }

    // ─────────────────────────────────────────────────────────
    public void ResetBell()
    {
        hasRung = false;
    }
}