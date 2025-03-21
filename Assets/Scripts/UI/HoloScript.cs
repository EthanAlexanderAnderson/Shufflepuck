using UnityEngine;

public class HoloScript : MonoBehaviour
{
    float edge = 340;
    float time = 2;
    private void OnEnable()
    {
        LeanTween.cancel(gameObject);
        HoloLeft();
    }

    private void HoloLeft()
    {
        transform.localPosition = new Vector3(-edge, transform.position.y, transform.position.z);
        LeanTween.moveLocalX(gameObject, edge, time).setEaseInOutSine().setOnComplete(HoloRight);
    }

    private void HoloRight()
    {
        transform.localPosition = new Vector3(edge, transform.position.y, transform.position.z);
        LeanTween.moveLocalX(gameObject, -edge, time).setEaseInOutSine().setOnComplete(HoloLeft);
    }
}
