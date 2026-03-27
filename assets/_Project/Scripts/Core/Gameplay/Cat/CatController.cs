using UnityEngine;

public class CatController : MonoBehaviour
{
    [SerializeField] private float limitLeft;
    [SerializeField] private float limitRight;

    public void MoveNormalized(float t, float deltaX)
    {
        float targetX = Mathf.Lerp(limitLeft, limitRight, t);

        if (Mathf.Abs(deltaX) > 0.01f)
        {
            float dir = Mathf.Sign(deltaX); 

            transform.localScale = new Vector3(
                dir * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }
}