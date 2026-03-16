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

    [Header("Health System")]
    [SerializeField] private int startingHealth = 3;
    private HealthSystem healthSystem;
    public HealthSystem Health => healthSystem;

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

        healthSystem = new HealthSystem(startingHealth);
        healthSystem.OnDeath += OnPlayerDeath;
        healthSystem.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= OnPlayerDeath;
            healthSystem.OnHealthChanged -= OnHealthChanged;
        }
    }

    public void StartNewRun()
    {
        currentFloor = 0;
        coins = 0;
        items.Clear();
        savedFloorNodes.Clear();

        healthSystem = new HealthSystem(startingHealth);
        healthSystem.OnDeath += OnPlayerDeath;
        healthSystem.OnHealthChanged += OnHealthChanged;

        SceneManager.LoadScene(mapScene);
    }

    public void EnterNode(MapNode node)
    {
        currentNode = node;

        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        if (mapGen != null)
        {
            savedFloorNodes = mapGen.GetCurrentFloorNodes();
        }

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
                SceneManager.LoadScene("Wordle");
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

            if (currentNode != null && currentNode.nodeType == NodeType.Boss)
            {
                AdvanceToNextFloor();
            }
            else
            {
                if (savedFloorNodes != null && savedFloorNodes.Count > 0 && currentNode != null)
                {
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

                    if (currentIndex >= 0 && currentIndex < savedFloorNodes.Count - 1)
                    {
                        savedFloorNodes[currentIndex].isVisited = true;
                        savedFloorNodes[currentIndex + 1].isAvailable = true;
                        Debug.Log($"Liberou nó {currentIndex + 2}");
                    }
                }

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
            RemoveHealth(1);

            if (Health.isDead)
            {
                Debug.Log("Jogador morreu, não carregar mapa");
                return; 
            }

            SceneManager.LoadScene(mapScene);
        }
    }

    private void AdvanceToNextFloor()
    {
        currentFloor++;
        if (currentFloor < maxFloors)
        {
            savedFloorNodes.Clear();
            SceneManager.LoadScene(mapScene);
        }
        else
        {
            Debug.Log("Parabéns! Você completou todos os andares!");
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    public List<MapNode> GetSavedFloorNodes()
    {
        return savedFloorNodes;
    }

    #region ========== MÉTODOS DO SISTEMA DE VIDA ==========

    public void AddHealth(int amount)
    {
        if (healthSystem != null)
            healthSystem.Heal(amount);
    }

    public void RemoveHealth(int amount)
    {
        Debug.Log($"RemoveHealth chamado com amount={amount}");
        if (healthSystem != null)
            healthSystem.TakeDamage(amount);
        else
            Debug.LogError("healthSystem é nulo!");
    }

    public void AddMaxHealth(int amount, bool healToo = true)
    {
        if (healthSystem != null)
            healthSystem.AddMaxHealth(amount, healToo);
    }

    private void OnPlayerDeath()
    {
        Debug.Log("GAME OVER! Você morreu!");
        ReturnToMenu();
    }

    private void OnHealthChanged(int current, int max)
    {
        Debug.Log($"Vida: {current}/{max}");

        HealthUI healthUI = FindObjectOfType<HealthUI>();
        if (healthUI != null)
        {
            healthUI.UpdateHearts(current, max);
        }
    }
    #endregion
}