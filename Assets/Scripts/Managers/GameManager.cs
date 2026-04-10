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

    [Header("Inventory")]
    public List<ItemData> ownedItems = new List<ItemData>();
    public List<ItemData> purchasedUniqueItems = new List<ItemData>();

    [Header("Active Item")]
    public ItemData activeItem;
    public int activeItemCooldown = 0;

    [Header("Health System")]
    [SerializeField] private int startingHealth = 3;
    public HealthSystem Health => healthSystem;

    [Header("Current Node")]
    public MapNode currentNode;

    [Header("Scenes")]
    public string menuScene = "Menu";
    public string mapScene = "Map";


    public bool hasContratoSangue = false;
    // Lista para salvar o estado dos nós do andar atual
    private HealthSystem healthSystem;
    private List<MapNode> savedFloorNodes = new List<MapNode>();
    private Dictionary<ItemData, int> activeItemCooldowns = new Dictionary<ItemData, int>();
    private ActiveItemUI activeItemUI;


    private void Start()
    {
        activeItemUI = FindObjectOfType<ActiveItemUI>(true);
        if(activeItemUI == null) 
        {
            Debug.LogWarning("ActiveItemUI não encontrado na cena");
        }
    }
   
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
        ownedItems.Clear();
        purchasedUniqueItems.Clear();
        activeItem = null;
        activeItemCooldown = 0;
        hasContratoSangue = false;

        healthSystem = new HealthSystem(startingHealth);
        healthSystem.OnDeath += OnPlayerDeath;
        healthSystem.OnHealthChanged += OnHealthChanged;

        SceneManager.LoadScene(mapScene);
    }

    public void EnterNode(MapNode node)
    {
        currentNode = node;
        PlayerIcon playerIcon = FindObjectOfType<PlayerIcon>();
        if (playerIcon != null) playerIcon.SetVisible(false);

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
            int baseCoins = Random.Range(2, 12);

            if (hasContratoSangue && currentNode != null && currentNode.nodeType != NodeType.Boss)
            {
                baseCoins *= 2;
                Debug.Log($"Contrato de Sangue: Moedas dobradas! +{baseCoins}");
            }
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
                        MapNode nextNode = savedFloorNodes[currentIndex + 1];
                        nextNode.isAvailable = true;
                        Debug.Log($"Liberou nó {currentIndex + 2}");

                        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
                        if (mapGen != null)
                        {
                            mapGen.FocusOnNode(nextNode);
                        }
                    }
                }
                ReduceActiveItemCooldown();
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

    #region ITEM MANAGMENT
    public void AddItem(ItemData item)
    {
        if (item.isActive)
        {
            // Se já existe um item ativo, descarta o antigo
            if (activeItem != null)
            {
                Debug.Log($"Substituindo item ativo: {activeItem.itemName} → {item.itemName}");
                // Remove o antigo da lista de itens (se você quiser que ele suma completamente)
                ownedItems.Remove(activeItem);
            }

            // Configura o novo item ativo
            activeItem = item;
            activeItemCooldown = 0;
            ownedItems.Add(item); // Adiciona à lista geral (opcional, mas útil para debug)

            // Notifica a UI para atualizar o ícone do item ativo
            UpdateActiveItemUI();
        }
        else
        {
            // Item passivo: adiciona normalmente
            ownedItems.Add(item);
            Debug.Log($"Item passivo adicionado: {item.itemName}");
        }
    }

    //public void OnNodeCompletedForCooldown()
    //{
    //    List<ItemData> itemsToUpdate = new List<ItemData>(activeItemCooldowns.Keys);
    //    foreach (var item in itemsToUpdate)
    //    {
    //        if (activeItemCooldowns[item] > 0)
    //        {
    //            activeItemCooldowns[item]--;
    //        }
    //    }
    //}
    public bool TryUseActiveItem()
    {
        Debug.Log($"Tentando usar: cooldown atual = {activeItemCooldown}");
        if (activeItem != null && activeItemCooldown == 0)
        {
            activeItemCooldown = activeItem.cooldownNodes;
            Debug.Log($"Usou! Novo cooldown = {activeItemCooldown}");
            activeItem.ApplyEffect();
            UpdateActiveItemUI();
            return true;
        }
        else if (activeItem != null && activeItemCooldown > 0)
        {
            Debug.Log($"Item ativo em cooldown: {activeItemCooldown} nós restantes");
            return false;
        }
        else
        {
            Debug.Log("Nenhum item ativo para usar");
            return false;
        }
    }
    public void ReduceActiveItemCooldown()
    {
        if (activeItem != null && activeItemCooldown > 0)
        {
            activeItemCooldown--;
            Debug.Log($"Cooldown do item ativo: {activeItemCooldown} restantes");
            UpdateActiveItemUI();
        }
    }
    public void UpdateActiveItemUI()
    {
       if(activeItemUI != null) 
       {
            activeItemUI.UpdateDisplay(activeItem, activeItemCooldown);
       }
    }
    public bool IsItemPurchased(ItemData item) 
    {
        return purchasedUniqueItems.Contains(item); 
    }

    public void MarkItemAsPurchased(ItemData item) 
    {
        if(item.isUnique && !purchasedUniqueItems.Contains(item)) 
        {
            purchasedUniqueItems.Add(item);
            Debug.Log($"Item único marcado como comprado: {item.itemName}");
        }
    }
    #endregion

    #region EVENTS
    public void OnEventCompleted()
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
                MapNode nextNode = savedFloorNodes[currentIndex + 1];
                nextNode.isAvailable = true;
                Debug.Log($"Evento completado, liberou nó {currentIndex + 2}");
            }
        }

        
    }

   
    #endregion
}