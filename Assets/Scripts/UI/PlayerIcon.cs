using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerIcon : MonoBehaviour
{
    [Header("Configs")]
    public Vector2 offset = new Vector2(0,60);

    [Header("Sprites por andar")]
    public List<Sprite> spritesPorAndar;

    [Header("Referências")]
    public Image iconImage;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (iconImage == null)
            iconImage = GetComponent<Image>();
    }

    public void UpdateIcon()
    {
        if (GameManager.Instance == null) return;

        int currentFloor = GameManager.Instance.currentFloor;

        if (spritesPorAndar != null && currentFloor < spritesPorAndar.Count)
        {
            iconImage.sprite = spritesPorAndar[currentFloor];
        }
        else if (spritesPorAndar != null && spritesPorAndar.Count > 0)
        {
            iconImage.sprite = spritesPorAndar[spritesPorAndar.Count - 1];
        }
    }

    public void AttachToNode(Transform nodeTransform)
    {
        transform.SetParent(nodeTransform);
        rectTransform.anchoredPosition = offset; // 60 pixels acima do nó
        rectTransform.localScale = Vector3.one;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}