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
    public GameObject resultPopupPanel;           
    public TextMeshProUGUI popupMessageText;      
    public Button popupCloseButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverMessage;


    public int currentRow = 0;
    private int currentCol = 0;
    private bool isGameActive = true;
    private bool usedActiveItemInThisNode = false;
    private bool isPopupActive = false;
    private bool cursorMovedToFilledTile = false;



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
        UpdateCursorHighlight();
        isGameActive = true;
        usedActiveItemInThisNode = false;
        cursorMovedToFilledTile = false;
        VirtualKeyboardManager keyboard = FindObjectOfType<VirtualKeyboardManager>();
        if (keyboard != null) keyboard.ResetKeyboardToDefault();

        if (invalidWordText) invalidWordText.SetActive(false);
        if (changeMapButton) changeMapButton.SetActive(false);
    }

    private void Update()
    {
        if (!isGameActive || isPopupActive) return;

        // Primeiro, verifica se Enter foi pressionado
        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleSubmit();
            return;
        }

        // Depois, processa backspace e letras
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            HandleBackspace();
        }
        else if (currentCol >= wordLength)
        {
            // não faz nada (ou poderia ter alguma lógica, mas não é necessário)
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
    public void HandleLetter(char letter)
    {
        Debug.Log($"=== HANDLE LETTER: letra='{letter}', currentRow={currentRow}, currentCol={currentCol} (antes)");
        cursorMovedToFilledTile = false;
        if (invalidWordText) invalidWordText.SetActive(false);

        foreach (Board board in boards)
        {
            if (!board.HasWon)
            {
                board.SetLetter(currentRow, currentCol, letter);
            }
        }
        currentCol++;
        Debug.Log($"=== HANDLE LETTER: currentCol agora é {currentCol} (depois)");
        UpdateCursorHighlight();
    }

    private void HandleBackspace()
    {
        Debug.Log($"=== BACKSPACE: currentRow={currentRow}, currentCol={currentCol} (antes), cursorMovedToFilledTile={cursorMovedToFilledTile}");
        if (invalidWordText) invalidWordText.SetActive(false);

        if (cursorMovedToFilledTile)
        {
            Debug.Log("Caso 1: Apagando na posição atual");
            foreach (Board board in boards)
            {
                if (!board.HasWon)
                {
                    board.SetLetter(currentRow, currentCol, '\0');
                }
            }
            cursorMovedToFilledTile = false;
        }
        else if (currentCol > 0)
        {
            Debug.Log("Caso 2: Apagando letra anterior");
            currentCol--;
            foreach (Board board in boards)
            {
                if (!board.HasWon)
                {
                    board.SetLetter(currentRow, currentCol, '\0');
                }
            }
        }
        Debug.Log($"=== BACKSPACE: currentCol agora é {currentCol} (depois)");
        UpdateCursorHighlight();
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

        // Verifica se todas as posições da linha atual têm letras
        bool allFilled = true;
        for (int i = 0; i < wordLength; i++)
        {
            Tile tile = activeBoard.GetTileAt(currentRow, i);
            if (tile == null || tile.letter == '\0')
            {
                allFilled = false;
                break;
            }
        }

        if (!allFilled)
        {
            if (invalidWordText) invalidWordText.SetActive(true);
            return;
        }

        // Se chegou aqui, a palavra está completa
        string attempt = activeBoard.GetRowWord(currentRow);
        Debug.Log($"Submit: tentativa='{attempt}'");

        if (!activeBoard.IsValidWord(attempt))
        {
            if (invalidWordText) invalidWordText.SetActive(true);
            return;
        }
        if (invalidWordText) invalidWordText.SetActive(false);

        // Submeter a linha em todos os boards ativos
        foreach (Board board in boards)
        {
            if (!board.HasWon)
            {
                board.SubmitRow(currentRow);
            }
        }

        // Atualiza teclado
        VirtualKeyboardManager keyboard = FindObjectOfType<VirtualKeyboardManager>();
        if (keyboard != null) keyboard.UpdateKeyboardFromBoards();

        // Verifica vitória
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
            Debug.Log("VITORIA");
            isGameActive = false;
            changeMapButton.SetActive(true);
            backToMapButton.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.OnNodeComplete(true);
            return;
        }

        // Avança para a próxima linha
        currentRow++;
        currentCol = 0;
        cursorMovedToFilledTile = false;
        UpdateCursorHighlight();

        // Verifica derrota
        if (currentRow >= maxRows)
        {
            bool anyLost = false;
            foreach (Board board in boards)
            {
                if (!board.HasWon) anyLost = true;
            }
            if (anyLost)
            {
                Debug.Log("DERROTA");
                isGameActive = false;
                changeMapButton.SetActive(true);
                backToMapButton.SetActive(true);
                ShowGameOverPanel();
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

    public void ShowGameOverPanel() 
    {
        if (gameOverPanel == null) return;
        string message = "Perdeu fi, ";

        if (boards.Count == 1)
        {
            message += $"a palavra secreta era: {boards[0].SecretWord.ToUpper()}";
        }
        else 
        {
            message += "as palavras secretas eram: ";
            for (int i = 0; i < boards.Count; i++) 
            {
                message += boards[i].SecretWord.ToUpper();
                if (i < boards.Count - 1) message += ", ";
            }
        }

        gameOverMessage.text = message;
        gameOverPanel.SetActive(true);
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
    public void ReturnToMapAfterLoss()
    {
        // Aplica a penalidade (perder vida, etc.)
        if (GameManager.Instance != null)
            GameManager.Instance.OnNodeComplete(false);

        // Depois carrega o mapa
        if (GameManager.Instance != null)
            SceneManager.LoadScene(GameManager.Instance.mapScene);
        else
            SceneManager.LoadScene(nextScene);
    }
    public void MoveCursorToTile(Tile tile)
    {
        Debug.Log($"=== MOVE CURSOR: tile.row={tile.rowIndex}, tile.col={tile.colIndex}, letter='{tile.letter}'");
        if (!isGameActive || isPopupActive) return;

        if (tile.rowIndex == currentRow)
        {
            currentCol = tile.colIndex;
            cursorMovedToFilledTile = (tile.letter != '\0');
            Debug.Log($"Cursor movido para coluna {currentCol}, cursorMovedToFilledTile={cursorMovedToFilledTile}");
            UpdateCursorHighlight();
        }
    }

    private void UpdateCursorHighlight()
    {
        foreach (Board board in boards)
        {
            if (board.HasWon) continue;

            Row currentRowObj = board.GetRow(currentRow);
            if (currentRowObj == null) continue;

            Tile[] tiles = currentRowObj.GetComponentsInChildren<Tile>();

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].SetHighlight(i == currentCol);
            }
        }
    }
}
