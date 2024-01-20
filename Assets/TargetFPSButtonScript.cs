using UnityEngine;
using UnityEngine.UI;

public class TargetFPSButtonScript : MonoBehaviour
{
    public Sprite selected;
    public Sprite deselected;

    [SerializeField] private GameObject thisButton;
    [SerializeField] private GameObject otherButton;
    [SerializeField] private int FPS;
    [SerializeField] private bool isSelected;

    public void SetFPS()
    {
        if (isSelected == false)
        {
            PlayerPrefs.SetInt("FPS", FPS);
            isSelected = true;
            thisButton.GetComponent<Image>().sprite = selected;
            otherButton.GetComponent<TargetFPSButtonScript>().Deselect();
        }
    }

    public void Deselect()
    {
        isSelected = false;
        thisButton.GetComponent<Image>().sprite = deselected;
    }

}
