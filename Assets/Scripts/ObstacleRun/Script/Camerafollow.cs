using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    [Header("Target")]
    public RectTransform cowRect;       // drag the Cow RectTransform here

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