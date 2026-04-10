using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    public Transform target;           // O nó atual (botão)
    public Transform container;        // O nodesContainer (que tem todos os botões)
    public float smoothSpeed = 5f;
    public Vector2 offset;             // Deslocamento opcional (ex: (0, 0) para centralizar)

    private Vector3 initialContainerPosition;

    private void Start()
    {
        if (container != null)
            initialContainerPosition = container.localPosition;
    }

    private void LateUpdate()
    {
        if (target == null || container == null) return;

       
        Vector3 desiredPosition = -target.localPosition + new Vector3(offset.x, offset.y, 0);

        container.localPosition = Vector3.Lerp(container.localPosition, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}