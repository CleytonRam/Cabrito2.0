using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeButton : MonoBehaviour
{
    public MapNode node;
    public Image background;
    public TextMeshProUGUI label;
    public Button button;

    private void Start()
    {
        button.onClick.AddListener(OnNodeClicked);
    }

    public void UpdateAppearance()
    {
        if (node == null) return;

        switch (node.nodeType)
        {
            case NodeType.Normal: label.text = "N"; break;
            case NodeType.Dueto: label.text = "D"; break;
            case NodeType.Quarteto: label.text = "Q"; break;
            case NodeType.Event: label.text = "?"; break;
            case NodeType.Shop: label.text = "$"; break;
            case NodeType.Boss: label.text = "B"; break;
            default: label.text = "?"; break;
        }

        if (node.isVisited)
            background.color = Color.gray;
        else if (node.isAvailable)
            background.color = Color.white;
        else
            background.color = Color.red;

        button.interactable = node.isAvailable && !node.isVisited;
    }

    public void OnNodeClicked()
    {
        if (node != null && node.isAvailable && !node.isVisited)
        {
            GameManager.Instance.EnterNode(node);
            // Não marca como visited aqui - será marcado quando voltar ao mapa
            // (o estado será restaurado pelo GameManager)
        }
    }
}