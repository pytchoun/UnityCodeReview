using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("TestLevel");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}