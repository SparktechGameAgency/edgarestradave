using UnityEngine;
using System.Collections;

public class LidButtonAnimation : MonoBehaviour
{
    public Transform lid;
    public float liftAngle = 42f;
    public float liftDuration = 0.2f;
    public float moveUpAmount = 20f;
    public float moveRightAmount = 10f;

    private bool isOpen = false;
    private bool isLocked = false;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    public static bool isAnyLidPlaying = false;

    void Awake()
    {
        if (lid != null)
        {
            originalLocalPosition = lid.localPosition;
            originalLocalRotation = lid.localRotation;
        }
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public void ResetLid()
    {
        if (lid != null)
        {
            lid.localPosition = originalLocalPosition;
            lid.localRotation = originalLocalRotation;
        }
        isOpen = false;
        isLocked = false;
    }

    public void OnButtonPressed()
    {
        // Block if animation is playing, food is animating,
        // or a lid is already open waiting for food click
        if (isLocked || isOpen || isAnyLidPlaying || RandomFoodButton.isAnyPlaying || RandomFoodButton.isFoodPending)
            return;

        StartCoroutine(OpenLid());
    }

    IEnumerator OpenLid()
    {
        isLocked = true;
        isAnyLidPlaying = true;

        Quaternion startRot = lid.localRotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, liftAngle);

        Vector3 startPos = lid.localPosition;
        Vector3 endPos = startPos + new Vector3(moveRightAmount, moveUpAmount, 0f);

        float elapsed = 0f;
        while (elapsed < liftDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / liftDuration;
            lid.localRotation = Quaternion.Slerp(startRot, endRot, t);
            lid.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        lid.localRotation = endRot;
        lid.localPosition = endPos;

        isOpen = true;
        isAnyLidPlaying = false;

        // Block other lids until food is clicked
        RandomFoodButton.isFoodPending = true;

        // Bounce the whole bowl plate up then settle back down
        StartCoroutine(BounceBody());

        // Trigger wobble on the bowl
        LidOpenWobble wobble = GetComponentInParent<LidOpenWobble>();
        if (wobble != null) wobble.PlayWobble();
    }

    IEnumerator BounceBody()
    {
        Transform body = transform.parent; // Plate root
        if (body == null) yield break;

        Vector3 originalPos = body.localPosition;
        float bounceHeight = 0.5f;
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Arc up then back down with elastic feel
            float y = Mathf.Sin(t * Mathf.PI) * bounceHeight * (1f - t * 0.5f);
            body.localPosition = originalPos + new Vector3(0f, y, 0f);
            yield return null;
        }

        body.localPosition = originalPos;
    }
}