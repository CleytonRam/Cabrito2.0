using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiBoardManager : MonoBehaviour
{
    [Header("Config")]
    public int maxRows = 6;
    public int wordLength = 5;
    public string nextScene = "";

    [Header("Referecences")]
    public List<Board> boards;
    public GameObject invalidWordText;
    public GameObject changeMapButton;
    public GameObject backToMapButton;

    [Header("Result Popup")]
    public GameObject resultPopupPanel;           // Painel que escurece a tela
    public TextMeshProUGUI popupMessageText;      // Texto da mensagem
    public Button popupCloseButton;               // Botão "OK"


    private int currentRow = 0;
    private int currentCol = 0;
    private bool isGameActive = true;
    private bool usedActiveItemInThisNode = false;
    private bool isPopupActive = false;



    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[]
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z,
    };


    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        foreach (Board board in boards)
        {
            string secret = board.GetRandomSolutionWord();
            board.NewGame(secret);
        }

        for (int i = 0; i < boards.Count; i++)
        {
            Debug.Log($"Board {i + 1} palavra secreta: {boards[i].SecretWord}");
        }

        foreach(ItemData item in GameManager.Instance.ownedItems) 
        {
            if (item.isActive == false)
            {
                item.ApplyEffect();
            }
        }
        foreach (Board board in boards)
        {
            board.SetVisibleRows(maxRows);
        }


        currentRow = 0;
        currentCol = 0;
        isGameActive = true;
        usedActiveItemInThisNode = false;

        if (invalidWordText) invalidWordText.SetActive(false);
        if (changeMapButton) changeMapButton.SetActive(false);
    }

    private void Update()
    {
        if (!isGameActive || isPopupActive) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!usedActiveItemInThisNode && GameManager.Instance.TryUseActiveItem())
            {
                usedActiveItemInThisNode = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            HandleBackspace();
        }

        else if (currentCol >= wordLength)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                HandleSubmit();
            }
        }
        else
        {
            for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
            {
                if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
                {
                    HandleLetter((char)SUPPORTED_KEYS[i]);
                    break;
                }
            }
        }
    }

    #region Letter handleling
    private void HandleLetter(char letter)
    {
        if (invalidWordText) invalidWordText.SetActive(false);

        foreach (Board board in boards)
        {
            if (!board.HasWon)
            {
                board.SetLetter(currentRow, currentCol, letter);
            }
        }
        currentCol++;
    }

    private void HandleBackspace()
    {
        if (invalidWordText) invalidWordText.SetActive(false);

        if (currentCol > 0)
        {
            currentCol--;
            foreach (Board board in boards)
            {
                if (!board.HasWon)
                {
                    board.SetLetter(currentRow, currentCol, '\0');
                }
            }
        }
    }

    private void HandleSubmit()
    {
        Board activeBoard = null;
        foreach (var board in boards)
        {
            if (!board.HasWon)
            {
                activeBoard = board;
                break;
            }
        }

        if (activeBoard == null) return;

        string attempt = activeBoard.GetRowWord(currentRow);

        if (!activeBoard.IsValidWord(attempt))
        {
            if (invalidWordText) invalidWordText.SetActive(true);
            return;
        }
        if (invalidWordText) invalidWordText.SetActive(false);

        foreach (Board board in boards)
        {
            if (!board.HasWon)
            {
                board.SubmitRow(currentRow);
            }
        }

        bool allWon = true;
        foreach (Board board in boards)
        {
            if (!board.HasWon)
            {
                allWon = false;
                break;
            }
        }

        if (allWon)
        {
            Debug.Log("VITORIA TODAS DESCOBERTAS");
            isGameActive = false;
            changeMapButton.SetActive(true);
            backToMapButton.SetActive(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNodeComplete(true);
            }
            return;
        }

        currentRow++;
        currentCol = 0;

        if (currentRow >= maxRows)
        {
            bool anyLost = false;
            foreach (Board board in boards)
            {
                if (!board.HasWon) anyLost = true;
            }
            if (anyLost)
            {
                Debug.Log("DERROTA SEU BURRAO");
                isGameActive = false;
                changeMapButton.SetActive(true);
                backToMapButton.SetActive(true);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnNodeComplete(false);
                }
            }
        }
    }
    #endregion


    #region Item things
    
    public void UndoLastAttempt() 
    {
        if(currentRow <= 0) 
        {
            Debug.Log("Sem tentativa para apagar");
            return;
        }

        currentRow--;
        currentCol = 0;

        foreach (Board board in boards) 
        {
            if (!board.HasWon) 
            {
                board.ClearRow(currentRow);
            }
        }
        Debug.Log($"Última tentativa (linha {currentRow + 1}) apagada dos boards que não venceram!");
    }


    #endregion


    public void ShowPopup(string message)
    {
        if (resultPopupPanel == null) return;

        // Configura mensagem
        popupMessageText.text = message;

        // Ativa o painel
        resultPopupPanel.SetActive(true);
        isPopupActive = true;

        // Para o input do jogo (não digita nem submete enquanto o popup estiver aberto)
        // O Update já vai ignorar se isPopupActive for true

        // Adiciona listener ao botão (remove anterior para evitar múltiplos)
        popupCloseButton.onClick.RemoveListener(ClosePopup);
        popupCloseButton.onClick.AddListener(ClosePopup);
    }


    private void ClosePopup()
    {
        if (resultPopupPanel != null)
            resultPopupPanel.SetActive(false);
        isPopupActive = false;
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(nextScene);
    }

    public void ReturnToMap()
    {
        if (GameManager.Instance != null) // se existe instância
        {
            SceneManager.LoadScene(GameManager.Instance.mapScene);
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}
