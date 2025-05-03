using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public SceneFader sceneFader; // 👈 Drag your SceneFader GameObject here in Inspector

    public void PlayGame()
    {
        SpawnPointManager.SetTargetSpawnPoint("DefaultSpawnPoint"); // 👈 Définit le spawn point
        sceneFader.FadeToScene("Main_Scene"); // 👈 Charge la scène
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
