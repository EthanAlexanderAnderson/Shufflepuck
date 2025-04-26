using UnityEngine;

public class PlinkoBucketScript : MonoBehaviour
{
    //dependacies
    private PlinkoManager plinkoManager;

    public int mainSideBonus;

    private void OnEnable()
    {
        plinkoManager = PlinkoManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("puck"))
        {
            return;
        }

        if (plinkoManager == null)
        {
            plinkoManager = PlinkoManager.Instance;
        }

        if (mainSideBonus <= 0)
        {
            plinkoManager.MainReward(transform);
        }
        else if (mainSideBonus == 1)
        {
            plinkoManager.SideReward(transform);
        }
        else
        {
            plinkoManager.BonusReward(transform);
        }
    }
}
