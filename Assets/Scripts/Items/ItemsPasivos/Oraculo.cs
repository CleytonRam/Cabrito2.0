using UnityEngine;

[CreateAssetMenu(fileName = "Oraculo", menuName = "TermoRoguelike/Itens/Oraculo")]
public class Oraculo : ItemData
{
    public override void ApplyEffect()
    {
        MultiBoardManager currentGame = FindObjectOfType<MultiBoardManager>();

        if (currentGame == null) return;
        foreach(Board board in currentGame.boards)
        {
            if(board.HasWon) continue;

            char firstLetter = board.SecretWord[0];

            int firstEmptyRow = 0;

            if(board.GetLetterAt(firstEmptyRow, 0 ) == '\0') 
            {
                board.SetLetter(firstEmptyRow, 0, firstLetter);
                board.SetState(firstEmptyRow, 0, board.occupiedState);
                Debug.Log($"Oráculo revelou a primeira letra '{firstLetter}' da palavra '{board.SecretWord}'");
            }
        }
    }
}
