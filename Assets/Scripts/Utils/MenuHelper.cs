using UnityEngine;

public class MenuHelper : MonoBehaviour
{
    public void QuitGame() 
    {
        Debug.Log("Saindo do jogo");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
