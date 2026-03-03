using UnityEngine;



public enum NodeType
{
    Normal,
    Dueto,
    Quarteto,
    Event,
    Shop,
    Boss,
    Treasure,
    Curse,
    Campfire
}
[System.Serializable]
public class MapNode
{

    public NodeType nodeType;
    public Vector2 position;
    public string secretWord;
    public bool isVisited;
    public bool isAvailable;

    public MapNode(NodeType type, Vector2 pos)
    {
        nodeType = type;
        position = pos;
        isVisited = false;
        isAvailable = false;
    }
}


