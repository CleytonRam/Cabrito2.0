using UnityEngine;

[CreateAssetMenu(fileName = "Borracha", menuName = "TermoRoguelike/Itens/Borracha")]
public class BorrachaItem : ItemData
{
    public override void ApplyEffect()
    {
        MultiBoardManager currentGame = FindAnyObjectByType<MultiBoardManager>();
        if (currentGame != null) 
        {
            currentGame.UndoLastAttempt();
            Debug.Log("Borracha: Ultima tentativa apagada");
        }
    }
}
