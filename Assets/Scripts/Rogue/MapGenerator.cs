using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MapGenerator : MonoBehaviour
{
    [Header("Configurações do Mapa")]
    public int nodesPerFloor = 5; // Incluindo o boss
    public float horizontalSpacing = 200f;
    [Range(0f, 1f)] public float eventChance = 0.4f;

    [Header("Prefabs")]
    public GameObject nodeButtonPrefab;
    public GameObject playerIconPrefab;

    [Header("UI References")]
    public Transform nodesContainer;
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI coinsText;

    private List<MapNodeButton> nodeButtons = new List<MapNodeButton>();
    private List<MapNode> currentFloorNodes = new List<MapNode>();
    private Dictionary<MapNode, MapNodeButton> nodeToButton = new Dictionary<MapNode, MapNodeButton>();

    private void Start()
    {
        List<MapNode> savedNodes = GameManager.Instance.GetSavedFloorNodes();

        if (savedNodes != null && savedNodes.Count > 0)
        {
            RestoreFloor(savedNodes);
        }
        else
        {
            GenerateFloor(GameManager.Instance.currentFloor);
        }

        MapCameraController camController = FindObjectOfType<MapCameraController>();
        if (camController != null)
        {
            camController.container = nodesContainer; // o pai dos nós
        }

        PositionIconOnCurrentNode();
        UpdateUI();
    }
    private void OnEnable()
    {
        PositionIconOnCurrentNode();
    }

    private void Update()
    {
        if (coinsText != null && GameManager.Instance != null)
            coinsText.text = $"Moedas: {GameManager.Instance.coins}";
    }

    public void GenerateFloor(int floorIndex)
    {
        // Limpa nós anteriores
        foreach (var btn in nodeButtons)
            if (btn != null) Destroy(btn.gameObject);
        nodeButtons.Clear();
        currentFloorNodes.Clear();

        List<MapNode> mainNodes = new List<MapNode>();
        bool hasShopThisFloor = (floorIndex >= 1);
        int shopPosition = hasShopThisFloor ? Random.Range(0, nodesPerFloor - 1) : -1;

        for (int i = 0; i < nodesPerFloor; i++)
        {
            NodeType type;
            if (i == nodesPerFloor - 1)
                type = NodeType.Boss;
            else if (i == shopPosition)
                type = NodeType.Shop;
            else
                type = GetRandomNodeType(floorIndex);

            MapNode node = new MapNode(type, new Vector2(i, floorIndex));
            mainNodes.Add(node);
        }

        List<MapNode> finalNodes = new List<MapNode>();
        for (int i = 0; i < mainNodes.Count; i++)
        {
            finalNodes.Add(mainNodes[i]);
            if (i < mainNodes.Count - 1 && Random.value < eventChance)
            {
                MapNode eventNode = new MapNode(NodeType.Event, new Vector2(i + 0.5f, floorIndex));
                finalNodes.Add(eventNode);
            }
        }

        for (int i = 0; i < finalNodes.Count; i++)
        {
            finalNodes[i].isAvailable = (i == 0);
            finalNodes[i].isVisited = false;
            currentFloorNodes.Add(finalNodes[i]);
        }

        for (int i = 0; i < currentFloorNodes.Count; i++)
        {
            CreateNodeButton(currentFloorNodes[i], floorIndex, i);
        }

        UpdateUI();
    }


    public void RestoreFloor(List<MapNode> savedNodes)
    {
        foreach (var btn in nodeButtons)
            if (btn != null) Destroy(btn.gameObject);
        nodeButtons.Clear();

        currentFloorNodes = savedNodes;

        for (int i = 0; i < currentFloorNodes.Count; i++)
        {
            CreateNodeButton(currentFloorNodes[i], GameManager.Instance.currentFloor, i);
        }
    }

    private NodeType GetRandomNodeType(int floorIndex)
    {
        float r = Random.value;
        if (r < 0.5f) return NodeType.Normal;
        else if (r < 0.8f) return NodeType.Dueto;
        else return NodeType.Quarteto;

    }

    private void CreateNodeButton(MapNode node, int floor, int index)
    {
        GameObject btnGO = Instantiate(nodeButtonPrefab, nodesContainer);
        RectTransform rect = btnGO.GetComponent<RectTransform>();
        float x = (index - (currentFloorNodes.Count - 1) / 2f) * horizontalSpacing;
        rect.anchoredPosition = new Vector2(x, 0);
        MapNodeButton btn = btnGO.GetComponent<MapNodeButton>();
        btn.node = node;
        btn.UpdateAppearance();
        nodeButtons.Add(btn);
        nodeToButton[node] = btn;

    }
    public void FocusOnNode(MapNode node)
    {
        if (nodeToButton.ContainsKey(node))
        {
            MapNodeButton targetButton = nodeToButton[node];

            MapCameraController camController = FindObjectOfType<MapCameraController>();
            if (camController != null)
            {
                camController.SetTarget(targetButton.transform);
            }

            PlayerIcon playerIcon = FindObjectOfType<PlayerIcon>();
            if (playerIcon != null)
            {
                playerIcon.AttachToNode(targetButton.transform);
                playerIcon.SetVisible(true);
            }
        }
    }
    private void PositionIconOnCurrentNode()
    {
        Debug.Log("PositionIconOnCurrentNode chamado");

        MapNode currentNode = null;
        foreach (var node in currentFloorNodes)
        {
            if (node.isAvailable && !node.isVisited)
            {
                currentNode = node;
                Debug.Log($"Nó atual (jogável): tipo={node.nodeType}");
                break;
            }
        }

        if (currentNode == null)
        {
            foreach (var node in currentFloorNodes)
            {
                if (node.isVisited)
                    currentNode = node;
            }
            if (currentNode != null)
                Debug.Log($"Nó atual (último visitado): tipo={currentNode.nodeType}");
        }

        // Encontra ou instancia o ícone
        PlayerIcon playerIcon = FindObjectOfType<PlayerIcon>();
        if (playerIcon == null && playerIconPrefab != null)
        {
            GameObject iconGO = Instantiate(playerIconPrefab);
            playerIcon = iconGO.GetComponent<PlayerIcon>();
            playerIcon.transform.SetParent(GameObject.Find("Canvas").transform, false);
        }

        if (playerIcon == null)
        {
            Debug.LogError("PlayerIcon não encontrado!");
            return;
        }

        if (currentNode != null && nodeToButton.ContainsKey(currentNode))
        {
            Debug.Log($"Anexando ícone ao nó: {currentNode.nodeType}");
            playerIcon.AttachToNode(nodeToButton[currentNode].transform);
            playerIcon.SetVisible(true);
            playerIcon.UpdateIcon();

            MapCameraController camController = FindObjectOfType<MapCameraController>();
            if (camController != null)
            {
                camController.SetTarget(nodeToButton[currentNode].transform);
            }
        }
        else
        {
            Debug.LogWarning("Nó atual não encontrado");
            playerIcon.SetVisible(true);
        }
    }

    public void OnNodeCompleted(MapNode completedNode)
    {
        int index = currentFloorNodes.IndexOf(completedNode);

        if (index >= 0 && index < currentFloorNodes.Count - 1)
        {
            MapNode nextNode = currentFloorNodes[index + 1];
            nextNode.isAvailable = true;

            foreach (var btn in nodeButtons)
            {
                if (btn.node == nextNode)
                {
                    btn.UpdateAppearance();
                    break;
                }
            }
        }
    }

    public List<MapNode> GetCurrentFloorNodes()
    {
        return currentFloorNodes;
    }

    private void UpdateUI()
    {
        if (floorText != null && GameManager.Instance != null)
            floorText.text = $"Andar {GameManager.Instance.currentFloor + 1}/{GameManager.Instance.maxFloors}";
    }
    
}