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
        if (isMainReward)
        {
            plinkoManager.MainReward();
        }
        else
        {
            plinkoManager.SideReward();
        }
    }
}
