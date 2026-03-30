using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;


[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    // Estados dos tiles (exatamente como você já tem)
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

    // Configura o board com uma nova palavra secreta
    public void NewGame(string secretWord)
    {
        word = secretWord.ToLower().Trim();
        ClearBoard();
        HasWon = false;
    }

    // Define a letra em uma célula específica (linha, coluna)
    public void SetLetter(int row, int col, char letter)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].tiles.Length)
        {
            rows[row].tiles[col].SetLetter(letter);
            // Não muda o estado aqui – será definido na submissão
        }
    }

    // Define o estado de uma célula (usado após submissão)
    public void SetState(int row, int col, Tile.State state)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].tiles.Length)
        {
            rows[row].tiles[col].SetState(state);
        }
    }

    // Limpa uma linha inteira
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

    // Retorna a palavra atual de uma linha (como string)
    public string GetRowWord(int row)
    {
        return rows[row].word;
    }

    // Submete a tentativa da linha especificada
    public void SubmitRow(int row)
    {
        if (HasWon || row < 0 || row >= rows.Length) return;

        Row currentRow = rows[row];
        string attempt = currentRow.word;

        if (!IsValidWord(attempt))
            return; // A validação será feita pelo gerenciador

        string remaining = word;

        // Passo 1: Letras corretas e incorretas
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

        // Passo 2: Letras em posição errada
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

        // Verifica se essa linha acertou a palavra
        if (IsRowCorrect(row))
            HasWon = true;
    }

    // Verifica se uma linha específica está completamente correta
    public bool IsRowCorrect(int row)
    {
        if (row < 0 || row >= rows.Length) return false;
        foreach (var tile in rows[row].tiles)
            if (tile.state != correctState) return false;
        return true;
    }

    // Valida se uma palavra existe no dicionário
    public bool IsValidWord(string word)
    {
        for (int i = 0; i < validWords.Length; i++)
            if (string.Equals(word, validWords[i], StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    // Retorna uma palavra secreta aleatória (útil para o gerenciador)
    public string GetRandomSolutionWord()
    {
        return solutions[UnityEngine.Random.Range(0, solutions.Length)].ToLower().Trim();
    }
    public string SecretWord => word;
    public void RevealRandomLetter()
    {
        // Se já venceu, não faz nada
        if (HasWon) return;

        // Encontra a primeira linha vazia (a primeira tentativa)
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

        if (firstEmptyRow == -1) return; // não tem linha vazia

        // Verifica quais posições ainda não estão preenchidas
        List<int> availablePositions = new List<int>();
        for (int i = 0; i < word.Length; i++)
        {
            if (GetLetterAt(firstEmptyRow, i) == '\0')
            {
                availablePositions.Add(i);
            }
        }

        if (availablePositions.Count == 0) return;

        // Escolhe uma posição aleatória
        int position = availablePositions[Random.Range(0, availablePositions.Count)];

        // Coloca a letra correta nessa posição
        char correctLetter = word[position];
        SetLetter(firstEmptyRow, position, correctLetter);
        SetState(firstEmptyRow, position, occupiedState);

        Debug.Log($"Caneta Favorita revelou '{correctLetter}' na posição {position + 1} da palavra '{word}'");
    }
    public char GetLetterAt(int row, int col)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].tiles.Length)
        {
            return rows[row].tiles[col].letter;
        }
        return '\0';
    }
}