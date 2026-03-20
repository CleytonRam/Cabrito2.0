using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("Config")]
    public int itemSlots = 4;

    [Header("Item pools")]
    public List<ItemData> commonItems;
    public List<ItemData> uncommonItems;
    public List<ItemData> rareItems;
    public List<ItemData> legendaryItems;

    [Header("Chances")]
    [Range(0, 100)] public int commonChance = 60;
    [Range(0, 100)] public int uncommonChance = 25;
    [Range(0, 100)] public int rareChance = 10;
    [Range(0, 100)] public int legendaryChance = 5;

    [Header("Slots Fixos")]
    public Transform[] slotPositions;

    [Header("Prefabs")]
    public GameObject shopSlotPrefab;

    [Header("UI References")]
    public Transform slotsContainer;
    public TextMeshProUGUI coinsText;
    public Button backButton;

    // Guarda os itens que estão na loja atual
    private List<ItemData> currentShopItems = new List<ItemData>();
    // Guarda os slots GameObject para poder destruir
    private List<GameObject> currentSlots = new List<GameObject>();

    private void Start()
    {
        UpdateCoinsUI();
        GenerateShop();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void UpdateCoinsUI()
    {
        if (coinsText != null && GameManager.Instance != null)
        {
            coinsText.text = $"Moedas: {GameManager.Instance.coins}";
        }
    }

    public void GenerateShop()
    {
        // Limpa slots antigos
        foreach (var slot in currentSlots)
            if (slot != null) Destroy(slot);
        currentSlots.Clear();
        currentShopItems.Clear();

        // Gera novos itens
        for (int i = 0; i < itemSlots; i++)
        {
            if (i >= slotPositions.Length) break;

            ItemData selectedItem = GetRandomItemByRarity();
            if (selectedItem != null)
            {
                currentShopItems.Add(selectedItem);

                // Instancia o slot na posição fixa
                GameObject slotGO = Instantiate(shopSlotPrefab, slotPositions[i].position, Quaternion.identity, slotsContainer);
                slotGO.transform.position = slotPositions[i].position;
                currentSlots.Add(slotGO);

                CreateSlot(selectedItem, slotGO);
            }
        }
    }

    public ItemData GetRandomItemByRarity()
    {
        int roll = Random.Range(0, 100);
        int cumulative = 0;

        // Verifica se o item é único (não foi comprado antes)
        List<ItemData> availableCommons = GetAvailableItems(commonItems);
        List<ItemData> availableUncommons = GetAvailableItems(uncommonItems);
        List<ItemData> availableRares = GetAvailableItems(rareItems);
        List<ItemData> availableLegendaries = GetAvailableItems(legendaryItems);

        cumulative += commonChance;
        if (roll < cumulative && availableCommons.Count > 0)
            return availableCommons[Random.Range(0, availableCommons.Count)];

        cumulative += uncommonChance;
        if (roll < cumulative && availableUncommons.Count > 0)
            return availableUncommons[Random.Range(0, availableUncommons.Count)];

        cumulative += rareChance;
        if (roll < cumulative && availableRares.Count > 0)
            return availableRares[Random.Range(0, availableRares.Count)];

        cumulative += legendaryChance;
        if (roll < cumulative && availableLegendaries.Count > 0)
            return availableLegendaries[Random.Range(0, availableLegendaries.Count)];

        // Fallback: pega qualquer item disponível
        if (availableCommons.Count > 0) return availableCommons[0];
        if (availableUncommons.Count > 0) return availableUncommons[0];
        if (availableRares.Count > 0) return availableRares[0];
        if (availableLegendaries.Count > 0) return availableLegendaries[0];

        return null;
    }

    // Filtra apenas os itens que ainda não foram comprados (se forem únicos)
    private List<ItemData> GetAvailableItems(List<ItemData> pool)
    {
        List<ItemData> available = new List<ItemData>();

        foreach (ItemData item in pool)
        {
            // Se o item não for único, ou se for único mas ainda não foi comprado
            if (!item.isUnique || !GameManager.Instance.IsItemPurchased(item))
            {
                available.Add(item);
            }
        }

        return available;
    }

    public void CreateSlot(ItemData item, GameObject slotGO)
    {
        Image iconImage = slotGO.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = slotGO.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = slotGO.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
        Button buyButton = slotGO.GetComponent<Button>();

        if (iconImage != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
        }

        if (nameText != null)
            nameText.text = item.itemName;

        if (priceText != null)
            priceText.text = item.price.ToString();

        //Image background = slotGO.GetComponent<Image>();
        //if (background != null)
        //{
        //    switch (item.rarity)
        //    {
        //        case ItemData.Rarity.Comum:
        //            background.color = Color.white;
        //            break;
        //        case ItemData.Rarity.Incomum:
        //            background.color = Color.blue;
        //            break;
        //        case ItemData.Rarity.Raro:
        //            background.color = Color.magenta;
        //            break;
        //        case ItemData.Rarity.Lendario:
        //            background.color = Color.yellow;
        //            break;
        //        case ItemData.Rarity.Maldito:
        //            background.color = Color.red;
        //            break;
        //    }
        //}

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => BuyItem(item, slotGO));
        }
    }

    public void BuyItem(ItemData item, GameObject slot)
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.coins >= item.price)
        {
            // Paga
            GameManager.Instance.coins -= item.price;

            // Se for item único, marca como comprado
            if (item.isUnique)
            {
                GameManager.Instance.MarkItemAsPurchased(item);
            }

            // Adiciona item ao inventário
            GameManager.Instance.AddItem(item);

            Debug.Log($"Comprou: {item.itemName} (Raridade: {item.rarity})");

            // REMOVE O SLOT DA PRATELEIRA
            Destroy(slot);

            // Remove da lista de slots ativos
            currentSlots.Remove(slot);

            // Remove da lista de itens da loja
            currentShopItems.Remove(item);

            // Atualiza moedas na UI
            UpdateCoinsUI();
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
        }
    }

    // Chamado quando clica no botão voltar
    public void OnBackButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentNode != null)
        {
            MapNode shopNode = GameManager.Instance.currentNode;

            // Marca o nó da loja como visitado
            shopNode.isVisited = true;

            // Pega os nós do andar salvos no GameManager
            List<MapNode> floorNodes = GameManager.Instance.GetSavedFloorNodes();
            if (floorNodes != null)
            {
                int currentIndex = floorNodes.IndexOf(shopNode);
                if (currentIndex >= 0 && currentIndex < floorNodes.Count - 1)
                {
                    MapNode nextNode = floorNodes[currentIndex + 1];
                    nextNode.isAvailable = true;
                    Debug.Log($"Liberou nó {currentIndex + 2} (próximo após a loja)");
                }
            }
        }

        ReturnToMap();
    }

    public void ReturnToMap()
    {
        if (GameManager.Instance != null)
            SceneManager.LoadScene(GameManager.Instance.mapScene);
    }
}