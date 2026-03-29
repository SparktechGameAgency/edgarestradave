////////////using UnityEngine;

////////////public class CameraFollow : MonoBehaviour
////////////{
////////////    public RectTransform target; // your cow
////////////    public float smoothSpeed = 5f;

////////////    private Vector3 velocity = Vector3.zero;

////////////    void LateUpdate()
////////////    {
////////////        if (target == null) return;

////////////        Vector3 targetPos = new Vector3(
////////////            target.position.x,
////////////            transform.position.y,
////////////            transform.position.z
////////////        );

////////////        transform.position = Vector3.SmoothDamp(
////////////            transform.position,
////////////            targetPos,
////////////            ref velocity,
////////////            0.2f
////////////        );
////////////    }
////////////}

//////////using UnityEngine;

//////////public class CameraFollowUI : MonoBehaviour
//////////{
//////////    public RectTransform cow;   // your cow
//////////    public float followStartX = 630f;
//////////    public float smoothSpeed = 5f;

//////////    private float initialCamX;

//////////    void Start()
//////////    {
//////////        initialCamX = transform.position.x;
//////////    }

//////////    void LateUpdate()
//////////    {
//////////        if (cow == null) return;

//////////        // Convert cow UI position to world position
//////////        float cowWorldX = cow.position.x;

//////////        // Only follow AFTER crossing threshold
//////////        if (cowWorldX > followStartX)
//////////        {
//////////            float targetX = cowWorldX;

//////////            Vector3 targetPos = new Vector3(
//////////                targetX,
//////////                transform.position.y,
//////////                transform.position.z
//////////            );

//////////            transform.position = Vector3.Lerp(
//////////                transform.position,
//////////                targetPos,
//////////                smoothSpeed * Time.deltaTime
//////////            );
//////////        }
//////////        else
//////////        {
//////////            // Stay at initial position
//////////            Vector3 pos = transform.position;
//////////            pos.x = initialCamX;
//////////            transform.position = pos;
//////////        }
//////////    }
//////////}

////////using UnityEngine;

////////public class CameraFollowUI : MonoBehaviour
////////{
////////    public RectTransform cow;   // your cow
////////    public float followStartX = 630f;
////////    public float smoothSpeed = 5f;

////////    private float initialCamX;

////////    void Start()
////////    {
////////        initialCamX = transform.position.x;
////////    }

////////    void LateUpdate()
////////    {
////////        if (cow == null) return;

////////        // Convert cow UI position to world position
////////        float cowWorldX = cow.position.x;

////////        // Only follow AFTER crossing threshold
////////        if (cowWorldX > followStartX)
////////        {
////////            float targetX = cowWorldX;

////////            Vector3 targetPos = new Vector3(
////////                targetX,
////////                transform.position.y,
////////                transform.position.z
////////            );

////////            transform.position = Vector3.Lerp(
////////                transform.position,
////////                targetPos,
////////                smoothSpeed * Time.deltaTime
////////            );
////////        }
////////        else
////////        {
////////            // Stay at initial position
////////            Vector3 pos = transform.position;
////////            pos.x = initialCamX;
////////            transform.position = pos;
////////        }
////////    }
////////}

//////using UnityEngine;

//////public class CameraFollowCow : MonoBehaviour
//////{
//////    public Transform cow;
//////    public float followStartX = 630f;
//////    public float smoothSpeed = 5f;

//////    private bool shouldFollow = false;

//////    void LateUpdate()
//////    {
//////        if (cow == null) return;

//////        // Activate follow only after cow crosses X = 630
//////        if (cow.position.x >= followStartX)
//////        {
//////            shouldFollow = true;
//////        }

//////        if (shouldFollow)
//////        {
//////            Vector3 targetPos = new Vector3(cow.position.x, transform.position.y, transform.position.z);
//////            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
//////        }
//////    }
//////}

////using UnityEngine;

////public class CameraFollow : MonoBehaviour
////{
////    public Transform target;
////    public float smoothSpeed = 5f;
////    public Vector3 offset;

////    void LateUpdate()
////    {
////        if (target == null) return;

////        Vector3 desiredPosition = target.position + offset;
////        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

////        transform.position = new Vector3(smoothedPosition.x, transform.position.y, transform.position.z);
////    }
////}

//using UnityEngine;

//public class CameraFollow : MonoBehaviour
//{
//    public Transform target;          // drag your Cow here
//    public float smoothSpeed = 5f;
//    public Vector3 offset;

//    [Header("Follow Boundaries")]
//    public float followStartX = 643f;   // camera starts following when cow reaches this X
//    public float maxCameraX = 6815f;  // camera never goes past this X

//    void LateUpdate()
//    {
//        if (target == null) return;

//        // Only follow after cow crosses X = 643
//        if (target.position.x < followStartX) return;

//        float targetX = target.position.x + offset.x;

//        // Clamp so camera never crosses X = 6815
//        targetX = Mathf.Min(targetX, maxCameraX);

//        Vector3 desiredPosition = new Vector3(targetX, transform.position.y, transform.position.z);
//        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

//        transform.position = smoothedPosition;
//    }
//}

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public RectTransform cowRect;       // ✅ drag the Cow RectTransform here

    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public float followStartX = 643f;   // anchoredPosition.x threshold
    public float maxCameraX = 6815f;  // camera hard stop (world X)

    [Header("Offset")]
    public float cameraOffsetX = 0f;     // optional horizontal offset

    private float initialCamX;

    void Start()
    {
        initialCamX = transform.position.x;
    }

    void LateUpdate()
    {
        if (cowRect == null) return;

        // ✅ Read anchoredPosition — this is the real UI coordinate
        float cowAnchoredX = cowRect.anchoredPosition.x;

        // Don't follow until cow passes the threshold
        if (cowAnchoredX < followStartX) return;

        // Map cow anchoredPosition.x → camera world X
        float targetX = cowAnchoredX + cameraOffsetX;

        // ✅ Clamp so camera never goes past maxCameraX
        targetX = Mathf.Min(targetX, maxCameraX);

        float smoothedX = Mathf.Lerp(transform.position.x, targetX, smoothSpeed * Time.deltaTime);

        transform.position = new Vector3(smoothedX, transform.position.y, transform.position.z);
    }
}