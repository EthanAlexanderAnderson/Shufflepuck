/* Each scoring zone has one of these scripts,
 * and a collider to detect pucks.
 */

using UnityEngine;

public class ScoreZoneScript : MonoBehaviour
{
    public bool boundary;
    public int zoneMultiplier;
    [SerializeField] private bool isContained;

    private PuckScript puck;
    private LogicScript logic;

    private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // when a puck enters, trigger it's function
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            FlashZoneBorder();
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            puck.EnterScoreZone(true, zoneMultiplier, boundary);
            logic.UpdateScores();
        }
    }

    // when a puck exits, trigger it's function
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isContained && !boundary) { return; }

        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            puck.ExitScoreZone(boundary, zoneMultiplier);
            logic.UpdateScores();
        }
    }

    // These two are only used to help me create CPU paths
    private void FlashZoneBorder()
    {
        if (lineRenderer == null) { return; }

        lineRenderer.enabled = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;

        // Tween Opacity
        LeanTween.value(gameObject, 1f, 0f, 0.5f)
            .setOnUpdate((float alpha) =>
            {
                Color startColor = lineRenderer.startColor;
                Color endColor = lineRenderer.endColor;
                startColor.a = alpha;
                endColor.a = alpha;
                lineRenderer.startColor = startColor;
                lineRenderer.endColor = endColor;
            })
            .setEase(LeanTweenType.easeOutQuad);

        // Tween Width
        LeanTween.value(gameObject, 1f, 0f, 0.5f)
            .setOnUpdate((float width) =>
            {
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
            })
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => lineRenderer.enabled = false); // Disable after fade-out
    }
}
