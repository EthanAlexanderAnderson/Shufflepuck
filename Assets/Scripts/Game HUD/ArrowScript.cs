using UnityEngine;

//Attach this script to a GameObject to rotate around the target position.
public class ArrowScript : MonoBehaviour
{
    // dependancies
    private LineScript line;
    private LogicScript logic;
    private ClientLogicScript clientLogic;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite noSpin;
    [SerializeField] private Sprite lightSpin;
    [SerializeField] private Sprite heavySpin;

    public GameObject anchorPoint;

    private float sideModifier;
    private string activeBar;

    private bool darkMode;

    // moving parameters
    [SerializeField] private float angleParameter = 0.6f;
    [SerializeField] private float powerParameter = 10f;

    // Start is called before the first frame update
    void Start()
    {
        line = LineScript.Instance;
        logic = LogicScript.Instance;
        clientLogic = ClientLogicScript.Instance;
    }

    private void OnEnable()
    {
        darkMode = PlayerPrefs.GetInt("darkMode", 0) == 1;
    }

    //Assign a GameObject in the Inspector to rotate around
    void Update()
    {
        if (logic.gameIsRunning)
        {
            sideModifier = logic.activeCompetitor.isPlayer ? (-3.6f) : (3.6f);
            activeBar = logic.activeBar;
        } 
        else
        {
            sideModifier = clientLogic.IsStartingPlayer() ? (-3.6f) : (3.6f);
            activeBar = clientLogic.activeBar;
        }

        // set side
        anchorPoint.transform.localPosition = new Vector3(sideModifier, 7f, 1);

        // calculate the pucks estimated position
        if (activeBar == "angle")
        {
            transform.localPosition = new Vector3(0, 9, 0);
            // Spin the object around the target at 20 degrees/second.
            anchorPoint.transform.rotation = Quaternion.Euler(0, 0, (-line.GetValue() + 50f) * angleParameter);
            spriteRenderer.sprite = noSpin;
        }
        else if (activeBar == "power")
        {
            transform.localPosition = new Vector3(0, (line.GetValue()/powerParameter) + 1.5f, 0);
            spriteRenderer.sprite = noSpin;
        }
        else if (activeBar == "spin")
        {
            // change image to curved arrow
            if (line.GetValue() < 20 || line.GetValue() > 80)
            {
                // flip the arrow
                spriteRenderer.sprite = heavySpin;
            }
            else if (line.GetValue() < 40 || line.GetValue() > 60)
            {
                spriteRenderer.sprite = lightSpin;
            }
            else
            {
                spriteRenderer.sprite = noSpin;
            }

            spriteRenderer.flipX = (line.GetValue() < 50);
        }
        spriteRenderer.color = darkMode ? Color.white : Color.black;
    }
}