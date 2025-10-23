using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;



public class ButtonHandler : MonoBehaviour
{
    bool state = true;
    public TMP_Text pauseandresume;
    private AI_handler AI_handler;
    private InitializerScript InitializerScript;
    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "AI-TR traffic system simulation/shared_data.txt");

    void Start()
    {
        AI_handler = FindFirstObjectByType<AI_handler>();
        InitializerScript = FindFirstObjectByType<InitializerScript>();
    }


    public void PauseandResumeGame()
    {
        if (state)
        {
            pauseandresume.text = "Resume";
            Time.timeScale = 0f;   // stop time
            Debug.Log("Game Paused");
            state = false;
        }
        else
        {
            pauseandresume.text = "Pause";
            Time.timeScale = InitializerScript.gameData.Overall_speed;
            Debug.Log("Game Resumed");
            state = true;
        }
    }

    public void RestartGame()
    {
        if (AI_handler.AI_Handler == false)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            string content = "1";
            File.WriteAllText(filePath, content);

            // If running in the editor
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                    // If running in a built application
                    Application.Quit();
#endif
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");

        if (InitializerScript.gameData.Benchmarking == 0 || InitializerScript.gameData.Benchmarking == 3)
        {
            string content = "0";
            File.WriteAllText(filePath, content);
        }

        // If running in the editor
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                // If running in a built application
                Application.Quit();
        #endif
    }
}
