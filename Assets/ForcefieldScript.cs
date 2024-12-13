using UnityEngine;

public class ForcefieldScript : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D boxCollider;
    [SerializeField] private bool forcefieldIsEnabled;
    [SerializeField] private bool playersPuck;

    void FixedUpdate()
    {
        if (!forcefieldIsEnabled) { return; }
        // get all pucks within boxCollider
        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, 0);
        foreach (Collider2D collider in colliders)
        {
            // if the collider is a puck
            if (collider.CompareTag("puck") && collider.GetComponent<PuckScript>().IsPlayersPuck() == playersPuck)
            {
                // get the puck's rigidbody
                Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
                // apply a force to the puck in the direction of the forcefield
                rb.AddForce(Vector3.down * 5);
            }
        }
    }

    public void EnableForcefield(bool isPlayersPuck)
    {
        forcefieldIsEnabled = true;
        playersPuck = isPlayersPuck;
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
    }

    public void DisableForcefield()
    {
        forcefieldIsEnabled = false;
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
    }

    public bool IsPlayers()
    {
        return playersPuck;
    }
}
