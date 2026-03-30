using UnityEngine;
using UnityEngine.UI;

public class ActiveItemUI : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;
    public Image cooldownBar;

    private ItemData currentItem;
    private int currentCooldown;
    private int totalCooldown;

    private void Start()
    {
        UpdateDisplay(null, 0);
    }

    public void UpdateDisplay(ItemData item, int cooldown)
    {
        currentItem = item;
        currentCooldown = cooldown;

        if (item != null) 
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;

            totalCooldown = item.cooldownNodes;

            float fill = (totalCooldown - cooldown) / (float)totalCooldown;
            cooldownBar.fillAmount = Mathf.Clamp01(fill);
        }

        else 
        {
            iconImage.sprite = null;
            iconImage.color = new Color(0.3f, 0.3f, 0.3f);
            cooldownBar.fillAmount = 0;
        }
    }
}
