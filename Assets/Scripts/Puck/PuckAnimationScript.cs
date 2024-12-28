using UnityEngine;

public class PuckAnimationScript : MonoBehaviour
{
    private int growShrinkDirection = 1;
    [SerializeField] private float growShrinkRatio; // 0.006
    [SerializeField] private float growShrinkMin; // 0.5
    [SerializeField] private float growShrinkMax; // 1
    [SerializeField] private float rotateRatio; // 1

    // every frame, rotate the sprite by 1 degree and shrink it by 0.1%
    void FixedUpdate()
    {
        transform.Rotate(0, 0, rotateRatio);
        transform.localScale += new Vector3(growShrinkRatio, growShrinkRatio, growShrinkRatio) * growShrinkDirection;
        if (transform.localScale.x > growShrinkMax || transform.localScale.x < growShrinkMin)
        {
            growShrinkDirection *= -1;
        }
    }
}
