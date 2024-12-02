using UnityEngine;

public class PlinkoBucketScript : MonoBehaviour
{
    //dependacies
    private PlinkoManager plinkoManager;

    public bool isMainReward;

    private void OnEnable()
    {
        plinkoManager = PlinkoManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (plinkoManager == null)
        {
            plinkoManager = PlinkoManager.Instance;
        }

        if (isMainReward)
        {
            plinkoManager.MainReward(transform);
        }
        else
        {
            plinkoManager.SideReward(transform);
        }
    }
}
