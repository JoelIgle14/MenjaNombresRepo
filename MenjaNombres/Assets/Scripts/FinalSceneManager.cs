using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FinalSceneManager : MonoBehaviour
{
    public TMP_Text pointsText;
    public TMP_Text timeText;
    public TMP_Text completedOrdersText;

    // Estos valores se pasarán desde el GameManager
    public int finalScore;
    public float finalGameTime;
    public int finalCompletedOrders;

    void Start()
    {
        // Cargar los datos de PlayerPrefs
        finalScore = PlayerPrefs.GetInt("FinalScore");
        finalGameTime = PlayerPrefs.GetFloat("FinalGameTime");
        finalCompletedOrders = PlayerPrefs.GetInt("FinalCompletedOrders");

        // Mostrar los valores al inicio de la escena de Game Over
        pointsText.text = "Puntuació: " + finalScore;
        timeText.text = "Temps: " + Mathf.FloorToInt(finalGameTime) + " segundos";
        completedOrdersText.text = "Comandes: " + finalCompletedOrders;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void QuitGame()
    {
        Debug.Log("Juego cerrado.");
        Application.Quit();
    }

}
