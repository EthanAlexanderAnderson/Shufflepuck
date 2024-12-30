using UnityEngine;

public class ScoreZoneCalibrator : MonoBehaviour
{
    public GameObject puck; //prefab

    public bool left;

    int clicks;

    float[] angle = { 0, 7.5f, 15, 22.5f, 30, 37.5f, 45, 52.5f, 60, 67.5f, 75, 82.5f, 90, 97.5f, 0, 9, 18, 27, 36, 45, 54, 63, 72, 81, 90, 99, 0, 10.5f, 21, 31.5f, 42, 52.5f, 63, 73.5f, 84, 94.5f, 0, 12, 24, 36, 48, 60, 72, 84, 96, 0, 17.5f, 35, 52.5f, 70, 87.5f, 0, 20, 40, 60, 80, 100, 0, 22.5f, 45, 67.5f, 90, 0, 25, 50, 75, 100 };
    float[] power = { 90, 78, 66, 55, 42 };
    float[] spin = { 50 };
    int shotAngleIterator = 0; 
    int shotPowerIterator = 0;
    int shotSpinIterator = 0;

    GameObject activePuckObject;
    PuckScript activePuckObjectScript;

    private void OnEnable()
    {
        clicks = 0;
        shotAngleIterator = 0;
        shotPowerIterator = 0;
        shotSpinIterator = 0;
    }

    void Update()
    {
        if (clicks > 10 && (activePuckObject == null || activePuckObjectScript.IsSafe()) && shotAngleIterator < angle.Length && shotPowerIterator < power.Length && shotSpinIterator < spin.Length && UIManagerScript.Instance.debugMode > 10 && !ClientLogicScript.Instance.isRunning)
        {
            activePuckObject = Instantiate(puck, new Vector3((left ? -3.6f : 3.6f), -10.0f, 0.0f), Quaternion.identity);
            activePuckObjectScript = activePuckObject.GetComponent<PuckScript>();
            activePuckObjectScript.InitPuck(false, -9999);
            activePuckObjectScript.Shoot(angle[shotAngleIterator], power[shotPowerIterator], spin[shotSpinIterator]);
        
            shotAngleIterator++;
            if (angle[shotAngleIterator] == 0) {
                shotPowerIterator++;
            }
        }
    }
    public void Increment()
    {
        if (UIManagerScript.Instance.debugMode < 10 || ClientLogicScript.Instance.isRunning) { return; }

        clicks++;
        if (clicks == 10)
        {
            PuckManager.Instance.ClearAllPucks();
        }
    }
}
