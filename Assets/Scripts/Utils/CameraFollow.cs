using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private void LateUpdate()
    {
        if(target == null) return;

        Vector3 desideredPosistion = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desideredPosistion, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
    public void SetTarget(Transform newTarget) 
    {
        target = newTarget;
    }
}
