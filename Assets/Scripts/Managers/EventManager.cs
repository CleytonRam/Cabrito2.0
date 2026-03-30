using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    [Header("UI References")]
    public Button backButton;
    public Button debugButton; // opcional

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToMap);

        // Botão de debug (opcional)
        if (debugButton != null)
            debugButton.onClick.AddListener(CompleteEventAndReturn);
    }

    public void ReturnToMap()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentNode != null)
        {
            MapNode eventNode = GameManager.Instance.currentNode;

            // Marca o nó do evento como visitado
            eventNode.isVisited = true;

            // Pega os nós do andar salvos no GameManager
            List<MapNode> floorNodes = GameManager.Instance.GetSavedFloorNodes();

            if (floorNodes != null)
            {
                int currentIndex = floorNodes.IndexOf(eventNode);

                if (currentIndex >= 0 && currentIndex < floorNodes.Count - 1)
                {
                    MapNode nextNode = floorNodes[currentIndex + 1];
                    nextNode.isAvailable = true;
                    Debug.Log($"Evento: liberou nó {currentIndex + 2}");
                }
            }
        }

        SceneManager.LoadScene(GameManager.Instance.mapScene);
    }

    // Método para debug (pular evento sem fazer nada)
    public void CompleteEventAndReturn()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentNode != null)
        {
            MapNode eventNode = GameManager.Instance.currentNode;
            eventNode.isVisited = true;

            List<MapNode> floorNodes = GameManager.Instance.GetSavedFloorNodes();

            if (floorNodes != null)
            {
                int currentIndex = floorNodes.IndexOf(eventNode);

                if (currentIndex >= 0 && currentIndex < floorNodes.Count - 1)
                {
                    floorNodes[currentIndex + 1].isAvailable = true;
                }
            }

            GameManager.Instance.ReduceActiveItemCooldown();
        }

        SceneManager.LoadScene(GameManager.Instance.mapScene);
    }
}