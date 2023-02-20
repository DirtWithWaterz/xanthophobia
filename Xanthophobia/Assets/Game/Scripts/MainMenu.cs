using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {

        SceneManager.LoadScene("Level0");
    }
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        System.GC.Collect();
        Application.Quit();
    }
    public void SettingsMenu()
    {
        Debug.Log("Settings Clicked");
        //wip
    }
}
