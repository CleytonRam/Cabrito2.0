using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Heart Settings")]
    public GameObject heartPrefab;
    public Transform heartsContainer;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public Sprite halfHeartSprite;

    private List<Image> heartImages = new List<Image>();

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log($"HealthUI.Start: current={GameManager.Instance.Health.CurrentHealth}, max={GameManager.Instance.Health.MaxHealth}");
            UpdateHearts(GameManager.Instance.Health.CurrentHealth, GameManager.Instance.Health.MaxHealth);
        }
    }

    public void UpdateHearts(int currentHealth, int maxHealth)
    {
        ClearHearts();

        for (int i = 0; i < maxHealth; i++)
        {
            CreateNewHeart();
        }

        for (int i = 0; i < heartImages.Count; i++)
        {
            heartImages[i].sprite = i < currentHealth ? fullHeartSprite : emptyHeartSprite;
        }
    }

    private void CreateNewHeart() 
    {
        GameObject newHeart = Instantiate(heartPrefab, heartsContainer);
        Image heartImage = newHeart.GetComponent<Image>();
        heartImages.Add(heartImage);
    }

    private void RemoveLastHeart() 
    {
        if (heartImages.Count > 0) 
        {
            int lastIndex = heartImages.Count - 1;
            Destroy(heartImages[lastIndex].gameObject);
            heartImages.RemoveAt(lastIndex);
        }
    }
    private void ClearHearts()
    {
        foreach (var heart in heartImages)
        {
            if (heart != null)
                Destroy(heart.gameObject);
        }
        heartImages.Clear();
    }

}
