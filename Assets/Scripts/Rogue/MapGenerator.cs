using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MapGenerator : MonoBehaviour
{
    [Header("Configurações do Mapa")]
    public int nodesPerFloor = 5; // Incluindo o boss
    public float horizontalSpacing = 200f;

    [Header("Prefabs")]
    public GameObject nodeButtonPrefab;

    [Header("UI References")]
    public Transform nodesContainer;
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI coinsText;

    private List<MapNodeButton> nodeButtons = new List<MapNodeButton>();
    private List<MapNode> currentFloorNodes = new List<MapNode>();

    private void Start()
    {
        // Verifica se temos um estado salvo
        List<MapNode> savedNodes = GameManager.Instance.GetSavedFloorNodes();

        if (savedNodes != null && savedNodes.Count > 0)
        {
            // Restaura o estado salvo
            RestoreFloor(savedNodes);
        }
        else
        {
            // Gera um novo andar
            GenerateFloor(GameManager.Instance.currentFloor);
        }

        UpdateUI();
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

        // Gera os nós do andar
        for (int i = 0; i < nodesPerFloor; i++)
        {
            NodeType type;

            // Último nó é sempre boss
            if (i == nodesPerFloor - 1)
                type = NodeType.Boss;
            else
                type = GetRandomNodeType(floorIndex);

            MapNode node = new MapNode(type, new Vector2(i, floorIndex));

            // Lógica de disponibilidade:
            // - Primeiro nó (índice 0) sempre disponível
            // - Demais nós começam indisponíveis
            node.isAvailable = (i == 0);
            node.isVisited = false;

            currentFloorNodes.Add(node);
            CreateNodeButton(node, floorIndex, i);
        }
    }

    public void RestoreFloor(List<MapNode> savedNodes)
    {
        // Limpa nós anteriores
        foreach (var btn in nodeButtons)
            if (btn != null) Destroy(btn.gameObject);
        nodeButtons.Clear();

        currentFloorNodes = savedNodes;

        // Recria os botões com o estado salvo
        for (int i = 0; i < currentFloorNodes.Count; i++)
        {
            CreateNodeButton(currentFloorNodes[i], GameManager.Instance.currentFloor, i);
        }
    }

    private NodeType GetRandomNodeType(int floorIndex)
    {
        // Distribuição simples
        float r = Random.value;
        if (r < 0.5f) return NodeType.Normal;
        else if (r < 0.8f) return NodeType.Dueto;
        else return NodeType.Quarteto;
    }

    private void CreateNodeButton(MapNode node, int floor, int index)
    {
        GameObject btnGO = Instantiate(nodeButtonPrefab, nodesContainer);
        RectTransform rect = btnGO.GetComponent<RectTransform>();

        float x = (index - (nodesPerFloor - 1) / 2f) * horizontalSpacing;
        rect.anchoredPosition = new Vector2(x, 0);

        MapNodeButton btn = btnGO.GetComponent<MapNodeButton>();
        btn.node = node;
        btn.UpdateAppearance();

        nodeButtons.Add(btn);
    }

    // Este método é chamado quando um nó é completado (via GameManager)
    public void OnNodeCompleted(MapNode completedNode)
    {
        int index = currentFloorNodes.IndexOf(completedNode);

        // Se não for o último nó (boss), libera o próximo
        if (index >= 0 && index < currentFloorNodes.Count - 1)
        {
            MapNode nextNode = currentFloorNodes[index + 1];
            nextNode.isAvailable = true;

            // Atualiza a aparência do botão do próximo nó
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