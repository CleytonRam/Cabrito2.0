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

    [Header("Current Node")]
    public MapNode currentNode;

    [Header("Scenes")]
    public string menuScene = "Menu";
    public string mapScene = "Map";

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewRun()
    {
        currentFloor = 0;
        coins = 0;
        items.Clear();
        SceneManager.LoadScene(mapScene);
    }

    public void EnterNode(MapNode node)
    {
        currentNode = node;

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

            if (Random.value < 0.5f)
            {
                Debug.Log("Ganhou um item! (sistema de itens será implementado)");
            }
        }
        else
        {
            Debug.Log("Perdeu a rodada");
            // Aqui você pode adicionar penalidades depois
        }

        // Volta para o mapa
        
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuScene);
    }
}