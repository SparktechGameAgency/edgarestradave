using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Cow")]
    public RectTransform femaleCow;
    public Vector2 cowStartPos;
    public Vector2 cowEndPos;
    public float cowSlideDuration = 1.2f;

    [Header("Thought Bubble")]
    public RectTransform thoughtBubble;
    public float bubblePopSpeed = 5f;
    public TextMeshProUGUI thoughtText;
    public float typingSpeed = 0.05f;

    [Header("Thought Bubble Messages")]
    public string message1 = "Hello!";
    public string message2 = "Today we are going to collect some corn from the field!";
    public string message3 = "Try clicking on the corn!";
    public string message4 = "Good job! Now try collecting all the corns!";
    public string message5 = "Well done collecting all the corns!";
    public string message6 = "We can now go to the next step!";

    [Header("Hand Pointer")]
    public RectTransform handPointer;
    public RectTransform cartTarget;
    public RectTransform cornTarget;
    public float handMoveSpeed = 3f;
    public float handPauseDuration = 0.4f;

    [Header("Corn Objects")]
    public GameObject[] cornObjects;

    private bool tutorialActive = true;
    private bool firstCornClicked = false;
    private bool allCornsCollected = false;
    private int placedCornCount = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 0f;
        handPointer.gameObject.SetActive(false);
        thoughtText.text = "";
        femaleCow.anchoredPosition = cowStartPos;
        thoughtBubble.localScale = Vector3.zero;

        SetCornsInteractable(false);
        StartCoroutine(PlayTutorial());
    }

    void SetCornsInteractable(bool state)
    {
        foreach (GameObject corn in cornObjects)
        {
            if (corn == null) continue;

            Button btn = corn.GetComponent<Button>();
            if (btn != null) btn.interactable = state;

            Image img = corn.GetComponent<Image>();
            if (img != null) img.raycastTarget = state;
        }
    }

    IEnumerator PlayTutorial()
    {
        yield return StartCoroutine(SlideCowIn());

        Time.timeScale = 1f;

        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(message1));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(message2));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(message3));

        SetupHandCornListener();

        handPointer.gameObject.SetActive(true);
        handPointer.position = cartTarget.position;
        StartCoroutine(LoopHand());

        yield return new WaitUntil(() => firstCornClicked);

        tutorialActive = false;
        handPointer.gameObject.SetActive(false);
        SetCornsInteractable(true);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(message4));

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        Debug.Log("Waiting for all corns to be placed...");
        yield return new WaitUntil(() => allCornsCollected);

        Debug.Log("All corns placed! Sliding cow back in.");
        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());

        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(message5));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(message6));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());
    }

    void SetupHandCornListener()
    {
        if (cornTarget == null) return;

        Image img = cornTarget.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        Button btn = cornTarget.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.onClick.AddListener(OnHandCornClicked);
            Debug.Log("Hand corn listener set via Button: " + cornTarget.name);
            return;
        }

        EventTrigger trigger = cornTarget.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = cornTarget.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => OnHandCornClicked());
        trigger.triggers.Add(entry);
        Debug.Log("Hand corn listener set via EventTrigger: " + cornTarget.name);
    }

    void OnHandCornClicked()
    {
        if (firstCornClicked) return;
        Debug.Log("Hand corn clicked!");
        firstCornClicked = true;
    }

    public void OnCornPlaced()
    {
        placedCornCount++;
        Debug.Log($"Corn placed: {placedCornCount} / {cornObjects.Length}");

        if (placedCornCount >= cornObjects.Length)
        {
            Debug.Log("All corns collected!");
            allCornsCollected = true;
        }
    }

    IEnumerator ShrinkBubble()
    {
        while (Vector3.Distance(thoughtBubble.localScale, Vector3.zero) > 0.01f)
        {
            thoughtBubble.localScale = Vector3.Lerp(
                thoughtBubble.localScale,
                Vector3.zero,
                Time.deltaTime * bubblePopSpeed
            );
            yield return null;
        }
        thoughtBubble.localScale = Vector3.zero;
        thoughtText.text = "";
    }

    IEnumerator SlideCowIn()
    {
        float elapsed = 0f;
        femaleCow.anchoredPosition = cowStartPos;

        while (elapsed < cowSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / cowSlideDuration);
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            femaleCow.anchoredPosition = Vector2.LerpUnclamped(cowStartPos, cowEndPos, smoothT);
            yield return null;
        }

        femaleCow.anchoredPosition = cowEndPos;
    }

    IEnumerator SlideCowOut()
    {
        float elapsed = 0f;
        Vector2 startPos = femaleCow.anchoredPosition;

        while (elapsed < cowSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cowSlideDuration);
            float smoothT = Mathf.Pow(t, 3f);
            femaleCow.anchoredPosition = Vector2.LerpUnclamped(startPos, cowStartPos, smoothT);
            yield return null;
        }

        femaleCow.anchoredPosition = cowStartPos;
        femaleCow.gameObject.SetActive(false);
    }

    IEnumerator PopBubble()
    {
        thoughtBubble.localScale = Vector3.zero;
        while (Vector3.Distance(thoughtBubble.localScale, Vector3.one) > 0.01f)
        {
            thoughtBubble.localScale = Vector3.Lerp(
                thoughtBubble.localScale,
                Vector3.one,
                Time.deltaTime * bubblePopSpeed
            );
            yield return null;
        }
        thoughtBubble.localScale = Vector3.one;
    }

    IEnumerator TypeText(string message)
    {
        thoughtText.text = "";
        foreach (char c in message)
        {
            thoughtText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator DeleteText()
    {
        while (thoughtText.text.Length > 0)
        {
            thoughtText.text = thoughtText.text.Substring(0, thoughtText.text.Length - 1);
            yield return new WaitForSeconds(typingSpeed * 0.5f);
        }
    }

    IEnumerator LoopHand()
    {
        while (tutorialActive)
        {
            yield return StartCoroutine(MoveHand(cartTarget.position));
            yield return new WaitForSeconds(handPauseDuration);
            yield return StartCoroutine(MoveHand(cornTarget.position));
            yield return new WaitForSeconds(handPauseDuration);
        }
    }

    IEnumerator MoveHand(Vector3 target)
    {
        while (Vector3.Distance(handPointer.position, target) > 2f)
        {
            handPointer.position = Vector3.Lerp(
                handPointer.position,
                target,
                Time.deltaTime * handMoveSpeed
            );
            yield return null;
        }
        handPointer.position = target;
    }
}