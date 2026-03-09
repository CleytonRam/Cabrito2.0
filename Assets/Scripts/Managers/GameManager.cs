using UnityEngine;
using Ebac.Core.Singleton;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Run State")]
    public int currentFloor = 0;
    public int maxFloors = 3;
    public int coins = 0;
    public List<Item> items = new List<Item>();

    [Header("Current Node")]
    public MapNode currentNode;

    [Header("Scenes")]
    public string menuScene = "Menu";
    public string mapScene = "Map";

    // Lista para salvar o estado dos nós do andar atual
    private List<MapNode> savedFloorNodes = new List<MapNode>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewRun()
    {
        currentFloor = 0;
        coins = 0;
        items.Clear();
        savedFloorNodes.Clear(); // Limpa qualquer estado salvo anterior
        SceneManager.LoadScene(mapScene);
    }

    public void EnterNode(MapNode node)
    {
        currentNode = node;

        // Salva o estado atual do andar antes de sair
        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        if (mapGen != null)
        {
            savedFloorNodes = mapGen.GetCurrentFloorNodes();
        }

        // Carrega a cena de jogo baseada no tipo do nó
        switch (node.nodeType)
        {
            case NodeType.Normal:
                SceneManager.LoadScene("Wordle");
                break;
            case NodeType.Dueto:
                SceneManager.LoadScene("Dueto");
                break;
            case NodeType.Quarteto:
                SceneManager.LoadScene("Quarteto");
                break;
            case NodeType.Event:
                SceneManager.LoadScene("Event");
                break;
            case NodeType.Shop:
                SceneManager.LoadScene("Shop");
                break;
            case NodeType.Boss:
                SceneManager.LoadScene("Wordle"); // por enquanto, Wordle normal
                break;
            default:
                SceneManager.LoadScene("Wordle");
                break;
        }
    }

    public void OnNodeComplete(bool won)
    {
        if (won)
        {
            coins += Random.Range(2, 12);
            Debug.Log($"Ganhou a rodada! Moedas: {coins}");

            // Se era um boss, avançar andar
            if (currentNode != null && currentNode.nodeType == NodeType.Boss)
            {
                AdvanceToNextFloor();
            }
            else
            {
                // --- NOVO: Libera o próximo nó no estado salvo ---
                if (savedFloorNodes != null && savedFloorNodes.Count > 0 && currentNode != null)
                {
                    // Encontra o índice do nó que acabamos de vencer
                    int currentIndex = -1;
                    for (int i = 0; i < savedFloorNodes.Count; i++)
                    {
                        if (savedFloorNodes[i].nodeType == currentNode.nodeType &&
                            savedFloorNodes[i].position == currentNode.position)
                        {
                            currentIndex = i;
                            break;
                        }
                    }

                    // Se encontrou e não é o último, libera o próximo
                    if (currentIndex >= 0 && currentIndex < savedFloorNodes.Count - 1)
                    {
                        savedFloorNodes[currentIndex].isVisited = true;
                        savedFloorNodes[currentIndex + 1].isAvailable = true;
                        Debug.Log($"Liberou nó {currentIndex + 2} (índice {currentIndex + 1})");
                    }
                }
                // ------------------------------------------------

                // Volta ao mapa - o MapGenerator vai restaurar o estado salvo
                SceneManager.LoadScene(mapScene);
            }

            if (Random.value < 0.5f)
            {
                Debug.Log("Ganhou um item! (em breve)");
            }
        }
        else
        {
            Debug.Log("Perdeu a rodada");
            SceneManager.LoadScene(mapScene);
        }
    }

    private void AdvanceToNextFloor()
    {
        currentFloor++;
        if (currentFloor < maxFloors)
        {
            // Limpa o estado salvo (novo andar = novo estado)
            savedFloorNodes.Clear();
            SceneManager.LoadScene(mapScene);
        }
        else
        {
            // Vitória da run!
            Debug.Log("Parabéns! Vocę completou todos os andares!");
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    // Método para o MapGenerator pegar o estado salvo
    public List<MapNode> GetSavedFloorNodes()
    {
        return savedFloorNodes;
    }
}