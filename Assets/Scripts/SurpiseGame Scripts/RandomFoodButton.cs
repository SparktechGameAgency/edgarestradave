using UnityEngine;
using System.Collections;

public class RandomFoodButton : MonoBehaviour
{
    [Header("Animations GameObjects")]
    public GameObject idleAnim;
    public GameObject eatAnim;
    public GameObject extraAnim;

    [Header("Food Identity")]
    [Tooltip("Unique food type name e.g. 'sausage', 'burger', 'chili'. Must match exactly across all buttons of same food.")]
    public string foodType;

    [Header("Spicy Settings")]
    [Tooltip("Check this in the Inspector if this button is the spicy/chilli one")]
    public bool isSpicy = false;

    [Header("Animation Durations")]
    public float eatDuration = 2f;
    public float extraDuration = 2f;

    [Header("Food Fly Settings")]
    public RectTransform foodObject;
    public RectTransform mouthTarget;
    public float flyDuration = 0.5f;
    public AnimationCurve flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float arcHeight = 150f;

    public static bool isAnyPlaying = false;

    // True when a lid is open and food must be clicked before opening another lid
    public static bool isFoodPending = false;

    private static CoroutineRunner runner;

    void Awake()
    {
        if (runner == null)
        {
            GameObject runnerObj = new GameObject("CoroutineRunner");
            runner = runnerObj.AddComponent<CoroutineRunner>();
            DontDestroyOnLoad(runnerObj);
        }
    }

    void Start()
    {
        ResetButton();
    }

    public void ResetButton()
    {
        if (idleAnim != null) idleAnim.SetActive(true);
        if (eatAnim != null) eatAnim.SetActive(false);
        if (extraAnim != null) extraAnim.SetActive(false);
    }

    public void OnFoodButtonPressed()
    {
        if (!isAnyPlaying && !LidButtonAnimation.isAnyLidPlaying)
        {
            runner.StartCoroutine(PlayEatAnimation());
        }
    }

    IEnumerator PlayEatAnimation()
    {
        isAnyPlaying = true;
        isFoodPending = false;

        bool matchesBubble = MatchesBubble();

        // If wrong food — Game Over instantly, no animation, no fly
        if (!isSpicy && !matchesBubble)
        {
            isAnyPlaying = false;
            gameObject.SetActive(false);

            if (RoundManager.Instance != null)
                RoundManager.Instance.TriggerGameOver();

            yield break;
        }

        // Only fly to mouth if correct or spicy
        if (foodObject != null && mouthTarget != null)
            yield return runner.StartCoroutine(FlyFoodToMouth());

        idleAnim.SetActive(false);

        if (isSpicy)
        {
            // Spicy: eat then reaction → Game Over
            eatAnim.SetActive(true);
            yield return new WaitForSeconds(eatDuration);
            eatAnim.SetActive(false);

            extraAnim.SetActive(true);
            yield return new WaitForSeconds(extraDuration);
            extraAnim.SetActive(false);
            idleAnim.SetActive(true);

            isAnyPlaying = false;
            gameObject.SetActive(false);

            if (RoundManager.Instance != null)
                RoundManager.Instance.TriggerGameOver();
        }
        else
        {
            // Correct food — eat animation then YouWin immediately
            eatAnim.SetActive(true);
            yield return new WaitForSeconds(eatDuration);
            eatAnim.SetActive(false);

            extraAnim.SetActive(true);
            yield return new WaitForSeconds(extraDuration);
            extraAnim.SetActive(false);

            idleAnim.SetActive(true);
            isAnyPlaying = false;
            gameObject.SetActive(false);

            if (RoundManager.Instance != null)
                RoundManager.Instance.RoundComplete();
        }
    }

    bool MatchesBubble()
    {
        if (RoundManager.Instance == null) return true;
        if (string.IsNullOrEmpty(foodType)) return true;
        return foodType == RoundManager.Instance.CurrentBubbleFoodType;
    }

    IEnumerator FlyFoodToMouth()
    {
        Vector3 startPos = foodObject.position;
        Vector3 endPos = mouthTarget.position;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyDuration);
            float curveT = flyCurve.Evaluate(t);

            Vector3 linearPos = Vector3.Lerp(startPos, endPos, curveT);
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
            linearPos.y += arc;

            foodObject.position = linearPos;
            yield return null;
        }

        foodObject.position = endPos;
        foodObject.gameObject.SetActive(false);
    }
}
