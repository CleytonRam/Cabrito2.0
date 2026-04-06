using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NovoEvento", menuName = "TermoRoguelike/Evento")]
public class EventData : ScriptableObject
{
    public string eventName;
    [TextArea(3, 5)] public string description;
    public Sprite eventArt;
    public List<Option> options;

    [System.Serializable]
    public class Option
    {
        public string optionText;
        public Consequence consequence;
    }

    [System.Serializable]
    public class Consequence
    {
        public int coinsChange = 0;
        public int healthChange = 0;
        public int maxHealthChange = 0;
        public ItemData itemReward;
        public bool removeActiveItem = false;
    }
}