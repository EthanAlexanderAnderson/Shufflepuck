using UnityEngine;

public class ShineScript : MonoBehaviour
{
    [SerializeField] private int distance;
    [SerializeField] private float speed;

    private void OnEnable()
    {
        LeanTween.cancel(gameObject);
        transform.localPosition = new Vector3(-distance, transform.position.y, transform.position.z);
        LeanTween.moveLocalX(gameObject, distance, 1/speed).setOnComplete(Shine);
    }

    private void Shine()
    {
        transform.localPosition = new Vector3(-distance, transform.position.y, transform.position.z);
        LeanTween.moveLocalX(gameObject, distance, 1/speed).setDelay(3/speed).setOnComplete(Shine);
    }
}
