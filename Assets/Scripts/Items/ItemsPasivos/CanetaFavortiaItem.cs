using UnityEngine;

[CreateAssetMenu(fileName = "CanetaFavorita", menuName = "TermoRoguelike/Itens/CanetaFavorita")]
public class CanetaFavoritaItem : ItemData
{
    public override void ApplyEffect()
    {
        MultiBoardManager currentGame = FindObjectOfType<MultiBoardManager>();
        if (currentGame != null)
        {
            foreach (Board board in currentGame.boards)
            {
                board.RevealRandomLetter();
            }
            Debug.Log("Caneta Favorita: Uma letra foi revelada!");
        }
    }
}