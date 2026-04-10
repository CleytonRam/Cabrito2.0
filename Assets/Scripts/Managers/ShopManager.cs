using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

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

    [Header("Purchase Popup")]
    public GameObject purchasePopupPanel;
    public TextMeshProUGUI popupMessageText;
    public Button popupCloseButton;

    [Header("Slots Fixos")]
    public Transform[] slotPositions;

    [Header("Prefabs")]
    public GameObject shopSlotPrefab;

    [Header("UI References")]
    public Transform slotsContainer;
    public TextMeshProUGUI coinsText;
    public Button backButton;

    private List<ItemData> currentShopItems = new List<ItemData>();
    private List<GameObject> currentSlots = new List<GameObject>();
    private bool isPopupActive = false;

    private void Start()
    {
        UpdateCoinsUI();
        GenerateShop();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(ClosePopup);

    }

    public void UpdateCoinsUI()
    {
        if (coinsText != null && GameManager.Instance != null)
        {
            coinsText.text = $"Moedas: {GameManager.Instance.coins}";
        }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void GenerateShop()
    {
        foreach (var slot in currentSlots)
            if (slot != null) Destroy(slot);
        currentSlots.Clear();
        currentShopItems.Clear();

        for (int i = 0; i < itemSlots; i++)
        {
            if (i >= slotPositions.Length) break;

            ItemData selectedItem = GetRandomItemByRarity();
            if (selectedItem != null)
            {
                currentShopItems.Add(selectedItem);

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

        if (availableCommons.Count > 0) return availableCommons[0];
        if (availableUncommons.Count > 0) return availableUncommons[0];
        if (availableRares.Count > 0) return availableRares[0];
        if (availableLegendaries.Count > 0) return availableLegendaries[0];

        return null;
    }

    private List<ItemData> GetAvailableItems(List<ItemData> pool)
    {
        List<ItemData> available = new List<ItemData>();

        foreach (ItemData item in pool)
        {
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
            GameManager.Instance.coins -= item.price;

            if (item.isUnique)
            {
                GameManager.Instance.MarkItemAsPurchased(item);
            }

            GameManager.Instance.AddItem(item);

            Debug.Log($"Comprou: {item.itemName} (Raridade: {item.rarity})");

            Destroy(slot);

            currentSlots.Remove(slot);

            currentShopItems.Remove(item);

            UpdateCoinsUI();
            ShowPurchasePopup(item.itemName, item.description);
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
        }
    }

    public void OnBackButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentNode != null)
        {
            MapNode shopNode = GameManager.Instance.currentNode;

            shopNode.isVisited = true;

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

    public void ShowPurchasePopup(string itemName, string itemDescription)
    {
        if (purchasePopupPanel == null) return;

        popupMessageText.text = $"Você comprou:\n<color=yellow>{itemName}</color>\n{itemDescription}";
        purchasePopupPanel.SetActive(true);
        isPopupActive = true;

        SetShopButtonsInteractable(false);
    }
    private void ClosePopup()
    {
        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(false);
        isPopupActive = false;

        SetShopButtonsInteractable(true);
    }
    private void SetShopButtonsInteractable(bool interactable)
    {
        foreach (var slot in currentSlots)
        {
            Button btn = slot.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
        if (backButton != null) backButton.interactable = interactable;
    }

    public void ReturnToMap()
    {
        if (GameManager.Instance != null)
            SceneManager.LoadScene(GameManager.Instance.mapScene);
    }
}