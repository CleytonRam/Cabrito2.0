using UnityEngine;

public class MapNodeButton : MonoBehaviour
{
    public MapNode node;

    public void OnNodeClicked() 
    {
        if(GameManager.Instance != null && node != null) 
        {
            GameManager.Instance.EnterNode(node);
        }
        else
        {
            Debug.LogError("Game manager ou node nao encontrado");
        }
    }
}
