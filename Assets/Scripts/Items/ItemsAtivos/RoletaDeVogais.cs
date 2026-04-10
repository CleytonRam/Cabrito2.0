using UnityEngine;

[CreateAssetMenu(fileName = "RoletaDeVogais", menuName = "TermoRoguelike/Itens/RoletaVogais")]
public class RoletaDeVogais : ItemData
{
    public override void ApplyEffect()
    {
        MultiBoardManager currentGame = FindObjectOfType<MultiBoardManager>();
        if (currentGame == null) return;

        int roll = Random.Range(1, 7);
        if (roll == 1)
        {
            GameManager.Instance.RemoveHealth(1);
            currentGame.ShowPopup(" Você tomou dano! ");
        }
        else
        {
            Board activeBoard = null;
            foreach (Board board in currentGame.boards)
            {
                if (!board.HasWon)
                {
                    activeBoard = board;
                    break;
                }
            }
            if (activeBoard != null)
            {
                string word = activeBoard.SecretWord;
                string vogaisPresentes = "";
                foreach (char vogal in "AEIOU")
                {
                    if (word.Contains(vogal.ToString().ToLower()))
                        vogaisPresentes += vogal + " ";
                }
                if (string.IsNullOrEmpty(vogaisPresentes))
                    vogaisPresentes = "Nenhuma vogal!";
                else
                    vogaisPresentes = vogaisPresentes.Trim();

                currentGame.ShowPopup($"Vogais na palavra: {vogaisPresentes}");
            }
        }
    }
}
