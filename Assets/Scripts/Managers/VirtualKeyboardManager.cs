using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VirtualKeyboardManager : MonoBehaviour
{
    [System.Serializable]
    public class VirtualKey
    {
        public string letter;
        public Button button;
        public TextMeshProUGUI letterText;
        public List<Image> quadrants;
    }

    public List<VirtualKey> keys;
    public Color defaultColor = new Color(0.55f, 0.29f, 0.11f); // Marrom
    public Color correctColor = Color.green;
    public Color wrongSpotColor = Color.yellow;
    public Color notPresentColor = Color.gray;

    private MultiBoardManager currentGame;

    void Start()
    {
        currentGame = FindObjectOfType<MultiBoardManager>();
       
        foreach (var key in keys)
        {
            
            if (key.button != null)
                key.button.onClick.AddListener(() => OnKeyPressed(key.letter));
        }
        ResetKeyboardToDefault();
    }

    public void ResetKeyboardToDefault()
    {
        foreach (var key in keys)
        {
            if (key.quadrants != null && key.quadrants.Count > 0)
            {
                foreach (var quad in key.quadrants)
                    quad.color = defaultColor;
            }
        }
    }

    public void UpdateKeyboardFromBoards()
    {
        if (currentGame == null) return;


        int boardCount = currentGame.boards.Count;
        bool isMultiBoard = (boardCount > 1);

        foreach (var key in keys)
        {
            char letter = key.letter[0];

            if (isMultiBoard)
            {
                for (int i = 0; i < boardCount && i < key.quadrants.Count; i++)
                {
                    char state = GetLetterStateInBoard(letter, i);
                    key.quadrants[i].color = GetColorFromState(state);
                }
            }
            else
            {
                char bestState = GetBestLetterState(letter);
                if (key.quadrants.Count > 0)
                    key.quadrants[0].color = GetColorFromState(bestState);
            }
        }
    }

    private char GetLetterStateInBoard(char letter, int boardIndex)
    {
        Board board = currentGame.boards[boardIndex];
        for (int row = 0; row <= currentGame.currentRow; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                Tile tile = board.GetTileAt(row, col);
                if (tile != null && tile.letter == letter)
                {
                    Debug.Log($"Letra {letter} no board {boardIndex} | tile.cor: {tile.state.fillColor} | correct.cor: {board.correctState.fillColor} | wrong.cor: {board.wrongSpotState.fillColor}");
                    // Compara as cores dos estados
                    if (IsSameColor(tile.state.fillColor, board.correctState.fillColor))
                        return 'G'; // Verde
                    if (IsSameColor(tile.state.fillColor, board.wrongSpotState.fillColor))
                        return 'Y'; // Amarelo
                }
            }
        }
        return 'N'; // Não testada ou não encontrada
    }

    private char GetBestLetterState(char letter)
    {
        char best = 'N';
        foreach (var board in currentGame.boards)
        {
            for (int row = 0; row <= currentGame.currentRow; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    Tile tile = board.GetTileAt(row, col);
                    if (tile != null && tile.letter == letter)
                    {
                        if (IsSameColor(tile.state.fillColor, board.correctState.fillColor))
                            return 'G'; // Verde (melhor estado)
                        if (IsSameColor(tile.state.fillColor, board.wrongSpotState.fillColor) && best != 'G')
                            best = 'Y'; // Amarelo (se não tiver verde)
                    }
                }
            }
        }
        return best;
    }

    private Color GetColorFromState(char state)
    {
        switch (state)
        {
            case 'G': return correctColor;
            case 'Y': return wrongSpotColor;
            case 'N': return defaultColor;
            default: return notPresentColor;
        }
    }
    private bool IsSameColor(Color a, Color b)
    {
        float tolerance = 0.05f; // tolerância de 5%
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    private void OnKeyPressed(string letter)
    {
        if (currentGame != null)
            currentGame.HandleLetter(letter[0]);
        else
            Debug.LogError("currentGame é nulo em OnKeyPressed!");
    }
}