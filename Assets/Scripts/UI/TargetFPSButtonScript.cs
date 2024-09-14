using UnityEngine;
using UnityEngine.UI;

public class TargetFPSButtonScript : MonoBehaviour
{
    public Sprite selected;
    public Sprite deselected;

    [SerializeField] private Image image; 

    [SerializeField] private GameObject thisButton;
    [SerializeField] private GameObject otherButton;
    [SerializeField] private int FPS;
    [SerializeField] private bool isSelected;

    private void OnEnable()
    {
        if (PlayerPrefs.GetInt("FPS") == FPS)
        {
            SetFPS();
        }
    }

    public void SetFPS()
    {
        if (isSelected == false)
        {
            PlayerPrefs.SetInt("FPS", FPS);
            isSelected = true;
            image.sprite = selected;
            // for all other buttons with tag FPSButton, deselect them
            GameObject[] buttons = GameObject.FindGameObjectsWithTag("FPSButton");
            foreach (GameObject button in buttons)
            {
                if (button != thisButton)
                {
                    button.GetComponent<TargetFPSButtonScript>().Deselect();
                }
            }
        }
    }

    public void Deselect()
    {
        isSelected = false;
        image.sprite = deselected;
    }

}
