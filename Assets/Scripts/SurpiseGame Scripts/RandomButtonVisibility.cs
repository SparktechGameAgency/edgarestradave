using UnityEngine;

public class RandomButtonVisibility : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject button1;
    public GameObject button2;
    public GameObject button3;

    // The food button that was selected this round — read by RoundManager for bubble
    public GameObject SelectedButton { get; private set; }

    void Start()
    {
        ShowRandomButton();
    }

    public void ShowRandomButton()
    {
        if (button1 != null) button1.SetActive(false);
        if (button2 != null) button2.SetActive(false);
        if (button3 != null) button3.SetActive(false);

        int randomIndex = Random.Range(0, 3);

        switch (randomIndex)
        {
            case 0: 
                if (button1 != null) { button1.SetActive(true); SelectedButton = button1; }
                break;
            case 1: 
                if (button2 != null) { button2.SetActive(true); SelectedButton = button2; }
                break;
            case 2: 
                if (button3 != null) { button3.SetActive(true); SelectedButton = button3; }
                break;
        }
    }
}
