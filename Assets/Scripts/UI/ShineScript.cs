using UnityEngine;

public class ShineScript : MonoBehaviour
{
    private void OnEnable()
    {
        Shine();
    }

    private void Shine()
    {
        transform.localPosition = new Vector3(-1000, transform.position.y, transform.position.z);
        LeanTween.moveLocalX(gameObject, 1000, 1f).setDelay(3f).setOnComplete(Shine);
    }
}
