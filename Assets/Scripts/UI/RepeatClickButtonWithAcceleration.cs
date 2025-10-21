// From Chat GPT

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class RepeatClickButtonWithAcceleration : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float repeatDelay = 0.5f;        // Initial delay before repeating starts
    public float initialRepeatRate = 0.2f;  // Starting repeat rate
    public float minRepeatRate = 0.05f;     // Minimum delay between repeats (max speed)
    public float acceleration = 0.95f;      // How much the repeatRate is multiplied per repeat (e.g. 0.95 = 5% faster)

    private Button button;
    private bool isHeld = false;
    private Coroutine repeatCoroutine;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        isHeld = true;
        repeatCoroutine = StartCoroutine(RepeatClickAccelerated());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        if (repeatCoroutine != null)
            StopCoroutine(repeatCoroutine);
    }

    private IEnumerator RepeatClickAccelerated()
    {
        yield return new WaitForSeconds(repeatDelay);

        float currentRate = initialRepeatRate;

        while (isHeld)
        {
            if (button.interactable)
                button.onClick.Invoke();
            yield return new WaitForSeconds(currentRate);
            currentRate = Mathf.Max(minRepeatRate, currentRate * acceleration);
        }
    }
}
