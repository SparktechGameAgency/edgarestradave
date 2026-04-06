using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    // =====================================================================
    // SHARED — Cow, Bubble, Hand, Luna
    // =====================================================================

    [Header("Luna Animator")]
    public AnimatorForLuna lunaAnimator;

    [Header("Female Cow")]
    public RectTransform femaleCow;
    public Vector2 cowStartPos;
    public Vector2 cowEndPos;
    public float cowSlideDuration = 1.2f;

    [Header("Thought Bubble")]
    public RectTransform thoughtBubble;
    public float bubblePopSpeed = 5f;
    public TextMeshProUGUI thoughtText;
    public float typingSpeed = 0.05f;

    [Header("Hand Pointer")]
    public RectTransform handPointer;
    public RectTransform handTip;              // Empty child GameObject placed at the finger tip
    public float handMoveSpeed = 3f;
    public float handPauseDuration = 0.4f;

    // =====================================================================
    // PHASE 1 — Collect Crops
    // =====================================================================

    [Header("Phase 1 - Scene Object")]
    public GameObject collectCrops;

    [Header("Phase 1 - Messages")]
    public string msg1_Hello = "Hello!";
    public string msg1_Intro = "Today we are going to collect some corn from the field!";
    public string msg1_ClickCorn = "Try clicking on the corn!";
    public string msg1_CollectAll = "Good job! Now try collecting all the corns!";
    public string msg1_WellDone = "Well done collecting all the corns!";
    public string msg1_NextStep = "We can now go to the next step!";

    [Header("Phase 1 - Hand Targets")]
    public RectTransform cartTarget;
    public RectTransform cornTarget;

    [Header("Phase 1 - Corn Objects")]
    public GameObject[] cornObjects;

    // Phase 1 private state
    private bool p1_handActive = false;
    private bool p1_firstCornClicked = false;
    private bool p1_allCornsPlaced = false;
    private int p1_placedCornCount = 0;

    // =====================================================================
    // PHASE 2 — Chicken Hut
    // =====================================================================

    [Header("Phase 2 - Scene Object")]
    public GameObject chickenHut;

    [Header("Phase 2 - Chicken Thought Bubble")]
    public RectTransform chickenThoughtBubble;   // Thought bubble above ChickenHut
    private Vector3 chickenBubbleOriginalScale;   // Saved on Start so pop targets the correct size

    [Header("Phase 2 - Messages")]
    public string msg2_FeedChicken = "Oky Now try Give some corns to the Chicken!";

    [Header("Phase 2 - Hand Targets")]
    public RectTransform cornCartButton;   // The cart button
    public RectTransform chickenCenter;    // Child "Chicken" inside ChickenHut

    [Header("Phase 2 - Chicken Animation")]
    public GameObject chickenIdle;         // Idle anim GameObject
    public GameObject chickenHappy;        // Happy anim GameObject
    public float happyAnimDuration = 2f;

    [Header("Phase 2 - Egg")]
    public RectTransform eggButton;        // Hidden egg button in canvas
    public float popSpeed = 8f;    // Speed of pop up/down scale anim

    [Header("Phase 2 - Messages")]
    public string msg2_GoodJob = "Good job!";
    public string msg2_Eggs = "Looks like we got some Eggs from her!";
    public string msg2_NextStep = "Lets go to the next step!";

    [Header("Phase 2 - Cart Settings")]
    public float cartMoveSpeed = 3f;

    // Phase 2 private state
    private bool p2_handActive = false;
    private bool p2_cartClicked = false;
    private Vector2 p2_cartOrigin;         // Saved cart position before it moves

    // =====================================================================
    // PHASE 3 — Cow Shop
    // =====================================================================

    [Header("Phase 3 - Scene Object")]
    public GameObject cowShop;

    [Header("Phase 3 - Messages")]
    public string msg3_NeedsCorns = "Looks like He also need some corns too!";
    public string msg3_GiveCorns = "Give him some corn!";
    public string msg3_GoodJob = "Good job!";
    public string msg3_GotMilks = "Looks like we got some milks from the cow!";
    public string msg3_NextStep = "Lets go to our last step!";

    [Header("Phase 3 - Hand Targets")]
    public RectTransform cowShopCartButton;  // The right cart button the player clicks
    public RectTransform cowCenter;          // Center of the cow inside CowShop

    [Header("Phase 3 - Milk Cart")]
    public RectTransform milkCart;           // Hidden MilkCart button that appears
    public float milkCartMoveSpeed = 3f;

    [Header("Phase 3 - Cow Animation")]
    public GameObject cowIdle;               // Idle anim GameObject inside CowShop
    public GameObject cowHappy;              // Happy anim GameObject inside CowShop
    public float cowHappyAnimDuration = 2f;

    [Header("Phase 3 - Cow Thought Bubble")]
    public RectTransform cowThoughtBubble;
    private Vector3 cowBubbleOriginalScale;

    // Phase 3 private state
    private bool p3_handActive = false;
    private bool p3_cartClicked = false;
    private Vector2 p3_cartOrigin;
    private Vector3 milkCartOriginalScale;

    // =====================================================================
    // PHASE 4 — Grind the Corn
    // =====================================================================

    [Header("Phase 4 - Scene Object")]
    public GameObject grindTheCorns;            // GrindtheCorns root panel

    [Header("Phase 4 - Messages")]
    public string msg4_PutCorn = "Put corn on the grinder!";
    public string msg4_GotFlour = "Great! We got some Flour!";
    public string msg4_MakeCake = "We can make a cake!";
    public string msg4_LetsGo = "Let's go make it!";

    [Header("Phase 4 - Hand Targets")]
    public RectTransform grindCartButton;   // The left corn cart button the player taps
    public RectTransform grinderTip;        // PlaceHolder — tip of the grinder (corn lands here)

    [Header("Phase 4 - Grinder Objects")]
    public GameObject grinder1;             // Grinder (1) — static image, visible first
    public GameObject grinder2;             // Grinder (2) — animated, hidden first
    public GrinderAnim grinderAnim;         // GrinderAnim component on Grinder (2)  [loop = false]

    [Header("Phase 4 - Flour Bag")]
    public RectTransform flourBagRect;      // FlourBag (1) RectTransform
    public FlourBagAnim flourBagAnim;       // FlourBagAnim component on FlourBag (1) [loop = false]
    public float flourBagMoveSpeed = 3f;

    [Header("Phase 4 - Grinder Thought Bubble")]
    public RectTransform grindThoughtBubble;    // Thought bubble above the grinder (same pattern as chicken/cow)
    private Vector3 grindBubbleOriginalScale;

    [Header("Phase 4 - Cart Settings")]
    public float grindCartMoveSpeed = 3f;

    // Phase 4 private state
    private bool p4_handActive = false;
    private bool p4_cartClicked = false;
    private Vector3 p4_cartOriginWorld;        // World-space position saved BEFORE cart moves

    // =====================================================================
    // UNITY LIFECYCLE
    // =====================================================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        handPointer.gameObject.SetActive(false);
        thoughtText.text = "";
        femaleCow.anchoredPosition = cowStartPos;
        thoughtBubble.localScale = Vector3.zero;

        // Phase 1 active, all others hidden
        if (collectCrops != null) collectCrops.SetActive(true);
        if (chickenHut != null) chickenHut.SetActive(false);
        if (cowShop != null) cowShop.SetActive(false);
        if (grindTheCorns != null) grindTheCorns.SetActive(false);

        if (chickenThoughtBubble != null)
        {
            chickenBubbleOriginalScale = chickenThoughtBubble.localScale;
            chickenThoughtBubble.gameObject.SetActive(false);
        }
        if (cowThoughtBubble != null)
        {
            cowBubbleOriginalScale = cowThoughtBubble.localScale;
            cowThoughtBubble.gameObject.SetActive(false);
        }

        if (milkCart != null)
        {
            milkCartOriginalScale = milkCart.localScale;
            milkCart.gameObject.SetActive(false);
        }

        // Phase 4: hide Grinder(2) and FlourBag until needed
        if (grinder2 != null) grinder2.SetActive(false);
        if (flourBagRect != null) flourBagRect.gameObject.SetActive(false);

        if (grindThoughtBubble != null)
        {
            grindBubbleOriginalScale = grindThoughtBubble.localScale;
            grindThoughtBubble.gameObject.SetActive(false);
        }

        Phase1_SetCornsInteractable(false);
        StartCoroutine(Phase1_Tutorial());
    }

    // =====================================================================
    // PHASE 1 COROUTINE
    // =====================================================================

    IEnumerator Phase1_Tutorial()
    {
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg1_Hello));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg1_Intro));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg1_ClickCorn));

        Phase1_SetupCornListener();

        p1_handActive = true;
        handPointer.gameObject.SetActive(true);
        handPointer.position = GetHandPositionForTarget(cartTarget);
        StartCoroutine(Phase1_LoopHand());

        yield return new WaitUntil(() => p1_firstCornClicked);

        p1_handActive = false;
        handPointer.gameObject.SetActive(false);
        Phase1_SetCornsInteractable(true);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg1_CollectAll));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        yield return new WaitUntil(() => p1_allCornsPlaced);

        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg1_WellDone));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg1_NextStep));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        // Hand off to Phase 2
        yield return new WaitForSeconds(1f);
        if (collectCrops != null) collectCrops.SetActive(false);
        if (chickenHut != null) chickenHut.SetActive(true);
        StartCoroutine(PopChickenBubble());
        StartCoroutine(Phase2_Tutorial());
    }

    // =====================================================================
    // PHASE 1 HELPERS
    // =====================================================================

    void Phase1_SetCornsInteractable(bool state)
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

    void Phase1_SetupCornListener()
    {
        if (cornTarget == null) return;

        Image img = cornTarget.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        Button btn = cornTarget.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.onClick.AddListener(Phase1_OnCornClicked);
            return;
        }

        EventTrigger trigger = cornTarget.GetComponent<EventTrigger>()
                            ?? cornTarget.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => Phase1_OnCornClicked());
        trigger.triggers.Add(entry);
    }

    void Phase1_OnCornClicked()
    {
        if (p1_firstCornClicked) return;
        p1_firstCornClicked = true;
    }

    public void Phase1_OnCornPlaced()
    {
        p1_placedCornCount++;
        if (p1_placedCornCount >= cornObjects.Length)
            p1_allCornsPlaced = true;
    }

    IEnumerator Phase1_LoopHand()
    {
        while (p1_handActive)
        {
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(cartTarget)));
            yield return new WaitForSeconds(handPauseDuration);
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(cornTarget)));
            yield return new WaitForSeconds(handPauseDuration);
        }
    }

    // =====================================================================
    // PHASE 2 COROUTINE
    // =====================================================================

    IEnumerator Phase2_Tutorial()
    {
        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg2_FeedChicken));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        Phase2_SetupCartListener();

        p2_handActive = true;
        handPointer.gameObject.SetActive(true);
        handPointer.position = GetHandPositionForTarget(cornCartButton);
        StartCoroutine(Phase2_LoopHand());

        yield return new WaitUntil(() => p2_cartClicked);

        p2_handActive = false;
        handPointer.gameObject.SetActive(false);

        p2_cartOrigin = cornCartButton.anchoredPosition;
        yield return StartCoroutine(Phase2_MoveCartToEgg());

        // Cow slides in after egg is settled
        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg2_GoodJob));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg2_Eggs));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg2_NextStep));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        // Hand off to Phase 3
        yield return new WaitForSeconds(1f);
        if (chickenHut != null) chickenHut.SetActive(false);
        if (cowShop != null) cowShop.SetActive(true);
        StartCoroutine(PopCowBubble());
        StartCoroutine(Phase3_Tutorial());
    }

    // =====================================================================
    // PHASE 2 HELPERS
    // =====================================================================

    void Phase2_SetupCartListener()
    {
        if (cornCartButton == null) return;

        Image img = cornCartButton.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        Button btn = cornCartButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.onClick.AddListener(Phase2_OnCartClicked);
            return;
        }

        EventTrigger trigger = cornCartButton.GetComponent<EventTrigger>()
                            ?? cornCartButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => Phase2_OnCartClicked());
        trigger.triggers.Add(entry);
    }

    void Phase2_OnCartClicked()
    {
        if (p2_cartClicked) return;
        p2_cartClicked = true;
    }

    IEnumerator Phase2_LoopHand()
    {
        while (p2_handActive)
        {
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(cornCartButton)));
            yield return new WaitForSeconds(handPauseDuration);
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(chickenCenter)));
            yield return new WaitForSeconds(handPauseDuration);
        }
    }

    IEnumerator Phase2_MoveCartToEgg()
    {
        if (cornCartButton == null || eggButton == null) yield break;

        // --- 1. Cart lerps to chicken ---
        Vector2 startPos = cornCartButton.anchoredPosition;
        Vector2 endPos = eggButton.anchoredPosition;
        float elapsed = 0f;
        float duration = Vector2.Distance(startPos, endPos) / (cartMoveSpeed * 100f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cornCartButton.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        cornCartButton.anchoredPosition = endPos;

        // --- 2. Thought bubble pops out ---
        yield return StartCoroutine(ShrinkChickenBubble());

        // --- 3. Chicken plays happy animation ---
        if (chickenIdle != null) chickenIdle.SetActive(false);
        if (chickenHappy != null) chickenHappy.SetActive(true);
        yield return new WaitForSeconds(happyAnimDuration);
        if (chickenHappy != null) chickenHappy.SetActive(false);
        if (chickenIdle != null) chickenIdle.SetActive(true);

        // --- 4. Cart pops down (scale to zero) ---
        while (cornCartButton.localScale.x > 0.01f)
        {
            cornCartButton.localScale = Vector3.Lerp(
                cornCartButton.localScale, Vector3.zero,
                Time.deltaTime * popSpeed);
            yield return null;
        }
        cornCartButton.localScale = Vector3.zero;
        cornCartButton.gameObject.SetActive(false);

        // --- 5. Egg pops up (scale from zero to one) ---
        eggButton.gameObject.SetActive(true);
        eggButton.localScale = Vector3.zero;
        while (Vector3.Distance(eggButton.localScale, Vector3.one) > 0.01f)
        {
            eggButton.localScale = Vector3.Lerp(
                eggButton.localScale, Vector3.one,
                Time.deltaTime * popSpeed);
            yield return null;
        }
        eggButton.localScale = Vector3.one;

        // --- 6. Egg lerps to cart's original position ---
        Vector2 eggStart = eggButton.anchoredPosition;
        elapsed = 0f;
        duration = Vector2.Distance(eggStart, p2_cartOrigin) / (cartMoveSpeed * 100f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            eggButton.anchoredPosition = Vector2.Lerp(eggStart, p2_cartOrigin, t);
            yield return null;
        }

        eggButton.anchoredPosition = p2_cartOrigin;
        Debug.Log("Egg reached cart position!");
    }

    // =====================================================================
    // PHASE 3 COROUTINE
    // =====================================================================

    IEnumerator Phase3_Tutorial()
    {
        // Cow slides in — first message
        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg3_NeedsCorns));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg3_GiveCorns));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        // Setup hand + cart listener
        Phase3_SetupCartListener();

        p3_handActive = true;
        handPointer.gameObject.SetActive(true);
        handPointer.position = GetHandPositionForTarget(cowShopCartButton);
        StartCoroutine(Phase3_LoopHand());

        yield return new WaitUntil(() => p3_cartClicked);

        p3_handActive = false;
        handPointer.gameObject.SetActive(false);

        p3_cartOrigin = cowShopCartButton.anchoredPosition;
        yield return StartCoroutine(Phase3_MoveCartToMilk());

        // Cow slides in — reward messages
        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg3_GoodJob));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg3_GotMilks));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg3_NextStep));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        // ── Hand off to Phase 4 ──
        yield return new WaitForSeconds(1f);
        if (cowShop != null) cowShop.SetActive(false);
        if (grindTheCorns != null) grindTheCorns.SetActive(true);
        StartCoroutine(PopGrindBubble());
        StartCoroutine(Phase4_Tutorial());
    }

    // =====================================================================
    // PHASE 3 HELPERS
    // =====================================================================

    void Phase3_SetupCartListener()
    {
        if (cowShopCartButton == null) return;

        Image img = cowShopCartButton.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        Button btn = cowShopCartButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.onClick.AddListener(Phase3_OnCartClicked);
            return;
        }

        EventTrigger trigger = cowShopCartButton.GetComponent<EventTrigger>()
                            ?? cowShopCartButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => Phase3_OnCartClicked());
        trigger.triggers.Add(entry);
    }

    void Phase3_OnCartClicked()
    {
        if (p3_cartClicked) return;
        p3_cartClicked = true;
    }

    IEnumerator Phase3_LoopHand()
    {
        while (p3_handActive)
        {
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(cowShopCartButton)));
            yield return new WaitForSeconds(handPauseDuration);
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(cowCenter)));
            yield return new WaitForSeconds(handPauseDuration);
        }
    }

    IEnumerator Phase3_MoveCartToMilk()
    {
        if (cowShopCartButton == null || milkCart == null) yield break;

        Vector3 cartStartWorld = cowShopCartButton.position;
        Vector3 milkTargetWorld = milkCart.position;
        Vector3 cartOriginWorld = cowShopCartButton.position;

        // --- 1. Right cart lerps to MilkCart's world position ---
        float elapsed = 0f;
        float duration = Vector3.Distance(cartStartWorld, milkTargetWorld) / (milkCartMoveSpeed * 100f);

        if (duration > 0f)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                cowShopCartButton.position = Vector3.Lerp(cartStartWorld, milkTargetWorld, t);
                yield return null;
            }
        }
        cowShopCartButton.position = milkTargetWorld;

        // --- 2. Thought bubble shrinks when cart arrives ---
        yield return StartCoroutine(ShrinkCowBubble());

        // --- 3. Cow plays happy animation ---
        if (cowIdle != null) cowIdle.SetActive(false);
        if (cowHappy != null) cowHappy.SetActive(true);
        yield return new WaitForSeconds(cowHappyAnimDuration);
        if (cowHappy != null) cowHappy.SetActive(false);
        if (cowIdle != null) cowIdle.SetActive(true);

        // --- 4. Right cart pops down ---
        while (cowShopCartButton.localScale.x > 0.01f)
        {
            cowShopCartButton.localScale = Vector3.Lerp(
                cowShopCartButton.localScale, Vector3.zero,
                Time.deltaTime * popSpeed);
            yield return null;
        }
        cowShopCartButton.localScale = Vector3.zero;
        cowShopCartButton.gameObject.SetActive(false);

        // --- 5. MilkCart pops up at the cart's arrival position ---
        milkCart.position = milkTargetWorld;
        milkCart.localScale = Vector3.zero;
        milkCart.gameObject.SetActive(true);

        while (Vector3.Distance(milkCart.localScale, milkCartOriginalScale) > 0.01f)
        {
            milkCart.localScale = Vector3.Lerp(
                milkCart.localScale, milkCartOriginalScale,
                Time.deltaTime * popSpeed);
            yield return null;
        }
        milkCart.localScale = milkCartOriginalScale;

        // --- 6. MilkCart lerps back to the cart's original world position ---
        Vector3 milkStartWorld = milkCart.position;
        elapsed = 0f;
        duration = Vector3.Distance(milkStartWorld, cartOriginWorld) / (milkCartMoveSpeed * 100f);

        if (duration > 0f)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                milkCart.position = Vector3.Lerp(milkStartWorld, cartOriginWorld, t);
                yield return null;
            }
        }
        milkCart.position = cartOriginWorld;
        Debug.Log("MilkCart reached cart position!");
    }

    // =====================================================================
    // PHASE 4 COROUTINE — Grind the Corn
    // =====================================================================

    IEnumerator Phase4_Tutorial()
    {
        // --- Cow slides in: instruction message ---
        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg4_PutCorn));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        // --- Hand points at the left corn cart, wait for player tap ---
        Phase4_SetupCartListener();

        p4_handActive = true;
        handPointer.gameObject.SetActive(true);
        handPointer.position = GetHandPositionForTarget(grindCartButton);
        StartCoroutine(Phase4_LoopHand());

        yield return new WaitUntil(() => p4_cartClicked);

        p4_handActive = false;
        handPointer.gameObject.SetActive(false);

        // Save world-space origin NOW — before the cart moves — so flour bag returns here
        p4_cartOriginWorld = grindCartButton.position;

        // --- Cart moves to grinder tip, then grinder sequence plays ---
        yield return StartCoroutine(Phase4_GrindSequence());

        // --- Cow slides in: reward messages ---
        femaleCow.gameObject.SetActive(true);
        femaleCow.anchoredPosition = cowStartPos;
        yield return StartCoroutine(SlideCowIn());
        yield return StartCoroutine(PopBubble());

        yield return StartCoroutine(TypeText(msg4_GotFlour));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg4_MakeCake));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(TypeText(msg4_LetsGo));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShrinkBubble());
        yield return StartCoroutine(SlideCowOut());

        Debug.Log("Phase 4 complete! Tutorial finished.");
        // StartCoroutine(Phase5_Tutorial()); ← add next phase here when ready
    }

    IEnumerator Phase4_GrindSequence()
    {
        // --- 1. Cart lerps to grinder tip (PlaceHolder) ---
        Vector3 cartStartWorld = grindCartButton.position;
        Vector3 grinderTipWorld = grinderTip.position;

        float elapsed = 0f;
        float duration = Vector3.Distance(cartStartWorld, grinderTipWorld) / (grindCartMoveSpeed * 100f);

        if (duration > 0f)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                grindCartButton.position = Vector3.Lerp(cartStartWorld, grinderTipWorld, t);
                yield return null;
            }
        }
        grindCartButton.position = grinderTipWorld;

        // --- 2. Grinder thought bubble shrinks when corn arrives ---
        yield return StartCoroutine(ShrinkGrindBubble());

        // --- 3. Cart pops away ---
        while (grindCartButton.localScale.x > 0.01f)
        {
            grindCartButton.localScale = Vector3.Lerp(
                grindCartButton.localScale, Vector3.zero,
                Time.deltaTime * popSpeed);
            yield return null;
        }
        grindCartButton.localScale = Vector3.zero;
        grindCartButton.gameObject.SetActive(false);

        // --- 4. Swap Grinder(1) → Grinder(2), brief pause ---
        if (grinder1 != null) grinder1.SetActive(false);
        if (grinder2 != null) grinder2.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);

        // --- 5. Play both animations ONCE (loop = false in Inspector) ---
        //        Show flour bag at its designed position, then start both anims
        if (flourBagRect != null) flourBagRect.gameObject.SetActive(true);
        if (grinderAnim != null) { grinderAnim.Reset(); grinderAnim.Play(); }
        if (flourBagAnim != null) { flourBagAnim.Reset(); flourBagAnim.Play(); }

        // Wait for the FULL duration of whichever anim is longer (+small buffer)
        float grinderDuration = (grinderAnim != null) ? (float)grinderAnim.frames.Length / grinderAnim.fps : 0f;
        float flourDuration = (flourBagAnim != null) ? (float)flourBagAnim.frames.Length / flourBagAnim.fps : 0f;
        yield return new WaitForSecondsRealtime(Mathf.Max(grinderDuration, flourDuration) + 0.1f);

        // --- 6. Flour bag lerps to the cart's original position (saved before cart moved) ---
        Vector3 flourStartWorld = flourBagRect.position;

        elapsed = 0f;
        duration = Vector3.Distance(flourStartWorld, p4_cartOriginWorld) / (flourBagMoveSpeed * 100f);

        if (duration > 0f)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                flourBagRect.position = Vector3.Lerp(flourStartWorld, p4_cartOriginWorld, t);
                yield return null;
            }
        }
        flourBagRect.position = p4_cartOriginWorld;
        Debug.Log("FlourBag reached cart position!");
    }

    // =====================================================================
    // PHASE 4 GRINDER BUBBLE HELPERS
    // =====================================================================

    IEnumerator PopGrindBubble()
    {
        if (grindThoughtBubble == null) yield break;

        var pulse = grindThoughtBubble.GetComponent("PulseAnimation") as MonoBehaviour;
        if (pulse != null) pulse.enabled = false;

        grindThoughtBubble.localScale = Vector3.zero;
        grindThoughtBubble.gameObject.SetActive(true);

        while (Vector3.Distance(grindThoughtBubble.localScale, grindBubbleOriginalScale) > 0.01f)
        {
            grindThoughtBubble.localScale = Vector3.Lerp(
                grindThoughtBubble.localScale, grindBubbleOriginalScale,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        grindThoughtBubble.localScale = grindBubbleOriginalScale;

        if (pulse != null) pulse.enabled = true;
    }

    IEnumerator ShrinkGrindBubble()
    {
        if (grindThoughtBubble == null) yield break;

        var pulse = grindThoughtBubble.GetComponent("PulseAnimation") as MonoBehaviour;
        if (pulse != null) pulse.enabled = false;

        while (Vector3.Distance(grindThoughtBubble.localScale, Vector3.zero) > 0.01f)
        {
            grindThoughtBubble.localScale = Vector3.Lerp(
                grindThoughtBubble.localScale, Vector3.zero,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        grindThoughtBubble.localScale = Vector3.zero;
        grindThoughtBubble.gameObject.SetActive(false);
    }

    // =====================================================================
    // PHASE 4 HELPERS
    // =====================================================================

    void Phase4_SetupCartListener()
    {
        if (grindCartButton == null) return;

        Image img = grindCartButton.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        Button btn = grindCartButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.onClick.AddListener(Phase4_OnCartClicked);
            return;
        }

        EventTrigger trigger = grindCartButton.GetComponent<EventTrigger>()
                            ?? grindCartButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => Phase4_OnCartClicked());
        trigger.triggers.Add(entry);
    }

    void Phase4_OnCartClicked()
    {
        if (p4_cartClicked) return;
        p4_cartClicked = true;
    }

    IEnumerator Phase4_LoopHand()
    {
        while (p4_handActive)
        {
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(grindCartButton)));
            yield return new WaitForSeconds(handPauseDuration);
            yield return StartCoroutine(MoveHand(GetHandPositionForTarget(grinderTip)));
            yield return new WaitForSeconds(handPauseDuration);
        }
    }

    // =====================================================================
    // SHARED COROUTINES — Cow, Bubble, Text, Hand
    // =====================================================================

    // =====================================================================
    // PHASE 2 CHICKEN BUBBLE HELPERS
    // =====================================================================

    IEnumerator PopChickenBubble()
    {
        if (chickenThoughtBubble == null) yield break;

        var pulse = chickenThoughtBubble.GetComponent("PulseAnimation") as MonoBehaviour;
        if (pulse != null) pulse.enabled = false;

        chickenThoughtBubble.localScale = Vector3.zero;
        chickenThoughtBubble.gameObject.SetActive(true);

        while (Vector3.Distance(chickenThoughtBubble.localScale, chickenBubbleOriginalScale) > 0.01f)
        {
            chickenThoughtBubble.localScale = Vector3.Lerp(
                chickenThoughtBubble.localScale, chickenBubbleOriginalScale,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        chickenThoughtBubble.localScale = chickenBubbleOriginalScale;

        if (pulse != null) pulse.enabled = true;
    }

    IEnumerator ShrinkChickenBubble()
    {
        if (chickenThoughtBubble == null) yield break;

        var pulse = chickenThoughtBubble.GetComponent("PulseAnimation") as MonoBehaviour;
        if (pulse != null) pulse.enabled = false;

        while (Vector3.Distance(chickenThoughtBubble.localScale, Vector3.zero) > 0.01f)
        {
            chickenThoughtBubble.localScale = Vector3.Lerp(
                chickenThoughtBubble.localScale, Vector3.zero,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        chickenThoughtBubble.localScale = Vector3.zero;
        chickenThoughtBubble.gameObject.SetActive(false);
    }

    IEnumerator PopCowBubble()
    {
        if (cowThoughtBubble == null) yield break;

        var pulse = cowThoughtBubble.GetComponent("PulseAnimation") as MonoBehaviour;
        if (pulse != null) pulse.enabled = false;

        cowThoughtBubble.localScale = Vector3.zero;
        cowThoughtBubble.gameObject.SetActive(true);

        while (Vector3.Distance(cowThoughtBubble.localScale, cowBubbleOriginalScale) > 0.01f)
        {
            cowThoughtBubble.localScale = Vector3.Lerp(
                cowThoughtBubble.localScale, cowBubbleOriginalScale,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        cowThoughtBubble.localScale = cowBubbleOriginalScale;

        if (pulse != null) pulse.enabled = true;
    }

    IEnumerator ShrinkCowBubble()
    {
        if (cowThoughtBubble == null) yield break;

        var pulse = cowThoughtBubble.GetComponent("PulseAnimation") as MonoBehaviour;
        if (pulse != null) pulse.enabled = false;

        while (Vector3.Distance(cowThoughtBubble.localScale, Vector3.zero) > 0.01f)
        {
            cowThoughtBubble.localScale = Vector3.Lerp(
                cowThoughtBubble.localScale, Vector3.zero,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        cowThoughtBubble.localScale = Vector3.zero;
        cowThoughtBubble.gameObject.SetActive(false);
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
                thoughtBubble.localScale, Vector3.one,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        thoughtBubble.localScale = Vector3.one;
    }

    IEnumerator ShrinkBubble()
    {
        while (Vector3.Distance(thoughtBubble.localScale, Vector3.zero) > 0.01f)
        {
            thoughtBubble.localScale = Vector3.Lerp(
                thoughtBubble.localScale, Vector3.zero,
                Time.deltaTime * bubblePopSpeed);
            yield return null;
        }
        thoughtBubble.localScale = Vector3.zero;
        thoughtText.text = "";
    }

    IEnumerator TypeText(string message)
    {
        if (lunaAnimator != null) lunaAnimator.Play();
        thoughtText.text = "";
        foreach (char c in message)
        {
            thoughtText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        if (lunaAnimator != null) lunaAnimator.Stop();
    }

    IEnumerator DeleteText()
    {
        while (thoughtText.text.Length > 0)
        {
            thoughtText.text = thoughtText.text.Substring(0, thoughtText.text.Length - 1);
            yield return new WaitForSeconds(typingSpeed * 0.5f);
        }
    }

    IEnumerator MoveHand(Vector3 target)
    {
        while (Vector3.Distance(handPointer.position, target) > 2f)
        {
            handPointer.position = Vector3.Lerp(
                handPointer.position, target,
                Time.deltaTime * handMoveSpeed);
            yield return null;
        }
        handPointer.position = target;
    }

    // Returns the exact world-space center of any RectTransform
    Vector3 GetRectCenter(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;
    }

    // Returns where to place the hand so its tip lands on the target center
    Vector3 GetHandPositionForTarget(RectTransform rt)
    {
        Vector3 targetCenter = GetRectCenter(rt);
        if (handTip == null) return targetCenter;
        Vector3 tipOffset = handTip.position - handPointer.position;
        return targetCenter - tipOffset;
    }
}