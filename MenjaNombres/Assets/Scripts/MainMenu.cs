using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Cargar la escena principal
    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    // Cerrar el juego
    public void QuitGame()
    {
        Debug.Log("Juego cerrado."); 
        Application.Quit();
    }
}
