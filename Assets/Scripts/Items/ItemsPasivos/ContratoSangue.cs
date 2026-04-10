using UnityEngine;

[CreateAssetMenu(fileName = "ContratoSangue", menuName = "TermoRoguelike/Itens/ContratoSangue")]
public class ContratoSangueItem : ItemData
{
    public override void ApplyEffect()
    {
        MultiBoardManager currentGame = FindObjectOfType<MultiBoardManager>();
        if (currentGame == null) return;

        int minRows = currentGame.boards.Count == 2 ? 2 : 4;

        currentGame.maxRows = Mathf.Max(minRows, currentGame.maxRows - 2);

        foreach (Board board in currentGame.boards)
        {
            board.SetVisibleRows(currentGame.maxRows);
        }

        GameManager.Instance.hasContratoSangue = true;
    }
}