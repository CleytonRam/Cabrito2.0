using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    [Header("Tiles")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;

    private Row[] rows;
    private string[] solutions;
    private string[] validWords;
    private string word;
    public bool HasWon { get; private set; }

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
        for (int r = 0; r < rows.Length; r++)
        {
            Tile[] tiles = rows[r].GetComponentsInChildren<Tile>();
            for (int c = 0; c < tiles.Length; c++)
            {
                tiles[c].rowIndex = r;
                tiles[c].colIndex = c;
            }
        }
    }

    private void Start()
    {
        LoadData();
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    }

    public void NewGame(string secretWord)
    {
        word = secretWord.ToLower().Trim();
        ClearBoard();
        HasWon = false;
    }

    public void SetLetter(int row, int col, char letter)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].tiles.Length)
            rows[row].tiles[col].SetLetter(letter);
    }

    public void SetState(int row, int col, Tile.State state)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].tiles.Length)
            rows[row].tiles[col].SetState(state);
    }

    public void ClearRow(int row)
    {
        if (row >= 0 && row < rows.Length)
        {
            for (int col = 0; col < rows[row].tiles.Length; col++)
            {
                rows[row].tiles[col].SetLetter('\0');
                rows[row].tiles[col].SetState(emptyState);
            }
        }
    }

    public void ClearBoard()
    {
        for (int row = 0; row < rows.Length; row++)
            ClearRow(row);
    }

    public string GetRowWord(int row)
    {
        return rows[row].word;
    }

    public void SubmitRow(int row)
    {
        if (HasWon || row < 0 || row >= rows.Length) return;

        Row currentRow = rows[row];
        string attempt = currentRow.word;

        if (!IsValidWord(attempt)) return;

        string remaining = word;

        // Passo 1: letras corretas e incorretas
        for (int i = 0; i < currentRow.tiles.Length; i++)
        {
            Tile tile = currentRow.tiles[i];
            if (tile.letter == word[i])
            {
                tile.SetState(correctState);
                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");
            }
            else if (!word.Contains(tile.letter))
            {
                tile.SetState(incorrectState);
            }
        }

        // Passo 2: letras em posição errada
        for (int i = 0; i < currentRow.tiles.Length; i++)
        {
            Tile tile = currentRow.tiles[i];
            if (tile.state != correctState && tile.state != incorrectState)
            {
                if (remaining.Contains(tile.letter))
                {
                    tile.SetState(wrongSpotState);
                    int index = remaining.IndexOf(tile.letter);
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                }
                else
                {
                    tile.SetState(incorrectState);
                }
            }
        }

        if (IsRowCorrect(row))
            HasWon = true;
    }

    public bool IsRowCorrect(int row)
    {
        if (row < 0 || row >= rows.Length) return false;
        foreach (var tile in rows[row].tiles)
            if (tile.state != correctState) return false;
        return true;
    }

    public bool IsValidWord(string word)
    {
        for (int i = 0; i < validWords.Length; i++)
            if (string.Equals(word, validWords[i], StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    public string GetRandomSolutionWord()
    {
        return solutions[Random.Range(0, solutions.Length)].ToLower().Trim();
    }

    public string SecretWord => word;

    public void RevealRandomLetter()
    {
        if (HasWon) return;

        int firstEmptyRow = -1;
        for (int i = 0; i < rows.Length; i++)
        {
            string rowWord = GetRowWord(i);
            if (string.IsNullOrEmpty(rowWord) || rowWord.Trim() == "" || rowWord.All(c => c == '\0'))
            {
                firstEmptyRow = i;
                break;
            }
        }
        if (firstEmptyRow == -1) return;

        List<int> availablePositions = new List<int>();
        for (int i = 0; i < word.Length; i++)
        {
            if (GetLetterAt(firstEmptyRow, i) == '\0')
                availablePositions.Add(i);
        }
        if (availablePositions.Count == 0) return;

        int position = availablePositions[Random.Range(0, availablePositions.Count)];
        char correctLetter = word[position];
        SetLetter(firstEmptyRow, position, correctLetter);
        SetState(firstEmptyRow, position, occupiedState);

        Debug.Log($"Caneta Favorita revelou '{correctLetter}' na posição {position + 1}");
    }

    public char GetLetterAt(int row, int col)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].tiles.Length)
            return rows[row].tiles[col].letter;
        return '\0';
    }

    public Row GetRow(int rowIndex)
    {
        if (rowIndex >= 0 && rowIndex < rows.Length)
            return rows[rowIndex];
        return null;
    }

    public Tile GetTileAt(int row, int col)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].tiles.Length)
            return rows[row].tiles[col];
        return null;
    }

    public void SetVisibleRows(int visibleRows)
    {
        for (int i = 0; i < rows.Length; i++)
            rows[i].gameObject.SetActive(i < visibleRows);
    }
}