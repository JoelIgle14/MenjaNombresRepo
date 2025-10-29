using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialScene : MonoBehaviour
{
    [Header("Scene Names")]
    public string tutorialSceneName = "TutorialScene";
    public string mainGameSceneName = "MainGameScene";

    [Header("Optional: Loading Delay")]
    public float loadDelay = 0.01f;

    void Start()
    {
        CheckAndLoadAppropriateScene();
    }

    void CheckAndLoadAppropriateScene()
    {
        // Check if tutorial has been completed
        int tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0);

        if (tutorialCompleted == 1)
        {
            // Tutorial already completed, go to main game
            Debug.Log("[InitialScene] Tutorial completed previously. Loading main game...");
            Invoke(nameof(LoadMainGame), loadDelay);
        }
        else
        {
            // First time playing, start tutorial
            Debug.Log("[InitialScene] First time player. Loading tutorial...");
            Invoke(nameof(LoadTutorial), loadDelay);
        }
    }

    void LoadTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    void LoadMainGame()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }
}