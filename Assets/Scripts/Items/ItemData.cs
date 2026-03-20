using UnityEngine;

[CreateAssetMenu(fileName = "NovoItem", menuName = "TermoRouguelike/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;

    [TextArea(2, 4)] public string description;
    public Sprite icon;
    public int price = 10;
    public Rarity rarity;
    public bool isUnique = false;
    public bool isActive;
    public int cooldownNodes = 2;

    public enum Rarity {Comum, Incomum, Raro, Lendario, Maldito}

    public virtual void ApplyEffect() 
    {
        Debug.Log($"Item pegado: {itemName} ");
    }
}
